using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Scioglie le tabelle di Word che in markdown non stanno in piedi.
    /// <para>
    /// In Word la tabella serve spesso a impaginare: una riga è una scheda, con un codice, un testo lungo con
    /// elenchi e immagini, e qualche attributo breve. Una cella markdown regge solo una riga di testo, e quel
    /// contenuto ne esce schiacciato. Qui una tabella resta tabella finché le sue celle sono «leggere»; se una
    /// cella porta un'immagine, un elenco, un'altra tabella o più di <see cref="HeavyWords"/> parole, ogni riga
    /// diventa una scheda: la prima colonna fa da titolo, il contenuto pesante torna testo normale, le colonne
    /// brevi diventano campi «Intestazione: valore». Una tabella a una colonna sola è un riquadro.
    /// </para>
    /// <para>
    /// Si lavora sull'HTML che esce da Word, dove paragrafi ed elenchi nelle celle sono ancora distinti. Le
    /// parole restano quelle: cambia la disposizione, e le intestazioni si ripetono in ogni scheda.
    /// </para>
    /// </summary>
    public static class WordTableCards
    {
        /// <summary>Oltre queste parole una cella non è più da tabella (utente, 17/11/2026).</summary>
        public const int HeavyWords = 60;

        // Una prima colonna più lunga di così non è un titolo.
        private const int TitleWords = 12;

        public static string Apply(string html, List<string> warnings)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            var root = document.DocumentNode;

            // Il livello dei titoli delle schede dipende dal titolo che precede la tabella nel documento
            // d'origine: lo si legge prima di cambiare qualcosa, o le schede di una tabella sposterebbero la successiva.
            var plan = new List<(HtmlNode Table, int BaseLevel)>();
            var level = 1;
            foreach (var node in root.Descendants().ToList())
            {
                if (node.Ancestors("table").Any())
                {
                    continue;
                }
                if (IsHeading(node))
                {
                    level = node.Name[1] - '0';
                }
                else if (node.Name == "table")
                {
                    plan.Add((node, level));
                }
            }

            foreach (var (table, baseLevel) in plan)
            {
                Melt(table, baseLevel, warnings);
            }
            return root.OuterHtml;
        }

        private static void Melt(HtmlNode table, int baseLevel, List<string> warnings)
        {
            var rows = table.Descendants("tr").Where(row => row.Ancestors("table").First() == table)
                .Select(row => row.ChildNodes.Where(cell => cell.Name == "td" || cell.Name == "th").ToList())
                .Where(cells => cells.Count > 0)
                .ToList();
            if (rows.Count == 0)
            {
                return;
            }

            var columns = rows.Max(cells => cells.Sum(Span));
            if (columns > 1 && !rows.Any(cells => cells.Any(IsHeavy)))
            {
                return;
            }
            if (rows.Any(cells => cells.Any(cell => cell.GetAttributeValue("rowspan", 1) > 1)))
            {
                warnings.Add(UiText.Say(
                    "Una tabella con celle unite in verticale ha contenuti troppo lunghi per una tabella markdown: è rimasta tabella, va sistemata a mano.",
                    "A table with vertically merged cells has contents too long for a markdown table: it stayed a table and needs fixing by hand."));
                return;
            }

            var output = new StringBuilder();
            if (columns == 1)
            {
                foreach (var cells in rows.Where(cells => !IsEmpty(cells[0])))
                {
                    MeltNested(cells[0], baseLevel, warnings);
                    output.Append("<blockquote>").Append(cells[0].InnerHtml).Append("</blockquote>");
                }
            }
            else
            {
                // Una riga con una sola cella piena, in una tabella larga, è il titolo di un gruppo di righe.
                bool IsGroup(List<HtmlNode> cells) => columns >= 3 && cells.Count(cell => !IsEmpty(cell)) == 1 && !IsHeavy(cells.First(cell => !IsEmpty(cell)));

                // Le intestazioni: la riga che Word ha marcato come tale o, in una tabella larga, la prima.
                // Una tabella a due colonne senza quella marca è fatta di coppie etichetta/valore: non ha intestazioni.
                var header = rows.FirstOrDefault(cells => !IsGroup(cells));
                if (header != null && !(header.All(cell => cell.Name == "th") || columns >= 3))
                {
                    header = null;
                }
                var labels = new string[columns];
                if (header != null)
                {
                    var column = 0;
                    foreach (var cell in header)
                    {
                        labels[column] = Inline(cell, " ");
                        column += Span(cell);
                    }
                }

                var cardLevel = Math.Min(baseLevel + 1, 6);
                var first = true;
                foreach (var cells in rows.Where(cells => cells != header))
                {
                    if (cells.All(IsEmpty))
                    {
                        continue;
                    }
                    if (IsGroup(cells))
                    {
                        output.Append("<p><strong>").Append(Inline(cells.First(cell => !IsEmpty(cell)), " ")).Append("</strong></p>");
                        continue;
                    }

                    var titled = IsTitle(cells[0]);
                    if (titled)
                    {
                        output.Append("<h").Append(cardLevel).Append('>').Append(Inline(cells[0], " ")).Append("</h").Append(cardLevel).Append('>');
                    }
                    else if (!first)
                    {
                        output.Append("<hr>");
                    }
                    first = false;

                    var fields = new StringBuilder();
                    void CloseFields()
                    {
                        if (fields.Length > 0)
                        {
                            output.Append("<ul>").Append(fields).Append("</ul>");
                            fields.Clear();
                        }
                    }

                    var index = 0;
                    foreach (var cell in cells)
                    {
                        var label = labels[Math.Min(index, columns - 1)];
                        var isTitleCell = titled && cell == cells[0];
                        index += Span(cell);
                        if (isTitleCell || IsEmpty(cell))
                        {
                            continue;
                        }

                        if (IsHeavy(cell))
                        {
                            CloseFields();
                            MeltNested(cell, titled ? cardLevel : baseLevel, warnings);
                            if (!string.IsNullOrWhiteSpace(label))
                            {
                                output.Append("<p><strong>").Append(label).Append("</strong></p>");
                            }
                            output.Append(cell.InnerHtml);
                        }
                        else if (string.IsNullOrWhiteSpace(label))
                        {
                            // Un valore senza intestazione è testo della scheda, non un campo.
                            CloseFields();
                            output.Append("<p>").Append(Inline(cell, "<br>")).Append("</p>");
                        }
                        else
                        {
                            fields.Append("<li><strong>").Append(label).Append(label.TrimEnd().EndsWith(":", StringComparison.Ordinal) ? string.Empty : ":").Append("</strong> ")
                                .Append(Inline(cell, "<br>")).Append("</li>");
                        }
                    }
                    CloseFields();
                }
            }

            var wrapper = HtmlNode.CreateNode("<div>" + output + "</div>");
            foreach (var child in wrapper.ChildNodes.ToList())
            {
                table.ParentNode.InsertBefore(child, table);
            }
            table.Remove();
        }

        // Le tabelle dentro una cella si sciolgono prima della cella che le contiene, un livello più sotto.
        private static void MeltNested(HtmlNode cell, int baseLevel, List<string> warnings)
        {
            foreach (var nested in cell.Descendants("table").Where(inner => inner.Ancestors("table").First() == cell.Ancestors("table").First()).ToList())
            {
                Melt(nested, baseLevel, warnings);
            }
        }

        private static bool IsHeavy(HtmlNode cell)
        {
            return cell.Descendants().Any(node => node.Name == "img" || node.Name == "ul" || node.Name == "ol" || node.Name == "table")
                || Words(cell) > HeavyWords;
        }

        private static bool IsTitle(HtmlNode cell)
        {
            return !IsEmpty(cell) && !IsHeavy(cell) && Words(cell) <= TitleWords;
        }

        private static bool IsEmpty(HtmlNode cell)
        {
            return Words(cell) == 0 && !cell.Descendants("img").Any();
        }

        private static int Words(HtmlNode cell)
        {
            return HtmlEntity.DeEntitize(cell.InnerText).Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private static int Span(HtmlNode cell)
        {
            return Math.Max(1, cell.GetAttributeValue("colspan", 1));
        }

        // Il contenuto di una cella leggera su una riga sola: i suoi paragrafi uno dopo l'altro.
        private static string Inline(HtmlNode cell, string separator)
        {
            var paragraphs = cell.ChildNodes.Where(node => node.Name == "p" && node.InnerText.Trim().Length > 0).ToList();
            return paragraphs.Count == 0 ? cell.InnerHtml.Trim() : string.Join(separator, paragraphs.Select(paragraph => paragraph.InnerHtml.Trim()));
        }

        private static bool IsHeading(HtmlNode node)
        {
            return node.Name.Length == 2 && node.Name[0] == 'h' && node.Name[1] >= '1' && node.Name[1] <= '6';
        }
    }
}
