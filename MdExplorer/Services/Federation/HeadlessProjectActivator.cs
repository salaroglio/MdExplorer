using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.DatabaseManager;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Federation
{
    /// <summary>
    /// Attiva un progetto <b>senza client</b> per quanto serve alla federazione (§12.7 regola 2:
    /// la città vive col Service, non con la UI). Il gate federato deve poter <b>scoprire e
    /// svegliare</b> gli agenti di un progetto anche se nessuno lo ha aperto in questa sessione:
    /// la scoperta (<see cref="AgentRegistryService"/>) legge i <c>.agent.md</c> dall'<b>indice
    /// Engine DB</b>, quindi qui li indicizziamo (idempotente) e scaldiamo il registry. NON
    /// concede trust (resta all'umano) né avvia il FileSystemWatcher (non serve al risveglio).
    /// </summary>
    public interface IHeadlessProjectActivator
    {
        /// <summary>Indicizza gli <c>.agent.md</c> del progetto (se mancanti) e riconcilia il registry.</summary>
        void ActivateForFederation(string projectPath);
    }

    public class HeadlessProjectActivator : IHeadlessProjectActivator
    {
        // Cartelle da saltare nella scansione degli agenti (rumore / working dir).
        private static readonly HashSet<string> SkipDirs = new(StringComparer.OrdinalIgnoreCase)
        {
            ".git", "node_modules", ".md", ".mdMetadata", "bin", "obj", ".vs",
        };

        private readonly IDatabaseManager _databaseManager;
        private readonly IAgentRegistryService _registry;
        private readonly ILogger<HeadlessProjectActivator> _logger;

        public HeadlessProjectActivator(
            IDatabaseManager databaseManager,
            IAgentRegistryService registry,
            ILogger<HeadlessProjectActivator> logger)
        {
            _databaseManager = databaseManager;
            _registry = registry;
            _logger = logger;
        }

        public void ActivateForFederation(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return;

            try
            {
                var indexed = EnsureAgentsIndexed(projectPath);
                // Riconcilia il registry (scopre i nuovi, mantiene trust/hash degli esistenti).
                _registry.RefreshCatalog(projectPath);
                if (indexed > 0)
                    _logger.LogInformation("[Federation] attivazione headless: {N} .agent.md indicizzati per '{Project}'", indexed, projectPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Federation] attivazione headless fallita per '{Project}'", projectPath);
            }
        }

        /// <summary>Inserisce nell'Engine DB le righe MarkdownFile dei .agent.md ancora mancanti. Ritorna quanti aggiunti.</summary>
        private int EnsureAgentsIndexed(string projectPath)
        {
            using var engine = _databaseManager.CreateIsolatedEngineDBForProjectPath(projectPath);
            engine.BeginTransaction();
            var dal = engine.GetDal<MarkdownFile>();
            var existing = dal.GetList().Select(m => m.Path).ToList();
            var existingSet = new HashSet<string>(existing.Where(p => p != null), StringComparer.OrdinalIgnoreCase);

            var added = 0;
            foreach (var file in EnumerateAgentFiles(projectPath))
            {
                if (existingSet.Contains(file)) continue;
                dal.Save(new MarkdownFile
                {
                    FileName = Path.GetFileName(file),
                    Path = file,
                    FileType = ".md",
                });
                added++;
            }
            engine.Commit();
            return added;
        }

        /// <summary>Cammina il progetto (saltando le cartelle di rumore) raccogliendo i <c>*.agent.md</c>.</summary>
        private static IEnumerable<string> EnumerateAgentFiles(string root)
        {
            var stack = new Stack<string>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                string[] entries;
                try { entries = Directory.GetFiles(dir); }
                catch { continue; }
                foreach (var f in entries)
                    if (f.EndsWith(".agent.md", StringComparison.OrdinalIgnoreCase))
                        yield return f;

                string[] subDirs;
                try { subDirs = Directory.GetDirectories(dir); }
                catch { continue; }
                foreach (var sub in subDirs)
                    if (!SkipDirs.Contains(Path.GetFileName(sub)))
                        stack.Push(sub);
            }
        }
    }
}
