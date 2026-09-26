using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MdExplorer.Features.Services.AI.ClaudeCode;
using MdExplorer.Service;

namespace MdExplorer.Utilities
{
    public enum ClaudeMcpRegistrationOutcome
    {
        Registered,
        AlreadyRegistered,
        /// <summary>An entry pointing somewhere else (an installation that moved) was replaced.</summary>
        Replaced,
        ClaudeNotInstalled,
        McpExecutableNotFound,
        Failed,
    }

    public sealed record ClaudeMcpRegistrationResult(ClaudeMcpRegistrationOutcome Outcome, string Message);

    /// <summary>
    /// MdExplorer's MCP server for Claude Code, in the two places it is needed.
    /// <list type="bullet">
    /// <item><description><see cref="RegisterInstallationForUser"/> — for a project whose harness is Claude Code: the
    /// server in the user's own Claude Code configuration, so <c>claude</c> in a terminal has MdExplorer's
    /// tools. User scope and not the project's <c>.mcp.json</c>, for the reason Copilot and opencode are
    /// registered globally: the entry holds the absolute path of THIS installation's executable, noise or a
    /// broken path for everyone else on the team.</description></item>
    /// <item><description><see cref="WriteSessionConfig()"/> — for every MarkAgent Claude Code session,
    /// whatever the harness: a file handed to <c>--mcp-config</c>. Before, the chat never had these
    /// tools.</description></item>
    /// </list>
    /// <para>
    /// The user configuration is only READ here (to see what is registered); it is WRITTEN by Claude Code's
    /// own CLI, <c>claude mcp add --scope user</c>, which merges the entry and keeps every other key and
    /// server (measured 13/09/2026). <c>claude mcp get</c> is not used: its output is for people, and it
    /// launches the server to check its health.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-Harness-Claude-Code.md, phase F2.</para>
    /// </summary>
    public static class ClaudeCodeMcp
    {
        public const string ServerName = "mdexplorer";

        /// <summary>
        /// Claude Code's own variable: when set, its user configuration is
        /// <c>$CLAUDE_CONFIG_DIR/.claude.json</c> instead of <c>~/.claude.json</c> (measured 13/09/2026).
        /// </summary>
        public const string ConfigDirVariable = "CLAUDE_CONFIG_DIR";

        private const string SessionConfigFileName = "claude-code-mcp.json";
        private const int CliTimeoutMs = 60_000;

        /// <summary>Where Claude Code keeps the user configuration holding <c>mcpServers</c>.</summary>
        public static string UserConfigFilePath()
        {
            var configDir = Environment.GetEnvironmentVariable(ConfigDirVariable);
            return !string.IsNullOrWhiteSpace(configDir)
                ? Path.Combine(configDir, ".claude.json")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".claude.json");
        }

        /// <summary>Command of the <c>mdexplorer</c> entry in the user configuration, or null when there is none.</summary>
        public static string RegisteredCommand() => ReadRegisteredEntry().Command;

        /// <summary>
        /// Registers this installation's MCP executable for the user, and logs the outcome.
        /// <para>
        /// Nome diverso da <see cref="RegisterForUser(string, string)"/> e non un overload: due metodi
        /// <c>(string)</c> che vogliono cose diverse — un eseguibile e un elenco di gruppi — si
        /// scambiano in silenzio al primo chiamante distratto (successo per davvero, il 22/09/2026,
        /// finché i test non l'hanno detto).
        /// </para>
        /// </summary>
        /// <param name="mcpGroupsArgument">
        /// I gruppi di funzionalità da esporre (<c>--groups</c>), o null per tutti. ⚠️ Questa è una
        /// registrazione <b>utente</b>, non di progetto: porta i gruppi dell'ultimo progetto aperto,
        /// ed è quello che vede un <c>claude</c> lanciato a mano in un terminale.
        /// </param>
        public static ClaudeMcpRegistrationResult RegisterInstallationForUser(string mcpGroupsArgument = null)
        {
            var result = RegisterForUser(ProjectsManager.ResolveMcpExecutable(AppDomain.CurrentDomain.BaseDirectory),
                                         mcpGroupsArgument);
            Console.WriteLine($"[ClaudeCodeMcp] {result.Outcome}: {result.Message}");
            return result;
        }

