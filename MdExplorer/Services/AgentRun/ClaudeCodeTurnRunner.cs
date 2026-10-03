using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services.AI.ClaudeCode;
using MdExplorer.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// An agent's turn on Claude Code (sprint 2026-09-29-Motore-LLM-Unico, F5): a session of its own, started for the
    /// turn in the agent's working folder (often a worktree) and closed after it.
    /// <para>
    /// Permissions as Copilot's <c>--allow-all-tools</c> (D11): in <c>dontAsk</c> only what is granted passes, so the
    /// shell, writing, the web and MdExplorer's MCP server are granted by name. The RunToken and the git identity travel
    /// in the environment of the CLI — inherited by the shell — and in the <c>env</c> of MdExplorer's MCP server, in a
    /// configuration file of the turn's own, deleted after it.
    /// </para>
    /// </summary>
    public sealed class ClaudeCodeTurnRunner
    {
        /// <summary>What an agent can use besides reading: the counterpart of Copilot's --allow-all-tools.</summary>
        public static readonly string[] AgentTools = { "Bash", "Edit", "Write", "NotebookEdit", "WebFetch", "WebSearch" };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public ClaudeCodeTurnRunner(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// The tools an agent may use: <see cref="AgentTools"/> minus what its manifest (<c>tools:</c>) does not declare.
        /// In <c>dontAsk</c> what is not granted is refused, so leaving the shell or writing out of the list is a limit,
        /// not an intention. A request without a manifest (<c>DeclaredTools</c> null) keeps the whole list.
        /// </summary>
        public static string[] ToolsFor(AgentTurnRequest request)
        {
            var denied = MdExplorer.Features.Agents.AgentToolCatalog.NativeToolsToDeny(request?.DeclaredTools);
            return AgentTools.Where(t =>
                !(denied.Contains("shell") && t == "Bash") &&
                !(denied.Contains("write") && (t == "Edit" || t == "Write" || t == "NotebookEdit"))).ToArray();
        }

        /// <summary>The options of an agent's session: MdExplorer's MCP server usable, the agent's tools, its environment.</summary>
        public static ClaudeCodeSessionOptions AgentOptions(string mcpConfigPath, AgentTurnRequest request) => new()
        {
            ToolPolicy = ClaudeCodeToolPolicy.Full,
            McpConfigPath = mcpConfigPath,
            AllowedMcpServers = new[] { ClaudeCodeMcp.ServerName },
            AllowedTools = ToolsFor(request),
            Environment = request.Environment,
            ProfileKey = "agent",
        };

        public async Task<AgentTurnResult> RunTurnAsync(AgentTurnRequest request, CancellationToken ct)
        {
            string groups;
            using (var scope = _scopeFactory.CreateScope())
                groups = McpToolGroupsSettings.WithAgents(
                    MdExplorer.Service.ProjectsManager.McpGroupsArgument(scope.ServiceProvider.GetService<IUserSettingsDB>(), request.ProjectPath));

            var nonce = Guid.NewGuid().ToString("N");
            var configPath = ClaudeCodeMcp.WriteSessionConfig(groups, nonce: nonce, mdexplorerEnvironment: request.Environment ?? new System.Collections.Generic.Dictionary<string, string>());
            if (configPath == null)
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError,
                    $"Agente '{request.AgentName}': il server MCP di MdExplorer non si trova, e senza gli strumenti della città " +
                    "un agente non può lavorare. Controlla l'installazione di MdExplorer (cartella mcp).");

            var model = string.IsNullOrWhiteSpace(request.RequestedModel) ? MarkAgentEngines.ClaudeDefaultModel : request.RequestedModel;
            var session = new ClaudeCodeSession(_loggerFactory.CreateLogger<ClaudeCodeSession>(), request.WorkingDirectory ?? request.ProjectPath,
                model, AgentOptions(configPath, request));
            var answer = new StringBuilder();
            try
            {
                await session.StartAsync(ct).ConfigureAwait(false);
                await foreach (var chunk in session.PromptAsync(request.ComposedPrompt, ct).ConfigureAwait(false))
                    if (chunk.Kind == ClaudeCodeChunk.KindMessage) answer.Append(chunk.Text);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // The CLI closed the turn in error: the turn limit is an unfinished job, anything else the engine's failure.
                var outcome = ex.Message.Contains("error_max_turns", StringComparison.Ordinal)
                    ? AgentTurnOutcome.Exhausted : AgentTurnOutcome.ProviderError;
                return AgentTurnResult.Failed(outcome, $"Claude Code ({model}): {ex.Message}", answer.ToString());
            }
            finally
            {
                await session.DisposeAsync().ConfigureAwait(false);
                try { File.Delete(configPath); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
            return AgentTurnResult.Completed(answer.ToString());
        }
    }
}
