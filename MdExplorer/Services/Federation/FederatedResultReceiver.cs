using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.AgentMemory;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Federation;
using MdExplorer.Services.AgentMemory;
using MdExplorer.Services.AgentRegistry;
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
        Task Receive(string projectPath, FederatedResultPayload payload);
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

        // Delta di confidence sul fatto di routing (Fase 7b). Reinforce/erode per delta: la
        // conoscenza è volatile e apprende su più episodi, non si sovrascrive.
        private const double ReinforceDelta = 0.10;    // success → il routing ha funzionato
        private const double ErodeDelta = -0.15;       // rejected → scelta sbagliata (peso maggiore)
        private const double LightErodeDelta = -0.05;  // not-ready → precondizione mancante, non colpa del routing
        private const double InitialConfidence = 0.6;  // confidence moderata alla prima comparsa del fatto
        private const int MemoryScanLimit = 500;       // fatti scanditi per trovare quello pertinente

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentMailbox _mailbox;
        private readonly IAgentRegistryService _registry;
        private readonly IAgentMemoryService _memory;
        private readonly IFusekiConnectionResolver _fusekiResolver;
        private readonly ILogger<FederatedResultReceiver> _logger;

        public FederatedResultReceiver(
            IServiceScopeFactory scopeFactory,
            IAgentMailbox mailbox,
            IAgentRegistryService registry,
            IAgentMemoryService memory,
            IFusekiConnectionResolver fusekiResolver,
            ILogger<FederatedResultReceiver> logger)
        {
            _scopeFactory = scopeFactory;
            _mailbox = mailbox;
            _registry = registry;
            _memory = memory;
            _fusekiResolver = fusekiResolver;
            _logger = logger;
        }

        public async Task Receive(string projectPath, FederatedResultPayload payload)
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
                // fallback silenzioso: lo si registra perché l'umano possa riprenderlo. NON
                // usciamo: la memoria (7b) va comunque aggiornata — l'esito è noto.
                _logger.LogWarning(
                    "[Federation] esito per RequestId {Req} correlato ma NON consegnato all'agente '{Agent}': {Reason}",
                    requestId, dispatch.OriginAgent, enqueue.RejectionReason);
            }

            // Fase 7b — memoria dagli esiti: reinforce/erode della confidence dell'agente
            // d'origine + fatto sul CHI/COSA. Best-effort: se Fuseki è disabilitato è il caso
            // normale (niente memoria, non un errore); se è abilitato ma rotto, fail-loud.
            await ReconcileMemoryAsync(projectPath, dispatch, verdict, reason);

            _logger.LogInformation(
                "[Federation] esito '{Verdict}' per RequestId {Req} → risvegliato '{Agent}' nella conversazione {Conv}.",
                verdict, requestId, dispatch.OriginAgent, dispatch.ConversationId);
        }

        /// <summary>
        /// Aggiorna la memoria dell'agente d'origine in base al verdict (Fase 7b). Il fatto di
        /// <b>routing</b> ("delegare [topic] a &lt;agente&gt; è affidabile") è STABILE tra gli
        /// episodi: success lo rinforza, rejected/not-ready lo erodono (per delta, non assoluto,
        /// così apprende su più esiti). In più: <c>rejected</c> asserisce un fatto sul CHI
        /// (routing sbagliato); <c>not-ready</c> asserisce la <b>precondizione</b> sul COSA
        /// (azionabile PRIMA del prossimo invio — shift-left del check da B verso A).
        /// </summary>
        private async Task ReconcileMemoryAsync(string projectPath, FederationDispatch dispatch, string verdict, string reason)
        {
            // Identità dell'agente d'origine: il grafo di memoria è forzato da AgentIdentity.Id
            // (stabile al rename), non dal nome. Non risolvibile → niente memoria (non è un errore).
            var entry = _registry.RefreshCatalog(projectPath)
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, dispatch.OriginAgent, StringComparison.OrdinalIgnoreCase));
            if (entry?.IdentityId == null)
            {
                _logger.LogInformation("[Federation/7b] identità di '{Agent}' non risolvibile: nessun aggiornamento di memoria.", dispatch.OriginAgent);
                return;
            }

            FusekiConnection conn;
            try
            {
                conn = await _fusekiResolver.ResolveAsync(projectPath);
            }
            catch (Exception ex)
            {
                // Fuseki abilitato (addon gestito?) ma non risolvibile: fail-loud, non un finto ok.
                _logger.LogWarning(ex, "[Federation/7b] risoluzione Fuseki fallita per '{Project}': memoria non aggiornata.", projectPath);
                return;
            }
            if (conn == null)
            {
                // Memoria non abilitata per il progetto: caso NORMALE, l'unico "niente memoria" lecito.
                _logger.LogDebug("[Federation/7b] memoria (Fuseki) non abilitata per '{Project}': nessun aggiornamento.", projectPath);
                return;
            }

            var identityId = entry.IdentityId.Value;
            var graph = AgentMemoryGraphs.ForAgent(identityId);
            var topics = AgentTopics.Split(dispatch.Topics);
            var tagsText = topics.Count > 0 ? string.Join(", ", topics) : dispatch.TargetAgent;

            // Delta sul fatto di routing secondo il verdict.
            double routingDelta;
            switch (verdict)
            {
                case FederationVerdict.Success: routingDelta = ReinforceDelta; break;
                case FederationVerdict.Rejected: routingDelta = ErodeDelta; break;
                case FederationVerdict.NotReady: routingDelta = LightErodeDelta; break;
                default:
                    _logger.LogInformation("[Federation/7b] verdict '{Verdict}' non riconosciuto: nessun aggiustamento di routing.", verdict);
                    return;
            }

            try
            {
                // Un solo scan del grafo: lo riuso sia per il routing sia per il fatto CHI/COSA.
                var existing = await _memory.ListAsync(conn, new[] { graph }, MemoryScanLimit);

                var routingStatement = $"routing verso '{dispatch.TargetAgent}' per [{tagsText}] è affidabile";
                await UpsertByDeltaAsync(conn, identityId, graph, existing, routingStatement, topics, routingDelta, dispatch);

                if (verdict == FederationVerdict.Rejected)
                {
                    // Fatto sul CHI: la scelta di routing è sbagliata (aboutTag not-for-me).
                    var whoStatement = $"'{dispatch.TargetAgent}' non è competente per [{tagsText}]";
                    var whoTags = topics.Concat(new[] { FederationReason.NotForMe }).ToList();
                    await UpsertByDeltaAsync(conn, identityId, graph, existing, whoStatement, whoTags, ReinforceDelta, dispatch);
                }
                else if (verdict == FederationVerdict.NotReady && !string.IsNullOrWhiteSpace(reason))
                {
                    // Fatto sul COSA: la precondizione mancante (aboutTag = reason macchina).
                    var whatStatement = $"prima di delegare [{tagsText}] a '{dispatch.TargetAgent}' serve: {reason}";
                    var whatTags = topics.Concat(new[] { reason }).ToList();
                    await UpsertByDeltaAsync(conn, identityId, graph, existing, whatStatement, whatTags, ReinforceDelta, dispatch);
                }

                _logger.LogInformation("[Federation/7b] memoria di '{Agent}' aggiornata (verdict '{Verdict}', delta routing {Delta:+0.00;-0.00}).",
                    dispatch.OriginAgent, verdict, routingDelta);
            }
            catch (Exception ex)
            {
                // Fuseki abilitato ma la scrittura è fallita: fail-loud, il ledger/wake restano validi.
                _logger.LogWarning(ex, "[Federation/7b] aggiornamento memoria fallito per '{Agent}' (Fuseki abilitato ma non scrivibile?).", dispatch.OriginAgent);
            }
        }

        /// <summary>
        /// Upsert per delta di un fatto identificato dal suo <paramref name="statement"/>: se esiste
        /// già nel grafo, ne aggiusta la confidence di <paramref name="delta"/> (clamp [0,1]); se
        /// non esiste, lo asserisce a confidence moderata (già orientata dal primo esito).
        /// </summary>
        private async Task UpsertByDeltaAsync(
            FusekiConnection conn, Guid identityId, string graph,
            IReadOnlyList<MemoryFactDetail> existing, string statement,
            IReadOnlyList<string> aboutTags, double delta, FederationDispatch dispatch)
        {
            var match = existing.FirstOrDefault(f => string.Equals(f.Statement, statement, StringComparison.Ordinal));
            if (match != null)
            {
                var newConfidence = Math.Clamp(match.Confidence + delta, 0.0, 1.0);
                await _memory.SetConfidenceAsync(conn, graph, match.FactUri, newConfidence);
            }
            else
            {
                await _memory.AssertFactAsync(conn, identityId, new LearnedFactInput
                {
                    Statement = statement,
                    Confidence = Math.Clamp(InitialConfidence + delta, 0.0, 1.0),
                    AboutTags = aboutTags,
                    // Provenance: il fatto è appreso dall'ESITO di questo intervento federato →
                    // la RequestId è il "run" che l'ha prodotto (correlabile al dispatch).
                    RunId = dispatch.RequestId,
                    ConversationId = dispatch.ConversationId.ToString(),
                    CreatedAtUtc = DateTime.UtcNow,
                });
            }
        }
    }
}
