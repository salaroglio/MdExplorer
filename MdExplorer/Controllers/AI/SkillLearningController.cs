using System;
using MdExplorer.Services.SkillLearning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// «Impara da questa attività» (sprint 2026-11-24-Skill-Che-Migliorano, F3): il prompt per il motore del tab, poi la
    /// risposta letta e controllata con le differenze. Il modello lo chiama il client, sulla sessione del tab.
    /// </summary>
    [Route("api/skills/learn")]
    [ApiController]
    public class SkillLearningController : ControllerBase
    {
        private readonly SkillLearningService _learning;
        private readonly ILogger<SkillLearningController> _logger;

        public SkillLearningController(SkillLearningService learning, ILogger<SkillLearningController> logger)
        {
            _learning = learning;
            _logger = logger;
        }

        public sealed class PrepareRequest
        {
            public string? ProjectPath { get; set; }
            public Guid ThreadId { get; set; }
        }

        public sealed class ReviewRequest
        {
            public string? ProjectPath { get; set; }
            public long From { get; set; }
            public long To { get; set; }
            public string? Answer { get; set; }
        }

        [HttpPost("prepare")]
        public IActionResult Prepare([FromBody] PrepareRequest request)
        {
            try
            {
                var prepared = _learning.Prepare(request?.ProjectPath, request?.ThreadId ?? Guid.Empty);
                _logger.LogInformation("[SkillLearning] prompt per la chat {ThreadId}: eventi {From}-{To}, {Chars} caratteri",
                    prepared.ThreadId, prepared.From, prepared.To, prepared.Prompt.Length);
                return Ok(new { prepared.ThreadId, prepared.Prompt, prepared.From, prepared.To, prepared.Harness, prepared.SkillsFolder, channel = SkillLearningService.Channel });
            }
            catch (InvalidOperationException ex)
            {
                // Il motivo è per l'utente (nessun motore dichiarato, chat di un altro progetto…): lo si dice com'è.
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("review")]
        public IActionResult Review([FromBody] ReviewRequest request)
        {
            try
            {
                return Ok(_learning.Review(request?.ProjectPath, request?.From ?? 0, request?.To ?? 0, request?.Answer));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
