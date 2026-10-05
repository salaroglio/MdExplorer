using System;
using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRun;
using Microsoft.AspNetCore.Mvc;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Who answers for each agent of a project, and «this agent is mine»: what the agents' registry shows
    /// next to each agent, and the gesture that gives an agent of nobody to the person at this computer.
    /// </summary>
    [ApiController]
    [Route("api/A2A/owners")]
    public class AgentOwnersController : ControllerBase
    {
        private readonly IAgentOwnershipDesk _desk;

        public AgentOwnersController(IAgentOwnershipDesk desk)
        {
            _desk = desk;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = "projectPath è obbligatorio." });
            var view = _desk.Describe(projectPath);
            return Ok(new
            {
                applies = view.Applies,
                me = view.Me,
                document = view.Document,
                documentProblem = view.DocumentProblem,
                agents = view.Agents.Select(Shape).ToList(),
            });
        }

        [HttpPost("assign-to-me")]
        public IActionResult AssignToMe([FromBody] AssignAgentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath) || request.AgentNames == null || request.AgentNames.Count == 0)
                return BadRequest(new { error = "projectPath e agentNames sono obbligatori." });
            try
            {
                return Ok(new { agents = _desk.AssignToMe(request.ProjectPath, request.AgentNames).Select(Shape).ToList() });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        private static object Shape(AgentOwnerVerdict v) => new
        {
            agentName = v.AgentName,
            // mine | someoneElse | unassigned | contested
            kind = char.ToLowerInvariant(v.Kind.ToString()[0]) + v.Kind.ToString().Substring(1),
            ownerName = v.OwnerName,
            ownerEmail = v.OwnerEmail,
            contestedBy = v.ContestedBy,
            canWorkHere = v.CanWorkHere,
            explanation = v.Explain(),
        };
    }

    public class AssignAgentRequest
    {
        public string ProjectPath { get; set; }
        /// <summary>The agents to take: all in one write of the document.</summary>
        public System.Collections.Generic.List<string> AgentNames { get; set; }
    }
}
