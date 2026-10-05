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
    /// committato: il lavoro si committa (dal pannello delle differenze) oppure si resta dentro. Ciò che è
    /// committato viene pubblicato all'uscita.
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
        private readonly MdExplorer.Services.Git.IRepoSyncService _sync;
        private readonly ILogger<AgentWorkspaceController> _logger;

        public AgentWorkspaceController(
            IAgentWorktreeManager worktree,
            IAgentWorktreeHoldService sessions,
            IDatabaseManager databaseManager,
            IFileSystemWatcherManager watchers,
            IServiceProvider services,
            IWorkingChangesService changes,
            MdExplorer.Services.Git.IRepoSyncService sync,
            ILogger<AgentWorkspaceController> logger)
        {
            _worktree = worktree;
            _sessions = sessions;
            _databaseManager = databaseManager;
            _watchers = watchers;
            _services = services;
            _changes = changes;
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

        /// <summary>
        /// Torna al progetto. Rifiuta se nella copia resta qualcosa di non committato: l'elenco dei file torna
        /// indietro, perché chi esce sappia cosa deve ancora committare.
        /// </summary>
        [HttpPost("leave")]
        public async Task<IActionResult> Leave([FromBody] WorkspaceRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath) || string.IsNullOrWhiteSpace(body.AgentName)
                || string.IsNullOrWhiteSpace(body.ConnectionId))
                return BadRequest(new { error = "projectPath, agentName e connectionId sono obbligatori." });

            var published = new System.Collections.Generic.List<string>();
            var desk = (await _worktree.ListSlotsAsync(body.ProjectPath)).FirstOrDefault(
                x => string.Equals(x.Agent, body.AgentName, StringComparison.OrdinalIgnoreCase));
            if (desk != null)
            {
                var uncommitted = await _worktree.UncommittedAsync(desk.Path);
                if (uncommitted.Count > 0)
                    return Conflict(new
                    {
                        error = $"Nella copia di '{body.AgentName}' ci sono {uncommitted.Count} file non committati: " +
                                "committali dal pannello «Differenze», poi torna al tuo lavoro.",
                        uncommitted,
                    });

                // Committato non basta: il lavoro deve essere anche pubblicato, perché è il ramo pubblicato che
                // l'approvazione fonde e che un collega vede. Lo si pubblica qui, repository per repository; se
                // non riesce si resta dentro, con il motivo — fuori dalla copia nessun contatore lo ricorderebbe.
                var view = await _changes.GetAsync(body.ProjectPath, body.AgentName);
                foreach (var repo in (view?.Repos ?? Array.Empty<RepoChanges>()).Where(r => r.Ahead > 0))
                {
                    var pushed = await _sync.PushAsync(body.ProjectPath, body.AgentName, repo.Path);
                    if (!pushed.Success)
                        return Conflict(new
                        {
                            error = $"Il lavoro nella copia di '{body.AgentName}' è committato ma non riesco a pubblicarlo " +
                                    $"('{repo.Label}': {pushed.Refused ?? pushed.Message}). Resti nella copia: riprova quando " +
                                    "il problema è risolto.",
                            notPublished = repo.Label,
                        });
                    // La radice della copia si chiama come la scrivania («slot-1»): alla persona si dice di chi è il lavoro.
                    published.Add(string.IsNullOrEmpty(repo.Path) ? body.AgentName : repo.Label);
                }
            }

            Back(body);
            var closed = _sessions.Close(body.ProjectPath, body.AgentName, discardWork: false);
            _logger.LogInformation("[Workspace] la finestra {Connection} torna al progetto {Path}.", body.ConnectionId, body.ProjectPath);
            return Ok(new { closed.Closed, closed.Message, published });
        }

        private void Back(WorkspaceRequest body)
        {
            ProjectsManager.PointAtWorkingCopy(_services, body.ProjectPath, body.ProjectPath);
            _databaseManager.RegisterConnection(body.ConnectionId, body.ProjectPath);
            _watchers.RegisterWatcher(body.ConnectionId, body.ProjectPath);
        }
    }
}
