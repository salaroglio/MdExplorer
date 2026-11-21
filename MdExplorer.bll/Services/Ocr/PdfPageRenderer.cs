using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Le pagine di un pdf come immagini PNG, una alla volta: il modello OCR legge immagini, non pdf.
    /// <para>
    /// 150 dpi è la risoluzione a cui il modello è stato provato (15/11/2026): un A4 esce 1240×1754,
    /// il testo a 10 pt resta leggibile e la pagina costa ~2150 token di immagine.
    /// </para>
    /// </summary>
    public static class PdfPageRenderer
    {
        public const int Dpi = 150;

        /// <summary>
        /// Il lato lungo massimo che il modello accetta (dalla sua guida): un A4 a 150 dpi sta sotto,
        /// un A3 o una tavola no, e va rimpicciolito prima di darglielo.
        /// </summary>
        public const int MaxLongEdge = 2048;

#pragma warning disable CA1416 // PDFium c'è per Windows, Linux e macOS: le piattaforme su cui gira il servizio.
        public static int CountPages(string pdfPath)
        {
            using (var stream = File.OpenRead(pdfPath))
            {
                return PDFtoImage.Conversion.GetPageCount(stream);
            }
        }

        /// <summary>
        /// Le pagine in ordine, ciascuna come PNG. Pigro: una pagina è in memoria solo mentre la si
        /// legge, così un pdf di 300 pagine non occupa 300 immagini.
        /// </summary>
        public static IEnumerable<byte[]> RenderPages(string pdfPath)
        {
            using (var stream = File.OpenRead(pdfPath))
            {
                foreach (var bitmap in PDFtoImage.Conversion.ToImages(stream, options: new PDFtoImage.RenderOptions(Dpi: Dpi)))
                {
                    using (bitmap)
                    {
                        var longEdge = Math.Max(bitmap.Width, bitmap.Height);
                        if (longEdge <= MaxLongEdge)
                        {
                            using (var encoded = bitmap.Encode(SKEncodedImageFormat.Png, 100))
                            {
                                yield return encoded.ToArray();
                            }
                            continue;
                        }

                        var scale = (double)MaxLongEdge / longEdge;
                        var size = new SKImageInfo((int)Math.Round(bitmap.Width * scale), (int)Math.Round(bitmap.Height * scale));
                        using (var smaller = bitmap.Resize(size, new SKSamplingOptions(SKCubicResampler.Mitchell)))
                        using (var encoded = smaller.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            yield return encoded.ToArray();
                        }
                    }
                }
            }
        }
#pragma warning restore CA1416
    }
}
