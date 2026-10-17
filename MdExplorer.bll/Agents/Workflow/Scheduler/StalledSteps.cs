using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents.Workflow.Scheduler
{
    /// <summary>Un passo di un altro computer fermo da troppo: chi deve fare il passo e da quando non succede niente.</summary>
    public class StalledStep
    {
        public WorkflowStep Step { get; set; }
        public string OwnerEmail { get; set; }
        public StepStatus Status { get; set; }
        public DateTime Since { get; set; }
        public int Round { get; set; }
        public int Attempt { get; set; }
    }

    /// <summary>
    /// I passi di un giro affidati a un altro computer che non danno notizie da <see cref="Threshold"/> (17/10/2026,
    /// piano «Avviso di lavoro non processato», F4). Chi li ha passati lo deve sapere: è da lì che parte una verifica fatta
    /// dalle persone. Puro: lo stato del giro, chi risponde di ogni passo, chi sono e l'ora arrivano da fuori.
    /// <para>
    /// Fermo vuol dire: «da avviare» o «al lavoro» senza eventi nuovi, oppure pronto a partire (i passi da cui dipende
    /// sono finiti) e mai toccato, perché il computer del responsabile non l'ha preso. Un passo consegnato, rifiutato,
    /// non avviato o finito ha già dato la sua notizia.
    /// </para>
    /// </summary>
    public static class StalledSteps
    {
        public static readonly TimeSpan Threshold = TimeSpan.FromHours(24);

        /// <param name="ownerOf">L'email di chi risponde del passo, null se non si sa (lo dice già un altro avviso).</param>
        /// <param name="me">L'email di chi usa questo computer.</param>
        public static IReadOnlyList<StalledStep> Find(WorkflowDescriptor wf, RoundState round, Func<WorkflowStep, string> ownerOf,
                                                      string me, DateTime now)
        {
            if (wf == null) throw new ArgumentNullException(nameof(wf));
            if (round == null) throw new ArgumentNullException(nameof(round));
            if (ownerOf == null) throw new ArgumentNullException(nameof(ownerOf));

            var views = WorkflowScheduler.Views(wf, round);

            var result = new List<StalledStep>();
            foreach (var step in wf.Steps.Where(s => s.Id != null && s.Trigger != null))
            {
                var owner = ownerOf(step);
                if (string.IsNullOrWhiteSpace(owner) || Same(owner, me)) continue;
                if (!PassedByMe(wf, round, step, ownerOf, me)) continue;

                var view = views[step.Id];
                DateTime? since = view.Status switch
                {
                    StepStatus.Held or StepStatus.Working => view.Last?.At,
                    StepStatus.NotStarted when step.Trigger.Kind != WorkflowTriggerKind.Launch
                                               && WorkflowScheduler.TriggerSatisfied(wf, round, views, step)
                        => LastNews(wf, round, step),
                    _ => null,
                };
                if (since == null || now - since.Value < Threshold) continue;
                result.Add(new StalledStep
                {
                    Step = step, OwnerEmail = owner.Trim().ToLowerInvariant(), Status = view.Status,
                    Since = since.Value, Round = view.Round, Attempt = view.Attempt,
                });
            }
            return result;
        }

        /// <summary>
        /// Chi ha passato il lavoro: il responsabile di uno dei passi da cui questo dipende o, se non ne ha (o nessuno di
        /// quelli è suo), chi ha lanciato il giro. Gli altri computer del team non ricevono l'avviso.
        /// </summary>
        private static bool PassedByMe(WorkflowDescriptor wf, RoundState round, WorkflowStep step, Func<WorkflowStep, string> ownerOf, string me)
        {
            var sources = step.Trigger.Sources.Select(wf.Step).Where(s => s != null).ToList();
            if (sources.Any(s => Same(ownerOf(s), me))) return true;
            return sources.All(s => string.IsNullOrWhiteSpace(ownerOf(s))) && Same(round.Header?.StartedBy, me);
        }

        /// <summary>L'ultima notizia dai passi da cui dipende: da lì il passo poteva partire. Senza, l'apertura del giro.</summary>
        private static DateTime LastNews(WorkflowDescriptor wf, RoundState round, WorkflowStep step)
        {
            var times = step.Trigger.Sources.SelectMany(round.EventsOf).Select(e => e.At).ToList();
            return times.Count > 0 ? times.Max() : round.Header?.StartedAt ?? DateTime.MinValue;
        }

        private static bool Same(string a, string b)
            => !string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b)
               && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
