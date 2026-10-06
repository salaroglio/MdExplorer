using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Who answers for each agent of a project, seen from this computer.</summary>
    public sealed class AgentOwnersView
    {
        /// <summary>False where the city is off: there the rule does not apply and nobody is asked.</summary>
        public bool Applies { get; init; }
        /// <summary>Who this computer's person is for the project (git email, or the impersonated one).</summary>
        public string Me { get; init; }
        /// <summary>The responsibilities' document, relative to the project; null when none is declared yet.</summary>
        public string Document { get; init; }
        /// <summary>Why the declared document is not being used (missing, refused), for the person.</summary>
        public string DocumentProblem { get; init; }
        public IReadOnlyList<AgentOwnerVerdict> Agents { get; init; } = Array.Empty<AgentOwnerVerdict>();
    }

    /// <summary>
    /// The responsibilities of a project's agents, to show them and to take one: «this agent is mine» writes
    /// a row in the project's document (a file of the team, in git), never anything about trust — that stays
    /// per person, in the user's database.
    /// </summary>
    public interface IAgentOwnershipDesk
    {
        AgentOwnersView Describe(string projectPath);

        /// <summary>
        /// Gives agents of nobody to this computer's person, <b>in one write</b> of the document: the file is
        /// watched, and a write per agent would make every open view reload it as many times. An agent that
        /// is already the person's is left as it is. Throws <see cref="InvalidOperationException"/>, with
        /// what to do, when it cannot — and then writes nothing: an agent already answers to someone else,
        /// the person has no git email, the document cannot be written.
        /// </summary>
        IReadOnlyList<AgentOwnerVerdict> AssignToMe(string projectPath, IEnumerable<string> agentNames);
    }

    public sealed class AgentOwnershipDesk : IAgentOwnershipDesk
    {
        /// <summary>Where the document goes in a project that has none.</summary>
        public const string DefaultDocument = "responsabilita-agenti.md";

        private readonly IProjectMetadataService _metadata;
        private readonly IProjectOwnershipService _ownership;
        private readonly IEffectiveOwnerIdentity _identity;
        private readonly IAgentRegistryService _registry;
        private readonly ILogger<AgentOwnershipDesk> _logger;

        public AgentOwnershipDesk(IProjectMetadataService metadata, IProjectOwnershipService ownership,
            IEffectiveOwnerIdentity identity, IAgentRegistryService registry, ILogger<AgentOwnershipDesk> logger)
        {
            _metadata = metadata;
            _ownership = ownership;
            _identity = identity;
            _registry = registry;
            _logger = logger;
        }

        public AgentOwnersView Describe(string projectPath)
        {
            var city = _metadata.GetAgentCity(projectPath);
            if (city?.Enabled != true)
                return new AgentOwnersView { Applies = false };

            var me = _identity.ResolveEmail(projectPath);
            var table = _ownership.GetActiveOwnership(projectPath);
            return new AgentOwnersView
            {
                Applies = true,
                Me = me,
                Document = string.IsNullOrWhiteSpace(city.OwnershipDoc) ? null : city.OwnershipDoc,
                DocumentProblem = ProblemOf(projectPath, city.OwnershipDoc),
                Agents = _registry.RefreshCatalog(projectPath)
                    .Where(e => e.IsCitizen && IsLlm(e))
                    .Select(e => AgentOwnerRule.Decide(table, e.Name, me))
                    .ToList(),
            };
        }

        public IReadOnlyList<AgentOwnerVerdict> AssignToMe(string projectPath, IEnumerable<string> agentNames)
        {
            var city = _metadata.GetAgentCity(projectPath);
            if (city?.Enabled != true)
                throw new InvalidOperationException("La città degli agenti è spenta in questo progetto: accendila nelle impostazioni del progetto.");

            var identity = _identity.Resolve(projectPath);
            if (string.IsNullOrWhiteSpace(identity.Email))
                throw new InvalidOperationException(
                    "Il progetto non ha un'email git su questo computer, quindi non si sa chi sei. " +
                    "Impostala (git config user.email) e riprova.");

            var problem = ProblemOf(projectPath, city.OwnershipDoc);
            if (problem != null)
                throw new InvalidOperationException(problem);

            var catalog = _registry.RefreshCatalog(projectPath);
            var table = _ownership.GetActiveOwnership(projectPath);
            var toTake = new List<string>();
            foreach (var asked in (agentNames ?? Enumerable.Empty<string>()).Where(n => !string.IsNullOrWhiteSpace(n))
                         .Select(n => n.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var agent = catalog.FirstOrDefault(e => e.IsCitizen && IsLlm(e) && string.Equals(e.Name, asked, StringComparison.OrdinalIgnoreCase));
                if (agent == null)
                    throw new InvalidOperationException($"L'agente '{asked}' non è tra gli agenti della città di questo progetto.");

                var now = AgentOwnerRule.Decide(table, agent.Name, identity.Email);
                if (now.Kind == AgentOwnerKind.Mine) continue;
                if (now.Kind != AgentOwnerKind.Unassigned)
                    // Taking an agent from the person who answers for it is not a button: it is a change to
                    // the team's document, made where the team sees it.
                    throw new InvalidOperationException(now.Explain() + " Per cambiare responsabile, o aggiungerne uno al team, modifica il documento delle responsabilità.");
                toTake.Add(agent.Name);
            }

            if (toTake.Count > 0)
            {
                var relative = string.IsNullOrWhiteSpace(city.OwnershipDoc) ? DefaultDocument : city.OwnershipDoc;
                var path = Path.Combine(projectPath, relative.Replace('/', Path.DirectorySeparatorChar));
                var document = File.Exists(path) ? File.ReadAllText(path) : null;
                var name = NameOf(projectPath, identity, table);
                foreach (var agent in toTake)
                    document = OwnershipDocWriter.AddRow(document, agent, name, identity.Email);

                Directory.CreateDirectory(Path.GetDirectoryName(path));
                // Written aside and moved into place: the document is watched, and a file rewritten in place
                // is announced twice (emptied, then filled) to every view that follows it.
                var aside = path + ".scrittura";
                File.WriteAllText(aside, document);
                File.Move(aside, path, overwrite: true);
                if (string.IsNullOrWhiteSpace(city.OwnershipDoc))
                {
                    city.OwnershipDoc = relative;
                    _metadata.SetAgentCity(projectPath, city);
                }
                _logger.LogInformation("[Ownership] {Agents} ora rispondono a {Email} ({Doc})", string.Join(", ", toTake), identity.Email, relative);
            }

            var after = _ownership.GetActiveOwnership(projectPath);
            return (agentNames ?? Enumerable.Empty<string>()).Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => AgentOwnerRule.Decide(after, n.Trim(), identity.Email)).ToList();
        }

        /// <summary>
        /// The rule is about the agents that write something a person answers for. An algorithmic agent is a
        /// piece of the application, the same on every computer: nobody takes it.
        /// </summary>
        private static bool IsLlm(AgentRegistryEntry e)
            => string.Equals(e.Kind, MdExplorer.Abstractions.Entities.UserDB.AgentIdentity.KindEnum.Llm, StringComparison.OrdinalIgnoreCase);

        /// <summary>Why a declared document is not in use; null when it is, or when none is declared.</summary>
        private static string ProblemOf(string projectPath, string document)
        {
            if (string.IsNullOrWhiteSpace(document)) return null;
            var path = Path.Combine(projectPath, document.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path)) return null;   // declared and not written yet: the first assignment writes it

            OwnershipParseResult parsed;
            try { parsed = OwnershipDocParser.Parse(File.ReadAllText(path)); }
            catch (Exception ex) { return $"Il documento delle responsabilità '{document}' non si legge: {ex.Message}"; }

            if (!parsed.IsOwnershipDoc)
                return $"Il documento delle responsabilità '{document}' non dichiara 'mde_type: ownership' in testa: finché non lo fa, nessun agente ha un responsabile.";
            if (parsed.HasErrors)
                return $"Il documento delle responsabilità '{document}' è rifiutato: {string.Join(" ", parsed.Errors)}";
            return null;
        }

        /// <summary>The person's name for the table: the one already written there, else git's, else the email's.</summary>
        private static string NameOf(string projectPath, OwnerIdentity identity, IReadOnlyList<OwnershipEntry> table)
        {
            var known = table?.FirstOrDefault(e => string.Equals(e.GitEmail, identity.Email, StringComparison.OrdinalIgnoreCase)
                                                   && !string.IsNullOrWhiteSpace(e.Responsible))?.Responsible;
            if (known != null) return known;
            if (!identity.Impersonated)
            {
                try
                {
                    using var repo = new LibGit2Sharp.Repository(projectPath);
                    var name = repo.Config.Get<string>("user.name")?.Value;
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
                catch { /* no repository, no name: the email says who it is */ }
            }
            return identity.Email.Split('@')[0];
        }
    }
}
