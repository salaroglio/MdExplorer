using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services.AI.OpenCode;
using MdExplorer.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// An agent's turn on opencode (sprint 2026-09-29-Motore-LLM-Unico, F5): a server of its own for the turn — the
    /// shared one cannot carry a turn's environment — configured only through <c>OPENCODE_CONFIG_CONTENT</c>, and a
    /// session in the agent's working folder. Permissions as Copilot's <c>--allow-all-tools</c> (D11): writing, the
    /// shell and the web allowed; an ask that still comes (a user's configuration) is rejected, never left hanging.
    /// The RunToken and the git identity in the server's environment (the shell inherits it) and in the
    /// <c>environment</c> of MdExplorer's MCP server.
    /// </summary>
    public sealed class OpenCodeTurnRunner
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<OpenCodeTurnRunner> _logger;

        public OpenCodeTurnRunner(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<OpenCodeTurnRunner>();
        }

        /// <summary>The configuration of an agent's server: MdExplorer's MCP server with the turn's environment, all allowed.</summary>
        public static string AgentConfig(string mcpExecutable, string mcpGroupsArgument, IReadOnlyDictionary<string, string> environment)
        {
            JsonObject Permissions() => new()
            {
                ["edit"] = "allow",
                ["bash"] = "allow",
                ["webfetch"] = "allow",
            };
            var config = new JsonObject
            {
                ["experimental"] = new JsonObject { ["continue_loop_on_deny"] = true },
                ["permission"] = Permissions(),
                // Also on the default agent: an agent-level rule in the user's configuration would win otherwise.
                ["agent"] = new JsonObject { ["build"] = new JsonObject { ["permission"] = Permissions() } },
            };
            if (mcpExecutable != null)
            {
                var command = new List<string> { mcpExecutable };
                if (!string.IsNullOrWhiteSpace(mcpGroupsArgument)) command.AddRange(new[] { "--groups", mcpGroupsArgument });
                config["mcp"] = new JsonObject
                {
                    [ClaudeCodeMcp.ServerName] = new JsonObject
                    {
                        ["type"] = "local",
                        ["command"] = new JsonArray(command.Select(a => (JsonNode)a).ToArray()),
                        ["environment"] = new JsonObject((environment ?? new Dictionary<string, string>())
                            .Select(e => KeyValuePair.Create(e.Key, (JsonNode)e.Value))),
                        ["enabled"] = true,
                    },
                };
            }
            return config.ToJsonString();
        }

        public async Task<AgentTurnResult> RunTurnAsync(AgentTurnRequest request, CancellationToken ct)
        {
            var model = string.IsNullOrWhiteSpace(request.RequestedModel) ? null : request.RequestedModel.Trim();
            if (model != null && model.IndexOf('/') <= 0)
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError,
                    $"Agente '{request.AgentName}': per opencode il modello va scritto 'fornitore/modello' (es. 'opencode/big-pickle'), " +
                    $"non '{model}'. Correggi 'runtime.model:' nella scheda o la scelta nella finestra di lancio.");

            var mcpExecutable = MdExplorer.Service.ProjectsManager.ResolveMcpExecutable(AppDomain.CurrentDomain.BaseDirectory);
            if (mcpExecutable == null)
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError,
                    $"Agente '{request.AgentName}': il server MCP di MdExplorer non si trova, e senza gli strumenti della città " +
                    "un agente non può lavorare. Controlla l'installazione di MdExplorer (cartella mcp).");
            string groups;
            using (var scope = _scopeFactory.CreateScope())
                groups = McpToolGroupsSettings.WithAgents(
                    MdExplorer.Service.ProjectsManager.McpGroupsArgument(scope.ServiceProvider.GetService<IUserSettingsDB>(), request.ProjectPath));

            var serverEnvironment = new Dictionary<string, string>(request.Environment ?? new Dictionary<string, string>())
            {
                ["OPENCODE_CONFIG_CONTENT"] = AgentConfig(mcpExecutable, groups, request.Environment),
            };
            var answer = new StringBuilder();
            using var server = new OpenCodeServer(_loggerFactory.CreateLogger<OpenCodeServer>(), serverEnvironment);
            await using var session = new OpenCodeSession(_loggerFactory.CreateLogger<OpenCodeSession>(), server,
                request.WorkingDirectory ?? request.ProjectPath, model, null, rejectPermissions: true);
            try
            {
                await foreach (var chunk in session.PromptAsync(request.ComposedPrompt, ct).ConfigureAwait(false))
                    if (chunk.Kind == OpenCodeChunk.KindMessage) answer.Append(chunk.Text);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // The turn must end on THIS server before it goes away.
                using var grace = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try { await session.AbortAsync(grace.Token); } catch (Exception ex) { _logger.LogWarning(ex, "[AgentRun] interruzione opencode"); }
                throw;
            }
            catch (Exception ex)
            {
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError,
                    $"opencode{(model == null ? "" : $" ({model})")}: {ex.Message}", answer.ToString());
            }
            return AgentTurnResult.Completed(answer.ToString());
        }
    }
}
