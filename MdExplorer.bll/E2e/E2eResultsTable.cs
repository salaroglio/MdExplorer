using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.E2e
{
    public sealed record E2eResultRow(string Date, int Test, string Outcome, string Details);

    /// <summary>
    /// The <c>## Esiti</c> table of a <c>.e2e.md</c> (D8, skill mde-e2e): MdExplorer adds rows itself when it
    /// replays the scripts (F5), the most recent on top, the old ones never removed. Everything else in the
    /// file stays as it is.
    /// </summary>
    public static class E2eResultsTable
    {
        public const string Header = "| Data | Test | Esito | Dettagli |";
        public const string Separator = "|---|---|---|---|";

        public static string AddRows(string markdown, IReadOnlyList<E2eResultRow> rows)
        {
            if (rows.Count == 0) return markdown;
            var newline = markdown.Contains("\r\n") ? "\r\n" : "\n";
            var lines = markdown.Replace("\r\n", "\n").Split('\n').ToList();
            var rendered = rows.Select(r => $"| {r.Date} | T{r.Test} | {Cell(r.Outcome)} | {Cell(r.Details)} |").ToList();

            var section = lines.FindIndex(l => l.TrimEnd() == "## Esiti");
            if (section < 0)
            {
                if (lines.Count > 0 && lines[^1].Length == 0) lines.RemoveAt(lines.Count - 1);
                lines.AddRange(new[] { "", "## Esiti", "", Header, Separator });
                lines.AddRange(rendered);
                lines.Add("");
                return string.Join(newline, lines);
            }

            var end = lines.FindIndex(section + 1, l => l.StartsWith("#", StringComparison.Ordinal));
            if (end < 0) end = lines.Count;
            var header = lines.FindIndex(section + 1, end - section - 1, l => l.TrimStart().StartsWith("|", StringComparison.Ordinal) && l.Contains("Esito"));
            if (header >= 0 && header + 1 < end && lines[header + 1].TrimStart().StartsWith("|", StringComparison.Ordinal))
            {
                lines.InsertRange(header + 2, rendered);
            }
            else
            {
                var table = new List<string> { "", Header, Separator };
                table.AddRange(rendered);
                lines.InsertRange(section + 1, table);
            }
            return string.Join(newline, lines);
        }

        private static string Cell(string text) => (text ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ").Trim();
    }
}
