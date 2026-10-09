using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Agents
{
    /// <summary>Una risposta pronta per diventare un pulsante: i segnaposto sono già riempiti.</summary>
    public class ResolvedReply
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Message { get; set; }
        /// <summary>I valori dei segnaposto (<c>codice</c> → <c>NC-2027-014</c>): con un workflow diventano le variabili del giro.</summary>
        public Dictionary<string, string> Values { get; set; }
    }

    /// <summary>
    /// Le risposte che un agente propone alla persona, messe insieme da due metà: ciò che la <b>scheda dichiara</b>
    /// (<c>a2a.replies</c>: testi con segnaposto <c>{nome}</c>) e ciò che il <b>messaggio indica</b> (quali risposte
    /// valgono adesso, e con quali valori). Qui niente si indovina: una risposta non dichiarata, un valore
    /// mancante o un testo vuoto sono un errore che torna all'agente, così lo corregge prima che il messaggio parta.
    /// </summary>
    public static class AgentReplyResolver
    {
        private static readonly Regex Placeholder = new(@"\{([A-Za-z0-9_\-]+)\}", RegexOptions.Compiled);

        /// <param name="declared">Le risposte dichiarate nella scheda del mittente.</param>
        /// <param name="proposed">Per ogni risposta proposta: <c>id</c> più i valori dei segnaposto.</param>
        /// <param name="error">Perché non si possono risolvere; null se è andata.</param>
        public static IList<ResolvedReply> Resolve(
            IEnumerable<AgentRegistryReply> declared,
            IEnumerable<IDictionary<string, string>> proposed,
            out string error)
        {
            error = null;
            var result = new List<ResolvedReply>();
            var catalog = (declared ?? Enumerable.Empty<AgentRegistryReply>())
                .Where(d => !string.IsNullOrWhiteSpace(d?.Id))
                .GroupBy(d => d.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var item in proposed ?? Enumerable.Empty<IDictionary<string, string>>())
            {
                if (item == null) continue;
                var values = new Dictionary<string, string>(item, StringComparer.OrdinalIgnoreCase);
                if (!values.TryGetValue("id", out var id) || string.IsNullOrWhiteSpace(id))
                {
                    error = "Ogni risposta proposta deve avere un 'id'.";
                    return null;
                }
                if (!catalog.TryGetValue(id.Trim(), out var card))
                {
                    var known = catalog.Count == 0 ? "nessuna" : string.Join(", ", catalog.Keys);
                    error = $"La risposta '{id}' non è dichiarata nella tua scheda (a2a.replies). Dichiarate: {known}.";
                    return null;
                }

                var resolved = new ResolvedReply
                {
                    Id = card.Id.Trim(),
                    Values = values.Where(kv => !string.Equals(kv.Key, "id", StringComparison.OrdinalIgnoreCase))
                                   .ToDictionary(kv => kv.Key, kv => kv.Value?.Trim(), StringComparer.Ordinal),
                };
                foreach (var (name, template, required) in new[]
                         {
                             ("label", card.Label, true), ("description", card.Description, true), ("message", card.Message, true),
                         })
                {
                    if (string.IsNullOrWhiteSpace(template))
                    {
                        error = $"La risposta '{card.Id}' nella tua scheda non ha '{name}': va scritta, perché è ciò che la persona legge o invia.";
                        return null;
                    }
                    string missing = null;
                    var text = Placeholder.Replace(template, m =>
                    {
                        if (values.TryGetValue(m.Groups[1].Value, out var v) && !string.IsNullOrWhiteSpace(v)) return v.Trim();
                        missing ??= m.Groups[1].Value;
                        return m.Value;
                    });
                    if (missing != null)
                    {
                        error = $"Per la risposta '{card.Id}' manca il valore di '{missing}': indicalo insieme all'id.";
                        return null;
                    }
                    if (name == "label") resolved.Label = text.Trim();
                    else if (name == "description") resolved.Description = text.Trim();
                    else resolved.Message = text.Trim();
                }
                result.Add(resolved);
            }
            return result;
        }

        /// <summary>
        /// Un agente che dichiara risposte deve dire, a ogni messaggio alla persona, quali valgono adesso: anche
        /// «nessuna», con una lista vuota. Senza, «non c'è niente da scegliere» e «me ne sono dimenticato» sarebbero lo
        /// stesso messaggio, e la persona resterebbe senza pulsanti senza sapere perché.
        /// </summary>
        /// <returns>Il motivo del rifiuto, da restituire all'agente; null se l'invio può partire.</returns>
        public static string MissingProposal(IEnumerable<AgentRegistryReply> declared, bool repliesPassed)
        {
            if (repliesPassed) return null;
            var ids = (declared ?? Enumerable.Empty<AgentRegistryReply>())
                .Where(d => !string.IsNullOrWhiteSpace(d?.Id))
                .Select(d => d.Id.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (ids.Count == 0) return null;
            return $"La tua scheda dichiara le risposte {string.Join(", ", ids)} (a2a.replies): passa 'replies' con quelle " +
                   "che la persona può scegliere adesso, con i loro valori, oppure [] se non ha niente da scegliere. " +
                   "Il messaggio non è partito: rimandalo con 'replies'.";
        }

        /// <summary>
        /// Ciò che l'agente riceve quando la persona gli scrive con parole sue. Un agente si sveglia senza ricordi:
        /// senza il suo messaggio di prima, citato, una domanda come «perché non vedo i pulsanti?» non avrebbe contesto.
        /// </summary>
        public static string QuoteFreeReply(string personText, string previousBody, IEnumerable<ResolvedReply> previousReplies)
        {
            var quoted = string.Join("\n", (previousBody ?? string.Empty).Trim()
                .Replace("\r\n", "\n").Split('\n').Select(l => "> " + l));
            var offered = (previousReplies ?? Enumerable.Empty<ResolvedReply>()).ToList();
            var replies = offered.Count == 0
                ? "nessuna"
                : string.Join("; ", offered.Select(r => $"«{r.Label}» (messaggio: {r.Message})"));
            return "[RISPOSTA LIBERA] " + (personText ?? string.Empty).Trim() + "\n\n" +
                   "La persona ti ha scritto con parole sue, rispondendo a questo tuo messaggio:\n" +
                   quoted + "\n\n" +
                   "Risposte che le avevi proposto come pulsanti: " + replies + ".";
        }
    }
}
