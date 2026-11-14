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

        private readonly MdExplorer.Features.Services.IUiLanguage _language;

        public AgentDeliveryReporter(IAgentMailbox mailbox, ILogger<AgentDeliveryReporter> logger, MdExplorer.Features.Services.IUiLanguage language)
        {
            _language = language;
            _mailbox = mailbox;
            _logger = logger;
        }

        public void ReportNotPublished(string projectPath, string agentName, DeliveryAttempt attempt, string contextId = null)
        {
            if (attempt?.Error == null) return;

            var body = MdExplorer.Features.Agents.AgentTexts.Pick(_language.Current,
                $"Ho finito il lavoro ma non sono riuscito a pubblicarlo: {attempt.Error}.\n\n" +
                $"Il lavoro non è perso: sta nella cartella `{attempt.WorktreePath}`. " +
                "Finché non viene pubblicato non compare tra le richieste da rivedere. " +
                "Di solito basta avere un `origin` su cui si può scrivere (permessi o rete) e rilanciare l'agente.",
                $"I finished the work but could not publish it: {attempt.Error}.\n\n" +
                $"The work is not lost: it is in the folder `{attempt.WorktreePath}`. " +
                "Until it is published it does not appear among the requests to review. " +
                "Usually it is enough to have an `origin` you can write to (permissions or network) and launch the agent again.");

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
