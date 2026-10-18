using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Il <b>cancello meccanico</b> del merge dei deliverable-doc (Fase 7g.1): decide se un
    /// deliverable pushato può essere fuso nel default. È il <b>seam pluggable</b> — la doc-CI è
    /// leggera/assente, quindi il default auto-approva; un futuro client CI (GitLab pipelines /
    /// SCM-Manager) o un <b>agente-revisore</b> (7g.2, sopra il mailbox) rimpiazza questa
    /// implementazione senza toccare il dispatcher. CI-rossa/verdict negativo → <c>false</c> →
    /// niente merge (l'agente rilavora nel feedback loop).
    /// </summary>
    public interface IDeliverableMergeGate
    {
        Task<bool> ShouldMergeAsync(string projectPath, string agentName, string branch, CancellationToken ct = default);
    }

    /// <summary>
    /// Default di 7g: <b>auto-approva</b> (doc-CI leggera/assente → auto-merge su push). Non
    /// costruisce alcun client CI (§7g.1: "non costruire un client CI pesante finché non serve").
    /// </summary>
    public sealed class AutoApproveMergeGate : IDeliverableMergeGate
    {
        private readonly ILogger<AutoApproveMergeGate> _logger;
        public AutoApproveMergeGate(ILogger<AutoApproveMergeGate> logger) => _logger = logger;

        public Task<bool> ShouldMergeAsync(string projectPath, string agentName, string branch, CancellationToken ct = default)
        {
            _logger.LogDebug("[MergeGate] auto-approvo il deliverable '{Branch}' di '{Agent}' (doc-CI assente).", branch, agentName);
            return Task.FromResult(true);
        }
    }
}
