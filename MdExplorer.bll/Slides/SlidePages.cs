using System.Collections.Generic;
using System.Globalization;

namespace MdExplorer.Features.Slides
{
    /// <summary>
    /// The pages of a deck a link asks for: <c>[Costi](vendite.md?pages=2,6-9)</c>. A page is one
    /// horizontal slide, numbered from 1 as written in the file; a vertical stack (<c>--</c>) counts
    /// as one page. Written as <c>N</c>, <c>N-M</c> or <c>N-</c> (from N to the end), separated by
    /// commas. The pages are shown in the deck's order, whatever the order they are listed in.
    /// A page the deck does not have is an error to show to the author, never an empty deck.
    /// </summary>
    public sealed class SlidePages
    {
        private readonly List<(int From, int? To)> _ranges;

        private SlidePages(List<(int From, int? To)> ranges)
        {
            _ranges = ranges;
        }

        /// <summary>Null when nothing is asked for: the whole deck.</summary>
        public static SlidePages Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var ranges = new List<(int From, int? To)>();
            foreach (var raw in text.Split(','))
            {
                var item = raw.Trim();
                var dash = item.IndexOf('-');
                if (dash < 0)
                {
                    var page = Number(item, text);
                    ranges.Add((page, page));
                }
                else
                {
                    var from = Number(item.Substring(0, dash).Trim(), text);
                    var toText = item.Substring(dash + 1).Trim();
                    var to = toText.Length == 0 ? (int?)null : Number(toText, text);
                    if (to < from)
                    {
                        throw new SlideDeckException($"The pages \"{text}\" are not valid: \"{item}\" goes backwards. Write them as 2,6-9 (from 2 and from 6 to 9) or 2- (from 2 to the end).");
                    }
                    ranges.Add((from, to));
                }
            }
            return new SlidePages(ranges);
        }

        private static int Number(string item, string whole)
        {
            if (!int.TryParse(item, NumberStyles.None, CultureInfo.InvariantCulture, out var number) || number < 1)
            {
                throw new SlideDeckException($"The pages \"{whole}\" are not valid: \"{item}\" is not a page number. Pages are numbered from 1; write them as 2,6-9 (from 2 and from 6 to 9) or 2- (from 2 to the end).");
            }
            return number;
        }

        /// <summary>Whether the page (1-based) is one asked for.</summary>
        public bool Includes(int page)
        {
            foreach (var (from, to) in _ranges)
            {
                if (page >= from && (to == null || page <= to))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>The deck has <paramref name="count"/> pages: one asked for beyond them is an error.</summary>
        public void CheckAgainst(int count)
        {
            foreach (var (from, to) in _ranges)
            {
                var last = to ?? from;
                if (from > count || last > count)
                {
                    throw new SlideDeckException($"The deck has {count} page{(count == 1 ? "" : "s")}, and the link asks for page {(from > count ? from : last)}. Change the pages of the link.");
                }
            }
        }
    }
}
