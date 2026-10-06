using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Agents.Workflow.Scheduler
{
    /// <summary>A che punto è un passo in un giro, ricavato dai suoi eventi.</summary>
    public enum StepStatus
    {
        /// <summary>Nessun evento: non è ancora toccato a lui.</summary>
        NotStarted,
        /// <summary>Aspetta che il responsabile lo avvii.</summary>
        Held,
        /// <summary>L'agente sta lavorando.</summary>
        Working,
        /// <summary>Ha consegnato: l'artefatto aspetta l'approvazione.</summary>
        Delivered,
        /// <summary>L'artefatto è stato rifiutato: fermo finché qualcuno (o il ciclo) non lo fa ripartire.</summary>
        Rejected,
        /// <summary>Il responsabile non l'ha avviato.</summary>
        Declined,
        Failed,
        /// <summary>Un giro di un ciclo «for» è approvato, ma ne mancano altri.</summary>
        RoundApproved,
        /// <summary>Finito: approvato (o concluso, se non produce artefatti) in tutti i giri che deve fare.</summary>
        Finished,
    }

    public class StepView
    {
        public WorkflowStep Step { get; set; }
        public StepStatus Status { get; set; }
        /// <summary>Il giro in corso (o l'ultimo), da 1.</summary>
        public int Round { get; set; } = 1;
        /// <summary>Il tentativo in corso (o l'ultimo) dentro il giro, da 1.</summary>
        public int Attempt { get; set; } = 1;
        /// <summary>Quanti giri il passo deve fare: 1, o <c>times</c> del suo ciclo «for».</summary>
        public int RoundsNeeded { get; set; } = 1;
        /// <summary>Quanti rifiuti nel giro in corso.</summary>
        public int RejectionsInRound { get; set; }
        /// <summary>L'ultimo evento del passo, se c'è.</summary>
        public RoundEvent Last { get; set; }
        /// <summary>I file consegnati nell'ultimo giro approvato: gli ingressi dei passi che vengono dopo.</summary>
        public IReadOnlyList<string> ApprovedFiles { get; set; } = Array.Empty<string>();
    }

    public enum SchedulerActionKind
    {
        /// <summary>Svegliare l'agente con il brief: il passo parte da solo.</summary>
        Start,
        /// <summary>Mettere il passo «da avviare» nella posta del responsabile.</summary>
        Hold,
        /// <summary>Il passo dovrebbe partire ma non può: c'è un motivo da dire.</summary>
        Blocked,
    }

    /// <summary>Una cosa da fare per un passo dei miei agenti. L'esecutore la fa e scrive l'evento corrispondente.</summary>
    public class SchedulerAction
    {
        public SchedulerActionKind Kind { get; set; }
        public WorkflowStep Step { get; set; }
        public int Round { get; set; } = 1;
        public int Attempt { get; set; } = 1;
        /// <summary>Il brief con le variabili sostituite.</summary>
        public string Brief { get; set; }
        /// <summary>I file dei passi da cui questo dipende (approvati), dalla radice del progetto.</summary>
        public IReadOnlyList<string> Inputs { get; set; } = Array.Empty<string>();
        /// <summary>Per un rifacimento: il motivo del rifiuto, che l'agente riceve.</summary>
        public string ReworkNote { get; set; }
        /// <summary><see cref="SchedulerActionKind.Blocked"/>: perché.</summary>
        public string Reason { get; set; }

        public override string ToString() => $"{Kind} {Step?.Id} r{Round}a{Attempt}" + (Reason == null ? "" : $": {Reason}");
    }

    /// <summary>
    /// Lo schedulatore: dato il workflow, un giro e quali agenti sono di chi lo esegue, dice che cosa fare adesso. È una
    /// funzione pura e ripetibile: niente file, niente git, niente orologio; chiamata due volte sullo stesso stato dà le
    /// stesse azioni, e un'azione già fatta (il suo evento è nel registro) non torna. Così lo stesso codice può girare in
    /// ogni MDE, ciascuno per i propri agenti (W12), o un giorno su un server per tutti.
    /// </summary>
    public static class WorkflowScheduler
    {
        private static readonly Regex Placeholder = new(@"\{([^{}\s]+)\}", RegexOptions.Compiled);

        /// <summary>Lo stato di un passo nel giro.</summary>
        public static StepView View(WorkflowDescriptor wf, RoundState round, WorkflowStep step)
        {
            var events = round.EventsOf(step.Id).Where(e => e.Type != RoundEventType.Replied).ToList();
            var forLoop = wf.LoopOf(step.Id, WorkflowLoopKind.Times);
            var view = new StepView { Step = step, RoundsNeeded = forLoop?.Times ?? 1, Last = events.LastOrDefault() };
            if (events.Count == 0) return view;

            view.Round = events.Max(e => e.Round);
            var current = events.Where(e => e.Round == view.Round).ToList();
            view.Attempt = current.Max(e => e.Attempt);
            view.RejectionsInRound = current.Count(e => e.Type == RoundEventType.Rejected);
            var last = current.Where(e => e.Attempt == view.Attempt).Last();

            // Un giro conta quando è approvato (con artefatto) o concluso (senza).
            var producing = step.Produces.Count > 0;
            var closing = producing ? RoundEventType.Approved : RoundEventType.Done;
            var closedRounds = events.Where(e => e.Type == closing).Select(e => e.Round).Distinct().Count();

            var lastApprovedDelivery = events.Where(e => e.Type == RoundEventType.Delivered
                                                         && events.Any(a => a.Type == RoundEventType.Approved && a.Round == e.Round && a.Attempt == e.Attempt))
                                             .LastOrDefault();
            view.ApprovedFiles = lastApprovedDelivery?.Files?.ToList() ?? (IReadOnlyList<string>)Array.Empty<string>();

            view.Status = last.Type switch
            {
                RoundEventType.Held => StepStatus.Held,
                RoundEventType.Started => StepStatus.Working,
                // Senza artefatto non c'è niente da approvare: una consegna vuota vale come conclusa.
                RoundEventType.Delivered => producing ? StepStatus.Delivered : StepStatus.Working,
                RoundEventType.Rejected => StepStatus.Rejected,
                RoundEventType.Declined => StepStatus.Declined,
                RoundEventType.Failed => StepStatus.Failed,
                RoundEventType.Approved or RoundEventType.Done =>
                    closedRounds >= view.RoundsNeeded ? StepStatus.Finished : StepStatus.RoundApproved,
                _ => StepStatus.Working,
            };
            return view;
        }

        /// <summary>Lo stato di tutti i passi del giro.</summary>
        public static IReadOnlyDictionary<string, StepView> Views(WorkflowDescriptor wf, RoundState round)
            => wf.Steps.Where(s => s.Id != null).ToDictionary(s => s.Id, s => View(wf, round, s));

        /// <summary>
        /// Che cosa fare adesso per i passi degli agenti di cui risponde chi esegue (<paramref name="isMine"/>). Gli altri
        /// passi li decide il computer del loro responsabile: qui servono solo a sapere se i miei possono partire.
        /// </summary>
        public static IReadOnlyList<SchedulerAction> Decide(WorkflowDescriptor wf, RoundState round, Func<string, bool> isMine)
        {
            if (wf == null) throw new ArgumentNullException(nameof(wf));
            if (round == null) throw new ArgumentNullException(nameof(round));
            isMine ??= _ => false;

            var views = Views(wf, round);
            var variables = round.Variables;
            var actions = new List<SchedulerAction>();

            foreach (var step in wf.Steps.Where(s => s.Id != null && s.Trigger != null && isMine(s.Agent)))
            {
                var view = views[step.Id];
                // Un lancio lo fa la persona: lo schedulatore non apre i giri, li prosegue.
                if (step.Trigger.Kind == WorkflowTriggerKind.Launch) continue;

                switch (view.Status)
                {
                    case StepStatus.NotStarted when TriggerSatisfied(wf, round, views, step):
                        actions.Add(Begin(wf, step, views, variables, round: 1, attempt: 1, reworkNote: null));
                        break;

                    case StepStatus.RoundApproved:
                        // «For»: il giro successivo, avviato come il passo dice (da solo o dal responsabile).
                        actions.Add(Begin(wf, step, views, variables, view.Round + 1, attempt: 1, reworkNote: null));
                        break;

                    case StepStatus.Rejected:
                        var until = wf.LoopOf(step.Id, WorkflowLoopKind.UntilApproved);
                        // Riparte da solo solo se l'autore l'ha scritto; altrimenti aspetta «Fai ripartire» della persona.
                        if (until?.Restart != WorkflowStart.Auto) break;
                        if (until.Max != null && view.RejectionsInRound > until.Max.Value)
                        {
                            actions.Add(new SchedulerAction
                            {
                                Kind = SchedulerActionKind.Blocked, Step = step, Round = view.Round, Attempt = view.Attempt,
                                Reason = $"«{step.Label}» è già stato rifatto {until.Max} {(until.Max == 1 ? "volta" : "volte")} in questo giro: è il massimo del ciclo «{until.Id}». Resta fermo.",
                            });
                            break;
                        }
                        // Un rifacimento automatico parte e basta: chi lo avvia l'ha già deciso scrivendo restart: auto.
                        var rework = Begin(wf, step, views, variables, view.Round, view.Attempt + 1, view.Last?.Note);
                        if (rework.Kind == SchedulerActionKind.Hold) rework.Kind = SchedulerActionKind.Start;
                        actions.Add(rework);
                        break;
                }
            }
            return actions;
        }

        /// <summary>Quello che fa partire il passo è successo?</summary>
        public static bool TriggerSatisfied(WorkflowDescriptor wf, RoundState round, IReadOnlyDictionary<string, StepView> views, WorkflowStep step)
        {
            var t = step.Trigger;
            switch (t.Kind)
            {
                case WorkflowTriggerKind.Reply:
                    return round.EventsOf(t.FromStep).Any(e => e.Type == RoundEventType.Replied
                                                               && string.Equals(e.Reply, t.ReplyId, StringComparison.OrdinalIgnoreCase));
                case WorkflowTriggerKind.After:
                    var finished = t.After.Select(a => views.TryGetValue(a, out var v) && v.Status == StepStatus.Finished).ToList();
                    return t.Wait == WorkflowWait.Any ? finished.Any(f => f) : finished.All(f => f);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Avviare un passo: il brief con le variabili e gli ingressi. Se manca una variabile il passo non parte e si dice
        /// quale: un agente che riceve «{codice}» tra parentesi lavorerebbe sul niente.
        /// </summary>
        private static SchedulerAction Begin(WorkflowDescriptor wf, WorkflowStep step, IReadOnlyDictionary<string, StepView> views,
            IReadOnlyDictionary<string, string> variables, int round, int attempt, string reworkNote)
        {
            var missing = new List<string>();
            var brief = step.Brief == null ? null : Placeholder.Replace(step.Brief, m =>
            {
                if (variables.TryGetValue(m.Groups[1].Value, out var v)) return v;
                missing.Add(m.Groups[1].Value);
                return m.Value;
            });
            if (missing.Count > 0)
                return new SchedulerAction
                {
                    Kind = SchedulerActionKind.Blocked, Step = step, Round = round, Attempt = attempt,
                    Reason = $"«{step.Label}» non può partire: nel giro manca il valore di {string.Join(", ", missing.Distinct().Select(n => "{" + n + "}"))}.",
                };

            var inputs = step.Trigger.Kind == WorkflowTriggerKind.After
                ? step.Trigger.After.SelectMany(a => InputsOf(wf, views, a)).Distinct().ToList()
                : new List<string>();
            return new SchedulerAction
            {
                Kind = step.Start == WorkflowStart.AskOwner ? SchedulerActionKind.Hold : SchedulerActionKind.Start,
                Step = step, Round = round, Attempt = attempt, Brief = brief, Inputs = inputs, ReworkNote = reworkNote,
            };
        }

        /// <summary>
        /// I file di un passo da cui si dipende, solo se è finito (con «il primo» gli altri possono non esserlo): quelli
        /// consegnati e approvati, o in mancanza quelli dichiarati.
        /// </summary>
        private static IEnumerable<string> InputsOf(WorkflowDescriptor wf, IReadOnlyDictionary<string, StepView> views, string stepId)
        {
            if (!views.TryGetValue(stepId, out var v) || v.Status != StepStatus.Finished) return Enumerable.Empty<string>();
            if (v.ApprovedFiles.Count > 0) return v.ApprovedFiles;
            return wf.Step(stepId)?.Produces.Where(p => !p.Contains('*')) ?? Enumerable.Empty<string>();
        }
    }
}
