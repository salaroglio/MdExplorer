using System;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace MdExplorer.Features.Exports
{
    /// <summary>
    /// Mette al centro le immagini di un documento Word appena prodotto da Pandoc.
    /// <para>
    /// <b>Perché serve.</b> Pandoc scrive l'immagine dentro un paragrafo normale e <b>non</b> ci
    /// mette nessun allineamento: il paragrafo eredita quello dello stile del corpo del testo, che
    /// è a sinistra. Misurato il 21/09/2026 con Pandoc 2.9.2.1 e il <c>reference.docx</c> di
    /// MdExplorer:
    /// <list type="bullet">
    /// <item><description>immagine con didascalia → paragrafo di stile <c>CaptionedFigure</c>
    /// (solo immagine) seguito da uno di stile <c>ImageCaption</c> (il testo della didascalia);</description></item>
    /// <item><description>immagine senza didascalia → paragrafo di stile <c>Corpotesto</c>, con dentro
    /// la sola immagine;</description></item>
    /// <item><description>immagine in mezzo a una frase → stesso paragrafo del testo;</description></item>
    /// <item><description>immagine in una cella di tabella → paragrafo <c>Compact</c> con il suo
    /// <c>jc</c> già scritto.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Da qui la regola: si centra un paragrafo che contiene un'immagine <b>e nient'altro che
    /// spazi</b> — così l'immagine dentro una frase resta dov'è, in mezzo al suo testo. La
    /// didascalia che segue va al centro con la sua immagine, altrimenti resterebbe spaiata a
    /// sinistra. Le celle di tabella si lasciano stare: lì l'allineamento è già deciso, e la
    /// larghezza della colonna è un'altra storia.
    /// </para>
    /// </summary>
    public static class WordImageLayout
    {
        /// <summary>Stile che Pandoc dà alla didascalia di una figura.</summary>
        private const string CaptionStyle = "ImageCaption";

        /// <summary>
        /// Centra le immagini "da sole" del documento.
        /// </summary>
        /// <param name="docxPath">Percorso del .docx da correggere, in scrittura.</param>
        /// <returns>Quante immagini sono state centrate.</returns>
        /// <exception cref="ArgumentException">Percorso vuoto.</exception>
        public static int CenterStandaloneImages(string docxPath)
        {
            if (string.IsNullOrWhiteSpace(docxPath))
                throw new ArgumentException("Serve il percorso del documento Word.", nameof(docxPath));

            using var document = WordprocessingDocument.Open(docxPath, true);
            var body = document.MainDocumentPart?.Document?.Body;
            if (body == null) return 0;

            var centrate = 0;
            foreach (var paragraph in body.Descendants<Paragraph>().ToList())
            {
                if (!IsStandaloneImage(paragraph)) continue;

                Center(paragraph);
                centrate++;

                // La didascalia sta nel paragrafo subito dopo: va dove va la sua immagine.
                if (paragraph.NextSibling<Paragraph>() is Paragraph next && HasStyle(next, CaptionStyle))
                {
                    Center(next);
                }
            }

            if (centrate > 0)
            {
                document.MainDocumentPart.Document.Save();
            }
            return centrate;
        }

        /// <summary>
        /// Il paragrafo è fatto di una (o più) immagini e basta? Il testo lo esclude: sarebbe
        /// un'immagine dentro una frase, e centrarla sposterebbe la frase.
        /// </summary>
        private static bool IsStandaloneImage(Paragraph paragraph)
        {
            // Dentro una tabella non si tocca: l'allineamento della cella è già scritto.
            if (paragraph.Ancestors<Table>().Any()) return false;

            if (!paragraph.Descendants<Drawing>().Any()) return false;

            return paragraph.Descendants<Text>().All(t => string.IsNullOrWhiteSpace(t.Text));
        }

        private static bool HasStyle(Paragraph paragraph, string styleId)
            => paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value == styleId;

        /// <summary>
        /// Scrive <c>jc=center</c> nel paragrafo. La proprietà tipizzata dell'SDK infila
        /// l'elemento al posto che lo schema di OpenXML pretende: aggiungerlo in coda a mano
        /// produrrebbe un documento che Word rifiuta di aprire.
        /// </summary>
        private static void Center(Paragraph paragraph)
        {
            var properties = paragraph.ParagraphProperties;
            if (properties == null)
            {
                properties = new ParagraphProperties();
                paragraph.PrependChild(properties);
            }
            properties.Justification = new Justification { Val = JustificationValues.Center };
        }
    }
}
