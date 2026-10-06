using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Agents.Workflow;
using MdExplorer.Services.AgentRegistry;
using Microsoft.AspNetCore.Mvc;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// Il workflow degli agenti (<c>*.workflow.json</c>, standard v2). Per ora la verifica: la usa chi lo scrive, una persona
    /// o un LLM con la skill <c>mde-workflow</c>, per sapere in un giro solo tutto ciò che non va e dove.
    /// </summary>
    [ApiController]
    [Route("api/A2A/workflow")]
    public class AgentWorkflowController : ControllerBase
    {
        private readonly IAgentRegistryService _registry;

        public AgentWorkflowController(IAgentRegistryService registry)
        {
            _registry = registry;
        }

        /// <param name="projectPath">La cartella del progetto.</param>
        /// <param name="path">Il file, dalla radice del progetto (per esempio <c>citta-degli-agenti/gara/gara.workflow.json</c>).</param>
        [HttpGet("check")]
        public IActionResult Check([FromQuery] string? projectPath, [FromQuery] string? path)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = "projectPath è obbligatorio." });
            WorkflowCheckResult result;
            try
            {
                result = WorkflowFile.Check(projectPath, path, _registry.RefreshCatalog(projectPath));
            }
            catch (FileNotFoundException ex) { return NotFound(new { error = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }

            return Ok(new
            {
                valid = result.IsValid,
                errors = result.Issues.Count(i => i.Severity == WorkflowSeverity.Error),
                warnings = result.Issues.Count(i => i.Severity == WorkflowSeverity.Warning),
                issues = result.Issues.Select(i => new
                {
                    severity = i.Severity == WorkflowSeverity.Error ? "error" : "warning",
                    path = i.Path,
                    message = i.Message,
                    fix = i.Fix,
                }),
                steps = result.Descriptor?.Steps.Count ?? 0,
            });
        }
    }
}
