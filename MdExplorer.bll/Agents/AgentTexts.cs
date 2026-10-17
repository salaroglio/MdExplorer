using System.Collections.Generic;
using System.Linq;
using System.Text;
using MdExplorer.Features.Agents.Workflow.Scheduler;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// I testi che MdExplorer scrive agli agenti e alla persona nella posta, nella lingua dell'interfaccia (16/10/2026).
    /// <para>
    /// Le etichette in testa ai messaggi agli agenti sono il protocollo con le schede: <c>[INCARICO]</c>/<c>[ASSIGNMENT]</c> e
    /// <c>[RISPOSTA LIBERA]</c>/<c>[FREE REPLY]</c>. Una scheda deve riconoscerle tutte e due, perché su un progetto lavorano
    /// persone con lingue diverse e ogni computer scrive nella sua.
    /// </para>
    /// </summary>
    public static class AgentTexts
    {
        public const string AssignmentIt = "[INCARICO]";
        public const string AssignmentEn = "[ASSIGNMENT]";
        public const string FreeReplyIt = "[RISPOSTA LIBERA]";
        public const string FreeReplyEn = "[FREE REPLY]";

        /// <summary>«it» o qualunque altra cosa (inglese): come <c>IUiLanguage.Current</c>.</summary>
        public static bool IsItalian(string lang) => lang == "it";

        public static string Pick(string lang, string it, string en) => IsItalian(lang) ? it : en;

        /// <summary>
        /// L'incarico di un passo del workflow, composto da MDE (workflow v3, F3): il lavoro della scheda, gli ingressi
        /// risolti (i file con il commit approvato), le uscite da consegnare, il brief del workflow, il giro e il passo.
        /// </summary>
        public static string Assignment(string lang, SchedulerAction action, string roundId)
        {
            var it = IsItalian(lang);
            string Say(string i, string e) => it ? i : e;
            var description = (action.SkillDescription ?? "").Trim().TrimEnd('.');
            var text = new StringBuilder((it ? AssignmentIt : AssignmentEn) + " "
                + Say($"Lavoro «{action.Skill}» della tua scheda", $"Job «{action.Skill}» of your card")
                + (description.Length > 0 ? ": " + description : "") + ".");

            if (action.Inputs.Count > 0)
            {
                text.Append(Say("\n\nIngressi:", "\n\nInputs:"));
                foreach (var input in action.Inputs)
                {
                    var head = "\n- " + input.Id + (string.IsNullOrWhiteSpace(input.Description) ? "" : $" ({input.Description.Trim()})");
                    if (input.Text != null) { text.Append(head + ": " + input.Text); continue; }
                    text.Append(head + ":");
                    foreach (var f in input.Files)
                        text.Append($"\n  - {f.Path} — " + Say($"uscita «{f.Output}» di «{f.StepLabel}», approvata", $"output «{f.Output}» of «{f.StepLabel}», approved")
                                    + (f.Commit == null ? "" : Say($" nel commit {f.Commit}", $" in commit {f.Commit}")));
                }
            }

            if (action.Outputs.Count > 0)
            {
                text.Append(Say("\n\nConsegni:", "\n\nYou deliver:"));
                foreach (var o in action.Outputs)
                {
                    var verb = o.Action switch
                    {
                        "updates" => Say("aggiorni", "you update"),
                        "downloads" => Say("scarichi", "you download"),
                        _ => Say("scrivi", "you write"),
                    };
                    text.Append($"\n- {o.Id}: {o.Path} ({verb})"
                        + (o.Path.Contains('*') ? Say("; il «*» del nome lo completi tu", "; you fill in the «*» of the name") : "")
                        + (string.IsNullOrWhiteSpace(o.Note) ? "" : " — " + o.Note.Trim()));
                }
            }

            if (!string.IsNullOrWhiteSpace(action.Brief))
                text.Append(Say("\n\nIndicazioni del workflow: ", "\n\nWorkflow notes: ") + action.Brief.Trim());

            text.Append(Say($"\n\nGiro {roundId}, passo «{action.Step.Label}»", $"\n\nRound {roundId}, step «{action.Step.Label}»"));
            if (action.Round > 1) text.Append(Say($", giro {action.Round}", $", round {action.Round}"));
            if (action.Attempt > 1) text.Append(Say($", tentativo {action.Attempt}", $", attempt {action.Attempt}"));
            return text.Append('.').ToString();
        }

        /// <summary>La persona ha scritto con parole sue a un messaggio che proponeva pulsanti.</summary>
        public static string FreeReply(string lang, string personText, string quotedPrevious, IReadOnlyList<ResolvedReply> offered)
        {
            var it = IsItalian(lang);
            var replies = offered == null || offered.Count == 0
                ? (it ? "nessuna" : "none")
                : string.Join("; ", offered.Select(r => it ? $"«{r.Label}» (messaggio: {r.Message})" : $"«{r.Label}» (message: {r.Message})"));
            return it
                ? $"{FreeReplyIt} {(personText ?? "").Trim()}\n\nLa persona ti ha scritto con parole sue, rispondendo a questo tuo messaggio:\n{quotedPrevious}\n\nRisposte che le avevi proposto come pulsanti: {replies}."
                : $"{FreeReplyEn} {(personText ?? "").Trim()}\n\nThe person wrote to you in their own words, replying to this message of yours:\n{quotedPrevious}\n\nReplies you had offered as buttons: {replies}.";
        }
    }
}
