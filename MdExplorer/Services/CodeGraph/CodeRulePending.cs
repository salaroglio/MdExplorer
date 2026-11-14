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
    /// Le regole e i linguaggi (D42 passo 2) che MarkAgent ha proposto e che aspettano il «Sì» dell'utente nel wizard
    /// «Migliora il grafo» (D42). In memoria: una proposta non approvata non va su disco né su Git; se il servizio si
    /// riavvia, Mark la ripropone. A ogni cambio parte a tutte le finestre l'evento <c>codeRuleProposals</c>
    /// {projectPath, count} (regole + linguaggi): il tab del progetto aperto accende la striscia.
    /// </summary>
    /// <summary>Programmi che MarkAgent propone di analizzare (F5): i semi e il perché, in parole per l'utente.</summary>
    public sealed class CodeSeedProposal
    {
        public string Id { get; set; }
        public string ProjectPath { get; set; }
        public List<string> Seeds { get; set; } = new();
        public string Reason { get; set; }
        public string ProposedBy { get; set; }
        public string ProposedAt { get; set; }
    }

    public sealed class CodeRulePending
    {
        private const string Event = "codeRuleProposals";

        private readonly ConcurrentDictionary<string, List<CodeRuleProposal>> _byProject = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, List<CodeLanguageProposal>> _languages = new(StringComparer.OrdinalIgnoreCase);
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
            lock (list)
            {
                list.RemoveAll(p => p.Rule.Id == proposal.Rule.Id);
                list.Add(proposal);
            }
            var count = Count(proposal.ProjectPath);
            _logger.LogInformation("[CodeRules] '{Project}': proposta la regola '{Rule}' ({Count} in attesa)", proposal.ProjectPath, proposal.Rule.Id, count);
            Notify(proposal.ProjectPath, count);
            return proposal;
        }

        /// <summary>Una proposta per lo stesso linguaggio sostituisce la precedente: Mark l'ha corretto.</summary>
        public CodeLanguageProposal AddLanguage(CodeLanguageProposal proposal)
        {
            proposal.Id = Guid.NewGuid().ToString("N");
            var list = _languages.GetOrAdd(Key(proposal.ProjectPath), _ => new List<CodeLanguageProposal>());
            lock (list)
            {
                list.RemoveAll(p => p.Preview.LanguageId == proposal.Preview.LanguageId);
                list.Add(proposal);
            }
            var count = Count(proposal.ProjectPath);
            _logger.LogInformation("[CodeRules] '{Project}': proposto il linguaggio '{Language}' ({Count} in attesa)", proposal.ProjectPath, proposal.Preview.LanguageId, count);
            Notify(proposal.ProjectPath, count);
            return proposal;
        }

        public List<CodeLanguageProposal> Languages(string projectPath)
        {
            if (!_languages.TryGetValue(Key(projectPath), out var list)) return new List<CodeLanguageProposal>();
            lock (list) return list.ToList();
        }

        public CodeLanguageProposal GetLanguage(string projectPath, string proposalId) => Languages(projectPath).FirstOrDefault(p => p.Id == proposalId);

        public bool RemoveLanguage(string projectPath, string proposalId)
        {
            if (!_languages.TryGetValue(Key(projectPath), out var list)) return false;
            int removed;
            lock (list) removed = list.RemoveAll(p => p.Id == proposalId);
            if (removed > 0) Notify(projectPath, Count(projectPath));
            return removed > 0;
        }

        /// <summary>Quante proposte aspettano nel progetto: regole, linguaggi e programmi da analizzare (F5).</summary>
        public int Count(string projectPath) => List(projectPath).Count + Languages(projectPath).Count + Seeds(projectPath).Count;

        // ---- F5 (11/11/2026): i programmi che MarkAgent propone di analizzare (code_analysis_propose)

        private readonly ConcurrentDictionary<string, List<CodeSeedProposal>> _seeds = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// MarkAgent propone dei semi (dopo «che cosa ti sarebbe servito nel grafo?»): restano qui finché l'utente non li
        /// analizza o li scarta; li lancia sempre l'utente, dalla finestra «Analizza questo programma». Un seme già proposto non
        /// si ripete: vince la proposta nuova, col suo motivo.
        /// </summary>
        public CodeSeedProposal AddSeeds(CodeSeedProposal proposal)
        {
            proposal.Id = Guid.NewGuid().ToString("N");
            var list = _seeds.GetOrAdd(Key(proposal.ProjectPath), _ => new List<CodeSeedProposal>());
            lock (list)
            {
                foreach (var old in list) old.Seeds.RemoveAll(s => proposal.Seeds.Contains(s));
                list.RemoveAll(p => p.Seeds.Count == 0);
                list.Add(proposal);
            }
            var count = Count(proposal.ProjectPath);
            _logger.LogInformation("[CodeRules] '{Project}': proposti {Seeds} programmi da analizzare ({Count} in attesa)", proposal.ProjectPath, proposal.Seeds.Count, count);
            Notify(proposal.ProjectPath, count);
            return proposal;
        }

        public List<CodeSeedProposal> Seeds(string projectPath)
        {
            if (!_seeds.TryGetValue(Key(projectPath), out var list)) return new List<CodeSeedProposal>();
            lock (list) return list.Select(p => new CodeSeedProposal { Id = p.Id, ProjectPath = p.ProjectPath, Seeds = p.Seeds.ToList(), Reason = p.Reason, ProposedBy = p.ProposedBy, ProposedAt = p.ProposedAt }).ToList();
        }

        /// <summary>Un seme proposto è stato aperto (analizzato o no): esce dalla proposta; una proposta vuota se ne va.</summary>
        public bool RemoveSeed(string projectPath, string seed)
        {
            if (!_seeds.TryGetValue(Key(projectPath), out var list)) return false;
            int removed = 0;
            lock (list)
            {
                foreach (var p in list) removed += p.Seeds.RemoveAll(s => s == seed);
                list.RemoveAll(p => p.Seeds.Count == 0);
            }
            if (removed > 0) Notify(projectPath, Count(projectPath));
            return removed > 0;
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
            int removed;
            lock (list) removed = list.RemoveAll(p => p.Id == proposalId);
            if (removed > 0) Notify(projectPath, Count(projectPath));
            return removed > 0;
        }

        private void Notify(string projectPath, int count) => _ = _hub.Clients.All.SendAsync(Event, new { projectPath, count });
    }
}
