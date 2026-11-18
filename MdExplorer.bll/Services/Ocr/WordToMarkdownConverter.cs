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

        /// <summary>Gli allegati incorporati nel Word usciti come file accanto alle immagini.</summary>
        public int Attachments { get; set; }
    }

    /// <summary>
    /// Word → markdown senza OCR: in un .docx il testo c'è già, leggerlo è esatto.
    /// <para>
    /// Due tappe, le stesse di MarkItDown di Microsoft: Mammoth porta il documento in HTML seguendo
    /// gli stili (Titolo 1 → h1, elenchi, tabelle), ReverseMarkdown porta l'HTML in markdown. Le
    /// immagini escono come file nella cartella indicata, non come dati dentro il markdown; gli allegati
    /// incorporati (PDF, Excel, altri Word) pure, con un collegamento al posto dell'icona. L'indice
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
            return Convert(docxPath, imagesDirectory, imagesLinkPrefix, MetafileToPng.Convert);
        }

        /// <param name="metafileToPng">Chi rende un EMF/WMF come PNG (byte, estensione, file da scrivere): null se ci è riuscito, altrimenti perché no.</param>
        internal static WordToMarkdownResult Convert(string docxPath, string imagesDirectory, string imagesLinkPrefix,
            Func<byte[], string, string, string> metafileToPng)
        {
            if (!File.Exists(docxPath))
            {
                throw new FileNotFoundException(
                    UiText.Say($"Il documento Word non c'è: {docxPath}", $"The Word document does not exist: {docxPath}"), docxPath);
            }

            var result = new WordToMarkdownResult();
            var links = new WordLinkRepair(result.Warnings);
            var metafilesLeft = 0;
            string whyMetafilesLeft = null;
            var converter = new Mammoth.DocumentConverter().AddStyleMap(WordLinkRepair.StyleMap).ImageConverter(image =>
            {
                Directory.CreateDirectory(imagesDirectory);
                result.Images++;
                var extension = ExtensionOf(image.ContentType);
                byte[] bytes;
                using (var source = image.GetStream())
                using (var buffer = new MemoryStream())
                {
                    source.CopyTo(buffer);
                    bytes = buffer.ToArray();
                }

                // I disegni vettoriali di Windows non si vedono in una pagina: escono come PNG, se si riesce.
                var fileName = $"img{result.Images}{extension}";
                if (extension == ".emf" || extension == ".wmf")
                {
                    var png = $"img{result.Images}.png";
                    var whyNot = metafileToPng(bytes, extension, Path.Combine(imagesDirectory, png));
                    if (whyNot == null)
                    {
                        fileName = png;
                        bytes = null;
                    }
                    else
                    {
                        metafilesLeft++;
                        whyMetafilesLeft = whyNot;
                    }
                }
                if (bytes != null)
                {
                    File.WriteAllBytes(Path.Combine(imagesDirectory, fileName), bytes);
                }
                var attributes = new Dictionary<string, string> { ["src"] = imagesLinkPrefix + "/" + fileName };
                if (!string.IsNullOrEmpty(image.AltText))
                {
                    // Su una riga sola: il testo alternativo che Word genera da solo va a capo, e in markdown un'immagine
                    // con il testo su più righe non la riconosce nessuno (nemmeno Mark, che non la descriveva).
                    attributes["alt"] = System.Text.RegularExpressions.Regex.Replace(image.AltText.Trim(), @"\s+", " ");
                }
                return attributes;
            });

            // Due passate sul docx prima di Mammoth, ognuna su una copia: la numerazione che lo fa cadere
            // (WordNumberingRepair) e gli allegati incorporati, che escono come file (WordAttachments).
            var attachments = new WordAttachments();
            Mammoth.IResult<string> html;
            using (var repaired = WordNumberingRepair.RepairedCopy(docxPath))
            using (var original = repaired == null ? File.OpenRead(docxPath) : null)
            using (var withoutAttachments = attachments.Extract((Stream)repaired ?? original, imagesDirectory, imagesLinkPrefix, result.Warnings))
            {
                if (withoutAttachments != null)
                {
                    html = converter.ConvertToHtml(withoutAttachments);
                }
                else if (repaired != null)
                {
                    repaired.Position = 0;
                    html = converter.ConvertToHtml(repaired);
                }
                else
                {
                    html = converter.ConvertToHtml(docxPath);
                }
            }
            result.Attachments = attachments.Count;
            foreach (var warning in html.Warnings)
            {
                // Di EMF e WMF risponde l'avviso qui sotto, che conta solo quelli rimasti tali.
                if (warning.IndexOf("unlikely to display in web browsers", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    result.Warnings.Add(warning);
                }
            }
            if (metafilesLeft > 0)
            {
                result.Warnings.Add(UiText.Say(
                    $"{metafilesLeft} immagini in formato EMF o WMF sono rimaste in quel formato, che nella pagina non si vede: {whyMetafilesLeft}.",
                    $"{metafilesLeft} images in EMF or WMF format stayed in that format, which a page cannot show: {whyMetafilesLeft}."));
            }

            // Segnalibri e indice: i rimandi di Word vanno portati sulle ancore dei titoli (WordLinkRepair).
            // Prima le tabelle che in markdown non reggono (WordTableCards): i titoli delle schede sono titoli come gli altri.
            var cleanHtml = links.PrepareHtml(WordTableCards.Apply(html.Value, result.Warnings));
            var markdown = new ReverseMarkdown.Converter(new ReverseMarkdown.Config
            {
                GithubFlavored = true,
                RemoveComments = true
            }).Convert(cleanHtml);

            result.Markdown = attachments.FinishMarkdown(links.FinishMarkdown(markdown.Replace("\r\n", "\n"))).Trim() + "\n";
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
