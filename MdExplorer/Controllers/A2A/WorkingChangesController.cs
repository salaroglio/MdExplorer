using System;
using System.Threading.Tasks;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Git;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Cosa è cambiato: i file toccati e le loro differenze, per il tab accanto ai documenti.
    /// <para>
    /// Segue il <b>contesto</b>: senza <c>agent</c> risponde sul lavoro dell'utente nel progetto,
    /// con <c>agent</c> sul posto di lavoro di quell'agente. È lo stesso gesto mentale — «cosa è
    /// cambiato qui» — e per questo è un endpoint solo: due endpoint avrebbero significato due
    /// notioni di "cambiato" destinate a divergere.
    /// </para>
    /// </summary>
    [ApiController]
    [Route("api/WorkingChanges")]
    public class WorkingChangesController : ControllerBase
    {
        private readonly IWorkingChangesService _changes;
        private readonly ISafePushService _push;
        private readonly ILogger<WorkingChangesController> _logger;

        public WorkingChangesController(
            IWorkingChangesService changes, ISafePushService push, ILogger<WorkingChangesController> logger)
        {
            _changes = changes;
            _push = push;
            _logger = logger;
        }

        /// <summary>
        /// Pubblica il progetto e i suoi submodule. <b>I figli prima, il padre per ultimo</b>:
        /// qualunque fallimento a monte lascia il remoto vecchio ma coerente, mai rotto.
        /// </summary>
        [HttpPost("push-all")]
        public async Task<IActionResult> PushAll([FromBody] PushAllRequest body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.ProjectPath))
                return BadRequest(new { error = "Nessun progetto indicato." });

            var result = await _push.PushEverythingAsync(body.ProjectPath, EmptyToNull(body.Agent));

            if (result.Refused != null)
            {
                // Rifiutato PRIMA di toccare qualsiasi remoto: e' una condizione che l'utente puo'
                // risolvere, non un errore del server.
                _logger.LogInformation("[PubblicaTutto] rifiutato: {Why}", result.Refused);
                return UnprocessableEntity(result);
            }

            _logger.LogInformation("[PubblicaTutto] {Esito}: {Passi} passi.",
                result.Success ? "riuscito" : "interrotto", result.Steps.Count);
            return result.Success ? Ok(result) : StatusCode(502, result);
        }

        public sealed class PushAllRequest
        {
            public string? ProjectPath { get; set; }
            public string? Agent { get; set; }
        }

        /// <summary>Elenco dei file diversi dal ramo di partenza, nel contesto indicato.</summary>
        [HttpGet("list")]
        public async Task<IActionResult> List([FromQuery] string? projectPath, [FromQuery] string? agent)
        {
            var view = await _changes.GetAsync(projectPath, EmptyToNull(agent));
            // Un problema di contesto non è un errore del server: è una condizione che l'utente
            // può risolvere (aprire un progetto, rimettere il lavoro su un posto).
            if (view.Problem != null) return UnprocessableEntity(view);
            return Ok(view);
        }

        /// <summary>Differenza testuale di un file, in unified diff.</summary>
        [HttpGet("diff")]
        public async Task<IActionResult> Diff(
            [FromQuery] string? projectPath, [FromQuery] string? agent,
            [FromQuery] string? path, [FromQuery] string? repo, [FromQuery] string? oldPath)
        {
            try
            {
                // oldPath arriva solo per le rinomine: senza, git non puo' accoppiare i due
                // lati e risponderebbe che il file e' nuovo.
                var diff = await _changes.DiffAsync(projectPath, EmptyToNull(agent), path, EmptyToNull(repo), EmptyToNull(oldPath));
                return Ok(new { path, repo, diff });
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return UnprocessableEntity(new { error = ex.Message }); }
        }

        /// <summary>
        /// Butta via le modifiche a un file. Irreversibile — nessun commit lo trattiene — quindi
        /// sta qui e non nella finestrella del commit: si scarta dopo aver visto il diff, non alla
        /// cieca.
        /// </summary>
        [HttpPost("discard")]
        public async Task<IActionResult> Discard([FromBody] DiscardRequest body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Path))
                return BadRequest(new { error = "Percorso mancante." });
            try
            {
                var what = await _changes.DiscardAsync(
                    body.ProjectPath, EmptyToNull(body.Agent), body.Path, EmptyToNull(body.Repo));
                _logger.LogInformation("[Changes] '{Repo}/{Path}' {What} (agente: {Agent}).",
                    body.Repo ?? ".", body.Path, what, body.Agent ?? "—");
                return Ok(new { path = body.Path, repo = body.Repo, outcome = what });
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
            catch (InvalidOperationException ex) { return UnprocessableEntity(new { error = ex.Message }); }
        }

        /// <summary>DTO nullable di proposito: un <c>string</c> non-nullable sarebbe implicitamente obbligatorio → 400 su null.</summary>
        public sealed class DiscardRequest
        {
            public string? ProjectPath { get; set; }
            public string? Agent { get; set; }
            public string? Path { get; set; }
            /// <summary>Repository di cui il percorso è relativo: vuoto = la radice del contesto.</summary>
            public string? Repo { get; set; }
        }

        private static string? EmptyToNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
