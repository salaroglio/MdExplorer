using Microsoft.AspNetCore.Mvc;
using System;
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
