using System;
using System.Collections.Generic;
using System.IO;

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
    /// immagini escono come file nella cartella indicata, non come dati dentro il markdown. L'indice
    /// e i rimandi interni restano attivi: puntano alle ancore dei titoli.
    /// </para>
    /// </summary>
    public static class WordToMarkdownConverter
    {
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
            var links = new WordLinkRepair(result.Warnings);
            var converter = new Mammoth.DocumentConverter().AddStyleMap(WordLinkRepair.StyleMap).ImageConverter(image =>
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

            // Segnalibri e indice: i rimandi di Word vanno portati sulle ancore dei titoli (WordLinkRepair).
            var cleanHtml = links.PrepareHtml(html.Value);
            var markdown = new ReverseMarkdown.Converter(new ReverseMarkdown.Config
            {
                GithubFlavored = true,
                RemoveComments = true
            }).Convert(cleanHtml);

            result.Markdown = links.FinishMarkdown(markdown.Replace("\r\n", "\n")).Trim() + "\n";
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
