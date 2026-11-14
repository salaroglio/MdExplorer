using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using PuppeteerSharp;

namespace MdExplorer.Features.Services.Html
{
    /// <summary>
    /// La foto di una pagina HTML com'è disegnata, per l'esportazione Word/PDF di <c>```html(file)</c> (C8 della valutazione
    /// del 07/11: nella pagina di MDE si vede la scheda Preview, nel documento esportato non c'era niente). .NET non ha un
    /// motore che disegni l'HTML: PuppeteerSharp pilota un browser GIÀ installato (Edge, che su Windows 11 c'è sempre, o
    /// Chrome/Chromium), nascosto, con la grandezza dello schermo fissata; non si scarica nessun browser.
    /// <para>
    /// Il browser parte con una cartella del profilo sua e temporanea: con quella dell'utente, un Edge già aperto
    /// prenderebbe la richiesta al posto di quello nascosto. Gli script della pagina girano (come nell'anteprima), l'accesso
    /// da un file agli altri file del disco no (è il comportamento di Chromium per le pagine <c>file://</c>).
    /// </para>
    /// </summary>
    public static class HtmlSnapshot
    {
        public const int Width = 1280;
        public const int Height = 800;
        private static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);

        /// <summary>Per provare un browser diverso: il percorso dell'eseguibile.</summary>
        public const string BrowserVariable = "MDE_HTML_BROWSER";

        /// <summary>Il browser da usare, o null se non ce n'è.</summary>
        public static string FindBrowser()
        {
            var chosen = Environment.GetEnvironmentVariable(BrowserVariable);
            if (!string.IsNullOrWhiteSpace(chosen)) return File.Exists(chosen) ? chosen : null;
            return Candidates().FirstOrDefault(File.Exists);
        }

        private static IEnumerable<string> Candidates()
        {
            if (OperatingSystem.IsWindows())
            {
                foreach (var root in new[] { "ProgramFiles(x86)", "ProgramFiles", "LocalAppData" }.Select(Environment.GetEnvironmentVariable).Where(r => !string.IsNullOrEmpty(r)))
                {
                    yield return Path.Combine(root, "Microsoft", "Edge", "Application", "msedge.exe");
                    yield return Path.Combine(root, "Google", "Chrome", "Application", "chrome.exe");
                }
                yield break;
            }
            if (OperatingSystem.IsMacOS())
            {
                yield return "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge";
                yield return "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";
                yield return "/Applications/Chromium.app/Contents/MacOS/Chromium";
                yield break;
            }
            var path = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var name in new[] { "google-chrome", "google-chrome-stable", "chromium", "chromium-browser", "microsoft-edge", "microsoft-edge-stable" })
                foreach (var dir in path)
                    yield return Path.Combine(dir, name);
        }

        /// <summary>Disegna <paramref name="htmlFile"/> e scrive la foto della pagina intera in <paramref name="pngFile"/>; null se è andata, altrimenti il motivo.</summary>
        public static async Task<string> RenderAsync(string htmlFile, string pngFile, CancellationToken ct = default)
        {
            var browserPath = FindBrowser();
            if (browserPath == null)
                return UiText.Say("nessun browser trovato (Edge o Chrome)", "no browser found (Edge or Chrome)");

            var profile = Path.Combine(Path.GetTempPath(), "mde-html-snapshot-" + Guid.NewGuid().ToString("N"));
            try
            {
                using var limit = CancellationTokenSource.CreateLinkedTokenSource(ct);
                limit.CancelAfter(Limit);
                await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    ExecutablePath = browserPath,
                    Headless = true,
                    UserDataDir = profile,
                    Timeout = (int)Limit.TotalMilliseconds,
                    Args = new[] { "--no-first-run", "--no-default-browser-check", "--hide-scrollbars", "--disable-extensions" },
                }).WaitAsync(limit.Token).ConfigureAwait(false);
                await using var page = await browser.NewPageAsync().WaitAsync(limit.Token).ConfigureAwait(false);
                await page.SetViewportAsync(new ViewPortOptions { Width = Width, Height = Height }).WaitAsync(limit.Token).ConfigureAwait(false);
                await page.GoToAsync(new Uri(Path.GetFullPath(htmlFile)).AbsoluteUri, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
                    Timeout = (int)Limit.TotalMilliseconds,
                }).WaitAsync(limit.Token).ConfigureAwait(false);
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(pngFile)));
                await page.ScreenshotAsync(pngFile, new ScreenshotOptions { FullPage = true, Type = ScreenshotType.Png }).WaitAsync(limit.Token).ConfigureAwait(false);
                return null;
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                return UiText.Say($"il browser non ha finito in {Limit.TotalSeconds:N0} secondi", $"the browser did not finish within {Limit.TotalSeconds:N0} seconds");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return UiText.Say("il browser non ha potuto disegnare la pagina: ", "the browser could not draw the page: ") + ex.Message;
            }
            finally
            {
                try { if (Directory.Exists(profile)) Directory.Delete(profile, true); } catch { /* il browser può tenere un file per un attimo */ }
            }
        }
    }
}
