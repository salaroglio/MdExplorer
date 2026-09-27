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

        /// <summary>The hub connection went away: nothing to restore for it.</summary>
        public CancellationToken ConnectionAborted { get; init; }
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

            // One secrets file per launch, with a random name, deleted at the end: the values never stay on
            // disk outside the credentials file (review of 27/09/2026).
            string secretsFile = null;
            if (plan.Secrets.Count > 0)
            {
                secretsFile = Path.Combine(dataFolder, "segreti", Guid.NewGuid().ToString("N") + ".env");
                E2eRunPlanner.WriteSecretsFile(secretsFile, plan.Secrets);
            }

            var deniedFiles = plan.DeniedPaths.Concat(secretsFile == null ? Array.Empty<string>() : new[] { secretsFile }).ToList();
            var temporaryConfigs = new List<string>();

            var index = 0;
            var tabUsed = false;
            try
            {
                foreach (var item in plan.Items)
                {
                    ct.ThrowIfCancellationRequested();
                    index++;
                    Directory.CreateDirectory(item.RunFolder);
                    var runStart = DateTime.UtcNow;

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

                    try
                    {
                        if (request.Engine == ProviderType.ClaudeCode)
                            await RunWithClaudeAsync(request, item, server, deniedFiles, key, dedicated, Forward, temporaryConfigs, ct);
                        else if (request.Engine == ProviderType.CopilotCli)
                            await RunWithCopilotAsync(request, item, server, deniedFiles, key, dedicated, Forward, ct);
                        else
                            await RunWithOpenCodeAsync(request, item, server, deniedFiles, dedicated, Forward, ct);
                    }
                    finally
                    {
                        // F5 also after an error or a Stop: whatever the agent already wrote is checked.
                        await PostRunAsync(request, item, plan, runStart, sink);
                    }

                    _logger.LogInformation("[E2e] {File}: eseguito ({Index}/{Total})", item.RelativeTestFile, index, plan.Items.Count);
                    await sink.Event(new { type = "test-end", file = item.RelativeTestFile, runFolder = item.RelativeRunFolder, answer = answer.ToString() });

                    if (item.Settings.CommitAfterRun.Value)
                    {
                        // D21, D26: only what the run produced or touched, screenshots included.
                        E2eCommitResult commit;
                        try
                        {
                            commit = E2eCommitter.Commit(item);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[E2e] commit dopo {File}", item.RelativeTestFile);
                            commit = new E2eCommitResult(false, null, null, "il commit non è riuscito: " + ex.Message);
                        }
                        await sink.Event(new
                        {
                            type = "commit",
                            file = item.RelativeTestFile,
                            committed = commit.Committed,
                            sha = commit.Sha,
                            message = commit.Message,
                            reason = commit.Reason,
                        });
                    }
                }
            }
            finally
            {
                // D28: the tab gets its shell back and loses Playwright, keeping the conversation — also when
                // the launch stops halfway. Never for a connection that is gone (it would start an orphan CLI),
                // and a failed restore must not hide what happened: the next chat turn restores anyway.
                if (tabUsed && !request.ConnectionAborted.IsCancellationRequested)
                {
                    try
                    {
                        if (request.Engine == ProviderType.ClaudeCode)
                            await _claudePool.RestoreChatAsync(request.ConnectionId, ClaudeCodeMcp.ChatOptions(request.McpGroupsArgument), CancellationToken.None);
                        if (request.Engine == ProviderType.CopilotCli)
                            await _copilotPool.RestoreChatAsync(request.ConnectionId, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[E2e] ripristino della chat del tab non riuscito: lo farà il prossimo turno di chat");
                    }
                }
                foreach (var file in temporaryConfigs.Append(secretsFile).Where(f => f != null))
                {
                    try { File.Delete(file); } catch (Exception ex) { _logger.LogWarning(ex, "[E2e] non riesco a cancellare {File}", file); }
                }
            }

            await sink.Event(new { type = "done", files = plan.Items.Select(i => i.RelativeTestFile) });
        }

        private async Task PostRunAsync(E2eLaunchRequest request, E2eRunItem item, E2eRunPlan plan, DateTime runStart, E2eLaunchSink sink)
        {
            try
            {
                var post = E2ePostRun.Process(item, plan.Secrets, runStart);
                await sink.Event(new
                {
                    type = "post-run",
                    file = item.RelativeTestFile,
                    fingerprinted = post.Fingerprinted.Count,
                    leaks = post.Leaks.Select(l => new { file = Path.GetRelativePath(request.ProjectPath, l.File).Replace('\\', '/'), key = l.Key, replaced = l.Replaced }),
                    problems = post.Problems,
                });
                if (post.Leaks.Count > 0)
                    _logger.LogWarning("[E2e] {File}: valori di credenziali trovati in {Count} punti", item.RelativeTestFile, post.Leaks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[E2e] controlli dopo {File}", item.RelativeTestFile);
                await sink.Event(new { type = "post-run", file = item.RelativeTestFile, fingerprinted = 0, leaks = Array.Empty<object>(),
                    problems = new[] { "i controlli dopo l'esecuzione non sono riusciti: " + ex.Message } });
            }
        }

        private async Task RunWithClaudeAsync(E2eLaunchRequest request, E2eRunItem item, E2ePlaywrightServer server,
            IReadOnlyList<string> deniedFiles, string key, bool dedicated, Func<string, string, Task> forward,
            List<string> temporaryConfigs, CancellationToken ct)
        {
            var bans = BannedFor(request.ProjectPath, deniedFiles);
            var mcpConfig = ClaudeCodeMcp.WriteSessionConfig(request.McpGroupsArgument, server);
            temporaryConfigs.Add(mcpConfig);
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
                DeniedReadNames = new[] { CredentialsNamePattern },
                DeniedWritePaths = ProtectedPaths,
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

            System.Text.Json.Nodes.JsonObject Permissions()
            {
                var read = new System.Text.Json.Nodes.JsonObject { ["*"] = "allow", ["*" + CredentialsNamePattern] = "deny" };
                foreach (var file in deniedFiles) read["*" + Path.GetFileName(file)] = "deny";
                var edit = new System.Text.Json.Nodes.JsonObject { ["*"] = "allow" };
                foreach (var p in ProtectedPaths) edit[p.EndsWith("/", StringComparison.Ordinal) ? p + "*" : p] = "deny";
                return new System.Text.Json.Nodes.JsonObject
                {
                    ["bash"] = "ask",
                    ["edit"] = edit,
                    ["webfetch"] = "deny",
                    ["read"] = read,
                };
            }
            var config = new System.Text.Json.Nodes.JsonObject
            {
                // Without it a rejected permission ends the whole turn: the test stopped at the agent's
                // first shell command (seen 27/09/2026). With it the refusal goes back to the model.
                ["experimental"] = new System.Text.Json.Nodes.JsonObject { ["continue_loop_on_deny"] = true },
                ["mcp"] = mcp,
                ["permission"] = Permissions(),
                // Also on the default agent: a user's global or project configuration with an agent-level
                // "bash: allow" would win over the top-level permission. Accepted by the free provider (27/09/2026).
                ["agent"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["build"] = new System.Text.Json.Nodes.JsonObject { ["permission"] = Permissions() },
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
            try
            {
                await foreach (var chunk in session.PromptAsync(item.Prompt, ct))
                    await forward(chunk.Kind, chunk.Text);
            }
            catch (OperationCanceledException)
            {
                // Stop: the turn must end on THIS server before it goes away, or a tool call left pending in the
                // tab's session could be run later by the shared server, which has the shell.
                using var grace = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try { await session.AbortAsync(grace.Token); } catch (Exception ex) { _logger.LogWarning(ex, "[E2e] interruzione opencode"); }
                throw;
            }
        }

        /// <summary>The naming convention of the skill for credentials files: every one of them is off limits.</summary>
        public const string CredentialsNamePattern = "credenziali-*.txt";

        /// <summary>
        /// The agents' own configuration, relative to the project: written by an agent steered by a page, a
        /// hook or a setting would run with the shell in the next session (review of 27/09/2026).
        /// </summary>
        public static readonly IReadOnlyList<string> ProtectedPaths = new[]
        {
            ".claude/", ".github/", ".opencode/", ".vscode/", ".md/", "opencode.json", ".mcp.json", "CLAUDE.md", "AGENTS.md",
        };

        /// <summary>
        /// Claude Code rules for a launch: no shell (a <c>cat</c> would read the credentials past any file rule),
        /// no reading or searching of the credentials and secrets files — every <c>credenziali-*.txt</c> of the
        /// project, not only this launch's — and no writing of the agents' configuration. Files of the project
        /// are written relative to it (<c>./…</c>, the form verified on Linux, and the same on every system);
        /// a file outside it as <c>//path</c> (verified on Linux; on Windows still to verify, F8).
        /// </summary>
        public static IReadOnlyList<string> BannedFor(string projectPath, IEnumerable<string> files)
        {
            var bans = new List<string> { "Bash" };
            void Deny(string target)
            {
                bans.Add($"Read({target})");
                bans.Add($"Grep({target})");
            }
            Deny("./**/" + CredentialsNamePattern);
            foreach (var file in files)
            {
                if (E2eRunPlanner.IsInside(file, projectPath))
                    Deny("./" + Path.GetRelativePath(projectPath, file).Replace('\\', '/'));
                else
                    Deny("//" + Path.GetFullPath(file).Replace('\\', '/').TrimStart('/'));
            }
            foreach (var p in ProtectedPaths)
            {
                var target = p.EndsWith("/", StringComparison.Ordinal) ? "./" + p + "**" : "./" + p;
                bans.Add($"Edit({target})");
                bans.Add($"Write({target})");
            }
            return bans;
        }

        /// <summary>"mde-e2e v&lt;n&gt;" of the skill MdExplorer installs: scripts written by another version are stale.</summary>
        public static string CurrentGenerator()
        {
            using var stream = typeof(E2eLaunchService).Assembly.GetManifestResourceStream("MdExplorer.Service.skills.mde_e2e.SKILL.md");
            if (stream == null) return null;
            using var reader = new StreamReader(stream);
            var version = MdeSkillUpdater.ExtractMdeMarker(reader.ReadToEnd()).Version;
            return version == null ? null : "mde-e2e v" + version;
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
