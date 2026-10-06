using System;
using System.Collections.Generic;
using System.Linq;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Esito dell'avviso che parte da «Approva». Mai silenzioso: se non è partito, dice perché.</summary>
    public sealed class ApprovalNotice
    {
        public bool Notified { get; init; }
        public string Recipient { get; init; }
        public string Error { get; init; }
    }

    /// <summary>
    /// Passa il lavoro al collega quando la persona approva: a nome della <b>persona</b> (mittente
    /// <c>user</c>), non dell'agente, e solo verso uno dei destinatari che la scheda dell'agente che
    /// consegna dichiara in <c>a2a.on_approval_notify</c>. L'agente del collega non si sveglia da solo: lo
    /// avvisa il gesto di chi approva.
    /// </summary>
    public interface IAgentApprovalNotifier
    {
        /// <summary>I destinatari possibili per le consegne di <paramref name="producerAgent"/> (vuota = nessun avviso).</summary>
        IReadOnlyList<ApprovalRecipient> CandidatesFor(string projectPath, string producerAgent);

        /// <summary>Che cosa fa l'agente che consegna (<c>a2a.summary</c>), per mostrarlo accanto ad «Autorizza»; null se non lo dichiara.</summary>
        string SummaryOf(string projectPath, string agentName);

        /// <summary>Accoda l'avviso a <paramref name="recipient"/>. Rivalida il destinatario dalle fonti.</summary>
        ApprovalNotice Notify(string projectPath, string producerAgent, string recipient, IEnumerable<string> files);
    }

    public class AgentApprovalNotifier : IAgentApprovalNotifier
    {
        private readonly IAgentRegistryService _registry;
        private readonly IAgentMailbox _mailbox;
        private readonly IAgentWakeGuard _wakeGuard;
        private readonly ILogger<AgentApprovalNotifier> _logger;

        private readonly MdExplorer.Services.IProjectMetadataService _projectMetadata;

        public AgentApprovalNotifier(IAgentRegistryService registry, IAgentMailbox mailbox, IAgentWakeGuard wakeGuard,
            ILogger<AgentApprovalNotifier> logger, MdExplorer.Services.IProjectMetadataService projectMetadata)
        {
            _projectMetadata = projectMetadata;
            _wakeGuard = wakeGuard;
            _registry = registry;
            _mailbox = mailbox;
            _logger = logger;
        }

        public IReadOnlyList<ApprovalRecipient> CandidatesFor(string projectPath, string producerAgent)
            // Con un workflow, chi viene dopo un'approvazione lo decide lo schedulatore (W14): nessun collega da scegliere.
            => WorkflowConfigured(projectPath)
                ? new List<ApprovalRecipient>()
                : ApprovalRoute.Candidates(_registry.RefreshCatalog(projectPath), producerAgent, e => _wakeGuard.WorksElsewhere(projectPath, e));

        private bool WorkflowConfigured(string projectPath)
            => !string.IsNullOrWhiteSpace(_projectMetadata?.GetAgentCity(projectPath)?.WorkflowDoc);

        public string SummaryOf(string projectPath, string agentName)
            => _registry.RefreshCatalog(projectPath)
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, agentName, StringComparison.OrdinalIgnoreCase))?.Summary;

        public ApprovalNotice Notify(string projectPath, string producerAgent, string recipient, IEnumerable<string> files)
        {
            // La cache non è l'autorità (§6/§7): si rilegge il catalogo al momento di avvisare.
            var catalog = _registry.RefreshCatalog(projectPath);
            var target = ApprovalRoute.Candidates(catalog, producerAgent, e => _wakeGuard.WorksElsewhere(projectPath, e))
                .FirstOrDefault(c => string.Equals(c.Name, recipient, StringComparison.OrdinalIgnoreCase));
            if (target == null)
                return Fail(recipient, $"'{recipient}' non è tra i destinatari dichiarati da '{producerAgent}' (on_approval_notify).");
            if (!target.Available)
                return Fail(recipient, target.Reason);

            // Con un workflow l'avviso non serve: dopo l'approvazione lo schedulatore fa partire chi viene dopo (W14).
            if (WorkflowConfigured(projectPath))
                return Fail(recipient, $"il progetto ha un workflow: dopo l'approvazione chi viene dopo lo fa partire MdExplorer, non un avviso a '{target.Name}'.");

            var maxHops = catalog.FirstOrDefault(e => string.Equals(e.Name, target.Name, StringComparison.OrdinalIgnoreCase))?.MaxHops;
            var result = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = projectPath,
                FromAgent = ApprovalRoute.Sender,
                ToAgent = target.Name,
                Body = ApprovalRoute.ComposeMessage(producerAgent, files),
                HopLimitOverride = maxHops,
                TriggerSource = "approval",
            });
            if (!result.Accepted)
                return Fail(recipient, result.RejectionReason);

            _logger.LogInformation("[Approval] la persona ha approvato il lavoro di '{From}' e ha avvisato '{To}' (conversazione {Conv})",
                producerAgent, target.Name, result.ConversationId);
            return new ApprovalNotice { Notified = true, Recipient = target.Name };
        }

        private ApprovalNotice Fail(string recipient, string error)
        {
            _logger.LogWarning("[Approval] avviso a '{To}' non partito: {Error}", recipient, error);
            return new ApprovalNotice { Notified = false, Recipient = recipient, Error = error };
        }
    }
}
