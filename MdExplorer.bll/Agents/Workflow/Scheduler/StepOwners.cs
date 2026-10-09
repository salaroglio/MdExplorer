using System;
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
    /// Altrimenti non si sa, e non si indovina: il passo aspetta che qualcuno scelga.
    /// </summary>
    public static class StepOwners
    {
        /// <param name="responsiblesOf">Le email di chi risponde di un agente, dal documento delle responsabilità.</param>
        public static StepOwner Of(WorkflowDescriptor wf, RoundState round, WorkflowStep step, Func<string, IReadOnlyList<string>> responsiblesOf)
        {
            var team = responsiblesOf(step.Agent) ?? Array.Empty<string>();
            var chosen = Chosen(wf, round, step, new HashSet<string>(StringComparer.Ordinal));
            if (chosen != null)
            {
                if (!team.Contains(chosen, StringComparer.OrdinalIgnoreCase))
                    return new StepOwner
                    {
                        Problem = $"«{step.Label}» è di {chosen}, che non risponde più dell'agente '{step.Agent}' " +
                                  $"({(team.Count == 0 ? "non ne risponde nessuno" : "ne rispondono " + string.Join(", ", team))}): " +
                                  "chi ne risponde lo deve prendere, o va corretto il documento delle responsabilità.",
                    };
                return new StepOwner { Email = chosen.ToLowerInvariant() };
            }
            if (team.Count == 1) return new StepOwner { Email = team[0].ToLowerInvariant() };
            if (team.Count == 0)
                return new StepOwner { Problem = $"«{step.Label}»: l'agente '{step.Agent}' non ha un responsabile nel documento delle responsabilità." };
            return new StepOwner
            {
                NeedsChoice = true,
                Problem = $"«{step.Label}»: l'agente '{step.Agent}' ha più responsabili ({string.Join(", ", team)}) e nessuno ha scelto a chi va. " +
                          "Lo sceglie chi fa partire il passo; se parte da solo, l'autore del workflow lo deve far venire da un passo dello stesso agente.",
            };
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
