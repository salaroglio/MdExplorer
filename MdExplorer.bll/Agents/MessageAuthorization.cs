using System;
using System.Collections.Generic;
using System.Linq;

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
