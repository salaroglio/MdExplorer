using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Da dove arriva il client di chat per un turno. Seam apposta: tenerlo fuori dal runner
    /// permette di provare il ciclo (tool passati, budget, esiti) <b>senza</b> un modello vero,
    /// e di aggiungere i provider concreti senza toccare la logica del turno.
    /// </summary>
    public interface IAgentChatClientFactory
    {
        /// <summary>Client per il progetto indicato, o <c>null</c> se non è configurato nessun provider.</summary>
        IChatClient Create(string projectPath);
    }

    /// <summary>
    /// Runner del turno su un provider generico, alternativo a <see cref="CopilotTurnRunner"/>.
    /// <para>
    /// Non implementa il ciclo di tool calling: lo prende da <see cref="FunctionInvokingChatClient"/>,
    /// che espone come parametri proprio ciò che ci serviva governare — il tetto di iterazioni,
    /// gli errori consecutivi, cosa fare di una chiamata a un tool sconosciuto. Il nostro
    /// compito è tre cose: <b>quali</b> tool esporre (catalogo × trust), <b>dove</b> può
    /// scrivere (la working directory del turno) e <b>com'è finito</b> (§Fase A).
    /// </para>
    /// </summary>
    public class ProviderTurnRunner : IAgentTurnRunner
    {
        /// <summary>
        /// Budget di iterazioni del ciclo. Ogni iterazione è una chiamata al provider, quindi è
        /// anche il tetto di costo per turno. Vive qui, come politica dell'harness, non come
        /// costante privata dentro un provider.
        /// </summary>
        public const int DefaultMaxIterations = 12;

        private readonly IAgentChatClientFactory _clients;
        private readonly IAgentMcpToolProvider _tools;
        private readonly ILogger<ProviderTurnRunner> _logger;

        public ProviderTurnRunner(
            IAgentChatClientFactory clients,
            IAgentMcpToolProvider tools,
            ILogger<ProviderTurnRunner> logger)
        {
            _clients = clients;
            _tools = tools;
            _logger = logger;
        }

        public async Task<AgentTurnResult> RunTurnAsync(AgentTurnRequest request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var baseClient = _clients.Create(request.ProjectPath)
                ?? throw new InvalidOperationException(
                    "Nessun provider AI configurato per questo progetto: impossibile risvegliare l'agente. " +
                    "Configuralo nelle impostazioni di progetto, oppure lascia il runner su Copilot.");

            // Manifesto e fiducia arrivano come DATI dalla richiesta: se passassero
            // dall'ambiente, chi dimentica di popolarlo otterrebbe un agente ridotto alla sola
            // lettura senza che nessuno se ne accorga.
            await using var toolSet = await _tools.OpenAsync(
                request.Environment, request.DeclaredTools, request.Trusted, ct);

            using var chat = new ChatClientBuilder(baseClient)
                .UseFunctionInvocation(configure: f =>
                {
                    // Il budget è dell'harness, non del provider.
                    f.MaximumIterationsPerRequest = DefaultMaxIterations;
                    // Una chiamata a un tool che non abbiamo esposto ferma il turno invece di
                    // essere ignorata: se il modello la tenta, vogliamo saperlo.
                    f.TerminateOnUnknownCalls = true;
                })
                .Build();

            var options = new ChatOptions
            {
                Tools = toolSet.Functions.Cast<AITool>().ToList(),
            };

            var messages = new List<ChatMessage>
            {
                new(ChatRole.User, request.ComposedPrompt),
            };

            ChatResponse response;
            try
            {
                response = await chat.GetResponseAsync(messages, options, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ProviderTurn] il provider ha fallito durante il turno");
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError, ex.Message);
            }

            var text = response.Text;
            var iterations = CountProviderRoundTrips(response);

            // Il ciclo si ferma al tetto lasciando l'ultima richiesta di tool INEVASA: se
            // nell'ultimo messaggio c'è ancora una chiamata a funzione, il turno non ha
            // concluso — ha smesso. È la differenza tra "ho finito" e "mi hanno fermato",
            // e a valle cambia tutto (deliverable, auto-merge, verdetto federato).
            if (EndsWithPendingToolCall(response))
            {
                return AgentTurnResult.Failed(
                    AgentTurnOutcome.Exhausted,
                    $"il ciclo di tool calling si è fermato al tetto di {DefaultMaxIterations} iterazioni con una chiamata ancora in sospeso",
                    text, iterations);
            }

            return AgentTurnResult.Completed(text, iterations);
        }

        /// <summary>Quante volte si è tornati al provider: una per messaggio di assistente.</summary>
        private static int CountProviderRoundTrips(ChatResponse response)
            => response?.Messages?.Count(m => m.Role == ChatRole.Assistant) ?? 0;

        private static bool EndsWithPendingToolCall(ChatResponse response)
        {
            var last = response?.Messages?.LastOrDefault();
            return last != null
                   && last.Role == ChatRole.Assistant
                   && last.Contents.OfType<FunctionCallContent>().Any();
        }
    }
}
