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
        /// <param name="runId">Il turno di lavoro che ha prodotto l'artefatto. null = non è un turno a pubblicare
        /// (una persona che corregge): la richiesta resta legata al turno che l'ha aperta.</param>
        AgentMergeRequest Open(string projectPath, string agentName, string publishedBranch,
                               string localBranch, string headSha, IEnumerable<ChangedFile> changed, string runId = null,
                               string triggerMessageId = null);

        /// <summary>Richieste ancora da decidere, più recenti prima.</summary>
        IReadOnlyList<AgentMergeRequest> Pending(string projectPath);

        /// <summary>
        /// Il ramo <b>pubblicato</b> su cui è aperta la richiesta di approvazione di questo ramo locale d'attività
        /// (null = nessuna richiesta in attesa). Il nome locale contiene l'identificativo dell'attività: è unico.
        /// </summary>
        string PublishedBranchOf(string localBranch);

        /// <summary>
        /// Pubblica la testa della copia di un agente dove i colleghi e l'approvazione la cercano: sul ramo della
        /// richiesta in attesa, che viene aggiornata; senza richiesta, come fa l'agente a fine lavoro, aprendone
        /// una. È l'UNICO modo di pubblicare una copia: il nome locale del ramo su origin non esiste, e spingerlo
        /// così com'è creerebbe un secondo ramo che nessuna richiesta guarda. Restituisce null, o il motivo.
        /// </summary>
        Task<string> PublishCopyAsync(string projectPath, string agentName, string deskPath, string localBranch, CancellationToken ct = default);

        AgentMergeRequest Get(Guid id);

        /// <summary>File toccati di una richiesta, decodificati.</summary>
        IReadOnlyList<ChangedFile> FilesOf(AgentMergeRequest request);

        /// <summary>Autorizza e fonde. L'esito del merge determina lo stato finale.</summary>
        Task<AgentMergeRequest> ApproveAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Rifiuta. <b>Non distrugge nulla</b>: il branch resta e il lavoro è ancora lì — da qui
        /// la strada naturale è aprire il worktree e metterci mano.
        /// </summary>
        /// <summary>
        /// Rifiuta, con il motivo (obbligatorio). Rifiutare <b>ferma</b>: niente riparte da solo. Se il lavoro
        /// l'aveva chiesto un altro agente resta «fermo» finché la persona non lo fa ripartire (<see cref="Rework"/>);
        /// se l'aveva chiesto la persona è un ramo chiuso.
        /// </summary>
        AgentMergeRequest Reject(Guid id, string note);

        /// <summary>I lavori rifiutati che qualcuno aspetta e che nessuno sta rifacendo: fermi, in attesa della persona.</summary>
        IReadOnlyList<AgentMergeRequest> Stopped(string projectPath);

        /// <summary>
        /// «Fai ripartire»: l'incarico che aveva prodotto il lavoro rifiutato torna in coda, con il motivo del
        /// rifiuto. È un gesto della persona, che nel frattempo può aver corretto la scheda dell'agente.
        /// </summary>
        AgentMergeRequest Rework(Guid id);

        /// <summary>Il lavoro di questa richiesta l'aveva chiesto un altro agente: qualcuno lo aspetta.</summary>
        bool SomeoneIsWaitingFor(AgentMergeRequest request);
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
                                      string localBranch, string headSha, IEnumerable<ChangedFile> changed, string runId = null,
                                      string triggerMessageId = null)
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
                if (!string.IsNullOrWhiteSpace(runId)) request.RunId = runId;
                if (!string.IsNullOrWhiteSpace(runId)) request.TriggerMessageId = triggerMessageId;   // il turno nuovo dice chi l'ha chiesto

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

        public string PublishedBranchOf(string localBranch)
        {
            if (string.IsNullOrWhiteSpace(localBranch)) return null;
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var found = db.GetDal<AgentMergeRequest>().GetList().ToList()
                .Where(r => r.Status == AgentMergeRequest.StatusEnum.Pending
                            && string.Equals(r.LocalBranch, localBranch, StringComparison.Ordinal))
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault()?.PublishedBranch;
            db.Commit();
            return found;
        }

        public async Task<string> PublishCopyAsync(
            string projectPath, string agentName, string deskPath, string localBranch, CancellationToken ct = default)
        {
            var published = PublishedBranchOf(localBranch);
            if (published != null)
            {
                var (head, error) = await _worktree.PublishToAsync(deskPath, published, ct);
                if (error != null) return $"'{published}': {error}";
                Open(projectPath, agentName, published, localBranch, head, await _worktree.ChangedFilesAsync(projectPath, agentName, ct));
                return null;
            }

            // Nessuna richiesta in attesa (già decisa, o mai nata). La consegna dell'agente committerebbe a suo
            // nome ciò che trova non committato: qui pubblica una persona, quindi prima deve aver committato lei.
            if ((await _worktree.UncommittedAsync(deskPath, ct)).Count > 0)
                return "ci sono modifiche non committate: committale prima di pubblicare";
            var attempt = await _worktree.TryCommitAndPushBranchAsync(projectPath, agentName, null, ct);
            if (attempt?.Pushed == null) return attempt?.Error ?? "non c'è niente da pubblicare";
            Open(projectPath, agentName, attempt.Pushed.Branch, attempt.Pushed.LocalBranch, attempt.Pushed.HeadSha,
                await _worktree.ChangedFilesAsync(projectPath, agentName, ct));
            return null;
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
        {
            if (string.IsNullOrWhiteSpace(note))
                throw new InvalidOperationException("Per rifiutare serve il motivo: è ciò che l'agente legge se il lavoro riparte.");
            // Rifiutare FERMA. Niente riparte da solo: se la risposta è sballata la causa è spesso nella scheda
            // dell'agente, e ripartire subito rifarebbe lo stesso errore prima che la persona possa correggerla.
            return Decide(id, AgentMergeRequest.StatusEnum.Rejected, note.Trim());
        }

        public bool SomeoneIsWaitingFor(AgentMergeRequest request) => WaitingTrigger(request) != null;

        public IReadOnlyList<AgentMergeRequest> Stopped(string projectPath)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var rejected = db.GetDal<AgentMergeRequest>().GetList().ToList()
                .Where(r => r.Status == AgentMergeRequest.StatusEnum.Rejected
                            && !string.IsNullOrEmpty(r.TriggerMessageId)
                            && AgentPathComparer.Equals(r.ProjectPath, projectPath))
                .ToList();
            var messages = db.GetDal<AgentMessage>();
            var stopped = new List<AgentMergeRequest>();
            foreach (var r in rejected)
            {
                if (!Guid.TryParse(r.TriggerMessageId, out var triggerId)) continue;
                var trigger = messages.GetList().FirstOrDefault(m => m.Id == triggerId);
                // Fermo = l'incarico è concluso (nessuno lo sta rifacendo) e l'aveva chiesto un altro agente.
                if (trigger != null && trigger.State == AgentMessage.StateEnum.Processed && !FromThePerson(trigger))
                    stopped.Add(r);
            }
            db.Commit();
            return stopped.OrderByDescending(r => r.DecidedAt ?? r.CreatedAt).ToList();
        }

        public AgentMergeRequest Rework(Guid id)
        {
            var request = Get(id) ?? throw new InvalidOperationException($"Richiesta {id} inesistente.");
            if (request.Status != AgentMergeRequest.StatusEnum.Rejected)
                throw new InvalidOperationException("Si fa ripartire solo un lavoro rifiutato.");
            if (WaitingTrigger(request) == null)
                throw new InvalidOperationException(
                    "Questo lavoro non l'aveva chiesto un altro agente: non c'è un incarico da rimettere in coda. Per riprovare, rilancia l'agente.");
            var triggerId = Guid.Parse(request.TriggerMessageId);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            try
            {
                var dal = db.GetDal<AgentMessage>();
                var m = dal.GetList().FirstOrDefault(x => x.Id == triggerId)
                    ?? throw new InvalidOperationException("L'incarico originale non c'è più: non posso rimetterlo in coda.");
                if (m.State != AgentMessage.StateEnum.Processed)
                    throw new InvalidOperationException("L'incarico è già in coda o in lavorazione: il lavoro sta ripartendo.");
                m.State = AgentMessage.StateEnum.Pending;
                m.ProcessedAt = null;
                m.NextAttemptAt = null;
                // Tentativi azzerati: non è un ritentativo dopo un errore, è lo stesso lavoro chiesto di nuovo.
                m.Attempts = 0;
                m.Error = null;
                m.ForcedAt = null;
                m.ReworkNote = request.Note;
                dal.Save(m);
                db.Commit();
            }
            catch
            {
                db.Rollback();
                throw;
            }
            _logger.LogInformation("[Merge] lavoro di '{Agent}' fatto ripartire dalla persona: l'incarico {Message} torna in coda con il motivo del rifiuto.",
                request.AgentName, triggerId);
            return request;
        }

        private static bool FromThePerson(AgentMessage m)
            => string.Equals(m.FromAgent, MdExplorer.Features.Agents.ConversationHopGuard.UserRecipient, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Il messaggio che ha fatto partire il turno, se viene da un <b>altro agente</b>: allora qualcuno aspetta
        /// questo artefatto. Un lavoro lanciato dalla persona (a mano, o rispondendo a un messaggio) non ha
        /// nessuno in attesa: rifiutato, è un ramo chiuso.
        /// </summary>
        private AgentMessage WaitingTrigger(AgentMergeRequest request)
        {
            if (request == null || !Guid.TryParse(request.TriggerMessageId, out var triggerId)) return null;
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            var trigger = db.GetDal<AgentMessage>().GetList().FirstOrDefault(m => m.Id == triggerId);
            db.Commit();
            return trigger != null && !FromThePerson(trigger) ? trigger : null;
        }

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
