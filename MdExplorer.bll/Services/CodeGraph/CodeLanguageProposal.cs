using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un file del campione verificato da MarkAgent nel sorgente: i fatti trovati ci sono davvero (D42 passo 2).</summary>
    public sealed class CodeLanguageSampleCheck
    {
        public string Path { get; set; }
        public bool Ok { get; set; }
        public string? Note { get; set; }
    }

    /// <summary>
    /// Un linguaggio (di solito un dialetto di pagine) che MarkAgent propone all'utente (D42 passo 2): aspetta il «Sì» del
    /// wizard come le regole. Non è su disco: un linguaggio non approvato non va su Git.
    /// </summary>
    public sealed class CodeLanguageProposal
    {
        public string Id { get; set; }
        public string ProjectPath { get; set; }
        /// <summary>Il testo di <c>language.yml</c>, come Mark l'ha provato.</summary>
        public string Definition { get; set; }
        public string Problem { get; set; }
        public List<CodeLanguageSampleCheck> SampleChecks { get; set; } = new();
        public List<CodeRuleAnalysis> FurtherAnalyses { get; set; } = new();
        public CodeLanguagePreview Preview { get; set; }
        public string ProposedBy { get; set; }
        public string ProposedAt { get; set; }
    }

    public static class CodeLanguageProposals
    {
        /// <summary>Che cosa manca perché la proposta arrivi all'utente: ogni file del campione aperto e giusto. Vuoto = si può proporre.</summary>
        public static List<string> SampleProblems(CodeLanguagePreview preview, IReadOnlyList<CodeLanguageSampleCheck> checks)
        {
            var problems = new List<string>();
            checks ??= Array.Empty<CodeLanguageSampleCheck>();
            foreach (var s in preview.Sample)
            {
                var check = checks.FirstOrDefault(c => string.Equals(c.Path, s.Path, StringComparison.Ordinal));
                if (check == null)
                    problems.Add(UiText.Say($"{s.Path} non verificato nel sorgente.", $"{s.Path} not checked in the source."));
                else if (!check.Ok)
                    problems.Add(UiText.Say($"{s.Path} è sbagliato: {check.Note}. Correggi il linguaggio.", $"{s.Path} is wrong: {check.Note}. Fix the language."));
            }
            return problems;
        }
    }
}
