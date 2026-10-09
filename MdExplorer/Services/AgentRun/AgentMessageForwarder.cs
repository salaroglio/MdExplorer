using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Federation;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Sends a message addressed <b>by name</b> to an agent that works on another person's computer, to
    /// that computer: the same encrypted road and the same human gate at the other end as a
    /// <c>request_intervention</c> (§12.6) — there the person who answers for the agent accepts the request,
    /// and only then their agent starts. Here nothing starts.
    /// <para>
    /// The message's own id is the request's idempotency key: a message that is tried again (the road was
    /// down) reaches the other end once.
    /// </para>
    /// <para>
    /// What it does not carry: the sender's unpublished work. The other computer starts from what is on
    /// <c>origin</c>; work passed along has to be approved (merged) first.
    /// </para>
    /// </summary>
    public interface IAgentMessageForwarder
    {
        /// <summary>True when the road took the message (delivered, or kept for a computer that is off).</summary>
        Task<bool> ForwardAsync(AgentMessage message, AgentOwnerVerdict owner, CancellationToken ct = default);
    }

    public sealed class AgentMessageForwarder : IAgentMessageForwarder
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFederationSender _sender;
        private readonly IEffectiveOwnerIdentity _identity;
        private readonly ILogger<AgentMessageForwarder> _logger;

        public AgentMessageForwarder(IServiceScopeFactory scopeFactory, IFederationSender sender,
            IEffectiveOwnerIdentity identity, ILogger<AgentMessageForwarder> logger)
        {
            _scopeFactory = scopeFactory;
            _sender = sender;
            _identity = identity;
            _logger = logger;
        }

        public async Task<bool> ForwardAsync(AgentMessage message, AgentOwnerVerdict owner, CancellationToken ct = default)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (owner == null || owner.Kind != AgentOwnerKind.SomeoneElse || string.IsNullOrWhiteSpace(owner.OwnerEmail))
                throw new ArgumentException("Only a message for an agent that answers to someone else is forwarded.", nameof(owner));

            var targetOwnerId = FederationRoom.ComputeUserId(owner.OwnerEmail);
            var federationId = Guid.NewGuid();

            // The ledger before the send, as for request_intervention: if it was sent, the ledger exists —
            // otherwise a result coming back would find nothing to close. Once per message.
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var conversation = db.GetDal<AgentConversation>().GetList().FirstOrDefault(c => c.Id == message.ConversationId);
                federationId = conversation?.FederationId ?? federationId;

                var ledger = db.GetDal<FederationDispatch>();
                var already = ledger.GetList().FirstOrDefault(d => d.RequestId == message.Id);
                if (already != null)
                {
                    federationId = already.FederationId;
                }
                else
                {
                    ledger.Save(new FederationDispatch
                    {
                        RequestId = message.Id,
                        FederationId = federationId,
                        ProjectPath = message.ProjectPath,
                        ConversationId = message.ConversationId,
                        OriginAgent = message.FromAgent,
                        TargetOwner = targetOwnerId,
                        TargetAgent = message.ToAgent,
                        Topics = message.Topics,
                        Status = FederationDispatch.StatusEnum.Pending,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
                db.Commit();
            }

            var sent = await _sender.SendFederatedRequestAsync(message.ProjectPath, targetOwnerId, new FederatedRequestPayload
            {
                Kind = FederationKind.RequestIntervention,
                RequestId = message.Id.ToString(),
                FederationId = federationId.ToString(),
                FromOwner = _identity.ResolveEmail(message.ProjectPath),
                FromAgent = message.FromAgent,
                Scope = owner.Scope,
                TargetAgent = message.ToAgent,
                Message = message.Body,
                Topics = AgentTopics.Split(message.Topics)?.ToList(),
            });
            if (!sent)
            {
                _logger.LogWarning("[Forward] '{To}' lavora sul computer di {Owner}, ma il collegamento tra le città non è attivo: messaggio {Id} non partito.",
                    message.ToAgent, owner.OwnerEmail, message.Id);
                return false;
            }

            // Sent: the conversation remembers who it went to. The hop was already counted when the message
            // was queued, so nothing is added here.
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var dal = db.GetDal<AgentConversation>();
                var conversation = dal.GetList().FirstOrDefault(c => c.Id == message.ConversationId);
                if (conversation != null)
                {
                    conversation.FederationId = conversation.FederationId ?? federationId;
                    conversation.RemoteOwner = owner.OwnerEmail;
                    conversation.RemoteAgent = message.ToAgent;
                    conversation.LastActivityAt = DateTime.UtcNow;
                    dal.Save(conversation);
                }
                db.Commit();
            }

            _logger.LogInformation("[Forward] {From} → '{To}' sul computer di {Owner} (ambito '{Scope}', richiesta {Id}).",
                message.FromAgent, message.ToAgent, owner.OwnerEmail, owner.Scope, message.Id);
            return true;
        }
    }
}
