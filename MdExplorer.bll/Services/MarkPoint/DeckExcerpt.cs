using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.MarkPoint
{
    /// <summary>
    /// The part of a deck MarkAgent reads when the deck is longer than its budget: the slide of the point
    /// whole, speaker notes included, then the nearest slides while they fit. Cutting the file at the budget
    /// lost the notes of the slides past it, and the notes are where a deck keeps its answers.
    /// </summary>
    public static class DeckExcerpt
    {
        private static readonly Regex Fence = new(@"^\s*(```|~~~)");
        private static readonly Regex Heading = new(@"^\s*#{1,6}\s+(.*?)\s*#*\s*$");

        /// <summary>
        /// <paramref name="deck"/> whole when it fits in <paramref name="maxChars"/>; otherwise the slide whose
        /// heading is <paramref name="slideTitle"/> and its neighbours, in file order. Without that slide (no title,
        /// or not found) the beginning of the deck, as before.
        /// </summary>
        public static string Take(string deck, string? slideTitle, int maxChars, out bool truncated)
        {
            truncated = deck.Length > maxChars;
            if (!truncated) return deck;

            var slides = Split(deck);
            var at = FindSlide(slides, slideTitle);
            if (at < 0) return deck.Substring(0, maxChars);

            // The point's slide always whole, even past the budget: it is what the user asked about.
            int first = at, last = at, used = slides[at].Length;
            while (true)
            {
                var grew = false;
                foreach (var i in new[] { last + 1, first - 1 })
                {
                    if (i < 0 || i >= slides.Count || used + slides[i].Length > maxChars) continue;
                    used += slides[i].Length;
                    if (i > last) last = i; else first = i;
                    grew = true;
                }
                if (!grew) break;
            }
            return string.Join("", slides.Skip(first).Take(last - first + 1));
        }

        /// <summary>The deck cut at its separators (<c>---</c> or <c>--</c> alone on a line, outside code); the front matter is the first piece.</summary>
        internal static List<string> Split(string deck)
        {
            var lines = deck.Replace("\r\n", "\n").Split('\n');
            var slides = new List<string>();
            var current = new List<string>();
            var inCode = false;
            var inFrontMatter = lines.Length > 0 && lines[0].Trim() == "---";
            for (var n = 0; n < lines.Length; n++)
            {
                var line = lines[n];
                if (inFrontMatter)
                {
                    current.Add(line);
                    if (n > 0 && line.Trim() == "---") { inFrontMatter = false; slides.Add(string.Join("\n", current) + "\n"); current.Clear(); }
                    continue;
                }
                if (Fence.IsMatch(line)) inCode = !inCode;
                if (!inCode && (line.Trim() == "---" || line.Trim() == "--"))
                {
                    current.Add(line);
                    slides.Add(string.Join("\n", current) + "\n");
                    current.Clear();
                    continue;
                }
                current.Add(line);
            }
            if (current.Count > 0) slides.Add(string.Join("\n", current));
            return slides;
        }

        private static int FindSlide(IReadOnlyList<string> slides, string? title)
        {
            if (string.IsNullOrWhiteSpace(title)) return -1;
            var wanted = Normalize(title);
            for (var i = 0; i < slides.Count; i++)
            {
                var inCode = false;
                foreach (var line in slides[i].Split('\n'))
                {
                    if (Fence.IsMatch(line)) { inCode = !inCode; continue; }
                    if (inCode) continue;
                    var m = Heading.Match(line);
                    if (m.Success && Normalize(m.Groups[1].Value) == wanted) return i;
                }
            }
            return -1;
        }

        /// <summary>The heading as the page shows it: no markdown emphasis or code marks, spaces collapsed.</summary>
        private static string Normalize(string s) =>
            Regex.Replace(Regex.Replace(s, @"[*_`]", ""), @"\s+", " ").Trim().ToLowerInvariant();
    }
}
