using System.Collections.Generic;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Agents;

namespace MdExplorer.Services.AgentRegistry
{
    /// <summary>
    /// Le "Pagine Gialle" del progetto (§6 Agent-Harness-A2A): scopre i cittadini
    /// dalle due sorgenti (file <c>.agent.md</c> + <c>IAlgorithmicAgent</c>), applica
    /// le regole di identità e persiste trust/identità in <c>AgentIdentity</c>.
    /// <para>
    /// Il catalogo in-memory è una <b>cache, mai l'autorità</b>: <see cref="RefreshCatalog"/>
    /// ri-legge sempre dalle fonti (filesystem + DB). Aggiornamento event-driven via
    /// project-open (<see cref="IProjectOpenedEventHandler"/>) e hook FSW sui
    /// <c>.agent.md</c>.
    /// </para>
    /// </summary>
    public interface IAgentRegistryService : IProjectOpenedEventHandler
    {
        /// <summary>Catalogo del progetto dalla cache; lo costruisce se assente.</summary>
        IReadOnlyList<AgentRegistryEntry> GetCatalog(string projectPath);

        /// <summary>Ri-legge dalle fonti, riconcilia <c>AgentIdentity</c>, aggiorna la cache.</summary>
        IReadOnlyList<AgentRegistryEntry> RefreshCatalog(string projectPath);

        /// <summary>
        /// Conferma il trust di un agente ancorandolo al contenuto attuale del blocco
        /// <c>a2a:</c>/<c>tools:</c> (R3). Fail-loud se l'agente non esiste o è escluso.
        /// </summary>
        AgentRegistryEntry TrustAgent(string projectPath, string agentName);

        /// <summary>Revoca il trust (Trusted/Enabled → false).</summary>
        AgentRegistryEntry UntrustAgent(string projectPath, string agentName);

        /// <summary>Hook FSW: un <c>.agent.md</c> è cambiato → refresh (async) del progetto.</summary>
        void OnAgentFileChanged(string projectPath);
    }
}
