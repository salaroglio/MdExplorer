using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Services.Federation;
using Microsoft.AspNetCore.Mvc;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Vista dello stato di federazione per la UI (§12.5 / Fase 6d). Loopback + guardia R12
    /// come gli altri A2A. Espone le <b>città locali attive</b> (progetti con la città accesa
    /// su questa macchina) e, in futuro, le <b>città remote</b> viste sul relay — oggi vuote
    /// finché il canale <c>/mdfed</c> non è collegato (seam rimandato di Fase 6b).
    /// </summary>
    [ApiController]
    [Route("api/A2A/federation")]
    public class FederationController : ControllerBase
    {
        private readonly IFederationState _state;

        public FederationController(IFederationState state)
        {
            _state = state;
        }

        [HttpGet("cities")]
        public IActionResult Cities([FromQuery] string? projectPath)
        {
            var local = _state.GetLocalCities().AsEnumerable();
            if (!string.IsNullOrWhiteSpace(projectPath))
                local = local.Where(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));

            return Ok(new
            {
                local = local.Select(c => new
                {
                    projectPath = c.ProjectPath,
                    projectName = c.ProjectName,
                    roomId = c.RoomId,
                    relayUrl = c.RelayUrl,
                }).ToList(),
                // Città remote viste sul relay: vuote finché /mdfed non è collegato (Fase 6b).
                remote = System.Array.Empty<object>(),
                relayConnected = false,
            });
        }
    }
}
