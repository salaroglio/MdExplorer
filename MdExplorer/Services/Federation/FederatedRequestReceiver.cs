using System;
using System.Collections.Generic;
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
        public string FederationId { get; set; }
        public string FromOwner { get; set; }
        public string FromAgent { get; set; }
        public string Scope { get; set; }
        /// <summary>Agente locale proposto (risolto dall'origine via ownership condivisa in git).</summary>
        public string TargetAgent { get; set; }
        public string Message { get; set; }
        public List<string> Topics { get; set; }
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

            var request = new FederationRequest
            {
                FederationId = fedId,
                ProjectPath = projectPath,
                FromOwner = payload.FromOwner,
                FromAgent = payload.FromAgent,
                TargetAgent = payload.TargetAgent.Trim(),
                Scope = payload.Scope,
                Message = payload.Message,
                Topics = AgentTopics.Join(payload.Topics),
                Status = FederationRequest.StatusEnum.Pending,
                CreatedAt = DateTime.UtcNow,
            };

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                db.GetDal<FederationRequest>().Save(request);
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
