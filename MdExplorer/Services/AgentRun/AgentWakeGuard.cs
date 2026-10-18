using MdExplorer.Features.Agents;
using MdExplorer.Services.Federation;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Whether an agent may work on this computer (<see cref="AgentOwnerRule"/>), asked wherever an agent is
    /// about to start: a colleague's message, an approval's notice, a person's launch, a schedule.
    /// <para>
    /// The rule belongs to the city: where the city is off there is no responsibilities' table and an agent
    /// is a person's own tool, launched by hand.
    /// </para>
    /// </summary>
    public interface IAgentWakeGuard
    {
        /// <summary>Null when the rule does not apply to the project (city off); otherwise whose the agent is.</summary>
        AgentOwnerVerdict Check(string projectPath, string agentName);

        /// <summary>
        /// True for an agent that works on another person's computer: here it is not enabled and never
        /// starts, but it can be written to — the message travels to its owner's computer.
        /// </summary>
        bool WorksElsewhere(string projectPath, AgentRegistryEntry agent);

        /// <summary>Who an agent or a person can write to from this computer: the agents enabled here, and those that work elsewhere.</summary>
        bool CanBeWrittenTo(string projectPath, AgentRegistryEntry agent);
    }

    public sealed class AgentWakeGuard : IAgentWakeGuard
    {
        private readonly IProjectMetadataService _metadata;
        private readonly IProjectOwnershipService _ownership;
        private readonly IEffectiveOwnerIdentity _identity;

        public AgentWakeGuard(IProjectMetadataService metadata, IProjectOwnershipService ownership, IEffectiveOwnerIdentity identity)
        {
            _metadata = metadata;
            _ownership = ownership;
            _identity = identity;
        }

        public AgentOwnerVerdict Check(string projectPath, string agentName)
        {
            if (_metadata.GetAgentCity(projectPath)?.Enabled != true)
                return null;
            // No table, or one that was refused: every agent is nobody's, and says so.
            return AgentOwnerRule.Decide(_ownership.GetActiveOwnership(projectPath), agentName, _identity.ResolveEmail(projectPath));
        }

        public bool WorksElsewhere(string projectPath, AgentRegistryEntry agent)
            => agent != null && agent.IsCitizen
               && string.Equals(agent.Kind, MdExplorer.Abstractions.Entities.UserDB.AgentIdentity.KindEnum.Llm, System.StringComparison.OrdinalIgnoreCase)
               && Check(projectPath, agent.Name)?.Kind == AgentOwnerKind.SomeoneElse;

        public bool CanBeWrittenTo(string projectPath, AgentRegistryEntry agent)
            => agent != null && agent.IsCitizen && (agent.Trusted || WorksElsewhere(projectPath, agent));
    }
}
