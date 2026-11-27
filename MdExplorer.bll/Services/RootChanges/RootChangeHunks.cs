using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MdExplorer.Features.Services.RootChanges
{
    /// <summary>
    /// Un pezzo della revisione: righe consecutive diverse fra l'originale e il file di adesso. Le righe tengono il loro «a capo»
    /// (<c>\n</c> o <c>\r\n</c>), così Annulla riscrive esattamente quello che c'era.
    /// </summary>
    public sealed class ChangeHunk
    {
        public int Index { get; init; }
        /// <summary>Dove comincia nell'originale (riga da 0) e le righe che c'erano.</summary>
        public int OriginalStart { get; init; }
        public IReadOnlyList<string> OriginalLines { get; init; }
        /// <summary>Dove comincia nel file di adesso (riga da 0) e le righe che ci sono.</summary>
        public int CurrentStart { get; init; }
        public IReadOnlyList<string> CurrentLines { get; init; }
        /// <summary>
        /// L'impronta del pezzo (posizioni e righe): chi chiede Tieni/Annulla la rimanda, e se il file è cambiato nel frattempo
        /// il pezzo non combacia più e l'operazione si rifiuta invece di toccare righe diverse.
        /// </summary>
        public string Hash { get; init; }
    }

    /// <summary>
    /// I pezzi fra l'originale e il file di adesso (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici, D7), e le due
    /// operazioni su un pezzo: Tieni (l'originale prende il pezzo nuovo) e Annulla (il file riprende il pezzo originale).
    /// <para>
    /// Non è <c>SkillLearning.LineDiff</c>: quello tiene una tabella righe×righe (un sorgente di 5.000 righe = 100 MB) e perde
    /// gli «a capo». Qui prima si tolgono le righe uguali in testa e in coda — le modifiche di MarkAgent sono locali — e la
    /// sottosequenza comune più lunga si calcola solo sul mezzo. Se il mezzo resta più grande di <see cref="MaxCells"/> celle
    /// (un file riscritto quasi tutto) il mezzo è UN pezzo: esatto, solo più grosso.
    /// </para>
    /// </summary>
    public static class RootChangeHunks
    {
        public const long MaxCells = 4_000_000;

        /// <summary>Le righe del testo, ognuna col suo «a capo»; l'ultima senza, se il testo non finisce con un a capo.</summary>
        public static List<string> Lines(string text)
        {
            var lines = new List<string>();
            if (string.IsNullOrEmpty(text)) return lines;
            var start = 0;
            for (var i = 0; i < text.Length; i++)
                if (text[i] == '\n') { lines.Add(text.Substring(start, i - start + 1)); start = i + 1; }
            if (start < text.Length) lines.Add(text.Substring(start));
            return lines;
        }

        public static IReadOnlyList<ChangeHunk> Compute(string original, string current)
        {
            var a = Lines(original);
            var b = Lines(current);
            var prefix = 0;
            while (prefix < a.Count && prefix < b.Count && a[prefix] == b[prefix]) prefix++;
            var suffix = 0;
            while (suffix < a.Count - prefix && suffix < b.Count - prefix && a[a.Count - 1 - suffix] == b[b.Count - 1 - suffix]) suffix++;
            var midA = a.GetRange(prefix, a.Count - prefix - suffix);
            var midB = b.GetRange(prefix, b.Count - prefix - suffix);
            var hunks = new List<ChangeHunk>();
            if (midA.Count == 0 && midB.Count == 0) return hunks;

            if ((long)midA.Count * midB.Count > MaxCells)
            {
                hunks.Add(Make(0, prefix, midA, prefix, midB));
                return hunks;
            }

            var lcs = new int[midA.Count + 1, midB.Count + 1];
            for (var i = midA.Count - 1; i >= 0; i--)
                for (var j = midB.Count - 1; j >= 0; j--)
                    lcs[i, j] = midA[i] == midB[j] ? lcs[i + 1, j + 1] + 1 : Math.Max(lcs[i + 1, j], lcs[i, j + 1]);

            int x = 0, y = 0;
            var removed = new List<string>();
            var added = new List<string>();
            int hunkA = -1, hunkB = -1;
            void Close()
            {
                if (removed.Count == 0 && added.Count == 0) return;
                hunks.Add(Make(hunks.Count, prefix + hunkA, removed.ToList(), prefix + hunkB, added.ToList()));
                removed.Clear(); added.Clear(); hunkA = -1; hunkB = -1;
            }
            void Open() { if (hunkA < 0) { hunkA = x; hunkB = y; } }
            while (x < midA.Count || y < midB.Count)
            {
                if (x < midA.Count && y < midB.Count && midA[x] == midB[y]) { Close(); x++; y++; }
                else if (y >= midB.Count || (x < midA.Count && lcs[x + 1, y] >= lcs[x, y + 1])) { Open(); removed.Add(midA[x]); x++; }
                else { Open(); added.Add(midB[y]); y++; }
            }
            Close();
            return hunks;
        }

        private static ChangeHunk Make(int index, int originalStart, List<string> originalLines, int currentStart, List<string> currentLines)
        {
            var text = originalStart + "\0" + string.Concat(originalLines) + "\0" + currentStart + "\0" + string.Concat(currentLines);
            return new ChangeHunk
            {
                Index = index, OriginalStart = originalStart, OriginalLines = originalLines, CurrentStart = currentStart,
                CurrentLines = currentLines,
                Hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16].ToLowerInvariant(),
            };
        }

        /// <summary>Tieni: l'originale prende le righe nuove del pezzo, che così sparisce dalla revisione.</summary>
        public static string Keep(string original, ChangeHunk hunk)
        {
            var lines = Lines(original);
            lines.RemoveRange(hunk.OriginalStart, hunk.OriginalLines.Count);
            lines.InsertRange(hunk.OriginalStart, hunk.CurrentLines);
            return string.Concat(lines);
        }

        /// <summary>Annulla: il file riprende le righe originali del pezzo.</summary>
        public static string Undo(string current, ChangeHunk hunk)
        {
            var lines = Lines(current);
            lines.RemoveRange(hunk.CurrentStart, hunk.CurrentLines.Count);
            lines.InsertRange(hunk.CurrentStart, hunk.OriginalLines);
            return string.Concat(lines);
        }
    }
}
