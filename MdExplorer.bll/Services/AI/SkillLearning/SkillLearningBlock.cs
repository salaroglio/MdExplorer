using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.AI.SkillLearning
{
    /// <summary>Una proposta del modello, e che cosa non va (sprint 2026-11-24-Skill-Che-Migliorano, F2).</summary>
    public sealed class SkillProposal
    {
        public const string ActionCreate = "create";
        public const string ActionUpdate = "update";

        /// <summary>Posizione nel blocco (0, 1, …): la identifica anche quando nome o azione non sono validi.</summary>
        public int Index { get; init; }
        public string Action { get; init; }
        public string Skill { get; init; }
        public string Why { get; init; }
        public IReadOnlyList<long> Evidence { get; init; } = Array.Empty<long>();
        /// <summary>Il testo intero di SKILL.md proposto.</summary>
        public string Content { get; init; }
        /// <summary>Perché non si può approvare, detto all'utente. Vuota = valida.</summary>
        public IReadOnlyList<string> Problems { get; init; } = Array.Empty<string>();
        public bool IsValid => Problems.Count == 0;
    }

    /// <summary>La risposta letta: la prosa per l'utente, le proposte, e i problemi del blocco intero.</summary>
    public sealed class SkillLearningAnswer
    {
        /// <summary>Il testo della risposta senza il blocco: che cosa il modello ha imparato.</summary>
        public string Prose { get; init; }
        public IReadOnlyList<SkillProposal> Proposals { get; init; } = Array.Empty<SkillProposal>();
        /// <summary>Il blocco manca, non è chiuso o non si legge: niente proposte, e il motivo.</summary>
        public string BlockProblem { get; init; }
    }

    /// <summary>Che cosa serve sapere del progetto per controllare le proposte.</summary>
    public sealed class SkillLearningContext
    {
        /// <summary>Le skill presenti nella cartella del motore (nome → è una mde-* di MdExplorer).</summary>
        public IReadOnlyDictionary<string, bool> ExistingSkills { get; init; } = new Dictionary<string, bool>();
        /// <summary>L'intervallo dell'attività: le prove devono cadere qui dentro.</summary>
        public long From { get; init; }
        public long To { get; init; }
    }

    /// <summary>
    /// Il blocco con cui il modello risponde a «Impara da questa attività» (prompt mde-mark-learn):
    /// <code>&lt;&lt;&lt;mde-learn {"proposals":[{action, skill, why, evidence, content}]} mde-learn&gt;&gt;&gt;</code>
    /// Il modello ragiona su che cosa proporre; qui si legge il blocco e si controlla ogni proposta. Una proposta che non
    /// passa resta, con i suoi motivi: l'utente vede perché, niente sparisce in silenzio.
    /// </summary>
    public static class SkillLearningBlock
    {
        public const string Start = "<<<mde-learn";
        public const string End = "mde-learn>>>";
        public const int MaxNameLength = 64;
        /// <summary>Oltre, una skill è troppo lunga per servire: si dice di dividerla (lo stesso tetto delle skill mde-*).</summary>
        public const int MaxContentLines = 500;

        private static readonly Regex NamePattern = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

        public static SkillLearningAnswer Read(string answer, SkillLearningContext context)
        {
            context ??= new SkillLearningContext();
            var text = answer ?? string.Empty;
            var start = text.LastIndexOf(Start, StringComparison.Ordinal);
            if (start < 0)
                return new SkillLearningAnswer { Prose = WithoutThreadBlock(text), BlockProblem = UiText.Say(
                    "il modello non ha mandato il blocco delle proposte", "the model did not send the proposals block") };
            var end = text.IndexOf(End, start + Start.Length, StringComparison.Ordinal);
            var prose = WithoutThreadBlock(text.Substring(0, start));
            if (end < 0)
                return new SkillLearningAnswer { Prose = prose, BlockProblem = UiText.Say(
                    "il blocco delle proposte è cominciato ma non è stato chiuso", "the proposals block was started but not closed") };

            var inner = text.Substring(start + Start.Length, end - start - Start.Length).Trim();
            JsonDocument doc;
            try { doc = JsonDocument.Parse(inner); }
            catch (JsonException ex)
            {
                return new SkillLearningAnswer { Prose = prose, BlockProblem = UiText.Say(
                    $"il blocco delle proposte non è JSON valido ({ex.Message})", $"the proposals block is not valid JSON ({ex.Message})") };
            }
            using (doc)
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object || !doc.RootElement.TryGetProperty("proposals", out var list)
                    || list.ValueKind != JsonValueKind.Array)
                    return new SkillLearningAnswer { Prose = prose, BlockProblem = UiText.Say(
                        "nel blocco manca l'elenco «proposals»", "the block has no «proposals» list") };

                var proposals = new List<SkillProposal>();
                var index = 0;
                foreach (var item in list.EnumerateArray())
                    proposals.Add(Check(index++, item, context));

                // Due proposte sulla stessa skill si pesterebbero: le si segna entrambe.
                foreach (var dup in proposals.Where(p => !string.IsNullOrEmpty(p.Skill)).GroupBy(p => p.Skill).Where(g => g.Count() > 1).SelectMany(g => g).ToList())
                {
                    var i = proposals.IndexOf(dup);
                    proposals[i] = With(dup, UiText.Say($"c'è un'altra proposta sulla stessa skill «{dup.Skill}»", $"another proposal is about the same skill «{dup.Skill}»"));
                }
                return new SkillLearningAnswer { Prose = prose, Proposals = proposals };
            }
        }

        private static SkillProposal Check(int index, JsonElement item, SkillLearningContext context)
        {
            var problems = new List<string>();
            if (item.ValueKind != JsonValueKind.Object)
                return new SkillProposal { Index = index, Problems = new[] { UiText.Say("la proposta non è un oggetto JSON", "the proposal is not a JSON object") } };

            var action = Str(item, "action");
            var skill = Str(item, "skill")?.Trim();
            var why = Str(item, "why")?.Trim();
            var content = Str(item, "content");
            var evidence = new List<long>();
            if (item.TryGetProperty("evidence", out var ev) && ev.ValueKind == JsonValueKind.Array)
                foreach (var e in ev.EnumerateArray())
                    if (e.ValueKind == JsonValueKind.Number && e.TryGetInt64(out var seq)) evidence.Add(seq);

            if (action != SkillProposal.ActionCreate && action != SkillProposal.ActionUpdate)
                problems.Add(UiText.Say($"azione «{action}» sconosciuta: create o update", $"unknown action «{action}»: create or update"));

            var nameProblem = NameProblem(skill);
            if (nameProblem != null) problems.Add(nameProblem);
            else
            {
                var exists = context.ExistingSkills.TryGetValue(skill, out var isMde);
                if (action == SkillProposal.ActionCreate && exists)
                    problems.Add(UiText.Say($"la skill «{skill}» c'è già: va proposta come update", $"the skill «{skill}» already exists: propose it as an update"));
                if (action == SkillProposal.ActionUpdate && !exists)
                    problems.Add(UiText.Say($"la skill «{skill}» da correggere non c'è nel progetto", $"the skill «{skill}» to update is not in the project"));
                if (action == SkillProposal.ActionUpdate && exists && isMde)
                    problems.Add(UiText.Say($"«{skill}» è di MdExplorer: si legge, non si corregge", $"«{skill}» belongs to MdExplorer: read only"));
            }

            if (string.IsNullOrWhiteSpace(why))
                problems.Add(UiText.Say("manca il perché («why»)", "the reason («why») is missing"));

            if (evidence.Count == 0)
                problems.Add(UiText.Say("mancano le prove dalla chat («evidence»)", "the evidence from the chat («evidence») is missing"));
            else
            {
                var outside = evidence.Where(s => s < context.From || s > context.To).ToList();
                if (context.To > 0 && outside.Count > 0)
                    problems.Add(UiText.Say($"prove fuori dall'attività ({context.From}-{context.To}): {string.Join(", ", outside)}",
                        $"evidence outside the activity ({context.From}-{context.To}): {string.Join(", ", outside)}"));
            }

            var contentProblem = ContentProblem(content, skill);
            if (contentProblem != null) problems.Add(contentProblem);

            return new SkillProposal
            {
                Index = index, Action = action, Skill = skill, Why = why, Evidence = evidence, Content = content, Problems = problems,
            };
        }

        /// <summary>Null se il nome va bene per una skill imparata; altrimenti il motivo.</summary>
        public static string NameProblem(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return UiText.Say("manca il nome della skill", "the skill name is missing");
            if (name.StartsWith("mde-", StringComparison.Ordinal))
                return UiText.Say($"«{name}»: i nomi mde-* sono di MdExplorer e verrebbero sovrascritti", $"«{name}»: mde-* names belong to MdExplorer and would be overwritten");
            if (name.Length > MaxNameLength)
                return UiText.Say($"nome di {name.Length} caratteri invece di {MaxNameLength} al massimo", $"name of {name.Length} characters instead of at most {MaxNameLength}");
            if (!NamePattern.IsMatch(name))
                return UiText.Say($"«{name}»: solo lettere minuscole, cifre e trattini", $"«{name}»: only lowercase letters, digits and hyphens");
            return null;
        }

        /// <summary>Null se il testo di SKILL.md è accettabile; altrimenti il motivo.</summary>
        public static string ContentProblem(string content, string skill)
        {
            if (string.IsNullOrWhiteSpace(content)) return UiText.Say("manca il testo della skill («content»)", "the skill text («content») is missing");
            var m = Regex.Match(content, @"\A---[ \t]*\r?\n(.*?)\r?\n---[ \t]*(\r?\n|\z)", RegexOptions.Singleline);
            if (!m.Success) return UiText.Say("il testo non comincia con il frontmatter (---)", "the text does not start with the frontmatter (---)");
            var fm = m.Groups[1].Value;
            var name = Regex.Match(fm, @"^name:\s*(.*)$", RegexOptions.Multiline);
            if (!name.Success) return UiText.Say("nel frontmatter manca «name»", "«name» is missing from the frontmatter");
            var declared = name.Groups[1].Value.Trim().Trim('"', '\'');
            if (skill != null && declared != skill)
                return UiText.Say($"nel frontmatter name è «{declared}», ma la skill è «{skill}»", $"the frontmatter name is «{declared}», but the skill is «{skill}»");
            var description = Regex.Match(fm, @"^description:\s*(.*)$", RegexOptions.Multiline);
            if (!description.Success || description.Groups[1].Value.Trim().Trim('"', '\'').Length == 0)
                return UiText.Say("nel frontmatter manca «description»", "«description» is missing from the frontmatter");
            if (Regex.IsMatch(fm, @"^(mde|learned):", RegexOptions.Multiline))
                return UiText.Say("il frontmatter non deve avere i blocchi «mde:» o «learned:»: li scrive MdExplorer", "the frontmatter must not have «mde:» or «learned:»: MdExplorer writes them");
            var lines = content.Split('\n').Length;
            if (lines > MaxContentLines)
                return UiText.Say($"{lines} righe invece di {MaxContentLines} al massimo: va divisa", $"{lines} lines instead of at most {MaxContentLines}: split it");
            return null;
        }

        /// <summary>
        /// Il blocco titolo/dettaglio della chat (mde-thread) non c'entra con «Impara», ma un modello che lo ha mandato a ogni
        /// turno lo aggiunge per abitudine anche qui (visto il 25/11/2026 con opencode): fuori dalla prosa.
        /// </summary>
        private static string WithoutThreadBlock(string text)
            => Regex.Replace(text ?? string.Empty, Regex.Escape(AI.ChatHistory.ChatThreadBlock.Start) + ".*?" + Regex.Escape(AI.ChatHistory.ChatThreadBlock.End),
                string.Empty, RegexOptions.Singleline).Trim();

        private static string Str(JsonElement item, string name)
            => item.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private static SkillProposal With(SkillProposal p, string problem) => new()
        {
            Index = p.Index, Action = p.Action, Skill = p.Skill, Why = p.Why, Evidence = p.Evidence, Content = p.Content,
            Problems = p.Problems.Concat(new[] { problem }).ToList(),
        };
    }
}
