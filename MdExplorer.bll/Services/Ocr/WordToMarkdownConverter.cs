using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>Esito della conversione di un Word: il markdown e ciò che il convertitore non ha capito.</summary>
    public class WordToMarkdownResult
    {
        public string Markdown { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public int Images { get; set; }
    }

    /// <summary>
    /// Word → markdown senza OCR: in un .docx il testo c'è già, leggerlo è esatto.
    /// <para>
    /// Due tappe, le stesse di MarkItDown di Microsoft: Mammoth porta il documento in HTML seguendo
    /// gli stili (Titolo 1 → h1, elenchi, tabelle), ReverseMarkdown porta l'HTML in markdown. Le
    /// immagini escono come file nella cartella indicata, non come dati dentro il markdown.
    /// </para>
    /// </summary>
    public static class WordToMarkdownConverter
    {
        // Ogni titolo di Word porta un segnalibro: in HTML è un'ancora vuota, in markdown diventerebbe «[]()».
        private static readonly Regex EmptyAnchor =
            new Regex(@"<a\s+id=""[^""]*""\s*>\s*</a>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <param name="docxPath">Il documento Word.</param>
        /// <param name="imagesDirectory">Dove scrivere le immagini del documento; creata solo se ce ne sono.</param>
        /// <param name="imagesLinkPrefix">Come il markdown indirizza quella cartella (termina senza «/»).</param>
        public static WordToMarkdownResult Convert(string docxPath, string imagesDirectory, string imagesLinkPrefix)
        {
            if (!File.Exists(docxPath))
            {
                throw new FileNotFoundException(
                    UiText.Say($"Il documento Word non c'è: {docxPath}", $"The Word document does not exist: {docxPath}"), docxPath);
            }

            var result = new WordToMarkdownResult();
            var converter = new Mammoth.DocumentConverter().ImageConverter(image =>
            {
                Directory.CreateDirectory(imagesDirectory);
                result.Images++;
                var fileName = $"img{result.Images}{ExtensionOf(image.ContentType)}";
                using (var source = image.GetStream())
                using (var target = File.Create(Path.Combine(imagesDirectory, fileName)))
                {
                    source.CopyTo(target);
                }
                var attributes = new Dictionary<string, string> { ["src"] = imagesLinkPrefix + "/" + fileName };
                if (!string.IsNullOrEmpty(image.AltText))
                {
                    attributes["alt"] = image.AltText;
                }
                return attributes;
            });

            Mammoth.IResult<string> html;
            using (var repaired = WordNumberingRepair.RepairedCopy(docxPath))
            {
                html = repaired == null ? converter.ConvertToHtml(docxPath) : converter.ConvertToHtml(repaired);
            }
            foreach (var warning in html.Warnings)
            {
                result.Warnings.Add(warning);
            }

            var cleanHtml = EmptyAnchor.Replace(html.Value, string.Empty);
            var markdown = new ReverseMarkdown.Converter(new ReverseMarkdown.Config
            {
                GithubFlavored = true,
                RemoveComments = true
            }).Convert(cleanHtml);

            result.Markdown = markdown.Replace("\r\n", "\n").Trim() + "\n";
            return result;
        }

        private static string ExtensionOf(string contentType)
        {
            switch ((contentType ?? string.Empty).ToLowerInvariant())
            {
                case "image/png": return ".png";
                case "image/jpeg": return ".jpg";
                case "image/gif": return ".gif";
                case "image/bmp": return ".bmp";
                case "image/tiff": return ".tiff";
                case "image/svg+xml": return ".svg";
                case "image/x-emf": return ".emf";
                case "image/x-wmf": return ".wmf";
                default:
                    throw new NotSupportedException(UiText.Say(
                        $"Il Word contiene un'immagine di un tipo che MdExplorer non sa salvare: «{contentType}».",
                        $"The Word document contains an image of a type MdExplorer cannot save: \"{contentType}\"."));
            }
        }
    }
}
