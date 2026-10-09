using System;
using MdExplorer.Features.Agents;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Dice all'umano che il lavoro di un agente c'è ma non è stato pubblicato. Prima questo
    /// finiva solo nel log del servizio: il run risultava riuscito e nessuna richiesta di
    /// revisione compariva, quindi sembrava che l'agente non avesse fatto nulla.
    /// </summary>
    public interface IAgentDeliveryReporter
    {
        /// <summary>
        /// Scrive all'utente, dalla posta dell'agente, perché il lavoro non è stato pubblicato e
        /// dove si trova. <paramref name="contextId"/> = conversazione in cui è nato il lavoro
        /// (null = nuovo thread).
        /// </summary>
        void ReportNotPublished(string projectPath, string agentName, DeliveryAttempt attempt, string contextId = null);
    }

    public class AgentDeliveryReporter : IAgentDeliveryReporter
    {
        private readonly IAgentMailbox _mailbox;
        private readonly ILogger<AgentDeliveryReporter> _logger;

        public AgentDeliveryReporter(IAgentMailbox mailbox, ILogger<AgentDeliveryReporter> logger)
        {
            _mailbox = mailbox;
            _logger = logger;
        }

        public void ReportNotPublished(string projectPath, string agentName, DeliveryAttempt attempt, string contextId = null)
        {
            if (attempt?.Error == null) return;

            var body =
                $"Ho finito il lavoro ma non sono riuscito a pubblicarlo: {attempt.Error}.\n\n" +
                $"Il lavoro non è perso: sta nella cartella `{attempt.WorktreePath}`. " +
                "Finché non viene pubblicato non compare tra le richieste da rivedere. " +
                "Di solito basta avere un `origin` su cui si può scrivere (permessi o rete) e rilanciare l'agente.";

            try
            {
                var result = _mailbox.Enqueue(new EnqueueRequest
                {
                    ProjectPath = projectPath,
                    FromAgent = agentName,
                    ToAgent = ConversationHopGuard.UserRecipient,
                    Body = body,
                    ContextId = contextId,
                });
                if (!result.Accepted)
                    _logger.LogError("[Delivery] '{Agent}': lavoro non pubblicato ({Err}) e l'avviso all'utente è stato rifiutato: {Why}",
                        agentName, attempt.Error, result.RejectionReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Delivery] '{Agent}': lavoro non pubblicato ({Err}) e l'avviso all'utente non è partito.", agentName, attempt.Error);
            }
        }
    }
}
