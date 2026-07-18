using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Utilities;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Il potatore dei worktree orfani (Fase 7c.5). Un worktree per-agente vive finché l'agente
    /// esiste; quando l'agente è <b>rimosso</b> (non più nel catalogo del progetto) o l'intero
    /// progetto è cancellato, il suo worktree va rimosso — altrimenti la cartella
    /// <c>{AppData}/MdExplorer/worktrees</c> cresce all'infinito. Sweep periodico + una passata
    /// all'avvio (dopo un breve ritardo). Conservativo: pota solo ciò che è chiaramente orfano
    /// (agente assente dal catalogo, o hash di progetto inesistente), mai un agente vivo.
    /// </summary>
    public sealed class AgentWorktreeReaper : BackgroundService
    {
        private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(60);
        private static string WorktreesRoot => Path.Combine(CrossPlatformPath.GetMdExplorerDataDirectory(), "worktrees");

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentWorktreeManager _worktree;
        private readonly ILogger<AgentWorktreeReaper> _logger;

        public AgentWorktreeReaper(
            IServiceScopeFactory scopeFactory,
            IAgentWorktreeManager worktree,
            ILogger<AgentWorktreeReaper> logger)
        {
            _scopeFactory = scopeFactory;
            _worktree = worktree;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try { await Task.Delay(StartupDelay, stoppingToken); }
            catch (OperationCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try { await SweepAsync(stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { _logger.LogWarning(ex, "[WorktreeReaper] sweep fallito (best-effort)."); }

                try { await Task.Delay(SweepInterval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task SweepAsync(CancellationToken ct)
        {
            // Niente root worktrees → niente da potare (caso normale finché la feature non è usata).
            if (!Directory.Exists(WorktreesRoot))
                return;

            List<Project> projects;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                projects = db.GetDal<Project>().GetList().ToList();
                db.Commit();
            }

            // 1) Cartelle-hash di progetti che non esistono più → rimozione diretta dell'intera dir.
            var liveHashes = new HashSet<string>(projects.Select(p => Helper.HGetHashString(p.Path)), StringComparer.OrdinalIgnoreCase);
            foreach (var hashDir in SafeSubdirs(WorktreesRoot))
            {
                if (liveHashes.Contains(Path.GetFileName(hashDir))) continue;
                _logger.LogInformation("[WorktreeReaper] progetto sparito → rimuovo worktree orfani {Dir}", hashDir);
                try { Directory.Delete(hashDir, recursive: true); }
                catch (Exception ex) { _logger.LogWarning(ex, "[WorktreeReaper] eliminazione {Dir} fallita.", hashDir); }
            }

            // 2) Per ogni progetto vivo con dei worktree: pota gli agenti non più nel catalogo.
            using var scope2 = _scopeFactory.CreateScope();
            var registry = scope2.ServiceProvider.GetRequiredService<IAgentRegistryService>();
            foreach (var project in projects)
            {
                ct.ThrowIfCancellationRequested();
                var root = _worktree.WorktreeRootForProject(project.Path);
                if (!Directory.Exists(root)) continue;

                HashSet<string> liveAgents;
                try
                {
                    // Solo agenti realmente persistiti (con identità): un catalogo vuoto per errore
                    // NON deve far potare tutto → se la lettura fallisce si salta il progetto.
                    liveAgents = new HashSet<string>(
                        registry.RefreshCatalog(project.Path).Where(e => e.IdentityId != null).Select(e => e.Name),
                        StringComparer.OrdinalIgnoreCase);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[WorktreeReaper] catalogo non leggibile per '{Project}': salto la potatura.", project.Path);
                    continue;
                }

                foreach (var agentDir in SafeSubdirs(root))
                {
                    var agentName = Path.GetFileName(agentDir);
                    if (liveAgents.Contains(agentName)) continue;
                    _logger.LogInformation("[WorktreeReaper] agente '{Agent}' non più nel catalogo di '{Project}': rimuovo il worktree.", agentName, project.Path);
                    try { await _worktree.RemoveWorktreeAsync(project.Path, agentName, ct); }
                    catch (Exception ex) { _logger.LogWarning(ex, "[WorktreeReaper] rimozione worktree '{Agent}' fallita.", agentName); }
                }
            }
        }

        private static IEnumerable<string> SafeSubdirs(string dir)
        {
            try { return Directory.EnumerateDirectories(dir); }
            catch { return Enumerable.Empty<string>(); }
        }
    }
}
