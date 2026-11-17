using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
#if WINDOWS
using System.Drawing;
using System.Drawing.Imaging;
#endif

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Un'immagine EMF o WMF (i disegni vettoriali di Windows: schemi incollati da Visio o Excel, anteprime di
    /// oggetti incorporati) resa come PNG. I browser non mostrano quei formati e i motori AI non li aprono:
    /// lasciata com'è, l'immagine nel documento convertito non si vede.
    /// <para>
    /// Su Windows la disegna Windows (GDI+), che è chi quel formato lo ha inventato. Altrove la converte
    /// LibreOffice, se è installato; se non c'è, lo si dice: nessun ripiego silenzioso.
    /// </para>
    /// </summary>
    public static class MetafileToPng
    {
        // Il doppio della misura a schermo: testi e linee restano leggibili, anche per chi li deve descrivere.
        private const float Scale = 2f;
        private const int MaxSide = 4000;
        private const int LibreOfficeTimeoutMs = 90_000;

        /// <param name="metafile">I byte dell'immagine come stanno nel Word.</param>
        /// <param name="extension">«.emf» o «.wmf».</param>
        /// <returns>null se <paramref name="pngPath"/> è stato scritto; altrimenti perché non lo è stato.</returns>
        public static string Convert(byte[] metafile, string extension, string pngPath)
        {
            // LibreOffice «converte» qualunque cosa, anche quattro byte a caso (li prende per testo): si guarda prima.
            if (!LooksLikeMetafile(metafile))
            {
                return UiText.Say("il file non è un EMF o un WMF valido", "the file is not a valid EMF or WMF");
            }
            if (OperatingSystem.IsWindows())
            {
#if WINDOWS
                try
                {
                    RenderWithWindows(metafile, pngPath);
                    return null;
                }
                catch (Exception ex) when (ex is System.Runtime.InteropServices.ExternalException || ex is ArgumentException || ex is OutOfMemoryException)
                {
                    // GDI+ dice così «non è un'immagine che so leggere».
                    return UiText.Say($"Windows non è riuscito a disegnarla ({ex.Message})", $"Windows could not draw it ({ex.Message})");
                }
#else
                return UiText.Say("questo MdExplorer è stato compilato senza la grafica di Windows", "this MdExplorer was built without the Windows graphics");
#endif
            }
            return ConvertWithLibreOffice(metafile, extension, pngPath);
        }

#if WINDOWS
        [SupportedOSPlatform("windows")]
        private static void RenderWithWindows(byte[] metafile, string pngPath)
        {
            using (var stream = new MemoryStream(metafile))
            using (var image = new Metafile(stream))
            {
                var scale = Math.Min(Scale, MaxSide / (float)Math.Max(1, Math.Max(image.Width, image.Height)));
                var width = Math.Max(1, (int)Math.Ceiling(image.Width * scale));
                var height = Math.Max(1, (int)Math.Ceiling(image.Height * scale));
                using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.Clear(Color.White);
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        graphics.DrawImage(image, new Rectangle(0, 0, width, height));
                    }
                    bitmap.Save(pngPath, ImageFormat.Png);
                }
            }
        }
#endif

        private static string ConvertWithLibreOffice(byte[] metafile, string extension, string pngPath)
        {
            var soffice = FindInPath("soffice") ?? FindInPath("libreoffice");
            if (soffice == null)
            {
                return UiText.Say(
                    "su questo sistema le converte LibreOffice, che non è installato",
                    "on this system LibreOffice converts them, and it is not installed");
            }

            var work = Path.Combine(Path.GetTempPath(), "mde-metafile-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(work);
            try
            {
                var source = Path.Combine(work, "image" + extension);
                File.WriteAllBytes(source, metafile);

                var start = new ProcessStartInfo(soffice)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                // Un profilo suo: con il LibreOffice dell'utente aperto, quello di sempre è occupato e la conversione non parte.
                start.ArgumentList.Add("-env:UserInstallation=" + new Uri(Path.Combine(work, "profile")).AbsoluteUri);
                start.ArgumentList.Add("--headless");
                start.ArgumentList.Add("--convert-to");
                start.ArgumentList.Add("png");
                start.ArgumentList.Add("--outdir");
                start.ArgumentList.Add(work);
                start.ArgumentList.Add(source);

                using (var process = Process.Start(start))
                {
                    var stderr = process.StandardError.ReadToEndAsync();
                    process.StandardOutput.ReadToEndAsync();
                    if (!process.WaitForExit(LibreOfficeTimeoutMs))
                    {
                        try { process.Kill(true); } catch (InvalidOperationException) { /* è finito proprio adesso */ }
                        return UiText.Say("LibreOffice non ha finito in tempo", "LibreOffice did not finish in time");
                    }

                    var produced = Path.Combine(work, "image.png");
                    if (!File.Exists(produced))
                    {
                        var reason = (stderr.Result ?? string.Empty).Split('\n').Select(line => line.Trim()).FirstOrDefault(line => line.Length > 0);
                        return UiText.Say(
                            $"LibreOffice non l'ha convertita (uscita {process.ExitCode}{(reason == null ? string.Empty : ": " + reason)})",
                            $"LibreOffice did not convert it (exit {process.ExitCode}{(reason == null ? string.Empty : ": " + reason)})");
                    }
                    File.Move(produced, pngPath, true);
                    return null;
                }
            }
            finally
            {
                try { Directory.Delete(work, true); } catch (IOException) { /* residuo nella cartella temporanea */ }
            }
        }

        // EMF: comincia con il record d'intestazione (1) e porta la firma « EMF» al byte 40.
        // WMF: l'intestazione «posizionabile» D7 CD C6 9A, oppure quella semplice (tipo 1 o 2, lunghezza 9 parole).
        private static bool LooksLikeMetafile(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 18)
            {
                return false;
            }
            var emf = bytes.Length >= 44 && bytes[0] == 1 && bytes[1] == 0 && bytes[2] == 0 && bytes[3] == 0
                && bytes[40] == 0x20 && bytes[41] == 0x45 && bytes[42] == 0x4D && bytes[43] == 0x46;
            var placeableWmf = bytes[0] == 0xD7 && bytes[1] == 0xCD && bytes[2] == 0xC6 && bytes[3] == 0x9A;
            var plainWmf = (bytes[0] == 1 || bytes[0] == 2) && bytes[1] == 0 && bytes[2] == 9 && bytes[3] == 0;
            return emf || placeableWmf || plainWmf;
        }

        private static string FindInPath(string program)
        {
            return (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(folder => Path.Combine(folder, program))
                .FirstOrDefault(File.Exists);
        }
    }
}
