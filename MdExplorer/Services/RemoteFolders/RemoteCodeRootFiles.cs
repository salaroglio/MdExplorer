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
    /// serverino. Il percorso assoluto di un elemento è la sua posizione <c>ssh://host/…</c>.
    /// </summary>
    public sealed class RemoteCodeRootFiles
    {
        private readonly IRemoteSessions _sessions;

        public RemoteCodeRootFiles(IRemoteSessions sessions) { _sessions = sessions; }

        /// <param name="relative">Dentro la radice, con <c>/</c>; <c>""</c> = la radice (da <see cref="CodeRootResolver.RemoteOf"/>).</param>
        public async Task<IReadOnlyList<CodeRootEntry>> ListAsync(CodeRootState root, string relative, CancellationToken ct)
        {
            var channel = await _sessions.OpenAsync(root.Remote, ct);
            var entries = (await channel.SendAsync(new RemoteRequest { Op = RemoteOps.List, Path = relative }, ct)).Entries ?? new List<RemoteEntry>();
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

        /// <summary>Il testo di un file, con l'impronta per riscriverlo; un binario o un file non UTF-8 è rifiutato dal serverino.</summary>
        public async Task<RemoteResponse> ReadAsync(CodeRootState root, string relative, CancellationToken ct)
        {
            var channel = await _sessions.OpenAsync(root.Remote, ct);
            return await channel.SendAsync(new RemoteRequest { Op = RemoteOps.Read, Path = relative }, ct);
        }
    }
}
