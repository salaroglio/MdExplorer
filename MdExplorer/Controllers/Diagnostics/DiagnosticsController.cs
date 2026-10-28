using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MdExplorer.Controllers.Diagnostics
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        /// <summary>
        /// Opens the Electron log file in the default text editor
        /// </summary>
        [HttpPost("OpenLog")]
        public IActionResult OpenLog()
        {
            try
            {
                // Get the log file path (same as Electron's app.getPath('userData'))
                // The folder name matches the "name" field in Electron's package.json: "mdexplorer"
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var logFilePath = Path.Combine(appData, "mdexplorer", "electron_main.log");

                if (!System.IO.File.Exists(logFilePath))
                {
                    return NotFound(new { error = $"Log file not found: {logFilePath}" });
                }

                // Open with default application
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = logFilePath,
                        UseShellExecute = true
                    });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", logFilePath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", logFilePath);
                }

                return Ok(new { success = true, path = logFilePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>GET api/Diagnostics/StartupNotices — gli avvisi di avvio (Java e simili) che la pagina dei progetti mostra finché non vengono chiusi.</summary>
        [HttpGet("StartupNotices")]
        public IActionResult StartupNotices([FromServices] MdExplorer.Services.Java.StartupNotices notices)
            => Ok(notices.All().Select(n => new { n.Id, n.Level, n.Kind, n.Params, n.At }));

        [HttpPost("StartupNotices/{id}/dismiss")]
        public IActionResult DismissNotice(string id, [FromServices] MdExplorer.Services.Java.StartupNotices notices)
            => notices.Dismiss(id) ? Ok(new { dismissed = id }) : NotFound(new { error = MdExplorer.Features.Services.UiText.Say($"avviso '{id}' non trovato", $"notice '{id}' not found") });

        /// <summary>GET api/Diagnostics/Java?path=… — la sonda di un java: versione maggiore, riga grezza, eseguibile o no. Senza path: il JavaPath impostato.</summary>
        [HttpGet("Java")]
        public IActionResult JavaProbe([FromQuery] string path, [FromServices] MdExplorer.Features.Utilities.IJavaLocator locator)
        {
            var target = string.IsNullOrWhiteSpace(path) ? locator.ResolveJavaPath() : path.Trim();
            var probe = locator.Probe(target);
            return Ok(new { path = target, probe.IsRunnable, probe.Major, probe.Raw, probe.Error, minimum = MdExplorer.Services.Java.JavaHealthService.MinimumJavaMajor, suitable = probe.IsRunnable && probe.Major >= MdExplorer.Services.Java.JavaHealthService.MinimumJavaMajor });
        }

        /// <summary>GET api/Diagnostics/JavaBest — il miglior Java del computer (≥ 21) fra tutti i candidati, o 404 se non c'è. Sonda tutto: qualche secondo.</summary>
        [HttpGet("JavaBest")]
        public IActionResult JavaBest([FromServices] MdExplorer.Features.Utilities.IJavaLocator locator)
        {
            var best = locator.FindBest(MdExplorer.Services.Java.JavaHealthService.MinimumJavaMajor);
            if (best == null) return NotFound(new { error = MdExplorer.Features.Services.UiText.Say($"Nessun Java {MdExplorer.Services.Java.JavaHealthService.MinimumJavaMajor} trovato sul computer", $"No Java {MdExplorer.Services.Java.JavaHealthService.MinimumJavaMajor} found on the computer"), candidates = MdExplorer.Features.Utilities.JavaDiscovery.EnumerateCandidates() });
            return Ok(new { best.Path, best.Major, best.Raw });
        }

        /// <summary>POST api/Diagnostics/JavaHealth — rilancia il controllo di salute di Java (sostituisce un JavaPath automatico non adatto, avvisa su uno scritto dall'utente).</summary>
        [HttpPost("JavaHealth")]
        public IActionResult JavaHealthRun([FromServices] MdExplorer.Services.Java.JavaHealthService health)
        {
            var r = health.Run();
            return Ok(new { outcome = r.Outcome.ToString(), r.ConfiguredPath, r.ConfiguredMajor, r.ConfiguredProblem, r.BestPath, r.BestMajor, r.EffectivePath, r.MinimumMajor });
        }

        /// <summary>
        /// Gets the log file path
        /// </summary>
        [HttpGet("LogPath")]
        public IActionResult GetLogPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logFilePath = Path.Combine(appData, "md-explorer", "electron_main.log");

            return Ok(new {
                path = logFilePath,
                exists = System.IO.File.Exists(logFilePath)
            });
        }
    }
}
