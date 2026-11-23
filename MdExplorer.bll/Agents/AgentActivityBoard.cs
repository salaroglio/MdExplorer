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
        /// <summary>The launch this turn belongs to, when it has one: the same id written on the merge request it may deliver.</summary>
        public string RunId { get; set; }
    }

    /// <summary>
    /// Who is working right now: one entry per agent's turn in progress, whatever started it (a person's
    /// launch, a schedule, a colleague's message). It exists so that the application can show it: until a
    /// turn ends nothing else tells the person that their gesture started something.
    /// </summary>
    public interface IAgentActivityBoard
    {
        /// <summary>Records a turn as running; disposing what it returns records its end, however it ended.</summary>
        AgentTurnInProgress Begin(string agentName, string projectPath, string engine, string runId = null);

        /// <summary>The turns running in a project, the oldest first.</summary>
        IReadOnlyList<AgentActivity> Running(string projectPath);

        /// <summary>
        /// Chiede di fermare un turno in corso (il pulsante «Ferma» del diario). true se il turno c'era: da lì lo ferma
        /// chi lo fa girare, che ascolta <see cref="AgentTurnInProgress.StopRequested"/>.
        /// </summary>
        bool Stop(Guid id);

        /// <summary>Raised with the project's path every time a turn of that project starts or ends.</summary>
        event Action<string> Changed;
    }

    public sealed class AgentActivityBoard : IAgentActivityBoard
    {
        private readonly ConcurrentDictionary<Guid, AgentActivity> _running = new();
        private readonly ConcurrentDictionary<Guid, AgentTurnInProgress> _turns = new();

        public event Action<string> Changed;

        public AgentTurnInProgress Begin(string agentName, string projectPath, string engine, string runId = null)
        {
            var activity = new AgentActivity
            {
                Id = Guid.NewGuid(),
                AgentName = agentName,
                ProjectPath = projectPath,
                Engine = engine,
                StartedAtUtc = DateTime.UtcNow,
                RunId = runId,
            };
            _running[activity.Id] = activity;
            Changed?.Invoke(projectPath);
            var turn = new AgentTurnInProgress(activity, () => End(activity));
            _turns[activity.Id] = turn;
            return turn;
        }

        public IReadOnlyList<AgentActivity> Running(string projectPath)
            => _running.Values
                .Where(a => AgentPathComparer.Equals(a.ProjectPath, projectPath))
                .OrderBy(a => a.StartedAtUtc)
                .ToList();

        public bool Stop(Guid id)
        {
            if (!_turns.TryGetValue(id, out var turn)) return false;
            turn.RequestStop();
            return true;
        }

        private void End(AgentActivity activity)
        {
            _turns.TryRemove(activity.Id, out _);
            if (_running.TryRemove(activity.Id, out _))
                Changed?.Invoke(activity.ProjectPath);
        }
    }

    /// <summary>Un turno in corso sulla lavagna: finisce quando lo si chiude. Porta l'attività, col suo identificativo.</summary>
    public sealed class AgentTurnInProgress : IDisposable
    {
        private Action _end;
        private readonly System.Threading.CancellationTokenSource _stop = new();

        /// <summary>Scatta quando qualcuno chiede di fermare il turno (<see cref="IAgentActivityBoard.Stop"/>).</summary>
        public System.Threading.CancellationToken StopRequested => _stop.Token;

        /// <summary>true se il turno è stato fermato a mano: chi lo fa girare lo dice nell'esito, non è un errore del motore.</summary>
        public bool Stopped => _stop.IsCancellationRequested;

        internal void RequestStop()
        {
            try { _stop.Cancel(); } catch (ObjectDisposedException) { /* il turno è finito nello stesso istante */ }
        }

        public AgentActivity Activity { get; }

        public AgentTurnInProgress(AgentActivity activity, Action end)
        {
            Activity = activity;
            _end = end;
        }

        public void Dispose()
        {
            System.Threading.Interlocked.Exchange(ref _end, null)?.Invoke();
            _stop.Dispose();
        }
    }
}
