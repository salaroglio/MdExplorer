using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
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
        private readonly IAgentApprovalNotifier _notifier;
        private readonly ILogger<AgentReviewController> _logger;

        public AgentReviewController(
            IAgentMergeRequestService requests,
            IAgentWorktreeManager worktree,
            IAgentWorktreeHoldService sessions,
            IAgentApprovalNotifier notifier,
            ILogger<AgentReviewController> logger)
        {
            _requests = requests;
            _worktree = worktree;
            _sessions = sessions;
            _notifier = notifier;
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

        /// <summary>
        /// Dove sono i documenti che un messaggio cita: in una consegna ancora da approvare (e di chi), già
        /// nel progetto, oppure da nessuna parte. Serve alla posta per mostrare, sotto un messaggio, gli
        /// artefatti che l'agente dice di aver scritto, e per sapere da dove aprirli.
        /// </summary>
        [HttpPost("artifacts")]
        public async Task<IActionResult> Artifacts([FromBody] ArtifactsRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.ProjectPath))
                return BadRequest(new { error = "projectPath è obbligatorio" });

            var pending = _requests.Pending(body.ProjectPath)
                .Select(r => new { Request = r, Files = _requests.FilesOf(r) })
                .ToList();
            var root = System.IO.Path.GetFullPath(body.ProjectPath);

            var artifacts = new List<object>();
            foreach (var path in (body.Paths ?? new List<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Replace('\\', '/').Trim().TrimStart('/'))
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                artifacts.Add(await Locate(path));
            }
            return Ok(new { artifacts });

            async Task<object> Locate(string path)
            {
                {
                    // Prima la consegna dello stesso agente, poi quella di chiunque.
                    var delivery = pending
                        .Where(x => x.Files.Any(f => string.Equals(f.Path, path, StringComparison.OrdinalIgnoreCase)))
                        .OrderByDescending(x => string.Equals(x.Request.AgentName, body.Agent, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();
                    if (delivery != null)
                        return (object)new { path, state = "pending", requestId = delivery.Request.Id, agentName = delivery.Request.AgentName };

                    var full = System.IO.Path.GetFullPath(System.IO.Path.Combine(root, path));
                    var inside = full.StartsWith(root + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                    if (inside && System.IO.File.Exists(full))
                        return new { path, state = "inProject", requestId = (Guid?)null, agentName = (string)null };

                    // Approvato ma non ancora nella cartella: sta sul ramo principale di origin, e arriva
                    // con «Scarica tutto». Dirlo è diverso da «non trovato».
                    var approved = inside && await _worktree.ReadFileAtAsync(root, "origin/HEAD", path) != null;
                    return new { path, state = approved ? "toPull" : "missing", requestId = (Guid?)null, agentName = (string)null };
                }
            }
        }

        public sealed class ArtifactsRequest
        {
            public string? ProjectPath { get; set; }
            /// <summary>L'agente che ha scritto il messaggio: a parità di percorso vale la sua consegna.</summary>
            public string? Agent { get; set; }
            public List<string>? Paths { get; set; }
        }

        /// <summary>
        /// Autorizza: la richiesta viene fusa nel ramo principale e, se la scheda dell'agente lo dichiara
        /// (<c>on_approval_notify</c>), la persona avvisa il collega.
        /// <para>
        /// Con <b>più di un destinatario</b> la persona deve scegliere (<c>notify</c>) oppure dire che non
        /// avvisa nessuno (<c>nobody</c>): senza scelta non si fonde niente e la risposta elenca i candidati.
        /// La scelta si controlla <b>prima</b> del merge, così non resta un lavoro fuso a metà strada.
        /// </para>
        /// </summary>
        [HttpPost("requests/{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveRequest? body = null)
        {
            try
            {
                var request = _requests.Get(id);
                if (request == null)
                    return UnprocessableEntity(new { error = $"Richiesta di merge {id} inesistente." });

                var candidates = _notifier.CandidatesFor(request.ProjectPath, request.AgentName);
                string recipient = null;

                if (candidates.Count > 0 && body?.Nobody != true)
                {
                    if (!string.IsNullOrWhiteSpace(body?.Notify))
                    {
                        var chosen = candidates.FirstOrDefault(c =>
                            string.Equals(c.Name, body.Notify.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (chosen == null)
                            return UnprocessableEntity(new
                            {
                                error = $"'{body.Notify}' non è tra i destinatari di '{request.AgentName}'.",
                                code = "recipient-unknown",
                                candidates = candidates.Select(ToCandidateDto).ToList(),
                            });
                        if (!chosen.Available)
                            return UnprocessableEntity(new
                            {
                                error = chosen.Reason,
                                code = "recipient-unavailable",
                                candidates = candidates.Select(ToCandidateDto).ToList(),
                            });
                        recipient = chosen.Name;
                    }
                    else if (candidates.Count == 1 && candidates[0].Available)
                    {
                        recipient = candidates[0].Name;
                    }
                    else
                    {
                        // Più destinatari, o l'unico non raggiungibile: la decisione è della persona.
                        return Conflict(new
                        {
                            error = candidates.Count == 1
                                ? candidates[0].Reason
                                : $"'{request.AgentName}' può passare il lavoro a più colleghi: scegli a chi, o approva senza avvisare nessuno.",
                            code = "choose-recipient",
                            needsChoice = true,
                            candidates = candidates.Select(ToCandidateDto).ToList(),
                        });
                    }
                }

                var r = await _requests.ApproveAsync(id, HttpContext.RequestAborted);
                if (r.Status != AgentMergeRequest.StatusEnum.Merged)
                {
                    // Autorizzata ma non fusa: e' una condizione da dire, non da nascondere
                    // dietro un 200 che sembra un successo. Se non e' fusa, nessuno viene avvisato.
                    return StatusCode(409, ToDto(r));
                }

                ApprovalNotice notice = null;
                if (recipient != null)
                    notice = _notifier.Notify(r.ProjectPath, r.AgentName, recipient,
                        _requests.FilesOf(r).Select(f => f.Path));

                return Ok(ToDto(r, notice));
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

        private static object ToCandidateDto(ApprovalRecipient c) => new
        {
            name = c.Name,
            role = c.Role,
            available = c.Available,
            reason = c.Reason,
        };

        private object ToDto(AgentMergeRequest r, ApprovalNotice notice = null) => new
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
            // Che cosa fa l'agente che ha consegnato (dalla sua scheda): chi approva deve poterlo rileggere qui.
            agentSummary = _notifier.SummaryOf(r.ProjectPath, r.AgentName),
            // A chi può passare il lavoro la persona che approva (vuota = nessun avviso): la UI la mostra
            // PRIMA di «Approva» e chiede la scelta se i candidati sono più d'uno.
            notifyCandidates = r.Status == AgentMergeRequest.StatusEnum.Pending
                ? _notifier.CandidatesFor(r.ProjectPath, r.AgentName).Select(ToCandidateDto).ToList()
                : new System.Collections.Generic.List<object>(),
            // Esito dell'avviso dopo «Approva»; assente se non c'era nessuno da avvisare.
            notice = notice == null ? null : new { notified = notice.Notified, recipient = notice.Recipient, error = notice.Error },
        };

        public class ApproveRequest
        {
            /// <summary>A quale collega passare il lavoro (obbligatorio se i candidati sono più d'uno).</summary>
            public string? Notify { get; set; }

            /// <summary>Approva senza avvisare nessuno.</summary>
            public bool Nobody { get; set; }
        }

        public class RejectRequest
        {
            /// <summary>Nullable di proposito: la UI può rifiutare senza motivare.</summary>
            public string? Note { get; set; }
        }
    }
}
