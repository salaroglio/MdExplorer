using MdExplorer.Features.Services.AI.ClaudeCode;
using MdExplorer.Features.Services.AI.CopilotChat;
using MdExplorer.Features.Services.AI.OpenCode;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.LlmSessions
{
    /// <summary>
    /// The read-only configuration of a session of its own, for each of the three engines (sprint
    /// 2026-09-29-Motore-LLM-Unico): the diagram sessions (D8) and the folder summaries (D6) answer, MdExplorer
    /// writes. One place, so the three engines stay read-only the same way everywhere.
    /// </summary>
    public static class ReadOnlyEngineSessions
    {
        /// <summary>Claude Code: no execution, nothing written (dontAsk denies what is not granted; the bans say it too).</summary>
        public static ClaudeCodeSessionOptions ClaudeOptions(string profileKey, string resumeSessionId = null) => new()
        {
            ToolPolicy = ClaudeCodeToolPolicy.NoExecution,
            DisallowedTools = new[] { "Edit", "Write", "NotebookEdit", "Bash" },
            AllowedTools = Array.Empty<string>(),
            ProfileKey = profileKey,
            ResumeSessionId = resumeSessionId,
        };

        /// <summary>Copilot (SDK transport): no shell, no writing, with the reason the model is told.</summary>
        public static CopilotSessionProfile CopilotProfile(string key, string reason, string resumeSessionId = null) => new()
        {
            Key = key,
            DenyShell = true,
            DenyWrite = true,
            DenyReason = reason,
            ResumeSessionId = resumeSessionId,
        };

        /// <summary>
        /// opencode configuration of a read-only session: writing and the shell on "ask", and every ask rejected by the
        /// session (rejectPermissions) — not "deny": the free provider refuses requests where the shell is removed
        /// (measured with the e2e tests, 27/09/2026). No web. Without continue_loop_on_deny a rejected permission ends
        /// the whole turn: the refusal goes back to the model instead.
        /// </summary>
        public static string OpenCodeConfig()
        {
            System.Text.Json.Nodes.JsonObject Permissions() => new()
            {
                ["edit"] = "ask",
                ["bash"] = "ask",
                ["webfetch"] = "deny",
            };
            var config = new System.Text.Json.Nodes.JsonObject
            {
                ["experimental"] = new System.Text.Json.Nodes.JsonObject { ["continue_loop_on_deny"] = true },
                ["permission"] = Permissions(),
                ["agent"] = new System.Text.Json.Nodes.JsonObject
                {
                    ["build"] = new System.Text.Json.Nodes.JsonObject { ["permission"] = Permissions() },
                },
            };
            return config.ToJsonString();
        }

        /// <summary>Whether the engine's CLI can be found in the service's PATH; null for no engine.</summary>
        public static bool? IsResolvable(MarkAgentEngine engine) => engine switch
        {
            MarkAgentEngine.Claude => ClaudeCodeProcessLauncher.IsResolvable(),
            MarkAgentEngine.Copilot => MdExplorer.Features.Services.AI.CopilotAcp.CopilotProcessLauncher.IsResolvable(),
            MarkAgentEngine.OpenCode => OpenCodeProcessLauncher.IsResolvable(),
            _ => null,
        };

        /// <summary>Why the project's engine cannot answer here; null when it can.</summary>
        public static string WhyNot(MarkAgentEngine engine) => IsResolvable(engine) switch
        {
            null => "Il progetto non ha un motore di MarkAgent: sceglilo nelle impostazioni del progetto.",
            false => $"Il motore del progetto ({MarkAgentEngines.CommandOf(engine)}) non si trova nel PATH del servizio: " +
                     "se l'hai installato con nvm, avvia MdExplorer da una shell che carica nvm.",
            _ => null,
        };

        public static string Label(MarkAgentEngine engine, string model)
        {
            var name = engine switch
            {
                MarkAgentEngine.Claude => "Claude Code",
                MarkAgentEngine.Copilot => "Copilot",
                MarkAgentEngine.OpenCode => "opencode",
                _ => engine.ToString(),
            };
            return string.IsNullOrWhiteSpace(model) ? name : $"{name} ({model})";
        }
    }

    /// <summary>
    /// A question in a read-only session of its own, started for it and closed after it: the folder summaries (D6 of
    /// sprint 2026-09-29-Motore-LLM-Unico), one independent call per document — a session kept for the whole job would
    /// grow by a whole document at every step and mix the summaries up.
    /// </summary>
    public sealed class OneShotReadOnlySession
    {
        private readonly ClaudeCodeSessionPool _claude;
        private readonly CopilotChatSessionPool _copilot;
        private readonly ILoggerFactory _loggerFactory;

        public OneShotReadOnlySession(ClaudeCodeSessionPool claude, CopilotChatSessionPool copilot, ILoggerFactory loggerFactory)
        {
            _claude = claude;
            _copilot = copilot;
            _loggerFactory = loggerFactory;
        }

        /// <summary>The text of the answer to <paramref name="prompt"/>, with the engine and model given.</summary>
        public async Task<string> AskAsync(string projectPath, MarkAgentEngine engine, string model, string prompt,
            string profileKey, string denyReason, CancellationToken ct)
        {
            var key = profileKey + "|" + Guid.NewGuid().ToString("N");
            var answer = new StringBuilder();
            switch (engine)
            {
                case MarkAgentEngine.Claude:
                    try
                    {
                        var session = await _claude.GetOrCreateAsync(key, projectPath, model, ReadOnlyEngineSessions.ClaudeOptions(profileKey), ct).ConfigureAwait(false);
                        await foreach (var chunk in session.PromptAsync(prompt, ct).ConfigureAwait(false))
                            if (chunk.Kind == ClaudeCodeChunk.KindMessage) answer.Append(chunk.Text);
                    }
                    finally { await _claude.ReleaseAsync(key).ConfigureAwait(false); }
                    break;
                case MarkAgentEngine.Copilot:
                    try
                    {
                        var session = await _copilot.GetOrCreateAsync(key, projectPath, model, ReadOnlyEngineSessions.CopilotProfile(profileKey, denyReason), ct).ConfigureAwait(false);
                        await foreach (var chunk in session.PromptAsync(prompt, ct).ConfigureAwait(false))
                            if (chunk.Kind == CopilotChatChunk.KindMessage) answer.Append(chunk.Text);
                    }
                    finally { await _copilot.ReleaseAsync(key).ConfigureAwait(false); }
                    break;
                case MarkAgentEngine.OpenCode:
                {
                    using var server = new OpenCodeServer(_loggerFactory.CreateLogger<OpenCodeServer>(),
                        new Dictionary<string, string> { ["OPENCODE_CONFIG_CONTENT"] = ReadOnlyEngineSessions.OpenCodeConfig() });
                    await using var session = new OpenCodeSession(_loggerFactory.CreateLogger<OpenCodeSession>(), server,
                        projectPath, model, null, rejectPermissions: true);
                    await foreach (var chunk in session.PromptAsync(prompt, ct).ConfigureAwait(false))
                        if (chunk.Kind == OpenCodeChunk.KindMessage) answer.Append(chunk.Text);
                    break;
                }
                default:
                    throw new InvalidOperationException(ReadOnlyEngineSessions.WhyNot(engine));
            }
            return answer.ToString();
        }
    }
}
