using System;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
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
        private readonly MdExplorer.Services.AgentRun.IAgentWorkflowExecutor _executor;
        private readonly IUserSettingsDB _session;

        public AgentWorkflowController(IAgentRegistryService registry, MdExplorer.Services.AgentRun.IAgentWorkflowExecutor executor, IUserSettingsDB session)
        {
            _session = session;
            _registry = registry;
            _executor = executor;
        }

        /// <summary>I giri che la persona ha archiviato nella sua posta.</summary>
        private System.Collections.Generic.HashSet<string> ArchivedRounds(string projectPath)
        {
            _session.BeginTransaction();
            var all = _session.GetDal<ArchivedRound>().GetList().ToList();
            _session.Commit();
            return all.Where(a => MdExplorer.Features.Agents.AgentPathComparer.Equals(a.ProjectPath, projectPath)).Select(a => a.RoundId)
                      .ToHashSet(StringComparer.Ordinal);
        }

        /// <summary>I giri del progetto, per la posta: ogni giro è una voce con i suoi passi e i turni dei suoi messaggi.</summary>
        [HttpGet("rounds")]
        public IActionResult Rounds([FromQuery] string? projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = "projectPath è obbligatorio." });
            var archived = ArchivedRounds(projectPath);
            return Ok(new
            {
                rounds = _executor.Rounds(projectPath).Select(r => new
                {
                    archived = archived.Contains(r.Id),
                    id = r.Id, title = r.Title, values = r.Values, startedAt = r.StartedAt, startedBy = r.StartedBy,
                    lastActivityAt = r.LastActivityAt, finished = r.Finished, stepsDone = r.StepsDone, stepsTotal = r.StepsTotal,
                    runs = r.Runs,
                    steps = r.Steps.Select(p => new { messageId = p.Id, step = p.Step, label = p.Label, agent = p.Agent, owner = p.Owner, state = p.State, note = p.Note, silent = p.Silent }),
                }),
            });
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
