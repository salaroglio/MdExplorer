using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Execution;

namespace MdExplorer.Features.Agents
{
    /// <summary>Cosa serve per svegliare un agente LLM su un messaggio ricevuto (§7 passo 5).</summary>
    public class LlmWakeRequest
    {
        public Guid RunId { get; set; }
        public string AgentName { get; set; }
        public string AgentFileContent { get; set; }
        public string ProjectPath { get; set; }
        public string ConversationId { get; set; }
        public string FromAgent { get; set; }
        public string MessageBody { get; set; }
        public IReadOnlyList<string> Topics { get; set; }
        public IReadOnlyList<AgentRosterEntry> Roster { get; set; }
    }

    /// <summary>Esito del risveglio: il dispatcher lo mappa su processed / retry-or-fail.</summary>
    public class LlmWakeOutcome
    {
        public bool Success { get; private set; }
        public string Output { get; private set; }
        public string Error { get; private set; }

        public static LlmWakeOutcome Ok(string output) => new LlmWakeOutcome { Success = true, Output = output };
        public static LlmWakeOutcome Fail(string error) => new LlmWakeOutcome { Success = false, Error = error };
    }

    public interface ILlmAgentWaker
    {
        Task<LlmWakeOutcome> WakeAsync(LlmWakeRequest request, CancellationToken ct = default);
    }

    /// <summary>
    /// Il cuore del risveglio LLM (§7, R1+R2): conia un <c>RunToken</c> legato all'identità
    /// dell'agente svegliato, compone il prompt di risveglio col messaggio come <b>dato</b>
    /// dentro delimitatori (anti prompt-injection), esegue un turno headless passando il
    /// token <b>nell'ambiente</b> del processo (mai nel prompt) e <b>revoca il token a fine
    /// run</b> — sempre, anche in caso di errore.
    /// <para>
    /// Vive in bll apposta: il seam <see cref="IAgentTurnRunner"/> permette a una fake
    /// deterministica di sostituire Copilot nei test (R10) senza spawn di processo, e senza
    /// un progetto di test che referenzi l'EXE del Service.
    /// </para>
    /// </summary>
    public class LlmAgentWaker : ILlmAgentWaker
    {
        // Nomi delle variabili d'ambiente che il processo figlio (e il suo MCP) leggerà per
        // autenticarsi verso il Service (lo step 5 le consuma nei tool SendAgentMessage/ListAgents).
        public const string EnvRunToken = "MDE_RUN_TOKEN";
        public const string EnvAgentName = "MDE_AGENT_NAME";
        public const string EnvProjectPath = "MDE_PROJECT_PATH";
        public const string EnvConversationId = "MDE_CONVERSATION_ID";
        public const string EnvFromAgent = "MDE_FROM_AGENT";

        private readonly IRunTokenStore _tokens;
        private readonly IAgentTurnRunner _runner;

        public LlmAgentWaker(IRunTokenStore tokens, IAgentTurnRunner runner)
        {
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        }

        public async Task<LlmWakeOutcome> WakeAsync(LlmWakeRequest request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.AgentName))
                throw new ArgumentException("AgentName mancante — non posso coniare un token senza identità.", nameof(request));
            if (string.IsNullOrWhiteSpace(request.AgentFileContent))
                throw new ArgumentException("AgentFileContent vuoto — rifiuto di svegliare un agente senza corpo.", nameof(request));

            // R2: il token è legato all'identità dell'agente svegliato e al contesto. Chi invierà
            // messaggi in uscita si autentica con questo token; il Service risale a queste claim.
            var token = _tokens.Mint(new RunTokenClaims
            {
                RunId = request.RunId,
                AgentName = request.AgentName,
                ProjectPath = request.ProjectPath,
                ConversationId = request.ConversationId,
            });

            try
            {
                // R1: il messaggio entra come DATO dentro delimitatori, non come istruzione.
                var prompt = AgentPromptComposer.ComposeMessageWakePrompt(
                    request.AgentFileContent, request.FromAgent, request.MessageBody, request.Roster, request.Topics);

                var env = new Dictionary<string, string>
                {
                    [EnvRunToken] = token,
                    [EnvAgentName] = request.AgentName ?? string.Empty,
                    [EnvProjectPath] = request.ProjectPath ?? string.Empty,
                    [EnvConversationId] = request.ConversationId ?? string.Empty,
                    [EnvFromAgent] = request.FromAgent ?? string.Empty,
                };

                // Firma git per-agente (§10): ogni commit che l'agente fa nel workspace è
                // attribuito a lui (`<name>@agents.mde`), non all'umano. git blame resta giusto.
                foreach (var kv in AgentGitIdentity.EnvFor(request.AgentName))
                    env[kv.Key] = kv.Value;

                var output = await _runner.RunTurnAsync(new AgentTurnRequest
                {
                    ComposedPrompt = prompt,
                    WorkingDirectory = request.ProjectPath,
                    Environment = env,
                }, ct);

                return LlmWakeOutcome.Ok(output);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Shutdown: non consumare un tentativo. Il messaggio resta 'delivered' e la
                // recovery all'avvio lo rimette 'pending'. Rilancio (il token è già revocato dal finally).
                throw;
            }
            catch (Exception ex)
            {
                return LlmWakeOutcome.Fail(ex.Message);
            }
            finally
            {
                // "Monouso" nel senso del run: il token vale finché il run è vivo, poi muore.
                _tokens.Revoke(token);
            }
        }
    }
}
