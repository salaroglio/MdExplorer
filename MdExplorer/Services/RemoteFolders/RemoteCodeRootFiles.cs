using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.RemoteProtocol;

namespace MdExplorer.Services.RemoteFolders
{
    /// <summary>
    /// I file di una radice esterna remota, per l'albero e per la pagina del documento (sprint
    /// 2026-10-21-Cartella-Remota-Via-SSH, F2): la stessa forma di <see cref="CodeRootBrowser"/>, ma i byte arrivano dal
    /// MDE Remote. Il percorso assoluto di un elemento è la sua posizione <c>ssh://host/…</c>.
    /// </summary>
    public sealed class RemoteCodeRootFiles
    {
        private readonly IRemoteSessions _sessions;

        public RemoteCodeRootFiles(IRemoteSessions sessions) { _sessions = sessions; }

        /// <param name="relative">Dentro la radice, con <c>/</c>; <c>""</c> = la radice (da <see cref="CodeRootResolver.RemoteOf"/>).</param>
        public async Task<IReadOnlyList<CodeRootEntry>> ListAsync(CodeRootState root, string relative, CancellationToken ct)
        {
            var channel = await _sessions.OpenAsync(root.Remote, ct);
            var entries = (await channel.SendAsync(WithRules(root, new RemoteRequest { Op = RemoteOps.List, Path = relative }), ct)).Entries ?? new List<RemoteEntry>();
            return entries.Select(e => new CodeRootEntry
            {
                Name = e.Name,
                LogicalPath = CodeRootPaths.Format(root.Alias, e.Path),
                AbsolutePath = root.Remote.Combine(e.Path).ToString(),
                IsFolder = e.IsFolder,
                IsText = e.IsText,
                Unreadable = e.Unreadable,
            }).ToList();
        }

        /// <summary>
        /// Un'operazione sui file della radice (F3, D7): crea, scrive, cartella nuova, cancella, sposta/rinomina, cerca.
        /// I percorsi sono già relativi alla radice; MDE Remote applica le sue regole (dentro la radice, niente link,
        /// impronta per scrivere, mai sopra un file esistente).
        /// </summary>
        public async Task<RemoteResponse> OperateAsync(CodeRootState root, RemoteRequest request, CancellationToken ct)
        {
            var channel = await _sessions.OpenAsync(root.Remote, ct);
            return await channel.SendAsync(WithRules(root, request), ct);
        }

        /// <summary>
        /// La stessa operazione su una radice LOCALE (FL): le regole di MDE Remote (<see cref="WorkspaceDispatcher"/>) sulla
        /// cartella del disco, così una radice si modifica allo stesso modo ovunque stia.
        /// </summary>
        public static RemoteResponse OperateLocal(CodeRootState root, RemoteRequest request)
        {
            var response = WorkspaceDispatcher.Dispatch(new RemoteWorkspace(root.RootPath), WithRules(root, request));
            if (!response.Ok) throw new RemoteOperationException(response.Error?.Code ?? RemoteErrorCodes.Internal, response.Error?.Message ?? "");
            return response;
        }

        /// <summary>Il testo di un file, con l'impronta per riscriverlo; un binario o un file non UTF-8 è rifiutato da MDE Remote.</summary>
        public async Task<RemoteResponse> ReadAsync(CodeRootState root, string relative, CancellationToken ct)
        {
            var channel = await _sessions.OpenAsync(root.Remote, ct);
            return await channel.SendAsync(WithRules(root, new RemoteRequest { Op = RemoteOps.Read, Path = relative }), ct);
        }

        /// <summary>Le codifiche per estensione della radice vanno con ogni richiesta: MDE Remote non ha configurazioni sue.</summary>
        private static RemoteRequest WithRules(CodeRootState root, RemoteRequest request)
        {
            if (root.ExtensionEncodings.Count > 0) request.ExtensionEncodings = new Dictionary<string, string>(root.ExtensionEncodings);
            return request;
        }
    }
}
