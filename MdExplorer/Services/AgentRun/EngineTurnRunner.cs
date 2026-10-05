using System;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.LlmSessions;
using MdExplorer.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>The engine and model an agent's turn runs on, and where the engine came from.</summary>
    public sealed record AgentEngineChoice(MarkAgentEngine Engine, string Model, bool FromProject)
    {
        public string Label => ReadOnlyEngineSessions.Label(Engine, Model);

        /// <summary>
        /// The engine of an agent's turn (sprint 2026-09-29-Motore-LLM-Unico, D3/D12): the one asked — the card's
        /// <c>runtime.provider</c> or the launch dialog's choice — otherwise the project's (the one of the MarkAgent
        /// tab). The model: the one asked; otherwise the project's when the engine is the project's, the engine's
        /// default when it is another (as the e2e tests, D10). An unknown engine, or a project without one, is an
        /// error that says what to do: never another engine in its place.
        /// </summary>
        public static AgentEngineChoice Resolve(string requestedProvider, string requestedModel,
            MarkAgentEngine projectEngine, string projectModel, out string error)
        {
            error = null;
            var model = string.IsNullOrWhiteSpace(requestedModel) ? null : requestedModel.Trim();
            if (string.IsNullOrWhiteSpace(requestedProvider))
            {
                if (projectEngine == MarkAgentEngine.None)
                {
                    error = "Il progetto non ha un motore di MarkAgent e l'agente non ne dichiara uno: scegli il motore nel tab " +
                            "MarkAgent, oppure scrivi 'runtime: provider: claude | copilot | opencode' nella scheda dell'agente.";
                    return null;
                }
                return new AgentEngineChoice(projectEngine, model ?? projectModel, true);
            }

            if (!TryParseProvider(requestedProvider, out var engine))
            {
                error = $"Motore '{requestedProvider.Trim()}' sconosciuto: i valori ammessi sono claude, copilot, opencode.";
                return null;
            }
            var fromProject = engine == projectEngine;
            return new AgentEngineChoice(engine, model ?? (fromProject ? projectModel : MarkAgentEngines.DefaultModelOf(engine)), fromProject);
        }

        /// <summary>The engine ids, plus the names cards already use ("copilot-cli", "claude-code").</summary>
        public static bool TryParseProvider(string provider, out MarkAgentEngine engine)
        {
            var id = (provider ?? string.Empty).Trim().ToLowerInvariant().Replace("-", "").Replace("_", "");
            if (id.StartsWith("copilot", StringComparison.Ordinal)) { engine = MarkAgentEngine.Copilot; return true; }
            if (id == "claude" || id == "claudecode") { engine = MarkAgentEngine.Claude; return true; }
            if (id == "opencode") { engine = MarkAgentEngine.OpenCode; return true; }
            engine = MarkAgentEngine.None;
            return false;
        }
    }

    /// <summary>
    /// The <see cref="IAgentTurnRunner"/> MdExplorer registers: it chooses the engine of the turn
    /// (<see cref="AgentEngineChoice.Resolve"/>) and hands the turn to the runner of that engine — Copilot CLI,
    /// Claude Code or opencode — each in a session of its own for the turn (agents work alone, also in parallel).
    /// </summary>
    public sealed class EngineTurnRunner : IAgentTurnRunner
    {
        private readonly CopilotTurnRunner _copilot;
        private readonly ClaudeCodeTurnRunner _claude;
        private readonly OpenCodeTurnRunner _openCode;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentActivityBoard _activity;
        private readonly ILogger<EngineTurnRunner> _logger;

        public EngineTurnRunner(CopilotTurnRunner copilot, ClaudeCodeTurnRunner claude, OpenCodeTurnRunner openCode,
            IServiceScopeFactory scopeFactory, IAgentActivityBoard activity, ILogger<EngineTurnRunner> logger)
        {
            _activity = activity;
            _copilot = copilot;
            _claude = claude;
            _openCode = openCode;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<AgentTurnResult> RunTurnAsync(AgentTurnRequest request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            MarkAgentEngine projectEngine;
            string projectModel;
            using (var scope = _scopeFactory.CreateScope())
            {
                (projectEngine, projectModel) = MdExplorer.Service.ProjectsManager.ProjectEngine(
                    scope.ServiceProvider.GetService<IUserSettingsDB>(), request.ProjectPath);
            }

            var choice = AgentEngineChoice.Resolve(request.RequestedProvider, request.RequestedModel, projectEngine, projectModel, out var error);
            if (choice == null)
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError, $"Agente '{request.AgentName}': {error}");

            if (ReadOnlyEngineSessions.IsResolvable(choice.Engine) == false)
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError,
                    $"Agente '{request.AgentName}': il motore {choice.Label} non si trova nel PATH del servizio " +
                    $"({MarkAgentEngines.CommandOf(choice.Engine)}). Se l'hai installato con nvm, avvia MdExplorer da una shell che carica nvm.")
                    .WithEngine(choice.Label);

            _logger.LogInformation("[AgentRun] {Agent}: turno su {Engine} ({Source})", request.AgentName, choice.Label,
                string.IsNullOrWhiteSpace(request.RequestedProvider) ? "motore del progetto" : "motore richiesto");

            var turn = new AgentTurnRequest
            {
                ComposedPrompt = request.ComposedPrompt,
                WorkingDirectory = request.WorkingDirectory,
                AgentName = request.AgentName,
                ProjectPath = request.ProjectPath,
                DeclaredTools = request.DeclaredTools,
                Trusted = request.Trusted,
                RequestedProvider = MarkAgentEngines.IdOf(choice.Engine),
                RequestedModel = choice.Model,
                Environment = request.Environment,
            };
            // From here to the end of the turn the agent is «at work» for whoever looks at the application.
            using var working = _activity.Begin(request.AgentName, request.ProjectPath, choice.Label);
            var result = choice.Engine switch
            {
                MarkAgentEngine.Copilot => await _copilot.RunTurnAsync(turn, ct),
                MarkAgentEngine.Claude => await _claude.RunTurnAsync(turn, ct),
                MarkAgentEngine.OpenCode => await _openCode.RunTurnAsync(turn, ct),
                _ => AgentTurnResult.Failed(AgentTurnOutcome.ProviderError, $"Agente '{request.AgentName}': nessun esecutore per {choice.Label}."),
            };
            return result.WithEngine(choice.Label);
        }
    }
}
