using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Services.RemoteFolders;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>L'anteprima di un seme (F3a): la chiusura scoperta e il linguaggio con cui la si è cercata.</summary>
    public sealed class CodeSeedPreview
    {
        public string Seed { get; init; }
        public string Language { get; init; }
        public CodeClosure Closure { get; init; }
        /// <summary>Dove stanno i file (disco o server): l'analisi del seme li legge da qui.</summary>
        [System.Text.Json.Serialization.JsonIgnore] public ICodeWorkspace Workspace { get; init; }
    }

    /// <summary>
    /// I semi (F3 del piano 2026-11-10-Analisi-Di-Programmi-Singoli): un file da cui l'utente vuole partire, e la chiusura delle
    /// sue dipendenze. F3a: l'anteprima — la scoperta della chiusura col linguaggio del file (a regole o Java), sulla radice
    /// del file (il progetto, una radice su disco, una radice SSH). Non scrive niente: è il parser, senza LLM.
    /// </summary>
    public sealed class CodeSeedService
    {
        private readonly ICodeRootService _roots;
        private readonly ICodeExtractorRegistry _extractors;
        private readonly RemoteCodeRootFiles _remoteFiles;

        public CodeSeedService(ICodeRootService roots, ICodeExtractorRegistry extractors, RemoteCodeRootFiles remoteFiles)
        {
            _roots = roots; _extractors = extractors; _remoteFiles = remoteFiles;
        }

        public async Task<CodeSeedPreview> PreviewAsync(string projectPath, string seed, IEnumerable<string> pruned, int? maxFiles, long? maxLines, CancellationToken ct = default,
            IEnumerable<string> included = null)
        {
            seed = (seed ?? throw new ArgumentException(UiText.Say("seed è obbligatorio: il file da cui partire", "seed is required: the file to start from"))).Replace('\\', '/').Trim('/');
            var catalog = CodeLanguageCatalog.ForProject(projectPath);
            var lang = catalog.ForFile(seed)
                ?? throw new InvalidOperationException(UiText.Say($"{seed}: nessun linguaggio del progetto ha questa estensione", $"{seed}: no language of the project has this extension"));
            var workspace = await WorkspaceOfAsync(projectPath, seed, ct);
            var request = new CodeClosureRequest
            {
                Seeds = new[] { seed },
                Pruned = new HashSet<string>((pruned ?? Enumerable.Empty<string>()).Select(p => p.Replace('\\', '/').Trim('/')), StringComparer.Ordinal),
                Included = new HashSet<string>((included ?? Enumerable.Empty<string>()).Select(p => p.Replace('\\', '/').Trim('/')), StringComparer.Ordinal),
                MaxFiles = maxFiles ?? new CodeClosureRequest().MaxFiles,
                MaxLines = maxLines ?? new CodeClosureRequest().MaxLines,
            };
            Func<CodeClosure> discover;
            if (lang.ExtractorKind == CodeExtractorKind.Declarative && lang.Definition != null && lang.Definition.Template == null)
                discover = () => new DeclarativeClosure(lang.Definition, workspace).Compute(request);
            else if (string.Equals(lang.Id, "java", StringComparison.OrdinalIgnoreCase))
            {
                var extractor = _extractors.ForProject(catalog).Get(lang.Id);
                if (extractor == null || _extractors.ForProject(catalog).Availability(lang.Id) != "ready")
                    throw new InvalidOperationException(UiText.Say($"Java: l'estrattore non è pronto ({_extractors.ForProject(catalog).Availability(lang.Id)}). Scarica la grammatica dalla finestra «Ambiente di sviluppo».",
                        $"Java: the extractor is not ready ({_extractors.ForProject(catalog).Availability(lang.Id)}). Download the grammar from the «Development environment» window."));
                discover = () => new JavaClosure(extractor, workspace).Compute(request);
            }
            else
                throw new InvalidOperationException(UiText.Say($"{lang.DisplayName}: l'analisi di un programma partendo da un file c'è per i linguaggi a regole e per Java",
                    $"{lang.DisplayName}: analysing a program from one file is available for rule-based languages and for Java"));
            // il parser è CPU (e per una radice SSH aspetta MDE Remote): fuori dal thread della richiesta
            var closure = await Task.Run(discover, ct);
            return new CodeSeedPreview { Seed = seed, Language = lang.Id, Closure = closure, Workspace = workspace };
        }

        /// <summary>
        /// I file fra cui cercare: la radice del seme. Un file del progetto: tutto il progetto; <c>@alias/…</c>: la radice, sul
        /// disco o sul server. La scoperta va dove vanno le dipendenze, non solo nella cartella del seme.
        /// </summary>
        private async Task<ICodeWorkspace> WorkspaceOfAsync(string projectPath, string seed, CancellationToken ct)
        {
            if (!seed.StartsWith("@", StringComparison.Ordinal)) return new LocalCodeWorkspace(projectPath, string.Empty);
            var (alias, _) = CodeRootPaths.Parse(seed);
            var root = (await _roots.ListAsync(projectPath, ct)).FirstOrDefault(r => r.Alias == alias)
                ?? throw new InvalidOperationException(UiText.Say($"La radice «@{alias}» non è dichiarata nel .development.yml.", $"The root «@{alias}» is not declared in .development.yml."));
            if (root.Status != CodeRootStatus.Linked)
                throw new InvalidOperationException(UiText.Say($"@{alias}: la radice non è collegata ({root.Reason}).", $"@{alias}: the root is not linked ({root.Reason})."));
            return root.IsRemote
                ? new RemoteCodeWorkspace(_remoteFiles, root, string.Empty, ct)
                : new LocalCodeWorkspace(root.RootPath, "@" + alias, root.ExtensionEncodings);
        }
    }
}
