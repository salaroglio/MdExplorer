using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Features.Services;
using TreeSitter;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Le zone che la grammatica tree-sitter non sa leggere (C6 della valutazione del 07/11). Tree-sitter le chiude in un
    /// nodo ERROR (o MISSING) e va avanti; gli estrattori guardano solo i nodi che conoscono, quindi quello che c'è dentro
    /// spariva dal grafo SENZA AVVISO. Misurato l'08/11: su 3.662 file Java di PlantUML uno solo, per un costrutto valido
    /// che la grammatica non conosce (<c>for (T x : (Cast) y)</c>), e il file intero è diventato una zona d'errore: nel
    /// grafo c'era solo l'intestazione. Qui non si recupera niente (il grafo resta quello di prima, nessuna nuova versione
    /// dell'estrattore): si dice, con le righe e un esempio.
    /// </summary>
    public static class TreeSitterParseErrors
    {
        /// <summary>Quante zone si elencano per file: oltre, solo il numero.</summary>
        public const int MaxZones = 3;

        /// <summary>L'avviso per il file, o null se la grammatica l'ha letto tutto.</summary>
        public static string Describe(Node root, string text, string logicalPath, string languageName)
        {
            if (root == null || !root.HasError) return null;
            // Le zone più interne: un ERROR che ne contiene altri direbbe solo «tutto il file».
            var zones = Innermost(root).ToList();
            if (zones.Count == 0) return null;
            var lines = text.Split('\n');
            string Rows(Node n)
            {
                var from = n.StartPosition.Row + 1;
                var to = n.EndPosition.Row + 1;
                return from == to ? from.ToString() : $"{from}–{to}";
            }
            var rows = string.Join(", ", zones.Take(MaxZones).Select(Rows)) + (zones.Count > MaxZones ? UiText.Say($" e altre {zones.Count - MaxZones}", $" and {zones.Count - MaxZones} more") : "");
            var first = zones[0];
            var example = first.StartPosition.Row < lines.Length ? lines[first.StartPosition.Row].Trim() : string.Empty;
            if (example.Length > 80) example = example.Substring(0, 80) + "…";
            return UiText.Say(
                $"{logicalPath}: righe {rows} non lette dalla grammatica {languageName} (es. «{example}»): nel grafo il file è incompleto, quello che c'è in quelle zone (e a volte tutto il file) manca",
                $"{logicalPath}: lines {rows} not read by the {languageName} grammar (e.g. «{example}»): the file is incomplete in the graph, what is in those zones (sometimes the whole file) is missing");
        }

        private static IEnumerable<Node> Innermost(Node node)
        {
            var found = false;
            foreach (var child in node.Children)
            {
                if (!child.HasError && !child.IsMissing) continue;
                foreach (var inner in Innermost(child)) { found = true; yield return inner; }
            }
            if (!found && (node.IsError || node.IsMissing)) yield return node;
        }
    }
}
