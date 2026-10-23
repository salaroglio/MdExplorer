using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>
    /// Le regole che MarkAgent ha proposto e che aspettano il «Sì» dell'utente nel wizard «Migliora il grafo» (D42). In
    /// memoria: una proposta non approvata non va su disco né su Git; se il servizio si riavvia, Mark la ripropone. A ogni
    /// cambio parte a tutte le finestre l'evento <c>codeRuleProposals</c> {projectPath, count}: il tab del progetto aperto
    /// accende il pulsante.
    /// </summary>
    public sealed class CodeRulePending
    {
        private const string Event = "codeRuleProposals";

        private readonly ConcurrentDictionary<string, List<CodeRuleProposal>> _byProject = new(StringComparer.OrdinalIgnoreCase);
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly ILogger<CodeRulePending> _logger;

        public CodeRulePending(IHubContext<MonitorMDHub> hub, ILogger<CodeRulePending> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        private static string Key(string projectPath) => Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        /// <summary>Una proposta per la stessa regola sostituisce la precedente: Mark l'ha corretta.</summary>
        public CodeRuleProposal Add(CodeRuleProposal proposal)
        {
            proposal.Id = Guid.NewGuid().ToString("N");
            var list = _byProject.GetOrAdd(Key(proposal.ProjectPath), _ => new List<CodeRuleProposal>());
            int count;
            lock (list)
            {
                list.RemoveAll(p => p.Rule.Id == proposal.Rule.Id);
                list.Add(proposal);
                count = list.Count;
            }
            _logger.LogInformation("[CodeRules] '{Project}': proposta la regola '{Rule}' ({Count} in attesa)", proposal.ProjectPath, proposal.Rule.Id, count);
            Notify(proposal.ProjectPath, count);
            return proposal;
        }

        public List<CodeRuleProposal> List(string projectPath)
        {
            if (!_byProject.TryGetValue(Key(projectPath), out var list)) return new List<CodeRuleProposal>();
            lock (list) return list.ToList();
        }

        public CodeRuleProposal Get(string projectPath, string proposalId) => List(projectPath).FirstOrDefault(p => p.Id == proposalId);

        public bool Remove(string projectPath, string proposalId)
        {
            if (!_byProject.TryGetValue(Key(projectPath), out var list)) return false;
            int removed, count;
            lock (list) { removed = list.RemoveAll(p => p.Id == proposalId); count = list.Count; }
            if (removed > 0) Notify(projectPath, count);
            return removed > 0;
        }

        private void Notify(string projectPath, int count) => _ = _hub.Clients.All.SendAsync(Event, new { projectPath, count });
    }
}
