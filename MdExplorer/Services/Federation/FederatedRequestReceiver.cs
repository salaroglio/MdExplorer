using System;
using System.Collections.Generic;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Federation
{
    /// <summary>
    /// Il payload (decifrato) di una richiesta di intervento federata che arriva sul relay
    /// (§12.6). Trasporta la semantica A2A dentro il tunnel cifrato di 6b; il ricevente
    /// materializza un gate umano da questo.
    /// </summary>
    public sealed class FederatedRequestPayload
    {
        /// <summary>
        /// Discriminante di busta (Fase 7a): <c>"request-intervention"</c> (default) instrada al
        /// gate umano; <c>"intervention-result"</c> è l'esito di ritorno (payload distinto,
        /// vedi <see cref="FederatedResultPayload"/>). Null/empty su un payload vecchio =
        /// <c>request-intervention</c> (retrocompatibilità).
        /// </summary>
        public string Kind { get; set; }

        /// <summary>
        /// Idempotency key univoca per SINGOLA emissione (§12.6): l'origine ne conia una nuova
        /// a ogni <c>request-intervention</c>. Il ricevente deduplica su questa — così una
        /// <b>redelivery del relay</b> (stesso RequestId) non crea un secondo gate, ma due
        /// interventi distinti (RequestId diversi) sì, anche a parità di testo.
        /// </summary>
        public string RequestId { get; set; }
        public string FederationId { get; set; }
        public string FromOwner { get; set; }
        public string FromAgent { get; set; }
        public string Scope { get; set; }
        /// <summary>Agente locale proposto (risolto dall'origine via ownership condivisa in git).</summary>
        public string TargetAgent { get; set; }
        public string Message { get; set; }
        public List<string> Topics { get; set; }

        /// <summary>
        /// Fase 7d.5 — riferimento di handoff: il branch ref COMPLETO (<c>agent/&lt;A&gt;/&lt;id&gt;</c>)
        /// che porta il lavoro dell'origine, già pushato su origin. <c>null</c> = nessun handoff
        /// (retrocompatibilità). B si sincronizza a questo ref come primo predicato d'esecuzione.
        /// </summary>
        public string HandoffRef { get; set; }
        /// <summary>Fase 7d.5 — sha di testa a cui B deve sincronizzarsi (la testa di <see cref="HandoffRef"/>).</summary>
        public string BaseCommit { get; set; }
    }

    /// <summary>
    /// Riceve una richiesta federata e ne materializza il <b>gate umano</b> (§12.6): la
    /// persiste come <see cref="FederationRequest"/> <c>pending</c> e notifica l'umano.
    /// <b>Non fa partire alcun run</b> — è il guardrail: solo l'approvazione esplicita
    /// (<see cref="FederationGate"/>) sveglia l'agente locale.
    /// </summary>
    public interface IFederatedRequestReceiver
    {
        /// <summary>Materializza il gate per il progetto locale <paramref name="projectPath"/>; ritorna l'id della richiesta.</summary>
        Guid Receive(string projectPath, FederatedRequestPayload payload);
    }

    public class FederatedRequestReceiver : IFederatedRequestReceiver
    {
        public const string SignalREvent = "federationRequestReceived";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly ILogger<FederatedRequestReceiver> _logger;

        public FederatedRequestReceiver(
            IServiceScopeFactory scopeFactory,
            IHubContext<MonitorMDHub> hub,
            ILogger<FederatedRequestReceiver> logger)
        {
            _scopeFactory = scopeFactory;
            _hub = hub;
            _logger = logger;
        }

        public Guid Receive(string projectPath, FederatedRequestPayload payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentException("projectPath richiesto per materializzare il gate.", nameof(projectPath));
            if (string.IsNullOrWhiteSpace(payload.TargetAgent))
                throw new ArgumentException("targetAgent richiesto: non so quale agente locale proporre.", nameof(payload));
            if (string.IsNullOrWhiteSpace(payload.Message))
                throw new ArgumentException("message richiesto.", nameof(payload));

            // FederationId: se assente/non valido lo generiamo (correlazione comunque tracciata).
            var fedId = Guid.TryParse(payload.FederationId, out var g) ? g : Guid.NewGuid();
            // RequestId (idempotency key della singola emissione): può mancare da un'origine
            // vecchia — allora il dedup ricade sul vecchio criterio (FederationId+Target+Message).
            Guid? requestId = Guid.TryParse(payload.RequestId, out var rq) ? rq : (Guid?)null;

            var request = new FederationRequest
            {
                FederationId = fedId,
                RequestId = requestId,
                ProjectPath = projectPath,
                FromOwner = payload.FromOwner,
                FromAgent = payload.FromAgent,
                TargetAgent = payload.TargetAgent.Trim(),
                Scope = payload.Scope,
                Message = payload.Message,
                Topics = AgentTopics.Join(payload.Topics),
                HandoffRef = string.IsNullOrWhiteSpace(payload.HandoffRef) ? null : payload.HandoffRef.Trim(),
                BaseCommit = string.IsNullOrWhiteSpace(payload.BaseCommit) ? null : payload.BaseCommit.Trim(),
                Status = FederationRequest.StatusEnum.Pending,
                CreatedAt = DateTime.UtcNow,
            };

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var dal = db.GetDal<FederationRequest>();

                // Idempotenza sulle RICONSEGNE del relay: se c'è un RequestId, deduplica SOLO su
                // quello (stessa emissione ri-consegnata) — così due interventi distinti con testo
                // identico ma RequestId diversi generano DUE gate. Origine vecchia senza RequestId:
                // ricade sul criterio storico (FederationId+Target+Message), solo contro i pending.
                FederationRequest duplicate;
                if (requestId.HasValue)
                {
                    duplicate = dal.GetList()
                        .Where(r => r.RequestId == requestId.Value)
                        .ToList()
                        .FirstOrDefault(r => AgentPathComparer.Equals(r.ProjectPath, projectPath));
                }
                else
                {
                    duplicate = dal.GetList()
                        .Where(r => r.FederationId == fedId
                                    && r.Status == FederationRequest.StatusEnum.Pending
                                    && r.TargetAgent == request.TargetAgent
                                    && r.Message == request.Message)
                        .ToList()
                        .FirstOrDefault(r => AgentPathComparer.Equals(r.ProjectPath, projectPath));
                }
                if (duplicate != null)
                {
                    db.Commit();
                    _logger.LogInformation("[Federation] richiesta federata duplicata (req {Req}, fed {Fed}) → riuso il gate {Id}",
                        requestId, fedId, duplicate.Id);
                    return duplicate.Id;
                }

                dal.Save(request);
                db.Commit();
            }

            NotifyUser(request);
            _logger.LogInformation("[Federation] richiesta federata da '{From}' per '{Target}' (ambito '{Scope}') → gate pending {Id}",
                payload.FromOwner, request.TargetAgent, payload.Scope, request.Id);
            return request.Id;
        }

        private void NotifyUser(FederationRequest r)
        {
            try
            {
                _hub.Clients.All.SendAsync(SignalREvent, new
                {
                    id = r.Id.ToString(),
                    federationId = r.FederationId.ToString(),
                    projectPath = r.ProjectPath,
                    fromOwner = r.FromOwner,
                    fromAgent = r.FromAgent,
                    targetAgent = r.TargetAgent,
                    scope = r.Scope,
                    createdAt = r.CreatedAt,
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Federation] notifica SignalR del gate {Id} fallita (best-effort)", r.Id);
            }
        }
    }
}
