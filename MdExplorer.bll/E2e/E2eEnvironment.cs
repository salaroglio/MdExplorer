using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.E2e
{
    /// <summary>One thing a test run needs: whether it is there, what was found, and what to do when it is not.</summary>
    public sealed record E2eRequirement(string Id, bool Ok, string Detail, string Remedy, bool Installable);

    public sealed class E2ePrerequisitesReport
    {
        public E2eRequirement Electron { get; init; }
        public E2eRequirement Browser { get; init; }
        public E2eRequirement PlaywrightMcp { get; init; }

        /// <summary>The value for <c>@playwright/mcp --browser</c>: <c>msedge</c>, <c>chrome</c> or <c>chromium</c>.</summary>
        public string BrowserArgument { get; init; }

        public string ElectronPath { get; init; }
        public string PlaywrightMcpCli { get; init; }

        public bool ReadyToRun => Electron.Ok && Browser.Ok && PlaywrightMcp.Ok;
    }

    /// <summary>
    /// What a test run needs on this computer (F3) and how to install what is missing, outside MdExplorer's
    /// package (D3, D7, D11, D20):
    /// <list type="bullet">
    /// <item>Node is always Electron's own (<c>ELECTRON_RUN_AS_NODE=1</c> on the executable that Electron puts
    /// in <c>MDEXPLORER_ELECTRON_EXE</c>): never a system Node, never installed;</item>
    /// <item>the browser is the one already installed (Edge first on Windows, Chrome first elsewhere); Chromium
    /// is downloaded by Playwright only when there is none;</item>
    /// <item><c>@playwright/mcp</c> is downloaded from the npm registry with its dependencies, each archive
    /// checked against the registry's sha512, without npm.</item>
    /// </list>
    /// </summary>
    public sealed class E2eEnvironment
    {
        public const string PlaywrightMcpVersion = "0.0.82";
        public const string ElectronVariable = "MDEXPLORER_ELECTRON_EXE";
        private const string Registry = "https://registry.npmjs.org/";
        private const string InstalledMarker = "mde-installed.json";

        private readonly string _toolsRoot;
        private readonly Func<string, string> _environment;

        /// <param name="toolsRoot">Where downloads go; MdExplorer uses <c>~/MdExplorer-Models</c>, like llama.cpp.</param>
        public E2eEnvironment(string toolsRoot = null, Func<string, string> environment = null)
        {
            _toolsRoot = toolsRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "MdExplorer-Models");
            _environment = environment ?? Environment.GetEnvironmentVariable;
        }

        public string PlaywrightMcpFolder => Path.Combine(_toolsRoot, "playwright-mcp", PlaywrightMcpVersion);
        public string PlaywrightMcpCli => Path.Combine(PlaywrightMcpFolder, "node_modules", "@playwright", "mcp", "cli.js");
        private string PlaywrightCoreCli => Path.Combine(PlaywrightMcpFolder, "node_modules", "playwright-core", "cli.js");

        public async Task<E2ePrerequisitesReport> CheckAsync(CancellationToken ct = default)
        {
            var electronPath = _environment(ElectronVariable);
            var electron = await CheckElectronAsync(electronPath, ct);
            var mcp = CheckPlaywrightMcp();
            var (browser, argument) = await CheckBrowserAsync(electron.Ok ? electronPath : null, mcp.Ok, ct);

            return new E2ePrerequisitesReport
            {
                Electron = electron,
                Browser = browser,
                PlaywrightMcp = mcp,
                BrowserArgument = argument,
                ElectronPath = electron.Ok ? electronPath : null,
                PlaywrightMcpCli = mcp.Ok ? PlaywrightMcpCli : null,
            };
        }

        // ---------------------------------------------------------------- Electron

        private static async Task<E2eRequirement> CheckElectronAsync(string path, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(path))
                return new E2eRequirement("electron", false,
                    $"MdExplorer non è stato avviato da ElectronMdExplorer: manca la variabile {ElectronVariable}.",
                    $"Avvia MdExplorer dall'app. In sviluppo, imposta {ElectronVariable} con il percorso dell'eseguibile di Electron (ElectronMdExplorer/node_modules/electron/dist/electron).",
                    false);
            if (!File.Exists(path))
                return new E2eRequirement("electron", false, $"L'eseguibile di Electron '{path}' non esiste.",
                    $"Correggi {ElectronVariable} oppure riavvia MdExplorer dall'app.", false);

            // A functional probe, not a guess from the file name: the packaged app could have the
            // RunAsNode fuse turned off.
            var (exit, output) = await RunAsNodeAsync(path, new[] { "-e", "process.stdout.write(process.versions.node)" }, TimeSpan.FromSeconds(20), ct);
            var match = Regex.Match(output ?? "", @"^(\d+)\.\d+\.\d+");
            if (exit != 0 || !match.Success)
                return new E2eRequirement("electron", false,
                    $"Electron non funziona come Node (uscita {exit}): {Tail(output)}",
                    "L'eseguibile di MdExplorer deve permettere ELECTRON_RUN_AS_NODE (fuse RunAsNode attiva).", false);
            if (int.Parse(match.Groups[1].Value) < 18)
                return new E2eRequirement("electron", false, $"Il Node di Electron è {output}: Playwright MCP chiede almeno Node 18.",
                    "Aggiorna MdExplorer.", false);

            return new E2eRequirement("electron", true, $"Node {output} di Electron ({path})", null, false);
        }

        // ---------------------------------------------------------------- Playwright MCP

        private E2eRequirement CheckPlaywrightMcp() =>
            File.Exists(Path.Combine(PlaywrightMcpFolder, InstalledMarker)) && File.Exists(PlaywrightMcpCli)
                ? new E2eRequirement("playwright-mcp", true, $"@playwright/mcp {PlaywrightMcpVersion} in {PlaywrightMcpFolder}", null, true)
                : new E2eRequirement("playwright-mcp", false, $"@playwright/mcp {PlaywrightMcpVersion} non è installato.",
                    "Installalo: MdExplorer scarica dal registro npm il server e le sue dipendenze (circa 19 MB) e ne verifica l'impronta.", true);

        /// <summary>
        /// Downloads <c>@playwright/mcp</c> and its dependencies into <see cref="PlaywrightMcpFolder"/>. Every
        /// version must be exact (no ranges) and every archive must match the registry's sha512: anything
        /// else stops the installation.
        /// <para>
        /// The files go straight into the final folder and <c>mde-installed.json</c> is written last: without it
        /// the folder counts as not installed (<see cref="CheckPlaywrightMcp"/>) and the next attempt starts from
        /// scratch. No staging folder renamed at the end: on Windows that rename failed with «Access to the path
        /// is denied» (28/09/2026) — the antivirus still holds the files just written, and a folder with open
        /// files cannot be moved.
        /// </para>
        /// </summary>
        public async Task<IReadOnlyList<string>> InstallPlaywrightMcpAsync(HttpClient http, CancellationToken ct = default)
        {
            var target = PlaywrightMcpFolder;
            var installed = new List<string>();
            var completed = false;
            // A previous attempt left something: an installed folder always has the marker, so this is debris.
            if (Directory.Exists(target)) Directory.Delete(target, true);
            RemoveOldStagingFolders();
            try
            {
                var pending = new Queue<(string Name, string Version)>();
                pending.Enqueue(("@playwright/mcp", PlaywrightMcpVersion));
                var seen = new HashSet<string>();

                while (pending.Count > 0)
                {
                    var (name, version) = pending.Dequeue();
                    if (!seen.Add(name + "@" + version)) continue;

                    var meta = JsonNode.Parse(await http.GetStringAsync(Registry + name + "/" + version, ct))
                        ?? throw new InvalidOperationException($"Il registro npm non ha risposto per {name}@{version}.");
                    var tarball = meta["dist"]?["tarball"]?.GetValue<string>()
                        ?? throw new InvalidOperationException($"{name}@{version}: il registro non indica l'archivio.");
                    var integrity = meta["dist"]?["integrity"]?.GetValue<string>()
                        ?? throw new InvalidOperationException($"{name}@{version}: il registro non indica l'impronta (integrity).");

                    var bytes = await http.GetByteArrayAsync(tarball, ct);
                    VerifyIntegrity(bytes, integrity, name, version);
                    ExtractPackage(bytes, Path.Combine(target, "node_modules", name.Replace('/', Path.DirectorySeparatorChar)));
                    installed.Add($"{name}@{version}");

                    if (meta["dependencies"] is JsonObject deps)
                    {
                        foreach (var (depName, depVersion) in deps)
                        {
                            var v = depVersion?.GetValue<string>();
                            if (v == null || !Regex.IsMatch(v, @"^\d+\.\d+\.\d+([-+][0-9A-Za-z.-]+)?$"))
                                throw new InvalidOperationException(
                                    $"{name}@{version} chiede {depName}@{v}: non è una versione esatta, e MdExplorer installa solo versioni esatte. Aggiorna la versione fissata di @playwright/mcp.");
                            pending.Enqueue((depName, v));
                        }
                    }
                }

                File.WriteAllText(Path.Combine(target, InstalledMarker), JsonSerializer.Serialize(new
                {
                    package = "@playwright/mcp",
                    version = PlaywrightMcpVersion,
                    installed,
                    date = DateTime.UtcNow.ToString("o"),
                }, new JsonSerializerOptions { WriteIndented = true }));

                completed = true;
                return installed;
            }
            finally
            {
                // A failed attempt: its files go, if the antivirus lets them. If it does not, what stays has no
                // marker, counts as not installed, and the next attempt deletes it first.
                if (!completed)
                {
                    try { if (Directory.Exists(target)) Directory.Delete(target, true); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
        }

        /// <summary>The <c>&lt;version&gt;.download-*</c> folders of the installer before 28/09/2026, left by a failed rename.</summary>
        private void RemoveOldStagingFolders()
        {
            var parent = Path.GetDirectoryName(PlaywrightMcpFolder)!;
            if (!Directory.Exists(parent)) return;
            foreach (var old in Directory.GetDirectories(parent, PlaywrightMcpVersion + ".download-*"))
            {
                try { Directory.Delete(old, true); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
        }

        public static void VerifyIntegrity(byte[] bytes, string integrity, string name, string version)
        {
            var sri = integrity.Split(' ').FirstOrDefault(s => s.StartsWith("sha512-", StringComparison.Ordinal))
                ?? throw new InvalidOperationException($"{name}@{version}: l'impronta del registro non è sha512 ('{integrity}').");
            var actual = "sha512-" + Convert.ToBase64String(SHA512.HashData(bytes));
            if (!string.Equals(actual, sri, StringComparison.Ordinal))
                throw new InvalidOperationException($"{name}@{version}: l'archivio scaricato non corrisponde all'impronta del registro. Installazione interrotta.");
        }

        /// <summary>An npm archive: a gzipped tar whose entries start with <c>package/</c>.</summary>
        public static void ExtractPackage(byte[] tgz, string target)
        {
            var root = Path.GetFullPath(target) + Path.DirectorySeparatorChar;
            using var gzip = new GZipStream(new MemoryStream(tgz), CompressionMode.Decompress);
            using var reader = new TarReader(gzip);
            while (reader.GetNextEntry() is { } entry)
            {
                if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile)) continue;
                var slash = entry.Name.IndexOf('/');
                if (slash < 0) continue;
                var destination = Path.GetFullPath(Path.Combine(root, entry.Name.Substring(slash + 1)));
                if (!destination.StartsWith(root, StringComparison.Ordinal))
                    throw new InvalidOperationException($"L'archivio contiene un percorso fuori dalla sua cartella: '{entry.Name}'.");
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                entry.ExtractToFile(destination, overwrite: true);
            }
        }

        // ---------------------------------------------------------------- Browser

        /// <summary>Browsers already on this computer, in the order MdExplorer prefers them (D11).</summary>
        public IEnumerable<(string Argument, string Path)> InstalledBrowserCandidates()
        {
            string Env(string name) => _environment(name) ?? "";
            if (OperatingSystem.IsWindows())
            {
                foreach (var root in new[] { Env("ProgramFiles(x86)"), Env("ProgramFiles"), Env("LOCALAPPDATA") }.Where(r => r.Length > 0))
                    yield return ("msedge", Path.Combine(root, "Microsoft", "Edge", "Application", "msedge.exe"));
                foreach (var root in new[] { Env("ProgramFiles"), Env("ProgramFiles(x86)"), Env("LOCALAPPDATA") }.Where(r => r.Length > 0))
                    yield return ("chrome", Path.Combine(root, "Google", "Chrome", "Application", "chrome.exe"));
            }
            else if (OperatingSystem.IsMacOS())
            {
                yield return ("chrome", "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome");
                yield return ("msedge", "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge");
            }
            else
            {
                yield return ("chrome", "/opt/google/chrome/chrome");
                yield return ("msedge", "/opt/microsoft/msedge/msedge");
            }
        }

        private async Task<(E2eRequirement, string)> CheckBrowserAsync(string electronPath, bool mcpInstalled, CancellationToken ct)
        {
            var found = InstalledBrowserCandidates().FirstOrDefault(c => File.Exists(c.Path));
            if (found.Path != null)
                return (new E2eRequirement("browser", true, $"{(found.Argument == "msedge" ? "Microsoft Edge" : "Google Chrome")} ({found.Path})", null, false), found.Argument);

            var chromium = electronPath != null && mcpInstalled ? await ChromiumLocationAsync(electronPath, ct) : null;
            if (chromium != null && File.Exists(Path.Combine(chromium, "INSTALLATION_COMPLETE")))
                return (new E2eRequirement("browser", true, $"Chromium di Playwright ({chromium})", null, false), "chromium");

            return (new E2eRequirement("browser", false, "Né Edge né Chrome sono installati.",
                "Installa Edge o Chrome, oppure fai scaricare a MdExplorer il Chromium di Playwright (circa 150 MB; serve prima @playwright/mcp).",
                electronPath != null && mcpInstalled), null);
        }

        /// <summary>Where Playwright puts its Chromium, asked to Playwright itself (<c>install --dry-run</c>).</summary>
        private async Task<string> ChromiumLocationAsync(string electronPath, CancellationToken ct)
        {
            var (exit, output) = await RunAsNodeAsync(electronPath, new[] { PlaywrightCoreCli, "install", "--dry-run", "chromium" }, TimeSpan.FromSeconds(30), ct);
            if (exit != 0) return null;
            var m = Regex.Match(output, @"^Chrome for Testing .*\n\s*Install location:\s*(.+?)\s*$", RegexOptions.Multiline);
            return m.Success ? m.Groups[1].Value : null;
        }

        /// <summary>Downloads Playwright's Chromium, with Electron's Node and Playwright's own installer.</summary>
        public async Task<string> InstallChromiumAsync(CancellationToken ct = default)
        {
            var report = await CheckAsync(ct);
            if (!report.Electron.Ok) throw new InvalidOperationException(report.Electron.Detail + " " + report.Electron.Remedy);
            if (!report.PlaywrightMcp.Ok) throw new InvalidOperationException("Installa prima @playwright/mcp: il Chromium lo scarica il suo Playwright.");

            var (exit, output) = await RunAsNodeAsync(report.ElectronPath, new[] { PlaywrightCoreCli, "install", "chromium" }, TimeSpan.FromMinutes(20), ct);
            if (exit != 0)
                throw new InvalidOperationException($"Il download di Chromium non è riuscito (uscita {exit}): {Tail(output)}");
            return Tail(output);
        }

        // ---------------------------------------------------------------- .NET SDK (replay only)

        /// <summary>
        /// The <c>dotnet</c> that can replay the scripts (F5): an SDK 8 or later. Not needed to run the tests with
        /// MarkAgent, only to replay them without an LLM.
        /// </summary>
        public async Task<(E2eRequirement Requirement, string Dotnet)> CheckDotnetAsync(CancellationToken ct = default)
        {
            var candidates = new List<string>();
            var root = _environment("DOTNET_ROOT");
            var exe = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
            if (!string.IsNullOrWhiteSpace(root)) candidates.Add(Path.Combine(root, exe));
            foreach (var dir in (_environment("PATH") ?? "").Split(Path.PathSeparator).Where(d => d.Trim().Length > 0))
                candidates.Add(Path.Combine(dir.Trim().Trim('"'), exe));
            var dotnet = candidates.FirstOrDefault(File.Exists);
            if (dotnet == null)
                return (new E2eRequirement("dotnet", false, ".NET SDK non trovato.",
                    "Per rigiocare gli script serve il .NET SDK 8 o successivo (dotnet.microsoft.com). Non serve per eseguire i test con MarkAgent.", false), null);

            var (exit, output) = await RunAsync(dotnet, new[] { "--list-sdks" }, TimeSpan.FromSeconds(30), ct);
            var majors = Regex.Matches(output ?? "", @"^(\d+)\.", RegexOptions.Multiline).Select(m => int.Parse(m.Groups[1].Value)).ToList();
            if (exit != 0 || !majors.Any(m => m >= 8))
                return (new E2eRequirement("dotnet", false, $".NET SDK 8 o successivo non installato (trovati: {(majors.Count == 0 ? "nessuno" : string.Join(", ", majors.Distinct()))}).",
                    "Installa il .NET SDK 8 o successivo per rigiocare gli script.", false), null);
            return (new E2eRequirement("dotnet", true, $".NET SDK {majors.Max()} ({dotnet})", null, false), dotnet);
        }

        // ---------------------------------------------------------------- processes

        private static Task<(int Exit, string Output)> RunAsNodeAsync(string electronPath, IEnumerable<string> args, TimeSpan timeout, CancellationToken ct)
            => RunAsync(electronPath, args, timeout, ct, asNode: true);

        private static async Task<(int Exit, string Output)> RunAsync(string executable, IEnumerable<string> args, TimeSpan timeout, CancellationToken ct, bool asNode = false)
        {
            var start = new ProcessStartInfo(executable)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var a in args) start.ArgumentList.Add(a);
            if (asNode) start.Environment["ELECTRON_RUN_AS_NODE"] = "1";

            using var process = new Process { StartInfo = start };
            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                return (-1, ex.Message);
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            var stdout = process.StandardOutput.ReadToEndAsync(cts.Token);
            var stderr = process.StandardError.ReadToEndAsync(cts.Token);
            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* already gone */ }
                if (ct.IsCancellationRequested) throw;
                return (-1, $"nessuna risposta entro {timeout.TotalSeconds:0} secondi");
            }
            return (process.ExitCode, ((await stdout) + (await stderr)).Trim());
        }

        private static string Tail(string text) =>
            string.IsNullOrEmpty(text) ? "" : text.Length <= 600 ? text : "…" + text.Substring(text.Length - 600);
    }
}
