using Ad.Tools.Dal.Extensions;
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
        private readonly Abstractions.DB.IUserSettingsDB _userSettingsDB;
        private readonly ILogger<E2eController> _logger;

        public E2eController(E2eEnvironment environment, IHttpClientFactory httpClientFactory,
            Features.Services.ITocGenerationService toc, Abstractions.DB.IUserSettingsDB userSettingsDB, ILogger<E2eController> logger)
        {
            _environment = environment;
            _httpClientFactory = httpClientFactory;
            _toc = toc;
            _userSettingsDB = userSettingsDB;
            _logger = logger;
        }

        public sealed class RunSettingsBody
        {
            public string Path { get; set; }
            public string ProjectPath { get; set; }
            /// <summary>claude, copilot, opencode; null = the project's engine (D10).</summary>
            public string? Engine { get; set; }
            /// <summary>The model of <see cref="Engine"/>; null = the engine's (the project's, for the project's engine).</summary>
            public string? Model { get; set; }
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
                return Ok(Describe(full, root, _userSettingsDB));
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
                    new E2eRunSettings(string.IsNullOrWhiteSpace(body.Engine) ? null : body.Engine.Trim().ToLowerInvariant(),
                        string.IsNullOrWhiteSpace(body.Engine) || string.IsNullOrWhiteSpace(body.Model) ? null : body.Model.Trim(),
                        body.CommitAfterRun, body.Headless), Path.GetFileName(settingsFile));
                if (written != text) await System.IO.File.WriteAllTextAsync(settingsFile, written, ct);
                return Ok(Describe(full, root, _userSettingsDB));
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
        public async Task<IActionResult> GetPlan([FromQuery] string path, [FromQuery] string projectPath, CancellationToken ct)
        {
            if (!TryResolve(path, projectPath, out var full, out var root, out var problem)) return BadRequest(new { error = problem });
            var plan = E2eRunPlanner.Plan(full, root, DateTime.Now);
            var (dotnet, _) = await _environment.CheckDotnetAsync(ct);
            return Ok(new
            {
                dotnet,
                canRun = plan.CanRun,
                errors = plan.Errors,
                warnings = plan.Warnings,
                items = plan.Items.Select(i => new
                {
                    file = i.RelativeTestFile,
                    runFolder = i.RelativeRunFolder,
                    tests = i.Preflight.Document?.Tests.Count ?? 0,
                    engine = TextSetting(i.Settings.Engine, root),
                    model = TextSetting(i.Settings.Model, root),
                    commitAfterRun = Setting(i.Settings.CommitAfterRun, root),
                    headless = Setting(i.Settings.Headless, root),
                    scripts = E2ePostRun.Scripts(i, Services.E2e.E2eLaunchService.CurrentGenerator()).Select(s => new { test = s.TestNumber, file = Path.GetFileName(s.Path), state = s.State, stale = s.Stale }),
                    replayPackages = ReplayPackages(i, root),
                }),
            });
        }

        /// <summary>
        /// Replays the scripts of a <c>.e2e.md</c> or of every test of a folder with <c>dotnet test</c>, without any
        /// LLM (F5): only the scripts still matching their test; outcomes on top of <c>## Esiti</c>; a commit
        /// when the test says so.
        /// </summary>
        [HttpPost("replay")]
        public async Task<IActionResult> Replay([FromBody] RunSettingsBody body, CancellationToken ct)
        {
            if (!TryResolve(body?.Path, body?.ProjectPath, out var full, out var root, out var problem)) return BadRequest(new { error = problem });
            var (dotnetRequirement, dotnet) = await _environment.CheckDotnetAsync(ct);
            if (dotnet == null) return UnprocessableEntity(new { error = dotnetRequirement.Detail + " " + dotnetRequirement.Remedy });
            var prerequisites = await _environment.CheckAsync(ct);
            if (!prerequisites.Browser.Ok) return UnprocessableEntity(new { error = prerequisites.Browser.Detail + " " + prerequisites.Browser.Remedy });

            // Each file on its own: a problem of another test (say, mixed headless settings in the MarkAgent tab,
            // which the replay never uses) must not stop this one.
            var plan = E2eRunPlanner.Plan(full, root, DateTime.Now);
            if (plan.Items.Count == 0) return UnprocessableEntity(new { error = string.Join("\n", plan.Errors) });
            // Support files of an older MdExplorer brought up to date before compiling (P4); none created here.
            var support = Services.E2e.E2eLaunchService.EnsureSupportFiles(plan.Items.Select(i => i.TestFile), root, create: false);
            if (support.Updated.Count > 0) _logger.LogInformation("[E2e] file di supporto aggiornati: {Files}", string.Join(", ", support.Updated));
            var generator = Services.E2e.E2eLaunchService.CurrentGenerator();

            var results = new System.Collections.Generic.List<E2eReplayFileResult>();
            foreach (var item in plan.Items)
            {
                if (!item.CanRun)
                {
                    // Its own checks and the planner's (paths outside the project) — second review, 27/09/2026.
                    results.Add(new E2eReplayFileResult { File = item.RelativeTestFile, Problem = string.Join(" ", item.Errors) });
                    continue;
                }
                var result = await E2eReplay.RunAsync(item, root, prerequisites.BrowserArgument, dotnet, DateTime.Now, plan.Secrets, generator, ct);
                if (item.Settings.CommitAfterRun.Value && result.Problem == null && result.Outcomes.Count > 0)
                {
                    var ok = result.Outcomes.Count(o => o.Passed);
                    var ko = result.Outcomes.Count - ok;
                    var message = $"test e2e (script): {item.RelativeTestFile} — " + string.Join(", ",
                        new[] { ok > 0 ? $"{ok} ✅" : null, ko > 0 ? $"{ko} ❌" : null }.Where(x => x != null))
                        + $" (esecuzione {Path.GetFileName(result.RunFolder)})";
                    result = new E2eReplayFileResult
                    {
                        File = result.File, RunFolder = result.RunFolder, Outcomes = result.Outcomes, Stale = result.Stale,
                        Commit = SafeCommit(item, message),
                    };
                }
                _logger.LogInformation("[E2e] rigioco {File}: {Ok}/{Total} superati{Problem}", item.RelativeTestFile,
                    result.Outcomes.Count(o => o.Passed), result.Outcomes.Count, result.Problem == null ? "" : " — " + result.Problem);
                results.Add(result);
            }

            return Ok(results.Select(r => new
            {
                file = r.File,
                runFolder = r.RunFolder == null ? null : Relative(root, r.RunFolder),
                outcomes = r.Outcomes.Select(o => new { test = o.Test, script = o.Script, passed = o.Passed, message = o.Message }),
                stale = r.Stale,
                problem = r.Problem,
                needsRestore = r.NeedsRestore,
                logs = r.Logs,
                commit = r.Commit == null ? null : new { committed = r.Commit.Committed, sha = r.Commit.Sha, message = r.Commit.Message, reason = r.Commit.Reason },
            }));
        }

        /// <summary>
        /// Downloads the packages the replay needs (<c>dotnet restore</c> of the tests projects of a test or of every
        /// test of a folder). Only on the user's request from the dialog: the replay itself never downloads (D3, D7).
        /// </summary>
        [HttpPost("replay-restore")]
        public async Task<IActionResult> RestoreReplayPackages([FromBody] RunSettingsBody body, CancellationToken ct)
        {
            if (!TryResolve(body?.Path, body?.ProjectPath, out var full, out var root, out var problem)) return BadRequest(new { error = problem });
            var (dotnetRequirement, dotnet) = await _environment.CheckDotnetAsync(ct);
            if (dotnet == null) return UnprocessableEntity(new { error = dotnetRequirement.Detail + " " + dotnetRequirement.Remedy });

            var restorePlan = E2eRunPlanner.Plan(full, root, DateTime.Now);
            // The csproj the packages are for is the current one (P4): updated before restoring, not after.
            Services.E2e.E2eLaunchService.EnsureSupportFiles(restorePlan.Items.Select(i => i.TestFile), root, create: false);
            var projects = restorePlan.Items
                .Select(i => E2eReplay.FindProject(i, root)).Where(p => p != null && !E2eReplay.PackagesRestored(p))
                .Distinct().ToList();
            if (projects.Count == 0) return Ok(new { restored = Array.Empty<string>() });

            var restored = new System.Collections.Generic.List<string>();
            foreach (var project in projects)
            {
                _logger.LogInformation("[E2e] dotnet restore {Project} (chiesto dall'utente)", project);
                var failure = await E2eReplay.RestoreAsync(project, dotnet, ct);
                if (failure != null)
                    return UnprocessableEntity(new { error = $"{Relative(root, project)}: {failure}", restored });
                restored.Add(Relative(root, project));
            }
            return Ok(new { restored });
        }

        /// <summary>The tests project of an item and whether its packages are downloaded; null without a project.</summary>
        private static object ReplayPackages(E2eRunItem item, string root)
        {
            var project = E2eReplay.FindProject(item, root);
            return project == null ? null : new { project = Relative(root, project), restored = E2eReplay.PackagesRestored(project) };
        }

        /// <summary>
        /// Adds to the project's .gitignore the lines that exclude the credentials files of a test or of every test
        /// of a folder (D12). Asked by the user from the dialog: the checks refuse a launch before that, so the agent
        /// could never do it itself on a new project (second review, 27/09/2026).
        /// </summary>
        [HttpPost("gitignore")]
        public IActionResult AddToGitIgnore([FromBody] RunSettingsBody body)
        {
            if (!TryResolve(body?.Path, body?.ProjectPath, out var full, out var root, out var problem)) return BadRequest(new { error = problem });
            var gitDir = LibGit2Sharp.Repository.Discover(root);
            if (gitDir == null) return UnprocessableEntity(new { error = "Il progetto non è un repository git: non serve nessun .gitignore." });
            using var repo = new LibGit2Sharp.Repository(gitDir);
            var gitignore = Path.Combine(repo.Info.WorkingDirectory, ".gitignore");

            var plan = E2eRunPlanner.Plan(full, root, DateTime.Now);
            var lines = plan.Items.Where(i => i.CredentialsFile != null && System.IO.File.Exists(i.CredentialsFile))
                .Where(i => !repo.Ignore.IsPathIgnored(Path.GetRelativePath(repo.Info.WorkingDirectory, i.CredentialsFile).Replace('\\', '/')))
                .Select(i => E2ePreflight.GitIgnoreLine(i.CredentialsFile)).Distinct().ToList();
            if (lines.Count == 0) return Ok(new { added = Array.Empty<string>() });

            var existing = System.IO.File.Exists(gitignore) ? System.IO.File.ReadAllText(gitignore) : "";
            var newline = existing.Contains("\r\n") ? "\r\n" : "\n";
            var text = existing.Length > 0 && !existing.EndsWith("\n") ? newline : "";
            text += "# credenziali dei test e2e (MdExplorer)" + newline + string.Join(newline, lines) + newline;
            System.IO.File.AppendAllText(gitignore, text);
            _logger.LogInformation("[E2e] aggiunte a .gitignore: {Lines}", string.Join(", ", lines));
            return Ok(new { added = lines });
        }

        private E2eCommitResult SafeCommit(E2eRunItem item, string message)
        {
            try
            {
                return E2eCommitter.Commit(item, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[E2e] commit dopo il rigioco di {File}", item.RelativeTestFile);
                return new E2eCommitResult(false, null, null, "il commit non è riuscito: " + ex.Message);
            }
        }

        private static object Describe(string full, string root, Abstractions.DB.IUserSettingsDB db)
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
                own = new { engine = own.Engine, model = own.Model, commitAfterRun = own.CommitAfterRun, headless = own.Headless,
                    obsoleteDedicatedSession = own.ObsoleteDedicatedSession },
                // What «inherit» ends up at when nobody says an engine (D10): the project's, the MarkAgent tab's.
                project = ProjectEngineOf(db, root),
                effective = new
                {
                    engine = TextSetting(effective.Engine, root),
                    model = TextSetting(effective.Model, root),
                    commitAfterRun = Setting(effective.CommitAfterRun, root),
                    headless = Setting(effective.Headless, root),
                },
            };
        }

        private static object Setting(E2eSetting setting, string root) =>
            new { value = setting.Value, source = setting.Source == null ? null : Relative(root, setting.Source) };

        private static object TextSetting(E2eTextSetting setting, string root) =>
            new { value = setting.Value, source = setting.Source == null ? null : Relative(root, setting.Source) };

        private static object ProjectEngineOf(Abstractions.DB.IUserSettingsDB db, string root)
        {
            var (engine, model) = MdExplorer.Service.ProjectsManager.ProjectEngine(db, root);
            return new { engine = engine == MdExplorer.Utilities.MarkAgentEngine.None ? null : MdExplorer.Utilities.MarkAgentEngines.IdOf(engine), model };
        }

        private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

        private bool TryResolve(string path, string projectPath, out string full, out string root, out string problem)
        {
            full = root = problem = null;
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
            {
                problem = "Il progetto non è indicato o non esiste.";
                return false;
            }
            // The root comes from the client: it must be a project MdExplorer knows, or these endpoints (which
            // write front matter and run dotnet test) would work on any folder a local process names.
            var comparisonKnown = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var asked = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectPath));
            // Materialized first: NHibernate LINQ cannot translate string.IsNullOrWhiteSpace (seen as a 500).
            // Inside a transaction: a read outside one breaks the next Commit of the shared session (project rule).
            System.Collections.Generic.List<string> paths;
            _userSettingsDB.BeginTransaction();
            try
            {
                paths = _userSettingsDB.GetDal<Abstractions.Entities.UserDB.Project>().GetList().ToList()
                    .Select(p => p.Path).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            }
            finally
            {
                _userSettingsDB.Commit();
            }
            var known = paths.Any(p => string.Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(p)), asked, comparisonKnown));
            if (!known)
            {
                problem = "Il percorso indicato non è un progetto di MdExplorer.";
                return false;
            }
            root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectPath));
            if (string.IsNullOrWhiteSpace(path))
            {
                problem = "Manca il file o la cartella.";
                return false;
            }
            full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(root, path));
            if (!E2eRunPlanner.IsInside(full, root))
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
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Every failure says what it is: on Windows the first one seen (28/09/2026) came back as a bare
                // 500, with nothing in the dialog to act on. File errors (a folder locked by the antivirus, access
                // denied) are not InvalidOperationException, and the dialog must show them all the same.
                _logger.LogError(ex, "[E2e] Installing {What} failed", what);
                var message = ex is InvalidOperationException or HttpRequestException
                    ? ex.Message
                    : $"{ex.GetType().Name}: {ex.Message}";
                return StatusCode(502, new { error = $"Installazione di {what} non riuscita: {message}" });
            }
            finally
            {
                InstallLock.Release();
            }
        }
    }
}
