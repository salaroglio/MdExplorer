using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Le pagine scritte dentro un file di un altro linguaggio (25/10/2026, utente: «gestisci anche i template inline»):
    /// Angular <c>template: `…`</c> nel .ts. Il dialetto che le legge lo dice con <c>template.inline</c>. Una pagina così
    /// è un file «virtuale»: stesso percorso assoluto del file che la contiene (l'hash, git e la freschezza sono i suoi),
    /// percorso logico con <see cref="Suffix"/>, uno script suo nel linguaggio del dialetto. Il testo è quello del file con
    /// tutto ciò che non è pagina sostituito da spazi: le righe restano quelle del file.
    /// </summary>
    public static class CodeInlinePages
    {
        public const string Suffix = "#template";

        public static bool IsInline(string logicalPath) => logicalPath != null && logicalPath.EndsWith(Suffix, StringComparison.Ordinal);

        /// <summary>Il file che contiene la pagina (o il percorso stesso, se non è una pagina dentro un file).</summary>
        public static string HostOf(string path) => IsInline(path) ? path[..^Suffix.Length] : path;

        public static string PathOf(string hostLogicalPath) => hostLogicalPath + Suffix;

        /// <summary>
        /// I file più le pagine che contengono: per ogni file con un'estensione che un dialetto legge «inline», e nel cui
        /// testo il dialetto trova almeno una pagina, un file virtuale in più. Senza dialetti inline, gli stessi file.
        /// </summary>
        public static List<CodeSourceFile> Expand(CodeLanguageCatalog catalog, IEnumerable<CodeSourceFile> files, IReadOnlyDictionary<string, string> encodings = null)
        {
            var list = files.ToList();
            if (!catalog.All.Any(l => l.Definition?.Template?.Inline != null)) return list;
            var result = new List<CodeSourceFile>(list);
            foreach (var f in list)
            {
                if (IsInline(f.LogicalPath)) continue;
                var dialect = catalog.InlineFor(Path.GetExtension(f.AbsolutePath));
                if (dialect == null) continue;
                string text;
                try { text = CodeSourceText.Read(f.AbsolutePath, encodings); }
                catch (IOException) { continue; }   // non leggibile: lo dice il suo estrattore
                if (dialect.Definition.Template.Inline.Match.Matches(text).Any(m => m.Groups["body"].Success && m.Groups["body"].Length > 0))
                    result.Add(new CodeSourceFile(f.AbsolutePath, PathOf(f.LogicalPath)));
            }
            return result;
        }

        /// <summary>Il testo del file con le sole pagine (gruppi <c>body</c>): il resto diventa spazi, gli a capo restano.</summary>
        public static string Blank(string text, Regex match)
        {
            var keep = new bool[text.Length];
            foreach (Match m in match.Matches(text))
            {
                var body = m.Groups["body"];
                if (!body.Success) continue;
                for (var i = body.Index; i < body.Index + body.Length; i++) keep[i] = true;
            }
            var sb = new StringBuilder(text.Length);
            for (var i = 0; i < text.Length; i++) sb.Append(keep[i] || text[i] is '\n' or '\r' ? text[i] : ' ');
            return sb.ToString();
        }
    }
}
