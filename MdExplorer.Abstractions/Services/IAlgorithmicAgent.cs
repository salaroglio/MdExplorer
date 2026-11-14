using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.Agents;

namespace MdExplorer.Abstractions.Services
{
    /// <summary>
    /// Un cittadino <b>algoritmico</b> della città degli agenti (§4, §6 del design
    /// doc Agent-Harness-A2A): codice C# deterministico, senza LLM, con la stessa
    /// cittadinanza di un agente <c>.agent.md</c> — stessa Agent Card, stessa
    /// mailbox, stesso trust, stesso log.
    /// <para>
    /// Tutte le implementazioni registrate nella DI vengono arruolate
    /// automaticamente dal registry (iniettabili come
    /// <see cref="System.Collections.Generic.IEnumerable{T}"/> di
    /// <see cref="IAlgorithmicAgent"/>, stesso pattern di <c>IEnumerable&lt;IAiProvider&gt;</c>).
    /// Candidati naturali: reindicizzazione, sync KG/Fuseki, pipeline parsing COBOL/PL1.
    /// </para>
    /// </summary>
    public interface IAlgorithmicAgent
    {
        /// <summary>
        /// La Agent Card dell'agente (name, role, skills) — l'equivalente in codice
        /// del blocco <c>a2a:</c> del frontmatter. Il <c>Name</c> deve rispettare le
        /// stesse regole di identità dei cittadini LLM (kebab-case, unico nel
        /// progetto, non riservato).
        /// </summary>
        AgentCardInfo GetCard();

        /// <summary>
        /// Esegue il task in ingresso in-process. Il risultato (successo/errore)
        /// determina lo stato terminale del task A2A (<c>completed</c>/<c>failed</c>).
        /// </summary>
        Task<AgentTaskResult> ExecuteAsync(AgentTaskContext ctx, CancellationToken ct);
    }
}
