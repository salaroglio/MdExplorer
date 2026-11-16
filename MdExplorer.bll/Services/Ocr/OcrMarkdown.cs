using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Da ciò che il modello OCR scrive per una pagina al markdown che finisce nel file.
    /// <para>
    /// Il modello restituisce già markdown, tranne le tabelle: quelle le scrive in OTSL, una riga di
    /// marcatori (<c>&lt;fcel&gt;</c> cella piena, <c>&lt;ecel&gt;</c> cella vuota, <c>&lt;lcel&gt;</c> /
    /// <c>&lt;ucel&gt;</c> / <c>&lt;xcel&gt;</c> cella fusa con quella a sinistra / sopra / entrambe,
    /// <c>&lt;nl&gt;</c> fine riga). Tradurle è un lavoro deterministico, quindi lo fa il codice e non
    /// un secondo giro di LLM.
    /// </para>
    /// </summary>
    public static class OcrMarkdown
    {
        private static readonly Regex OtslToken =
            new Regex(@"<(fcel|ecel|lcel|ucel|xcel|ched|rhed|srow|nl)>", RegexOptions.Compiled);

        private static readonly Regex WholeFence =
            new Regex(@"\A\s*```(?:markdown|md)?[ \t]*\r?\n(?<body>.*?)\r?\n```\s*\z",
                RegexOptions.Compiled | RegexOptions.Singleline);

        /// <summary>Il markdown di una pagina: tolto l'eventuale recinto, tradotte le tabelle.</summary>
        public static string FromPage(string modelOutput)
        {
            if (string.IsNullOrWhiteSpace(modelOutput))
            {
                return string.Empty;
            }

            var text = modelOutput.Replace("\r\n", "\n");
            // Ciò che precede «</think>» è ragionamento del modello, non testo della pagina.
            var thinkEnd = text.LastIndexOf("</think>", StringComparison.Ordinal);
            if (thinkEnd >= 0)
            {
                text = text.Substring(thinkEnd + "</think>".Length);
            }
            var fenced = WholeFence.Match(text);
            if (fenced.Success)
            {
                text = fenced.Groups["body"].Value;
            }

            var output = new List<string>();
            var table = new StringBuilder();
            foreach (var line in text.Split('\n'))
            {
                if (IsOtslLine(line))
                {
                    table.Append(line.Trim());
                    continue;
                }
                if (table.Length > 0)
                {
                    output.Add(TableFromOtsl(table.ToString()));
                    table.Clear();
                }
                output.Add(line);
            }
            if (table.Length > 0)
            {
                output.Add(TableFromOtsl(table.ToString()));
            }
            return string.Join("\n", output).Trim();
        }

        /// <summary>Le pagine in fila, separate da una riga vuota; le pagine vuote non lasciano buchi.</summary>
        public static string JoinPages(IEnumerable<string> pages)
        {
            return string.Join("\n\n", pages.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim())) + "\n";
        }

        private static bool IsOtslLine(string line)
        {
            var match = OtslToken.Match(line);
            return match.Success && string.IsNullOrWhiteSpace(line.Substring(0, match.Index));
        }

        /// <summary>
        /// Una tabella OTSL in markdown. Senza celle fuse esce la tabella a barre verticali; con celle
        /// fuse esce HTML, perché la tabella markdown non sa dire «questa cella copre due colonne» e
        /// spezzarla in celle vuote cambierebbe il significato della tabella.
        /// </summary>
        public static string TableFromOtsl(string otsl)
        {
            var rows = ParseRows(otsl);
            if (rows.Count == 0)
            {
                return string.Empty;
            }

            var width = rows.Max(r => r.Count);
            foreach (var row in rows)
            {
                while (row.Count < width)
                {
                    row.Add(new Cell("ecel", string.Empty));
                }
            }

            var merged = rows.Any(r => r.Any(c => c.IsMerge));
            return merged ? HtmlTable(rows, width) : PipeTable(rows, width);
        }

        private static List<List<Cell>> ParseRows(string otsl)
        {
            var rows = new List<List<Cell>>();
            var current = new List<Cell>();
            var matches = OtslToken.Matches(otsl);
            for (var i = 0; i < matches.Count; i++)
            {
                var kind = matches[i].Groups[1].Value;
                if (kind == "nl")
                {
                    if (current.Count > 0)
                    {
                        rows.Add(current);
                        current = new List<Cell>();
                    }
                    continue;
                }
                var start = matches[i].Index + matches[i].Length;
                var end = i + 1 < matches.Count ? matches[i + 1].Index : otsl.Length;
                current.Add(new Cell(kind, otsl.Substring(start, end - start).Trim()));
            }
            if (current.Count > 0)
            {
                rows.Add(current);
            }
            return rows;
        }

        private static string PipeTable(List<List<Cell>> rows, int width)
        {
            var sb = new StringBuilder();
            for (var r = 0; r < rows.Count; r++)
            {
                sb.Append("| ").Append(string.Join(" | ", rows[r].Select(c => PipeText(c.Text)))).Append(" |\n");
                if (r == 0)
                {
                    sb.Append("| ").Append(string.Join(" | ", Enumerable.Repeat("---", width))).Append(" |\n");
                }
            }
            return sb.ToString().TrimEnd('\n');
        }

        private static string PipeText(string text) =>
            text.Replace("|", "\\|").Replace("\n", "<br>");

        private static string HtmlTable(List<List<Cell>> rows, int width)
        {
            var sb = new StringBuilder("<table>\n");
            for (var r = 0; r < rows.Count; r++)
            {
                sb.Append("<tr>");
                for (var c = 0; c < width; c++)
                {
                    var cell = rows[r][c];
                    if (cell.IsMerge)
                    {
                        continue;
                    }

                    var colspan = 1;
                    while (c + colspan < width && rows[r][c + colspan].Kind == "lcel")
                    {
                        colspan++;
                    }
                    var rowspan = 1;
                    while (r + rowspan < rows.Count && rows[r + rowspan][c].Kind == "ucel")
                    {
                        rowspan++;
                    }

                    var tag = r == 0 || cell.Kind == "ched" || cell.Kind == "rhed" ? "th" : "td";
                    sb.Append('<').Append(tag);
                    if (colspan > 1)
                    {
                        sb.Append(" colspan=\"").Append(colspan).Append('"');
                    }
                    if (rowspan > 1)
                    {
                        sb.Append(" rowspan=\"").Append(rowspan).Append('"');
                    }
                    sb.Append('>').Append(WebUtility.HtmlEncode(cell.Text)).Append("</").Append(tag).Append('>');
                }
                sb.Append("</tr>\n");
            }
            return sb.Append("</table>").ToString();
        }

        private sealed class Cell
        {
            public Cell(string kind, string text)
            {
                Kind = kind;
                Text = text;
            }

            public string Kind { get; }
            public string Text { get; }
            public bool IsMerge => Kind == "lcel" || Kind == "ucel" || Kind == "xcel";
        }
    }
}
