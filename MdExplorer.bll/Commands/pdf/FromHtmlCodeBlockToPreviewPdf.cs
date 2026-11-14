using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Services.Html;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands.pdf
{
    /// <summary>
    /// <c>```html(file)</c> nell'esportazione Word/PDF (C8 della valutazione del 07/11): la foto della pagina disegnata,
    /// come la scheda Preview che MDE mostra per prima, invece del blocco vuoto che arrivava a pandoc. La foto la fa un
    /// browser già installato (<see cref="HtmlSnapshot"/>) e sta in <c>.md/</c> col nome = impronta del contenuto: se
    /// l'HTML non cambia non si rifà (come per i diagrammi; i file che la pagina carica, css e immagini, non entrano
    /// nell'impronta). Senza browser, o se la pagina non si disegna, una riga che lo dice e dove sta il file. Un blocco
    /// <c>```html</c> SENZA file resta codice, come prima: chi documenta pezzi di HTML lo vuole leggere.
    /// </summary>
    public class FromHtmlCodeBlockToPreviewPdf : FromHtmlCodeBlockToPreview, ICommandPdf
    {
        public FromHtmlCodeBlockToPreviewPdf(ILogger<FromHtmlCodeBlockToPreview> logger, IHelper helper)
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
                var declared = match.Groups[1].Value.Trim();
                if (declared.Length == 0 || regions.IsCode(match.Index)) continue;
                string replacement;
                try
                {
                    replacement = Snapshot(declared, requestInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[FromHtmlCodeBlockToPreviewPdf] {File}", declared);
                    replacement = Note(declared, ex.Message);
                }
                (markdown, increment) = ManageReplaceOnMD(markdown, increment, match, replacement);
            }
            return markdown;
        }

        private string Snapshot(string declared, RequestInfo requestInfo)
        {
            var content = ExternalFileResolver.ReadInsideProject(declared, requestInfo, 512_000, out var absolute, out var error);
            if (content == null) return Note(declared, error);

            var name = "html-" + Hash(content + "|" + HtmlSnapshot.Width + "x" + HtmlSnapshot.Height) + ".png";
            var png = Path.Combine(requestInfo.CurrentRoot, ".md", name);
            if (!File.Exists(png))
            {
                var failed = HtmlSnapshot.RenderAsync(absolute, png).GetAwaiter().GetResult();
                if (failed != null) return Note(declared, failed);
                _logger.LogInformation("[FromHtmlCodeBlockToPreviewPdf] {File} → {Png}", declared, png);
            }
            // Come i diagrammi (FromPlantumlToSvgPdf): pandoc parte dalla cartella del progetto. Solo la larghezza: l'altezza
            // segue la proporzione della pagina.
            return $"\n\n![{Path.GetFileName(declared)}](.md/{name}){{width=\"6.3in\"}}\n\n";
        }

        private static string Note(string declared, string why)
            => "\n\n> " + MdExplorer.Features.Services.UiText.Say($"Anteprima HTML: vedi `{declared}` ({why})", $"HTML preview: see `{declared}` ({why})") + "\n\n";

        private static string Hash(string text)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Substring(0, 16).ToLowerInvariant();
        }
    }
}
