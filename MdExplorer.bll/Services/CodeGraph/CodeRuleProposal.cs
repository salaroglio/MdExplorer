using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Un collegamento del campione verificato da MarkAgent nel sorgente (punto 3 della robustezza, D42): il punto di chiamata
    /// (file e riga, come nel campione della prova a secco) e se il metodo trovato è davvero quello eseguito.
    /// </summary>
    public sealed class CodeRuleSampleCheck
    {
        public string Path { get; set; }
        public int Line { get; set; }
        public bool Ok { get; set; }
        public string? Note { get; set; }
    }

    /// <summary>Un'analisi che si potrebbe fare, aggiungere o espandere, nata da un problema (D42): resta come proposta.</summary>
    public sealed class CodeRuleAnalysis
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        /// <summary>Il problema da cui è nata (quello del wizard).</summary>
        public string? Problem { get; set; }
        public string? ProposedBy { get; set; }
        public string? ProposedAt { get; set; }
        /// <summary><c>proposta</c>, <c>fatta</c>, <c>scartata</c>.</summary>
        public string? Status { get; set; } = "proposta";
    }

    /// <summary>
    /// Una regola che MarkAgent propone all'utente (D42): aspetta nel tab MarkAgent il «Sì» del wizard. Non è su disco:
    /// una regola non approvata non va su Git.
    /// </summary>
    public sealed class CodeRuleProposal
    {
        public string Id { get; set; }
        public string ProjectPath { get; set; }
        public CodeEnrichmentRule Rule { get; set; }
        /// <summary>Il problema che la regola risolve, in parole semplici: la prima cosa che il wizard mostra.</summary>
        public string Problem { get; set; }
        public List<CodeRuleSampleCheck> SampleChecks { get; set; } = new();
        public List<CodeRuleAnalysis> FurtherAnalyses { get; set; } = new();
        public CodeRulePreview Preview { get; set; }
        public string ProposedBy { get; set; }
        public string ProposedAt { get; set; }
    }

    /// <summary>I controlli sulla proposta e l'elenco delle analisi proposte del progetto.</summary>
    public static class CodeRuleProposals
    {
        public const string AnalysesFileName = "analisi-proposte.json";

        public static string AnalysesPath(string projectPath) => Path.Combine(CodeGraphPaths.GetRulesFolder(projectPath), AnalysesFileName);

        /// <summary>
        /// Che cosa manca perché la proposta arrivi all'utente: ogni collegamento del campione verificato e giusto. Vuoto =
        /// si può proporre. Il campione è deterministico: è lo stesso che <c>code_rule_try</c> ha mostrato a Mark.
        /// </summary>
        public static List<string> SampleProblems(CodeRulePreview preview, IReadOnlyList<CodeRuleSampleCheck> checks)
        {
            var problems = new List<string>();
            checks ??= Array.Empty<CodeRuleSampleCheck>();
            foreach (var s in preview.Sample)
            {
                var check = checks.FirstOrDefault(c => c.Line == s.Site.Line && string.Equals(c.Path, s.Site.Path, StringComparison.Ordinal));
                if (check == null)
                    problems.Add(UiText.Say($"{s.Site.Path}:{s.Site.Line} («{s.Value}») non verificato nel sorgente.", $"{s.Site.Path}:{s.Site.Line} («{s.Value}») not checked in the source."));
                else if (!check.Ok)
                    problems.Add(UiText.Say($"{s.Site.Path}:{s.Site.Line} («{s.Value}») è sbagliato: {check.Note}. Correggi la regola.", $"{s.Site.Path}:{s.Site.Line} («{s.Value}») is wrong: {check.Note}. Fix the rule."));
            }
            return problems;
        }

        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public static List<CodeRuleAnalysis> ReadAnalyses(string projectPath)
        {
            var path = AnalysesPath(projectPath);
            if (!File.Exists(path)) return new List<CodeRuleAnalysis>();
            try { return JsonSerializer.Deserialize<List<CodeRuleAnalysis>>(File.ReadAllText(path), Json) ?? new List<CodeRuleAnalysis>(); }
            catch (JsonException ex) { throw new CodeRuleException(UiText.Say($"'{AnalysesFileName}' non si legge: {ex.Message}", $"'{AnalysesFileName}' cannot be read: {ex.Message}")); }
        }

        /// <summary>Aggiunge le analisi nuove (lo stesso titolo non si ripete); torna l'elenco intero.</summary>
        public static List<CodeRuleAnalysis> AddAnalyses(string projectPath, IEnumerable<CodeRuleAnalysis> items, string problem, string proposedBy)
        {
            var list = ReadAnalyses(projectPath);
            var added = false;
            foreach (var a in items ?? Enumerable.Empty<CodeRuleAnalysis>())
            {
                if (string.IsNullOrWhiteSpace(a?.Title) || list.Any(x => string.Equals(x.Title?.Trim(), a.Title.Trim(), StringComparison.OrdinalIgnoreCase))) continue;
                list.Add(new CodeRuleAnalysis
                {
                    Title = a.Title.Trim(), Description = a.Description?.Trim(), Problem = problem, ProposedBy = proposedBy,
                    ProposedAt = DateTime.UtcNow.ToString("o"), Status = "proposta",
                });
                added = true;
            }
            if (added)
            {
                Directory.CreateDirectory(CodeGraphPaths.GetRulesFolder(projectPath));
                File.WriteAllText(AnalysesPath(projectPath), JsonSerializer.Serialize(list, Json), new UTF8Encoding(false));
            }
            return list;
        }
    }
}
