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
            "- Una riga per persona: il suo ruolo, la sua email git e i suoi agenti. Più persone con lo stesso ruolo sono un team.\n" +
            "- La persona si riconosce dall'email git con cui lavora nel progetto.\n" +
            "- MdExplorer aggiunge una riga quando qualcuno dichiara suo un agente; il resto lo scrive il gruppo.\n\n" +
            "| Ruolo | Responsabile | Git Email | Agenti |\n" +
            "|--------|--------------|-----------|--------|\n";

        /// <summary>The same document for an English project (M6 of the composable methods): the parser reads both.</summary>
        public const string NewDocumentEnglish =
            "---\n" +
            "mde_type: ownership\n" +
            "title: Who answers for which agent\n" +
            "---\n\n" +
            "# Who answers for which agent\n\n" +
            "## TL;DR\n\n" +
            "Every agent of the city answers to a person: it works only on their computer, and that person judges what the\n" +
            "agent writes. This table says who. An agent that is not here does not start.\n\n" +
            "- One row per person: their role, their git email and their agents. Several people with the same role are a team.\n" +
            "- The person is recognised by the git email they work with in the project.\n" +
            "- MdExplorer adds a row when someone declares an agent theirs; the rest is written by the group.\n\n" +
            "| Role | Owner | Git Email | Agents |\n" +
            "|------|-------|-----------|--------|\n";

        /// <summary>The document a project starts with, in its language: Italian for <c>it</c>, English otherwise.</summary>
        public static string NewDocumentFor(string language) => language == "it" ? NewDocument : NewDocumentEnglish;

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

                if (columns.Contains("retired-scope"))
                    throw new InvalidOperationException(
                        MdExplorer.Features.Services.UiText.Say("La colonna «Ambito» del documento delle responsabilità ora si chiama «Ruolo»: rinominala e riprova.", "The «Ambito» column of the responsibilities document is now called «Ruolo»: rename it and try again."));
                foreach (var needed in new[] { ("role", "Ruolo"), ("gitEmail", "Git Email"), ("agents", "Agenti") })
                    if (!columns.Contains(needed.Item1))
                        throw new InvalidOperationException(
                            MdExplorer.Features.Services.UiText.Say($"La tabella del documento delle responsabilità non ha la colonna '{needed.Item2}': aggiungila e riprova.", $"The table of the responsibilities document has no column '{needed.Item2}': add it and try again."));

                var last = i + 1;
                while (last + 1 < lines.Count && OwnershipDocParser.LooksLikeTableRow(lines[last + 1])) last++;

                // The role: the one of a row that already cites the agent (an open role the group prepared, or a colleague
                // who took it before: then the rows of that role are its team); otherwise the agent's own name, which the
                // group renames as it likes. An open row that cites only this agent is filled in place.
                string Column(string[] cells, string canonical)
                {
                    var at = Array.IndexOf(columns, canonical);
                    return at >= 0 && at < cells.Length ? cells[at].Trim() : string.Empty;
                }
                bool Cites(string[] cells) => OwnershipDocParser.SplitAgents(Column(cells, "agents"))
                    .Any(a => string.Equals(a, agentName.Trim(), StringComparison.OrdinalIgnoreCase));
                bool IsOpen(string[] cells) => Column(cells, "gitEmail").Length == 0 && Column(cells, "responsible").Length == 0;

                string role = null;
                for (var r = i + 2; r <= last; r++)
                {
                    var cells = OwnershipDocParser.SplitRow(lines[r]);
                    if (!Cites(cells)) continue;
                    role ??= Column(cells, "role");
                    if (!IsOpen(cells) || OwnershipDocParser.SplitAgents(Column(cells, "agents")).Count != 1) continue;
                    lines[r] = "| " + string.Join(" | ", columns.Select((c, k) => c switch
                    {
                        "responsible" => Clean(responsible),
                        "gitEmail" => gitEmail.Trim().ToLowerInvariant(),
                        _ => k < cells.Length ? cells[k].Trim() : string.Empty,
                    })) + " |";
                    return string.Join(newline, lines);
                }
                if (string.IsNullOrWhiteSpace(role)) role = agentName.Trim();

                var row = columns.Select(c => c switch
                {
                    "role" => role,
                    "responsible" => Clean(responsible),
                    "gitEmail" => gitEmail.Trim().ToLowerInvariant(),
                    "agents" => agentName.Trim(),
                    _ => string.Empty,
                });
                lines.Insert(last + 1, "| " + string.Join(" | ", row) + " |");
                return string.Join(newline, lines);
            }

            throw new InvalidOperationException(
                MdExplorer.Features.Services.UiText.Say("Il documento delle responsabilità non ha una tabella con le colonne Ruolo | Responsabile | Git Email | Agenti: aggiungila e riprova.", "The responsibilities document has no table with the columns Ruolo | Responsabile | Git Email | Agenti: add it and try again."));
        }

        /// <summary>
        /// The document with an <b>open role</b> (a row with the role and its agents, nobody yet) for each role it does not
        /// have, by name: what a template method declares (P2 of the templates' plan). A role the document already has is
        /// left as it is — the group may have filled it, or changed its agents. <paramref name="added"/> says which roles
        /// were added. The description goes in the «Descrizione» column, when the table has one. Same table rules as
        /// <see cref="AddRow"/>.
        /// </summary>
        /// <param name="language">The project's language, for a document that does not exist yet.</param>
        public static string AddOpenRoles(string markdown, IEnumerable<(string Role, string Description, IReadOnlyList<string> Agents)> roles, out IReadOnlyList<string> added,
            string language = "it")
        {
            var result = new List<string>();
            added = result;
            var wanted = (roles ?? Enumerable.Empty<(string, string, IReadOnlyList<string>)>()).Where(r => !string.IsNullOrWhiteSpace(r.Role)).ToList();
            if (string.IsNullOrWhiteSpace(markdown)) markdown = NewDocumentFor(language);
            if (wanted.Count == 0) return markdown;

            var newline = markdown.Contains("\r\n") ? "\r\n" : "\n";
            var lines = markdown.Replace("\r\n", "\n").Split('\n').ToList();
            for (var i = 0; i < lines.Count - 1; i++)
            {
                if (!OwnershipDocParser.LooksLikeTableRow(lines[i]) || !OwnershipDocParser.IsSeparatorRow(lines[i + 1])) continue;
                var columns = OwnershipDocParser.SplitRow(lines[i])
                    .Select(h => OwnershipDocParser.HeaderAliases.TryGetValue(h.Trim(), out var canonical) ? canonical : null)
                    .ToArray();
                if (columns.All(c => c == null)) continue;
                if (columns.Contains("retired-scope"))
                    throw new InvalidOperationException(
                        MdExplorer.Features.Services.UiText.Say("La colonna «Ambito» del documento delle responsabilità ora si chiama «Ruolo»: rinominala e riprova.", "The «Ambito» column of the responsibilities document is now called «Ruolo»: rename it and try again."));
                foreach (var needed in new[] { ("role", "Ruolo"), ("agents", "Agenti") })
                    if (!columns.Contains(needed.Item1))
                        throw new InvalidOperationException(
                            MdExplorer.Features.Services.UiText.Say($"La tabella del documento delle responsabilità non ha la colonna '{needed.Item2}': aggiungila e riprova.", $"The table of the responsibilities document has no column '{needed.Item2}': add it and try again."));

                var last = i + 1;
                while (last + 1 < lines.Count && OwnershipDocParser.LooksLikeTableRow(lines[last + 1])) last++;
                var roleAt = Array.IndexOf(columns, "role");
                var present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (var r = i + 2; r <= last; r++)
                {
                    var cells = OwnershipDocParser.SplitRow(lines[r]);
                    if (roleAt < cells.Length) present.Add(cells[roleAt].Trim());
                }
                foreach (var (role, description, agents) in wanted)
                {
                    if (!present.Add(Clean(role))) continue;
                    var row = columns.Select(c => c switch
                    {
                        "role" => Clean(role),
                        "description" => Clean(description),
                        "agents" => string.Join(", ", (agents ?? Array.Empty<string>()).Select(Clean)),
                        _ => string.Empty,
                    });
                    lines.Insert(++last, "| " + string.Join(" | ", row) + " |");
                    result.Add(Clean(role));
                }
                return string.Join(newline, lines);
            }
            throw new InvalidOperationException(
                MdExplorer.Features.Services.UiText.Say("Il documento delle responsabilità non ha una tabella con le colonne Ruolo | Responsabile | Git Email | Agenti: aggiungila e riprova.", "The responsibilities document has no table with the columns Ruolo | Responsabile | Git Email | Agenti: add it and try again."));
        }

        /// <summary>A cell is one line without the column separator.</summary>
        private static string Clean(string cell)
            => (cell ?? string.Empty).Replace("|", " ").Replace("\r", " ").Replace("\n", " ").Trim();
    }
}
