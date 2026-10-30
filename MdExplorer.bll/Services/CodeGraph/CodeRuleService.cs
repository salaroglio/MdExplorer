using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Le regole di arricchimento di un progetto (D42, 23/10/2026): prova a secco, applicazione, rimozione e riapplicazione
    /// dopo una nuova scansione. Una regola applicata sono due file in <c>.mde-code/_rules/</c>, condivisi con Git: la
    /// regola (<c>&lt;id&gt;.rule.json</c>) e i suoi archi (<c>&lt;id&gt;.rules.ttl</c>), caricati in Fuseki come gli altri
    /// script. Togliere la regola toglie i suoi archi, e nient'altro.
    /// </summary>
    public sealed class CodeRuleService
    {
        private readonly ICodeGraphStore _store;
        private readonly ILogger<CodeRuleService> _logger;

        public CodeRuleService(ICodeGraphStore store, ILogger<CodeRuleService> logger)
        {
            _store = store;
            _logger = logger;
        }

        public static string DefinitionPath(string projectPath, string ruleId) => Path.Combine(CodeGraphPaths.GetRulesFolder(projectPath), ruleId + CodeGraphPaths.RuleDefinitionSuffix);
        public static string ScriptPath(string projectPath, string ruleId) => Path.Combine(CodeGraphPaths.GetRulesFolder(projectPath), ruleId + CodeGraphPaths.RuleScriptSuffix);

        /// <summary>Che cosa farebbe la regola, senza scrivere niente.</summary>
        public async Task<CodeRulePreview> PreviewAsync(string projectPath, CodeEnrichmentRule rule, CancellationToken ct = default)
        {
            rule.Validate();
            if (rule.Kind == CodeRuleKind.Registration) return await PreviewRegistrationAsync(projectPath, rule, ct);
            var aliasMap = ReadAliasMap(projectPath, rule);
            var sites = await _store.CallSitesAsync(projectPath, rule.Call.Callee, ct);
            var paired = rule.Owner.From == CodeRuleOwnerSource.PairedCall
                ? await _store.CallSitesAsync(projectPath, rule.Owner.PairedCall.Callee, ct)
                : new List<CodeCallSiteView>();
            List<CodeOwnerMember> owners;
            if (rule.Owner.From == CodeRuleOwnerSource.Page)
            {
                // tipo 3: le pagine dei dialetti di pagine del progetto
                var pageLanguages = CodeLanguageCatalog.ForProject(projectPath).All.Where(l => l.Definition?.Template != null).Select(l => l.Id).ToList();
                if (pageLanguages.Count == 0)
                    throw new CodeRuleException(UiText.Say("owner.from = page: il progetto non ha un dialetto di pagine (.mde-languages/<id>/ con template), quindi nel grafo non ci sono pagine.",
                        "owner.from = page: the project has no page dialect (.mde-languages/<id>/ with template), so there are no pages in the graph."));
                owners = await _store.PagesAsync(projectPath, pageLanguages, ct);
            }
            else owners = await _store.OwnersWithMembersAsync(projectPath, CodeRuleEngine.OwnerKeys(rule, sites, paired, aliasMap), ct);
            return CodeRuleEngine.Evaluate(rule, sites, paired, aliasMap, owners);
        }

        /// <summary>
        /// Una regola di registrazione (D43): i fatti dal grafo (punti con X.class e loro catene, o simboli annotati), le
        /// coppie, le classi, gli archi <c>resolvesTo</c>.
        /// </summary>
        private async Task<CodeRulePreview> PreviewRegistrationAsync(string projectPath, CodeEnrichmentRule rule, CancellationToken ct)
        {
            var sites = new List<CodeCallSiteView>();
            var chains = new Dictionary<string, List<CodeCallSiteView>>(StringComparer.Ordinal);
            var annotated = new List<CodeAnnotatedView>();
            if (rule.Anchor.Call != null)
            {
                sites = await _store.TypeCallSitesAsync(projectPath, rule.Anchor.Call, ct);
                foreach (var end in new[] { rule.Abstraction, rule.Implementation }.Where(e => e.From == CodeRuleEndSource.Chain).DistinctBy(e => e.Callee))
                    foreach (var kv in await _store.ChainsAsync(projectPath, rule.Anchor.Call, end.Callee, ct))
                        chains[kv.Key] = kv.Value;   // una sola chiamata della catena per regola: abstraction e implementation la condividono
            }
            else annotated = await _store.AnnotatedAsync(projectPath, rule.Anchor.Annotation, ct);
            var candidates = CodeRegistrationRuleEngine.Prepare(rule, sites, chains, annotated);
            var owners = await _store.OwnersWithMembersAsync(projectPath, CodeRegistrationRuleEngine.OwnerKeys(candidates), ct);
            return CodeRegistrationRuleEngine.Evaluate(rule, candidates, owners);
        }

        /// <summary>
        /// Applica la regola: se MdExplorer la respinge non scrive niente e lo dice con i motivi; se no scrive i due file e
        /// carica gli archi. Una regola con lo stesso id si sostituisce.
        /// </summary>
        public async Task<CodeRulePreview> ApplyAsync(string projectPath, CodeEnrichmentRule rule, string writtenBy, CancellationToken ct = default)
        {
            var preview = await PreviewAsync(projectPath, rule, ct);
            if (!preview.Accepted)
                throw new CodeRuleException(UiText.Say("Regola respinta: ", "Rule rejected: ") + string.Join(" ", preview.RejectReasons));
            rule.WrittenBy = writtenBy;
            rule.WrittenAt = DateTime.UtcNow.ToString("o");
            Directory.CreateDirectory(CodeGraphPaths.GetRulesFolder(projectPath));
            File.WriteAllText(DefinitionPath(projectPath, rule.Id), rule.ToJson(), new UTF8Encoding(false));
            await WriteAndLoadAsync(projectPath, rule, preview, ct);
            _logger?.LogInformation("[CodeRules] '{Project}': regola '{Rule}' applicata, {Edges} archi ({Resolved}/{Total} chiamate)",
                projectPath, rule.Id, preview.Edges.Count, preview.Resolved, preview.Resolved + preview.Unresolved);
            return preview;
        }

        /// <summary>Via la regola e i suoi archi. Falso se la regola non c'era.</summary>
        public async Task<bool> RemoveAsync(string projectPath, string ruleId, CancellationToken ct = default)
        {
            if (!CodeEnrichmentRule.IdShape.IsMatch(ruleId ?? string.Empty))
                throw new CodeRuleException(UiText.Say($"id '{ruleId}' non valido.", $"invalid id '{ruleId}'."));
            var definition = DefinitionPath(projectPath, ruleId);
            var script = ScriptPath(projectPath, ruleId);
            var existed = File.Exists(definition) || File.Exists(script);
            if (File.Exists(script))
            {
                File.Delete(script);
                await _store.RemoveScriptAsync(projectPath, script, ct);
            }
            if (File.Exists(definition)) File.Delete(definition);
            return existed;
        }

        /// <summary>Le regole del progetto, come stanno su disco. Una regola che non si legge è un errore con il nome del file.</summary>
        public List<CodeEnrichmentRule> List(string projectPath)
        {
            var folder = CodeGraphPaths.GetRulesFolder(projectPath);
            if (!Directory.Exists(folder)) return new List<CodeEnrichmentRule>();
            return Directory.EnumerateFiles(folder, "*" + CodeGraphPaths.RuleDefinitionSuffix).OrderBy(f => f, StringComparer.Ordinal).Select(f =>
            {
                try { return CodeEnrichmentRule.Parse(File.ReadAllText(f)); }
                catch (CodeRuleException ex) { throw new CodeRuleException($"'{Path.GetFileName(f)}': {ex.Message}"); }
            }).ToList();
        }

        /// <summary>
        /// Dopo una nuova scansione le chiamate cambiano: ogni regola rifà i suoi archi. Una regola già approvata non si
        /// respinge da sola: se ora collega meno, i numeri lo dicono e il log lo segnala.
        /// </summary>
        public async Task<List<CodeRulePreview>> ReapplyAllAsync(string projectPath, CancellationToken ct = default)
        {
            var previews = new List<CodeRulePreview>();
            foreach (var rule in List(projectPath))
            {
                var preview = await PreviewAsync(projectPath, rule, ct);
                await WriteAndLoadAsync(projectPath, rule, preview, ct);
                if (!preview.Accepted)
                    _logger?.LogWarning("[CodeRules] '{Project}': la regola '{Rule}' ora non passerebbe i controlli: {Why}", projectPath, rule.Id, string.Join(" ", preview.RejectReasons));
                previews.Add(preview);
            }
            return previews;
        }

        /// <summary>L'id dei collegamenti che MdExplorer fa da sé fra pagine e script: col trattino basso, nessuna regola può chiamarsi così.</summary>
        public const string PageScriptsId = "_page-scripts";

        /// <summary>
        /// La seconda passata delle pagine (D42 passo 2c, 24/10/2026): a grafo caricato, ogni chiamata di una pagina verso una
        /// funzione dei file JS che la pagina carica (o delle pagine che include) diventa un arco <c>calls</c>, <c>extracted</c>
        /// se la funzione è una sola, <c>ambiguous</c> se sono di più. Per Angular (D42 B) anche: componente «renders» il suo
        /// template, chiamata del template → metodo del componente, <c>&lt;app-x&gt;</c> «instantiates» il componente. Nessuna
        /// approvazione: sono le regole standard di HTML, JS e Angular, non una convenzione del progetto. Gli archi stanno in <c>.mde-code/_rules/_page-scripts.rules.ttl</c>, rifatto a ogni
        /// scansione; senza pagine il file non c'è. Torna quanti archi ha scritto.
        /// </summary>
        public async Task<int> ApplyPageScriptsAsync(string projectPath, IReadOnlyCollection<string> pageLanguages, CancellationToken ct = default)
        {
            var path = ScriptPath(projectPath, PageScriptsId);
            if (pageLanguages == null || pageLanguages.Count == 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    await _store.RemoveScriptAsync(projectPath, path, ct);
                }
                return 0;
            }
            var links = (await _store.PageScriptCallsAsync(projectPath, pageLanguages, ct))
                .Select(c => new CodePageLink { SourceId = c.CallerId, TargetId = c.FunctionId, Relation = "calls", Name = c.Callee, Line = c.Line, Path = c.Path })
                .Concat(await _store.AngularLinksAsync(projectPath, pageLanguages, ct));
            // più bersagli per lo stesso nome nella stessa riga = ambiguous; un arco per (sorgente, bersaglio, relazione), come negli ast
            var edges = links.GroupBy(l => (l.SourceId, l.Relation, l.Name, l.Line))
                .SelectMany(g =>
                {
                    var targets = g.Select(l => l.TargetId).Distinct(StringComparer.Ordinal).ToList();
                    return targets.Select(t => new ScriptEdge
                    {
                        SourceId = g.Key.SourceId, TargetId = t, Relation = g.Key.Relation, Line = g.Key.Line, Path = g.First().Path,
                        Confidence = targets.Count == 1 ? "extracted" : "ambiguous",
                    });
                })
                .GroupBy(e => (e.SourceId, e.TargetId, e.Relation)).Select(g => g.First())
                .OrderBy(e => e.Path, StringComparer.Ordinal).ThenBy(e => e.Line).ThenBy(e => e.SourceId, StringComparer.Ordinal).ThenBy(e => e.TargetId, StringComparer.Ordinal)
                .ToList();
            var script = new CodeScript
            {
                Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Rule, Source = PageScriptsId,
                    WrittenBy = "MdExplorer", WrittenAt = string.Empty,
                },
                RuleDescription = "MdExplorer: le chiamate delle pagine verso le funzioni dei file JS che caricano (anche dalle pagine incluse) e, per Angular, componente ↔ template e <app-x> → componente (D42).",
            };
            script.Edges.AddRange(edges);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var text = CodeScriptTurtle.Write(script);
            // lo stesso testo non si riscrive: niente modifiche fasulle per Git a ogni scansione
            if (!File.Exists(path) || File.ReadAllText(path, Encoding.UTF8) != text) File.WriteAllText(path, text, new UTF8Encoding(false));
            var loaded = await _store.LoadScriptAsync(projectPath, path, ct);
            if (loaded.Error != null) throw new CodeRuleException(loaded.Error);
            _logger?.LogInformation("[CodeRules] '{Project}': {Count} chiamate delle pagine collegate alle funzioni JS", projectPath, edges.Count);
            return edges.Count;
        }

        /// <summary>L'id degli archi delle registrazioni (<see cref="CodeRegistrations"/>): col trattino basso, come <see cref="PageScriptsId"/>.</summary>
        public const string RegistrationsId = "_registrations";

        /// <summary>
        /// Le registrazioni del progetto (30/10/2026) diventano archi <c>resolvesTo</c> in
        /// <c>.mde-code/_rules/_registrations.rules.ttl</c>, rifatto a ogni scansione da TUTTI i file di registrazione del
        /// progetto (<paramref name="files"/>: percorso logico e testo); senza registrazioni il file non c'è. Nessuna
        /// approvazione: è lo standard del meccanismo (ServiceLoader), non una convenzione del progetto. Torna gli archi
        /// scritti e le righe che il grafo non sa risolvere.
        /// </summary>
        public async Task<(int Edges, List<string> Misses)> ApplyRegistrationsAsync(string projectPath, IReadOnlyList<(string Path, string Text)> files, CancellationToken ct = default)
        {
            var path = ScriptPath(projectPath, RegistrationsId);
            var registrations = files.Where(f => CodeRegistrations.IsServiceLoaderFile(f.Path)).OrderBy(f => f.Path, StringComparer.Ordinal)
                .SelectMany(f => CodeRegistrations.ParseServiceLoader(f.Path, f.Text)).ToList();
            if (registrations.Count == 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    await _store.RemoveScriptAsync(projectPath, path, ct);
                }
                return (0, new List<string>());
            }
            var owners = await _store.OwnersWithMembersAsync(projectPath,
                registrations.SelectMany(r => new[] { r.AbstractionQualifiedName, r.ImplementationQualifiedName }), ct);
            // la chiave cerca anche per nome semplice: qui conta solo il nome qualificato
            var edges = CodeRegistrations.Edges(registrations, owners, out var misses);
            var script = new CodeScript
            {
                Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Rule, Source = RegistrationsId,
                    WrittenBy = "MdExplorer", WrittenAt = string.Empty,
                },
                RuleDescription = "MdExplorer: le registrazioni (Java ServiceLoader, META-INF/services) — l'interfaccia, e ogni suo metodo, si risolve nelle implementazioni registrate.",
            };
            script.Edges.AddRange(edges);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var text = CodeScriptTurtle.Write(script);
            if (!File.Exists(path) || File.ReadAllText(path, Encoding.UTF8) != text) File.WriteAllText(path, text, new UTF8Encoding(false));
            var loaded = await _store.LoadScriptAsync(projectPath, path, ct);
            if (loaded.Error != null) throw new CodeRuleException(loaded.Error);
            _logger?.LogInformation("[CodeRules] '{Project}': {Registrations} registrazioni, {Edges} archi resolvesTo, {Misses} righe fuori dal grafo",
                projectPath, registrations.Count, edges.Count, misses.Count);
            return (edges.Count, misses);
        }

        private async Task WriteAndLoadAsync(string projectPath, CodeEnrichmentRule rule, CodeRulePreview preview, CancellationToken ct)
        {
            var script = new CodeScript
            {
                Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Rule, Source = rule.Id,
                    WrittenBy = rule.WrittenBy ?? string.Empty, WrittenAt = rule.WrittenAt ?? string.Empty,
                },
                RuleDescription = rule.Description,
            };
            script.Edges.AddRange(preview.Edges);
            var path = ScriptPath(projectPath, rule.Id);
            // le descrizioni dei collegamenti (D43) restano per i punti che ci sono ancora: se il fatto è cambiato lo dicono
            // le impronte, e «Descrivi con Mark» le rifà
            var points = preview.Edges.Where(e => e.Path != null && e.Line != null).Select(e => (e.Path, e.Line.Value)).ToHashSet();
            script.LinkDocs.AddRange((ReadScript(projectPath, rule.Id)?.LinkDocs ?? new List<ScriptLinkDoc>()).Where(d => points.Contains((d.Path, d.Line))));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, CodeScriptTurtle.Write(script), new UTF8Encoding(false));
            var loaded = await _store.LoadScriptAsync(projectPath, path, ct);
            if (loaded.Error != null) throw new CodeRuleException(loaded.Error);
        }

        /// <summary>Lo script degli archi di una regola com'è su disco; null se non c'è.</summary>
        public static CodeScript ReadScript(string projectPath, string ruleId)
        {
            var path = ScriptPath(projectPath, ruleId);
            return File.Exists(path) ? CodeScriptParser.Parse(File.ReadAllText(path, Encoding.UTF8), path) : null;
        }

        /// <summary>
        /// Le descrizioni dei collegamenti nuove (D43), fuse con quelle che ci sono (stesso punto = si sostituisce), scritte
        /// nello script della regola e caricate. Chi chiama tiene il lucchetto delle scritture.
        /// </summary>
        public async Task SaveLinkDocsAsync(string projectPath, string ruleId, IEnumerable<ScriptLinkDoc> docs, CancellationToken ct = default)
        {
            var script = ReadScript(projectPath, ruleId) ?? throw new CodeRuleException(UiText.Say($"La regola «{ruleId}» non ha archi su disco.", $"The rule «{ruleId}» has no edges on disk."));
            foreach (var d in docs)
            {
                script.LinkDocs.RemoveAll(x => x.Path == d.Path && x.Line == d.Line);
                script.LinkDocs.Add(d);
            }
            var path = ScriptPath(projectPath, ruleId);
            File.WriteAllText(path, CodeScriptTurtle.Write(script), new UTF8Encoding(false));
            var loaded = await _store.LoadScriptAsync(projectPath, path, ct);
            if (loaded.Error != null) throw new CodeRuleException(loaded.Error);
        }

        private static IReadOnlyDictionary<string, string> ReadAliasMap(string projectPath, CodeEnrichmentRule rule)
        {
            if (rule.Owner.AliasMap == null) return null;
            var path = Path.Combine(projectPath, rule.Owner.AliasMap.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                throw new CodeRuleException(UiText.Say($"La mappa degli alias '{rule.Owner.AliasMap}' non c'è nel progetto.", $"The alias map '{rule.Owner.AliasMap}' is not in the project."));
            return PropertiesFile.Parse(File.ReadAllText(path));
        }
    }
}
