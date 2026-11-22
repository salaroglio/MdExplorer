using System;
using System.Collections.Concurrent;
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
    /// Dice alle finestre aperte che il diario di un turno è cambiato (<see cref="IAgentTurnDiary"/>): l'evento
    /// <c>agentTurnDiaryChanged</c> porta solo l'identificativo del turno, il progetto e la versione. Il testo lo
    /// chiede chi ha il pannello aperto (<c>GET api/A2A/activity/diary/{id}?after=…</c>): un turno che scrive molto non
    /// manda il suo testo a tutte le finestre. Al più un avviso ogni <see cref="Interval"/> per turno.
    /// </summary>
    public sealed class AgentTurnDiaryBroadcaster : IHostedService
    {
        public const string DiaryEvent = "agentTurnDiaryChanged";
        public static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(300);

        private readonly IAgentTurnDiary _diary;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ILogger<AgentTurnDiaryBroadcaster> _logger;
        // Un turno con un avviso già in partenza: gli altri cambiamenti di quell'intervallo viaggiano con lui.
        private readonly ConcurrentDictionary<Guid, string> _pending = new();

        public AgentTurnDiaryBroadcaster(IAgentTurnDiary diary, IHubContext<MonitorMDHub> hubContext, ILogger<AgentTurnDiaryBroadcaster> logger)
        {
            _diary = diary;
            _hubContext = hubContext;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _diary.Changed += OnChanged;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _diary.Changed -= OnChanged;
            return Task.CompletedTask;
        }

        private void OnChanged(Guid id, string projectPath)
        {
            if (!_pending.TryAdd(id, projectPath)) return;
            // Il diario viene scritto da dentro il turno: il turno non aspetta le finestre.
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Interval);
                    _pending.TryRemove(id, out _);
                    var view = _diary.Read(id, long.MaxValue);   // solo la testata: nessuna voce è più nuova di così
                    if (view == null) return;
                    await _hubContext.Clients.All.SendAsync(DiaryEvent, new
                    {
                        id,
                        projectPath,
                        version = view.Version,
                        finished = view.FinishedAtUtc != null,
                    });
                }
                catch (Exception ex)
                {
                    _pending.TryRemove(id, out _);
                    _logger.LogWarning(ex, "[AgentTurnDiary] SignalR send failed");
                }
            });
        }
    }
}
