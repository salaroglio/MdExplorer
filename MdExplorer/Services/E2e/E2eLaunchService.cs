using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Features.E2e;
using MdExplorer.Features.Services.AI.ClaudeCode;
using MdExplorer.Features.Services.AI.CopilotChat;
using MdExplorer.Features.Services.AI.OpenCode;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.E2e
{
    /// <summary>Where a launch reports what happens: the hub turns these into SignalR messages.</summary>
    public sealed class E2eLaunchSink
    {
        /// <summary>A structured event (plan, refused, prerequisites, test-start, test-end, done).</summary>
        public Func<object, Task> Event { get; init; }

        /// <summary>A piece of the agent's answer.</summary>
        public Func<string, Task> Chunk { get; init; }

        /// <summary>A status line about a tool the agent is using (click, screenshot, …).</summary>
        public Func<string, Task> Tool { get; init; }
    }

    public sealed class E2eLaunchRequest
    {
        public string ConnectionId { get; init; }
        public string ProjectPath { get; init; }

        /// <summary>A <c>.e2e.md</c> or a folder, absolute or relative to the project.</summary>
        public string Target { get; init; }

        public ProviderType Engine { get; init; }
        public string ModelId { get; init; }
        public string McpGroupsArgument { get; init; }
    }

    /// <summary>
    /// Runs the e2e tests of a file or a folder with the MarkAgent engine (F4): plan and checks (F4a),
    /// prerequisites (F3), the secrets file of the launch, the Playwright server and the bans for the
    /// engine, then one two-line prompt per test file — in the MarkAgent tab session or in a session of its
    /// own, as the settings say (D21, D25). The skill mde-e2e does the rest.
    /// </summary>
    public sealed class E2eLaunchService
    {
        private readonly E2eEnvironment _environment;
        private readonly ClaudeCodeSessionPool _claudePool;
        private readonly CopilotChatSessionPool _copilotPool;
        private readonly OpenCodeSessionPool _openCodePool;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<E2eLaunchService> _logger;

        public E2eLaunchService(E2eEnvironment environment, ClaudeCodeSessionPool claudePool, CopilotChatSessionPool copilotPool,
            OpenCodeSessionPool openCodePool, ILoggerFactory loggerFactory, ILogger<E2eLaunchService> logger)
        {
            _environment = environment;
            _claudePool = claudePool;
            _copilotPool = copilotPool;
            _openCodePool = openCodePool;
            _loggerFactory = loggerFactory;
            _logger = logger;
        }

        public async Task RunAsync(E2eLaunchRequest request, E2eLaunchSink sink, CancellationToken ct)
        {
            var target = Path.IsPathRooted(request.Target) ? request.Target : Path.Combine(request.ProjectPath, request.Target);
            var plan = E2eRunPlanner.Plan(target, request.ProjectPath, DateTime.Now);

            await sink.Event(new
            {
                type = "plan",
                errors = plan.Errors,
                warnings = plan.Warnings,
                items = plan.Items.Select(i => new
                {
                    file = i.RelativeTestFile,
                    runFolder = i.RelativeRunFolder,
                    dedicatedSession = i.Settings.DedicatedSession,
                    commitAfterRun = i.Settings.CommitAfterRun,
                    headless = i.Settings.Headless,
                }),
            });
            if (!plan.CanRun)
            {
                await sink.Event(new { type = "refused", errors = plan.Errors });
                return;
            }

            if (request.Engine is not (ProviderType.ClaudeCode or ProviderType.CopilotCli or ProviderType.OpenCode))
            {
                await sink.Event(new
                {
                    type = "refused",
                    errors = new[] { $"I test e2e si eseguono con Claude Code, Copilot o opencode: il motore {request.Engine} non sa guidare un browser." },
                });
                return;
            }

            var prerequisites = await _environment.CheckAsync(ct);
            if (!prerequisites.ReadyToRun)
            {
                await sink.Event(new { type = "prerequisites", report = prerequisites });
                return;
            }

            var dataFolder = DataFolder();
            var diagnostics = Path.Combine(dataFolder, "diagnostica", Hash(request.ProjectPath));
            Directory.CreateDirectory(diagnostics);

            string secretsFile = null;
            if (plan.Secrets.Count > 0)
            {
                var content = string.Join("\n", plan.Secrets.OrderBy(k => k.Key, StringComparer.Ordinal).Select(k => k.Key + "=" + k.Value));
                secretsFile = Path.Combine(dataFolder, "segreti", Hash(content) + ".env");
                E2eRunPlanner.WriteSecretsFile(secretsFile, plan.Secrets);
            }

            var deniedFiles = plan.DeniedPaths.Concat(secretsFile == null ? Array.Empty<string>() : new[] { secretsFile }).ToList();
            var bans = BannedFor(deniedFiles);

            var index = 0;
            var tabUsed = false;
            try
            {
            foreach (var item in plan.Items)
            {
                ct.ThrowIfCancellationRequested();
                index++;
                Directory.CreateDirectory(item.RunFolder);

                var server = E2ePlaywrightServer.For(prerequisites, item.Settings.Headless.Value, secretsFile, diagnostics);
                var dedicated = item.Settings.DedicatedSession.Value;
                tabUsed |= !dedicated;
                var key = dedicated ? request.ConnectionId + "|e2e|" + Guid.NewGuid().ToString("N") : request.ConnectionId;
                await sink.Event(new { type = "test-start", file = item.RelativeTestFile, index, total = plan.Items.Count, session = dedicated ? "dedicated" : "tab" });

                var answer = new StringBuilder();
                async Task Forward(string kind, string text)
                {
                    if (kind == "message")
                    {
                        answer.Append(text);
                        await sink.Chunk(text);
                    }
                    else if (kind == "tool")
                    {
                        await sink.Tool(text);
                    }
                }

                if (request.Engine == ProviderType.ClaudeCode)
                    await RunWithClaudeAsync(request, item, server, bans, key, dedicated, Forward, ct);
                else if (request.Engine == ProviderType.CopilotCli)
                    await RunWithCopilotAsync(request, item, server, deniedFiles, key, dedicated, Forward, ct);
                else
                    await RunWithOpenCodeAsync(request, item, server, deniedFiles, dedicated, Forward, ct);

                _logger.LogInformation("[E2e] {File}: eseguito ({Index}/{Total})", item.RelativeTestFile, index, plan.Items.Count);
                await sink.Event(new { type = "test-end", file = item.RelativeTestFile, runFolder = item.RelativeRunFolder, answer = answer.ToString() });
            }

            }
            finally
            {
                // D28: the tab gets its shell back and loses Playwright, keeping the conversation — also
                // when the launch stops halfway (error, Stop).
                if (tabUsed && request.Engine == ProviderType.ClaudeCode)
                    await _claudePool.RestoreChatAsync(request.ConnectionId, ClaudeCodeMcp.ChatOptions(request.McpGroupsArgument), CancellationToken.None);
                if (tabUsed && request.Engine == ProviderType.CopilotCli)
                    await _copilotPool.RestoreChatAsync(request.ConnectionId, CancellationToken.None);
            }

            await sink.Event(new { type = "done", files = plan.Items.Select(i => i.RelativeTestFile) });
        }

        private async Task RunWithClaudeAsync(E2eLaunchRequest request, E2eRunItem item, E2ePlaywrightServer server,
            IReadOnlyList<string> bans, string key, bool dedicated, Func<string, string, Task> forward, CancellationToken ct)
        {
            var mcpConfig = ClaudeCodeMcp.WriteSessionConfig(request.McpGroupsArgument, server);
            var options = new ClaudeCodeSessionOptions
            {
                McpConfigPath = mcpConfig,
                AllowedMcpServers = new[] { ClaudeCodeMcp.ServerName, E2ePlaywrightServer.Name },
                DisallowedTools = bans,
                // The skill writes the report, the scripts, the results and the site map: only
                // inside the project (the session's working directory).
                AllowedTools = new[] { "Edit(./**)", "Write(./**)" },
                ProfileKey = Path.GetFileName(mcpConfig) + "|" + Hash(string.Join("\n", bans)),
            };
            try
            {
                var session = await _claudePool.GetOrCreateAsync(key, request.ProjectPath, request.ModelId, options, ct);
                await foreach (var chunk in session.PromptAsync(item.Prompt, ct))
                    await forward(chunk.Kind == ClaudeCodeChunk.KindMessage ? "message" : chunk.Kind == ClaudeCodeChunk.KindTool ? "tool" : chunk.Kind, chunk.Text);
            }
            finally
            {
                if (dedicated) await _claudePool.ReleaseAsync(key);
            }
        }

        /// <summary>
        /// Copilot SDK (F4c): MdExplorer's and Playwright's MCP servers declared in the session itself, no
        /// shell (D29), no reading of the credentials and secrets files (the permission callback).
        /// </summary>
        private async Task RunWithCopilotAsync(E2eLaunchRequest request, E2eRunItem item, E2ePlaywrightServer server,
            IReadOnlyList<string> deniedFiles, string key, bool dedicated, Func<string, string, Task> forward, CancellationToken ct)
        {
            var servers = new Dictionary<string, CopilotMcpServer>
            {
                [E2ePlaywrightServer.Name] = new CopilotMcpServer(server.Command, server.Args, server.Env),
            };
            var mcpExecutable = MdExplorer.Service.ProjectsManager.ResolveMcpExecutable(AppDomain.CurrentDomain.BaseDirectory);
            if (mcpExecutable != null)
            {
                servers[ClaudeCodeMcp.ServerName] = new CopilotMcpServer(mcpExecutable,
                    string.IsNullOrWhiteSpace(request.McpGroupsArgument) ? Array.Empty<string>() : new[] { "--groups", request.McpGroupsArgument },
                    new Dictionary<string, string>());
            }
            else
            {
                _logger.LogWarning("[E2e] MdExplorer.Mcp non trovato: la sessione Copilot dei test parte senza gli strumenti di MdExplorer");
            }

            var profile = new CopilotSessionProfile
            {
                McpServers = servers,
                DeniedReadPaths = deniedFiles,
                DenyShell = true,
                Key = Hash(string.Join("\n", servers.OrderBy(s => s.Key).Select(s => s.Key + " " + s.Value.Command + " " + string.Join(" ", s.Value.Args)))
                    + "\n" + string.Join("\n", deniedFiles)),
            };
            try
            {
                var session = await _copilotPool.GetOrCreateAsync(key, request.ProjectPath, request.ModelId, profile, ct);
                await foreach (var chunk in session.PromptAsync(item.Prompt, ct))
                    await forward(chunk.Kind, chunk.Text);
            }
            finally
            {
                if (dedicated) await _copilotPool.ReleaseAsync(key);
            }
        }

        /// <summary>
        /// opencode (F4d): a server of its own for the launch, configured only through
        /// <c>OPENCODE_CONFIG_CONTENT</c> — MdExplorer's and Playwright's MCP servers, the shell on "ask" and
        /// every ask rejected (the free provider refuses requests where the shell is removed, verified
        /// 27/09/2026), the credentials and secrets files not readable. In the MarkAgent tab it works on the
        /// tab's own conversation: opencode keeps sessions in a database every server shares, and the shared
        /// server never gets Playwright, so D28 holds by itself.
        /// </summary>
        private async Task RunWithOpenCodeAsync(E2eLaunchRequest request, E2eRunItem item, E2ePlaywrightServer server,
            IReadOnlyList<string> deniedFiles, bool dedicated, Func<string, string, Task> forward, CancellationToken ct)
        {
            var mcp = new System.Text.Json.Nodes.JsonObject
            {
                [E2ePlaywrightServer.Name] = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "local",
                    ["command"] = new System.Text.Json.Nodes.JsonArray(new[] { server.Command }.Concat(server.Args).Select(a => (System.Text.Json.Nodes.JsonNode)a).ToArray()),
                    ["environment"] = new System.Text.Json.Nodes.JsonObject(server.Env.Select(e => KeyValuePair.Create(e.Key, (System.Text.Json.Nodes.JsonNode)e.Value))),
                    ["enabled"] = true,
                },
            };
            var mcpExecutable = MdExplorer.Service.ProjectsManager.ResolveMcpExecutable(AppDomain.CurrentDomain.BaseDirectory);
            if (mcpExecutable != null)
            {
                var command = new List<string> { mcpExecutable };
                if (!string.IsNullOrWhiteSpace(request.McpGroupsArgument)) command.AddRange(new[] { "--groups", request.McpGroupsArgument });
                mcp[ClaudeCodeMcp.ServerName] = new System.Text.Json.Nodes.JsonObject
                {
                    ["type"] = "local",
                    ["command"] = new System.Text.Json.Nodes.JsonArray(command.Select(a => (System.Text.Json.Nodes.JsonNode)a).ToArray()),
                    ["enabled"] = true,
                };
            }

            var read = new System.Text.Json.Nodes.JsonObject { ["*"] = "allow" };
            foreach (var file in deniedFiles) read["*" + Path.GetFileName(file)] = "deny";
            var config = new System.Text.Json.Nodes.JsonObject
            {
                // Without it a rejected permission ends the whole turn: the test stopped at the agent's
                // first shell command (seen 27/09/2026). With it the refusal goes back to the model.
                ["experimental"] = new System.Text.Json.Nodes.JsonObject { ["continue_loop_on_deny"] = true },
                ["mcp"] = mcp,
                ["permission"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["bash"] = "ask",
                    ["edit"] = "allow",
                    ["webfetch"] = "deny",
                    ["read"] = read,
                },
            };

            using var e2eServer = new OpenCodeServer(_loggerFactory.CreateLogger<OpenCodeServer>(),
                new Dictionary<string, string> { ["OPENCODE_CONFIG_CONTENT"] = config.ToJsonString() });

            string existing = null;
            if (!dedicated)
            {
                var tab = await _openCodePool.GetOrCreateAsync(request.ConnectionId, request.ProjectPath, request.ModelId, ct);
                existing = await tab.EnsureStartedAsync(ct);
            }

            await using var session = new OpenCodeSession(_loggerFactory.CreateLogger<OpenCodeSession>(), e2eServer,
                request.ProjectPath, request.ModelId, existing, rejectPermissions: true);
            await foreach (var chunk in session.PromptAsync(item.Prompt, ct))
                await forward(chunk.Kind, chunk.Text);
        }

        /// <summary>
        /// What the agent must not do during a launch: run shell commands (a <c>cat</c> would read the
        /// credentials past any file rule) and read or search the credentials and secrets files. Claude Code
        /// writes an absolute path as <c>//path</c> (verified 27/09/2026 on Linux).
        /// </summary>
        public static IReadOnlyList<string> BannedFor(IEnumerable<string> files)
        {
            var bans = new List<string> { "Bash" };
            foreach (var file in files)
            {
                var path = "/" + file.Replace('\\', '/').TrimStart('/');
                // TODO F8: the form for a Windows path (//C:/…) is still to be verified on Windows.
                bans.Add($"Read(/{path})");
                bans.Add($"Grep(/{path})");
            }
            return bans;
        }

        private static string DataFolder()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(appData))
                throw new InvalidOperationException("Cartella dati dell'utente non disponibile: non posso preparare segreti e diagnostica dei test e2e.");
            return Path.Combine(appData, "MdExplorer", "e2e");
        }

        private static string Hash(string text) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).Substring(0, 12).ToLowerInvariant();
    }
}
