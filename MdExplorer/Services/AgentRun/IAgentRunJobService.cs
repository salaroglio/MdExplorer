using System;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Request to execute a <c>*.agent.md</c> agent headless (Copilot CLI) inside a project.
    /// </summary>
    public class AgentRunRequestModel
    {
        public Guid RunId { get; set; } = Guid.NewGuid();
        public string ProjectPath { get; set; }
        public string AgentFilePath { get; set; }
        /// <summary>Normalized prompt with parameter placeholders already substituted.</summary>
        public string PreparedPrompt { get; set; }
        /// <summary>
        /// "manual" | "cron" | "commit" | "projectOpen" | "message" (dispatcher) |
        /// "federated-result" (Fase 7a: risveglio di ritorno di un intervento federato).
        /// Stringhe letterali, non costanti.
        /// </summary>
        public string TriggerSource { get; set; } = "manual";
        /// <summary>SignalR connection to notify; null → broadcast to all clients.</summary>
        public string ConnectionId { get; set; }
        /// <summary>Set when the run originates from a saved schedule.</summary>
        public Guid? ScheduleId { get; set; }
    }

    /// <summary>
    /// Runs a <c>*.agent.md</c> agent headless in the background. One concurrent run per
    /// agent file; progress is streamed via the <c>agentJobProgress</c> SignalR event on
    /// MonitorMDHub. Pattern twin of <see cref="MarkActions.IMarkFolderJobService"/>.
    /// </summary>
    public interface IAgentRunJobService
    {
        /// <summary>
        /// Starts the run. Throws <see cref="InvalidOperationException"/> synchronously if a
        /// run for the same agent file is already in progress (callers map it to HTTP 409).
        /// </summary>
        Task RunAsync(AgentRunRequestModel request, CancellationToken ct = default);

        /// <summary>Requests cancellation of the run for the given agent file, if any.</summary>
        void Cancel(string agentFilePath);

        bool IsRunning(string agentFilePath);
    }
}
