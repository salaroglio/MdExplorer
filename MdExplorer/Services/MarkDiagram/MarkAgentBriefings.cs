using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Services.MarkDiagram
{
    /// <summary>
    /// «Ragguagli» for the MarkAgent tab (sprint 2026-09-29-Motore-LLM-Unico, D9): what was done in parallel in
    /// another session — a change to a document confirmed from «spiega il diagramma» — set aside and given to the
    /// tab's LLM at the head of the user's next message, so it knows without a turn of its own. Not a chat message:
    /// the tab shows a small «N ragguagli in attesa» sign, with the text on hover (event <c>markAgentBriefings</c>).
    /// Keyed by the page's monitor connection, which the tab's AI connection is linked to.
    /// </summary>
    public sealed class MarkAgentBriefings
    {
        /// <summary>Kept short: a briefing says what changed and where, the details are in the document.</summary>
        private const int MaxLength = 600;

        private const string Event = "markAgentBriefings";

        private readonly ConcurrentDictionary<string, List<string>> _pending = new();
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ILogger<MarkAgentBriefings> _logger;

        public MarkAgentBriefings(IHubContext<MonitorMDHub> hubContext, ILogger<MarkAgentBriefings> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void Add(string monitorConnectionId, string text)
        {
            if (string.IsNullOrWhiteSpace(monitorConnectionId) || string.IsNullOrWhiteSpace(text)) return;
            var trimmed = text.Trim();
            if (trimmed.Length > MaxLength) trimmed = trimmed.Substring(0, MaxLength) + "…";
            var list = _pending.GetOrAdd(monitorConnectionId, _ => new List<string>());
            List<string> snapshot;
            lock (list)
            {
                list.Add(trimmed);
                snapshot = list.ToList();
            }
            _logger.LogInformation("[MarkAgentBriefings] {Connection}: ragguaglio messo da parte ({Count} in attesa)", monitorConnectionId, snapshot.Count);
            Notify(monitorConnectionId, snapshot);
        }

        /// <summary>The briefings waiting for <paramref name="monitorConnectionId"/>, removed.</summary>
        public IReadOnlyList<string> TakeAll(string monitorConnectionId)
        {
            if (string.IsNullOrWhiteSpace(monitorConnectionId) || !_pending.TryRemove(monitorConnectionId, out var list))
                return Array.Empty<string>();
            List<string> taken;
            lock (list) taken = list.ToList();
            if (taken.Count > 0) Notify(monitorConnectionId, new List<string>());
            return taken;
        }

        /// <summary>The block put at the head of the tab's prompt.</summary>
        public static string Block(IReadOnlyList<string> briefings)
        {
            // "\n" and not AppendLine: the same prompt on every platform.
            var sb = new StringBuilder();
            sb.Append("[Ragguagli sulle attività fatte in parallelo in MdExplorer, dall'ultimo messaggio. Tienine conto; " +
                      "non rispondere a questi, rispondi al messaggio dell'utente che segue.]\n");
            foreach (var b in briefings) sb.Append("- ").Append(b).Append('\n');
            sb.Append("[Fine dei ragguagli]\n\n");
            return sb.ToString();
        }

        private void Notify(string monitorConnectionId, List<string> items) =>
            _ = _hubContext.Clients.Client(monitorConnectionId).SendAsync(Event, new { count = items.Count, items });
    }
}
