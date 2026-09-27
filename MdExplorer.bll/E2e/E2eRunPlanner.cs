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

        public bool CanRun => Errors.Count == 0 && Items.Count > 0;

        public IEnumerable<string> DeniedPaths =>
            Items.Select(i => i.CredentialsFile).Where(f => f != null).Distinct(StringComparer.Ordinal);
    }

    /// <summary>How to start <c>@playwright/mcp</c> with Electron's Node (D20).</summary>
    public sealed record E2ePlaywrightServer(string Command, IReadOnlyList<string> Args, IReadOnlyDictionary<string, string> Env)
    {
        public const string Name = "playwright";

        /// <param name="outputDir">Only the server's diagnostic files go there: screenshots go where the agent
        /// names them, relative to the project (verified 27/09/2026). Always given, or the server writes
        /// <c>.playwright-mcp/</c> in the project.</param>
        public static E2ePlaywrightServer For(E2ePrerequisitesReport prerequisites, bool headless, string secretsFile, string outputDir)
        {
            if (!prerequisites.ReadyToRun)
                throw new InvalidOperationException("Mancano dei prerequisiti per i test e2e: apri il wizard di installazione.");
            if (string.IsNullOrWhiteSpace(outputDir))
                throw new ArgumentException("La cartella dei file diagnostici è obbligatoria.", nameof(outputDir));

            var args = new List<string> { prerequisites.PlaywrightMcpCli, "--browser", prerequisites.BrowserArgument };
            if (headless) args.Add("--headless");
            args.AddRange(new[] { "--isolated", "--codegen", "csharp", "--output-dir", outputDir });
            if (secretsFile != null) args.AddRange(new[] { "--secrets", secretsFile });

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

            var stamp = now.ToString("yyyy-MM-dd_HH-mm");
            var items = new List<E2eRunItem>();
            var secrets = new Dictionary<string, string>(StringComparer.Ordinal);
            var secretSource = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var file in files)
            {
                var relative = Relative(root, file);
                var preflight = E2ePreflight.Check(file);
                errors.AddRange(preflight.Errors);
                warnings.AddRange(preflight.Warnings);
                if (preflight.Document == null) continue;

                E2eEffectiveRunSettings settings;
                try
                {
                    settings = E2eRunSettingsResolver.Resolve(file, root);
                }
                catch (E2eFormatException ex)
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
                        errors.Add($"{relative}: 'e2e.artifacts' ({preflight.Document.Artifacts}) porta fuori dal progetto.");
                    else
                        runFolder = Path.Combine(artifacts, "esecuzioni", stamp);
                }

                string credentials = null;
                if (preflight.Document.Credentials != null)
                {
                    credentials = Path.GetFullPath(Path.Combine(folder, preflight.Document.Credentials));
                    if (File.Exists(credentials))
                    {
                        foreach (var (key, value) in ReadCredentials(credentials))
                        {
                            if (secrets.TryGetValue(key, out var existing) && existing != value)
                                errors.Add($"La chiave '{key}' ha valori diversi in '{Relative(root, secretSource[key])}' e in '{Relative(root, credentials)}': in un lancio le chiavi devono essere uniche. Rinominane una.");
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
                    Prompt = relativeRun == null ? null : $"Esegui i test di {relative}.\nCartella dell'esecuzione: {relativeRun}/\n",
                });
            }

            // In the MarkAgent tab there is one Playwright server, started with or without --headless (D25).
            var inTab = items.Where(i => !i.Settings.DedicatedSession.Value).ToList();
            if (inTab.Select(i => i.Settings.Headless.Value).Distinct().Count() > 1)
            {
                errors.Add("Questi test girano nella sessione del tab MarkAgent, dove il browser è uno solo, ma alcuni chiedono il browser nascosto e altri visibile: "
                    + string.Join(", ", inTab.Select(i => $"{i.RelativeTestFile} ({(i.Settings.Headless.Value ? "nascosto" : "visibile")})"))
                    + ". Uniforma l'impostazione headless (per esempio sulla cartella) oppure usa sessioni dedicate.");
            }

            return new E2eRunPlan { ProjectRoot = root, Items = items, Errors = errors, Warnings = warnings, Secrets = secrets };
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
                    || value.Contains('#') || value.StartsWith("\"") || value.StartsWith("'") || value.StartsWith("`");
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
                .Where(l => l.Length > 0 && !l.StartsWith("#") && l.IndexOf('=') > 0)
                .Select(l => (l.Substring(0, l.IndexOf('=')).Trim(), l.Substring(l.IndexOf('=') + 1).Trim()));

        private static IEnumerable<string> TestFilesUnder(string folder)
        {
            foreach (var file in Directory.GetFiles(folder, "*.e2e.md").OrderBy(f => f, StringComparer.Ordinal))
                yield return file;
            foreach (var sub in Directory.GetDirectories(folder).OrderBy(d => d, StringComparer.Ordinal))
            {
                var name = Path.GetFileName(sub);
                if (name.StartsWith(".") || name is "bin" or "obj" or "node_modules") continue;
                foreach (var file in TestFilesUnder(sub)) yield return file;
            }
        }

        private static E2eRunPlan Refused(string root, string error) => new()
        {
            ProjectRoot = root,
            Items = Array.Empty<E2eRunItem>(),
            Errors = new[] { error },
            Warnings = Array.Empty<string>(),
            Secrets = new Dictionary<string, string>(),
        };

        private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

        private static bool IsInside(string path, string root)
        {
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return string.Equals(Path.TrimEndingDirectorySeparator(path), root, comparison)
                || path.StartsWith(root + Path.DirectorySeparatorChar, comparison);
        }
    }
}
