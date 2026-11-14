using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents
{
    /// <summary>An agent's turn that is running right now.</summary>
    public sealed class AgentActivity
    {
        public Guid Id { get; set; }
        public string AgentName { get; set; }
        public string ProjectPath { get; set; }
        /// <summary>The engine the turn runs on, as shown to the person (Copilot, Claude Code, opencode).</summary>
        public string Engine { get; set; }
        public DateTime StartedAtUtc { get; set; }
    }

    /// <summary>
    /// Who is working right now: one entry per agent's turn in progress, whatever started it (a person's
    /// launch, a schedule, a colleague's message). It exists so that the application can show it: until a
    /// turn ends nothing else tells the person that their gesture started something.
    /// </summary>
    public interface IAgentActivityBoard
    {
        /// <summary>Records a turn as running; disposing what it returns records its end, however it ended.</summary>
        IDisposable Begin(string agentName, string projectPath, string engine);

        /// <summary>The turns running in a project, the oldest first.</summary>
        IReadOnlyList<AgentActivity> Running(string projectPath);

        /// <summary>Raised with the project's path every time a turn of that project starts or ends.</summary>
        event Action<string> Changed;
    }

    public sealed class AgentActivityBoard : IAgentActivityBoard
    {
        private readonly ConcurrentDictionary<Guid, AgentActivity> _running = new();

        public event Action<string> Changed;

        public IDisposable Begin(string agentName, string projectPath, string engine)
        {
            var activity = new AgentActivity
            {
                Id = Guid.NewGuid(),
                AgentName = agentName,
                ProjectPath = projectPath,
                Engine = engine,
                StartedAtUtc = DateTime.UtcNow,
            };
            _running[activity.Id] = activity;
            Changed?.Invoke(projectPath);
            return new Turn(this, activity);
        }

        public IReadOnlyList<AgentActivity> Running(string projectPath)
            => _running.Values
                .Where(a => AgentPathComparer.Equals(a.ProjectPath, projectPath))
                .OrderBy(a => a.StartedAtUtc)
                .ToList();

        private void End(AgentActivity activity)
        {
            if (_running.TryRemove(activity.Id, out _))
                Changed?.Invoke(activity.ProjectPath);
        }

        private sealed class Turn : IDisposable
        {
            private readonly AgentActivityBoard _board;
            private readonly AgentActivity _activity;

            public Turn(AgentActivityBoard board, AgentActivity activity)
            {
                _board = board;
                _activity = activity;
            }

            public void Dispose() => _board.End(_activity);
        }
    }
}
