using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Una richiesta di "un turno" a un provider LLM headless: il prompt già composto,
    /// la working directory e le variabili d'ambiente del processo figlio (dove viaggia
    /// il <c>RunToken</c>, mai nel prompt — R2).
    /// </summary>
    public class AgentTurnRequest
    {
        public string ComposedPrompt { get; set; }
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Override d'ambiente per il processo del provider. Il figlio eredita l'ambiente
        /// del Service e vi aggiunge/sovrascrive queste chiavi — è il canale del RunToken.
        /// </summary>
        public IReadOnlyDictionary<string, string> Environment { get; set; }
    }

    /// <summary>
    /// Seam provider-agnostico del run LLM (§7). Isola "esegui un turno headless e ridammi
    /// il testo" dal provider concreto (Copilot CLI): l'implementazione reale vive nel
    /// Service (<c>CopilotTurnRunner</c>), una fake deterministica sostituisce il provider
    /// nei test senza spawn di processo (R10). Prima esisteva solo un cast diretto a
    /// <c>CopilotCliProvider</c> che rendeva impossibile la fake.
    /// </summary>
    public interface IAgentTurnRunner
    {
        Task<string> RunTurnAsync(AgentTurnRequest request, CancellationToken ct = default);
    }
}