        /// <param name="mcpExecutable">
        /// Absolute path of <c>MdExplorer.Mcp</c>. Only a direct executable: the <c>dotnet run</c> form is not
        /// offered, for the reasons given on <c>ProjectsManager.ResolveMcpExecutable</c>.
        /// </param>
        public static ClaudeMcpRegistrationResult RegisterForUser(string mcpExecutable, string mcpGroupsArgument = null)
        {
            if (string.IsNullOrWhiteSpace(mcpExecutable))
            {
                return new(ClaudeMcpRegistrationOutcome.McpExecutableNotFound,
                    "MdExplorer.Mcp non trovato accanto al servizio né nella sua build: l'MCP di MdExplorer non è " +
                    "registrato per Claude Code. Ricompila o reinstalla MdExplorer e riapri il progetto.");
            }
            if (!ClaudeCodeProcessLauncher.IsResolvable())
            {
                return new(ClaudeMcpRegistrationOutcome.ClaudeNotInstalled,
                    "Claude Code CLI non trovato nel PATH: l'MCP di MdExplorer non è registrato per Claude Code. " +
                    "Installa Claude Code e riapri il progetto.");
            }

            var configPath = UserConfigFilePath();
            var wantedArguments = string.IsNullOrWhiteSpace(mcpGroupsArgument)
                ? Array.Empty<string>()
                : new[] { "--groups", mcpGroupsArgument };

            var existing = ReadRegisteredEntry();
            // Gli argomenti fanno parte di cosa è registrato: una voce giusta ma con gruppi vecchi
            // NON è "già registrata", altrimenti la scelta dei gruppi non arriverebbe mai a chi ha
            // già la voce nel suo file.
            if (existing.Command != null && SamePath(existing.Command, mcpExecutable)
                && existing.Arguments.SequenceEqual(wantedArguments, StringComparer.Ordinal))
            {
                return new(ClaudeMcpRegistrationOutcome.AlreadyRegistered, $"'{ServerName}' già registrato in {configPath}.");
            }

            var replacing = existing.Command != null;
            if (replacing)
            {
                var remove = RunClaude($"mcp remove --scope user {ServerName}");
                if (remove.ExitCode != 0)
                {
                    return new(ClaudeMcpRegistrationOutcome.Failed,
                        $"`claude mcp remove` fallito (exit {remove.ExitCode}): {remove.Output}");
                }
            }

            var addArguments = wantedArguments.Length == 0
                ? Quote(mcpExecutable)
                : $"{Quote(mcpExecutable)} {string.Join(" ", wantedArguments.Select(Quote))}";
            var add = RunClaude($"mcp add --scope user {ServerName} -- {addArguments}");
            if (add.ExitCode != 0)
            {
                return new(ClaudeMcpRegistrationOutcome.Failed,
                    $"`claude mcp add` fallito (exit {add.ExitCode}): {add.Output}");
            }

            // The CLI's exit code is not the proof: the file is.
            var now = ReadRegisteredEntry();
            if (!SamePath(now.Command, mcpExecutable))
            {
                return new(ClaudeMcpRegistrationOutcome.Failed,
                    $"`claude mcp add` ha risposto 0, ma in {configPath} '{ServerName}' punta a '{now.Command ?? "(niente)"}'.");
            }

            return replacing
                ? new(ClaudeMcpRegistrationOutcome.Replaced, $"'{ServerName}' spostato da '{existing.Command}' a '{mcpExecutable}' in {configPath}.")
                : new(ClaudeMcpRegistrationOutcome.Registered, $"'{ServerName}' registrato in {configPath}: {mcpExecutable}.");
        }

