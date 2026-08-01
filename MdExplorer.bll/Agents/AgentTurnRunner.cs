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

    /// <summary>Come si è concluso un turno. Solo <see cref="Completed"/> è un successo.</summary>
    public enum AgentTurnOutcome
    {
        /// <summary>Il turno è arrivato in fondo: il testo è la risposta dell'agente.</summary>
        Completed,

        /// <summary>
        /// Il ciclo di tool calling ha esaurito il budget di iterazioni senza concludere. NON è
        /// un successo: il lavoro è a metà, e trattarlo come riuscito farebbe pubblicare un
        /// deliverable parziale e — se federato — insegnerebbe alla memoria che il routing ha
        /// funzionato.
        /// </summary>
        Exhausted,

        /// <summary>Un tool ha fallito in modo che il modello non ha saputo recuperare.</summary>
        ToolFailure,

        /// <summary>Il provider ha fallito (processo in errore, API irraggiungibile, quota).</summary>
        ProviderError,

        /// <summary>Annullato (shutdown): non consuma tentativi, il messaggio torna in coda.</summary>
        Cancelled,
    }

    /// <summary>
    /// Esito di un turno.
    /// <para>
    /// Perché non basta una stringa: un runner può fallire <b>restituendo testo</b> invece di
    /// sollevare — è il caso del tetto di iterazioni di un ciclo di tool calling, che ritorna un
    /// messaggio di scuse come se fosse una risposta. A valle il dispatcher marcherebbe il
    /// messaggio come processato, pubblicherebbe il branch del deliverable, farebbe scattare
    /// l'auto-merge e spedirebbe all'origine un verdetto di successo che <b>rinforza</b> la
    /// confidence del fatto di routing. Un fallimento muto non si limita a fallire: insegna la
    /// cosa sbagliata.
    /// </para>
    /// </summary>
    public class AgentTurnResult
    {
        public AgentTurnOutcome Outcome { get; init; }

        /// <summary>Testo prodotto dal turno (anche negli esiti non riusciti, se c'è).</summary>
        public string Text { get; init; }

        /// <summary>Iterazioni di tool calling consumate, quando il runner sa contarle.</summary>
        public int? Iterations { get; init; }

        /// <summary>Dettaglio leggibile del perché non è <see cref="AgentTurnOutcome.Completed"/>.</summary>
        public string Diagnostic { get; init; }

        public bool IsSuccess => Outcome == AgentTurnOutcome.Completed;

        public static AgentTurnResult Completed(string text, int? iterations = null)
            => new() { Outcome = AgentTurnOutcome.Completed, Text = text, Iterations = iterations };

        public static AgentTurnResult Failed(AgentTurnOutcome outcome, string diagnostic, string text = null, int? iterations = null)
            => new() { Outcome = outcome, Diagnostic = diagnostic, Text = text, Iterations = iterations };
    }

    /// <summary>
    /// Seam provider-agnostico del run LLM (§7). Isola "esegui un turno headless e dimmi com'è
    /// andato" dal provider concreto (Copilot CLI): l'implementazione reale vive nel
    /// Service (<c>CopilotTurnRunner</c>), una fake deterministica sostituisce il provider
    /// nei test senza spawn di processo (R10). Prima esisteva solo un cast diretto a
    /// <c>CopilotCliProvider</c> che rendeva impossibile la fake.
    /// </summary>
    public interface IAgentTurnRunner
    {
        Task<AgentTurnResult> RunTurnAsync(AgentTurnRequest request, CancellationToken ct = default);
    }
}
