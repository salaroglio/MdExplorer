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

        /// <summary>La persona lancia un agente a mano: se il workflow ha un passo di lancio per lui, si apre un giro.</summary>
        string OnLaunched(string projectPath, string agentName, string runId);

        /// <summary>Un turno è finito: consegna, conclusione senza artefatto, o fallimento.</summary>
        void OnRunEnded(string projectPath, string agentName, string runId, Guid? triggerMessageId, AgentMergeRequest delivered, bool succeeded, string error);

        /// <summary>La persona ha approvato o rifiutato un artefatto.</summary>
        void OnDecided(AgentMergeRequest request);

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
        bool TryReply(string projectPath, AgentMessage answered, ResolvedReply chosen, IReadOnlyDictionary<string, string> assign = null);

        /// <summary>I passi che un pulsante farebbe partire, con chi ne può rispondere: per far scegliere chi preme (W22).</summary>
        IReadOnlyList<ReplyTarget> ReplyTargets(string projectPath, AgentMessage answered, string replyId);

        /// <summary>
        /// Chi ha un passo «da avviare» lo passa a un collega che risponde dello stesso agente (W22: per esempio perché va in
        /// ferie). Throws <see cref="InvalidOperationException"/>, con cosa fare, quando non si può.
        /// </summary>
        void PassTo(AgentMessage held, string toEmail, string note);

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
    }

    /// <summary>Un passo di un giro visto da chi lo aspetta.</summary>
    public sealed class RoundProgress
    {
        public string Id { get; init; }
        public string Round { get; init; }
        public string Step { get; init; }
        public string Label { get; init; }
        public string Agent { get; init; }
        /// <summary>Chi ne risponde, se si sa.</summary>
        public string Owner { get; init; }
        /// <summary>waiting · tostart · declined · working · approval · approved · rejected · done · failed</summary>
        public string State { get; init; }
        public string Note { get; init; }
    }

    /// <summary>Un passo che un pulsante fa partire, e chi ne può rispondere.</summary>
    public sealed class ReplyTarget
    {
        public string Step { get; init; }
        public string Label { get; init; }
        public string Agent { get; init; }
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
            ILogger<AgentWorkflowExecutor> logger)
        {
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
                TellUser(projectPath, agentName, $"Il workflow non può aprire il giro di «{launch.Label}»: il progetto non è un repository git, e il registro dei giri vive in git.");
                return null;
            }

            return Locked(projectPath, () =>
            {
                var now = DateTime.UtcNow;
                var id = RoundLedger.NewRoundId(jsonPath, now);
                RoundLedger.Open(_store.Root(projectPath), new RoundHeader { Id = id, Workflow = jsonPath, StartedAt = now, StartedBy = Me(projectPath) });
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
            var wf = Workflow(projectPath, out _);
            var step = wf?.Step(link.Step);
            if (step == null) return;

            RoundEvent evt;
            if (delivered != null)
                evt = new RoundEvent
                {
                    Type = RoundEventType.Delivered, Branch = delivered.PublishedBranch, Run = runId,
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

        public void OnDecided(AgentMergeRequest request)
        {
            if (request == null) return;
            var type = request.Status == AgentMergeRequest.StatusEnum.Merged ? RoundEventType.Approved
                     : request.Status == AgentMergeRequest.StatusEnum.Rejected ? RoundEventType.Rejected
                     : null;
            if (type == null) return;
            var link = LinkOfRun(request.ProjectPath, request.RunId);
            if (link == null) return;
            var step = Workflow(request.ProjectPath, out _)?.Step(link.Step);
            if (step == null) return;

            Locked(request.ProjectPath, () => Write(request.ProjectPath, link.RoundId, step, new RoundEvent
            {
                Type = type, At = DateTime.UtcNow, By = Me(request.ProjectPath), Round = link.Round, Attempt = link.Attempt,
                Run = request.RunId, Note = type == RoundEventType.Rejected ? request.Note : null,
            }, $"giro {link.RoundId}: «{step.Label}» {Verb(type)}"));
            Evaluate(request.ProjectPath, link.RoundId);
        }

        public bool TryRework(AgentMergeRequest request)
        {
            var link = request == null ? null : LinkOfRun(request.ProjectPath, request.RunId);
            if (link == null) return false;
            var wf = Workflow(request.ProjectPath, out _);
            var step = wf?.Step(link.Step);
            if (step == null) return false;

            var round = RoundLedger.Load(_store.Root(request.ProjectPath), link.RoundId);
            var view = WorkflowScheduler.View(wf, round, step);
            if (view.Status != StepStatus.Rejected)
                throw new InvalidOperationException($"«{step.Label}» non è fermo per un rifiuto ({view.Status}): non c'è niente da far ripartire.");
            // Il limite c'è solo se l'autore l'ha scritto (W9).
            var until = wf.LoopOf(step.Id, WorkflowLoopKind.UntilApproved);
            if (until?.Max != null && view.RejectionsInRound > until.Max.Value)
                throw new InvalidOperationException($"«{step.Label}» è già stato rifatto {until.Max} {(until.Max == 1 ? "volta" : "volte")}: è il massimo che il workflow dà al ciclo «{until.Id}». Il lavoro resta fermo.");

            var action = new SchedulerAction
            {
                Kind = SchedulerActionKind.Start, Step = step, Round = view.Round, Attempt = view.Attempt + 1,
                Brief = ResolveBrief(step, round.Variables), ReworkNote = request.Note,
                Inputs = Array.Empty<string>(),
            };
            Locked(request.ProjectPath, () => Execute(request.ProjectPath, link.RoundId, action, startedBy: Me(request.ProjectPath), resumed: false));
            return true;
        }

        public int? ReworksLeft(AgentMergeRequest request, out bool linked, out string why)
        {
            why = null;
            var link = request == null ? null : LinkOfRun(request.ProjectPath, request.RunId);
            linked = link != null;
            if (link == null) return null;
            var wf = Workflow(request.ProjectPath, out _);
            var step = wf?.Step(link.Step);
            var until = step == null ? null : wf.LoopOf(step.Id, WorkflowLoopKind.UntilApproved);
            if (until?.Max == null) return null;
            var view = WorkflowScheduler.View(wf, RoundLedger.Load(_store.Root(request.ProjectPath), link.RoundId), step);
            var left = Math.Max(0, until.Max.Value - (view.RejectionsInRound - 1));
            if (left == 0)
                why = $"«{step.Label}» è già stato rifatto {until.Max} {(until.Max == 1 ? "volta" : "volte")}: è il massimo che il workflow dà al ciclo «{until.Id}». Il lavoro resta fermo.";
            return left;
        }

        public IReadOnlyList<ReplyTarget> ReplyTargets(string projectPath, AgentMessage answered, string replyId)
        {
            var wf = Workflow(projectPath, out _);
            var link = wf == null || answered == null ? null : LinkOfRun(projectPath, answered.RunId);
            if (link == null) return Array.Empty<ReplyTarget>();
            return wf.Steps.Where(s => s.Trigger?.Kind == WorkflowTriggerKind.Reply && s.Trigger.FromStep == link.Step
                                       && string.Equals(s.Trigger.ReplyId, replyId, StringComparison.OrdinalIgnoreCase))
                .Select(s => new ReplyTarget { Step = s.Id, Label = s.Label, Agent = s.Agent, Owners = Team(projectPath, s.Agent) })
                .ToList();
        }

        public bool TryReply(string projectPath, AgentMessage answered, ResolvedReply chosen, IReadOnlyDictionary<string, string> assign = null)
        {
            if (answered == null || chosen == null) return false;
            var wf = Workflow(projectPath, out var jsonPath);
            if (wf == null) return false;
            var link = LinkOfRun(projectPath, answered.RunId);
            if (link == null) return false;
            // Il workflow fa partire qualcosa con questo pulsante sotto questo passo? Altrimenti risponde l'agente, come sempre.
            if (!wf.Steps.Any(s => s.Trigger?.Kind == WorkflowTriggerKind.Reply && s.Trigger.FromStep == link.Step
                                   && string.Equals(s.Trigger.ReplyId, chosen.Id, StringComparison.OrdinalIgnoreCase)))
                return false;
            if (chosen.Values == null)
                throw new InvalidOperationException("Questo pulsante è di prima dello schedulatore e non porta i suoi valori: rilancia la ricerca per avere i pulsanti nuovi.");

            // Chi preme sceglie a chi va ogni passo di un agente con un team (W22); con un solo responsabile non c'è scelta.
            var assignments = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var t in ReplyTargets(projectPath, answered, chosen.Id))
            {
                string to = null;
                assign?.TryGetValue(t.Step, out to);
                if (string.IsNullOrWhiteSpace(to))
                {
                    if (t.NeedsChoice)
                        throw new InvalidOperationException($"«{t.Label}»: l'agente '{t.Agent}' ha più responsabili " +
                            $"({string.Join(", ", t.Owners.Select(o => o.Email))}): scegli a chi va.");
                    continue;
                }
                if (!t.Owners.Any(o => string.Equals(o.Email, to.Trim(), StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException($"«{t.Label}»: {to} non risponde dell'agente '{t.Agent}' " +
                        $"(ne rispondono {(t.Owners.Count == 0 ? "nessuno" : string.Join(", ", t.Owners.Select(o => o.Email)))}).");
                assignments[t.Step] = to.Trim().ToLowerInvariant();
            }

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
                foreach (var e in origin.EventsOf(source.Id).Where(e => e.Type != RoundEventType.Replied))
                    RoundLedger.Append(_store.Root(projectPath), id, source.Id, source.Agent, e);
                Write(projectPath, id, source, replied, $"giro {id}: «{chosen.Label}» (dalla ricerca del giro {origin.Header.Id})");
                _logger.LogInformation("[Workflow] giro {Round} aperto da «{Reply}» sul giro {From}", id, chosen.Label, origin.Header.Id);
                return id;
            });
            if (target != null) Evaluate(projectPath, target);
            return true;
        }

        public void OnOwnerStarted(AgentMessage message)
        {
            if (message?.WorkflowRound == null) return;
            var step = Workflow(message.ProjectPath, out _)?.Step(message.WorkflowStep);
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
            var step = Workflow(message.ProjectPath, out _)?.Step(message.WorkflowStep);
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
                    Locked(projectPath, () =>
                    {
                        var round = RoundLedger.Load(_store.Root(projectPath), id);
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
                            if (view.Status == StepStatus.Held && !HasPendingAssignment(projectPath, id, step.Id))
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
            if (held?.WorkflowRound == null) throw new InvalidOperationException("Questo incarico non è di un giro del workflow: non c'è un passo da passare.");
            var projectPath = held.ProjectPath;
            var wf = Workflow(projectPath, out _) ?? throw new InvalidOperationException("Il progetto non ha un workflow attivo.");
            var step = wf.Step(held.WorkflowStep) ?? throw new InvalidOperationException($"Il passo '{held.WorkflowStep}' non è nel workflow.");
            var to = (toEmail ?? "").Trim().ToLowerInvariant();
            var me = Me(projectPath);
            Locked(projectPath, () =>
            {
                var round = RoundLedger.Load(_store.Root(projectPath), held.WorkflowRound);
                var now = OwnerOf(projectPath, wf, round, step);
                if (!Same(now.Email, me))
                    throw new InvalidOperationException($"«{step.Label}» non è tuo ({now.Email ?? now.Problem}): lo passa chi ne risponde.");
                var team = Team(projectPath, step.Agent);
                if (Same(to, me)) throw new InvalidOperationException($"«{step.Label}» è già tuo.");
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
            var wf = Workflow(projectPath, out _);
            var step = wf?.Step(stepId);
            if (step == null) return null;
            var owner = OwnerOf(projectPath, wf, RoundLedger.Load(_store.Root(projectPath), roundId), step);
            if (Same(owner.Email, Me(projectPath))) return null;
            return owner.Email != null
                ? $"«{step.Label}» è di {owner.Email}: lo decide chi ne risponde, dal suo computer."
                : owner.Problem;
        }

        public IReadOnlyList<RoundProgress> ProgressOfRun(string projectPath, string runId)
        {
            var wf = Workflow(projectPath, out _);
            if (wf == null || string.IsNullOrWhiteSpace(runId) || !_store.IsAvailable(projectPath)) return Array.Empty<RoundProgress>();
            var result = new List<RoundProgress>();
            foreach (var source in wf.Steps)
                foreach (var round in RoundsOfRun(projectPath, runId, source.Id)
                             .Where(r => r.EventsOf(source.Id).Any(e => e.Type == RoundEventType.Replied)))
                {
                    var views = WorkflowScheduler.Views(wf, round);
                    foreach (var step in wf.Steps.Where(s => s.Id != source.Id && s.Trigger?.Kind != WorkflowTriggerKind.Launch))
                    {
                        var view = views[step.Id];
                        var owner = OwnerOf(projectPath, wf, round, step);
                        var state = view.Status switch
                        {
                            StepStatus.NotStarted => "waiting",
                            StepStatus.Held => "tostart",
                            StepStatus.Declined => "declined",
                            StepStatus.Working or StepStatus.RoundApproved => "working",
                            StepStatus.Delivered => "approval",
                            StepStatus.Rejected => "rejected",
                            StepStatus.Failed => "failed",
                            StepStatus.Finished => step.Produces.Count > 0 ? "approved" : "done",
                            _ => "working",
                        };
                        result.Add(new RoundProgress
                        {
                            Id = round.Header.Id + "/" + step.Id, Round = round.Header.Id, Step = step.Id, Label = step.Label,
                            Agent = step.Agent, Owner = owner.Email, State = state,
                            Note = state is "declined" or "rejected" or "failed" ? view.Last?.Note : null,
                        });
                    }
                }
            return result;
        }

        /// <summary>Chi risponde del passo, dal registro e dal documento delle responsabilità (W22).</summary>
        private StepOwner OwnerOf(string projectPath, WorkflowDescriptor wf, RoundState round, WorkflowStep step)
            => StepOwners.Of(wf, round, step, agent => Team(projectPath, agent).Select(o => o.Email).ToList());

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
            var held = dal.GetList().ToList().Where(m => m.State == AgentMessage.StateEnum.Pending && m.WorkflowRound == roundId
                                                         && m.WorkflowStep == step.Id && AgentPathComparer.Equals(m.ProjectPath, projectPath)).ToList();
            foreach (var m in held)
            {
                m.State = AgentMessage.StateEnum.Failed;
                m.Error = $"«{step.Label}» ora è di {who}: lo riprende il suo MdExplorer.";
                m.ProcessedAt = DateTime.UtcNow;
                m.DeferredReason = null;
                m.NextAttemptAt = null;
                dal.Save(m);
            }
            db.Commit();
            if (held.Count > 0)
                _logger.LogInformation("[Workflow] giro {Round}: «{Step}» passato a {Who}, tolto dalla posta di questo computer", roundId, step.Label, who);
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
                var why = $"la consegna di «{step.Label}» è sul ramo {delivered.Branch}, che su origin non si trova ({(fetch.Err + head.Err).Trim()})";
                if (_told.TryAdd($"{projectPath}|{roundId}|adopt|{delivered.Branch}", 0))
                {
                    _logger.LogError("[Workflow] giro {Round}: {Why}", roundId, why);
                    TellUser(projectPath, step.Agent, $"Il giro {roundId} non riesce a riprendere {why}.");
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
                var wf = Workflow(projectPath, out _);
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
                            TellUser(projectPath, s.Agent, $"Il giro {roundId} è fermo: {owners[s.Id].Problem}");
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
                    TellUser(projectPath, step.Agent, $"Il giro {roundId} è fermo: {action.Reason}");
                }
                return;
            }

            var hold = action.Kind == SchedulerActionKind.Hold;
            var result = _mailbox.Enqueue(new EnqueueRequest
            {
                ProjectPath = projectPath,
                FromAgent = ConversationHopGuard.UserRecipient,
                ToAgent = step.Agent,
                Body = Assignment(roundId, action),
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
                TellUser(projectPath, step.Agent, $"Il giro {roundId} non è riuscito a far partire «{step.Label}»: {result.RejectionReason}");
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

        /// <summary>Il testo che l'agente riceve: l'incarico del workflow, dove si trova, i file da cui parte.</summary>
        private static string Assignment(string roundId, SchedulerAction action)
        {
            var sb = new StringBuilder();
            sb.Append("[INCARICO] ").Append(action.Brief?.Trim());
            sb.Append("\n\nGiro ").Append(roundId).Append(", passo «").Append(action.Step.Label).Append('»');
            if (action.Round > 1) sb.Append(", giro ").Append(action.Round);
            if (action.Attempt > 1) sb.Append(", tentativo ").Append(action.Attempt);
            sb.Append('.');
            if (action.Inputs.Count > 0)
            {
                sb.Append("\nFile da cui parti (approvati):");
                foreach (var f in action.Inputs) sb.Append("\n- ").Append(f);
            }
            return sb.ToString();
        }

        private static string ResolveBrief(WorkflowStep step, IReadOnlyDictionary<string, string> variables)
            => step.Brief == null ? null
               : System.Text.RegularExpressions.Regex.Replace(step.Brief, @"\{([^{}\s]+)\}",
                   m => variables.TryGetValue(m.Groups[1].Value, out var v) ? v : m.Value);

        // ---- registro e git ----

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

        private WorkflowDescriptor Workflow(string projectPath, out string jsonPath)
        {
            jsonPath = null;
            var doc = _metadata.GetAgentCity(projectPath)?.WorkflowDoc;
            if (string.IsNullOrWhiteSpace(doc)) return null;
            var wf = WorkflowDocument.LoadActive(projectPath, doc, out var problem);
            if (problem != null)
            {
                if (_told.TryAdd(projectPath + "|workflow|" + problem, 0))
                    _logger.LogError("[Workflow] il workflow di {Project} non si legge: {Problem}. Lo schedulatore è fermo.", projectPath, problem);
                return null;
            }
            jsonPath = WorkflowDocument.JsonPathOf(projectPath, doc);
            return wf;
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
