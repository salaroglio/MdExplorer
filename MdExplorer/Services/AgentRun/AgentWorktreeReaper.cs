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
        /// <summary>Dove stavano i worktree prima del 02/08/2026. Resta solo per svuotarla.</summary>
        private static string LegacyWorktreesRoot => Path.Combine(CrossPlatformPath.GetMdExplorerDataDirectory(), "worktrees");

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentWorktreeManager _worktree;
        private readonly IAgentWorktreePreference _preference;
        private readonly ILogger<AgentWorktreeReaper> _logger;

        public AgentWorktreeReaper(
            IServiceScopeFactory scopeFactory,
            IAgentWorktreeManager worktree,
            IAgentWorktreePreference preference,
            ILogger<AgentWorktreeReaper> logger)
        {
            _scopeFactory = scopeFactory;
            _worktree = worktree;
            _preference = preference;
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
            List<Project> projects;
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                projects = db.GetDal<Project>().GetList().ToList();
                db.Commit();
            }

            // 1) I worktree stavano in AppData: dal 02/08/2026 vivono dentro il progetto. Quelli
            // vecchi non li usa più nessuno — nessun percorso del codice li nomina — ma pesano
            // quanto una copia intera della documentazione a testa. Li tolgo una volta sola.
            if (Directory.Exists(LegacyWorktreesRoot))
            {
                _logger.LogInformation(
                    "[WorktreeReaper] rimuovo i worktree della vecchia posizione ({Dir}): ora stanno dentro i progetti.",
                    LegacyWorktreesRoot);
                try { Directory.Delete(LegacyWorktreesRoot, recursive: true); }
                catch (Exception ex) { _logger.LogWarning(ex, "[WorktreeReaper] eliminazione di {Dir} fallita.", LegacyWorktreesRoot); }

                // I repo hanno ancora, in .git/worktrees, i riferimenti a cartelle che non ci sono
                // più: senza prune, git rifiuta di ricreare un worktree con lo stesso nome.
                foreach (var project in projects)
                {
                    try { await _worktree.PruneWorktreesAsync(project.Path, ct); }
                    catch (Exception ex) { _logger.LogWarning(ex, "[WorktreeReaper] prune di '{Project}' fallito.", project.Path); }
                }
            }

            // 2) Per ogni progetto vivo: branch fusi e posti eccedenti.
            foreach (var project in projects)
            {
                ct.ThrowIfCancellationRequested();

                // Fase 7d.4 — cleanup per raggiungibilità: i branch agent/* già fusi nel default
                // possono sparire (solo main persiste, §7ter). Locale-only: la cancellazione dei
                // ref remoti è outward-facing e la lascia al flusso di merge (7g). Non-git → lista vuota.
                try
                {
                    foreach (var branch in await _worktree.ListMergedAgentBranchesAsync(project.Path, null, ct))
                    {
                        _logger.LogInformation("[WorktreeReaper] branch '{Branch}' fuso in main → cancello (raggiungibile da main).", branch);
                        await _worktree.DeleteBranchAsync(project.Path, branch, remoteToo: false, ct);
                    }
                }
                catch (Exception ex) { _logger.LogWarning(ex, "[WorktreeReaper] cleanup branch fusi per '{Project}' fallito.", project.Path); }

                var root = _worktree.WorktreeRootForProject(project.Path);
                if (!Directory.Exists(root)) continue;

                // Con il pool, un agente che sparisce dal catalogo non lascia una cartella
                // orfana: il suo posto viene semplicemente riciclato dal prossimo. Quello che
                // invece resta indietro sono i posti eccedenti, quando qualcuno abbassa il
                // numero nelle impostazioni: continuerebbero a occupare una copia intera del
                // progetto senza che nessuno li usi mai più.
                IReadOnlyList<WorktreeSlot> slots;
                try { slots = await _worktree.ListSlotsAsync(project.Path, ct); }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[WorktreeReaper] posti non leggibili per '{Project}': salto la potatura.", project.Path);
                    continue;
                }

                var limit = _preference.SlotsFor(project.Path);
                foreach (var slot in slots.Where(x => x.Index > limit && !x.Held))
                {
                    _logger.LogInformation(
                        "[WorktreeReaper] posto {Index} oltre il limite di {Limit} per '{Project}': lo rimuovo.",
                        slot.Index, limit, project.Path);
                    try { await _worktree.RemoveSlotAsync(project.Path, slot.Index, ct); }
                    catch (Exception ex) { _logger.LogWarning(ex, "[WorktreeReaper] rimozione del posto {Index} fallita.", slot.Index); }
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
