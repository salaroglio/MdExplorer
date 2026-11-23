using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.SourceMapping
{
    /// <summary>Una parte cambiata come la riceve la pagina: come trovarla nei due documenti resi.</summary>
    public sealed class MarkedChangeRegion
    {
        public int Id { get; set; }
        /// <summary>changed | added | removed.</summary>
        public string Kind { get; set; }
        /// <summary>
        /// false: i blocchi stanno fra i marcatori <c>mde-chg-s</c> / <c>mde-chg-e</c> con <c>data-r</c> = <see cref="Id"/>.
        /// true: sono voci d'elenco, da cercare per <c>data-mde-line-start</c> (righe da 1 dei testi marcati).
        /// </summary>
        public bool ListItems { get; set; }
        public List<int> OldItems { get; set; } = new List<int>();
        public List<int> NewItems { get; set; } = new List<int>();
        /// <summary>Voci tolte: vanno prima della voce che inizia a questa riga del nuovo (0 = non vale).</summary>
        public int NewBeforeItem { get; set; }
        /// <summary>Voci tolte in coda: vanno dopo la voce che inizia a questa riga del nuovo (0 = non vale).</summary>
        public int NewAfterItem { get; set; }
    }

    public sealed class MarkedChanges
    {
        public string OldText { get; set; }
        public string NewText { get; set; }
        public List<MarkedChangeRegion> Regions { get; set; } = new List<MarkedChangeRegion>();
        public bool HiddenChanges { get; set; }
    }

    /// <summary>
    /// Prepara i due testi per la resa con i riquadri: attorno a ogni parte fatta di blocchi interi mette due marcatori
    /// (righe HTML che la resa lascia passare); delle voci d'elenco dà le righe, contate nei testi con i marcatori.
    /// Diagrammi e immagini resi non portano le righe del sorgente, e un marcatore dentro un elenco lo spezzerebbe:
    /// per questo i due modi.
    /// </summary>
    public static class MarkdownChangeMarkup
    {
        public const string StartClass = "mde-chg-s";
        public const string EndClass = "mde-chg-e";

        public static string StartMarker(int id) => $"<div class=\"{StartClass}\" data-r=\"{id}\"></div>";
        public static string EndMarker(int id) => $"<div class=\"{EndClass}\" data-r=\"{id}\"></div>";

        public static MarkedChanges Build(MarkdownChanges changes)
        {
            var oldBefore = new SortedDictionary<int, List<string>>();   // riga → righe da mettere PRIMA di lei
            var newBefore = new SortedDictionary<int, List<string>>();
            var id = 0;
            var numbered = new List<(MarkdownChangeRegion region, int id)>();
            foreach (var region in changes.Regions)
            {
                id++;
                numbered.Add((region, id));
                if (region.ListItems) continue;
                if (region.HasOld) Surround(oldBefore, region.OldStart, region.OldEnd, id);
                if (region.HasNew) Surround(newBefore, region.NewStart, region.NewEnd, id);
                else Add(newBefore, region.NewBefore, "", StartMarker(id), "", EndMarker(id), "");
            }

            var (oldText, oldMap) = Weave(changes.OldLines, oldBefore);
            var (newText, newMap) = Weave(changes.NewLines, newBefore);

            var result = new MarkedChanges { OldText = oldText, NewText = newText, HiddenChanges = changes.HiddenChanges };
            foreach (var (region, regionId) in numbered)
            {
                var marked = new MarkedChangeRegion { Id = regionId, Kind = region.Kind, ListItems = region.ListItems };
                if (region.ListItems)
                {
                    marked.OldItems = region.OldItemLines.Select(l => oldMap[l] + 1).ToList();
                    marked.NewItems = region.NewItemLines.Select(l => newMap[l] + 1).ToList();
                    if (region.NewBefore >= 0) marked.NewBeforeItem = newMap[region.NewBefore] + 1;
                    if (region.NewAfterItem >= 0) marked.NewAfterItem = newMap[region.NewAfterItem] + 1;
                }
                result.Regions.Add(marked);
            }
            return result;
        }

        private static void Surround(SortedDictionary<int, List<string>> before, int start, int end, int id)
        {
            Add(before, start, "", StartMarker(id), "");
            Add(before, end + 1, "", EndMarker(id), "");
        }

        private static void Add(SortedDictionary<int, List<string>> before, int line, params string[] lines)
        {
            if (!before.TryGetValue(line, out var list)) before[line] = list = new List<string>();
            list.AddRange(lines);
        }

        /// <summary>Il testo con le righe inserite, e per ogni riga di partenza il suo indice (da 0) nel testo nuovo.</summary>
        private static (string text, int[] map) Weave(string[] lines, SortedDictionary<int, List<string>> before)
        {
            var output = new List<string>(lines.Length + before.Count * 3);
            var map = new int[lines.Length + 1];
            for (var i = 0; i <= lines.Length; i++)
            {
                if (before.TryGetValue(i, out var extra)) output.AddRange(extra);
                map[i] = output.Count;
                if (i < lines.Length) output.Add(lines[i]);
            }
            return (string.Join("\n", output), map);
        }
    }
}
