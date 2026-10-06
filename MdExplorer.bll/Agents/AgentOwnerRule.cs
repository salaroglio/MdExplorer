using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents
{
    /// <summary>Whose an agent is, seen from this computer.</summary>
    public enum AgentOwnerKind
    {
        /// <summary>The person of this computer is one of the people who answer for the agent: it works here.</summary>
        Mine,
        /// <summary>Other people answer for it: it works on their computers, never here.</summary>
        SomeoneElse,
        /// <summary>Nobody answers for it: it does not work anywhere until it is assigned.</summary>
        Unassigned,
    }

    /// <summary>One of the people who answer for an agent (a row of the responsibilities' table).</summary>
    public sealed class AgentOwnerPerson
    {
        public string Name { get; init; }
        public string Email { get; init; }
        public string Scope { get; init; }
    }

    public sealed class AgentOwnerVerdict
    {
        public AgentOwnerKind Kind { get; init; }
        public string AgentName { get; init; }
        /// <summary>
        /// Everyone who answers for the agent (W20): a team can share one agent («sviluppatore»), and who starts a piece
        /// of work chooses which of them takes it.
        /// </summary>
        public IReadOnlyList<AgentOwnerPerson> Owners { get; init; } = Array.Empty<AgentOwnerPerson>();
        /// <summary>The responsible's name, git email and ambit, when there is exactly one: where a message is forwarded.</summary>
        public string OwnerName => Owners.Count == 1 ? Owners[0].Name : null;
        public string OwnerEmail => Owners.Count == 1 ? Owners[0].Email : null;
        public string Scope => Owners.Count == 1 ? Owners[0].Scope : null;
        /// <summary>Who this computer's person is for the project (git email); empty when it has none.</summary>
        public string LocalEmail { get; init; }

        /// <summary>An agent works only on the computer of one of the people who answer for it.</summary>
        public bool CanWorkHere => Kind == AgentOwnerKind.Mine;

        /// <summary>True when <paramref name="email"/> is one of the people who answer for the agent.</summary>
        public bool IsOwner(string email)
            => !string.IsNullOrWhiteSpace(email) && Owners.Any(o => string.Equals(o.Email, email.Trim(), StringComparison.OrdinalIgnoreCase));

        public static string Describe(AgentOwnerPerson p) => string.IsNullOrWhiteSpace(p.Name) ? p.Email : $"{p.Name} ({p.Email})";

        /// <summary>Why the agent does not work here and what to do, for the person. Null when it does.</summary>
        public string Explain()
        {
            switch (Kind)
            {
                case AgentOwnerKind.SomeoneElse:
                    var who = string.Join(", ", Owners.Select(Describe));
                    var me = string.IsNullOrWhiteSpace(LocalEmail)
                        ? "Su questo computer il progetto non ha un'email git: senza, nessun agente risulta tuo."
                        : $"Su questo computer sei {LocalEmail}.";
                    return Owners.Count == 1
                        ? $"L'agente '{AgentName}' risponde a {who}: lavora solo sul suo computer, non su questo. {me}"
                        : $"L'agente '{AgentName}' risponde a {who}: lavora solo sui loro computer, non su questo. {me}";
                case AgentOwnerKind.Unassigned:
                    return $"L'agente '{AgentName}' non ha un responsabile: non parte finché non lo assegni a una persona " +
                           "nel documento delle responsabilità del progetto.";
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// The rule of where an agent works: <b>on the computer of a person who answers for its output</b>, and nowhere else.
    /// The responsibilities' table (<see cref="OwnershipEntry"/>) says who those people are — one, or a team (W20); this
    /// computer's person is a git email. An agent of nobody does not work at all: someone must answer for what an agent
    /// writes before it writes. Which member of a team takes a piece of a workflow is decided by who starts it, and
    /// written in the round (W22), not here.
    /// </summary>
    public static class AgentOwnerRule
    {
        public static AgentOwnerVerdict Decide(IEnumerable<OwnershipEntry> ownership, string agentName, string localEmail)
        {
            var name = (agentName ?? string.Empty).Trim();
            var local = (localEmail ?? string.Empty).Trim();

            var owners = (ownership ?? Enumerable.Empty<OwnershipEntry>())
                .Where(e => e?.Agents != null && !string.IsNullOrWhiteSpace(e.GitEmail)
                            && e.Agents.Any(a => string.Equals(a?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
                .GroupBy(e => e.GitEmail.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new AgentOwnerPerson
                {
                    Email = g.Key.ToLowerInvariant(),
                    Name = g.Select(e => e.Responsible).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)),
                    Scope = g.First().Scope,
                })
                .ToList();

            var kind = owners.Count == 0 ? AgentOwnerKind.Unassigned
                     : local.Length > 0 && owners.Any(o => string.Equals(o.Email, local, StringComparison.OrdinalIgnoreCase)) ? AgentOwnerKind.Mine
                     : AgentOwnerKind.SomeoneElse;
            return new AgentOwnerVerdict { Kind = kind, AgentName = name, Owners = owners, LocalEmail = local };
        }
    }
}
