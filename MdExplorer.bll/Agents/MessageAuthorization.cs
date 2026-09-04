using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Yaml;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Il filtro fine del destinatario (§6, R-send): un cittadino dichiara in
    /// <c>accepts_messages_from</c> chi può scrivergli. Enforce al momento del <b>send
    /// autenticato</b> (dove il mittente è certificato dal RunToken, non spoofabile).
    /// <para>
    /// Regola — <b>default-deny</b> coerente col principio "la cittadinanza è read-only,
    /// la messaggistica è un privilegio esplicito" (§10):
    /// <list type="bullet">
    /// <item>l'umano (<c>user</c>) è sempre ammesso: è la fonte fidata per definizione;</item>
    /// <item><c>"*"</c> nella lista → chiunque nel progetto;</item>
    /// <item>altrimenti il nome del mittente deve comparire (case-insensitive);</item>
    /// <item>lista vuota/assente → nessuno (il destinatario non ha aperto la porta).</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class MessageAuthorization
    {
        /// <summary>Mittente attribuito ai chiamanti del gateway che non si dichiarano.</summary>
        public const string ExternalSender = "external";

        /// <summary>
        /// Normalizza il mittente <b>dichiarato</b> dal gateway non-autenticato (§7):
        /// assente → <see cref="ExternalSender"/>; presente → deve essere un identificatore
        /// kebab-case non riservato (stesse regole dei nomi cittadino). In particolare
        /// <c>user</c> è rifiutato: l'esenzione hop e la riapertura delle conversazioni
        /// esaurite spettano solo a canali dove il mittente è certificato, mai a una
        /// stringa dichiarata. Restituisce null e valorizza <paramref name="error"/> se
        /// il nome è inaccettabile (fail-loud).
        /// </summary>
        public static string ResolveDeclaredSender(string declaredFromAgent, out string error)
        {
            error = null;
            var declared = declaredFromAgent?.Trim();
            if (string.IsNullOrEmpty(declared))
                return ExternalSender;

            var nameError = YamlAgentCardParser.ValidateAgentName(declared);
            if (nameError != null)
            {
                error = $"fromAgent dichiarato non valido. {nameError}";
                return null;
            }
            return declared;
        }

        public static bool IsSenderAccepted(IEnumerable<string> acceptsMessagesFrom, string senderName)
        {
            var sender = senderName?.Trim();
            if (string.IsNullOrEmpty(sender))
                return false;

            // L'umano è sempre ammesso (coerente con l'esenzione hop di ConversationHopGuard).
            if (string.Equals(sender, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
                return true;

            if (acceptsMessagesFrom == null)
                return false;

            var list = acceptsMessagesFrom
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();

            if (list.Any(s => s == "*"))
                return true;

            return list.Any(s => string.Equals(s, sender, StringComparison.OrdinalIgnoreCase));
        }
    }
}
