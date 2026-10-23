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
            var aliasMap = ReadAliasMap(projectPath, rule);
            var sites = await _store.CallSitesAsync(projectPath, rule.Call.Callee, ct);
            var paired = rule.Owner.From == CodeRuleOwnerSource.PairedCall
                ? await _store.CallSitesAsync(projectPath, rule.Owner.PairedCall.Callee, ct)
                : new List<CodeCallSiteView>();
            var keys = CodeRuleEngine.OwnerKeys(rule, sites, paired, aliasMap);
            var owners = await _store.OwnersWithMembersAsync(projectPath, keys, ct);
            return CodeRuleEngine.Evaluate(rule, sites, paired, aliasMap, owners);
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
            Directory.CreateDirectory(Path.GetDirectoryName(path));
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
