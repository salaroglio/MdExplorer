using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Writes into the responsibilities' document the one thing the application writes there: «this agent
    /// answers to this person», as a new row of its table. Everything else in the document is the team's
    /// and is left as it is — the text, the other rows, the order and the names of the columns.
    /// </summary>
    public static class OwnershipDocWriter
    {
        /// <summary>The document a project starts with when it has none.</summary>
        public const string NewDocument =
            "---\n" +
            "mde_type: ownership\n" +
            "title: Chi risponde di quale agente\n" +
            "---\n\n" +
            "# Chi risponde di quale agente\n\n" +
            "## TL;DR\n\n" +
            "Ogni agente della città risponde a una persona: lavora solo sul suo computer, e quella persona valuta ciò che\n" +
            "l'agente scrive. Questa tabella dice chi. Un agente che non compare qui non parte.\n\n" +
            "- Una riga per ambito: chi ne risponde, la sua email git e i suoi agenti.\n" +
            "- La persona si riconosce dall'email git con cui lavora nel progetto.\n" +
            "- MdExplorer aggiunge una riga quando qualcuno dichiara suo un agente; il resto lo scrive il gruppo.\n\n" +
            "| Ambito | Responsabile | Git Email | Agenti |\n" +
            "|--------|--------------|-----------|--------|\n";

        /// <summary>
        /// The document with one more row giving <paramref name="agentName"/> to that person.
        /// <paramref name="markdown"/> null or blank = no document yet: <see cref="NewDocument"/> plus the row.
        /// Throws <see cref="InvalidOperationException"/>, with what to fix, when the document has no table
        /// this can add a row to.
        /// </summary>
        public static string AddRow(string markdown, string agentName, string responsible, string gitEmail)
        {
            if (string.IsNullOrWhiteSpace(agentName)) throw new ArgumentException("agentName is required", nameof(agentName));
            if (string.IsNullOrWhiteSpace(gitEmail)) throw new ArgumentException("gitEmail is required", nameof(gitEmail));
            if (string.IsNullOrWhiteSpace(markdown)) markdown = NewDocument;

            var newline = markdown.Contains("\r\n") ? "\r\n" : "\n";
            var lines = markdown.Replace("\r\n", "\n").Split('\n').ToList();

            for (var i = 0; i < lines.Count - 1; i++)
            {
                if (!OwnershipDocParser.LooksLikeTableRow(lines[i]) || !OwnershipDocParser.IsSeparatorRow(lines[i + 1])) continue;

                var header = OwnershipDocParser.SplitRow(lines[i]);
                var columns = header
                    .Select(h => OwnershipDocParser.HeaderAliases.TryGetValue(h.Trim(), out var canonical) ? canonical : null)
                    .ToArray();
                if (columns.All(c => c == null)) continue;   // not the responsibilities' table

                foreach (var needed in new[] { ("scope", "Ambito"), ("gitEmail", "Git Email"), ("agents", "Agenti") })
                    if (!columns.Contains(needed.Item1))
                        throw new InvalidOperationException(
                            $"La tabella del documento delle responsabilità non ha la colonna '{needed.Item2}': aggiungila e riprova.");

                var last = i + 1;
                var scopeColumn = Array.IndexOf(columns, "scope");
                var scopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (last + 1 < lines.Count && OwnershipDocParser.LooksLikeTableRow(lines[last + 1]))
                {
                    last++;
                    var cells = OwnershipDocParser.SplitRow(lines[last]);
                    if (scopeColumn < cells.Length) scopes.Add(cells[scopeColumn].Trim());
                }

                // An ambit is unique in the table: the agent's own name, numbered if someone already used it.
                var scope = agentName.Trim();
                for (var n = 2; scopes.Contains(scope); n++) scope = $"{agentName.Trim()} ({n})";

                var row = columns.Select(c => c switch
                {
                    "scope" => scope,
                    "responsible" => Clean(responsible),
                    "gitEmail" => gitEmail.Trim().ToLowerInvariant(),
                    "agents" => agentName.Trim(),
                    _ => string.Empty,
                });
                lines.Insert(last + 1, "| " + string.Join(" | ", row) + " |");
                return string.Join(newline, lines);
            }

            throw new InvalidOperationException(
                "Il documento delle responsabilità non ha una tabella con le colonne Ambito | Responsabile | Git Email | Agenti: aggiungila e riprova.");
        }

        /// <summary>A cell is one line without the column separator.</summary>
        private static string Clean(string cell)
            => (cell ?? string.Empty).Replace("|", " ").Replace("\r", " ").Replace("\n", " ").Trim();
    }
}
