using System;
using System.Collections.Generic;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Federation
{
    /// <summary>Discriminanti di busta federata (Fase 7a). Campo macchina: stringhe esatte.</summary>
    public static class FederationKind
    {
        /// <summary>Richiesta di intervento → gate umano (comportamento storico, default).</summary>
        public const string RequestIntervention = "request-intervention";
        /// <summary>Esito di ritorno di un intervento delegato → risveglio dell'agente d'origine.</summary>
        public const string InterventionResult = "intervention-result";
    }

    /// <summary>
    /// Il payload (decifrato) dell'<c>intervention-result</c> (Fase 7a — il cerchio di ritorno):
    /// la città bersaglio riporta all'origine l'esito del lavoro delegato. Viaggia nello stesso
    /// tunnel cifrato della richiesta (6b), distinto da <see cref="FederationKind.InterventionResult"/>.
    /// L'origine lo correla per <see cref="RequestId"/> contro il ledger
    /// <see cref="FederationDispatch"/> e risveglia l'agente che aveva smistato.
    /// </summary>
    public sealed class FederatedResultPayload
    {
        /// <summary>Sempre <see cref="FederationKind.InterventionResult"/>.</summary>
        public string Kind { get; set; }
        /// <summary>Chiave di correlazione col ledger d'origine (la stessa RequestId della richiesta).</summary>
        public string RequestId { get; set; }
        public string FederationId { get; set; }
        /// <summary>Esito: <see cref="Federation.FederationVerdict"/> (success/rejected/not-ready).</summary>
        public string Verdict { get; set; }
        /// <summary>Reason codificato (<see cref="FederationReason"/>) — opzionale.</summary>
        public string Reason { get; set; }
        public List<string> Topics { get; set; }
    }

    /// <summary>
    /// Riceve l'esito di ritorno di un intervento federato (Fase 7a): lo correla al ledger
    /// <see cref="FederationDispatch"/> per <c>RequestId</c>, marca il dispatch <c>completed</c> e
    /// <b>risveglia l'agente d'origine</b> con l'esito. Un esito con <c>RequestId</c> sconosciuto
    /// (nessuna riga di ledger) viene <b>scartato</b> — è il filtro anti-avvelenamento.
    /// </summary>
    public interface IFederatedResultReceiver
    {
        void Receive(string projectPath, FederatedResultPayload payload);
    }

    public class FederatedResultReceiver : IFederatedResultReceiver
    {
        /// <summary>
        /// Mittente-sentinella del risveglio di ritorno: non è un agente né l'utente. L'esito è
        /// una <b>notifica di sistema</b> che rientra nella conversazione d'origine.
        /// </summary>
        public const string FederationSender = "<federation>";

        /// <summary>Etichetta d'audit del risveglio (TriggerSource dell'AgentExecutionLog).</summary>
        public const string TriggerSource = "federated-result";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentMailbox _mailbox;
        private readonly ILogger<FederatedResultReceiver> _logger;

        public FederatedResultReceiver(
            IServiceScopeFactory scopeFactory,
            IAgentMailbox mailbox,
            ILogger<FederatedResultReceiver> logger)
        {
            _scopeFactory = scopeFactory;
            _mailbox = mailbox;
            _logger = logger;
        }

        public void Receive(string projectPath, FederatedResultPayload payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("projectPath richiesto per correlare l'esito.", nameof(projectPath));

            if (!Guid.TryParse(payload.RequestId, out var requestId))
            {
                _logger.LogWarning("[Federation] intervention-result con RequestId assente/non valido ('{Req}'): scartato.", payload.RequestId);
                return;
            }

            FederationDispatch dispatch;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var dal = db.GetDal<FederationDispatch>();

                // Correlazione per RequestId, ristretta al progetto locale (AgentPathComparer non è
                // traducibile in SQL → materializza con .ToList() prima del confronto path).
                dispatch = dal.GetList()
                    .Where(d => d.RequestId == requestId)
                    .ToList()
                    .FirstOrDefault(d => AgentPathComparer.Equals(d.ProjectPath, projectPath));

                if (dispatch == null)
                {
                    db.Commit();
                    // Filtro anti-avvelenamento: nessuna riga di ledger per questa RequestId → non
                    // abbiamo mai smistato questo intervento. Non risvegliare nulla.
                    _logger.LogWarning("[Federation] intervention-result per RequestId {Req} sconosciuto (nessun dispatch): scartato.", requestId);
                    return;
                }

                if (dispatch.Status == FederationDispatch.StatusEnum.Completed)
                {
                    db.Commit();
                    // Idempotenza: una riconsegna del relay non deve risvegliare due volte l'origine.
                    _logger.LogInformation("[Federation] intervention-result per RequestId {Req} già completato: idempotente, ignoro.", requestId);
                    return;
                }

                dispatch.Status = FederationDispatch.StatusEnum.Completed;
                dispatch.CompletedAt = DateTime.UtcNow;
                dal.Save(dispatch);
                db.Commit();
            }

            var verdict = string.IsNullOrWhiteSpace(payload.Verdict) ? "(nessun verdetto)" : payload.Verdict.Trim();
            var reason = string.IsNullOrWhiteSpace(payload.Reason) ? null : payload.Reason.Trim();
            var body = reason == null
                ? $"[Esito federato] L'intervento delegato a '{dispatch.TargetAgent}' ({dispatch.TargetOwner}) si è concluso: {verdict}."
                : $"[Esito federato] L'intervento delegato a '{dispatch.TargetAgent}' ({dispatch.TargetOwner}) si è concluso: {verdict} — {reason}.";

            // Risveglio dell'agente d'origine, nella conversazione d'origine. FromAgent sentinella
            // "<federation>" (provenienza), destinatario = OriginAgent catturato al dispatch (NON
            // StartedBy). Il dispatcher lo prende dal percorso LLM esistente.
            var enqueue = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = dispatch.ProjectPath,
                FromAgent = FederationSender,
                ToAgent = dispatch.OriginAgent,
                Body = body,
                ContextId = dispatch.ConversationId.ToString(),
                Topics = AgentTopics.Split(dispatch.Topics),
                TriggerSource = TriggerSource,
            });

            if (!enqueue.Accepted)
            {
                // Fail-loud (REGOLA #2): l'esito è stato correlato e il ledger chiuso, ma la
                // conversazione d'origine non può più accoglierlo (killed/exhausted). Non è un
                // fallback silenzioso: lo si registra perché l'umano possa riprenderlo.
                _logger.LogWarning(
                    "[Federation] esito per RequestId {Req} correlato ma NON consegnato all'agente '{Agent}': {Reason}",
                    requestId, dispatch.OriginAgent, enqueue.RejectionReason);
                return;
            }

            // TODO 7b: reinforce/erode — qui la memoria dell'agente d'origine verrà aggiornata
            // in base al verdict (Success → rinforzo; Rejected/NotReady → erosione + fatto).

            _logger.LogInformation(
                "[Federation] esito '{Verdict}' per RequestId {Req} → risvegliato '{Agent}' nella conversazione {Conv}.",
                verdict, requestId, dispatch.OriginAgent, dispatch.ConversationId);
        }
    }
}
