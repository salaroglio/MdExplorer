using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Services.CodeGraph;
using Microsoft.AspNetCore.Mvc;

namespace MdExplorer.Controllers.CodeGraph
{
    /// <summary>
    /// Le radici di codice esterne (sprint 2026-10-12-Radici-Di-Codice-Esterne, F1): elenco con lo stato su questa
    /// macchina, collegare, scollegare, risolvere un percorso logico. Gli errori arrivano come messaggio (400), mai 500.
    /// </summary>
    [ApiController]
    [Route("api/coderoots")]
    public class CodeRootsController : ControllerBase
    {
        private readonly ICodeRootService _roots;

        public CodeRootsController(ICodeRootService roots) { _roots = roots; }

        public class LinkRequest { public string ProjectPath { get; set; } public string Alias { get; set; } public string LocalPath { get; set; } }
        public class UnlinkRequest { public string ProjectPath { get; set; } public string Alias { get; set; } }
        public class DismissRequest { public string ProjectPath { get; set; } public List<string> Aliases { get; set; } }

        /// <summary>GET /api/coderoots?projectPath — le radici dichiarate e il loro stato qui.</summary>
        [HttpGet]
        public Task<IActionResult> List([FromQuery] string projectPath, CancellationToken ct) => Guarded(async () =>
            Ok((await _roots.ListAsync(projectPath, ct)).Select(View)), projectPath);

        /// <summary>POST /api/coderoots/link {projectPath, alias, localPath} — collega (e, se nuova, dichiara) una radice.</summary>
        [HttpPost("link")]
        public Task<IActionResult> Link([FromBody] LinkRequest request, CancellationToken ct) => Guarded(async () =>
            Ok(View(await _roots.LinkAsync(request.ProjectPath, request.Alias, request.LocalPath, ct))), request?.ProjectPath);

        /// <summary>POST /api/coderoots/unlink {projectPath, alias} — dimentica la posizione su questa macchina.</summary>
        [HttpPost("unlink")]
        public Task<IActionResult> Unlink([FromBody] UnlinkRequest request) => Guarded(() =>
        {
            _roots.Unlink(request.ProjectPath, request.Alias);
            return Task.FromResult<IActionResult>(Ok(new { alias = request.Alias, linked = false }));
        }, request?.ProjectPath);

        /// <summary>GET /api/coderoots/resolve?projectPath&amp;path — il percorso assoluto di un percorso logico (o relativo al progetto).</summary>
        [HttpGet("resolve")]
        public Task<IActionResult> Resolve([FromQuery] string projectPath, [FromQuery] string path, CancellationToken ct) => Guarded(async () =>
        {
            var resolver = await _roots.CreateResolverAsync(projectPath, ct);
            return Ok(new { path, absolutePath = resolver.Resolve(path) });
        }, projectPath);

        /// <summary>GET /api/coderoots/changes?projectPath&amp;alias — ramo e file modificati del repository della radice (sola lettura).</summary>
        [HttpGet("changes")]
        public Task<IActionResult> Changes([FromQuery] string projectPath, [FromQuery] string alias, CancellationToken ct) => Guarded(async () =>
        {
            var c = await _roots.ChangesAsync(projectPath, alias, ct);
            return Ok(new { c.Alias, c.IsGit, c.Branch, files = c.Files.Select(f => new { status = f.Status, path = f.Path }) });
        }, projectPath);

        /// <summary>GET /api/coderoots/reminder?projectPath — le radici non utilizzabili qui da ricordare all'apertura.</summary>
        [HttpGet("reminder")]
        public Task<IActionResult> Reminder([FromQuery] string projectPath, CancellationToken ct) => Guarded(async () =>
            Ok((await _roots.PendingReminderAsync(projectPath, ct)).Select(View)), projectPath);

        /// <summary>POST /api/coderoots/reminder/dismiss {projectPath, aliases} — «non ricordarmelo più» per questi alias.</summary>
        [HttpPost("reminder/dismiss")]
        public Task<IActionResult> DismissReminder([FromBody] DismissRequest request) => Guarded(() =>
        {
            _roots.DismissReminder(request.ProjectPath, request.Aliases);
            return Task.FromResult<IActionResult>(Ok(new { dismissed = request.Aliases }));
        }, request?.ProjectPath);

        private static object View(CodeRootState s) => new
        {
            s.Alias, s.Repository, s.Subpath, s.Tags, s.Languages, s.LocalPath, s.RootPath,
            status = s.Status.ToString(), s.Reason, s.ActualRemotes,
        };

        private async Task<IActionResult> Guarded(Func<Task<IActionResult>> action, string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return BadRequest(new { error = $"projectPath non esiste: {projectPath}" });
            try { return await action(); }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or FormatException)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
