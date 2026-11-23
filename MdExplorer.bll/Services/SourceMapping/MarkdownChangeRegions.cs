using System;
using System.Collections.Generic;
using System.Linq;
using Markdig;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace MdExplorer.Features.Services.SourceMapping
{
    /// <summary>
    /// Una parte cambiata di un documento markdown: i blocchi interi (o le voci di un elenco) che contengono una
    /// modifica, su tutti e due i lati. Le righe sono indici da 0 nei testi normalizzati di <see cref="MarkdownChanges"/>,
    /// estremi compresi; -1 quando da quel lato la parte non c'è.
    /// </summary>
    public sealed class MarkdownChangeRegion
    {
        /// <summary>La parte è fatta di voci dello stesso elenco: si trova dalle righe delle voci, non da marcatori.</summary>
        public bool ListItems { get; set; }
        public int OldStart { get; set; } = -1;
        public int OldEnd { get; set; } = -1;
        public int NewStart { get; set; } = -1;
        public int NewEnd { get; set; } = -1;
        /// <summary>Solo per le voci d'elenco: la riga d'inizio di ogni voce, nel vecchio e nel nuovo.</summary>
        public List<int> OldItemLines { get; set; } = new List<int>();
        public List<int> NewItemLines { get; set; } = new List<int>();
        /// <summary>
        /// Dove va, nel nuovo, una parte che lì non ha niente (tolta): prima del blocco o della voce che inizia a questa
        /// riga. Uguale al numero di righe del nuovo = in fondo al documento. -1 = non serve, o vale <see cref="NewAfterItem"/>.
        /// </summary>
        public int NewBefore { get; set; } = -1;
        /// <summary>Voci tolte in coda a un elenco: vanno dopo la voce che inizia a questa riga.</summary>
        public int NewAfterItem { get; set; } = -1;

        public bool HasOld => OldStart >= 0;
        public bool HasNew => NewStart >= 0;
        /// <summary>changed | added | removed.</summary>
        public string Kind => !HasOld ? "added" : !HasNew ? "removed" : "changed";
    }

    public sealed class MarkdownChanges
    {
        public List<MarkdownChangeRegion> Regions { get; set; } = new List<MarkdownChangeRegion>();
        /// <summary>
        /// Sono cambiate anche righe che nella pagina non hanno un blocco loro: intestazione YAML, definizioni di link,
        /// note a piè di pagina. Non stanno in nessun riquadro: la vista lo dice e rimanda alla vista testuale.
        /// </summary>
        public bool HiddenChanges { get; set; }
        /// <summary>I due testi come sono stati confrontati: una riga per elemento, senza fine riga.</summary>
        public string[] OldLines { get; set; }
        public string[] NewLines { get; set; }
    }

    /// <summary>
    /// Trova le parti cambiate fra due versioni di un documento markdown. È un algoritmo esatto: confronto per righe
    /// (Myers) e confini dei blocchi di Markdig. Una modifica si allarga al blocco di primo livello che la contiene;
    /// in un elenco di primo livello alla voce, quando le voci cambiate hanno accanto una voce rimasta uguale.
    /// </summary>
    public static class MarkdownChangeRegions
    {
        private sealed class Unit
        {
            public int Start;
            public int End;
            /// <summary>L'elenco di cui è una voce; null per un blocco intero.</summary>
            public Block List;
            public bool Touched;
            public int Peer = -1;
        }

        /// <exception cref="InvalidOperationException">Il documento è troppo grande o troppo diverso per il confronto: lo dice, non approssima.</exception>
        public static MarkdownChanges Compute(string oldText, string newText, MarkdownPipeline pipeline)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            var oldNorm = Normalize(oldText);
            var newNorm = Normalize(newText);
            var oldLines = oldNorm.Split('\n');
            var newLines = newNorm.Split('\n');
            if (oldLines.Length > MarkdownSourceMapService.MaxLines || newLines.Length > MarkdownSourceMapService.MaxLines)
                throw new InvalidOperationException(UiText.Say(
                    $"Il documento supera le {MarkdownSourceMapService.MaxLines} righe: le parti cambiate non si calcolano. Usa la vista testuale.",
                    $"The document is over {MarkdownSourceMapService.MaxLines} lines: the changed parts cannot be computed. Use the text view."));

            var (oldToNew, newToOld) = MatchLines(oldLines, newLines);
            var oldDoc = Markdown.Parse(oldNorm, pipeline);
            var newDoc = Markdown.Parse(newNorm, pipeline);
            var oldStarts = LineStarts(oldNorm);
            var newStarts = LineStarts(newNorm);

            var whole = new HashSet<Block>();   // elenchi trattati come blocco intero
            while (true)
            {
                var oldUnits = Units(oldDoc, oldStarts, whole);
                var newUnits = Units(newDoc, newStarts, whole);
                Pair(oldUnits, newUnits, oldToNew, newToOld);
                var regions = Walk(oldUnits, newUnits, newLines.Length, out var toCollapse);
                if (toCollapse.Count > 0)
                {
                    foreach (var list in toCollapse) whole.Add(list);
                    continue;
                }
                return new MarkdownChanges
                {
                    Regions = regions,
                    HiddenChanges = HasHiddenChanges(oldUnits, oldToNew, oldLines) || HasHiddenChanges(newUnits, newToOld, newLines),
                    OldLines = oldLines,
                    NewLines = newLines,
                };
            }
        }

        private static string Normalize(string text) => (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');

        private static int[] LineStarts(string text)
        {
            var starts = new List<int> { 0 };
            for (var i = 0; i < text.Length; i++) if (text[i] == '\n') starts.Add(i + 1);
            return starts.ToArray();
        }

        private static int LineOf(int[] starts, int offset)
        {
            var index = Array.BinarySearch(starts, offset);
            return index >= 0 ? index : ~index - 1;
        }

        /// <summary>Per ogni riga, la riga uguale dell'altro testo, o -1.</summary>
        private static (int[] oldToNew, int[] newToOld) MatchLines(string[] a, string[] b)
        {
            var oldToNew = Enumerable.Repeat(-1, a.Length).ToArray();
            var newToOld = Enumerable.Repeat(-1, b.Length).ToArray();
            var prefix = 0;
            while (prefix < a.Length && prefix < b.Length && a[prefix] == b[prefix]) { oldToNew[prefix] = prefix; newToOld[prefix] = prefix; prefix++; }
            var suffix = 0;
            while (suffix < a.Length - prefix && suffix < b.Length - prefix && a[a.Length - 1 - suffix] == b[b.Length - 1 - suffix])
            {
                oldToNew[a.Length - 1 - suffix] = b.Length - 1 - suffix;
                newToOld[b.Length - 1 - suffix] = a.Length - 1 - suffix;
                suffix++;
            }
            var an = a.Length - prefix - suffix;
            var bm = b.Length - prefix - suffix;
            if (an == 0 || bm == 0) return (oldToNew, newToOld);

            var matched = MarkdownSourceMapService.MyersMatch(a, prefix, an, b, prefix, bm);
            if (matched == null)
                throw new InvalidOperationException(UiText.Say(
                    $"Le due versioni differiscono per più di {MarkdownSourceMapService.MaxDiffDistance} righe: le parti cambiate non si calcolano. Usa la vista testuale.",
                    $"The two versions differ by more than {MarkdownSourceMapService.MaxDiffDistance} lines: the changed parts cannot be computed. Use the text view."));
            foreach (var pair in matched) { oldToNew[pair.Key] = pair.Value; newToOld[pair.Value] = pair.Key; }
            return (oldToNew, newToOld);
        }

        /// <summary>I blocchi di primo livello che nella pagina si vedono; di un elenco, le sue voci (se non è trattato intero).</summary>
        private static List<Unit> Units(MarkdownDocument document, int[] starts, HashSet<Block> whole)
        {
            var units = new List<Unit>();
            foreach (var block in document)
            {
                if (block is YamlFrontMatterBlock || block is LinkReferenceDefinitionGroup || block is FootnoteGroup || block is BlankLineBlock) continue;
                if (block is ListBlock list && !whole.Contains(list) && list.Count > 0)
                {
                    foreach (var item in list) units.Add(UnitOf(item, starts, list));
                    continue;
                }
                units.Add(UnitOf(block, starts, null));
            }
            return units;
        }

        private static Unit UnitOf(Block block, int[] starts, Block list)
        {
            var start = block.Line;
            var end = block.Span.End >= 0 ? LineOf(starts, block.Span.End) : start;
            return new Unit { Start = start, End = Math.Max(start, end), List = list };
        }

        /// <summary>
        /// Un blocco è rimasto uguale quando ogni sua riga ha la riga uguale nell'altro testo, e quelle righe sono
        /// esattamente un blocco dello stesso tipo. Due paragrafi fusi togliendo una riga vuota non lo sono.
        /// </summary>
        private static void Pair(List<Unit> oldUnits, List<Unit> newUnits, int[] oldToNew, int[] newToOld)
        {
            foreach (var u in oldUnits) { u.Touched = true; u.Peer = -1; }
            foreach (var u in newUnits) { u.Touched = true; u.Peer = -1; }
            var newByStart = new Dictionary<int, int>();
            for (var j = 0; j < newUnits.Count; j++) newByStart[newUnits[j].Start] = j;

            for (var i = 0; i < oldUnits.Count; i++)
            {
                var o = oldUnits[i];
                var first = oldToNew[o.Start];
                if (first < 0 || !newByStart.TryGetValue(first, out var j)) continue;
                var n = newUnits[j];
                if (n.End - n.Start != o.End - o.Start || (o.List == null) != (n.List == null)) continue;
                var same = true;
                for (var k = 0; k <= o.End - o.Start && same; k++) same = oldToNew[o.Start + k] == n.Start + k;
                if (!same) continue;
                o.Touched = false; o.Peer = j;
                n.Touched = false; n.Peer = i;
            }
        }

        private static List<MarkdownChangeRegion> Walk(List<Unit> oldUnits, List<Unit> newUnits, int newLineCount, out HashSet<Block> toCollapse)
        {
            toCollapse = new HashSet<Block>();
            var regions = new List<MarkdownChangeRegion>();
            int i = 0, j = 0;
            while (i < oldUnits.Count || j < newUnits.Count)
            {
                var oldSame = i < oldUnits.Count && !oldUnits[i].Touched;
                var newSame = j < newUnits.Count && !newUnits[j].Touched;
                if (oldSame && newSame)
                {
                    if (oldUnits[i].Peer != j)
                        throw new InvalidOperationException("MarkdownChangeRegions: blocchi uguali fuori passo fra le due versioni (errore interno).");
                    i++; j++; continue;
                }

                var oldRun = new List<Unit>();
                var newRun = new List<Unit>();
                while (i < oldUnits.Count && oldUnits[i].Touched) oldRun.Add(oldUnits[i++]);
                while (j < newUnits.Count && newUnits[j].Touched) newRun.Add(newUnits[j++]);
                if (oldRun.Count == 0 && newRun.Count == 0)
                    throw new InvalidOperationException("MarkdownChangeRegions: blocchi uguali fuori passo fra le due versioni (errore interno).");

                var before = i - oldRun.Count - 1 >= 0 && j - newRun.Count - 1 >= 0
                    ? (oldUnits[i - oldRun.Count - 1], newUnits[j - newRun.Count - 1]) : ((Unit, Unit)?)null;
                var after = i < oldUnits.Count && j < newUnits.Count ? (oldUnits[i], newUnits[j]) : ((Unit, Unit)?)null;
                var nextNewStart = j < newUnits.Count ? newUnits[j].Start : newLineCount;

                var items = oldRun.Concat(newRun).Where(u => u.List != null).ToList();
                if (items.Count == 0)
                {
                    regions.Add(BlockRegion(oldRun, newRun, nextNewStart));
                    continue;
                }

                var oldLists = oldRun.Select(u => u.List).Distinct().ToList();
                var newLists = newRun.Select(u => u.List).Distinct().ToList();
                var onlyItems = items.Count == oldRun.Count + newRun.Count && oldLists.Count <= 1 && newLists.Count <= 1;
                (Unit, Unit)? neighbour = null;
                if (onlyItems)
                {
                    foreach (var candidate in new[] { before, after })
                    {
                        if (candidate == null) continue;
                        var (o, n) = candidate.Value;
                        if (o.List == null || n.List == null) continue;
                        if (oldLists.Count == 1 && oldLists[0] != o.List) continue;
                        if (newLists.Count == 1 && newLists[0] != n.List) continue;
                        neighbour = candidate; break;
                    }
                }
                if (neighbour == null)
                {
                    // Voci mescolate ad altri blocchi, o un elenco senza una voce rimasta uguale accanto: l'elenco si tratta intero.
                    foreach (var item in items) toCollapse.Add(item.List);
                    continue;
                }

                var region = new MarkdownChangeRegion
                {
                    ListItems = true,
                    OldItemLines = oldRun.Select(u => u.Start).ToList(),
                    NewItemLines = newRun.Select(u => u.Start).ToList(),
                };
                Span(region, oldRun, newRun);
                if (newRun.Count == 0)
                {
                    var newList = neighbour.Value.Item2.List;
                    if (after != null && after.Value.Item2.List == newList) region.NewBefore = after.Value.Item2.Start;
                    else region.NewAfterItem = before.Value.Item2.Start;
                }
                regions.Add(region);
            }
            return regions;
        }

        private static MarkdownChangeRegion BlockRegion(List<Unit> oldRun, List<Unit> newRun, int nextNewStart)
        {
            var region = new MarkdownChangeRegion();
            Span(region, oldRun, newRun);
            if (newRun.Count == 0) region.NewBefore = nextNewStart;
            return region;
        }

        private static void Span(MarkdownChangeRegion region, List<Unit> oldRun, List<Unit> newRun)
        {
            if (oldRun.Count > 0) { region.OldStart = oldRun[0].Start; region.OldEnd = oldRun[oldRun.Count - 1].End; }
            if (newRun.Count > 0) { region.NewStart = newRun[0].Start; region.NewEnd = newRun[newRun.Count - 1].End; }
        }

        /// <summary>Una riga non vuota, senza la sua uguale nell'altro testo, fuori da ogni blocco che si vede.</summary>
        private static bool HasHiddenChanges(List<Unit> units, int[] toOther, string[] lines)
        {
            var covered = new bool[lines.Length];
            foreach (var u in units) for (var k = u.Start; k <= u.End && k < covered.Length; k++) covered[k] = true;
            for (var k = 0; k < lines.Length; k++)
                if (!covered[k] && toOther[k] < 0 && lines[k].Trim().Length > 0) return true;
            return false;
        }
    }
}
