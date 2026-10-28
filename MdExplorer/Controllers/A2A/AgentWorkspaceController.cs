using System;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Service;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Services.FileSystemWatcherManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.Features.Services;

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
        private readonly MdExplorer.Services.Git.ISafePushService _push;
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
            MdExplorer.Services.Git.ISafePushService push,
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
            _push = push;
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
                return BadRequest(new { error = UiText.Say("projectPath, agentName e connectionId sono obbligatori.", "projectPath, agentName and connectionId are required.") });

            var desk = (await _worktree.ListSlotsAsync(body.ProjectPath)).FirstOrDefault(
                x => string.Equals(x.Agent, body.AgentName, StringComparison.OrdinalIgnoreCase));
            if (desk == null)
                return UnprocessableEntity(new
                {
                    error = UiText.Say(
                        $"'{body.AgentName}' non ha una copia di lavoro in questo momento: la sua scrivania è passata a un " +
                        "altro agente. Il suo lavoro si riapre dalla posta, con «Ci metto mano».",
                        $"'{body.AgentName}' has no working copy right now: its desk has passed to another agent. " +
                        "Its work can be reopened from the mail, with «Take it over»."),
                });
            if (desk.Running && !desk.Held)
                return UnprocessableEntity(new
                {
                    error = UiText.Say($"'{body.AgentName}' sta lavorando adesso: aspetta che finisca, poi entra nella sua copia.", $"'{body.AgentName}' is working right now: wait until it finishes, then enter its copy."),
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
                return UnprocessableEntity(new { error = UiText.Say($"Non riesco ad aprire la copia di '{body.AgentName}': {ex.Message}", $"I can't open the copy of '{body.AgentName}': {ex.Message}") });
            }

            _logger.LogInformation("[Workspace] la finestra {Connection} lavora nella copia di '{Agent}': {Path}",
                body.ConnectionId, body.AgentName, desk.Path);
            return Ok(new { worktreePath = desk.Path, branch = desk.Branch, slot = desk.Index });
        }

        /// <summary>Ciò che resta da salvare in una copia: file senza commit e commit non pubblicati, per repository.</summary>
        private sealed class PendingWork
        {
            public System.Collections.Generic.List<RepoChanges> Dirty { get; } = new();
            /// <summary>Submodule con commit da pubblicare (il loro «avanti» è rispetto al loro ramo remoto).</summary>
            public System.Collections.Generic.List<RepoChanges> SubmodulesAhead { get; } = new();
            /// <summary>Commit della copia non ancora sul ramo pubblicato dell'agente.</summary>
            public int RootUnpublished { get; set; }
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
            work.RootUnpublished = await _worktree.UnpublishedCommitsAsync(desk.Path, _requests.PublishedBranchOf(desk.Branch));
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
                return BadRequest(new { error = UiText.Say("projectPath e agentName sono obbligatori.", "projectPath and agentName are required.") });
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
                return BadRequest(new { error = UiText.Say("projectPath, agentName e connectionId sono obbligatori.", "projectPath, agentName and connectionId are required.") });

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
                        error = UiText.Say($"Nella copia di '{body.AgentName}' c'è lavoro da salvare: serve la tua autorizzazione per farne il commit e pubblicarlo.", $"There is unsaved work in the copy of '{body.AgentName}': your authorization is needed to commit and publish it."),
                        needsAuthorization = true,
                        pending = Describe(work, body.AgentName),
                    });

                if (work.Dirty.Count > 0 && string.IsNullOrWhiteSpace(body.CommitMessage))
                    return Conflict(new { error = UiText.Say("Manca il messaggio del commit.", "The commit message is missing.") });

                // Prima i submodule, poi chi li contiene: il commit del padre registra la loro nuova versione.
                foreach (var repo in work.Dirty.OrderByDescending(r => r.Depth))
                {
                    var dir = string.IsNullOrEmpty(repo.Path)
                        ? desk.Path
                        : System.IO.Path.Combine(desk.Path, repo.Path.Replace('/', System.IO.Path.DirectorySeparatorChar));
                    var problem = await _worktree.CommitAllAsync(dir, body.CommitMessage);
                    if (problem != null)
                        return Conflict(new { error = UiText.Say($"Non riesco a fare il commit in '{Name(repo)}': {problem}. Resti nella copia.", $"I can't commit in '{Name(repo)}': {problem}. You stay in the copy.") });
                    committed.Add(Name(repo));
                }

                // Niente deve restare fuori da un commit: se resta, non si esce.
                var left = await _worktree.UncommittedAsync(desk.Path);
                if (left.Count > 0)
                    return Conflict(new
                    {
                        error = UiText.Say($"Dopo il commit nella copia di '{body.AgentName}' restano {left.Count} file senza commit. Resti nella copia.", $"After the commit in the copy of '{body.AgentName}', {left.Count} files are still uncommitted. You stay in the copy."),
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
                    // Sul ramo che l'agente aveva pubblicato: la richiesta di approvazione resta una, e ora parla
                    // anche di ciò che la persona ha corretto.
                    var problem = await _requests.PublishCopyAsync(body.ProjectPath, body.AgentName, desk.Path, desk.Branch);
                    if (problem != null)
                        return Conflict(new { error = NotPublished(body.AgentName, body.AgentName, problem) });
                    published.Insert(0, body.AgentName);
                }
            }

            Back(body);
            var closed = _sessions.Close(body.ProjectPath, body.AgentName, discardWork: false);
            _logger.LogInformation("[Workspace] la finestra {Connection} torna al progetto {Path}.", body.ConnectionId, body.ProjectPath);
            return Ok(new { closed.Closed, closed.Message, committed, published });
        }

        private static string NotPublished(string agentName, string what, string why)
            => UiText.Say(
                $"Il lavoro nella copia di '{agentName}' ha il commit ma non riesco a pubblicarlo ('{what}': {why}). " +
                "Resti nella copia: riprova quando il problema è risolto.",
                $"The work in the copy of '{agentName}' is committed but I can't publish it ('{what}': {why}). " +
                "You stay in the copy: try again once the problem is solved.");

        /// <summary>
        /// Prima che un agente parta: che cosa, nella cartella della persona, l'agente NON vedrebbe. La sua copia
        /// nasce da <c>origin</c>, quindi i file senza commit e i commit non pubblicati restano fuori.
        /// </summary>
        [HttpPost("project-pending")]
        public async Task<IActionResult> ProjectPending([FromBody] WorkspaceRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath))
                return BadRequest(new { error = UiText.Say("projectPath è obbligatorio.", "projectPath is required.") });
            var view = await _changes.GetAsync(body.ProjectPath, null);
            if (view?.Problem != null || view?.NotAGitRepository == true)
                return Ok(new { uncommitted = Array.Empty<object>(), unpublished = Array.Empty<object>() });
            var repos = view?.Repos ?? Array.Empty<RepoChanges>();
            string Name(RepoChanges r) => string.IsNullOrEmpty(r.Path) ? "progetto" : r.Label;
            return Ok(new
            {
                uncommitted = repos.Where(r => r.Uncommitted.Count > 0)
                    .Select(r => new { repo = r.Path, label = Name(r), files = r.Uncommitted.Select(f => f.Path).ToList() }).ToList(),
                unpublished = repos.Where(r => r.Ahead > 0)
                    .Select(r => new { repo = r.Path, label = Name(r), commits = r.Ahead }).ToList(),
            });
        }

        /// <summary>
        /// La persona ha scelto di salvare prima di far partire l'agente: commit di ciò che è cambiato (i submodule
        /// prima, poi chi li contiene) e pubblicazione di tutto. Se qualcosa non riesce lo si dice, e l'agente non parte.
        /// </summary>
        [HttpPost("project-save")]
        public async Task<IActionResult> ProjectSave([FromBody] WorkspaceRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath))
                return BadRequest(new { error = UiText.Say("projectPath è obbligatorio.", "projectPath is required.") });

            var view = await _changes.GetAsync(body.ProjectPath, null);
            var dirty = (view?.Repos ?? Array.Empty<RepoChanges>()).Where(r => r.Uncommitted.Count > 0).ToList();
            if (dirty.Count > 0 && string.IsNullOrWhiteSpace(body.CommitMessage))
                return Conflict(new { error = UiText.Say("Manca il messaggio del commit.", "The commit message is missing.") });

            foreach (var repo in dirty.OrderByDescending(r => r.Depth))
            {
                var dir = string.IsNullOrEmpty(repo.Path)
                    ? body.ProjectPath
                    : System.IO.Path.Combine(body.ProjectPath, repo.Path.Replace('/', System.IO.Path.DirectorySeparatorChar));
                var problem = await _worktree.CommitAllAsync(dir, body.CommitMessage);
                if (problem != null)
                    return Conflict(new { error = UiText.Say(
                        $"Non riesco a fare il commit in '{(string.IsNullOrEmpty(repo.Path) ? "progetto" : repo.Label)}': {problem}. L'agente non è partito.",
                        $"I can't commit in '{(string.IsNullOrEmpty(repo.Path) ? "project" : repo.Label)}': {problem}. The agent did not start.") });
            }

            // origin può essere più avanti della cartella: è la condizione normale durante un giro, perché ogni
            // «Autorizza» pubblica senza passare di qui. Prima si scarica ciò che manca, poi si pubblica; se lo
            // scaricamento non riesce (un conflitto) lo si dice, e l'agente non parte.
            view = await _changes.GetAsync(body.ProjectPath, null);
            if ((view?.Repos ?? Array.Empty<RepoChanges>()).Any(r => r.Behind > 0))
            {
                var pulled = await _sync.PullAllAsync(body.ProjectPath);
                if (!pulled.Success)
                    return Conflict(new
                    {
                        error = UiText.Say(
                            "Committato, ma su origin c'è lavoro che non riesco a scaricare nella tua cartella: " +
                            $"{pulled.Refused ?? pulled.Message} L'agente non è partito.",
                            "Committed, but origin has work I can't download into your folder: " +
                            $"{pulled.Refused ?? pulled.Message} The agent did not start."),
                    });
            }

            var pushed = await _push.PushEverythingAsync(body.ProjectPath, null);
            if (pushed.Refused != null)
                return Conflict(new { error = UiText.Say($"Committato, ma non pubblicato: {pushed.Refused} L'agente non è partito.", $"Committed, but not published: {pushed.Refused} The agent did not start.") });
            if (!pushed.Success)
                return Conflict(new { error = UiText.Say("Commit fatto, ma la pubblicazione si è interrotta: controlla il pannello «in attesa di push». L'agente non è partito.", "Committed, but publishing was interrupted: check the «to push» panel. The agent did not start.") });
            return Ok(new { saved = true });
        }

        private void Back(WorkspaceRequest body)
        {
            ProjectsManager.PointAtWorkingCopy(_services, body.ProjectPath, body.ProjectPath);
            _databaseManager.RegisterConnection(body.ConnectionId, body.ProjectPath);
            _watchers.RegisterWatcher(body.ConnectionId, body.ProjectPath);
        }
    }
}
