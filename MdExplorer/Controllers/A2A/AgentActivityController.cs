using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRun;
using Microsoft.AspNetCore.Mvc;
using MdExplorer.Features.Services;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Who is working right now in a project: what a window asks when it opens, before the first
    /// <c>agentActivityChanged</c> event reaches it.
    /// </summary>
    [ApiController]
    [Route("api/A2A/activity")]
    public class AgentActivityController : ControllerBase
    {
        private readonly IAgentActivityBoard _board;
        private readonly IAgentTurnDiary _diary;

        public AgentActivityController(IAgentActivityBoard board, IAgentTurnDiary diary)
        {
            _board = board;
            _diary = diary;
        }

        /// <summary>
        /// Il diario di un turno: che cosa ha scritto, pensato e usato l'agente. <paramref name="after"/> = la versione
        /// che chi chiede ha già: tornano solo le voci cambiate dopo (una risposta che cresce torna intera).
        /// </summary>
        [HttpGet("diary/{id}")]
        public IActionResult Diary(System.Guid id, [FromQuery] long after = 0)
        {
            var view = _diary.Read(id, after);
            if (view == null)
                return NotFound(UiText.Say($"Nessun diario per il turno {id}: non è mai partito, o è fra quelli più vecchi che non si conservano.",
                    $"No diary for turn {id}: it never started, or it is among the older ones that are not kept."));
            return Ok(view);
        }

        /// <summary>
        /// «Ferma» dal diario: chiede di fermare un turno in corso. 404 se non c'è più (è finito nel frattempo): chi ha
        /// premuto lo vede dal diario, che si è già chiuso col suo esito.
        /// </summary>
        [HttpPost("stop/{id}")]
        public IActionResult Stop(System.Guid id)
        {
            if (!_board.Stop(id))
                return NotFound(UiText.Say("Questo turno non è più in corso: non c'è niente da fermare.", "This turn is no longer running: there is nothing to stop."));
            return Ok(new { stopping = true });
        }

        /// <summary>I turni di un progetto, i più recenti prima: quelli in corso e quelli finiti, senza le voci.</summary>
        [HttpGet("recent")]
        public IActionResult Recent([FromQuery] string projectPath, [FromQuery] int max = 20)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(UiText.Say("projectPath è obbligatorio.", "projectPath is required."));
            return Ok(_diary.Recent(projectPath, System.Math.Clamp(max, 1, 100)));
        }

        [HttpGet("running")]
        public IActionResult Running([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(UiText.Say("projectPath è obbligatorio.", "projectPath is required."));
            return Ok(AgentActivityBroadcaster.Payload(_board, projectPath));
        }
    }
}
