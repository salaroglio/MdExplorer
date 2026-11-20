using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il risultato di una SELECT in formato tabellare (SPARQL TSV, <c>text/tab-separated-values</c>): per le letture grandi
    /// (tutti gli archi della navigazione) è circa un quarto del JSON e si legge senza costruire un albero (misurato il
    /// 20/11/2026 su 350 mila archi: 290 MB di JSON contro 69 MB). Ogni valore torna come in <see cref="SparqlResults"/>: un IRI
    /// senza parentesi angolari, un letterale nella sua forma lessicale (senza virgolette, tipo o lingua), vuoto = non legato (null).
    /// </summary>
    public static class SparqlTsv
    {
        public sealed record Table(string[] Columns, List<string[]> Rows)
        {
            public int Column(string name) => Array.IndexOf(Columns, name);
        }

        public static Table Parse(string tsv)
        {
            var rows = new List<string[]>();
            if (string.IsNullOrEmpty(tsv)) return new Table(Array.Empty<string>(), rows);
            var end = tsv.IndexOf('\n');
            var header = (end < 0 ? tsv : tsv.Substring(0, end)).TrimEnd('\r').Split('\t');
            var columns = new string[header.Length];
            for (var i = 0; i < header.Length; i++) columns[i] = header[i].TrimStart('?');
            var pos = end < 0 ? tsv.Length : end + 1;
            while (pos < tsv.Length)
            {
                var next = tsv.IndexOf('\n', pos);
                var lineEnd = next < 0 ? tsv.Length : next;
                if (lineEnd > pos && tsv[lineEnd - 1] == '\r') lineEnd--;
                if (lineEnd > pos)
                {
                    var row = new string[columns.Length];
                    var field = 0; var start = pos;
                    for (var i = pos; i <= lineEnd && field < columns.Length; i++)
                    {
                        if (i < lineEnd && tsv[i] != '\t') continue;
                        row[field++] = Term(tsv, start, i);
                        start = i + 1;
                    }
                    rows.Add(row);
                }
                pos = next < 0 ? tsv.Length : next + 1;
            }
            return new Table(columns, rows);
        }

        /// <summary>Un termine RDF scritto come in Turtle: &lt;iri&gt;, "letterale"(^^tipo|@lingua), numero o booleano nudo, _:nodo.</summary>
        private static string Term(string s, int start, int end)
        {
            if (end <= start) return null;
            var c = s[start];
            if (c == '<' && s[end - 1] == '>') return s.Substring(start + 1, end - start - 2);
            if (c == '"') return Unescape(s, start + 1, ClosingQuote(s, start + 1, end));
            return s.Substring(start, end - start);   // 42, true, _:b0
        }

        private static int ClosingQuote(string s, int from, int end)
        {
            for (var i = from; i < end; i++)
            {
                if (s[i] == '\\') { i++; continue; }
                if (s[i] == '"') return i;
            }
            throw new FormatException("SPARQL TSV: letterale senza virgolette di chiusura");
        }

        private static string Unescape(string s, int from, int to)
        {
            var slash = s.IndexOf('\\', from, to - from);
            if (slash < 0) return s.Substring(from, to - from);
            var sb = new StringBuilder(to - from);
            for (var i = from; i < to; i++)
            {
                var ch = s[i];
                if (ch != '\\' || i + 1 >= to) { sb.Append(ch); continue; }
                var e = s[++i];
                switch (e)
                {
                    case 't': sb.Append('\t'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u': sb.Append((char)int.Parse(s.AsSpan(i + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture)); i += 4; break;
                    case 'U': sb.Append(char.ConvertFromUtf32(int.Parse(s.AsSpan(i + 1, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture))); i += 8; break;
                    default: sb.Append(e); break;   // \" \' \\
                }
            }
            return sb.ToString();
        }
    }
}
