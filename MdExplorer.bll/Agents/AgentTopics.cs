using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Serializzazione degli argomenti (§8) del messaggio per la persistenza: una riga per
    /// argomento, ripuliti dai vuoti e dai duplicati (case-insensitive), ordine preservato.
    /// Metadata di contesto, nessun entity-linking.
    /// </summary>
    public static class AgentTopics
    {
        public static string Join(IEnumerable<string> topics)
        {
            var clean = Normalize(topics);
            return clean.Count == 0 ? null : string.Join("\n", clean);
        }

        public static List<string> Split(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return new List<string>();
            return Normalize(stored.Split('\n'));
        }

        private static List<string> Normalize(IEnumerable<string> topics)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var t in topics ?? Enumerable.Empty<string>())
            {
                var trimmed = t?.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                if (seen.Add(trimmed))
                    result.Add(trimmed);
            }
            return result;
        }
    }
}
