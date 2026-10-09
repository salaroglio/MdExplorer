using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Agents
{
    /// <summary>Un possibile destinatario dell'avviso che parte da «Approva».</summary>
    public sealed class ApprovalRecipient
    {
        public string Name { get; init; }
        public string Role { get; init; }

        /// <summary>True se l'avviso gli può arrivare davvero (cittadino del progetto e fidato; la persona è sempre ammessa dalla whitelist).</summary>
        public bool Available { get; init; }

        /// <summary>Perché non è raggiungibile; null se lo è.</summary>
        public string Reason { get; init; }
    }

    /// <summary>
    /// La tabella delle corrispondenze di «Approva»: per un agente che consegna lavoro, a chi può passarlo la
    /// persona (<c>a2a.on_approval_notify</c>) e se ciascuno è raggiungibile adesso.
    /// <para>
    /// Pura, senza I/O: decide solo sul catalogo che le si dà. La regola che ne discende sta nel controller:
    /// <b>nessun destinatario</b> = si approva e basta (comportamento di sempre); <b>uno</b> = l'avviso va a lui;
    /// <b>più di uno</b> = la persona sceglie, e senza scelta non si approva.
    /// </para>
    /// </summary>
    public static class ApprovalRoute
    {
        /// <summary>Il mittente dell'avviso: la persona che ha premuto «Approva», non l'agente.</summary>
        public const string Sender = ConversationHopGuard.UserRecipient;

        /// <param name="worksElsewhere">
        /// True for an agent that answers to another person: it is not enabled on this computer and does not
        /// need to be — the notice travels to that person's computer. Null = every agent works here.
        /// </param>
        public static IReadOnlyList<ApprovalRecipient> Candidates(IEnumerable<AgentRegistryEntry> catalog, string producerName,
            Func<AgentRegistryEntry, bool> worksElsewhere = null)
        {
            var citizens = (catalog ?? Enumerable.Empty<AgentRegistryEntry>())
                .Where(e => e.IsCitizen && string.IsNullOrEmpty(e.RegistrationError))
                .ToList();

            var producer = citizens.FirstOrDefault(e => Same(e.Name, producerName));
            if (producer?.OnApprovalNotify == null || producer.OnApprovalNotify.Count == 0)
                return Array.Empty<ApprovalRecipient>();

            return producer.OnApprovalNotify
                .Select(name =>
                {
                    var target = citizens.FirstOrDefault(e => Same(e.Name, name));
                    string reason = null;
                    if (target == null)
                        reason = $"'{name}' non esiste fra gli agenti del progetto.";
                    else if (!target.Trusted && worksElsewhere?.Invoke(target) != true)
                        reason = $"'{name}' non è fidato: concedi la fiducia nel registro.";
                    return new ApprovalRecipient
                    {
                        Name = target?.Name ?? name,
                        Role = target?.Role,
                        Available = reason == null,
                        Reason = reason,
                    };
                })
                .ToList();
        }

        /// <summary>Il testo dell'avviso: cosa è stato approvato e che è già sul ramo principale.</summary>
        public static string ComposeMessage(string producer, IEnumerable<string> files)
        {
            var list = (files ?? Enumerable.Empty<string>()).Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
            var sb = new StringBuilder();
            sb.Append("[APPROVATO] La persona ha approvato il lavoro di '").Append(producer)
              .Append("': è già nel ramo principale del progetto.");
            if (list.Count > 0)
            {
                sb.Append("\nFile consegnati:");
                foreach (var f in list) sb.Append("\n- ").Append(f);
            }
            sb.Append("\nÈ il tuo turno: procedi secondo le tue istruzioni.");
            return sb.ToString();
        }

        private static bool Same(string a, string b)
            => string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
