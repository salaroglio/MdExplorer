using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Services;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands.pdf
{
    /// <summary>
    /// <c>```text(percorso)</c> nell'esportazione Word/PDF (C8 della valutazione del 07/11). Prima il comando c'era solo
    /// per la pagina, e a pandoc arrivava un blocco vuoto con scritto <c>text(percorso)</c>: nel documento il file non c'era.
    /// Qui diventa un normale blocco di codice con il contenuto del file e, sopra, il suo nome; un file che non si legge
    /// diventa una riga che lo dice, mai un vuoto.
    /// </summary>
    public class FromTextCodeBlockToPreviewPdf : FromTextCodeBlockToPreview, ICommandPdf
    {
        public FromTextCodeBlockToPreviewPdf(ILogger<FromTextCodeBlockToPreview> logger, IHelper helper)
            : base(logger, helper)
        {
        }

        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var matches = GetMatches(markdown);
            if (matches.Count == 0) return markdown;
            var regions = MarkdownCodeRegions.Of(markdown);
            var increment = 0;
            foreach (Match match in matches)
            {
                if (regions.IsCode(match.Index)) continue;   // un esempio dentro un blocco più lungo resta testo
                var declared = match.Groups[1].Value.Trim();
                string replacement;
                try
                {
                    var content = ExternalFileResolver.ReadInsideProject(declared, requestInfo, TextFileView.MaxBytes, out var absolute, out var error);
                    replacement = content == null
                        ? $"\n\n> ⚠️ `text({declared})`: {error}\n\n"
                        : Block(declared, TextFileView.LanguageFor(absolute), content);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FromTextCodeBlockToPreviewPdf] {File}", declared);
                    replacement = $"\n\n> ⚠️ `text({declared})`: {ex.Message}\n\n";
                }
                (markdown, increment) = ManageReplaceOnMD(markdown, increment, match, replacement);
            }
            return markdown;
        }

        /// <summary>Il nome del file e il suo contenuto, in un recinto più lungo di ogni fila di backtick del contenuto.</summary>
        internal static string Block(string declared, string language, string content)
        {
            var longest = Regex.Matches(content, "`+").Cast<Match>().Select(m => m.Length).DefaultIfEmpty(0).Max();
            var fence = new string('`', Math.Max(3, longest + 1));
            var body = content.Replace("\r\n", "\n").TrimEnd('\n');
            return $"\n\n**{Path.GetFileName(declared)}** — `{declared}`\n\n{fence}{language}\n{body}\n{fence}\n\n";
        }
    }
}
