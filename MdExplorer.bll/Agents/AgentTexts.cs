using System.Collections.Generic;
using System.Linq;

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

        /// <summary>L'incarico di un passo del workflow: il brief, il giro e il passo, e i file approvati da cui parte.</summary>
        public static string Assignment(string lang, string brief, string roundId, string stepLabel, int round, int attempt, IReadOnlyList<string> inputs)
        {
            var it = IsItalian(lang);
            var text = (it ? AssignmentIt : AssignmentEn) + " " + (brief ?? "").Trim()
                + (it ? $"\n\nGiro {roundId}, passo «{stepLabel}»" : $"\n\nRound {roundId}, step «{stepLabel}»");
            if (round > 1) text += it ? $", giro {round}" : $", round {round}";
            if (attempt > 1) text += it ? $", tentativo {attempt}" : $", attempt {attempt}";
            text += ".";
            if (inputs != null && inputs.Count > 0)
                text += (it ? "\nFile da cui parti (approvati):" : "\nFiles you start from (approved):")
                        + string.Concat(inputs.Select(f => "\n- " + f));
            return text;
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
