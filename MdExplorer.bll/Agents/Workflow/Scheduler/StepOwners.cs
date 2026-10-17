using System;
using MdExplorer.Features.Services;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents.Workflow.Scheduler
{
    /// <summary>Chi risponde di un passo di un giro, o perché non si sa.</summary>
    public sealed class StepOwner
    {
        /// <summary>L'email della persona; null quando non si sa (vedi <see cref="Problem"/>).</summary>
        public string Email { get; init; }
        /// <summary>Perché non si sa, o perché chi risulta non va bene: da dire alla persona.</summary>
        public string Problem { get; init; }
        /// <summary>Il passo aspetta che qualcuno scelga tra più responsabili (non è un errore del workflow).</summary>
        public bool NeedsChoice { get; init; }
        /// <summary>
        /// Il passo è di una persona che non risponde più dell'agente (ha cambiato email, è uscita dal team) e l'agente ha più
        /// responsabili: lo prende uno di loro («Prendo io»). Vuoto negli altri casi.
        /// </summary>
        public IReadOnlyList<string> CanTake { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Chi risponde di un passo (W20-W22). Un agente può essere di un team; la responsabilità si assegna al <b>pezzo di
    /// workflow</b>: chi fa scattare la richiesta sceglie, e da lì quella persona ne risponde. Puro e deterministico: ogni
    /// computer, leggendo lo stesso registro, arriva alla stessa persona, ed è solo il suo computer che fa il passo.
    /// <list type="number">
    /// <item>l'ultimo «passato a» scritto nel passo;</item>
    /// <item>la scelta fatta con il pulsante che fa partire il passo;</item>
    /// <item>chi ha già agito sul passo (il primo evento: un lancio, un «da avviare», un avvio);</item>
    /// <item>il responsabile del passo da cui viene, se è dello stesso agente (risalendo il workflow);</item>
    /// <item>l'unico responsabile dell'agente.</item>
    /// </list>
    /// Se la persona così trovata non risponde più dell'agente: l'unico responsabile, se è uno; altrimenti lo prende uno del
    /// team (<see cref="StepOwner.CanTake"/>).
    /// Altrimenti non si sa, e non si indovina: il passo aspetta che qualcuno scelga.
    /// </summary>
    public static class StepOwners
    {
        /// <param name="responsiblesOf">Le email di chi risponde di un agente, dal documento delle responsabilità.</param>
        /// <param name="membersOf">Le email di chi ha un ruolo: per i passi delle persone (workflow v3, F4).</param>
        public static StepOwner Of(WorkflowDescriptor wf, RoundState round, WorkflowStep step, Func<string, IReadOnlyList<string>> responsiblesOf,
            Func<string, IReadOnlyList<string>> membersOf = null)
        {
            if (step.IsPerson) return OfPerson(wf, round, step, membersOf ?? throw new ArgumentNullException(nameof(membersOf),
                $"step '{step.Id}' is a person's step: who has the role «{step.Person}» is needed."));
            var team = responsiblesOf(step.Agent) ?? Array.Empty<string>();
            var chosen = Chosen(wf, round, step, new HashSet<string>(StringComparer.Ordinal));
            if (chosen != null)
            {
                if (team.Contains(chosen, StringComparer.OrdinalIgnoreCase))
                    return new StepOwner { Email = chosen.ToLowerInvariant() };
                // Chi era scritto nel registro non risponde più dell'agente (email cambiata, uscito dal team: 15/10/2026).
                // Con un solo responsabile il passo è suo: non c'è niente da scegliere, e ogni computer arriva a lui.
                if (team.Count == 1) return new StepOwner { Email = team[0].ToLowerInvariant() };
                return new StepOwner
                {
                    Problem = UiText.Say(
                        $"«{step.Label}» è di {chosen}, che non risponde più dell'agente '{step.Agent}' " +
                        $"({(team.Count == 0 ? "non ne risponde nessuno" : "ne rispondono " + string.Join(", ", team))}): " +
                        (team.Count == 0 ? "va corretto il documento delle responsabilità." : "uno di loro lo prende con «Prendo io»."),
                        $"«{step.Label}» belongs to {chosen}, who is no longer an owner of agent '{step.Agent}' " +
                        $"({(team.Count == 0 ? "it has no owner" : "owners: " + string.Join(", ", team))}): " +
                        (team.Count == 0 ? "the responsibilities document must be fixed." : "one of them takes it with «I'll take it».")),
                    CanTake = team.Select(e => e.ToLowerInvariant()).ToList(),
                };
            }
            if (team.Count == 1) return new StepOwner { Email = team[0].ToLowerInvariant() };
            if (team.Count == 0)
                return new StepOwner { Problem = UiText.Say(
                    $"«{step.Label}»: l'agente '{step.Agent}' non ha un responsabile nel documento delle responsabilità.",
                    $"«{step.Label}»: agent '{step.Agent}' has no owner in the responsibilities document.") };
            return new StepOwner
            {
                NeedsChoice = true,
                Problem = UiText.Say(
                    $"«{step.Label}»: l'agente '{step.Agent}' ha più responsabili ({string.Join(", ", team)}) e nessuno ha scelto a chi va. " +
                    "Lo sceglie chi fa partire il passo; se parte da solo, l'autore del workflow lo deve far venire da un passo dello stesso agente.",
                    $"«{step.Label}»: agent '{step.Agent}' has more than one owner ({string.Join(", ", team)}) and no one has chosen who gets it. " +
                    "Whoever starts the step chooses; if it starts by itself, the workflow author must make it follow a step of the same agent."),
            };
        }

        /// <summary>
        /// Il passo di una persona è di chi ha il suo ruolo. Con una persona sola è suo. Con un team: chi è scritto nel registro
        /// (un «passato a», la scelta di chi ha premuto il pulsante che lo fa partire, chi ha già agito), altrimenti la prima
        /// persona del team che ha agito sui passi da cui viene (chi ha fatto la sintesi la legge anche). Se nessuno, si sceglie.
        /// </summary>
        private static StepOwner OfPerson(WorkflowDescriptor wf, RoundState round, WorkflowStep step, Func<string, IReadOnlyList<string>> membersOf)
        {
            var team = membersOf(step.Person) ?? Array.Empty<string>();
            if (team.Count == 0)
                return new StepOwner { Problem = UiText.Say(
                    $"«{step.Label}»: nessuno ha il ruolo «{step.Person}» nel documento delle responsabilità.",
                    $"«{step.Label}»: no one has the role «{step.Person}» in the responsibilities document.") };
            if (team.Count == 1) return new StepOwner { Email = team[0].ToLowerInvariant() };

            bool InTeam(string email) => !string.IsNullOrWhiteSpace(email) && team.Contains(email.Trim(), StringComparer.OrdinalIgnoreCase);
            var written = Written(round, step);
            if (written != null)
                return InTeam(written)
                    ? new StepOwner { Email = written.Trim().ToLowerInvariant() }
                    : new StepOwner
                    {
                        Problem = UiText.Say(
                            $"«{step.Label}» è di {written}, che non ha più il ruolo «{step.Person}» (ce l'hanno {string.Join(", ", team)}): uno di loro lo prende con «Prendo io».",
                            $"«{step.Label}» belongs to {written}, who no longer has the role «{step.Person}» (it belongs to {string.Join(", ", team)}): one of them takes it with «I'll take it»."),
                        CanTake = team.Select(e => e.ToLowerInvariant()).ToList(),
                    };

            // Risalendo il workflow: il primo del team che ha agito (avviato, consegnato, approvato, premuto un pulsante).
            var seen = new HashSet<string>(StringComparer.Ordinal) { step.Id };
            var queue = new Queue<string>(Sources(wf, step));
            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (id == null || !seen.Add(id)) continue;
                var actor = round.EventsOf(id).Select(e => e.By).FirstOrDefault(InTeam);
                if (actor != null) return new StepOwner { Email = actor.Trim().ToLowerInvariant() };
                var upstream = wf.Step(id);
                if (upstream != null) foreach (var s in Sources(wf, upstream)) queue.Enqueue(s);
            }
            return new StepOwner
            {
                NeedsChoice = true,
                Problem = UiText.Say(
                    $"«{step.Label}»: il ruolo «{step.Person}» è di più persone ({string.Join(", ", team)}) e nessuno ha scelto a chi tocca. " +
                    "Lo sceglie chi fa partire il passo; se parte da solo, deve venire da un passo in cui una di loro ha già agito.",
                    $"«{step.Label}»: the role «{step.Person}» belongs to more than one person ({string.Join(", ", team)}) and no one has chosen whose turn it is. " +
                    "Whoever starts the step chooses; if it starts by itself, it must follow a step one of them has already acted on."),
            };
        }

        /// <summary>Chi il registro dice per il passo: un «passato a», la scelta del pulsante che lo fa partire, chi ha già agito.</summary>
        private static string Written(RoundState round, WorkflowStep step)
        {
            var own = round.EventsOf(step.Id).ToList();
            var passed = own.LastOrDefault(e => e.Type == RoundEventType.Assigned && !string.IsNullOrWhiteSpace(e.Owner));
            if (passed != null) return passed.Owner.Trim();
            var t = step.Trigger;
            if (t?.Kind == WorkflowTriggerKind.Reply)
            {
                var pressed = round.EventsOf(t.FromStep).LastOrDefault(e => e.Type == RoundEventType.Replied
                                                                            && string.Equals(e.Reply, t.ReplyId, StringComparison.OrdinalIgnoreCase));
                if (pressed?.Assign != null && pressed.Assign.TryGetValue(step.Id, out var to) && !string.IsNullOrWhiteSpace(to))
                    return to.Trim();
            }
            return own.FirstOrDefault(e => e.Type != RoundEventType.Replied && !string.IsNullOrWhiteSpace(e.By))?.By.Trim();
        }

        /// <summary>La persona scritta nel registro, o ereditata dal passo da cui viene; null se nessuno l'ha ancora detto.</summary>
        private static string Chosen(WorkflowDescriptor wf, RoundState round, WorkflowStep step, HashSet<string> visited)
        {
            if (step == null || !visited.Add(step.Id)) return null;
            var own = round.EventsOf(step.Id).ToList();

            var passed = own.LastOrDefault(e => e.Type == RoundEventType.Assigned && !string.IsNullOrWhiteSpace(e.Owner));
            if (passed != null) return passed.Owner.Trim();

            var t = step.Trigger;
            if (t?.Kind == WorkflowTriggerKind.Reply)
            {
                var pressed = round.EventsOf(t.FromStep).LastOrDefault(e => e.Type == RoundEventType.Replied
                                                                            && string.Equals(e.Reply, t.ReplyId, StringComparison.OrdinalIgnoreCase));
                if (pressed?.Assign != null && pressed.Assign.TryGetValue(step.Id, out var to) && !string.IsNullOrWhiteSpace(to))
                    return to.Trim();
            }

            var first = own.FirstOrDefault(e => e.Type != RoundEventType.Replied && !string.IsNullOrWhiteSpace(e.By));
            if (first != null) return first.By.Trim();

            // Il pezzo di workflow continua: un passo dello stesso agente che viene da un passo suo resta di chi l'aveva.
            foreach (var source in Sources(wf, step))
            {
                var upstream = wf.Step(source);
                if (upstream == null) continue;
                if (string.Equals(upstream.Agent, step.Agent, StringComparison.OrdinalIgnoreCase))
                {
                    var owner = Chosen(wf, round, upstream, visited);
                    if (owner != null) return owner;
                }
                else
                {
                    var further = Ancestor(wf, round, upstream, step.Agent, visited);
                    if (further != null) return further;
                }
            }
            return null;
        }

        /// <summary>Risalendo da un passo di un altro agente, il primo passo dello stesso agente che ha un responsabile.</summary>
        private static string Ancestor(WorkflowDescriptor wf, RoundState round, WorkflowStep from, string agent, HashSet<string> visited)
        {
            foreach (var source in Sources(wf, from))
            {
                var s = wf.Step(source);
                if (s == null || visited.Contains(s.Id)) continue;
                if (string.Equals(s.Agent, agent, StringComparison.OrdinalIgnoreCase))
                {
                    var owner = Chosen(wf, round, s, visited);
                    if (owner != null) return owner;
                }
                else
                {
                    visited.Add(s.Id);
                    var further = Ancestor(wf, round, s, agent, visited);
                    if (further != null) return further;
                }
            }
            return null;
        }

        private static IEnumerable<string> Sources(WorkflowDescriptor wf, WorkflowStep step)
        {
            var t = step.Trigger;
            if (t == null) return Enumerable.Empty<string>();
            return t.Kind switch
            {
                WorkflowTriggerKind.After => t.After ?? (IEnumerable<string>)Array.Empty<string>(),
                WorkflowTriggerKind.Reply => new[] { t.FromStep },
                _ => Enumerable.Empty<string>(),
            };
        }
    }
}
