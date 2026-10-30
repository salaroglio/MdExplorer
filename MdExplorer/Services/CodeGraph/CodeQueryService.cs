using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>Una domanda pronta non si può fare così: nome sconosciuto o parametro sbagliato. Il messaggio dice come rimediare.</summary>
    public sealed class CodeQueryException : Exception
    {
        public CodeQueryException(string message) : base(message) { }
    }

    /// <summary>
    /// Le domande pronte sul grafo del codice (D43, 30/10/2026): Mark le chiama per nome con <c>code_query</c>, MdExplorer
    /// controlla i parametri ed esegue — SPARQL scritto e provato qui, e per i punti ciechi anche i file su disco. Mark
    /// non scrive SPARQL (skill mde-code).
    /// </summary>
    public sealed class CodeQueryService
    {
        public const string UnknownExternals = CodeQueryCatalog.UnknownExternals;
        public const string WhereIsIt = CodeQueryCatalog.WhereIsIt;
        public const string NotAnalyzed = CodeQueryCatalog.NotAnalyzed;

        private readonly ICodeGraphStore _store;
        private readonly ICodeAnalysisService _analysis;
        private readonly ICodeRootService _roots;

        public CodeQueryService(ICodeGraphStore store, ICodeAnalysisService analysis, ICodeRootService roots)
        {
            _store = store; _analysis = analysis; _roots = roots;
        }

        public async Task<object> RunAsync(string projectPath, string name, IReadOnlyDictionary<string, string> parameters, CancellationToken ct = default)
        {
            var info = CodeQueryCatalog.All.FirstOrDefault(q => q.Name == name)
                ?? throw new CodeQueryException(UiText.Say($"Domanda «{name}» sconosciuta. Quelle pronte: {string.Join(", ", CodeQueryCatalog.All.Select(q => q.Name))} (code_schema le spiega).",
                    $"Unknown question «{name}». The ready ones: {string.Join(", ", CodeQueryCatalog.All.Select(q => q.Name))} (code_schema explains them)."));
            parameters ??= new Dictionary<string, string>();
            var unknown = parameters.Keys.Where(k => !info.Parameters.ContainsKey(k)).ToList();
            if (unknown.Count > 0)
                throw new CodeQueryException(UiText.Say($"«{name}» non ha i parametri {string.Join(", ", unknown)}: ha {string.Join(", ", info.Parameters.Keys)}.",
                    $"«{name}» has no parameters {string.Join(", ", unknown)}: it has {string.Join(", ", info.Parameters.Keys)}."));
            string P(string key) => parameters.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;
            int Int(string key, int min, int max, int fallback)
            {
                var v = P(key);
                if (v == null) return fallback;
                if (!int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) || n < min || n > max)
                    throw new CodeQueryException(UiText.Say($"«{key}» vale «{v}»: dev'essere un numero fra {min} e {max}.", $"«{key}» is «{v}»: it must be a number between {min} and {max}."));
                return n;
            }
            var folder = P("folder")?.Replace('\\', '/').TrimEnd('/');
            var prefix = folder == null ? null : folder + "/";
            return name switch
            {
                UnknownExternals => await UnknownExternalsAsync(projectPath, prefix, Int("limit", 1, 200, 30), ct),
                WhereIsIt => await WhereIsItAsync(projectPath, prefix, P("type"), Int("limit", 1, 50, 10), ct),
                NotAnalyzed => await NotAnalyzedAsync(projectPath, folder, ct),
                _ => throw new InvalidOperationException($"domanda «{name}» nel catalogo ma senza esecuzione"),
            };
        }

        private async Task<object> UnknownExternalsAsync(string projectPath, string prefix, int limit, CancellationToken ct)
        {
            var types = CodeExternals.Group(await _store.ExternalUsesAsync(projectPath, prefix, ct));
            return new
            {
                totalTypes = types.Count(t => t.Type != CodeExternals.UnknownOwner),
                callsWithUnknownType = types.FirstOrDefault(t => t.Type == CodeExternals.UnknownOwner)?.Uses ?? 0,
                packages = types.Where(t => t.Package != null).GroupBy(t => t.Package).Select(g => new { package = g.Key, types = g.Count(), uses = g.Sum(t => t.Uses) })
                    .OrderByDescending(p => p.uses).ThenBy(p => p.package, StringComparer.Ordinal).Take(limit),
                types = types.Take(limit).Select(t => new { t.Type, t.Package, t.Uses, t.Callers, members = t.Members.Take(10).Select(m => new { m.Name, m.Uses }), t.Example }),
                next = UiText.Say("where-is-it dice dove, nel repository, sta quello che il grafo non ha analizzato", "where-is-it tells where, in the repository, what the graph did not analyse is"),
            };
        }

        private async Task<object> WhereIsItAsync(string projectPath, string prefix, string type, int limit, CancellationToken ct)
        {
            var types = type != null ? new List<string> { type }
                : CodeExternals.Group(await _store.ExternalUsesAsync(projectPath, prefix, ct)).Where(t => t.Type != CodeExternals.UnknownOwner).Take(limit).Select(t => t.Type).ToList();
            var scopes = await _analysis.ProgramScopesAsync(projectPath, ct);
            var programs = scopes.Where(s => s.AbsolutePath != null).Select(s => Path.GetFullPath(s.AbsolutePath)).ToList();
            var roots = new List<string> { projectPath };
            roots.AddRange((await _roots.ListAsync(projectPath, ct)).Where(r => r.Status == CodeRootStatus.Linked && !r.IsRemote && r.LocalPath != null).Select(r => r.LocalPath));
            var hits = await Task.Run(() => CodeRepositorySearch.Search(roots, types,
                f => programs.Any(p => PathContainment.IsInside(p, Path.GetFullPath(f))), ct), ct);
            string Show(string file)
            {
                var full = Path.GetFullPath(file);
                var root = roots.Select(Path.GetFullPath).FirstOrDefault(r => PathContainment.IsInside(r, full));
                return root == null ? full : Path.GetRelativePath(root, full).Replace('\\', '/');
            }
            var found = hits.Select(h => h.Type).ToHashSet(StringComparer.Ordinal);
            return new
            {
                searched = types,
                roots = roots.Select(r => Path.GetFullPath(r)),
                hits = hits.Select(h => new { h.Type, h.Kind, file = Show(h.File), h.Detail, inProgram = h.InProgram, h.Remedy }),
                notFound = types.Where(t => !found.Contains(t)).Select(t => new
                {
                    type = t,
                    remedy = UiText.Say("da nessuna parte nel repository: è una libreria esterna (Maven, NuGet, npm) o un programma che sta altrove; il grafo non lo vedrà, dillo nel documento",
                        "nowhere in the repository: an external library (Maven, NuGet, npm) or a program kept elsewhere; the graph will not see it, say so in the document"),
                }),
                remoteNotSearched = scopes.Where(s => s.AbsolutePath == null && s.Unusable != null).Select(s => new { s.Folder, why = s.Unusable }),
            };
        }

        private async Task<object> NotAnalyzedAsync(string projectPath, string folder, CancellationToken ct)
        {
            var catalog = CodeLanguageCatalog.ForProject(projectPath);
            var result = new List<object>();
            foreach (var scope in (await _analysis.ProgramScopesAsync(projectPath, ct)).Where(s => folder == null || s.Folder == folder || s.Folder.StartsWith(folder + "/", StringComparison.Ordinal) || folder.StartsWith(s.Folder + "/", StringComparison.Ordinal)))
            {
                var (describable, described) = await _store.DescribedCountsAsync(projectPath, scope.Folder + "/", ct);
                if (scope.AbsolutePath == null)
                {
                    result.Add(new { scope.Folder, unusable = scope.Unusable, describable, described });
                    continue;
                }
                var groups = await Task.Run(() => Classify(projectPath, scope, catalog, ct), ct);
                result.Add(new
                {
                    scope.Folder, languages = scope.Languages,
                    analyzedFiles = groups.Where(g => g.Status == "analyzed").Sum(g => g.Count),
                    notAnalyzed = groups.Where(g => g.Status != "analyzed").OrderByDescending(g => g.Count).Take(25)
                        .Select(g => new { g.Status, g.Extension, g.Count, examples = g.Examples, remedy = Remedy(g.Status) }),
                    describable, described, withoutDescription = describable - described,
                });
            }
            return new
            {
                programs = result,
                note = UiText.Say("le cartelle di build e delle dipendenze (bin, obj, target, build, node_modules…) non si guardano, come nella scansione",
                    "build and dependency folders (bin, obj, target, build, node_modules…) are not looked at, as in the scan"),
            };
        }

        public sealed record FileGroup(string Status, string Extension, int Count, List<string> Examples);

        /// <summary>I file della cartella per stato: nel grafo, linguaggio scelto ma non estratto, linguaggio non scelto, nessun lettore.</summary>
        public static List<FileGroup> Classify(string projectPath, CodeProgramScope scope, CodeLanguageCatalog catalog, CancellationToken ct)
        {
            var chosen = new HashSet<string>(scope.Languages, StringComparer.OrdinalIgnoreCase);
            var groups = new Dictionary<(string, string), (int Count, List<string> Examples)>();
            var stack = new Stack<string>();
            stack.Push(scope.AbsolutePath);
            while (stack.Count > 0)
            {
                ct.ThrowIfCancellationRequested();
                var dir = stack.Pop();
                IEnumerable<string> entries;
                try { entries = Directory.EnumerateFileSystemEntries(dir).ToList(); }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException) { continue; }
                foreach (var e in entries)
                {
                    if (Directory.Exists(e)) { if (!CodeChangeScanner.IgnoredDirectoryNames.Contains(Path.GetFileName(e))) stack.Push(e); continue; }
                    var logical = scope.ToLogical(e);
                    var language = catalog.ForFile(e);
                    var status = language == null ? "no-parser"
                        : !chosen.Contains(language.Id) ? "language-not-chosen"
                        : File.Exists(CodeScriptWriter.ScriptPathFor(projectPath, logical, CodeScriptKind.Ast)) ? "analyzed" : "chosen-not-extracted";
                    var key = (status, Path.GetExtension(e).ToLowerInvariant() is { Length: > 0 } x ? x : "(none)");
                    groups.TryGetValue(key, out var g);
                    g.Examples ??= new List<string>();
                    if (g.Examples.Count < 3) g.Examples.Add(logical);
                    groups[key] = (g.Count + 1, g.Examples);
                }
            }
            return groups.Select(kv => new FileGroup(kv.Key.Item1, kv.Key.Item2, kv.Value.Count, kv.Value.Examples)).ToList();
        }

        private static string Remedy(string status) => status switch
        {
            "no-parser" => UiText.Say("nessun lettore per questo tipo di file (configurazione, template, dati): un dialetto di pagine o un linguaggio definito nel progetto può leggerlo, se conta",
                "no reader for this kind of file (configuration, template, data): a page dialect or a project-defined language can read it, if it matters"),
            "language-not-chosen" => UiText.Say("il linguaggio c'è ma non è scelto per questa cartella: «Ambiente di sviluppo»", "the language exists but is not chosen for this folder: «Development environment»"),
            "chosen-not-extracted" => UiText.Say("linguaggio scelto ma senza script: estrattore mancante (grammatica non scaricata) o «Estrai la struttura» non ancora fatto",
                "language chosen but no script: missing extractor (grammar not downloaded) or «Extract the structure» not done yet"),
            _ => null,
        };
    }
}
