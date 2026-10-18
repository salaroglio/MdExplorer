using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Tells every open window who is working, each time an agent's turn starts or ends
    /// (<see cref="IAgentActivityBoard"/>): the <c>agentActivityChanged</c> event carries the project and the
    /// whole list of its running turns, so that a window never has to add and subtract by itself.
    /// </summary>
    public sealed class AgentActivityBroadcaster : IHostedService
    {
        public const string ActivityEvent = "agentActivityChanged";

        private readonly IAgentActivityBoard _board;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ILogger<AgentActivityBroadcaster> _logger;

        public AgentActivityBroadcaster(IAgentActivityBoard board, IHubContext<MonitorMDHub> hubContext,
            ILogger<AgentActivityBroadcaster> logger)
        {
            _board = board;
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _board.Changed += OnChanged;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _board.Changed -= OnChanged;
            return Task.CompletedTask;
        }

        /// <summary>What a window receives and what the endpoint answers: the same shape.</summary>
        public static object Payload(IAgentActivityBoard board, string projectPath) => new
        {
            projectPath,
            running = board.Running(projectPath).Select(a => new
            {
                id = a.Id,
                agentName = a.AgentName,
                engine = a.Engine,
                startedAt = a.StartedAtUtc,
            }).ToList(),
        };

        private void OnChanged(string projectPath)
        {
            // The board is told from inside an agent's turn: the turn does not wait for the windows.
            _ = Task.Run(async () =>
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync(ActivityEvent, Payload(_board, projectPath));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[AgentActivity] SignalR send failed");
                }
            });
        }
    }
}
