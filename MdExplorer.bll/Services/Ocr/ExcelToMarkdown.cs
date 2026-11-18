using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Un Excel (.xlsx, .xlsm) in markdown, senza AI: ogni foglio visibile è un capitolo con la sua tabella. I valori
    /// sono quelli che Excel ha calcolato e salvato, non le formule; la prima riga piena fa da intestazione.
    /// <para>
    /// Quello che in una tabella markdown non c'è non viene portato: grafici, colori, celle unite (il valore resta
    /// nella prima cella). Un foglio oltre <see cref="MaxRows"/> righe o <see cref="MaxColumns"/> colonne viene
    /// tagliato, e lo si dice nel documento e fra gli avvisi.
    /// </para>
    /// </summary>
    public static class ExcelToMarkdown
    {
        public const int MaxRows = 500;
        public const int MaxColumns = 40;

        public static string Convert(string xlsxPath, string title, List<string> warnings)
        {
            var markdown = new StringBuilder("# " + OneLine(title) + "\n");
            using (var document = SpreadsheetDocument.Open(xlsxPath, false))
            {
                var workbook = document.WorkbookPart;
                var shared = (workbook.SharedStringTablePart?.SharedStringTable?.Elements<SharedStringItem>() ?? Enumerable.Empty<SharedStringItem>())
                    .Select(Text).ToList();
                var dateStyles = DateStyles(workbook.WorkbookStylesPart?.Stylesheet, out var percentStyles);

                var written = 0;
                var hidden = 0;
                foreach (var sheet in workbook.Workbook.Sheets.Elements<Sheet>())
                {
                    if (sheet.State != null && sheet.State.Value != SheetStateValues.Visible)
                    {
                        hidden++;
                        continue;
                    }
                    if (!(workbook.GetPartById(sheet.Id) is WorksheetPart part))
                    {
                        continue;   // un foglio-grafico: non ha celle
                    }

                    // riga → (colonna → testo), solo le celle piene
                    var rows = new SortedDictionary<int, SortedDictionary<int, string>>();
                    foreach (var row in part.Worksheet.Descendants<Row>())
                    {
                        var position = 0;
                        foreach (var cell in row.Elements<Cell>())
                        {
                            var (line, column) = Reference(cell.CellReference?.Value, (int)(row.RowIndex?.Value ?? 0), ++position);
                            position = column;
                            var text = Value(cell, shared, dateStyles, percentStyles);
                            if (text.Length > 0)
                            {
                                if (!rows.TryGetValue(line, out var cells))
                                {
                                    rows[line] = cells = new SortedDictionary<int, string>();
                                }
                                cells[column] = text;
                            }
                        }
                    }
                    if (rows.Count == 0)
                    {
                        continue;
                    }

                    written++;
                    var name = OneLine(sheet.Name?.Value ?? string.Empty);
                    markdown.Append("\n## ").Append(name).Append("\n\n");
                    var first = rows.Values.Min(cells => cells.Keys.First());
                    var last = rows.Values.Max(cells => cells.Keys.Last());
                    var columns = Math.Min(last - first + 1, MaxColumns);
                    var kept = rows.Take(MaxRows).ToList();
                    if (rows.Count > MaxRows || last - first + 1 > MaxColumns)
                    {
                        var cut = UiText.Say(
                            $"Il foglio «{name}» ha {rows.Count} righe e {last - first + 1} colonne: qui ci sono le prime {kept.Count} righe e {columns} colonne.",
                            $"The sheet \"{name}\" has {rows.Count} rows and {last - first + 1} columns: here are the first {kept.Count} rows and {columns} columns.");
                        warnings.Add(cut);
                        markdown.Append("> ").Append(cut).Append("\n\n");
                    }

                    for (var i = 0; i < kept.Count; i++)
                    {
                        markdown.Append('|');
                        for (var column = first; column < first + columns; column++)
                        {
                            markdown.Append(' ').Append(kept[i].Value.TryGetValue(column, out var text) ? Escape(text) : string.Empty).Append(" |");
                        }
                        markdown.Append('\n');
                        if (i == 0)
                        {
                            markdown.Append('|').Append(string.Concat(Enumerable.Repeat(" --- |", columns))).Append('\n');
                        }
                    }
                }

                if (written == 0)
                {
                    markdown.Append('\n').Append(UiText.Say("_Il file Excel non ha celle piene._", "_The Excel file has no filled cells._")).Append('\n');
                }
                if (hidden > 0)
                {
                    warnings.Add(UiText.Say(
                        $"L'Excel «{OneLine(title)}» ha {hidden} fogli nascosti: non sono stati convertiti.",
                        $"The Excel file \"{OneLine(title)}\" has {hidden} hidden sheets: they were not converted."));
                }
            }
            return markdown.ToString();
        }

        private static string Value(Cell cell, List<string> shared, HashSet<uint> dateStyles, HashSet<uint> percentStyles)
        {
            var raw = cell.CellValue?.Text ?? string.Empty;
            var type = cell.DataType?.Value;
            if (type == CellValues.SharedString)
            {
                return int.TryParse(raw, out var index) && index >= 0 && index < shared.Count ? shared[index].Trim() : string.Empty;
            }
            if (type == CellValues.InlineString)
            {
                return (cell.InlineString?.InnerText ?? string.Empty).Trim();
            }
            if (type == CellValues.Boolean)
            {
                return raw == "1" ? UiText.Say("VERO", "TRUE") : UiText.Say("FALSO", "FALSE");
            }
            if (type == CellValues.String || type == CellValues.Error || type == CellValues.Date)
            {
                return raw.Trim();
            }
            if (raw.Length == 0 || !double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                return raw.Trim();
            }

            var style = cell.StyleIndex?.Value ?? 0;
            if (dateStyles.Contains(style) && number >= 0 && number < 2958466)
            {
                var date = DateTime.FromOADate(number);
                return number < 1 ? date.ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                    : date.TimeOfDay == TimeSpan.Zero ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            // Come lo ha salvato Excel, senza le code del binario (0.30000000000000004).
            return percentStyles.Contains(style)
                ? (number * 100).ToString("0.############", CultureInfo.InvariantCulture) + "%"
                : number.ToString("0.###############", CultureInfo.InvariantCulture);
        }

        // Quali stili di cella mostrano una data o un'ora (i formati di serie di Excel e quelli scritti a mano), e quali una percentuale.
        private static HashSet<uint> DateStyles(Stylesheet styles, out HashSet<uint> percentStyles)
        {
            var dates = new HashSet<uint>();
            percentStyles = new HashSet<uint>();
            if (styles?.CellFormats == null)
            {
                return dates;
            }
            var custom = (styles.NumberingFormats?.Elements<NumberingFormat>() ?? Enumerable.Empty<NumberingFormat>())
                .Where(format => format.NumberFormatId != null)
                .ToDictionary(format => format.NumberFormatId.Value, format => format.FormatCode?.Value ?? string.Empty);
            uint index = 0;
            foreach (var format in styles.CellFormats.Elements<CellFormat>())
            {
                var id = format.NumberFormatId?.Value ?? 0;
                if (custom.TryGetValue(id, out var code))
                {
                    var bare = Regex.Replace(code, @"""[^""]*""|\[[^\]]*\]|\\.", string.Empty);
                    if (Regex.IsMatch(bare, "[dmyhs]", RegexOptions.IgnoreCase))
                    {
                        dates.Add(index);
                    }
                    else if (bare.Contains("%"))
                    {
                        percentStyles.Add(index);
                    }
                }
                else if ((id >= 14 && id <= 22) || (id >= 45 && id <= 47))
                {
                    dates.Add(index);
                }
                else if (id == 9 || id == 10)
                {
                    percentStyles.Add(index);
                }
                index++;
            }
            return dates;
        }

        private static string Text(SharedStringItem item)
        {
            // Solo il testo vero: le guide fonetiche (rPh) stanno nello stesso elemento e non si leggono.
            return item.Text != null ? item.Text.Text : string.Concat(item.Elements<Run>().Select(run => run.Text?.Text ?? string.Empty));
        }

        // «BC12» → riga 12, colonna 55. Una cella senza riferimento sta dopo la precedente.
        private static (int Row, int Column) Reference(string reference, int row, int next)
        {
            if (string.IsNullOrEmpty(reference))
            {
                return (row, next);
            }
            var column = 0;
            var i = 0;
            for (; i < reference.Length && char.IsLetter(reference[i]); i++)
            {
                column = column * 26 + (char.ToUpperInvariant(reference[i]) - 'A' + 1);
            }
            return (int.TryParse(reference.Substring(i), out var line) ? line : row, column == 0 ? next : column);
        }

        private static string Escape(string text)
        {
            return text.Replace("\\", "\\\\").Replace("|", "\\|").Replace("\r\n", "\n").Replace("\n", "<br>").Trim();
        }

        private static string OneLine(string text) => Regex.Replace(text ?? string.Empty, @"\s+", " ").Trim();
    }
}
