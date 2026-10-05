using System;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Service;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Lavorare nella copia di un agente «come dopo un cambio di ramo»: la finestra viene ripuntata sulla sua
    /// scrivania, e da lì albero, indice, ricerca, documenti e modifiche sono quelli della copia.
    /// <para>
    /// Entrare apre la <b>sessione d'intervento</b>: la scrivania resta di chi è entrato e l'agente va in coda,
    /// così nessun turno la ripulisce mentre una persona ci lavora. Uscire pretende che non resti niente di non
    /// salvato: chi esce autorizza commit e pubblicazione in un gesto solo, oppure resta dentro.
    /// </para>
    /// <para>Il progetto resta lo stesso per tutto ciò che è della città (posta, registro, fiducia): cambia solo
    /// la cartella su cui lavora <b>questa finestra</b>.</para>
    /// </summary>
    [ApiController]
    [Route("api/AgentWorkspace")]
    public class AgentWorkspaceController : ControllerBase
    {
        private readonly IAgentWorktreeManager _worktree;
        private readonly IAgentWorktreeHoldService _sessions;
        private readonly IDatabaseManager _databaseManager;
        private readonly IFileSystemWatcherManager _watchers;
        private readonly IServiceProvider _services;
        private readonly IWorkingChangesService _changes;
        private readonly IAgentMergeRequestService _requests;
        private readonly MdExplorer.Services.Git.IRepoSyncService _sync;
        private readonly ILogger<AgentWorkspaceController> _logger;

        public AgentWorkspaceController(
            IAgentWorktreeManager worktree,
            IAgentWorktreeHoldService sessions,
            IDatabaseManager databaseManager,
            IFileSystemWatcherManager watchers,
            IServiceProvider services,
            IWorkingChangesService changes,
            IAgentMergeRequestService requests,
            MdExplorer.Services.Git.IRepoSyncService sync,
            ILogger<AgentWorkspaceController> logger)
        {
            _worktree = worktree;
            _sessions = sessions;
            _databaseManager = databaseManager;
            _watchers = watchers;
            _services = services;
            _changes = changes;
            _requests = requests;
            _sync = sync;
            _logger = logger;
        }

        /// <summary>Il motivo con cui si apre la sessione: distingue «sono dentro la copia» da una revisione.</summary>
        private const string InsideReason = "lavoro nella copia dell'agente";

        /// <summary>
        /// La copia in cui si era entrati e da cui non si è ancora usciti (null = nessuna). La finestra può essere
        /// stata ricaricata: lo stato sta qui, nella sessione d'intervento, e la finestra lo riprende.
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> Current([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return Ok(new { agent = (string?)null });
            var inside = (await _worktree.ListSlotsAsync(projectPath)).FirstOrDefault(
                x => x.Held && x.Agent != null
                     && string.Equals(_sessions.ReasonFor(projectPath, x.Agent), InsideReason, StringComparison.Ordinal));
            return Ok(new { agent = inside?.Agent });
        }

        public sealed class WorkspaceRequest
        {
            public string? ProjectPath { get; set; }
            public string? AgentName { get; set; }
            public string? ConnectionId { get; set; }
            /// <summary>Chi esce ha visto cosa c'è da salvare e ha detto «committa e pubblica».</summary>
            public bool? Authorized { get; set; }
            public string? CommitMessage { get; set; }
        }

        /// <summary>Entra nella copia dell'agente: sessione d'intervento aperta, finestra ripuntata.</summary>
        [HttpPost("enter")]
        public async Task<IActionResult> Enter([FromBody] WorkspaceRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath) || string.IsNullOrWhiteSpace(body.AgentName)
                || string.IsNullOrWhiteSpace(body.ConnectionId))
                return BadRequest(new { error = "projectPath, agentName e connectionId sono obbligatori." });

            var desk = (await _worktree.ListSlotsAsync(body.ProjectPath)).FirstOrDefault(
                x => string.Equals(x.Agent, body.AgentName, StringComparison.OrdinalIgnoreCase));
            if (desk == null)
                return UnprocessableEntity(new
                {
                    error = $"'{body.AgentName}' non ha una copia di lavoro in questo momento: la sua scrivania è passata a un " +
                            "altro agente. Il suo lavoro si riapre dalla posta, con «Ci metto mano».",
                });
            if (desk.Running && !desk.Held)
                return UnprocessableEntity(new
                {
                    error = $"'{body.AgentName}' sta lavorando adesso: aspetta che finisca, poi entra nella sua copia.",
                });

            // Prima la sessione, poi la finestra: se la sessione fallisse, ci si troverebbe a lavorare in una
            // copia che un agente può ancora ripulire.
            _sessions.Open(body.ProjectPath, body.AgentName, InsideReason);

            try
            {
                ProjectsManager.PointAtWorkingCopy(_services, desk.Path, body.ProjectPath);
                _databaseManager.RegisterConnection(body.ConnectionId, desk.Path);
                _watchers.RegisterWatcher(body.ConnectionId, desk.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Workspace] ingresso nella copia di '{Agent}' fallito.", body.AgentName);
                Back(body);
                _sessions.Close(body.ProjectPath, body.AgentName, discardWork: false);
                return UnprocessableEntity(new { error = $"Non riesco ad aprire la copia di '{body.AgentName}': {ex.Message}" });
            }

            _logger.LogInformation("[Workspace] la finestra {Connection} lavora nella copia di '{Agent}': {Path}",
                body.ConnectionId, body.AgentName, desk.Path);
            return Ok(new { worktreePath = desk.Path, branch = desk.Branch, slot = desk.Index });
        }

        /// <summary>Ciò che resta da salvare in una copia: file non committati e commit non pubblicati, per repository.</summary>
        private sealed class PendingWork
        {
            public System.Collections.Generic.List<RepoChanges> Dirty { get; } = new();
            /// <summary>Submodule con commit da pubblicare (il loro «avanti» è rispetto al loro ramo remoto).</summary>
            public System.Collections.Generic.List<RepoChanges> SubmodulesAhead { get; } = new();
            /// <summary>Commit della copia non ancora sul ramo pubblicato dell'agente.</summary>
            public int RootUnpublished { get; set; }
            /// <summary>La richiesta di approvazione aperta su questo ramo, se c'è: dice su quale ramo si pubblica.</summary>
            public Abstractions.Entities.UserDB.AgentMergeRequest Request { get; set; }
            public bool Any => Dirty.Count > 0 || SubmodulesAhead.Count > 0 || RootUnpublished > 0;
        }

        private async Task<PendingWork> ReadPendingAsync(string projectPath, string agentName, WorktreeSlot desk)
        {
            var work = new PendingWork();
            var view = await _changes.GetAsync(projectPath, agentName);
            foreach (var r in view?.Repos ?? Array.Empty<RepoChanges>())
            {
                if (r.Uncommitted.Count > 0) work.Dirty.Add(r);
                if (!string.IsNullOrEmpty(r.Path) && r.Ahead > 0) work.SubmodulesAhead.Add(r);
            }
            work.Request = _requests.Pending(projectPath).FirstOrDefault(
                r => string.Equals(r.LocalBranch, desk.Branch, StringComparison.Ordinal));
            work.RootUnpublished = await _worktree.UnpublishedCommitsAsync(desk.Path, work.Request?.PublishedBranch);
            return work;
        }

        private static object Describe(PendingWork work, string agentName)
        {
            string Name(RepoChanges r) => string.IsNullOrEmpty(r.Path) ? agentName : r.Label;
            var unpublished = work.SubmodulesAhead.Select(r => new { repo = r.Path, label = Name(r), commits = r.Ahead }).ToList();
            if (work.RootUnpublished > 0) unpublished.Insert(0, new { repo = "", label = agentName, commits = work.RootUnpublished });
            return new
            {
                uncommitted = work.Dirty.Select(r => new { repo = r.Path, label = Name(r), files = r.Uncommitted.Select(f => f.Path).ToList() }).ToList(),
                unpublished,
            };
        }

        /// <summary>
        /// Cosa c'è da salvare nella copia prima di uscirne: è ciò che la finestra di uscita mostra a chi deve
        /// autorizzare. Il lavoro che l'agente ha già consegnato non conta: è già pubblicato.
        /// </summary>
        [HttpPost("pending")]
        public async Task<IActionResult> Pending([FromBody] WorkspaceRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath) || string.IsNullOrWhiteSpace(body.AgentName))
                return BadRequest(new { error = "projectPath e agentName sono obbligatori." });
            var desk = (await _worktree.ListSlotsAsync(body.ProjectPath)).FirstOrDefault(
                x => string.Equals(x.Agent, body.AgentName, StringComparison.OrdinalIgnoreCase));
            if (desk == null) return Ok(Describe(new PendingWork(), body.AgentName));
            return Ok(Describe(await ReadPendingAsync(body.ProjectPath, body.AgentName, desk), body.AgentName));
        }

        /// <summary>
        /// Torna al progetto. Se nella copia c'è qualcosa da salvare serve l'<b>autorizzazione</b> di chi esce
        /// (<c>authorized</c>, con il messaggio del commit): allora il servizio committa, pubblica sul ramo
        /// dell'agente, aggiorna la richiesta di approvazione, e solo dopo riporta la finestra sul progetto. Senza
        /// autorizzazione risponde 409 con ciò che resta da salvare; se commit o pubblicazione non riescono si
        /// resta dentro, con il motivo.
        /// </summary>
        [HttpPost("leave")]
        public async Task<IActionResult> Leave([FromBody] WorkspaceRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath) || string.IsNullOrWhiteSpace(body.AgentName)
                || string.IsNullOrWhiteSpace(body.ConnectionId))
                return BadRequest(new { error = "projectPath, agentName e connectionId sono obbligatori." });

            var published = new System.Collections.Generic.List<string>();
            var committed = new System.Collections.Generic.List<string>();
            var desk = (await _worktree.ListSlotsAsync(body.ProjectPath)).FirstOrDefault(
                x => string.Equals(x.Agent, body.AgentName, StringComparison.OrdinalIgnoreCase));
            if (desk != null)
            {
                var work = await ReadPendingAsync(body.ProjectPath, body.AgentName, desk);
                string Name(RepoChanges r) => string.IsNullOrEmpty(r.Path) ? body.AgentName : r.Label;

                if (work.Any && body.Authorized != true)
                    return Conflict(new
                    {
                        error = $"Nella copia di '{body.AgentName}' c'è lavoro da salvare: serve la tua autorizzazione per committarlo e pubblicarlo.",
                        needsAuthorization = true,
                        pending = Describe(work, body.AgentName),
                    });

                if (work.Dirty.Count > 0 && string.IsNullOrWhiteSpace(body.CommitMessage))
                    return Conflict(new { error = "Manca il messaggio del commit." });

                // Prima i submodule, poi chi li contiene: il commit del padre registra la loro nuova versione.
                foreach (var repo in work.Dirty.OrderByDescending(r => r.Depth))
                {
                    var dir = string.IsNullOrEmpty(repo.Path)
                        ? desk.Path
                        : System.IO.Path.Combine(desk.Path, repo.Path.Replace('/', System.IO.Path.DirectorySeparatorChar));
                    var problem = await _worktree.CommitAllAsync(dir, body.CommitMessage);
                    if (problem != null)
                        return Conflict(new { error = $"Non riesco a committare in '{Name(repo)}': {problem}. Resti nella copia." });
                    committed.Add(Name(repo));
                }

                // Niente deve restare fuori da un commit: se resta, non si esce.
                var left = await _worktree.UncommittedAsync(desk.Path);
                if (left.Count > 0)
                    return Conflict(new
                    {
                        error = $"Dopo il commit nella copia di '{body.AgentName}' restano {left.Count} file non committati. Resti nella copia.",
                        uncommitted = left,
                    });

                // Committato non basta: è il ramo pubblicato che l'approvazione fonde e che un collega vede.
                work = await ReadPendingAsync(body.ProjectPath, body.AgentName, desk);
                foreach (var repo in work.SubmodulesAhead.OrderByDescending(r => r.Depth))
                {
                    var pushed = await _sync.PushAsync(body.ProjectPath, body.AgentName, repo.Path);
                    if (!pushed.Success)
                        return Conflict(new { error = NotPublished(body.AgentName, Name(repo), pushed.Refused ?? pushed.Message) });
                    published.Add(Name(repo));
                }

                if (work.RootUnpublished > 0)
                {
                    if (work.Request != null)
                    {
                        // Lo stesso ramo che l'agente aveva pubblicato: la richiesta di approvazione resta una, e
                        // ora parla anche di ciò che la persona ha corretto.
                        var (head, error) = await _worktree.PublishToAsync(desk.Path, work.Request.PublishedBranch);
                        if (error != null)
                            return Conflict(new { error = NotPublished(body.AgentName, work.Request.PublishedBranch, error) });
                        _requests.Open(body.ProjectPath, body.AgentName, work.Request.PublishedBranch, desk.Branch, head,
                            await _worktree.ChangedFilesAsync(body.ProjectPath, body.AgentName));
                    }
                    else
                    {
                        // Nessuna richiesta aperta su questo ramo (già decisa, o mai nata): si pubblica come fa
                        // l'agente a fine lavoro, e il lavoro torna ad avere una richiesta su cui decidere.
                        var attempt = await _worktree.TryCommitAndPushBranchAsync(body.ProjectPath, body.AgentName, body.CommitMessage);
                        if (attempt?.Pushed == null)
                            return Conflict(new { error = NotPublished(body.AgentName, desk.Branch, attempt?.Error ?? "niente da pubblicare") });
                        _requests.Open(body.ProjectPath, body.AgentName, attempt.Pushed.Branch, attempt.Pushed.LocalBranch,
                            attempt.Pushed.HeadSha, await _worktree.ChangedFilesAsync(body.ProjectPath, body.AgentName));
                    }
                    published.Insert(0, body.AgentName);
                }
            }

            Back(body);
            var closed = _sessions.Close(body.ProjectPath, body.AgentName, discardWork: false);
            _logger.LogInformation("[Workspace] la finestra {Connection} torna al progetto {Path}.", body.ConnectionId, body.ProjectPath);
            return Ok(new { closed.Closed, closed.Message, committed, published });
        }

        private static string NotPublished(string agentName, string what, string why)
            => $"Il lavoro nella copia di '{agentName}' è committato ma non riesco a pubblicarlo ('{what}': {why}). " +
               "Resti nella copia: riprova quando il problema è risolto.";

        private void Back(WorkspaceRequest body)
        {
            ProjectsManager.PointAtWorkingCopy(_services, body.ProjectPath, body.ProjectPath);
            _databaseManager.RegisterConnection(body.ConnectionId, body.ProjectPath);
            _watchers.RegisterWatcher(body.ConnectionId, body.ProjectPath);
        }
    }
}
