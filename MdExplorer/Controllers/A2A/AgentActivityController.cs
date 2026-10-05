using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRun;
using Microsoft.AspNetCore.Mvc;

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

        public AgentActivityController(IAgentActivityBoard board)
        {
            _board = board;
        }

        [HttpGet("running")]
        public IActionResult Running([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest("projectPath è obbligatorio.");
            return Ok(AgentActivityBroadcaster.Payload(_board, projectPath));
        }
    }
}
