using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Features.Agents.Workflow.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// L'esecutore del workflow (S3): il ponte tra ciò che succede nel servizio (un lancio, una consegna, un'approvazione,
    /// un rifiuto, un pulsante, un «Avvia» dalla posta) e lo schedulatore. Scrive l'evento nel registro del giro, chiede
    /// allo schedulatore che cosa fare per i passi dei <b>propri</b> agenti, e lo fa: accoda l'incarico con il brief, o lo
    /// mette «da avviare». Senza un workflow attivo non fa niente: tutto resta come prima.
    /// </summary>
    public interface IAgentWorkflowExecutor
    {
        /// <summary>Il progetto ha un workflow configurato (anche se con errori: allora non si fa niente, e lo si dice).</summary>
        bool IsActive(string projectPath);

        /// <summary>
        /// I passi che questo computer ha passato a un altro e che da 24 ore non danno notizie: un avviso ciascuno, una volta
        /// (17/10/2026, avviso di lavoro non processato). Quanti ne ha avvisati.
        /// </summary>
        int CheckStalledSteps(string projectPath, DateTime now);

        /// <summary>La persona lancia un agente a mano: se il workflow ha un passo di lancio per lui, si apre un giro.</summary>
        string OnLaunched(string projectPath, string agentName, string runId);

        /// <summary>Un turno è finito: consegna, conclusione senza artefatto, o fallimento.</summary>
        void OnRunEnded(string projectPath, string agentName, string runId, Guid? triggerMessageId, AgentMergeRequest delivered, bool succeeded, string error);

        /// <summary>
        /// La persona ha approvato o rifiutato un artefatto. <paramref name="mergeCommit"/>: per un'approvazione, il commit
        /// del ramo principale in cui la versione approvata è entrata.
        /// </summary>
        void OnDecided(AgentMergeRequest request, string mergeCommit = null);

        /// <summary>«Fai ripartire» un lavoro rifiutato di un giro. False = non è di un giro: si fa come sempre.</summary>
        bool TryRework(AgentMergeRequest request);

        /// <summary>
        /// Per un lavoro rifiutato di un giro: quante volte si può ancora far ripartire. Null = senza limite, o non è di un giro
        /// (<paramref name="linked"/> dice quale dei due).
        /// </summary>
        int? ReworksLeft(AgentMergeRequest request, out bool linked, out string why);

        /// <summary>
        /// La persona ha premuto un pulsante sotto un messaggio. False = il workflow non lo gestisce: si risponde all'agente come
        /// sempre. <paramref name="assign"/>: a chi va ogni passo che il pulsante fa partire, per gli agenti di un team (W22).
        /// Throws <see cref="InvalidOperationException"/>, con cosa fare, se manca una scelta o non va bene.
        /// </summary>
        /// <paramref name="values"/>: i valori che la persona ha scritto per l'esito di un suo passo (workflow v3, F4).
        bool TryReply(string projectPath, AgentMessage answered, ResolvedReply chosen, IReadOnlyDictionary<string, string> assign = null,
            IReadOnlyDictionary<string, string> values = null);

        /// <summary>
        /// Il «tocca a te» del passo di una persona non aspetta più un esito: l'ha dato, o il passo non c'è più. Per la posta:
        /// un messaggio chiuso non è «da fare».
        /// </summary>
        bool IsPersonStepClosed(AgentMessage message);

        /// <summary>I passi che un pulsante farebbe partire, con chi ne può rispondere: per far scegliere chi preme (W22).</summary>
        IReadOnlyList<ReplyTarget> ReplyTargets(string projectPath, AgentMessage answered, string replyId);

        /// <summary>
        /// Chi ha un passo «da avviare» lo passa a un collega che risponde dello stesso agente (W22: per esempio perché va in
        /// ferie). Throws <see cref="InvalidOperationException"/>, con cosa fare, quando non si può.
        /// </summary>
        void PassTo(AgentMessage held, string toEmail, string note);

        /// <summary>
        /// «Prendo io»: il passo era di chi non risponde più dell'agente e l'agente ha un team; lo prende chi ne risponde, da
        /// questo computer. Il registro lo scrive, e il passo riparte da qui (da avviare, o da approvare).
        /// </summary>
        void TakeOver(string projectPath, string roundId, string stepId);

        /// <summary>Il responsabile ha avviato un passo «da avviare».</summary>
        void OnOwnerStarted(AgentMessage message);

        /// <summary>Il responsabile non ha avviato un passo «da avviare».</summary>
        void OnOwnerDeclined(AgentMessage message, string reason);

        /// <summary>
        /// Dopo un cambio di responsabile (una delega, la sua fine): i passi in sospeso dei giri seguono l'agente. Su questo
        /// computer ricompaiono quelli degli agenti diventati miei («da avviare», consegne da approvare); quelli degli agenti
        /// non più miei escono dalla posta, con il motivo. Poi lo schedulatore fa partire ciò che ora tocca a me.
        /// </summary>
        void Reconcile(string projectPath, bool refresh = true);

        /// <summary>
        /// Perché questa persona non può decidere su un passo di un giro (ne risponde un altro); null se può, o se non è di un
        /// giro. Un passo ha un solo computer che ne scrive il registro.
        /// </summary>
        string NotYours(AgentMessage message);

        /// <summary>Come sopra, per una richiesta di approvazione: vale solo se è di un giro.</summary>
        string NotYours(AgentMergeRequest request);

        /// <summary>
        /// A che punto sono i giri aperti dal messaggio di un turno (chi ha premuto il pulsante li segue da lì): una riga per
        /// passo, letta dal registro. Vuoto se quel turno non ha aperto giri.
        /// </summary>
        IReadOnlyList<RoundProgress> ProgressOfRun(string projectPath, string runId);

        /// <summary>I giri del progetto, dal più recente: per la posta, dove ogni giro è una voce con i suoi passi e i suoi messaggi.</summary>
        IReadOnlyList<RoundSummary> Rounds(string projectPath);
    }

    /// <summary>Un giro, visto dalla posta.</summary>
    public sealed class RoundSummary
    {
        public string Id { get; init; }
        /// <summary>Il titolo del workflow («Gara»).</summary>
        public string Title { get; init; }
        /// <summary>I valori del giro (il bando: «NC-2027-014»), nell'ordine delle variabili del workflow.</summary>
        public IReadOnlyList<string> Values { get; init; } = Array.Empty<string>();
        public DateTime StartedAt { get; init; }
        public string StartedBy { get; init; }
        /// <summary>L'ultimo evento del giro: per ordinare e per dire «aggiornato».</summary>
        public DateTime LastActivityAt { get; init; }
        /// <summary>Tutti i passi sono conclusi (approvati, conclusi, non avviati per scelta, falliti).</summary>
        public bool Finished { get; init; }
        /// <summary>
        /// Non concluso ma fermo: nessun passo può più andare avanti senza che qualcuno ne faccia ripartire uno rifiutato,
        /// non avviato o fallito (<see cref="WorkflowScheduler.IsStopped"/>). Si può archiviare come un giro concluso.
        /// </summary>
        public bool Stopped { get; init; }
        public int StepsDone { get; init; }
        public int StepsTotal { get; init; }
        public IReadOnlyList<RoundProgress> Steps { get; init; } = Array.Empty<RoundProgress>();
        /// <summary>I turni di lavoro del giro: i messaggi scritti da questi turni sono messaggi del giro.</summary>
        public IReadOnlyList<string> Runs { get; init; } = Array.Empty<string>();
    }

    /// <summary>Un passo di un giro visto da chi lo aspetta.</summary>
    public sealed class RoundProgress
    {
        public string Id { get; init; }
        public string Round { get; init; }
        public string Step { get; init; }
        public string Label { get; init; }
        public string Agent { get; init; }
        /// <summary>Il passo di una persona: il suo ruolo (e <see cref="Agent"/> è null).</summary>
        public string Person { get; init; }
        /// <summary>Chi ne risponde, se si sa.</summary>
        public string Owner { get; init; }
        /// <summary>Il passo è di chi non risponde più dell'agente e io posso prenderlo («Prendo io»).</summary>
        public bool CanTake { get; init; }
        /// <summary>waiting · tostart · asked (tocca a una persona) · declined · working · approval · approved · rejected · done · failed</summary>
        public string State { get; init; }
        public string Note { get; init; }
        /// <summary>
        /// Il turno che ha consegnato (o concluso) il passo non ha scritto alla persona: la scheda dell'agente lo chiede, il
        /// modello a volte se ne dimentica. Lo si dice, così chi segue il giro non aspetta un messaggio che non arriverà.
        /// </summary>
        public bool Silent { get; init; }
    }

    /// <summary>Un passo che un pulsante fa partire, e chi ne può rispondere.</summary>
    public sealed class ReplyTarget
    {
        public string Step { get; init; }
        public string Label { get; init; }
        public string Agent { get; init; }
        /// <summary>Il passo di una persona: il suo ruolo (e <see cref="Agent"/> è null).</summary>
        public string Person { get; init; }
        public IReadOnlyList<AgentOwnerPerson> Owners { get; init; } = Array.Empty<AgentOwnerPerson>();
        /// <summary>Più responsabili: chi preme deve scegliere.</summary>
        public bool NeedsChoice => Owners.Count > 1;
    }

    public class AgentWorkflowExecutor : IAgentWorkflowExecutor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MdExplorer.Services.IProjectMetadataService _metadata;
        private readonly IAgentWakeGuard _wakeGuard;
        private readonly MdExplorer.Services.Federation.IEffectiveOwnerIdentity _identity;
        private readonly IAgentMailbox _mailbox;
        private readonly IRoundStore _store;
        private readonly MdExplorer.Services.IProjectOwnershipService _ownership;
        private readonly IWorkflowBell _bell;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<MdExplorer.Hubs.MonitorMDHub> _hub;
        private readonly ILogger<AgentWorkflowExecutor> _logger;

        /// <summary>Un lock per progetto: registro e git di un progetto si toccano uno alla volta.</summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>Gli avvisi «bloccato» già dati: lo schedulatore ripete l'azione finché la causa resta, l'avviso no.</summary>
        private readonly ConcurrentDictionary<string, byte> _told = new();

        /// <summary>La lingua dell'interfaccia: incarichi e avvisi in posta la seguono (16/10/2026).</summary>
        private readonly MdExplorer.Features.Services.IUiLanguage _language;
        private readonly IOverdueWorkWatcher _overdue;

        public AgentWorkflowExecutor(
            IServiceScopeFactory scopeFactory,
            MdExplorer.Services.IProjectMetadataService metadata,
            IAgentWakeGuard wakeGuard,
            MdExplorer.Services.Federation.IEffectiveOwnerIdentity identity,
            IAgentMailbox mailbox,
            IRoundStore store,
            MdExplorer.Services.IProjectOwnershipService ownership,
            IWorkflowBell bell,
            Microsoft.AspNetCore.SignalR.IHubContext<MdExplorer.Hubs.MonitorMDHub> hub,
            ILogger<AgentWorkflowExecutor> logger,
            MdExplorer.Features.Services.IUiLanguage language,
            IOverdueWorkWatcher overdue)
        {
            _language = language;
            _overdue = overdue;
            _bell = bell;
            _hub = hub;
            _ownership = ownership;
            _store = store;
            _scopeFactory = scopeFactory;
            _metadata = metadata;
            _wakeGuard = wakeGuard;
            _identity = identity;
            _mailbox = mailbox;
            _logger = logger;
        }

        public bool IsActive(string projectPath)
            => !string.IsNullOrWhiteSpace(_metadata.GetAgentCity(projectPath)?.WorkflowDoc);

        // ---- gli eventi ----

        public string OnLaunched(string projectPath, string agentName, string runId)
        {
            var wf = Workflow(projectPath, out var jsonPath);
            var launch = wf?.Steps.FirstOrDefault(s => Same(s.Agent, agentName) && s.Trigger?.Kind == WorkflowTriggerKind.Launch);
            if (launch == null) return null;
            if (!_store.IsAvailable(projectPath))
            {
                // Il registro dei giri vive in git: senza, il giro non si apre, e lo si dice invece di farlo sparire.
                _logger.LogError("[Workflow] {Project} non è un repository git: il giro di «{Step}» non si apre.", projectPath, launch.Label);
                TellUser(projectPath, agentName, MdExplorer.Features.Agents.AgentTexts.Pick(_language.Current,
                    $"Il workflow non può aprire il giro di «{launch.Label}»: il progetto non è un repository git, e il registro dei giri vive in git.",
                    $"The workflow cannot open the round of «{launch.Label}»: the project is not a git repository, and the round log lives in git."));
                return null;
            }

            return Locked(projectPath, () =>
            {
                var now = DateTime.UtcNow;
                var id = RoundLedger.NewRoundId(jsonPath, now);
                RoundLedger.Open(_store.Root(projectPath), new RoundHeader { Id = id, Workflow = jsonPath, StartedAt = now, StartedBy = Me(projectPath) });
                SnapshotWorkflow(projectPath, id, jsonPath, wf);
                Write(projectPath, id, launch, new RoundEvent { Type = RoundEventType.Started, At = now, By = Me(projectPath), Run = runId },
                    $"giro {id}: «{launch.Label}» lanciato");
                _logger.LogInformation("[Workflow] giro {Round} aperto: {Agent} lanciato a mano (passo '{Step}')", id, agentName, launch.Id);
                return id;
            });
        }

        public void OnRunEnded(string projectPath, string agentName, string runId, Guid? triggerMessageId, AgentMergeRequest delivered, bool succeeded, string error)
        {
            var link = triggerMessageId != null ? LinkOfMessage(triggerMessageId.Value) : LinkOfRun(projectPath, runId);
            if (link == null) return;
            var wf = WorkflowFor(projectPath, link.RoundId);
            var step = wf?.Step(link.Step);
            if (step == null) return;

            RoundEvent evt;
            if (delivered != null)
                evt = new RoundEvent
                {
                    Type = RoundEventType.Delivered, Branch = delivered.PublishedBranch, Commit = delivered.HeadSha, Run = runId,
                    Files = DeliveredFiles(delivered),
                };
            else if (succeeded && step.Produces.Count > 0)
                // Doveva consegnare e non l'ha fatto: è un fallimento che si dice, non un «concluso» che fa andare avanti.
                evt = new RoundEvent { Type = RoundEventType.Failed, Run = runId, Note = $"«{step.Label}» doveva consegnare {string.Join(", ", step.Produces)} e non ha scritto niente." };
            else if (succeeded)
                evt = new RoundEvent { Type = RoundEventType.Done, Run = runId };
            else
                evt = new RoundEvent { Type = RoundEventType.Failed, Run = runId, Note = error };

            evt.At = DateTime.UtcNow; evt.By = Me(projectPath); evt.Round = link.Round; evt.Attempt = link.Attempt;
            Locked(projectPath, () => Write(projectPath, link.RoundId, step, evt, $"giro {link.RoundId}: «{step.Label}» {Verb(evt.Type)}"));
            Evaluate(projectPath, link.RoundId);
        }

        public void OnDecided(AgentMergeRequest request, string mergeCommit = null)
        {
            if (request == null) return;
            var type = request.Status == AgentMergeRequest.StatusEnum.Merged ? RoundEventType.Approved
                     : request.Status == AgentMergeRequest.StatusEnum.Rejected ? RoundEventType.Rejected
                     : null;
            if (type == null) return;
            var link = LinkOfRun(request.ProjectPath, request.RunId);
            if (link == null) return;
            var step = WorkflowFor(request.ProjectPath, link.RoundId)?.Step(link.Step);
            if (step == null) return;

            Locked(request.ProjectPath, () => Write(request.ProjectPath, link.RoundId, step, new RoundEvent
            {
                Type = type, At = DateTime.UtcNow, By = Me(request.ProjectPath), Round = link.Round, Attempt = link.Attempt,
                Run = request.RunId, Note = type == RoundEventType.Rejected ? request.Note : null,
                // La versione giudicata e, se approvata, dove è entrata: «approvato» vale per quel commit, non per il file.
                Commit = request.HeadSha, Merge = type == RoundEventType.Approved ? mergeCommit : null,
            }, $"giro {link.RoundId}: «{step.Label}» {Verb(type)}"));
            Evaluate(request.ProjectPath, link.RoundId);
        }

        public bool TryRework(AgentMergeRequest request)
        {
            var link = request == null ? null : LinkOfRun(request.ProjectPath, request.RunId);
            if (link == null) return false;
            var wf = WorkflowFor(request.ProjectPath, link.RoundId);
            var step = wf?.Step(link.Step);
            if (step == null) return false;

            var round = RoundLedger.Load(_store.Root(request.ProjectPath), link.RoundId);
            var view = WorkflowScheduler.View(wf, round, step);
            if (view.Status != StepStatus.Rejected)
                throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"«{step.Label}» non è fermo per un rifiuto ({view.Status}): non c'è niente da far ripartire.", $"«{step.Label}» is not stopped by a decline ({view.Status}): there is nothing to restart."));
            // Il limite c'è solo se l'autore l'ha scritto (W9).
            var until = wf.LoopOf(step.Id, WorkflowLoopKind.UntilApproved);
            if (until?.Max != null && view.RejectionsInRound > until.Max.Value)
                throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"«{step.Label}» è già stato rifatto {until.Max} {(until.Max == 1 ? "volta" : "volte")}: è il massimo che il workflow dà al ciclo «{until.Id}». Il lavoro resta fermo.", $"«{step.Label}» has already been redone {until.Max} {(until.Max == 1 ? "time" : "times")}: that is the most the workflow allows for the loop «{until.Id}». The work stays stopped."));

            // Lo stesso incarico, con gli stessi ingressi risolti: prima ripartiva senza i file da cui il passo parte.
            var action = WorkflowScheduler.Rework(wf, round, step, request.Note);
            Locked(request.ProjectPath, () => Execute(request.ProjectPath, link.RoundId, action, startedBy: Me(request.ProjectPath), resumed: false));
            return true;
        }

        public int? ReworksLeft(AgentMergeRequest request, out bool linked, out string why)
        {
            why = null;
            var link = request == null ? null : LinkOfRun(request.ProjectPath, request.RunId);
            linked = link != null;
            if (link == null) return null;
            var wf = WorkflowFor(request.ProjectPath, link.RoundId);
            var step = wf?.Step(link.Step);
            var until = step == null ? null : wf.LoopOf(step.Id, WorkflowLoopKind.UntilApproved);
            if (until?.Max == null) return null;
            var view = WorkflowScheduler.View(wf, RoundLedger.Load(_store.Root(request.ProjectPath), link.RoundId), step);
            var left = Math.Max(0, until.Max.Value - (view.RejectionsInRound - 1));
            if (left == 0)
                why = MdExplorer.Features.Services.UiText.Say($"«{step.Label}» è già stato rifatto {until.Max} {(until.Max == 1 ? "volta" : "volte")}: è il massimo che il workflow dà al ciclo «{until.Id}». Il lavoro resta fermo.", $"«{step.Label}» has already been redone {until.Max} {(until.Max == 1 ? "time" : "times")}: that is the most the workflow allows for the loop «{until.Id}». The work stays stopped.");
            return left;
        }

        public IReadOnlyList<ReplyTarget> ReplyTargets(string projectPath, AgentMessage answered, string replyId)
        {
            var link = answered == null ? null : SourceOf(projectPath, answered);
            var wf = link == null ? null : WorkflowFor(projectPath, link.RoundId);
            if (wf == null) return Array.Empty<ReplyTarget>();
            return wf.Steps.Where(s => s.Trigger?.Kind == WorkflowTriggerKind.Reply && s.Trigger.FromStep == link.Step
                                       && string.Equals(s.Trigger.ReplyId, replyId, StringComparison.OrdinalIgnoreCase))
                .Select(s => new ReplyTarget { Step = s.Id, Label = s.Label, Agent = s.Agent, Person = s.Person, Owners = TeamOf(projectPath, s) })
                .ToList();
        }

        public bool TryReply(string projectPath, AgentMessage answered, ResolvedReply chosen, IReadOnlyDictionary<string, string> assign = null,
            IReadOnlyDictionary<string, string> values = null)
        {
            if (answered == null || chosen == null) return false;
            if (answered.TriggerSource == MdExplorer.Features.Agents.AgentTexts.PersonStepSource)
            {
                ClosePersonStep(answered, chosen, assign, values);
                return true;
            }
            var wf = Workflow(projectPath, out var jsonPath);
            if (wf == null) return false;
            var link = LinkOfRun(projectPath, answered.RunId);
            if (link == null) return false;
            // Il workflow fa partire qualcosa con questo pulsante sotto questo passo? Altrimenti risponde l'agente, come sempre.
            if (!wf.Steps.Any(s => s.Trigger?.Kind == WorkflowTriggerKind.Reply && s.Trigger.FromStep == link.Step
                                   && string.Equals(s.Trigger.ReplyId, chosen.Id, StringComparison.OrdinalIgnoreCase)))
                return false;
            if (chosen.Values == null)
                throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say("Questo pulsante è di prima dello schedulatore e non porta i suoi valori: rilancia la ricerca per avere i pulsanti nuovi.", "This button predates the scheduler and does not carry its values: launch the search again to get the new buttons."));

            // Chi preme sceglie a chi va ogni passo di un agente con un team (W22); con un solo responsabile non c'è scelta.
            var assignments = Assignments(projectPath, answered, chosen.Id, assign);

            var source = wf.Step(link.Step);
            var target = Locked(projectPath, () =>
            {
                // Tutti i giri nati da questa ricerca (lo stesso turno): uno per pulsante premuto (W19).
                var rounds = RoundsOfRun(projectPath, answered.RunId, source.Id);
                bool Pressed(RoundState r, out bool same)
                {
                    var p = r.EventsOf(source.Id).Where(e => e.Type == RoundEventType.Replied
                                                             && string.Equals(e.Reply, chosen.Id, StringComparison.OrdinalIgnoreCase)).ToList();
                    same = p.Any(e => SameValues(e.Values, chosen.Values));
                    return p.Count > 0;
                }
                var replied = new RoundEvent
                {
                    Type = RoundEventType.Replied, At = DateTime.UtcNow, By = Me(projectPath), Reply = chosen.Id,
                    Values = new Dictionary<string, string>(chosen.Values, StringComparer.Ordinal),
                    Assign = assignments.Count > 0 ? assignments : null,
                };

                foreach (var r in rounds)
                    if (Pressed(r, out var same) && same)
                        return null;   // questo pulsante ha già il suo giro
                var free = rounds.FirstOrDefault(r => !Pressed(r, out _));
                if (free != null)
                {
                    Write(projectPath, free.Header.Id, source, replied, $"giro {free.Header.Id}: «{chosen.Label}»");
                    return free.Header.Id;
                }

                // Un altro bando dalla stessa ricerca (W19): un giro nuovo, che parte da quella ricerca.
                var origin = rounds.First();
                var now = DateTime.UtcNow;
                var id = RoundLedger.NewRoundId(jsonPath, now);
                RoundLedger.Open(_store.Root(projectPath), new RoundHeader { Id = id, Workflow = jsonPath, StartedAt = now, StartedBy = Me(projectPath) });
                SnapshotWorkflow(projectPath, id, jsonPath, wf);
                foreach (var e in origin.EventsOf(source.Id).Where(e => e.Type != RoundEventType.Replied))
                    RoundLedger.Append(_store.Root(projectPath), id, source.Id, source.Agent, e);
                Write(projectPath, id, source, replied, $"giro {id}: «{chosen.Label}» (dalla ricerca del giro {origin.Header.Id})");
                _logger.LogInformation("[Workflow] giro {Round} aperto da «{Reply}» sul giro {From}", id, chosen.Label, origin.Header.Id);
                return id;
            });
            if (target != null) Evaluate(projectPath, target);
            return true;
        }

        /// <summary>
        /// Chi preme un pulsante sceglie a chi va ogni passo che fa partire, quando il suo agente o il suo ruolo è di un team
        /// (W22); con una persona sola non c'è scelta. Throws con cosa fare se manca una scelta o non va bene.
        /// </summary>
        private Dictionary<string, string> Assignments(string projectPath, AgentMessage answered, string replyId, IReadOnlyDictionary<string, string> assign)
        {
            var assignments = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var t in ReplyTargets(projectPath, answered, replyId))
            {
                var who = t.Person != null
                    ? MdExplorer.Features.Services.UiText.Say($"il ruolo «{t.Person}»", $"the role «{t.Person}»")
                    : MdExplorer.Features.Services.UiText.Say($"l'agente '{t.Agent}'", $"agent '{t.Agent}'");
                string to = null;
                assign?.TryGetValue(t.Step, out to);
                if (string.IsNullOrWhiteSpace(to))
                {
                    if (t.NeedsChoice)
                        throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say(
                            $"«{t.Label}»: {who} è di più persone ({string.Join(", ", t.Owners.Select(o => o.Email))}): scegli a chi va.",
                            $"«{t.Label}»: {who} belongs to more than one person ({string.Join(", ", t.Owners.Select(o => o.Email))}): choose who gets it."));
                    continue;
                }
                if (!t.Owners.Any(o => string.Equals(o.Email, to.Trim(), StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say(
                        $"«{t.Label}»: {to} non è fra chi ha {who} ({(t.Owners.Count == 0 ? "nessuno" : string.Join(", ", t.Owners.Select(o => o.Email)))}).",
                        $"«{t.Label}»: {to} is not among those who have {who} ({(t.Owners.Count == 0 ? "no one" : string.Join(", ", t.Owners.Select(o => o.Email)))})."));
                assignments[t.Step] = to.Trim().ToLowerInvariant();
            }
            return assignments;
        }

        /// <summary>
        /// La persona chiude il suo passo con un esito (workflow v3, F4). Solo chi ne risponde, solo mentre le tocca. Prima si
        /// controllano i valori dell'esito e i documenti che il passo dichiara: scritti, nel commit, pubblicati. Poi il registro
        /// scrive l'esito (con i valori e le scelte per i passi che fa partire) e la chiusura (documenti, commit, versioni ricevute).
        /// </summary>
        private void ClosePersonStep(AgentMessage message, ResolvedReply chosen, IReadOnlyDictionary<string, string> assign,
            IReadOnlyDictionary<string, string> values)
        {
            var projectPath = message.ProjectPath;
            var wf = WorkflowFor(projectPath, message.WorkflowRound)
                ?? throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say("Il progetto non ha un workflow attivo.", "The project has no active workflow."));
            var step = wf.Step(message.WorkflowStep);
            if (step?.IsPerson != true)
                throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"Il passo '{message.WorkflowStep}' non è il passo di una persona nel workflow del giro.", $"Step '{message.WorkflowStep}' is not a person's step in the round's workflow."));
            var outcome = step.Outcomes.FirstOrDefault(o => string.Equals(o.Id, chosen.Id, StringComparison.Ordinal))
                ?? throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"'{chosen.Id}' non è un esito di «{step.Label}».", $"'{chosen.Id}' is not an outcome of «{step.Label}»."));

            var carried = new Dictionary<string, string>(StringComparer.Ordinal);
            var missing = new List<string>();
            foreach (var name in outcome.Values)
            {
                string v = null;
                values?.TryGetValue(name, out v);
                if (string.IsNullOrWhiteSpace(v)) missing.Add(name);
                else carried[name] = v.Trim();
            }
            if (missing.Count > 0)
                throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say(
                    $"«{outcome.Label}» porta {string.Join(", ", missing.Select(n => "{" + n + "}"))}: scrivilo prima di premere.",
                    $"«{outcome.Label}» carries {string.Join(", ", missing.Select(n => "{" + n + "}"))}: write it before pressing."));

            var assignments = Assignments(projectPath, message, outcome.Id, assign);
            var me = Me(projectPath);
            Locked(projectPath, () =>
            {
                var round = RoundLedger.Load(_store.Root(projectPath), message.WorkflowRound);
                var owner = OwnerOf(projectPath, wf, round, step);
                if (!Same(owner.Email, me))
                    throw new InvalidOperationException(owner.Email != null
                        ? MdExplorer.Features.Services.UiText.Say($"«{step.Label}» tocca a {owner.Email}: lo chiude lei, dal suo computer.", $"«{step.Label}» is {owner.Email}'s turn: they close it, from their own computer.")
                        : owner.Problem);
                var view = WorkflowScheduler.View(wf, round, step);
                if (view.Status != StepStatus.Asked)
                    throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"«{step.Label}» non aspetta un esito ({view.Status}).", $"«{step.Label}» is not waiting for an outcome ({view.Status})."));

                var (files, commit) = PersonDocuments(projectPath, step, view.Last?.Commit);
                var received = WorkflowScheduler.Ask(wf, round, step).Inputs.SelectMany(i => i.Files)
                    .Select(f => f.Commit == null ? f.Path : f.Path + "@" + f.Commit).ToList();
                var now = DateTime.UtcNow;
                RoundLedger.Append(_store.Root(projectPath), message.WorkflowRound, step.Id, null, new RoundEvent
                {
                    Type = RoundEventType.Replied, At = now, By = me, Reply = outcome.Id,
                    Values = carried.Count > 0 ? carried : null, Assign = assignments.Count > 0 ? assignments : null,
                });
                Write(projectPath, message.WorkflowRound, step, new RoundEvent
                {
                    Type = RoundEventType.Done, At = now, By = me, Files = files, Commit = commit,
                    Received = received.Count > 0 ? received : null,
                }, $"giro {message.WorkflowRound}: «{step.Label}» — {outcome.Label}");
            });
            MarkRead(message.Id);
            _logger.LogInformation("[Workflow] giro {Round}: «{Step}» chiuso da {Me} con «{Outcome}»", message.WorkflowRound, step.Label, me, outcome.Label);
            Evaluate(projectPath, message.WorkflowRound);
        }

        /// <summary>
        /// I documenti che il passo di una persona dichiara (<c>produces</c>): per ognuno almeno un file cambiato da quando il
        /// passo le è toccato, senza modifiche fuori dal commit, e il commit pubblicato (chi viene dopo lo legge dal suo
        /// computer). Niente di questo si indovina: manca qualcosa = si dice che cosa fare. Torna i file e il loro ultimo commit.
        /// </summary>
        private (List<string> Files, string Commit) PersonDocuments(string projectPath, WorkflowStep step, string askedCommit)
        {
            if (step.Outputs.Count == 0) return (null, null);
            string Say(string it, string en) => MdExplorer.Features.Services.UiText.Say(it, en);
            if (string.IsNullOrWhiteSpace(askedCommit))
                throw new InvalidOperationException(Say($"Il registro non dice da quale commit «{step.Label}» è toccato a te: non si può sapere quali documenti hai scritto.",
                    $"The ledger does not say from which commit «{step.Label}» became your turn: there is no way to know which documents you wrote."));

            var prefix = Git(projectPath, "rev-parse", "--show-prefix").Out.Trim();
            var dirty = Git(projectPath, "status", "--porcelain=v1", "--untracked-files=all").Out
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Length > 3 ? l.Substring(3).Trim().Trim('"') : "")
                .Select(p => p.Contains(" -> ") ? p.Substring(p.IndexOf(" -> ", StringComparison.Ordinal) + 4) : p)
                .Where(p => p.StartsWith(prefix, StringComparison.Ordinal)).Select(p => p.Substring(prefix.Length)).ToList();
            var diff = Git(projectPath, "diff", "--name-only", "--relative", askedCommit, "HEAD");
            if (diff.Code != 0)
                throw new InvalidOperationException(Say($"Non riesco a confrontare i documenti con il commit {askedCommit}: {diff.Err.Trim()}",
                    $"I cannot compare the documents with commit {askedCommit}: {diff.Err.Trim()}"));
            var changed = diff.Out.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToList();

            var files = new List<string>();
            foreach (var o in step.Outputs)
            {
                var notCommitted = dirty.Where(p => WorkflowScheduler.Matches(o.Path, p)).ToList();
                if (notCommitted.Count > 0)
                    throw new InvalidOperationException(Say($"«{o.Id}»: {string.Join(", ", notCommitted)} ha modifiche fuori dal commit. Fai il commit e pubblicalo, poi chiudi il passo.",
                        $"«{o.Id}»: {string.Join(", ", notCommitted)} has changes outside the commit. Commit and publish them, then close the step."));
                var mine = changed.Where(p => WorkflowScheduler.Matches(o.Path, p)).ToList();
                if (mine.Count == 0)
                    throw new InvalidOperationException(Say($"«{o.Id}»: da quando il passo ti è toccato non hai scritto {o.Path}. Scrivilo, fai il commit e pubblicalo, poi chiudi il passo.",
                        $"«{o.Id}»: since the step became your turn you have not written {o.Path}. Write it, commit and publish it, then close the step."));
                files.AddRange(mine);
            }
            var last = Git(projectPath, new[] { "log", "-1", "--format=%H", askedCommit + "..HEAD", "--" }.Concat(files).ToArray()).Out.Trim();
            var upstream = Git(projectPath, "rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}");
            if (upstream.Code != 0)
                throw new InvalidOperationException(Say("Il ramo non ha un remoto: pubblicalo, così chi viene dopo legge i tuoi documenti.",
                    "The branch has no remote: publish it, so whoever comes next can read your documents."));
            if (Git(projectPath, "merge-base", "--is-ancestor", last, "@{u}").Code != 0)
                throw new InvalidOperationException(Say($"Il commit dei tuoi documenti ({last}) non è pubblicato su {upstream.Out.Trim()}: pubblicalo, poi chiudi il passo.",
                    $"The commit of your documents ({last}) is not published on {upstream.Out.Trim()}: publish it, then close the step."));
            return (files.Distinct().ToList(), last);
        }

        public bool IsPersonStepClosed(AgentMessage message)
        {
            if (message?.TriggerSource != MdExplorer.Features.Agents.AgentTexts.PersonStepSource || message.WorkflowRound == null) return false;
            var wf = WorkflowFor(message.ProjectPath, message.WorkflowRound);
            var step = wf?.Step(message.WorkflowStep);
            if (step == null) return true;
            return WorkflowScheduler.View(wf, RoundLedger.Load(_store.Root(message.ProjectPath), message.WorkflowRound), step).Status != StepStatus.Asked;
        }

        public void OnOwnerStarted(AgentMessage message)
        {
            if (message?.WorkflowRound == null) return;
            var step = WorkflowFor(message.ProjectPath, message.WorkflowRound)?.Step(message.WorkflowStep);
            if (step == null) return;
            Locked(message.ProjectPath, () => Write(message.ProjectPath, message.WorkflowRound, step, new RoundEvent
            {
                Type = RoundEventType.Started, At = DateTime.UtcNow, By = Me(message.ProjectPath),
                Round = message.WorkflowRoundNo ?? 1, Attempt = message.WorkflowAttempt ?? 1,
                Note = message.OwnerNote, Engine = message.StartProvider, Model = message.StartModel,
            }, $"giro {message.WorkflowRound}: «{step.Label}» avviato dal responsabile"));
        }

        public void OnOwnerDeclined(AgentMessage message, string reason)
        {
            if (message?.WorkflowRound == null) return;
            var step = WorkflowFor(message.ProjectPath, message.WorkflowRound)?.Step(message.WorkflowStep);
            if (step == null) return;
            Locked(message.ProjectPath, () => Write(message.ProjectPath, message.WorkflowRound, step, new RoundEvent
            {
                Type = RoundEventType.Declined, At = DateTime.UtcNow, By = Me(message.ProjectPath),
                Round = message.WorkflowRoundNo ?? 1, Attempt = message.WorkflowAttempt ?? 1, Note = reason,
            }, $"giro {message.WorkflowRound}: «{step.Label}» non avviato"));
        }

        public void Reconcile(string projectPath, bool refresh = true)
        {
            var wf = Workflow(projectPath, out _);
            if (wf == null || !_store.IsAvailable(projectPath)) return;
            if (refresh)
            try { _store.Refresh(projectPath); }
            catch (Exception ex) { _logger.LogWarning(ex, "[Workflow] il registro dei giri non si è aggiornato da origin: si lavora con quello che c'è"); }
            foreach (var id in RoundLedger.RoundIds(_store.Root(projectPath)))
            {
                try
                {
                    var rwf = WorkflowFor(projectPath, id);
                    if (rwf == null) continue;
                    Locked(projectPath, () =>
                    {
                        var round = RoundLedger.Load(_store.Root(projectPath), id);
                        var wf = rwf;   // il workflow con cui è nato il giro
                        var views = WorkflowScheduler.Views(wf, round);
                        var me = Me(projectPath);
                        foreach (var step in wf.Steps)
                        {
                            var view = views[step.Id];
                            if (view.Status == StepStatus.NotStarted) continue;
                            var owner = OwnerOf(projectPath, wf, round, step);
                            if (!Same(owner.Email, me))
                            {
                                Withdraw(projectPath, id, step, owner.Email ?? owner.Problem);
                                continue;
                            }
                            if (view.Status == StepStatus.Asked && !HasPersonMessage(projectPath, id, step.Id))
                                Execute(projectPath, id, WorkflowScheduler.Ask(wf, round, step), startedBy: null, resumed: true);
                            else if (view.Status == StepStatus.Held && !HasPendingAssignment(projectPath, id, step.Id))
                                Execute(projectPath, id, WorkflowScheduler.Resume(wf, round, step), startedBy: null, resumed: true);
                            else if (view.Status == StepStatus.Delivered)
                                AdoptDelivery(projectPath, id, step, view.Last);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Workflow] giro {Round}: la ripresa dei passi in sospeso è fallita", id);
                    continue;
                }
                Evaluate(projectPath, id);
            }
        }

        public void PassTo(AgentMessage held, string toEmail, string note)
        {
            if (held?.WorkflowRound == null) throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say("Questo incarico non è di un giro del workflow: non c'è un passo da passare.", "This assignment is not part of a workflow round: there is no step to hand over."));
            var projectPath = held.ProjectPath;
            var wf = WorkflowFor(projectPath, held.WorkflowRound) ?? throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say("Il progetto non ha un workflow attivo.", "The project has no active workflow."));
            var step = wf.Step(held.WorkflowStep) ?? throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"Il passo '{held.WorkflowStep}' non è nel workflow.", $"Step '{held.WorkflowStep}' is not in the workflow."));
            var to = (toEmail ?? "").Trim().ToLowerInvariant();
            var me = Me(projectPath);
            Locked(projectPath, () =>
            {
                var round = RoundLedger.Load(_store.Root(projectPath), held.WorkflowRound);
                var now = OwnerOf(projectPath, wf, round, step);
                if (!Same(now.Email, me))
                    throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"«{step.Label}» non è tuo ({now.Email ?? now.Problem}): lo passa chi ne risponde.", $"«{step.Label}» is not yours ({now.Email ?? now.Problem}): its owner hands it over."));
                var team = Team(projectPath, step.Agent);
                if (Same(to, me)) throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"«{step.Label}» è già tuo.", $"«{step.Label}» is already yours."));
                if (!team.Any(o => Same(o.Email, to)))
                    throw new InvalidOperationException($"{(to.Length == 0 ? "Scegli a chi" : to + " non risponde dell'agente '" + step.Agent + "'")}: " +
                        $"«{step.Label}» si passa a chi ne risponde con te ({string.Join(", ", team.Where(o => !Same(o.Email, me)).Select(o => o.Email))}).");
                var view = WorkflowScheduler.View(wf, round, step);
                Write(projectPath, held.WorkflowRound, step, new RoundEvent
                {
                    Type = RoundEventType.Assigned, At = DateTime.UtcNow, By = me, Owner = to, Round = view.Round, Attempt = view.Attempt,
                    Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                }, $"giro {held.WorkflowRound}: «{step.Label}» passato a {to}");
                Withdraw(projectPath, held.WorkflowRound, step, to);
            });
            _logger.LogInformation("[Workflow] giro {Round}: «{Step}» passato da {Me} a {To}", held.WorkflowRound, step.Label, me, to);
        }

        public void TakeOver(string projectPath, string roundId, string stepId)
        {
            var wf = WorkflowFor(projectPath, roundId) ?? throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say("Il progetto non ha un workflow attivo.", "The project has no active workflow."));
            var step = wf.Step(stepId) ?? throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"Il passo '{stepId}' non è nel workflow.", $"Step '{stepId}' is not in the workflow."));
            var me = Me(projectPath);
            Locked(projectPath, () =>
            {
                var round = RoundLedger.Load(_store.Root(projectPath), roundId);
                var now = OwnerOf(projectPath, wf, round, step);
                if (now.Email != null)
                    throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say($"«{step.Label}» è di {now.Email}: si prende solo un passo di chi non ne risponde più.", $"«{step.Label}» belongs to {now.Email}: you can only take a step from someone who is no longer its owner."));
                if (!now.CanTake.Any(e => Same(e, me)))
                    throw new InvalidOperationException(now.CanTake.Count == 0
                        ? now.Problem
                        : $"«{step.Label}» lo prende chi risponde dell'agente '{step.Agent}' ({string.Join(", ", now.CanTake)}), e tu ({me}) no.");
                var view = WorkflowScheduler.View(wf, round, step);
                Write(projectPath, roundId, step, new RoundEvent
                {
                    Type = RoundEventType.Assigned, At = DateTime.UtcNow, By = me, Owner = me, Round = view.Round, Attempt = view.Attempt,
                    Note = "preso da chi ne risponde: chi lo aveva non risponde più dell'agente",
                }, $"giro {roundId}: «{step.Label}» preso da {me}");
            });
            _logger.LogInformation("[Workflow] giro {Round}: «{Step}» preso da {Me}", roundId, step.Label, me);
            // Il passo ora è mio: si riprende da qui (incarico da avviare, consegna da approvare) e si valuta il giro.
            Reconcile(projectPath, refresh: false);
            Evaluate(projectPath, roundId);
        }

        public string NotYours(AgentMessage message)
        {
            if (message?.WorkflowRound == null) return null;
            return NotYours(message.ProjectPath, message.WorkflowRound, message.WorkflowStep);
        }

        public string NotYours(AgentMergeRequest request)
        {
            var link = request == null ? null : LinkOfRun(request.ProjectPath, request.RunId);
            return link == null ? null : NotYours(request.ProjectPath, link.RoundId, link.Step);
        }

        private string NotYours(string projectPath, string roundId, string stepId)
        {
            var wf = WorkflowFor(projectPath, roundId);
            var step = wf?.Step(stepId);
            if (step == null) return null;
            var owner = OwnerOf(projectPath, wf, RoundLedger.Load(_store.Root(projectPath), roundId), step);
            if (Same(owner.Email, Me(projectPath))) return null;
            return owner.Email != null
                ? MdExplorer.Features.Services.UiText.Say($"«{step.Label}» è di {owner.Email}: lo decide chi ne risponde, dal suo computer.", $"«{step.Label}» belongs to {owner.Email}: the owner decides, from their own computer.")
                : owner.Problem;
        }

        public IReadOnlyList<RoundProgress> ProgressOfRun(string projectPath, string runId)
        {
            var wf = Workflow(projectPath, out _);
            if (wf == null || string.IsNullOrWhiteSpace(runId) || !_store.IsAvailable(projectPath)) return Array.Empty<RoundProgress>();
            var result = new List<RoundProgress>();
            var wrote = RunsThatWrote(projectPath);
            foreach (var source in wf.Steps)
                foreach (var round in RoundsOfRun(projectPath, runId, source.Id)
                             .Where(r => r.EventsOf(source.Id).Any(e => e.Type == RoundEventType.Replied)))
                {
                    var rwf = WorkflowFor(projectPath, round.Header.Id);
                    if (rwf == null) continue;
                    var views = WorkflowScheduler.Views(rwf, round);
                    foreach (var step in rwf.Steps.Where(s => s.Id != source.Id && s.Trigger?.Kind != WorkflowTriggerKind.Launch))
                        result.Add(Progress(projectPath, rwf, round, step, views[step.Id], wrote));
                }
            return result;
        }

        public IReadOnlyList<RoundSummary> Rounds(string projectPath)
        {
            var wf = Workflow(projectPath, out _);
            if (wf == null || !_store.IsAvailable(projectPath)) return Array.Empty<RoundSummary>();
            var result = new List<RoundSummary>();
            var wrote = RunsThatWrote(projectPath);
            foreach (var id in RoundLedger.RoundIds(_store.Root(projectPath)))
            {
                RoundState round;
                try { round = RoundLedger.Load(_store.Root(projectPath), id); }
                catch (Exception ex) { _logger.LogError(ex, "[Workflow] giro {Round} illeggibile", id); continue; }
                var rwf = WorkflowFor(projectPath, id);
                if (rwf == null) continue;
                var views = WorkflowScheduler.Views(rwf, round);
                var steps = rwf.Steps.Select(step => Progress(projectPath, rwf, round, step, views[step.Id], wrote)).ToList();
                var closed = new[] { "approved", "done", "declined", "failed" };
                var events = round.Steps.Values.SelectMany(r => r.Events).ToList();
                var variables = round.Variables;
                result.Add(new RoundSummary
                {
                    Id = id, Title = rwf.Title, StartedAt = round.Header.StartedAt, StartedBy = round.Header.StartedBy,
                    Values = rwf.Variables.Keys.Where(variables.ContainsKey).Select(k => variables[k]).ToList(),
                    LastActivityAt = events.Count > 0 ? events.Max(e => e.At) : round.Header.StartedAt,
                    Steps = steps,
                    StepsTotal = steps.Count,
                    StepsDone = steps.Count(s => s.State is "approved" or "done"),
                    Finished = steps.All(s => closed.Contains(s.State)),
                    Stopped = !steps.All(s => closed.Contains(s.State)) && WorkflowScheduler.IsStopped(rwf, round, views),
                    Runs = events.Where(e => !string.IsNullOrEmpty(e.Run)).Select(e => e.Run.Replace("-", "")).Distinct().ToList(),
                });
            }
            return result.OrderByDescending(r => r.LastActivityAt).ToList();
        }

        /// <summary>I turni (senza trattini) che hanno scritto almeno un messaggio alla persona, nel progetto.</summary>
        private HashSet<string> RunsThatWrote(string projectPath)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var runs = db.GetDal<AgentMessage>().GetList()
                .Where(m => m.ToAgent == ConversationHopGuard.UserRecipient && m.RunId != null)
                .Select(m => new { m.RunId, m.ProjectPath }).ToList();
            db.Commit();
            return runs.Where(m => AgentPathComparer.Equals(m.ProjectPath, projectPath))
                       .Select(m => m.RunId.Replace("-", "").ToLowerInvariant()).ToHashSet();
        }

        private RoundProgress Progress(string projectPath, WorkflowDescriptor wf, RoundState round, WorkflowStep step, StepView view,
            HashSet<string> wrote)
        {
            var owner = OwnerOf(projectPath, wf, round, step);
            var state = view.Status switch
            {
                StepStatus.NotStarted => "waiting",
                StepStatus.Held => "tostart",
                StepStatus.Asked => "asked",
                StepStatus.Declined => "declined",
                StepStatus.Working or StepStatus.RoundApproved => "working",
                StepStatus.Delivered => "approval",
                StepStatus.Rejected => "rejected",
                StepStatus.Failed => "failed",
                StepStatus.Finished => step.Produces.Count > 0 && !step.IsPerson ? "approved" : "done",
                _ => "working",
            };
            return new RoundProgress
            {
                Id = round.Header.Id + "/" + step.Id, Round = round.Header.Id, Step = step.Id, Label = step.Label,
                Agent = step.Agent, Person = step.Person, Owner = owner.Email, State = state,
                CanTake = owner.CanTake.Any(e => Same(e, Me(projectPath))),
                Note = state is "declined" or "rejected" or "failed" ? view.Last?.Note : null,
                Silent = state is "approval" or "approved" or "done" && Silent(round, step, wrote),
            };
        }

        /// <summary>Il turno che ha consegnato o concluso il passo (l'ultimo) non ha scritto alla persona.</summary>
        private static bool Silent(RoundState round, WorkflowStep step, HashSet<string> wrote)
        {
            var finishing = round.EventsOf(step.Id).LastOrDefault(e => (e.Type == RoundEventType.Delivered || e.Type == RoundEventType.Done)
                                                                       && !string.IsNullOrEmpty(e.Run));
            return finishing != null && !wrote.Contains(finishing.Run.Replace("-", "").ToLowerInvariant());
        }

        /// <summary>Chi risponde del passo, dal registro e dal documento delle responsabilità (W22).</summary>
        private StepOwner OwnerOf(string projectPath, WorkflowDescriptor wf, RoundState round, WorkflowStep step)
            => StepOwners.Of(wf, round, step, agent => Team(projectPath, agent).Select(o => o.Email).ToList(),
                role => AgentOwnerRule.Members(_ownership.GetActiveOwnership(projectPath), role).Select(o => o.Email).ToList());

        /// <summary>Chi può fare un passo: chi risponde del suo agente, o chi ha il suo ruolo.</summary>
        private IReadOnlyList<AgentOwnerPerson> TeamOf(string projectPath, WorkflowStep step)
            => step.IsPerson ? AgentOwnerRule.Members(_ownership.GetActiveOwnership(projectPath), step.Person) : Team(projectPath, step.Agent);

        /// <summary>Chi risponde di un agente: una persona o un team.</summary>
        private IReadOnlyList<AgentOwnerPerson> Team(string projectPath, string agent)
            => AgentOwnerRule.Decide(_ownership.GetActiveOwnership(projectPath), agent, Me(projectPath)).Owners;

        /// <summary>Un incarico «da avviare» di un passo che non è più mio esce dalla posta, con il perché. Sotto il lock.</summary>
        private void Withdraw(string projectPath, string roundId, WorkflowStep step, string who)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<AgentMessage>();
            // Il «tocca a te» di una persona è già consegnato (processed) appena entra in posta: lo si ritira finché non è archiviato.
            var held = dal.GetList().ToList().Where(m => (m.State == AgentMessage.StateEnum.Pending
                                                          || m.TriggerSource == MdExplorer.Features.Agents.AgentTexts.PersonStepSource && m.ArchivedAt == null)
                                                         && m.WorkflowRound == roundId
                                                         && m.WorkflowStep == step.Id && AgentPathComparer.Equals(m.ProjectPath, projectPath)).ToList();
            foreach (var m in held)
            {
                if (m.ToAgent == ConversationHopGuard.UserRecipient)
                {
                    // Il «tocca a te» di un passo di una persona: non è un lavoro fallito, è passato a un altro.
                    m.ArchivedAt = m.ReadAt = DateTime.UtcNow;
                    dal.Save(m);
                    continue;
                }
                m.State = AgentMessage.StateEnum.Failed;
                m.Error = MdExplorer.Features.Services.UiText.Say($"«{step.Label}» ora è di {who}: lo riprende il suo MdExplorer.", $"«{step.Label}» now belongs to {who}: their MdExplorer picks it up.");
                m.ProcessedAt = DateTime.UtcNow;
                m.DeferredReason = null;
                m.NextAttemptAt = null;
                dal.Save(m);
            }
            db.Commit();
            if (held.Count > 0)
                _logger.LogInformation("[Workflow] giro {Round}: «{Step}» passato a {Who}, tolto dalla posta di questo computer", roundId, step.Label, who);
        }

        /// <summary>Questo computer ha già messo in posta il «tocca a te» del passo (anche se la persona l'ha archiviato).</summary>
        private bool HasPersonMessage(string projectPath, string roundId, string stepId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var found = db.GetDal<AgentMessage>().GetList().ToList().Any(m => m.TriggerSource == MdExplorer.Features.Agents.AgentTexts.PersonStepSource
                                                                           && m.WorkflowRound == roundId && m.WorkflowStep == stepId
                                                                           && AgentPathComparer.Equals(m.ProjectPath, projectPath));
            db.Commit();
            return found;
        }

        private void MarkRead(Guid messageId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<AgentMessage>();
            var m = dal.GetList().FirstOrDefault(x => x.Id == messageId);
            if (m != null && m.ReadAt == null) { m.ReadAt = DateTime.UtcNow; dal.Save(m); }
            db.Commit();
        }

        private bool HasPendingAssignment(string projectPath, string roundId, string stepId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var found = db.GetDal<AgentMessage>().GetList().ToList().Any(m => m.State == AgentMessage.StateEnum.Pending && m.WorkflowRound == roundId
                                                                           && m.WorkflowStep == stepId && AgentPathComparer.Equals(m.ProjectPath, projectPath));
            db.Commit();
            return found;
        }

        /// <summary>
        /// Una consegna in attesa di approvazione, nata sul computer di un altro: la richiesta si ricostruisce dal ramo pubblicato
        /// (W16: i rami degli agenti sono pubblicati a ogni consegna). Sotto il lock.
        /// </summary>
        private void AdoptDelivery(string projectPath, string roundId, WorkflowStep step, RoundEvent delivered)
        {
            if (delivered?.Branch == null) return;
            using var scope = _scopeFactory.CreateScope();
            var requests = scope.ServiceProvider.GetRequiredService<IAgentMergeRequestService>();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var known = db.GetDal<AgentMergeRequest>().GetList().ToList().Any(r => string.Equals(r.PublishedBranch, delivered.Branch, StringComparison.OrdinalIgnoreCase)
                                                                                 && AgentPathComparer.Equals(r.ProjectPath, projectPath));
            db.Commit();
            if (known) return;

            var fetch = Git(projectPath, "fetch", "--quiet", "origin", delivered.Branch);
            var head = Git(projectPath, "rev-parse", "--verify", "origin/" + delivered.Branch);
            if (fetch.Code != 0 || head.Code != 0)
            {
                var why = MdExplorer.Features.Services.UiText.Say($"la consegna di «{step.Label}» è sul ramo {delivered.Branch}, che su origin non si trova ({(fetch.Err + head.Err).Trim()})", $"the delivery of «{step.Label}»: its branch {delivered.Branch} is not on origin ({(fetch.Err + head.Err).Trim()})");
                if (_told.TryAdd($"{projectPath}|{roundId}|adopt|{delivered.Branch}", 0))
                {
                    _logger.LogError("[Workflow] giro {Round}: {Why}", roundId, why);
                    TellUser(projectPath, step.Agent, MdExplorer.Features.Agents.AgentTexts.Pick(_language.Current,
                        $"Il giro {roundId} non riesce a riprendere {why}.", $"Round {roundId} cannot resume {why}."));
                }
                return;
            }
            var diff = Git(projectPath, "diff", "--name-status", "HEAD...origin/" + delivered.Branch);
            requests.Open(projectPath, step.Agent, delivered.Branch, "origin/" + delivered.Branch, head.Out.Trim(),
                AgentMergeRequestService.ParseNameStatus(diff.Out), delivered.Run);
            _logger.LogInformation("[Workflow] giro {Round}: la consegna di «{Step}» ({Branch}) ora si approva da questo computer", roundId, step.Label, delivered.Branch);
        }

        // ---- lo schedulatore e le sue azioni ----

        /// <summary>Chiede allo schedulatore che cosa fare per i miei passi del giro, e lo fa.</summary>
        private void Evaluate(string projectPath, string roundId)
        {
            try
            {
                var wf = WorkflowFor(projectPath, roundId);
                if (wf == null) return;
                Locked(projectPath, () =>
                {
                    var round = RoundLedger.Load(_store.Root(projectPath), roundId);
                    var me = Me(projectPath);
                    var owners = wf.Steps.ToDictionary(s => s.Id, s => OwnerOf(projectPath, wf, round, s));
                    var actions = WorkflowScheduler.Decide(wf, round, (WorkflowStep s) => Same(owners[s.Id].Email, me));
                    // Un passo pronto a partire di cui non si sa chi risponde: non si indovina, si dice (una volta).
                    var views = WorkflowScheduler.Views(wf, round);
                    foreach (var s in wf.Steps.Where(s => owners[s.Id].Email == null && s.Trigger?.Kind != WorkflowTriggerKind.Launch
                                                          && views[s.Id].Status == StepStatus.NotStarted
                                                          && WorkflowScheduler.TriggerSatisfied(wf, round, views, s)))
                        if (_told.TryAdd($"{projectPath}|{roundId}|{s.Id}|owner|{owners[s.Id].Problem}", 0))
                        {
                            _logger.LogWarning("[Workflow] giro {Round}: {Problem}", roundId, owners[s.Id].Problem);
                            TellUser(projectPath, s.Agent ?? MdExplorer.Features.Agents.AgentTexts.PersonStepSender, MdExplorer.Features.Agents.AgentTexts.Pick(_language.Current,
                                $"Il giro {roundId} è fermo: {owners[s.Id].Problem}", $"Round {roundId} is stopped: {owners[s.Id].Problem}"));
                        }
                    foreach (var action in actions)
                        Execute(projectPath, roundId, action, startedBy: null, resumed: false);
                    return 0;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workflow] giro {Round}: la valutazione dello schedulatore è fallita", roundId);
            }
        }

        /// <summary>Fa un'azione: l'incarico in coda (subito o «da avviare») e il suo evento nel registro. Va chiamato sotto il lock.</summary>
        private void Execute(string projectPath, string roundId, SchedulerAction action, string startedBy, bool resumed)
        {
            var step = action.Step;
            if (action.Kind == SchedulerActionKind.Blocked)
            {
                if (_told.TryAdd($"{projectPath}|{roundId}|{step.Id}|{action.Round}|{action.Attempt}|{action.Reason}", 0))
                {
                    _logger.LogWarning("[Workflow] giro {Round}: {Reason}", roundId, action.Reason);
                    TellUser(projectPath, step.Agent ?? MdExplorer.Features.Agents.AgentTexts.PersonStepSender, MdExplorer.Features.Agents.AgentTexts.Pick(_language.Current,
                        $"Il giro {roundId} è fermo: {action.Reason}", $"Round {roundId} is stopped: {action.Reason}"));
                }
                return;
            }
            if (action.Kind == SchedulerActionKind.Ask)
            {
                AskPerson(projectPath, roundId, action, resumed);
                return;
            }

            var hold = action.Kind == SchedulerActionKind.Hold;
            var result = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = projectPath,
                FromAgent = ConversationHopGuard.UserRecipient,
                ToAgent = step.Agent,
                Body = MdExplorer.Features.Agents.AgentTexts.Assignment(_language.Current, action, roundId),
                TriggerSource = "workflow",
                AwaitOwner = hold,
                ReworkNote = action.ReworkNote,
                WorkflowRound = roundId,
                WorkflowStep = step.Id,
                WorkflowRoundNo = action.Round,
                WorkflowAttempt = action.Attempt,
            });
            if (!result.Accepted)
            {
                // Senza l'incarico in coda, il passo non partirebbe mai e nessuno saprebbe perché.
                _logger.LogError("[Workflow] giro {Round}: l'incarico per '{Agent}' (passo '{Step}') non è entrato in coda: {Why}",
                    roundId, step.Agent, step.Id, result.RejectionReason);
                TellUser(projectPath, step.Agent, MdExplorer.Features.Agents.AgentTexts.Pick(_language.Current,
                    $"Il giro {roundId} non è riuscito a far partire «{step.Label}»: {result.RejectionReason}",
                    $"Round {roundId} could not start «{step.Label}»: {result.RejectionReason}"));
                return;
            }

            Write(projectPath, roundId, step, new RoundEvent
            {
                Type = hold ? RoundEventType.Held : RoundEventType.Started, At = DateTime.UtcNow,
                By = startedBy ?? Me(projectPath), Round = action.Round, Attempt = action.Attempt, Note = action.ReworkNote,
            }, $"giro {roundId}: «{step.Label}» {(hold ? "da avviare" : action.Attempt > 1 ? "rifatto" : "partito")}{(resumed ? $" (ripreso da {Me(projectPath)})" : "")}");
            _logger.LogInformation("[Workflow] giro {Round}: «{Step}» {What} per '{Agent}' (giro {N}, tentativo {A})",
                roundId, step.Label, hold ? "da avviare" : "partito", step.Agent, action.Round, action.Attempt);
            if (hold)
                // Chi ne risponde lo deve sapere adesso: badge e avviso, come per ogni «da avviare».
                _ = _hub.Clients.All.SendAsync("agentStartRequested", new
                {
                    messageId = result.MessageId.ToString(), fromAgent = ConversationHopGuard.UserRecipient, toAgent = step.Agent,
                    step = step.Label, projectPath,
                });
        }

        // ---- registro e git ----

        /// <summary>
        /// «Tocca a te» nella posta della persona: il compito, che cosa riceve, che cosa scrive, e gli esiti come pulsanti.
        /// La prima volta il registro scrive «asked» con il commit del progetto (da lì si contano i suoi documenti); ripreso su
        /// un altro computer, il messaggio torna in posta e il registro resta com'è. Va chiamato sotto il lock.
        /// </summary>
        private void AskPerson(string projectPath, string roundId, SchedulerAction action, bool resumed)
        {
            var step = action.Step;
            var replies = step.Outcomes.Select(o => new ResolvedReply
            {
                Id = o.Id, Label = o.Label, Message = o.Label,
                Description = o.Values.Count == 0 ? "" : MdExplorer.Features.Agents.AgentTexts.Pick(_language.Current,
                    "Porta " + string.Join(", ", o.Values.Select(v => "{" + v + "}")) + ".", "Carries " + string.Join(", ", o.Values.Select(v => "{" + v + "}")) + "."),
                Values = o.Values.ToDictionary(v => v, _ => "", StringComparer.Ordinal),
            }).ToList();
            string head = null;
            if (!resumed)
            {
                var h = Git(projectPath, "rev-parse", "HEAD");
                if (h.Code != 0)
                {
                    _logger.LogError("[Workflow] giro {Round}: «{Step}» non può toccare alla persona, il progetto non ha un commit: {Err}", roundId, step.Label, h.Err);
                    return;
                }
                head = h.Out.Trim();
            }
            var result = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = projectPath,
                FromAgent = MdExplorer.Features.Agents.AgentTexts.PersonStepSender,
                ToAgent = ConversationHopGuard.UserRecipient,
                Body = MdExplorer.Features.Agents.AgentTexts.PersonTurn(_language.Current, action, roundId),
                Replies = System.Text.Json.JsonSerializer.Serialize(replies),
                TriggerSource = MdExplorer.Features.Agents.AgentTexts.PersonStepSource,
                WorkflowRound = roundId,
                WorkflowStep = step.Id,
                WorkflowRoundNo = 1,
                WorkflowAttempt = 1,
            });
            if (!result.Accepted)
            {
                _logger.LogError("[Workflow] giro {Round}: «tocca a te» di «{Step}» non entrato in posta: {Why}", roundId, step.Label, result.RejectionReason);
                return;
            }
            if (!resumed)
                Write(projectPath, roundId, step, new RoundEvent
                {
                    Type = RoundEventType.Asked, At = DateTime.UtcNow, By = Me(projectPath), Commit = head,
                }, $"giro {roundId}: «{step.Label}» tocca a {Me(projectPath)}");
            _logger.LogInformation("[Workflow] giro {Round}: «{Step}» tocca a {Me}{Resumed}", roundId, step.Label, Me(projectPath), resumed ? " (ripreso)" : "");
        }

        /// <summary>Scrive un evento nel file del passo e lo committa (solo quei file). Va chiamato sotto il lock.</summary>
        private void Write(string projectPath, string roundId, WorkflowStep step, RoundEvent evt, string commitMessage)
        {
            RoundLedger.Append(_store.Root(projectPath), roundId, step.Id, step.Agent, evt);
            CommitAndPublish(projectPath, roundId, commitMessage);
        }

        /// <summary>
        /// Committa la cartella del giro sul ramo del registro e la pubblica (W16): è così che gli altri computer lo vengono a
        /// sapere. Il ramo è suo, non quello della persona. Se la pubblicazione non riesce si dice nel log: il commit resta.
        /// </summary>
        private void CommitAndPublish(string projectPath, string roundId, string message)
        {
            var problem = _store.Publish(projectPath, roundId, message);
            if (problem != null)
                _logger.LogWarning("[Workflow] giro {Round}: {Problem} (partirà con la prossima pubblicazione).", roundId, problem);
            else
                _bell.Ring(projectPath, roundId);   // gli altri computer guardano adesso, non al prossimo controllo
        }

        private static (int Code, string Out, string Err) Git(string cwd, params string[] args) => GitCli.Run(cwd, args);

        // ---- legami ----

        private sealed class Link
        {
            public string RoundId;
            public string Step;
            public int Round = 1;
            public int Attempt = 1;
        }

        /// <summary>Il passo di un giro che un turno (RunId) ha lavorato: dal suo evento nel registro.</summary>
        private Link LinkOfRun(string projectPath, string runId)
        {
            if (string.IsNullOrWhiteSpace(runId) || Workflow(projectPath, out _) == null || !_store.IsAvailable(projectPath)) return null;
            var key = runId.Replace("-", "");
            foreach (var id in RoundLedger.RoundIds(_store.Root(projectPath)).Reverse())
            {
                RoundState round;
                try { round = RoundLedger.Load(_store.Root(projectPath), id); }
                catch (Exception ex) { _logger.LogError(ex, "[Workflow] giro {Round} illeggibile", id); continue; }
                foreach (var record in round.Steps.Values)
                {
                    var e = record.Events.LastOrDefault(x => x.Run != null && string.Equals(x.Run.Replace("-", ""), key, StringComparison.OrdinalIgnoreCase));
                    if (e != null) return new Link { RoundId = id, Step = record.Step, Round = e.Round, Attempt = e.Attempt };
                }
            }
            return null;
        }

        /// <summary>I giri in cui un turno ha lavorato un passo, dal più vecchio: il primo è quello che il turno ha aperto.</summary>
        private List<RoundState> RoundsOfRun(string projectPath, string runId, string stepId)
        {
            var key = (runId ?? "").Replace("-", "");
            var found = new List<RoundState>();
            foreach (var id in RoundLedger.RoundIds(_store.Root(projectPath)))
            {
                RoundState round;
                try { round = RoundLedger.Load(_store.Root(projectPath), id); }
                catch (Exception ex) { _logger.LogError(ex, "[Workflow] giro {Round} illeggibile", id); continue; }
                if (round.EventsOf(stepId).Any(e => e.Run != null && string.Equals(e.Run.Replace("-", ""), key, StringComparison.OrdinalIgnoreCase)))
                    found.Add(round);
            }
            return found.OrderBy(r => r.Header.StartedAt).ToList();
        }

        /// <summary>
        /// Il passo sotto il cui messaggio stanno i pulsanti: il «tocca a te» di una persona lo porta scritto; il messaggio di un
        /// agente lo ricava dal turno che l'ha scritto.
        /// </summary>
        private Link SourceOf(string projectPath, AgentMessage message)
            => message.TriggerSource == MdExplorer.Features.Agents.AgentTexts.PersonStepSource && message.WorkflowRound != null
                ? new Link { RoundId = message.WorkflowRound, Step = message.WorkflowStep }
                : LinkOfRun(projectPath, message.RunId);

        /// <summary>Il passo di un giro che un incarico dello schedulatore fa partire: scritto sul messaggio.</summary>
        private Link LinkOfMessage(Guid messageId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var m = db.GetDal<AgentMessage>().GetList().FirstOrDefault(x => x.Id == messageId);
            db.Commit();
            return m?.WorkflowRound == null ? null
                : new Link { RoundId = m.WorkflowRound, Step = m.WorkflowStep, Round = m.WorkflowRoundNo ?? 1, Attempt = m.WorkflowAttempt ?? 1 };
        }

        // ---- di contorno ----

        /// <summary>
        /// Il workflow di un giro (16/10/2026): quello con cui è nato, salvato nel registro con le schede a cui era unito
        /// (17/10/2026), così un workflow o una scheda cambiati a giro aperto non fermano né cambiano i passi rimasti. Un
        /// giro aperto prima che si salvasse usa quello attuale, come prima. Con il workflow spento (nessun documento) non
        /// lavora niente.
        /// </summary>
        private WorkflowDescriptor WorkflowFor(string projectPath, string roundId)
        {
            if (string.IsNullOrWhiteSpace(_metadata.GetAgentCity(projectPath)?.WorkflowDoc)) return null;
            if (string.IsNullOrWhiteSpace(roundId) || !_store.IsAvailable(projectPath)) return Workflow(projectPath, out _);
            string json;
            try { json = RoundLedger.LoadWorkflowSnapshot(_store.Root(projectPath), roundId); }
            catch (ArgumentException) { return Workflow(projectPath, out _); }   // id non di un giro: chi chiama lo dirà
            if (json == null) return Workflow(projectPath, out _);
            var parsed = WorkflowFile.ReadSnapshot(json, RoundLedger.LoadCardsSnapshot(_store.Root(projectPath), roundId));
            if (parsed.IsValid) return parsed.Descriptor;
            var first = parsed.Issues.First(i => i.Severity == WorkflowSeverity.Error);
            if (_told.TryAdd($"{projectPath}|{roundId}|snapshot", 0))
                _logger.LogError("[Workflow] giro {Round}: il workflow con cui è nato non si legge ({Problem}): il giro è fermo, va chiuso.", roundId, first);
            return null;
        }

        /// <summary>Salva nel giro appena aperto il workflow com'è adesso e le schede a cui è unito: il giro andrà avanti con quelli.</summary>
        private void SnapshotWorkflow(string projectPath, string roundId, string jsonPath, WorkflowDescriptor wf)
        {
            var full = Path.Combine(projectPath, jsonPath.Replace('/', Path.DirectorySeparatorChar));
            RoundLedger.SaveWorkflowSnapshot(_store.Root(projectPath), roundId, File.ReadAllText(full));
            RoundLedger.SaveCardsSnapshot(_store.Root(projectPath), roundId, wf.Cards.ToJson());
        }

        private WorkflowDescriptor Workflow(string projectPath, out string jsonPath)
        {
            jsonPath = null;
            var doc = _metadata.GetAgentCity(projectPath)?.WorkflowDoc;
            if (string.IsNullOrWhiteSpace(doc)) return null;
            var wf = WorkflowDocument.LoadActive(projectPath, doc, _metadata.GetAgentCity(projectPath)?.OwnershipDoc, out var problem);
            if (problem != null)
            {
                if (_told.TryAdd(projectPath + "|workflow|" + problem, 0))
                    _logger.LogError("[Workflow] il workflow di {Project} non si legge: {Problem}. Lo schedulatore è fermo.", projectPath, problem);
                return null;
            }
            jsonPath = WorkflowDocument.JsonPathOf(projectPath, doc);
            return wf;
        }

        public int CheckStalledSteps(string projectPath, DateTime now)
        {
            var wf = Workflow(projectPath, out _);
            if (wf == null || !_store.IsAvailable(projectPath)) return 0;
            var me = Me(projectPath);
            if (string.IsNullOrWhiteSpace(me)) return 0;
            var told = 0;
            foreach (var id in RoundLedger.RoundIds(_store.Root(projectPath)))
            {
                try
                {
                    var rwf = WorkflowFor(projectPath, id);
                    if (rwf == null) continue;
                    var round = RoundLedger.Load(_store.Root(projectPath), id);
                    var owners = rwf.Steps.ToDictionary(s => s.Id, s => OwnerOf(projectPath, rwf, round, s).Email);
                    foreach (var stalled in StalledSteps.Find(rwf, round, s => owners[s.Id], me, now))
                    {
                        var label = stalled.Step.Label;
                        var who = stalled.OwnerEmail;
                        var since = stalled.Since;
                        var key = $"round:{id}:{stalled.Step.Id}:{stalled.Round}:{stalled.Attempt}";
                        if (_overdue.NotifyOnce(projectPath, key, now, () => TellUser(projectPath, stalled.Step.Agent,
                                MdExplorer.Features.Agents.AgentTexts.Pick(_language.Current,
                                    $"[ATTENZIONE] Giro {id}: «{label}» tocca a {who} e non dà notizie dal {since:dd/MM/yyyy HH:mm} (UTC), da più di 24 ore. " +
                                    $"MdExplorer non sa perché: computer spento, incarico mai avviato o agente fermo. Verifica con {who}. " +
                                    "Quando hai chiarito, archivia questo messaggio.",
                                    $"[ATTENTION] Round {id}: «{label}» is up to {who} and has given no news since {since:yyyy-MM-dd HH:mm} (UTC), more than 24 hours. " +
                                    $"MdExplorer does not know why: computer off, assignment never started or agent stuck. Check with {who}. " +
                                    "When it is clear, archive this message."))))
                        {
                            told++;
                            _logger.LogWarning("[Workflow] giro {Round}: «{Step}» di {Who} fermo dal {Since:u}: avvisata la persona.", id, label, who, since);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Workflow] giro {Round}: il controllo dei passi fermi è fallito", id);
                }
            }
            return told;
        }

        private string Me(string projectPath)
        {
            try { return _identity.ResolveEmail(projectPath); }
            catch { return null; }
        }

        private void TellUser(string projectPath, string agent, string body)
        {
            try
            {
                _mailbox.Enqueue(new EnqueueRequest
                {
                    ProjectPath = projectPath, FromAgent = agent, ToAgent = ConversationHopGuard.UserRecipient, Body = body,
                });
            }
            catch (Exception ex) { _logger.LogError(ex, "[Workflow] avviso alla persona non partito"); }
        }

        private List<string> DeliveredFiles(AgentMergeRequest request)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetService<IAgentMergeRequestService>();
                return svc?.FilesOf(request).Where(f => f.Change != "deleted").Select(f => f.Path).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Workflow] file consegnati non letti per {Branch}", request.PublishedBranch);
                return new List<string>();
            }
        }

        private T Locked<T>(string projectPath, Func<T> work)
        {
            var gate = _locks.GetOrAdd(projectPath, _ => new SemaphoreSlim(1, 1));
            gate.Wait();
            try { return work(); }
            finally { gate.Release(); }
        }

        private void Locked(string projectPath, Action work) => Locked(projectPath, () => { work(); return 0; });

        private static string Verb(string type) => type switch
        {
            RoundEventType.Delivered => "consegnato",
            RoundEventType.Approved => "approvato",
            RoundEventType.Rejected => "rifiutato",
            RoundEventType.Done => "concluso",
            RoundEventType.Failed => "fallito",
            _ => type,
        };

        private static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private static bool SameValues(Dictionary<string, string> a, Dictionary<string, string> b)
            => (a?.Count ?? 0) == (b?.Count ?? 0) && (a ?? new()).All(kv => b != null && b.TryGetValue(kv.Key, out var v) && v == kv.Value);
    }
}
