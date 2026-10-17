using System;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Federation;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Federation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Federazione lato UI (§12.5/§12.6, Fasi 6c-6d). Loopback + guardia R12. Espone le città
    /// locali attive e — cuore della 6c — il <b>gate umano</b> delle richieste federate: nessun
    /// agente parte finché l'umano di questa città non autorizza esplicitamente.
    /// </summary>
    [ApiController]
    [Route("api/A2A/federation")]
    public class FederationController : ControllerBase
    {
        private readonly IFederationState _state;
        private readonly IUserSettingsDB _session;
        private readonly IAgentMailbox _mailbox;
        private readonly IAgentRegistryService _registry;
        private readonly IEffectiveOwnerIdentity _identity;
        private readonly MdExplorer.Services.IProjectOwnershipService _ownership;
        private readonly IFederationSender _federationSender;
        private readonly ILogger<FederationController> _logger;

        public FederationController(
            IFederationState state,
            IUserSettingsDB session,
            IAgentMailbox mailbox,
            IAgentRegistryService registry,
            IEffectiveOwnerIdentity identity,
            MdExplorer.Services.IProjectOwnershipService ownership,
            IFederationSender federationSender,
            ILogger<FederationController> logger)
        {
            _state = state;
            _session = session;
            _mailbox = mailbox;
            _registry = registry;
            _identity = identity;
            _ownership = ownership;
            _federationSender = federationSender;
            _logger = logger;
        }

        // ---- Impersonazione utente per il test della città (identità-padrone effettiva) ----

        /// <summary>Elenco degli utenti-padrone della città dall'ownership doc (per il selettore identità).</summary>
        [HttpGet("users")]
        public IActionResult Users([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = UiText.Say("projectPath è obbligatorio.", "projectPath is required.") });
            var entries = _ownership.GetActiveOwnership(projectPath);
            if (entries == null)
                return StatusCode(409, new { error = UiText.Say("Città non attiva o senza documento di ownership valido.", "City not active or without a valid ownership document.") });

            var me = _identity.ResolveEmail(projectPath);
            // OwnerId delle città REMOTE accese ora sulla stanza (badge presenza): la mia macchina
            // non compare nel roster del relay, quindi la mia riga resta sempre offline qui.
            var onlineOwners = _state.GetRemotePresence(projectPath)
                .Select(p => p.OwnerId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var users = entries
                .Select(e => e.GitEmail)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(e => e, StringComparer.OrdinalIgnoreCase)
                .Select(email =>
                {
                    var ownerId = MdExplorer.Features.Federation.FederationRoom.ComputeUserId(email);
                    var isMe = string.Equals(email, me, StringComparison.OrdinalIgnoreCase);
                    return new
                    {
                        email,
                        ownerId,
                        // Il nome della colonna «Responsabile» (Anna, Marco): l'email da sola non dice chi è.
                        displayName = NameOf(projectPath, email, entries, isMe && !_identity.Resolve(projectPath).Impersonated) ?? email,
                        isMe,
                        online = onlineOwners.Contains(ownerId),
                    };
                })
                .ToList();
            return Ok(new { testMode = _identity.IsTestModeEnabled(), users });
        }

        /// <summary>Stato dell'identità effettiva del progetto (per il banner).</summary>
        [HttpGet("impersonate")]
        public IActionResult ImpersonationStatus([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = UiText.Say("projectPath è obbligatorio.", "projectPath is required.") });
            var id = _identity.Resolve(projectPath);
            var name = NameOf(projectPath, id.Email, _ownership.GetActiveOwnership(projectPath), realMe: !id.Impersonated);
            return Ok(new { testMode = _identity.IsTestModeEnabled(), email = id.Email, name, ownerId = id.OwnerId, impersonated = id.Impersonated });
        }

        /// <summary>
        /// Il nome di chi ha questa email: la colonna «Responsabile» della tabella delle responsabilità (sta nel
        /// progetto, uguale su tutti i computer, vale anche per chi si impersona); se l'email non c'è e si tratta
        /// di me davvero (non impersonato), il <c>user.name</c> di git del progetto. Null se nessuno dei due lo dice.
        /// </summary>
        private static string NameOf(string projectPath, string email,
            System.Collections.Generic.IEnumerable<MdExplorer.Features.Agents.OwnershipEntry> entries, bool realMe)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var fromTable = entries?
                .Where(e => string.Equals(e.GitEmail?.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Responsible?.Trim())
                .FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
            if (fromTable != null) return fromTable;
            if (!realMe) return null;
            try
            {
                using var repo = new LibGit2Sharp.Repository(projectPath);
                var gitName = repo.Config.Get<string>("user.name")?.Value?.Trim();
                return string.IsNullOrWhiteSpace(gitName) ? null : gitName;
            }
            catch (LibGit2Sharp.RepositoryNotFoundException) { return null; }
        }

        /// <summary>Abilita/disabilita la modalità test identità (globale).</summary>
        [HttpPost("impersonate/test-mode")]
        public IActionResult SetTestMode([FromBody] TestModeRequest body)
        {
            _identity.SetTestMode(body?.Enabled == true);
            return Ok(new { testMode = _identity.IsTestModeEnabled() });
        }

        /// <summary>Agisci come l'utente indicato (solo in modalità test; email dev'essere un padrone del doc).</summary>
        [HttpPost("impersonate")]
        public async Task<IActionResult> Impersonate([FromBody] ImpersonateRequest body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.ProjectPath) || string.IsNullOrWhiteSpace(body.Email))
                return BadRequest(new { error = UiText.Say("projectPath ed email sono obbligatori.", "projectPath and email are required.") });
            if (!_identity.IsTestModeEnabled())
                return StatusCode(409, new { error = UiText.Say("Abilita la modalità test identità prima di impersonare un utente.", "Enable identity test mode before impersonating a user.") });

            var entries = _ownership.GetActiveOwnership(body.ProjectPath);
            var known = entries?.Any(e => string.Equals(e.GitEmail, body.Email.Trim(), StringComparison.OrdinalIgnoreCase)) == true;
            if (!known)
                return UnprocessableEntity(new { error = UiText.Say($"'{body.Email}' non è un padrone dell'ownership doc: impersonazione rifiutata.", $"'{body.Email}' is not an owner in the ownership doc: impersonation refused.") });

            _identity.SetImpersonation(body.ProjectPath.Trim(), body.Email.Trim());
            await _federationSender.ReconnectProjectAsync(body.ProjectPath.Trim());   // riconnetti col nuovo ownerId
            var id = _identity.Resolve(body.ProjectPath.Trim());
            return Ok(new { impersonated = id.Impersonated, email = id.Email, ownerId = id.OwnerId });
        }

        /// <summary>Torna alla tua identità reale.</summary>
        [HttpDelete("impersonate")]
        public async Task<IActionResult> StopImpersonation([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = UiText.Say("projectPath è obbligatorio.", "projectPath is required.") });
            _identity.ClearImpersonation(projectPath.Trim());
            await _federationSender.ReconnectProjectAsync(projectPath.Trim());
            var id = _identity.Resolve(projectPath.Trim());
            return Ok(new { impersonated = id.Impersonated, email = id.Email, ownerId = id.OwnerId });
        }

        [HttpGet("cities")]
        public IActionResult Cities([FromQuery] string? projectPath)
        {
            var local = _state.GetLocalCities().AsEnumerable();
            if (!string.IsNullOrWhiteSpace(projectPath))
                local = local.Where(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));
            var localList = local.ToList();

            // Città REMOTE accese sulle stanze in scope (roster del relay decifrato lato service).
            var remote = localList
                .SelectMany(c => _state.GetRemotePresence(c.ProjectPath)
                    .Select(p => new
                    {
                        projectPath = c.ProjectPath,
                        roomId = c.RoomId,
                        ownerId = p.OwnerId,
                        gitEmail = p.GitEmail,
                        agents = (p.Agents ?? new System.Collections.Generic.List<MdExplorer.Features.Federation.FederatedAgentSummary>())
                            .Select(a => new { name = a.Name, role = a.Role, skills = a.Skills })
                            .ToList(),
                    }))
                .ToList();

            return Ok(new
            {
                local = localList.Select(c => new
                {
                    projectPath = c.ProjectPath,
                    projectName = c.ProjectName,
                    roomId = c.RoomId,
                    relayUrl = c.RelayUrl,
                }).ToList(),
                remote,
                relayConnected = localList.Any(c => _state.IsRelayConnected(c.ProjectPath)),
            });
        }

        // ---- gate umano delle richieste federate (§12.6) ----

        /// <summary>Le richieste federate in attesa di autorizzazione (o tutte con includeDecided).</summary>
        [HttpGet("requests")]
        public IActionResult Requests([FromQuery] string? projectPath, [FromQuery] bool includeDecided = false)
        {
            // Filtro di stato a livello SQL; il path (comparazione normalizzata, non
            // traducibile) si applica in memoria sul set già ridotto. Lettura dentro una
            // transazione esplicita (igiene della sessione condivisa UserDB).
            _session.BeginTransaction();
            var query = _session.GetDal<FederationRequest>().GetList();
            if (!includeDecided)
                query = query.Where(r => r.Status == FederationRequest.StatusEnum.Pending);
            var fetched = query.ToList();
            _session.Commit();

            var reqs = fetched.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(projectPath))
                reqs = reqs.Where(r => AgentPathComparer.Equals(r.ProjectPath, projectPath));

            var items = reqs
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    id = r.Id,
                    federationId = r.FederationId,
                    projectPath = r.ProjectPath,
                    fromOwner = r.FromOwner,
                    fromAgent = r.FromAgent,
                    targetAgent = r.TargetAgent,
                    scope = r.Scope,
                    message = r.Message,
                    topics = AgentTopics.Split(r.Topics),
                    status = r.Status,
                    createdAt = r.CreatedAt,
                    decidedAt = r.DecidedAt,
                })
                .ToList();
            return Ok(new { requests = items });
        }

        /// <summary>
        /// L'umano AUTORIZZA una richiesta federata: apre una conversazione locale (con
        /// <c>FederationId</c> + controparte remota) e sveglia l'agente bersaglio — solo ORA
        /// parte un run. Può cambiare l'agente proposto. Fail-loud se già decisa o se l'agente
        /// non è un cittadino trusted (è l'umano che si assume la responsabilità del trust).
        /// </summary>
        [HttpPost("requests/{id}/approve")]
        public IActionResult Approve(Guid id, [FromBody] ApproveFederationRequest? body)
        {
            var dal = _session.GetDal<FederationRequest>();
            _session.BeginTransaction();
            var req = dal.GetList().FirstOrDefault(r => r.Id == id);
            _session.Commit();
            if (req == null)
                return NotFound(new { error = UiText.Say($"Richiesta federata '{id}' non trovata.", $"Federated request '{id}' not found.") });
            if (req.Status != FederationRequest.StatusEnum.Pending)
                return UnprocessableEntity(new { error = UiText.Say($"Richiesta già decisa (stato: '{req.Status}').", $"Request already decided (status: '{req.Status}').") });

            var targetAgent = string.IsNullOrWhiteSpace(body?.TargetAgent) ? req.TargetAgent : body.TargetAgent.Trim();

            // L'agente bersaglio deve essere un cittadino trusted di questo progetto.
            var citizen = _registry.RefreshCatalog(req.ProjectPath)
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, targetAgent, StringComparison.OrdinalIgnoreCase));
            if (citizen == null)
                return NotFound(new { error = UiText.Say($"L'agente '{targetAgent}' non è un cittadino del progetto.", $"The agent '{targetAgent}' is not a citizen of the project.") });
            if (!citizen.Trusted)
                return StatusCode(403, new { error = UiText.Say($"L'agente '{targetAgent}' non è trusted: concedi il trust prima di autorizzare.", $"The agent '{targetAgent}' is not trusted: grant trust before authorizing.") });

            // CLAIM ATOMICO (anti doppio-click / doppia UI): in UNA transazione ri-leggo la
            // richiesta, verifico che sia ancora pending, e la marco Approved insieme alla
            // creazione della conversazione. Una seconda approvazione concorrente troverà lo
            // stato già Approved e si fermerà — nessun doppio wake dell'agente.
            _session.BeginTransaction();
            var reqFresh = dal.GetList().FirstOrDefault(r => r.Id == id);
            if (reqFresh == null || reqFresh.Status != FederationRequest.StatusEnum.Pending)
            {
                _session.Commit();
                return UnprocessableEntity(new { error = UiText.Say("Richiesta già decisa (approvazione concorrente?).", "Request already decided (concurrent approval?).") });
            }
            var conv = new AgentConversation
            {
                ProjectPath = req.ProjectPath,
                StartedBy = ConversationHopGuard.UserRecipient,
                Status = AgentConversation.StatusEnum.Active,
                HopCount = 0,
                HopLimit = ConversationHopGuard.ClampHopLimit(citizen.MaxHops),
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
                FederationId = reqFresh.FederationId,
                RequestId = reqFresh.RequestId,   // Fase 7a: ponte per l'intervention-result di ritorno
                RemoteOwner = reqFresh.FromOwner,
                RemoteAgent = reqFresh.FromAgent,
                HandoffRef = reqFresh.HandoffRef, // Fase 7d.5: B si sincronizza a questo ref al wake
                BaseCommit = reqFresh.BaseCommit,
            };
            _session.GetDal<AgentConversation>().Save(conv);
            reqFresh.Status = FederationRequest.StatusEnum.Approved;
            reqFresh.DecidedAt = DateTime.UtcNow;
            dal.Save(reqFresh);
            _session.Commit();
            var convId = conv.Id;

            // L'umano vouches: mittente 'user' (hop esente, sempre ammesso). La provenienza
            // remota vera vive sulla conversazione (RemoteOwner/RemoteAgent + FederationId).
            var enqueue = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = req.ProjectPath,
                FromAgent = ConversationHopGuard.UserRecipient,
                ToAgent = targetAgent,
                Body = req.Message,
                ContextId = convId.ToString(),
                HopLimitOverride = citizen.MaxHops,
                Topics = AgentTopics.Split(req.Topics),
            });
            if (!enqueue.Accepted)
            {
                // REVERT del claim: l'enqueue è fallito, quindi nessun agente è stato svegliato.
                // Riporto la richiesta a pending (ritentabile) ed elimino la conversazione orfana.
                _session.BeginTransaction();
                var reqRevert = dal.GetList().FirstOrDefault(r => r.Id == id);
                if (reqRevert != null)
                {
                    reqRevert.Status = FederationRequest.StatusEnum.Pending;
                    reqRevert.DecidedAt = null;
                    dal.Save(reqRevert);
                }
                _session.GetDal<AgentConversation>().Delete(conv);
                _session.Commit();
                return StatusCode(409, new { error = enqueue.RejectionReason });
            }

            _logger.LogInformation("[Federation] richiesta {Id} APPROVATA → sveglio '{Agent}' in conversazione {Conv}", id, targetAgent, convId);
            return Ok(new { approved = true, conversationId = convId.ToString(), targetAgent });
        }

        /// <summary>
        /// L'umano RIFIUTA: nessun run parte, la richiesta è chiusa, e l'origine lo sa (esito <c>declined</c>, 17/10/2026):
        /// senza, dopo 24 ore avviserebbe la sua persona che il lavoro non è stato processato.
        /// </summary>
        [HttpPost("requests/{id}/reject")]
        public async System.Threading.Tasks.Task<IActionResult> Reject(Guid id)
        {
            var dal = _session.GetDal<FederationRequest>();
            _session.BeginTransaction();
            var req = dal.GetList().FirstOrDefault(r => r.Id == id);
            if (req == null)
            {
                _session.Commit();
                return NotFound(new { error = UiText.Say($"Richiesta federata '{id}' non trovata.", $"Federated request '{id}' not found.") });
            }
            if (req.Status != FederationRequest.StatusEnum.Pending)
            {
                _session.Commit();
                return UnprocessableEntity(new { error = UiText.Say($"Richiesta già decisa (stato: '{req.Status}').", $"Request already decided (status: '{req.Status}').") });
            }

            req.Status = FederationRequest.StatusEnum.Rejected;
            req.DecidedAt = DateTime.UtcNow;
            dal.Save(req);
            _session.Commit();

            _logger.LogInformation("[Federation] richiesta {Id} RIFIUTATA dall'umano", id);

            var told = false;
            if (req.RequestId.HasValue && !string.IsNullOrWhiteSpace(req.FromOwner))
            {
                var sent = await _federationSender.SendFederatedResultAsync(req.ProjectPath, FederationRoom.ComputeUserId(req.FromOwner),
                    new FederatedResultPayload
                    {
                        Kind = FederationKind.InterventionResult,
                        RequestId = req.RequestId.Value.ToString(),
                        FederationId = req.FederationId.ToString(),
                        Verdict = FederationVerdict.Declined,
                    });
                told = sent != RelayDelivery.NotSent;
                if (!told)
                    _logger.LogWarning("[Federation] rifiuto della richiesta {Id} non riportato a {Owner}: relay non raggiungibile.", id, req.FromOwner);
            }
            return Ok(new { rejected = true, originTold = told });
        }
    }

    public class ApproveFederationRequest
    {
        /// <summary>Override opzionale dell'agente proposto.</summary>
        public string? TargetAgent { get; set; }
    }

    public class TestModeRequest
    {
        public bool Enabled { get; set; }
    }

    public class ImpersonateRequest
    {
        public string? ProjectPath { get; set; }
        public string? Email { get; set; }
    }
}
