using System;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Services.RootChanges;
using MdExplorer.RemoteProtocol;
using MdExplorer.Services.RemoteFolders;

namespace MdExplorer.Services.RootChanges
{
    /// <summary>
    /// Il cancello delle radici per MarkAgent (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici, modello «arretra» come
    /// VS Code): le scritture vanno sul file vero — build, test ed Eclipse le vedono — ma prima della PRIMA scrittura su un file
    /// il cancello ne salva l'originale, così la revisione può annullare pezzo per pezzo. Le letture passano dirette.
    /// Vale per le radici locali e remote allo stesso modo (<c>real</c> = MDE Remote o le stesse regole sul disco).
    /// Solo per chi lo chiede (<c>review</c>, i tool di MarkAgent): l'editor dei file delle radici, che è l'utente, no.
    /// </summary>
    public sealed class RootChangeGate
    {
        private readonly RootChangeStore _store;

        public RootChangeGate(RootChangeStore store) => _store = store;

        public delegate Task<RemoteResponse> RealOperation(RemoteRequest request);

        public async Task<RemoteResponse> OperateAsync(string projectPath, CodeRootState root, RemoteRequest request, RealOperation real)
        {
            var relative = RootChangeStore.Normalize(request.Path);
            switch (request.Op)
            {
                case RemoteOps.Write:
                case RemoteOps.Create:
                {
                    var taken = await KeepOriginalAsync(projectPath, root, relative, real);
                    var response = await RealOrForgetAsync(projectPath, root, real, request, taken ? relative : null);
                    await AfterWriteAsync(projectPath, root, relative, request.Text, response.Stamp, real);
                    return response;
                }
                case RemoteOps.Delete:
                {
                    if ((await real(new RemoteRequest { Op = RemoteOps.Stat, Path = relative })).Entry?.IsFolder == true)
                        throw new RemoteOperationException(RemoteErrorCodes.Unsupported, UiText.Say(
                            $"'{relative}' è una cartella: sulle radici MarkAgent cancella un file per volta, così ogni cancellazione si rivede e si annulla.",
                            $"'{relative}' is a folder: on code roots MarkAgent deletes one file at a time, so each deletion can be reviewed and undone."));
                    var taken = await KeepOriginalAsync(projectPath, root, relative, real);
                    var response = await RealOrForgetAsync(projectPath, root, real, request, taken ? relative : null);
                    AfterDelete(projectPath, root, relative);
                    return response;
                }
                case RemoteOps.Move:
                {
                    var to = RootChangeStore.Normalize(request.To);
                    if ((await real(new RemoteRequest { Op = RemoteOps.Stat, Path = relative })).Entry?.IsFolder == true)
                        throw new RemoteOperationException(RemoteErrorCodes.Unsupported, UiText.Say(
                            $"'{relative}' è una cartella: sulle radici MarkAgent sposta un file per volta, così ogni spostamento si rivede e si annulla.",
                            $"'{relative}' is a folder: on code roots MarkAgent moves one file at a time, so each move can be reviewed and undone."));
                    var takenFrom = await KeepOriginalAsync(projectPath, root, relative, real);
                    var takenTo = await KeepOriginalAsync(projectPath, root, to, real);
                    var response = await RealOrForgetAsync(projectPath, root, real, request, takenFrom ? relative : null, takenTo ? to : null);
                    AfterDelete(projectPath, root, relative);
                    var moved = await real(new RemoteRequest { Op = RemoteOps.Read, Path = to });
                    await AfterWriteAsync(projectPath, root, to, moved.Text, moved.Stamp, real);
                    return response;
                }
                default:
                    return await real(request);
            }
        }

        /// <summary>
        /// L'operazione vera; se è rifiutata (impronta, codifica, file che c'è già…) il file non è cambiato, quindi gli originali
        /// presi apposta in questa chiamata si tolgono: altrimenti resterebbe da rivedere un file mai toccato.
        /// </summary>
        private async Task<RemoteResponse> RealOrForgetAsync(string projectPath, CodeRootState root, RealOperation real, RemoteRequest request,
            params string[] takenNow)
        {
            try { return await real(request); }
            catch
            {
                foreach (var relative in takenNow)
                    if (relative != null) _store.Remove(projectPath, root.Alias, relative);
                throw;
            }
        }

        /// <summary>
        /// L'originale si prende una volta sola: la prima volta che MarkAgent tocca il file da quando l'utente l'ha rivisto.
        /// Vero se l'ha preso adesso.
        /// </summary>
        private async Task<bool> KeepOriginalAsync(string projectPath, CodeRootState root, string relative, RealOperation real)
        {
            if (_store.Get(projectPath, root.Alias, relative) != null) return false;
            RemoteResponse current = null;
            try { current = await real(new RemoteRequest { Op = RemoteOps.Read, Path = relative }); }
            catch (RemoteOperationException ex) when (ex.Code == RemoteErrorCodes.NotFound) { }
            _store.Save(projectPath, new RootChange
            {
                Alias = root.Alias, Relative = relative, OriginalExists = current != null, OriginalText = current?.Text,
                Encoding = current?.Encoding, Bom = current?.Bom == true, LastStamp = current?.Stamp,
                CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now,
            });
            return true;
        }

        private async Task AfterWriteAsync(string projectPath, CodeRootState root, string relative, string text, string stamp, RealOperation real)
        {
            var change = _store.Get(projectPath, root.Alias, relative);
            if (change == null) return;
            // Tornato com'era: non c'è più niente da rivedere.
            if (change.OriginalExists && string.Equals(text, change.OriginalText, StringComparison.Ordinal))
            {
                _store.Remove(projectPath, root.Alias, relative);
                return;
            }
            change.LastStamp = stamp ?? (await real(new RemoteRequest { Op = RemoteOps.Read, Path = relative })).Stamp;
            change.UpdatedAt = DateTime.Now;
            _store.Save(projectPath, change);
        }

        private void AfterDelete(string projectPath, CodeRootState root, string relative)
        {
            var change = _store.Get(projectPath, root.Alias, relative);
            if (change == null) return;
            // Creato e poi cancellato da MarkAgent: per l'utente non è cambiato niente.
            if (!change.OriginalExists) { _store.Remove(projectPath, root.Alias, relative); return; }
            change.LastStamp = null;
            change.UpdatedAt = DateTime.Now;
            _store.Save(projectPath, change);
        }
    }
}
