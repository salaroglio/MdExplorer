using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Features.Services.AI.Codex;
using MdExplorer.Service;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Le opzioni delle sessioni Codex di MdExplorer, in un posto solo: il server MCP <c>mdexplorer</c> coi gruppi del
    /// progetto, le regole con cui MdExplorer risponde alle richieste di approvazione, il web. Gemello di
    /// <see cref="ClaudeCodeMcp.ChatOptions"/>.
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F2 (D5, D6, D7).</para>
    /// </summary>
    public static class CodexChat
    {
        /// <summary>
        /// Il server MCP di MdExplorer per una sessione Codex, o null (e lo si dice a log) quando l'eseguibile non c'è.
        /// </summary>
        /// <param name="mcpGroupsArgument">I gruppi del progetto (<c>--groups</c>), null = tutti.</param>
        /// <param name="environment">Valori d'ambiente del server: il RunToken e l'identità git di un agente.</param>
        public static CodexMcpServer MdExplorerServer(string mcpGroupsArgument, IReadOnlyDictionary<string, string> environment = null,
            IReadOnlyList<string> disabledTools = null)
        {
            var executable = ProjectsManager.ResolveMcpExecutable(AppDomain.CurrentDomain.BaseDirectory);
            if (executable == null)
            {
                Console.WriteLine("[CodexChat] MdExplorer.Mcp non trovato: la sessione Codex parte senza gli strumenti di MdExplorer.");
                return null;
            }
            return new CodexMcpServer
            {
                Name = ClaudeCodeMcp.ServerName,
                Command = executable,
                Args = string.IsNullOrWhiteSpace(mcpGroupsArgument) ? Array.Empty<string>() : new[] { "--groups", mcpGroupsArgument },
                Env = environment,
                DisabledTools = disabledTools,
            };
        }

        /// <summary>
        /// La chat del tab MarkAgent: scrive nella cartella del progetto (<c>workspace-write</c>), ogni comando e modifica
        /// passano dalle regole di <see cref="ApproveForChat"/>, il web segue la rotella del progetto.
        /// </summary>
        public static CodexSessionOptions ChatOptions(string mcpGroupsArgument, IReadOnlyList<string> codeRoots = null, bool webAccess = false)
        {
            var server = MdExplorerServer(mcpGroupsArgument);
            var roots = codeRoots ?? Array.Empty<string>();
            return new CodexSessionOptions
            {
                Sandbox = CodexSandbox.WorkspaceWrite,
                Approve = ApproveForChat(roots),
                McpServers = server == null ? Array.Empty<CodexMcpServer>() : new[] { server },
                WebSearch = webAccess,
                McpGroups = mcpGroupsArgument ?? string.Empty,
                ExtraRootsKey = string.Join("\n", roots),
            };
        }

        /// <summary>
        /// Le regole della chat per le richieste di approvazione di Codex:
        /// <list type="bullet">
        /// <item><description>niente <c>ssh</c>/<c>scp</c>/<c>sftp</c>/<c>rsync</c>: i file delle radici remote passano da MDE Remote, che
        /// tiene la codifica (come per gli altri motori, <see cref="RemoteCopyShellRules"/>);</description></item>
        /// <item><description>niente modifiche native dentro una radice di codice collegata: si cambiano con <c>remote_edit</c>/<c>remote_write</c>,
        /// che passano dal cancello di MDE e ne tengono l'originale per la revisione (sprint 2026-11-27, D4). Il sandbox
        /// <c>workspace-write</c> le negherebbe comunque, perché le radici stanno fuori dalla cartella del progetto;</description></item>
        /// <item><description>il resto sì: è la parità con gli altri motori nella chat.</description></item>
        /// </list>
        /// </summary>
        public static Func<CodexApprovalRequest, CodexApprovalDecision> ApproveForChat(IReadOnlyList<string> codeRoots)
        {
            var roots = (codeRoots ?? Array.Empty<string>()).Where(r => !string.IsNullOrWhiteSpace(r)).Select(FullDirectory).ToList();
            return request =>
            {
                if (request.Kind == CodexApprovalRequest.KindCommand && RemoteCopyShellRules.IsRemoteCopy(request.Command))
                    return CodexApprovalDecision.No(UiText.Say(
                        "ssh/scp/sftp/rsync non si usano: i file delle radici remote si leggono e si cambiano con gli strumenti remote_* di MdExplorer.",
                        "ssh/scp/sftp/rsync are not used: files of remote roots are read and changed with MdExplorer's remote_* tools."));
                if (request.Kind == CodexApprovalRequest.KindFileChange)
                {
                    var inRoot = request.Paths.FirstOrDefault(p => roots.Any(r => IsInside(p, r)));
                    if (inRoot != null)
                        return CodexApprovalDecision.No(UiText.Say(
                            $"{inRoot} sta in una radice di codice collegata: si cambia con remote_edit o remote_write, che ne tengono l'originale per la revisione.",
                            $"{inRoot} is in a linked code root: change it with remote_edit or remote_write, which keep its original for the review."));
                }
                return CodexApprovalDecision.Yes;
            };
        }

        private static string FullDirectory(string path)
        {
            var full = Path.GetFullPath(path);
            return full.EndsWith(Path.DirectorySeparatorChar) ? full : full + Path.DirectorySeparatorChar;
        }

        private static bool IsInside(string path, string directory)
        {
            try
            {
                return Path.GetFullPath(path).StartsWith(directory, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Il server MCP di MdExplorer per il Codex che l'utente apre nel terminale: <c>[mcp_servers.mdexplorer]</c> nel
    /// <c>config.toml</c> di <c>$CODEX_HOME</c>, scritto con il writer di Codex (<c>config/value/write</c> dell'app-server), che
    /// conserva il resto del file, come <c>claude mcp add</c> per Claude Code. Con <c>env_vars</c> = <c>DOTNET_ROOT</c>: senza,
    /// Codex avvia il server .NET in un ambiente ripulito e quello muore (misurato il 28/11).
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F5.</para>
    /// </summary>
    public static class CodexMcp
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

        /// <summary>Registra (o aggiorna) il server per l'utente, e lo dice a log. Niente scrittura se è già com'è.</summary>
        public static void RegisterInstallationForUser(string mcpGroupsArgument)
        {
            var server = CodexChat.MdExplorerServer(mcpGroupsArgument);
            if (server == null) return;
            if (!CodexProcessLauncher.IsResolvable())
            {
                Console.WriteLine("[CodexMcp] Codex non è nel PATH: il server MCP di MdExplorer non viene registrato per il terminale.");
                return;
            }
            try
            {
                var outcome = RegisterAsync(server).GetAwaiter().GetResult();
                Console.WriteLine($"[CodexMcp] {outcome}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CodexMcp] registrazione del server MCP di MdExplorer non riuscita: {ex.Message}");
            }
        }

        /// <summary>Il valore di <c>mcp_servers.mdexplorer</c> che MdExplorer scrive. Pubblico per i test.</summary>
        public static System.Text.Json.Nodes.JsonObject Entry(CodexMcpServer server) => new()
        {
            ["command"] = server.Command,
            ["args"] = new System.Text.Json.Nodes.JsonArray(server.Args.Select(a => (System.Text.Json.Nodes.JsonNode)a).ToArray()),
            ["env_vars"] = new System.Text.Json.Nodes.JsonArray(CodexSessionOptions.McpForwardedVariables.Select(v => (System.Text.Json.Nodes.JsonNode)v).ToArray()),
        };

        public static async System.Threading.Tasks.Task<string> RegisterAsync(CodexMcpServer server, IReadOnlyDictionary<string, string> environment = null)
        {
            using var cts = new System.Threading.CancellationTokenSource(Timeout);
            await using var app = new CodexAppServer(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, Path.GetTempPath(), environment);
            await app.StartAsync(cts.Token).ConfigureAwait(false);

            var entry = Entry(server);
            var read = await app.RequestAsync("config/read", new { }, cts.Token).ConfigureAwait(false);
            if (read.TryGetProperty("config", out var config) && config.TryGetProperty("mcp_servers", out var servers)
                && servers.ValueKind == System.Text.Json.JsonValueKind.Object && servers.TryGetProperty(ClaudeCodeMcp.ServerName, out var current)
                && Same(current, entry))
                return $"server MCP '{ClaudeCodeMcp.ServerName}' già registrato in {app.CodexHome}: niente da scrivere";

            await app.RequestAsync("config/value/write", new
            {
                keyPath = "mcp_servers." + ClaudeCodeMcp.ServerName,
                mergeStrategy = "replace",
                value = entry,
            }, cts.Token).ConfigureAwait(false);
            return $"server MCP '{ClaudeCodeMcp.ServerName}' registrato in {app.CodexHome}/config.toml ({server.Command} {string.Join(" ", server.Args)})";
        }

        private static bool Same(System.Text.Json.JsonElement current, System.Text.Json.Nodes.JsonObject wanted)
        {
            string Text(System.Text.Json.JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String ? v.GetString() : null;
            string[] List(System.Text.Json.JsonElement e, string name) => e.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.Array
                ? v.EnumerateArray().Select(x => x.GetString()).ToArray() : Array.Empty<string>();
            return Text(current, "command") == (string)wanted["command"]
                && List(current, "args").SequenceEqual(wanted["args"].AsArray().Select(a => (string)a))
                && List(current, "env_vars").SequenceEqual(wanted["env_vars"].AsArray().Select(a => (string)a));
        }
    }
}
