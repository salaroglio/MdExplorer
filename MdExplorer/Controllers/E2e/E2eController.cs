using MdExplorer.Features.E2e;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.E2e
{
    /// <summary>
    /// End-to-end tests written in markdown (sprint docs-internal/Sprints/2026-09-26-Test-E2E-Da-Markdown.md).
    /// F3: what a run needs on this computer, and the installation of what is missing, asked by the
    /// secondary wizard only when the user runs a test (D7).
    /// </summary>
    [ApiController]
    [Route("api/e2e")]
    public class E2eController : ControllerBase
    {
        // One installation at a time: two wizards (two windows) must not write the same folder.
        private static readonly SemaphoreSlim InstallLock = new(1, 1);

        private readonly E2eEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Features.Services.ITocGenerationService _toc;
        private readonly ILogger<E2eController> _logger;

        public E2eController(E2eEnvironment environment, IHttpClientFactory httpClientFactory,
            Features.Services.ITocGenerationService toc, ILogger<E2eController> logger)
        {
            _environment = environment;
            _httpClientFactory = httpClientFactory;
            _toc = toc;
            _logger = logger;
        }

        public sealed class RunSettingsBody
        {
            public string Path { get; set; }
            public string ProjectPath { get; set; }
            public bool? DedicatedSession { get; set; }
            public bool? CommitAfterRun { get; set; }
            public bool? Headless { get; set; }
        }

        /// <summary>
        /// The run settings of a <c>.e2e.md</c> or of a folder (D21-D24): what the target itself says (null =
        /// inherited) and the effective value of each, with where it comes from, relative to the project.
        /// </summary>
        [HttpGet("settings")]
        public IActionResult GetSettings([FromQuery] string path, [FromQuery] string projectPath)
        {
            if (!TryResolve(path, projectPath, out var full, out var root, out var problem)) return BadRequest(new { error = problem });
            try
            {
                return Ok(Describe(full, root));
            }
            catch (E2eFormatException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Writes the settings of a file (its front matter) or of a folder (the front matter of its
        /// <c>&lt;folder&gt;.md.directory</c>, generated first when the folder has none). Only <c>e2e.run</c>
        /// changes; a null value removes the key (inherit).
        /// </summary>
        [HttpPut("settings")]
        public async Task<IActionResult> PutSettings([FromBody] RunSettingsBody body, CancellationToken ct)
        {
            if (!TryResolve(body?.Path, body?.ProjectPath, out var full, out var root, out var problem)) return BadRequest(new { error = problem });

            var settingsFile = Directory.Exists(full) ? E2eRunSettingsResolver.FolderSettingsPath(full) : full;
            if (!Directory.Exists(full) && !full.EndsWith(".e2e.md", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Le impostazioni dei test si scrivono su un file .e2e.md o su una cartella." });

            if (!System.IO.File.Exists(settingsFile))
            {
                // The folder's summary document does not exist yet: generate it with the existing function.
                if (!await _toc.GenerateTocAsync(full, settingsFile, ct) || !System.IO.File.Exists(settingsFile))
                    return StatusCode(500, new { error = $"Non sono riuscito a creare il documento di riassunto della cartella ({Path.GetFileName(settingsFile)}), dove vanno le impostazioni." });
            }

            try
            {
                var text = await System.IO.File.ReadAllTextAsync(settingsFile, ct);
                var written = E2eFrontMatter.WriteRunSettings(text,
                    new E2eRunSettings(body.DedicatedSession, body.CommitAfterRun, body.Headless), Path.GetFileName(settingsFile));
                if (written != text) await System.IO.File.WriteAllTextAsync(settingsFile, written, ct);
                return Ok(Describe(full, root));
            }
            catch (E2eFormatException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }
        }

        /// <summary>
        /// What a launch on <paramref name="path"/> would do, without doing it: the tests, their settings and
        /// run folders, and every problem that would stop it (F2 checks). Nothing is written.
        /// </summary>
        [HttpGet("plan")]
        public IActionResult GetPlan([FromQuery] string path, [FromQuery] string projectPath)
        {
            if (!TryResolve(path, projectPath, out var full, out var root, out var problem)) return BadRequest(new { error = problem });
            var plan = E2eRunPlanner.Plan(full, root, DateTime.Now);
            return Ok(new
            {
                canRun = plan.CanRun,
                errors = plan.Errors,
                warnings = plan.Warnings,
                items = plan.Items.Select(i => new
                {
                    file = i.RelativeTestFile,
                    runFolder = i.RelativeRunFolder,
                    tests = i.Preflight.Document?.Tests.Count ?? 0,
                    dedicatedSession = Setting(i.Settings.DedicatedSession, root),
                    commitAfterRun = Setting(i.Settings.CommitAfterRun, root),
                    headless = Setting(i.Settings.Headless, root),
                }),
            });
        }

        private static object Describe(string full, string root)
        {
            var isFolder = Directory.Exists(full);
            var settingsFile = isFolder ? E2eRunSettingsResolver.FolderSettingsPath(full) : full;
            var own = System.IO.File.Exists(settingsFile)
                ? E2eFrontMatter.ReadRunSettings(System.IO.File.ReadAllText(settingsFile), Path.GetFileName(settingsFile))
                : E2eRunSettings.None;
            var effective = E2eRunSettingsResolver.Resolve(full, root);
            return new
            {
                target = isFolder ? "folder" : "file",
                path = Relative(root, full),
                settingsFile = Relative(root, settingsFile),
                settingsFileExists = System.IO.File.Exists(settingsFile),
                own = new { dedicatedSession = own.DedicatedSession, commitAfterRun = own.CommitAfterRun, headless = own.Headless },
                effective = new
                {
                    dedicatedSession = Setting(effective.DedicatedSession, root),
                    commitAfterRun = Setting(effective.CommitAfterRun, root),
                    headless = Setting(effective.Headless, root),
                },
            };
        }

        private static object Setting(E2eSetting setting, string root) =>
            new { value = setting.Value, source = setting.Source == null ? null : Relative(root, setting.Source) };

        private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

        private static bool TryResolve(string path, string projectPath, out string full, out string root, out string problem)
        {
            full = root = problem = null;
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
            {
                problem = "Il progetto non è indicato o non esiste.";
                return false;
            }
            root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectPath));
            if (string.IsNullOrWhiteSpace(path))
            {
                problem = "Manca il file o la cartella.";
                return false;
            }
            full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!(string.Equals(full, root, comparison) || full.StartsWith(root + Path.DirectorySeparatorChar, comparison)))
            {
                problem = "Il percorso è fuori dal progetto.";
                return false;
            }
            if (!Directory.Exists(full) && !System.IO.File.Exists(full))
            {
                problem = "Il file o la cartella non esiste.";
                return false;
            }
            return true;
        }

        [HttpGet("prerequisites")]
        public async Task<ActionResult<E2ePrerequisitesReport>> GetPrerequisites(CancellationToken ct) =>
            await _environment.CheckAsync(ct);

        [HttpPost("prerequisites/playwright-mcp")]
        public Task<IActionResult> InstallPlaywrightMcp(CancellationToken ct) =>
            Install("@playwright/mcp", async () =>
            {
                var http = _httpClientFactory.CreateClient();
                http.Timeout = TimeSpan.FromMinutes(10);
                var installed = await _environment.InstallPlaywrightMcpAsync(http, ct);
                return string.Join(", ", installed);
            });

        [HttpPost("prerequisites/chromium")]
        public Task<IActionResult> InstallChromium(CancellationToken ct) =>
            Install("Chromium", () => _environment.InstallChromiumAsync(ct));

        private async Task<IActionResult> Install(string what, Func<Task<string>> install)
        {
            if (!await InstallLock.WaitAsync(0))
                return Conflict(new { error = "C'è già un'installazione in corso: aspetta che finisca." });
            try
            {
                _logger.LogInformation("[E2e] Installing {What}", what);
                var detail = await install();
                _logger.LogInformation("[E2e] Installed {What}: {Detail}", what, detail);
                return Ok(new { detail, report = await _environment.CheckAsync() });
            }
            catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException)
            {
                _logger.LogError(ex, "[E2e] Installing {What} failed", what);
                return StatusCode(502, new { error = ex.Message });
            }
            finally
            {
                InstallLock.Release();
            }
        }
    }
}
