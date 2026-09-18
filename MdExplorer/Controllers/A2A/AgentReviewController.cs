using System;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Services.AgentRun;
using MdExplorer.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// La revisione del lavoro degli agenti: cosa hanno prodotto, e cosa ne fai.
    /// <para>
    /// Tre gesti, non uno: <b>autorizzo</b> (fonde), <b>rifiuto</b> (non distrugge nulla, il
    /// branch resta), <b>ci metto mano</b> (apre il worktree sul filesystem e mette l'agente in
    /// coda). Il terzo è quello che rende il rifiuto qualcosa di più di un "no": senza, un
    /// lavoro bocciato resterebbe in un limbo che nessuno riprende.
    /// </para>
    /// <para>Canale UI: loopback, come gli altri controller della città.</para>
    /// </summary>
    [ApiController]
    [Route("api/AgentReview")]
    public class AgentReviewController : ControllerBase
    {
        private readonly IAgentMergeRequestService _requests;
        private readonly IAgentWorktreeManager _worktree;
        private readonly IAgentWorktreeHoldService _sessions;
        private readonly ILogger<AgentReviewController> _logger;

        public AgentReviewController(
            IAgentMergeRequestService requests,
            IAgentWorktreeManager worktree,
            IAgentWorktreeHoldService sessions,
            ILogger<AgentReviewController> logger)
        {
            _requests = requests;
            _worktree = worktree;
            _sessions = sessions;
            _logger = logger;
        }

        /// <summary>Richieste in attesa di decisione, con i file toccati già dentro.</summary>
        [HttpGet("requests")]
        public IActionResult Pending([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = "projectPath è obbligatorio" });

            var list = _requests.Pending(projectPath).Select(r => ToDto(r)).ToList();
            return Ok(new { requests = list });
        }

        /// <summary>Autorizza: la richiesta viene fusa nel ramo principale.</summary>
        [HttpPost("requests/{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                var r = await _requests.ApproveAsync(id, HttpContext.RequestAborted);
                if (r.Status != AgentMergeRequest.StatusEnum.Merged)
                {
                    // Autorizzata ma non fusa: e' una condizione da dire, non da nascondere
                    // dietro un 200 che sembra un successo.
                    return StatusCode(409, ToDto(r));
                }
                return Ok(ToDto(r));
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }
        }

        /// <summary>Rifiuta. Il branch resta: il lavoro non si butta, si riprende.</summary>
        [HttpPost("requests/{id}/reject")]
        public IActionResult Reject(Guid id, [FromBody] RejectRequest body)
        {
            try { return Ok(ToDto(_requests.Reject(id, body?.Note))); }
            catch (InvalidOperationException ex) { return UnprocessableEntity(new { error = ex.Message }); }
        }

        /// <summary>
        /// «Ci metto mano»: apre la sessione d'intervento (l'agente va in coda) e apre la
        /// directory del worktree nel file manager, dove il branch è già in check-out.
        /// </summary>
        [HttpPost("requests/{id}/take")]
        public async Task<IActionResult> Take(Guid id)
        {
            var r = _requests.Get(id);
            if (r == null) return NotFound(new { error = "Richiesta inesistente." });

            // I posti di lavoro sono pochi e si riciclano: quello dove l'agente ha prodotto
            // questo lavoro può essere già passato a un altro. Il lavoro però è un branch, quindi
            // si rimette su un posto — è la ragione per cui non basta comporre un percorso.
            string worktreePath;
            try
            {
                worktreePath = await _worktree.MaterializeForReviewAsync(
                    r.ProjectPath, r.AgentName, r.LocalBranch);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Review] impossibile rimettere '{Branch}' su un posto di lavoro.", r.LocalBranch);
                return UnprocessableEntity(new { error = ex.Message });
            }

            // Prima la sessione, poi la cartella: se aprissimo prima il file manager e la
            // sessione fallisse, l'utente si troverebbe a modificare un worktree che l'agente
            // può ancora ripulire.
            _sessions.Open(r.ProjectPath, r.AgentName, $"revisione di {r.PublishedBranch}");

            var opened = CrossPlatformProcess.OpenFolder(worktreePath);
            if (!opened)
                _logger.LogWarning("[Review] impossibile aprire il file manager su '{Path}'", worktreePath);

            return Ok(new
            {
                worktreePath,
                folderOpened = opened,
                sessionOpen = true,
                agentQueued = true,
            });
        }

        /// <summary>
        /// Chiude la sessione d'intervento. <c>discard=true</c> = ho annullato: la richiesta
        /// torna in coda perché l'agente la rifaccia.
        /// </summary>
        [HttpPost("requests/{id}/release")]
        public IActionResult Release(Guid id, [FromQuery] bool discard = false)
        {
            var r = _requests.Get(id);
            if (r == null) return NotFound(new { error = "Richiesta inesistente." });

            var result = _sessions.Close(r.ProjectPath, r.AgentName, discard);
            return Ok(new { result.Closed, result.Requeued, result.Message });
        }

        private object ToDto(AgentMergeRequest r) => new
        {
            id = r.Id,
            agentName = r.AgentName,
            branch = r.PublishedBranch,
            headSha = r.HeadSha,
            createdAt = r.CreatedAt,
            status = r.Status,
            note = r.Note,
            // Sessione d'intervento in corso su questo agente: la UI deve poter mostrare
            // "ci stai lavorando" invece di riproporre "prendi in mano".
            sessionOpen = _sessions.IsHeld(r.ProjectPath, r.AgentName),
            files = _requests.FilesOf(r).Select(f => new { change = f.Change, path = f.Path }).ToList(),
        };

        public class RejectRequest
        {
            /// <summary>Nullable di proposito: la UI può rifiutare senza motivare.</summary>
            public string? Note { get; set; }
        }
    }
}
