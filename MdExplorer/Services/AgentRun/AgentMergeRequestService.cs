using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Un file toccato dal lavoro dell'agente, come lo vedrà l'umano.</summary>
    public sealed class ChangedFile
    {
        /// <summary><c>added</c> / <c>modified</c> / <c>deleted</c> / <c>renamed</c>.</summary>
        public string Change { get; init; }
        public string Path { get; init; }
    }

    /// <summary>
    /// Le richieste di merge dei deliverable: una «pull request» interna a MDE.
    /// <para>
    /// Il gate meccanico (<see cref="IDeliverableMergeGate"/>) non è stato buttato: resta il
    /// punto dove una CI o un agente-revisore potranno pre-qualificare il lavoro. È cambiato
    /// cosa succede al suo «sì»: prima fondeva, ora <b>propone</b>.
    /// </para>
    /// </summary>
    public interface IAgentMergeRequestService
    {
        /// <summary>
        /// Registra una richiesta per un deliverable pubblicato. Idempotente sul branch: se
        /// l'agente ripubblica la stessa attività, la richiesta esistente si aggiorna invece di
        /// diventare un doppione nell'elenco dell'umano.
        /// </summary>
        AgentMergeRequest Open(string projectPath, string agentName, string publishedBranch,
                               string localBranch, string headSha, IEnumerable<ChangedFile> changed);

        /// <summary>Richieste ancora da decidere, più recenti prima.</summary>
        IReadOnlyList<AgentMergeRequest> Pending(string projectPath);

        AgentMergeRequest Get(Guid id);

        /// <summary>File toccati di una richiesta, decodificati.</summary>
        IReadOnlyList<ChangedFile> FilesOf(AgentMergeRequest request);

        /// <summary>Autorizza e fonde. L'esito del merge determina lo stato finale.</summary>
        Task<AgentMergeRequest> ApproveAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Rifiuta. <b>Non distrugge nulla</b>: il branch resta e il lavoro è ancora lì — da qui
        /// la strada naturale è aprire il worktree e metterci mano.
        /// </summary>
        AgentMergeRequest Reject(Guid id, string note);
    }

    public class AgentMergeRequestService : IAgentMergeRequestService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAgentWorktreeManager _worktree;
        private readonly ILogger<AgentMergeRequestService> _logger;

        public AgentMergeRequestService(
            IServiceScopeFactory scopeFactory,
            IAgentWorktreeManager worktree,
            ILogger<AgentMergeRequestService> logger)
        {
            _scopeFactory = scopeFactory;
            _worktree = worktree;
            _logger = logger;
        }

        public AgentMergeRequest Open(string projectPath, string agentName, string publishedBranch,
                                      string localBranch, string headSha, IEnumerable<ChangedFile> changed)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || string.IsNullOrWhiteSpace(publishedBranch))
                throw new ArgumentException("projectPath e publishedBranch sono obbligatori");

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            try
            {
                var dal = db.GetDal<AgentMergeRequest>();

                // Idempotenza sul branch pubblicato: ripubblicare la stessa attività aggiorna la
                // richiesta invece di riempire l'elenco dell'umano di doppioni.
                var existing = dal.GetList().ToList().FirstOrDefault(r =>
                    string.Equals(r.PublishedBranch, publishedBranch, StringComparison.OrdinalIgnoreCase)
                    && AgentPathComparer.Equals(r.ProjectPath, projectPath));

                var request = existing ?? new AgentMergeRequest
                {
                    ProjectPath = projectPath,
                    AgentName = agentName,
                    PublishedBranch = publishedBranch,
                    CreatedAt = DateTime.UtcNow,
                };

                request.LocalBranch = localBranch;
                request.HeadSha = headSha;
                request.ChangedFiles = Encode(changed);

                if (existing != null)
                {
                    // L'agente ha rilavorato: una richiesta già rifiutata torna in gioco, perché
                    // il contenuto NON è più quello che l'umano aveva bocciato.
                    request.Status = AgentMergeRequest.StatusEnum.Pending;
                    request.DecidedAt = null;
                    request.Note = null;
                }
                else
                {
                    request.Status = AgentMergeRequest.StatusEnum.Pending;
                }

                dal.Save(request);
                db.Commit();

                _logger.LogInformation("[Merge] richiesta {Status} per '{Agent}': {Branch}",
                    existing == null ? "aperta" : "aggiornata", agentName, publishedBranch);
                return request;
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }

        public IReadOnlyList<AgentMergeRequest> Pending(string projectPath)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var list = db.GetDal<AgentMergeRequest>().GetList().ToList()
                .Where(r => r.Status == AgentMergeRequest.StatusEnum.Pending
                            && AgentPathComparer.Equals(r.ProjectPath, projectPath))
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
            db.Commit();
            return list;
        }

        public AgentMergeRequest Get(Guid id)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var r = db.GetDal<AgentMergeRequest>().GetList().FirstOrDefault(x => x.Id == id);
            db.Commit();
            return r;
        }

        public IReadOnlyList<ChangedFile> FilesOf(AgentMergeRequest request) => Decode(request?.ChangedFiles);

        public async Task<AgentMergeRequest> ApproveAsync(Guid id, CancellationToken ct = default)
        {
            var request = Get(id)
                ?? throw new InvalidOperationException($"Richiesta di merge {id} inesistente.");

            if (request.Status != AgentMergeRequest.StatusEnum.Pending)
                throw new InvalidOperationException(
                    $"La richiesta è già stata decisa ({request.Status}): non si autorizza due volte.");

            // Il merge è un'operazione LOCALE e vuole il ref locale: il nome pubblicato vive su
            // origin e non ha un ref in casa.
            var outcome = await _worktree.MergeDeliverableIntoDefaultAsync(
                request.ProjectPath, request.AgentName, request.LocalBranch, ct);

            var merged = outcome == DeliverableMergeOutcome.Merged;
            return Decide(id,
                merged ? AgentMergeRequest.StatusEnum.Merged : AgentMergeRequest.StatusEnum.Failed,
                merged ? null
                       : outcome == DeliverableMergeOutcome.Conflict
                           ? "Il merge è in conflitto con il ramo principale: serve l'intervento manuale."
                           : "Il merge non è riuscito.");
        }

        public AgentMergeRequest Reject(Guid id, string note)
            => Decide(id, AgentMergeRequest.StatusEnum.Rejected,
                      string.IsNullOrWhiteSpace(note) ? "Rifiutata dall'umano." : note.Trim());

        private AgentMergeRequest Decide(Guid id, string status, string note)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            try
            {
                var dal = db.GetDal<AgentMergeRequest>();
                var r = dal.GetList().FirstOrDefault(x => x.Id == id)
                    ?? throw new InvalidOperationException($"Richiesta di merge {id} inesistente.");

                r.Status = status;
                r.DecidedAt = DateTime.UtcNow;
                r.Note = note;
                dal.Save(r);
                db.Commit();

                _logger.LogInformation("[Merge] richiesta di '{Agent}' → {Status} ({Branch})",
                    r.AgentName, status, r.PublishedBranch);
                return r;
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }

        // ---- codifica dei file toccati -------------------------------------
        // Una riga per file, "<stato>\t<percorso>": leggibile a occhio in DB e senza dipendenze
        // da un serializzatore per una struttura così semplice.

        private static string Encode(IEnumerable<ChangedFile> files)
            => string.Join("\n", (files ?? Enumerable.Empty<ChangedFile>())
                .Where(f => !string.IsNullOrWhiteSpace(f?.Path))
                .Select(f => $"{f.Change}\t{f.Path}"));

        private static IReadOnlyList<ChangedFile> Decode(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<ChangedFile>();
            return raw.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line =>
                {
                    var parts = line.Split('\t', 2);
                    return parts.Length == 2
                        ? new ChangedFile { Change = parts[0], Path = parts[1] }
                        : new ChangedFile { Change = "modified", Path = line };
                })
                .ToList();
        }

        /// <summary>Da <c>git diff --name-status</c> alla forma leggibile.</summary>
        public static IReadOnlyList<ChangedFile> ParseNameStatus(string diffOutput)
        {
            if (string.IsNullOrWhiteSpace(diffOutput)) return Array.Empty<ChangedFile>();

            return diffOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split('\t', StringSplitOptions.RemoveEmptyEntries))
                .Where(p => p.Length >= 2)
                .Select(p => new ChangedFile
                {
                    Change = p[0].Trim().ToUpperInvariant() switch
                    {
                        "A" => "added",
                        "D" => "deleted",
                        var s when s.StartsWith("R") => "renamed",
                        _ => "modified",
                    },
                    // Su un rename git dà due percorsi: interessa la destinazione.
                    Path = p[^1].Trim(),
                })
                .ToList();
        }
    }
}
