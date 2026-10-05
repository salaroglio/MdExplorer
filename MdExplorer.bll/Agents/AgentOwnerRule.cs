using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents
{
    /// <summary>Whose an agent is, seen from this computer.</summary>
    public enum AgentOwnerKind
    {
        /// <summary>The person of this computer answers for the agent: it works here.</summary>
        Mine,
        /// <summary>Someone else answers for it: it works on that person's computer, never here.</summary>
        SomeoneElse,
        /// <summary>Nobody answers for it: it does not work anywhere until it is assigned.</summary>
        Unassigned,
        /// <summary>The responsibilities' table gives it to more than one person: as good as nobody.</summary>
        Contested,
    }

    public sealed class AgentOwnerVerdict
    {
        public AgentOwnerKind Kind { get; init; }
        public string AgentName { get; init; }
        /// <summary>The responsible's name and git email, when there is exactly one.</summary>
        public string OwnerName { get; init; }
        public string OwnerEmail { get; init; }
        /// <summary>The ambit of the responsible's row that names the agent: what a message sent to that person's computer is filed under.</summary>
        public string Scope { get; init; }
        /// <summary>The git emails the table names, when they are more than one.</summary>
        public IReadOnlyList<string> ContestedBy { get; init; } = Array.Empty<string>();
        /// <summary>Who this computer's person is for the project (git email); empty when it has none.</summary>
        public string LocalEmail { get; init; }

        /// <summary>Only an agent of this computer's person works on this computer.</summary>
        public bool CanWorkHere => Kind == AgentOwnerKind.Mine;

        /// <summary>Why the agent does not work here and what to do, for the person. Null when it does.</summary>
        public string Explain()
        {
            switch (Kind)
            {
                case AgentOwnerKind.SomeoneElse:
                    var who = string.IsNullOrWhiteSpace(OwnerName) ? OwnerEmail : $"{OwnerName} ({OwnerEmail})";
                    var me = string.IsNullOrWhiteSpace(LocalEmail)
                        ? "Su questo computer il progetto non ha un'email git: senza, nessun agente risulta tuo."
                        : $"Su questo computer sei {LocalEmail}.";
                    return $"L'agente '{AgentName}' risponde a {who}: lavora solo sul suo computer, non su questo. {me}";
                case AgentOwnerKind.Contested:
                    return $"L'agente '{AgentName}' ha più di un responsabile nel documento delle responsabilità " +
                           $"({string.Join(", ", ContestedBy)}): non parte finché non ne resta uno solo.";
                case AgentOwnerKind.Unassigned:
                    return $"L'agente '{AgentName}' non ha un responsabile: non parte finché non lo assegni a una persona " +
                           "nel documento delle responsabilità del progetto.";
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// The rule of where an agent works: <b>on the computer of the person who answers for its output</b>, and
    /// nowhere else. The responsibilities' table (<see cref="OwnershipEntry"/>) says who that person is; this
    /// computer's person is a git email. An agent of nobody does not work at all: someone must answer for
    /// what an agent writes before it writes.
    /// </summary>
    public static class AgentOwnerRule
    {
        public static AgentOwnerVerdict Decide(IEnumerable<OwnershipEntry> ownership, string agentName, string localEmail)
        {
            var name = (agentName ?? string.Empty).Trim();
            var local = (localEmail ?? string.Empty).Trim();

            var rows = (ownership ?? Enumerable.Empty<OwnershipEntry>())
                .Where(e => e?.Agents != null && !string.IsNullOrWhiteSpace(e.GitEmail)
                            && e.Agents.Any(a => string.Equals(a?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            var emails = rows.Select(e => e.GitEmail.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (emails.Count == 0)
                return new AgentOwnerVerdict { Kind = AgentOwnerKind.Unassigned, AgentName = name, LocalEmail = local };
            if (emails.Count > 1)
                return new AgentOwnerVerdict { Kind = AgentOwnerKind.Contested, AgentName = name, ContestedBy = emails, LocalEmail = local };

            var mine = local.Length > 0 && string.Equals(emails[0], local, StringComparison.OrdinalIgnoreCase);
            return new AgentOwnerVerdict
            {
                Kind = mine ? AgentOwnerKind.Mine : AgentOwnerKind.SomeoneElse,
                AgentName = name,
                OwnerName = rows[0].Responsible,
                OwnerEmail = emails[0],
                Scope = rows[0].Scope,
                LocalEmail = local,
            };
        }
    }
}
