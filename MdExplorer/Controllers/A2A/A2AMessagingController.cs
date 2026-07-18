using System;
using System.Collections.Generic;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Federation;
using MdExplorer.Services;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Federation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Il canale <b>autenticato</b> agente→agente (§7 passo 6, R2). Un agente svegliato da
    /// un messaggio riceve un <c>RunToken</c> nell'ambiente del suo processo; i tool MCP
    /// <c>SendAgentMessage</c>/<c>ListAgents</c> lo presentano qui nell'header
    /// <c>X-MDE-Run-Token</c>. Il Service risale all'identità certificata (<see cref="RunTokenClaims"/>)
    /// e <b>il mittente è quello — non un campo del body</b>: lo spoofing è strutturalmente
    /// impossibile. Il contesto conversazione è quello del token, così gli hop del fan-out
    /// si accumulano e il guardrail anti-loop non è aggirabile. Loopback-only + guardia R12.
    /// </summary>
    [ApiController]
    [Route("api/A2A/messages")]
    public class A2AMessagingController : ControllerBase
    {
        public const string RunTokenHeader = "X-MDE-Run-Token";

        // Tetto anti-flood del ramo agente→umano (§9): l'escalation è hop-esente e senza
        // whitelist, quindi questo è l'unico limite. Conta i non-letti della conversazione:
        // l'umano che legge/risponde libera il budget, un agente in loop si ferma qui.
        private const int MaxUnreadUserMessagesPerConversation = 10;

        private readonly IRunTokenStore _tokens;
        private readonly IAgentRegistryService _registry;
        private readonly IAgentMailbox _mailbox;
        private readonly IProjectOwnershipService _ownership;
        private readonly IFederationSender _federationSender;
        private readonly IUserSettingsDB _session;
        private readonly MdExplorer.Services.AgentRun.IAgentWorktreeManager _worktree;
        private readonly IProjectMetadataService _projectMetadata;
        private readonly ILogger<A2AMessagingController> _logger;

        public A2AMessagingController(
            IRunTokenStore tokens,
            IAgentRegistryService registry,
            IAgentMailbox mailbox,
            IProjectOwnershipService ownership,
            IFederationSender federationSender,
            IUserSettingsDB session,
            MdExplorer.Services.AgentRun.IAgentWorktreeManager worktree,
            IProjectMetadataService projectMetadata,
            ILogger<A2AMessagingController> logger)
        {
            _tokens = tokens;
            _registry = registry;
            _mailbox = mailbox;
            _ownership = ownership;
            _federationSender = federationSender;
            _session = session;
            _worktree = worktree;
            _projectMetadata = projectMetadata;
            _logger = logger;
        }

        /// <summary>Worktree per-agente attivo per il progetto? (opt-in <c>agentCity.useAgentWorktrees</c>, Fase 7c).</summary>
        private bool UseWorktree(string projectPath)
        {
            try { var c = _projectMetadata.GetAgentCity(projectPath); return c != null && c.UseAgentWorktrees; }
            catch { return false; }
        }

        /// <summary>
        /// Invia un messaggio a un altro cittadino, a nome dell'agente identificato dal
        /// RunToken. Enforce: destinatario cittadino+trusted e la sua whitelist
        /// <c>accepts_messages_from</c> ammette il mittente (§6). Accoda nella mailbox;
        /// la consegna è asincrona (dispatcher).
        /// </summary>
        [HttpPost("send")]
        public IActionResult Send([FromBody] SendAgentMessageRequest request)
        {
            var claims = ResolveClaims();
            if (claims == null)
                return Unauthorized(new { error = "RunToken assente o non valido: questo endpoint è riservato agli agenti svegliati da un messaggio." });

            if (request == null || string.IsNullOrWhiteSpace(request.ToAgent))
                return BadRequest(new { error = "toAgent è obbligatorio." });
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "message è obbligatorio." });

            var to = request.ToAgent.Trim();

            // Escalation all'umano (§9): 'user' è sempre un destinatario valido — non è un
            // cittadino, non ha whitelist, e l'escalation non deve mai morire per trust/budget.
            // Salta i controlli su cittadinanza/accepts e accoda (hop esente lato guard). È la
            // metà "la città parla all'umano" della Fase 4.
            if (string.Equals(to, ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase))
            {
                // Anti-flood: essendo hop-esente, senza un tetto un agente in loop (o
                // prompt-injected) potrebbe riempire la inbox e affogare le escalation vere.
                if (Guid.TryParse(claims.ConversationId, out var userConvId))
                {
                    _session.BeginTransaction();
                    var unreadInThread = _session.GetDal<AgentMessage>().GetList()
                        .Count(m => m.ConversationId == userConvId
                                    && m.ToAgent == ConversationHopGuard.UserRecipient
                                    && m.ReadAt == null);
                    _session.Commit();
                    if (unreadInThread >= MaxUnreadUserMessagesPerConversation)
                        return StatusCode(429, new { error = $"Tetto messaggi verso l'umano raggiunto ({MaxUnreadUserMessagesPerConversation} non letti in questa conversazione): attendi che l'umano legga o risponda." });
                }

                var toUser = _mailbox.Enqueue(new EnqueueRequest
                {
                    ProjectPath = claims.ProjectPath,
                    FromAgent = claims.AgentName,               // R2: mittente certificato
                    ToAgent = ConversationHopGuard.UserRecipient,
                    Body = request.Message,
                    ContextId = claims.ConversationId,          // stesso thread del risveglio
                    Topics = request.Topics,
                });
                if (!toUser.Accepted)
                    return StatusCode(409, new { error = toUser.RejectionReason });

                _logger.LogInformation("[A2A/send] {From} -> user accodato (task {Task})", claims.AgentName, toUser.TaskId);
                return Ok(new
                {
                    accepted = true,
                    taskId = toUser.TaskId,
                    conversationId = toUser.ConversationId.ToString(),
                });
            }

            // Ri-validazione del destinatario dalle fonti (§6/§7): la cache non è mai l'autorità.
            var recipient = _registry.RefreshCatalog(claims.ProjectPath)
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, to, StringComparison.OrdinalIgnoreCase));
            if (recipient == null)
                return NotFound(new { error = $"Destinatario '{to}' non trovato o non cittadino nel progetto." });
            if (!recipient.Trusted)
                return StatusCode(403, new { error = $"Destinatario '{to}' non è trusted: non è possibile inviargli messaggi." });

            // Il filtro fine del destinatario (§6): la sua whitelist deve ammettere il mittente
            // CERTIFICATO dal token (non un nome dichiarato dal chiamante).
            if (!MessageAuthorization.IsSenderAccepted(recipient.AcceptsMessagesFrom, claims.AgentName))
                return StatusCode(403, new { error = $"'{claims.AgentName}' non è tra i mittenti accettati da '{to}' (accepts_messages_from)." });

            var result = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = claims.ProjectPath,
                FromAgent = claims.AgentName,               // R2: mittente certificato, non spoofabile
                ToAgent = to,
                Body = request.Message,
                ContextId = claims.ConversationId,          // stessa conversazione → gli hop si accumulano (anti-loop)
                HopLimitOverride = recipient.MaxHops,
                Topics = request.Topics,                    // §8: contesto dichiarato dal mittente
            });

            if (!result.Accepted)
                return StatusCode(409, new { error = result.RejectionReason });

            _logger.LogInformation("[A2A/send] {From} -> {To} accodato (task {Task})", claims.AgentName, to, result.TaskId);
            return Ok(new
            {
                accepted = true,
                taskId = result.TaskId,
                conversationId = result.ConversationId.ToString(),
            });
        }

        /// <summary>
        /// La rubrica runtime dell'agente: i colleghi <b>trusted</b> del suo progetto (escluso
        /// sé stesso), risolti dal token. È il "chi posso contattare" server-side, coerente
        /// con la rubrica iniettata nel prompt di risveglio (§6).
        /// </summary>
        [HttpGet("roster")]
        public IActionResult Roster()
        {
            var claims = ResolveClaims();
            if (claims == null)
                return Unauthorized(new { error = "RunToken assente o non valido." });

            var colleagues = _registry.RefreshCatalog(claims.ProjectPath)
                .Where(e => e.IsCitizen && e.Trusted)
                .Where(e => !string.Equals(e.Name, claims.AgentName, StringComparison.OrdinalIgnoreCase))
                .Select(e => new
                {
                    name = e.Name,
                    role = e.Role,
                    kind = e.Kind,
                    skills = (e.Skills ?? new List<AgentRegistrySkill>())
                        .Select(s => s.Id)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToList(),
                    acceptsFromYou = MessageAuthorization.IsSenderAccepted(e.AcceptsMessagesFrom, claims.AgentName),
                })
                .ToList();

            return Ok(colleagues);
        }

        /// <summary>
        /// Chiede l'intervento di un agente di un'ALTRA città su un ambito (§12.6, federazione).
        /// L'harness risolve il destinatario dalla tabella di ownership (deterministico, non
        /// l'LLM): ambito → responsabile (gitEmail) + agente. Consuma 1 hop nella conversazione
        /// d'origine, correla i due lati con un <c>FederationId</c>, e spedisce la richiesta
        /// cifrata al relay. A destinazione parte il <b>gate umano</b> (nessun run senza ok).
        /// </summary>
        [HttpPost("request-intervention")]
        public async System.Threading.Tasks.Task<IActionResult> RequestIntervention([FromBody] RequestInterventionRequest request)
        {
            var claims = ResolveClaims();
            if (claims == null)
                return Unauthorized(new { error = "RunToken assente o non valido." });
            if (request == null || string.IsNullOrWhiteSpace(request.Scope))
                return BadRequest(new { error = "scope è obbligatorio." });
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "message è obbligatorio." });

            // Ownership del progetto (richiede federazione attiva + doc valido).
            var entries = _ownership.GetActiveOwnership(claims.ProjectPath);
            if (entries == null)
                return StatusCode(409, new { error = "Città non attiva o senza documento di ownership valido." });

            var entry = OwnershipResolver.Resolve(entries, request.Scope);
            if (entry == null)
                return NotFound(new { error = $"Ambito '{request.Scope}' non presente nell'ownership del progetto." });

            var targetAgent = OwnershipResolver.PickAgent(entry, request.PreferredAgent);
            if (string.IsNullOrWhiteSpace(targetAgent))
                return UnprocessableEntity(new { error = $"L'ambito '{entry.Scope}' non elenca alcun agente." });

            var targetOwnerId = FederationRoom.ComputeUserId(entry.GitEmail);

            // Hop + correlazione FederationId sulla conversazione d'origine (§12.6). Il
            // guardrail si VALUTA prima del send, ma hop e correlazione si committano solo
            // DOPO un send riuscito: un relay irraggiungibile (503) non deve bruciare il
            // budget hop della conversazione a ogni retry.
            var fedId = Guid.NewGuid();
            AgentConversation conv = null;
            if (Guid.TryParse(claims.ConversationId, out var convId))
            {
                _session.BeginTransaction();
                var dal = _session.GetDal<AgentConversation>();
                conv = dal.GetList().FirstOrDefault(c => c.Id == convId);
                if (conv != null)
                {
                    if (conv.Status == AgentConversation.StatusEnum.Killed)
                    {
                        _session.Commit();
                        return StatusCode(409, new { error = "La conversazione è terminata (killed)." });
                    }
                    // La richiesta federata è un fan-out: conta 1 hop (non esente). Qui si VALUTA
                    // soltanto (l'incremento vero avviene dopo il send, con re-read fresco).
                    var decision = ConversationHopGuard.Evaluate(claims.AgentName, targetAgent, conv.HopCount, conv.HopLimit);
                    if (!decision.Allowed)
                    {
                        conv.Status = AgentConversation.StatusEnum.Exhausted;
                        dal.Save(conv);
                        _session.Commit();
                        return StatusCode(409, new { error = $"Limite hop raggiunto ({conv.HopLimit}): conversazione esaurita." });
                    }
                    fedId = conv.FederationId ?? fedId;
                }
                _session.Commit();
            }

            var payload = new FederatedRequestPayload
            {
                // Idempotency key FRESCA per ogni chiamata: distingue una redelivery del relay
                // (stesso RequestId → il ricevente la deduplica) da due interventi distinti
                // (RequestId diversi → due gate), anche a parità di testo. FederationId invece
                // resta stabile per la conversazione (correlazione).
                RequestId = Guid.NewGuid().ToString(),
                FederationId = fedId.ToString(),
                FromOwner = ResolveLocalGitEmail(claims.ProjectPath),
                FromAgent = claims.AgentName,               // R2: identità certificata dal token
                Scope = entry.Scope,
                TargetAgent = targetAgent,
                Message = request.Message,
                Topics = request.Topics,
            };

            // Fase 7d.5 — passaggio dati via GIT: se l'origine gira in worktree, PRIMA di spedire
            // la richiesta pubblica il branco di lavoro (commit → push) e allega il ref di handoff,
            // così il destinatario potrà sincronizzarsi al lavoro dell'origine (§6: commit→push→richiesta→sync).
            if (UseWorktree(claims.ProjectPath))
            {
                var pushed = await _worktree.CommitAndPushBranchAsync(claims.ProjectPath, claims.AgentName, $"handoff {payload.RequestId}");
                if (pushed != null)
                {
                    payload.HandoffRef = pushed.Branch;
                    payload.BaseCommit = pushed.HeadSha;
                    _logger.LogInformation("[A2A/request-intervention] handoff pubblicato: {Ref}@{Sha}", pushed.Branch, pushed.HeadSha);
                }
            }

            var sent = await _federationSender.SendFederatedRequestAsync(claims.ProjectPath, targetOwnerId, payload);
            if (!sent)
                return StatusCode(503, new { error = "Nessuna connessione federata attiva per il progetto (città accesa e relay raggiungibile?)." });

            // Send riuscito: solo ora l'hop è consumato e la correlazione persistita. Ri-leggo la
            // conversazione FRESCA e incremento di 1 (delta), NON riscrivo il valore assoluto
            // calcolato prima del send: due richieste concorrenti sulla stessa conversazione
            // devono sommare entrambi gli hop, non sovrascriversi (altrimenti il guardrail
            // anti-loop regredirebbe). La correlazione non si clobbera se già impostata.
            if (conv != null)
            {
                _session.BeginTransaction();
                var dal = _session.GetDal<AgentConversation>();
                var fresh = dal.GetList().FirstOrDefault(c => c.Id == convId);
                if (fresh != null)
                {
                    fresh.HopCount = fresh.HopCount + 1;
                    fresh.FederationId = fresh.FederationId ?? fedId;
                    fresh.RemoteOwner = entry.GitEmail;
                    fresh.RemoteAgent = targetAgent;
                    fresh.LastActivityAt = DateTime.UtcNow;
                    dal.Save(fresh);
                }
                _session.Commit();
            }

            // Ledger LATO ORIGINE (Fase 7a): registra la richiesta smistata come pending, con la
            // stessa RequestId inviata nel payload = chiave di correlazione dell'esito che tornerà.
            // OriginAgent = l'agente locale da risvegliare al ritorno (claims certificati, NON
            // StartedBy che vale spesso "user"). Persistito solo dopo un send riuscito.
            _session.BeginTransaction();
            _session.GetDal<FederationDispatch>().Save(new FederationDispatch
            {
                RequestId = Guid.Parse(payload.RequestId),
                FederationId = fedId,
                ProjectPath = claims.ProjectPath,
                ConversationId = convId,
                OriginAgent = claims.AgentName,
                TargetOwner = targetOwnerId,
                TargetAgent = targetAgent,
                Topics = AgentTopics.Join(request.Topics),
                Status = FederationDispatch.StatusEnum.Pending,
                CreatedAt = DateTime.UtcNow,
            });
            _session.Commit();

            _logger.LogInformation("[A2A/request-intervention] {From} → ambito '{Scope}' ({Agent}@{Owner}), fed {Fed}",
                claims.AgentName, entry.Scope, targetAgent, entry.GitEmail, fedId);
            return Ok(new
            {
                requested = true,
                federationId = fedId.ToString(),
                targetAgent,
                targetOwner = entry.GitEmail,
            });
        }

        /// <summary>
        /// Il ritorno del cerchio (Fase 7a): l'agente <b>B</b> chiama qui quando conclude un
        /// intervento delegato. Dalla sua conversazione federata (claims certificati) ricava
        /// <c>RequestId</c> + controparte e spedisce all'origine un <c>intervention-result</c>
        /// cifrato. Rotta assoluta: sta col canale autenticato (RunToken), non con la UI loopback.
        /// </summary>
        [HttpPost("~/api/A2A/federation/result")]
        public async System.Threading.Tasks.Task<IActionResult> ReportResult([FromBody] FederationResultRequest request)
        {
            var claims = ResolveClaims();
            if (claims == null)
                return Unauthorized(new { error = "RunToken assente o non valido." });
            if (request == null || string.IsNullOrWhiteSpace(request.Verdict))
                return BadRequest(new { error = "verdict è obbligatorio." });

            if (!Guid.TryParse(claims.ConversationId, out var convId))
                return UnprocessableEntity(new { error = "Il RunToken non porta una conversazione valida." });

            AgentConversation conv;
            _session.BeginTransaction();
            conv = _session.GetDal<AgentConversation>().GetList().FirstOrDefault(c => c.Id == convId);
            _session.Commit();
            if (conv == null)
                return NotFound(new { error = "Conversazione non trovata." });

            // Deve essere il lato DESTINAZIONE di una federazione: RemoteOwner (l'origine) e
            // RequestId (il ponte di 7a) sono le due precondizioni per correlare l'esito.
            if (string.IsNullOrWhiteSpace(conv.RemoteOwner))
                return UnprocessableEntity(new { error = "La conversazione non è federata: nessun esito da riportare." });
            if (!conv.RequestId.HasValue)
                return UnprocessableEntity(new { error = "La conversazione federata non porta un RequestId: impossibile correlare l'esito all'origine." });

            var targetOwnerId = FederationRoom.ComputeUserId(conv.RemoteOwner);

            var payload = new FederatedResultPayload
            {
                Kind = FederationKind.InterventionResult,
                RequestId = conv.RequestId.Value.ToString(),
                FederationId = conv.FederationId?.ToString(),
                Verdict = request.Verdict.Trim(),
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                Topics = request.Topics,
            };

            var sent = await _federationSender.SendFederatedResultAsync(claims.ProjectPath, targetOwnerId, payload);
            if (!sent)
                return StatusCode(503, new { error = "Nessuna connessione federata attiva per il progetto (città accesa e relay raggiungibile?)." });

            _logger.LogInformation("[A2A/federation/result] {Agent} → esito '{Verdict}' per req {Req} verso {Owner}",
                claims.AgentName, payload.Verdict, payload.RequestId, conv.RemoteOwner);
            return Ok(new { reported = true, requestId = payload.RequestId, verdict = payload.Verdict });
        }

        private static string ResolveLocalGitEmail(string projectPath)
        {
            try
            {
                using var repo = new LibGit2Sharp.Repository(projectPath);
                return repo.Config.Get<string>("user.email")?.Value;
            }
            catch { return null; }
        }

        private RunTokenClaims ResolveClaims()
        {
            var token = Request.Headers[RunTokenHeader].ToString();
            return string.IsNullOrWhiteSpace(token) ? null : _tokens.Validate(token);
        }
    }

    /// <summary>
    /// Body del RequestIntervention. Campi nullable (memoria dto_nullable_implicit_required):
    /// la validazione automatica risponderebbe 400 prima del check del token.
    /// </summary>
    public class RequestInterventionRequest
    {
        public string? Scope { get; set; }
        public string? Message { get; set; }
        public string? PreferredAgent { get; set; }
        public List<string>? Topics { get; set; }
    }

    /// <summary>
    /// Body dell'<c>intervention-result</c> (Fase 7a). Campi nullable di proposito
    /// (dto_nullable_implicit_required): <c>Verdict</c> non-nullable risponderebbe 400 PRIMA del
    /// check del token — lo validiamo a mano dopo l'auth, così un non autenticato vede 401.
    /// </summary>
    public class FederationResultRequest
    {
        /// <summary>Esito: <c>success</c> | <c>rejected</c> | <c>not-ready</c> (FederationVerdict).</summary>
        public string? Verdict { get; set; }
        /// <summary>Reason codificato opzionale (FederationReason): <c>not-for-me</c>, <c>merge-conflict-with-main</c>, …</summary>
        public string? Reason { get; set; }
        public List<string>? Topics { get; set; }
    }

    /// <summary>
    /// Body del send autenticato. Il mittente NON è qui: viene dal RunToken. Campi
    /// nullable di proposito: la validazione automatica di <c>[ApiController]</c> tratta
    /// i reference type non-nullable come <c>[Required]</c> e risponderebbe 400 PRIMA del
    /// nostro controllo del token — vogliamo invece 401 per un chiamante non autenticato
    /// (vedi memoria dto_nullable_implicit_required).
    /// </summary>
    public class SendAgentMessageRequest
    {
        public string? ToAgent { get; set; }
        public string? Message { get; set; }

        /// <summary>Argomenti dichiarati dal mittente (§8): metadata di contesto, opzionale.</summary>
        public List<string>? Topics { get; set; }
    }
}
