using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.E2e
{
    /// <summary>One test file of a launch.</summary>
    public sealed class E2eRunItem
    {
        public string TestFile { get; init; }

        /// <summary>The test file relative to the project, with <c>/</c>: what the prompt names.</summary>
        public string RelativeTestFile { get; init; }

        public E2ePreflightResult Preflight { get; init; }
        public E2eEffectiveRunSettings Settings { get; init; }

        /// <summary><c>&lt;artifacts&gt;/esecuzioni/&lt;yyyy-MM-dd_HH-mm&gt;</c>, created by the launcher.</summary>
        public string RunFolder { get; init; }

        public string RelativeRunFolder { get; init; }

        /// <summary>The two lines MdExplorer sends to the agent (the skill does the rest).</summary>
        public string Prompt { get; init; }

        /// <summary>The credentials file of this test, when it has one: the agent must not read it.</summary>
        public string CredentialsFile { get; init; }

        /// <summary>What stops this file (its checks, paths outside the project): the other files still run.</summary>
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        public bool CanRun => Errors.Count == 0;
    }

    /// <summary>
    /// What a launch will do (F4a), the same for every engine: which tests, with which settings, where their
    /// results go, which secrets the Playwright server needs and which files the agent must not read.
    /// </summary>
    public sealed class E2eRunPlan
    {
        public string ProjectRoot { get; init; }
        public IReadOnlyList<E2eRunItem> Items { get; init; }

        /// <summary>What blocks the launch, each with what to do.</summary>
        public IReadOnlyList<string> Errors { get; init; }

        public IReadOnlyList<string> Warnings { get; init; }

        /// <summary>Every credential of every test of the launch, for the server's <c>--secrets</c> file.</summary>
        public IReadOnlyDictionary<string, string> Secrets { get; init; }

        /// <summary>
        /// Errors that concern the launch as a whole (conflicting credential keys, mixed headless settings in the
        /// MarkAgent tab): with any of them nothing runs. The errors of a single file are in its item.
        /// </summary>
        public IReadOnlyList<string> LaunchErrors { get; init; } = Array.Empty<string>();

        public bool CanRun => LaunchErrors.Count == 0 && Items.Any(i => i.CanRun);

        public IEnumerable<string> DeniedPaths =>
            Items.Select(i => i.CredentialsFile).Where(f => f != null).Distinct(StringComparer.Ordinal);
    }

    /// <summary>How to start <c>@playwright/mcp</c> with Electron's Node (D20).</summary>
    public sealed record E2ePlaywrightServer(string Command, IReadOnlyList<string> Args, IReadOnlyDictionary<string, string> Env)
    {
        public const string Name = "playwright";

        /// <summary>
        /// Tools of @playwright/mcp 0.0.82 no agent may use during a test (second review, 27/09/2026):
        /// <c>browser_run_code_unsafe</c> runs arbitrary code in the server (it can read any file, the
        /// credentials included, past every file rule) and <c>browser_evaluate</c> can read a field back after
        /// the server typed a secret into it — the redaction replaces only the exact value, so a value returned
        /// split or encoded reaches the LLM (seen: character codes read back in the dotenv probe).
        /// </summary>
        public static readonly IReadOnlyList<string> BannedTools = new[] { "browser_evaluate", "browser_run_code_unsafe" };

        /// <summary>
        /// The tools of @playwright/mcp 0.0.82 an agent may use: an explicit list, so a tool a newer version adds
        /// stays out until it is looked at.
        /// </summary>
        public static readonly IReadOnlyList<string> AllowedTools = new[]
        {
            "browser_click", "browser_close", "browser_console_messages", "browser_drag", "browser_drop",
            "browser_emulate_media", "browser_file_upload", "browser_fill_form", "browser_find", "browser_handle_dialog",
            "browser_hover", "browser_navigate", "browser_navigate_back", "browser_network_request",
            "browser_network_requests", "browser_press_key", "browser_resize", "browser_select_option",
            "browser_snapshot", "browser_tabs", "browser_take_screenshot", "browser_type", "browser_wait_for",
        };

        /// <param name="outputDir">Only the server's diagnostic files go there: screenshots go where the agent
        /// names them, relative to the project (verified 27/09/2026). Always given, or the server writes
        /// <c>.playwright-mcp/</c> in the project.</param>
        /// <param name="initScript">A script run in every page before the page's own (<c>--init-script</c>): it
        /// switches on the signals of the sites instrumented with the mde-e2e-signals skill, without
        /// <c>browser_evaluate</c> (banned).</param>
        public static E2ePlaywrightServer For(E2ePrerequisitesReport prerequisites, bool headless, string secretsFile, string outputDir, string initScript = null)
        {
            if (!prerequisites.ReadyToRun)
                throw new InvalidOperationException("Mancano dei prerequisiti per i test e2e: apri il wizard di installazione.");
            if (string.IsNullOrWhiteSpace(outputDir))
                throw new ArgumentException("La cartella dei file diagnostici è obbligatoria.", nameof(outputDir));

            var args = new List<string> { prerequisites.PlaywrightMcpCli, "--browser", prerequisites.BrowserArgument };
            if (headless) args.Add("--headless");
            args.AddRange(new[] { "--isolated", "--codegen", "csharp", "--output-dir", outputDir });
            if (secretsFile != null) args.AddRange(new[] { "--secrets", secretsFile });
            if (initScript != null) args.AddRange(new[] { "--init-script", initScript });

            return new E2ePlaywrightServer(prerequisites.ElectronPath, args,
                new Dictionary<string, string> { ["ELECTRON_RUN_AS_NODE"] = "1" });
        }
    }

    public static class E2eRunPlanner
    {
        /// <param name="target">A <c>.e2e.md</c> file, or a folder: every <c>.e2e.md</c> below it.</param>
        public static E2eRunPlan Plan(string target, string projectRoot, DateTime now)
        {
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectRoot));
            var full = Path.GetFullPath(target);
            var errors = new List<string>();
            var warnings = new List<string>();

            List<string> files;
            if (Directory.Exists(full)) files = TestFilesUnder(full).ToList();
            else if (File.Exists(full)) files = new List<string> { full };
            else return Refused(root, $"'{target}' non esiste.");

            if (files.Count == 0)
                return Refused(root, $"Nella cartella '{Relative(root, full)}' non ci sono file .e2e.md.");

            var stamp = now.ToString("yyyy-MM-dd_HH-mm", System.Globalization.CultureInfo.InvariantCulture);
            var items = new List<E2eRunItem>();
            var secrets = new Dictionary<string, string>(StringComparer.Ordinal);
            var secretSource = new Dictionary<string, string>(StringComparer.Ordinal);

            var launchErrors = new List<string>();
            foreach (var file in files)
            {
                var relative = Relative(root, file);
                var itemErrors = new List<string>();
                E2ePreflightResult preflight;
                try
                {
                    preflight = E2ePreflight.Check(file);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or LibGit2Sharp.LibGit2SharpException)
                {
                    errors.Add($"{relative}: non riesco a controllarlo ({ex.Message}).");
                    continue;
                }
                itemErrors.AddRange(preflight.Errors);
                warnings.AddRange(preflight.Warnings);
                if (preflight.Document == null) continue;

                E2eEffectiveRunSettings settings;
                try
                {
                    settings = E2eRunSettingsResolver.Resolve(file, root);
                }
                catch (Exception ex) when (ex is E2eFormatException or ArgumentException or IOException)
                {
                    errors.Add(ex.Message);
                    continue;
                }

                var folder = Path.GetDirectoryName(file)!;
                string runFolder = null;
                if (preflight.Document.Artifacts != null)
                {
                    var artifacts = Path.GetFullPath(Path.Combine(folder, preflight.Document.Artifacts));
                    if (!IsInside(artifacts, root))
                        itemErrors.Add($"{relative}: 'e2e.artifacts' ({preflight.Document.Artifacts}) porta fuori dal progetto.");
                    else
                        runFolder = FreeRunFolder(Path.Combine(artifacts, "esecuzioni"), stamp);
                }
                if (preflight.Document.SiteMap != null && !IsInside(Path.GetFullPath(Path.Combine(folder, preflight.Document.SiteMap)), root))
                    itemErrors.Add($"{relative}: 'e2e.siteMap' ({preflight.Document.SiteMap}) porta fuori dal progetto.");

                string credentials = null;
                if (preflight.Document.Credentials != null)
                {
                    credentials = Path.GetFullPath(Path.Combine(folder, preflight.Document.Credentials));
                    // A test cloned from somebody else could point at any file of this computer (a cloud key)
                    // and have its values typed into the author's site: only files of the project.
                    if (!IsInside(credentials, root))
                    {
                        itemErrors.Add($"{relative}: 'e2e.credentials' ({preflight.Document.Credentials}) porta fuori dal progetto: il file delle credenziali deve stare nel progetto.");
                        credentials = null;
                    }
                    else if (File.Exists(credentials))
                    {
                        foreach (var (key, value) in ReadCredentials(credentials))
                        {
                            if (secrets.TryGetValue(key, out var existing) && existing != value)
                                launchErrors.Add($"La chiave '{key}' ha valori diversi in '{Relative(root, secretSource[key])}' e in '{Relative(root, credentials)}': in un lancio le chiavi devono essere uniche. Rinominane una.");
                            else
                            {
                                secrets[key] = value;
                                secretSource[key] = credentials;
                            }
                        }
                    }
                }

                var relativeRun = runFolder == null ? null : Relative(root, runFolder);
                items.Add(new E2eRunItem
                {
                    TestFile = file,
                    RelativeTestFile = relative,
                    Preflight = preflight,
                    Settings = settings,
                    RunFolder = runFolder,
                    RelativeRunFolder = relativeRun,
                    CredentialsFile = credentials,
                    Errors = itemErrors,
                    Prompt = relativeRun == null ? null : $"Esegui i test di {relative}.\nCartella dell'esecuzione: {relativeRun}/\n",
                });
            }

            // The tests always run in a session of their own (D10): no shared browser to agree on.
            // Errors shows everything (for the dialog): files that could not be read, each file's own problems,
            // the launch-wide ones.
            errors.AddRange(items.SelectMany(i => i.Errors));
            errors.AddRange(launchErrors);
            return new E2eRunPlan { ProjectRoot = root, Items = items, Errors = errors, Warnings = warnings, Secrets = secrets, LaunchErrors = launchErrors };
        }

        /// <summary>
        /// Writes the secrets in the dotenv format the Playwright server reads (<c>--secrets</c>), readable only
        /// by the user. Values that dotenv would change (spaces at the ends, <c>#</c>, quotes) are quoted;
        /// a value that cannot be quoted safely stops the launch.
        /// </summary>
        public static void WriteSecretsFile(string path, IReadOnlyDictionary<string, string> secrets)
        {
            var sb = new StringBuilder("# MdExplorer: credenziali dei test e2e di questo lancio. Non modificare.\n");
            foreach (var (key, value) in secrets)
            {
                if (value.IndexOfAny(new[] { '\n', '\r' }) >= 0)
                    throw new InvalidOperationException($"La credenziale '{key}' contiene un a capo: non si può passare al server Playwright.");
                var needsQuotes = value.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1]))
                    || value.Contains('#') || value.StartsWith("\"", StringComparison.Ordinal) || value.StartsWith("'", StringComparison.Ordinal) || value.StartsWith("`", StringComparison.Ordinal);
                string written;
                if (!needsQuotes) written = value;
                else if (!value.Contains('\'')) written = "'" + value + "'";
                else if (!value.Contains('"')) written = "\"" + value + "\"";
                else throw new InvalidOperationException($"La credenziale '{key}' contiene sia apici singoli che doppi insieme a spazi o '#': non si può passare al server Playwright senza cambiarla.");
                sb.Append(key).Append('=').Append(written).Append('\n');
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, string.Empty);
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            File.WriteAllText(path, sb.ToString());
        }

        private static IEnumerable<(string Key, string Value)> ReadCredentials(string path) =>
            File.ReadAllLines(path)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0 && !l.StartsWith("#", StringComparison.Ordinal) && l.IndexOf('=') > 0)
                .Select(l => (l.Substring(0, l.IndexOf('=')).Trim(), l.Substring(l.IndexOf('=') + 1).Trim()));

        /// <summary>
        /// Every credentials file named by any <c>.e2e.md</c> of the project, whatever its name: a launch denies
        /// the agent all of them, not only its own (second review, 27/09/2026). Unreadable test files are skipped.
        /// </summary>
        public static IReadOnlyList<string> CredentialFilesInProject(string projectRoot)
        {
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(projectRoot));
            var found = new List<string>();
            foreach (var test in TestFilesUnder(root))
            {
                try
                {
                    var e2e = E2eFrontMatter.ReadMapping(File.ReadAllText(test), Path.GetFileName(test));
                    if (e2e != null && e2e.Children.TryGetValue(new YamlDotNet.RepresentationModel.YamlScalarNode("credentials"), out var node)
                        && node is YamlDotNet.RepresentationModel.YamlScalarNode scalar && !string.IsNullOrWhiteSpace(scalar.Value))
                    {
                        var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(test)!, scalar.Value.Trim()));
                        if (IsInside(path, root)) found.Add(path);
                    }
                }
                catch (Exception ex) when (ex is E2eFormatException or IOException) { /* its own preflight says it */ }
            }
            return found.Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal).ToList();
        }

        private static IEnumerable<string> TestFilesUnder(string folder)
        {
            foreach (var file in Directory.GetFiles(folder, "*.e2e.md").OrderBy(f => f, StringComparer.Ordinal))
                yield return file;
            foreach (var sub in Directory.GetDirectories(folder).OrderBy(d => d, StringComparer.Ordinal))
            {
                var name = Path.GetFileName(sub);
                if (name.StartsWith(".", StringComparison.Ordinal) || name is "bin" or "obj" or "node_modules") continue;
                foreach (var file in TestFilesUnder(sub)) yield return file;
            }
        }

        private static E2eRunPlan Refused(string root, string error) => new()
        {
            LaunchErrors = new[] { error },
            ProjectRoot = root,
            Items = Array.Empty<E2eRunItem>(),
            Errors = new[] { error },
            Warnings = Array.Empty<string>(),
            Secrets = new Dictionary<string, string>(),
        };

        private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

        /// <summary>The run folder of this minute; with a suffix when a launch of the same minute already has it.</summary>
        private static string FreeRunFolder(string runs, string stamp)
        {
            var candidate = Path.Combine(runs, stamp);
            for (var n = 2; Directory.Exists(candidate); n++) candidate = Path.Combine(runs, stamp + "-" + n);
            return candidate;
        }

        public static bool IsInside(string path, string root)
        {
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
            var trimmedRoot = Path.TrimEndingDirectorySeparator(root);
            // A project on a drive root ("D:\", "/") keeps its separator: do not add a second one.
            var prefix = trimmedRoot.EndsWith(Path.DirectorySeparatorChar) ? trimmedRoot : trimmedRoot + Path.DirectorySeparatorChar;
            return string.Equals(full, trimmedRoot, comparison) || full.StartsWith(prefix, comparison);
        }
    }
}