        /// <summary>
        /// The <c>--mcp-config</c> file for a MarkAgent Claude Code session, in MdExplorer's own data folder
        /// (never in the project). Null — and said — when the MCP executable cannot be found: the session
        /// then starts without MdExplorer's tools.
        /// </summary>
        public static string WriteSessionConfig(string mcpGroupsArgument = null)
        {
            var mcpExecutable = ProjectsManager.ResolveMcpExecutable(AppDomain.CurrentDomain.BaseDirectory);
            if (mcpExecutable == null)
            {
                Console.WriteLine("[ClaudeCodeMcp] MdExplorer.Mcp non trovato: la sessione Claude Code parte senza gli strumenti di MdExplorer.");
                return null;
            }

            // An empty string when the folder does not exist (seen on Linux with a missing XDG_CONFIG_HOME):
            // writing "MdExplorer/..." relative to the current directory would land in a random place.
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(appData))
            {
                Console.WriteLine("[ClaudeCodeMcp] Cartella dati dell'utente non disponibile: la sessione Claude Code parte senza gli strumenti di MdExplorer.");
                return null;
            }

            return WriteSessionConfig(mcpExecutable, Path.Combine(appData, "MdExplorer"), mcpGroupsArgument);
        }

        /// <summary>Writes <c>{ "mcpServers": { "mdexplorer": … } }</c> in <paramref name="directory"/>; returns its path.</summary>
        public static string WriteSessionConfig(string mcpExecutable, string directory, string mcpGroupsArgument = null)
        {
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, SessionConfigFileName);

            var config = new JsonObject
            {
                ["mcpServers"] = new JsonObject
                {
                    [ServerName] = new JsonObject
                    {
                        ["type"] = "stdio",
                        ["command"] = mcpExecutable,
                        // I gruppi di funzionalita' MCP di QUESTO progetto: il file si riscrive a
                        // ogni sessione di chat, quindi la scelta vale dalla prossima in poi.
                        ["args"] = string.IsNullOrWhiteSpace(mcpGroupsArgument)
                            ? new JsonArray()
                            : new JsonArray("--groups", mcpGroupsArgument),
                    },
                },
            };

            // Atomic: two sessions starting together must not read a half-written file.
            var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temp, config.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temp, path, overwrite: true);
            return path;
        }

        private static (string Command, string[] Arguments) ReadRegisteredEntry()
        {
            var path = UserConfigFilePath();
            if (!File.Exists(path)) return (null, Array.Empty<string>());
            try
            {
                // Shared read: a running Claude Code may be writing its own file right now.
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream);
                var entry = JsonNode.Parse(reader.ReadToEnd())?["mcpServers"]?[ServerName];
                if (entry == null) return (null, Array.Empty<string>());
                var arguments = (entry["args"] as JsonArray)?
                    .Select(a => a?.GetValue<string>() ?? string.Empty).ToArray() ?? Array.Empty<string>();
                return (entry["command"]?.GetValue<string>(), arguments);
            }
            catch (Exception ex) when (ex is JsonException || ex is IOException || ex is InvalidOperationException)
            {
                // Unreadable: it is not ours to repair. Registering again through the CLI is safe either way.
                Console.WriteLine($"[ClaudeCodeMcp] {path} non leggibile ({ex.Message}): lo considero senza '{ServerName}'.");
                return (null, Array.Empty<string>());
            }
        }

        private static (int ExitCode, string Output) RunClaude(string arguments)
        {
            var psi = ClaudeCodeProcessLauncher.BuildStartInfo(arguments);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;

            using var process = Process.Start(psi);
            if (process == null) return (-1, "processo non avviato");
            process.StandardInput.Close();
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(CliTimeoutMs))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* already gone */ }
                return (-1, $"nessuna risposta in {CliTimeoutMs / 1000} secondi");
            }
            return (process.ExitCode, (stdout.Result + " " + stderr.Result).Trim());
        }

        private static string Quote(string value) => value.IndexOf(' ') >= 0 ? "\"" + value + "\"" : value;

        private static bool SamePath(string a, string b)
            => a != null && b != null && string.Equals(a, b, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
}
