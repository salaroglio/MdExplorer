using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.RemoteProtocol;
using MdExplorer.Services.RemoteFolders;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>
    /// I file di una radice remota per la scoperta della chiusura (F2c, 10/11/2026), con le operazioni che MDE Remote ha già:
    /// <c>tree</c> per l'elenco, <c>read</c> per i file che entrano, <c>grep</c> per l'indice delle dichiarazioni — sul server
    /// tornano solo le righe che dichiarano qualcosa, non i 20 milioni di righe. Le codifiche della radice le applica MDE Remote.
    /// Un <c>grep</c> troncato non si usa a metà: la scoperta si ferma e lo dice (niente indici incompleti in silenzio).
    /// </summary>
    public sealed class RemoteCodeWorkspace : ICodeWorkspace
    {
        /// <summary>Quante righe al più per un'espressione: le dichiarazioni di una radice enorme (una o poche per file).</summary>
        public const int MaxGrepHits = 500_000;

        private readonly RemoteCodeRootFiles _files;
        private readonly CodeRootState _root;
        private readonly string _sub;
        private readonly CancellationToken _ct;
        private List<string> _list;

        /// <param name="sub">Dentro la radice, con <c>/</c>; <c>""</c> = tutta la radice.</param>
        public RemoteCodeWorkspace(RemoteCodeRootFiles files, CodeRootState root, string sub, CancellationToken ct = default)
        {
            if (!root.IsRemote) throw new InvalidOperationException($"@{root.Alias}: non è una radice remota");
            _files = files; _root = root; _sub = (sub ?? string.Empty).Trim('/'); _ct = ct;
        }

        private string Logical(string relative) => CodeRootPaths.Format(_root.Alias, relative);

        private string Relative(string logical)
        {
            var (alias, rest) = CodeRootPaths.Parse(logical);
            if (!string.Equals(alias, _root.Alias, StringComparison.Ordinal))
                throw new ArgumentException(UiText.Say($"{logical}: non è un file di @{_root.Alias}", $"{logical}: not a file of @{_root.Alias}"));
            return rest;
        }

        public IReadOnlyList<string> Files()
        {
            if (_list != null) return _list;
            var response = _files.OperateAsync(_root, new RemoteRequest { Op = RemoteOps.Tree, Path = _sub, Exclude = CodeChangeScanner.IgnoredDirectoryNames.ToList() }, _ct)
                .GetAwaiter().GetResult();
            return _list = (response.Entries ?? new List<RemoteEntry>()).Where(e => !e.IsFolder).Select(e => Logical(e.Path)).OrderBy(p => p, StringComparer.Ordinal).ToList();
        }

        public string Read(string logicalPath) => _files.ReadAsync(_root, Relative(logicalPath), _ct).GetAwaiter().GetResult().Text ?? string.Empty;

        public CodeSourceFile Source(string logicalPath) => new(_root.Remote.Combine(Relative(logicalPath)).ToString(), logicalPath);

        public IEnumerable<(string Path, int Line, string Text)> Grep(Regex pattern, IReadOnlyCollection<string> extensions)
        {
            var response = _files.OperateAsync(_root, new RemoteRequest
            {
                Op = RemoteOps.Grep, Path = _sub, Pattern = pattern.ToString(), IgnoreCase = pattern.Options.HasFlag(RegexOptions.IgnoreCase),
                MaxHits = MaxGrepHits, Exclude = CodeChangeScanner.IgnoredDirectoryNames.ToList(),
            }, _ct).GetAwaiter().GetResult();
            if (response.Truncated == true)
                throw new InvalidOperationException(UiText.Say(
                    $"@{_root.Alias}: più di {MaxGrepHits} righe per «{pattern}»: l'indice delle dichiarazioni non sarebbe completo. Restringi la radice (un contesto più piccolo).",
                    $"@{_root.Alias}: more than {MaxGrepHits} lines for «{pattern}»: the declaration index would not be complete. Narrow the root (a smaller context)."));
            foreach (var hit in response.Hits ?? new List<RemoteHit>())
                if (extensions.Contains(Path.GetExtension(hit.Path), StringComparer.OrdinalIgnoreCase))
                    yield return (Logical(hit.Path), hit.Line, hit.Text);
        }
    }
}
