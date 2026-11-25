using System;
using System.Collections.Generic;

namespace MdExplorer.Features.Services.AI.SkillLearning
{
    /// <summary>Una riga del confronto: uguale, tolta o aggiunta.</summary>
    public sealed class DiffLine
    {
        public const string Same = "same";
        public const string Removed = "del";
        public const string Added = "add";

        public string Kind { get; init; }
        public string Text { get; init; }
    }

    /// <summary>
    /// Il confronto riga per riga fra il SKILL.md che c'è e quello proposto, per la finestra di «Impara» (sprint skill che
    /// migliorano, F3). Sottosequenza comune più lunga: le skill stanno sotto le 500 righe, quindi il costo quadratico va
    /// bene ed è esatto (niente euristiche che spostano le righe).
    /// </summary>
    public static class LineDiff
    {
        public static IReadOnlyList<DiffLine> Compute(string before, string after)
        {
            var a = Split(before);
            var b = Split(after);
            var lcs = new int[a.Length + 1, b.Length + 1];
            for (var i = a.Length - 1; i >= 0; i--)
                for (var j = b.Length - 1; j >= 0; j--)
                    lcs[i, j] = a[i] == b[j] ? lcs[i + 1, j + 1] + 1 : Math.Max(lcs[i + 1, j], lcs[i, j + 1]);

            var result = new List<DiffLine>();
            int x = 0, y = 0;
            while (x < a.Length && y < b.Length)
            {
                if (a[x] == b[y]) { result.Add(new DiffLine { Kind = DiffLine.Same, Text = a[x] }); x++; y++; }
                else if (lcs[x + 1, y] >= lcs[x, y + 1]) { result.Add(new DiffLine { Kind = DiffLine.Removed, Text = a[x] }); x++; }
                else { result.Add(new DiffLine { Kind = DiffLine.Added, Text = b[y] }); y++; }
            }
            for (; x < a.Length; x++) result.Add(new DiffLine { Kind = DiffLine.Removed, Text = a[x] });
            for (; y < b.Length; y++) result.Add(new DiffLine { Kind = DiffLine.Added, Text = b[y] });
            return result;
        }

        private static string[] Split(string text)
        {
            if (string.IsNullOrEmpty(text)) return Array.Empty<string>();
            var lines = text.Replace("\r\n", "\n").Split('\n');
            // L'a capo finale non è una riga in più.
            return lines.Length > 0 && lines[^1].Length == 0 ? lines[..^1] : lines;
        }
    }
}
