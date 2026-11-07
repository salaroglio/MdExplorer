using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// I percorsi che git scrive nel suo output (N2 della valutazione del 07/11, docs-internal/QA/2026-11-07-Valutazione-Qualita-MDE.md).
    /// Senza <c>core.quotePath=false</c> git mette fra virgolette e in ottale ogni nome con lettere accentate: un conflitto su
    /// <c>perché.md</c> esce come <c>"perch\303\251.md"</c>, e chi toglieva solo le virgolette cercava sul disco un file che non c'è
    /// (il controllo dei segni di conflitto non lo vedeva, e i segni potevano finire nel commit).
    /// </summary>
    public static class GitPaths
    {
        /// <summary>
        /// Da mettere davanti agli argomenti di ogni git che MDE lancia: i nomi con lettere accentate escono in chiaro. Git cita ancora
        /// i nomi con virgolette, barre rovesciate o caratteri di controllo: per quelli c'è <see cref="Unquote"/>.
        /// </summary>
        public static readonly IReadOnlyList<string> QuotePathOff = new[] { "-c", "core.quotePath=false" };

        /// <summary>Lo stesso, per chi passa gli argomenti in una stringa sola.</summary>
        public const string QuotePathOffText = "-c core.quotePath=false";

        /// <summary>
        /// Un percorso come lo scrive git, in chiaro: senza virgolette e con le sequenze del C decodificate (<c>\"</c>, <c>\\</c>,
        /// <c>\t</c>, <c>\n</c>…, e gli ottali <c>\303\251</c>, che sono i byte UTF-8 di una lettera). Un percorso senza virgolette
        /// torna com'è (solo senza spazi ai lati).
        /// </summary>
        public static string Unquote(string path)
        {
            var p = (path ?? string.Empty).Trim();
            if (p.Length < 2 || p[0] != '"' || p[^1] != '"') return p;

            var bytes = new List<byte>(p.Length);
            for (var i = 1; i < p.Length - 1; i++)
            {
                var c = p[i];
                if (c != '\\' || i + 1 >= p.Length - 1)
                {
                    bytes.AddRange(Encoding.UTF8.GetBytes(c.ToString()));
                    continue;
                }
                var n = p[++i];
                if (n >= '0' && n <= '7' && i + 2 < p.Length - 1 && IsOctal(p[i + 1]) && IsOctal(p[i + 2]))
                {
                    bytes.Add((byte)((n - '0') * 64 + (p[i + 1] - '0') * 8 + (p[i + 2] - '0')));
                    i += 2;
                    continue;
                }
                bytes.Add(n switch
                {
                    'a' => (byte)7, 'b' => (byte)8, 't' => (byte)'\t', 'n' => (byte)'\n', 'v' => (byte)11, 'f' => (byte)12, 'r' => (byte)'\r',
                    _ => (byte)n,   // \" e \\ (e qualunque altro carattere citato) valgono sé stessi
                });
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        private static bool IsOctal(char c) => c >= '0' && c <= '7';
    }
}
