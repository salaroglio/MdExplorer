using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.RemoteProtocol;
using MdExplorer.Services.RemoteFolders;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>
    /// Una radice remota vista dallo scanner (sprint Cartella-Remota-Via-SSH, G): i file del perimetro, i candidati e i
    /// testi letti finora. I testi stanno in memoria solo per l'analisi (G-D2); sul disco locale vanno gli script.
    /// </summary>
    public sealed class RemoteCodeScan
    {
        private readonly RemoteCodeRootFiles _files;
        private readonly CodeRootState _root;
        private readonly Dictionary<string, string> _relative = new(StringComparer.Ordinal);

        public CodeScanResult Scan { get; } = new() { Mode = "remote" };
        public InMemoryCodeSources Sources { get; }
        /// <summary>Le impronte di adesso, da ricordare nello stato dello scanner se l'analisi va a buon fine.</summary>
        public string Stamps { get; }

        internal RemoteCodeScan(RemoteCodeRootFiles files, CodeRootState root, IReadOnlyList<RemoteEntry> entries, string previousStamps, CodeLanguageCatalog languages)
        {
            _files = files; _root = root;
            Sources = new InMemoryCodeSources(entries.Select(e => CodeRootPaths.Format(root.Alias, e.Path)));
            var previous = ParseStamps(previousStamps);
            var stamps = new StringBuilder();
            foreach (var e in entries)
            {
                if (languages.ForFile(e.Path) == null) continue;   // come lo scanner fuori da git: solo i file di un linguaggio
                var logical = CodeRootPaths.Format(root.Alias, e.Path);
                var file = new CodeSourceFile(root.Remote.Combine(e.Path).ToString(), logical);
                _relative[logical] = e.Path;
                Scan.AllFiles.Add(file);
                stamps.Append(logical).Append('\t').Append(e.Stamp).Append('\n');
                if (!previous.TryGetValue(logical, out var was) || was != e.Stamp) Scan.Candidates.Add(file);
            }
            Stamps = stamps.ToString();
            Scan.Reason = previous.Count == 0
                ? UiText.Say($"radice remota {root.Remote}: prima analisi, tutti i file sono candidati", $"remote root {root.Remote}: first analysis, every file is a candidate")
                : UiText.Say($"radice remota {root.Remote}: {Scan.Candidates.Count} file cambiati sul server dall'ultima analisi (impronte)", $"remote root {root.Remote}: {Scan.Candidates.Count} files changed on the server since the last analysis (stamps)");
        }

        /// <summary>
        /// Legge dal server i testi che mancano, uno alla volta (MDE Remote serve una richiesta alla volta). Le codifiche
        /// per estensione le applica MDE Remote (G-D5): il testo è quello che leggerebbe chi ha il codice in locale.
        /// </summary>
        public async Task LoadAsync(IEnumerable<CodeSourceFile> files, CancellationToken ct)
        {
            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();
                if (Sources.Has(file)) continue;
                var host = CodeInlinePages.HostOf(file.LogicalPath);
                if (!_relative.TryGetValue(host, out var relative))
                    throw new InvalidOperationException(UiText.Say($"{file.LogicalPath}: non è un file della radice remota analizzata", $"{file.LogicalPath}: not a file of the analysed remote root"));
                try { Sources.Add(file, (await _files.ReadAsync(_root, relative, ct)).Text ?? string.Empty); }
                catch (RemoteOperationException ex) { Sources.Fail(file, $"{ex.Code}: {ex.Message}"); }
            }
        }

        private static Dictionary<string, string> ParseStamps(string text)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var line in (text ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var tab = line.IndexOf('\t');
                if (tab > 0) map[line[..tab]] = line[(tab + 1)..];
            }
            return map;
        }
    }

    /// <summary>
    /// Lo scanner delle radici remote (G-D3): <c>tree</c> di MDE Remote dà tutti i file con la loro impronta in una
    /// richiesta, e le impronte dell'ultima analisi dicono che cosa è cambiato, come il commit per git.
    /// </summary>
    public sealed class RemoteCodeScanner
    {
        private readonly RemoteCodeRootFiles _files;

        public RemoteCodeScanner(RemoteCodeRootFiles files) { _files = files; }

        /// <param name="sub">Dentro la radice, con <c>/</c>; <c>""</c> = tutta la radice (un contesto P2 è una sua sottocartella).</param>
        public async Task<RemoteCodeScan> ScanAsync(CodeRootState root, string sub, string previousStamps, CodeLanguageCatalog languages, CancellationToken ct)
        {
            if (!root.IsRemote) throw new InvalidOperationException($"@{root.Alias}: non è una radice remota");
            var response = await _files.OperateAsync(root, new RemoteRequest { Op = RemoteOps.Tree, Path = sub ?? string.Empty, Exclude = CodeChangeScanner.IgnoredDirectoryNames.ToList() }, ct);
            return new RemoteCodeScan(_files, root, response.Entries ?? new List<RemoteEntry>(), previousStamps, languages);
        }
    }
}
