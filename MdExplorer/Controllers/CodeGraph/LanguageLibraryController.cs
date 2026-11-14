using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.CodeGraph
{
    /// <summary>
    /// La libreria dei linguaggi del grafo (12/11/2026): i pacchetti si caricano una volta sola, nelle impostazioni generali,
    /// e si attivano nei progetti che li usano, nelle impostazioni del progetto (<see cref="LanguageLibrary"/>).
    /// </summary>
    [ApiController]
    [Route("api/language-library")]
    public class LanguageLibraryController : ControllerBase
    {
        private readonly MdExplorer.Services.CodeGraph.ICodeAnalysisService _analysis;
        private readonly ILogger<LanguageLibraryController> _logger;

        public LanguageLibraryController(MdExplorer.Services.CodeGraph.ICodeAnalysisService analysis, ILogger<LanguageLibraryController> logger)
        {
            _analysis = analysis;
            _logger = logger;
        }

        /// <summary>La cartella della libreria, accanto agli altri dati di MdExplorer su questo computer.</summary>
        public static string Root => Path.Combine(CrossPlatformPath.GetAppDataPath(), LanguageLibrary.FolderName);

        public class SourceRequest { public string SourcePath { get; set; } public bool Replace { get; set; } }
        public class IdRequest { public string Id { get; set; } }
        public class ProjectIdRequest { public string ProjectPath { get; set; } public string Id { get; set; } public bool Replace { get; set; } }

        /// <summary>GET /api/language-library — i pacchetti della libreria, anche quelli che non si leggono (con il motivo).</summary>
        [HttpGet]
        public IActionResult List() => Ok(new { folder = Root, packages = LanguageLibrary.List(Root) });

        /// <summary>POST /api/language-library/import {sourcePath, replace} — un pacchetto (cartella o .zip) nella libreria; 409 = c'è già, con le due versioni.</summary>
        [HttpPost("import")]
        public IActionResult Import([FromBody] SourceRequest request) => Guarded(() =>
        {
            var r = LanguageLibrary.Import(Root, request?.SourcePath, request?.Replace == true);
            _logger.LogInformation("[LanguageLibrary] '{Id}' v{Version} nella libreria da {Source}{Replaced}", r.Id, r.Version, request?.SourcePath, r.Replaced ? " (sostituito)" : "");
            return Ok(new { id = r.Id, displayName = r.DisplayName, version = r.Version, replaced = r.Replaced, skills = r.Skills });
        });

        /// <summary>POST /api/language-library/remove {id} — via dalla libreria; i progetti che lo hanno attivo lo tengono.</summary>
        [HttpPost("remove")]
        public IActionResult Remove([FromBody] IdRequest request) => Guarded(() =>
        {
            var removed = LanguageLibrary.Remove(Root, request?.Id);
            if (removed) _logger.LogInformation("[LanguageLibrary] '{Id}' tolto dalla libreria", request.Id);
            return Ok(new { removed });
        });

        /// <summary>GET /api/language-library/project?projectPath — libreria e progetto insieme: attivo, versioni, chi lo usa.</summary>
        [HttpGet("project")]
        public async Task<IActionResult> ForProject([FromQuery] string projectPath, CancellationToken ct)
        {
            var bad = ValidateProject(projectPath); if (bad != null) return bad;
            var used = await UsedByAsync(projectPath, ct);
            return Ok(new { folder = Root, packages = LanguageLibrary.ForProject(Root, projectPath, id => used.TryGetValue(id, out var l) ? l : Enumerable.Empty<string>()) });
        }

        /// <summary>POST /api/language-library/activate {projectPath, id, replace} — dalla libreria al progetto (anche per aggiornarlo), con le sue skill.</summary>
        [HttpPost("activate")]
        public IActionResult Activate([FromBody] ProjectIdRequest request) => Guarded(() =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            var r = LanguageLibrary.Activate(Root, request.ProjectPath, request.Id, request.Replace);
            var skills = SyncSkills(request.ProjectPath);
            _logger.LogInformation("[LanguageLibrary] '{Id}' v{Version} attivato in {Project}{Replaced}; skill: {Skills}", r.Id, r.Version, request.ProjectPath, r.Replaced ? " (aggiornato)" : "", skills.Count == 0 ? "nessuna" : string.Join("; ", skills));
            return Ok(new { id = r.Id, displayName = r.DisplayName, version = r.Version, replaced = r.Replaced, installed = skills });
        });

        /// <summary>POST /api/language-library/deactivate {projectPath, id} — via dal progetto, con le sue skill; rifiutato se una cartella lo usa.</summary>
        [HttpPost("deactivate")]
        public async Task<IActionResult> Deactivate([FromBody] ProjectIdRequest request, CancellationToken ct)
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            var used = await UsedByAsync(request.ProjectPath, ct);
            return Guarded(() =>
            {
                LanguageLibrary.Deactivate(request.ProjectPath, request.Id, used.TryGetValue(request.Id ?? "", out var l) ? l : new List<string>());
                var skills = SyncSkills(request.ProjectPath);
                _logger.LogInformation("[LanguageLibrary] '{Id}' disattivato in {Project}; skill: {Skills}", request.Id, request.ProjectPath, skills.Count == 0 ? "nessuna" : string.Join("; ", skills));
                return Ok(new { deactivated = true, removed = skills });
            });
        }

        /// <summary>POST /api/language-library/adopt {projectPath, id, replace} — un pacchetto arrivato con il progetto entra nella libreria.</summary>
        [HttpPost("adopt")]
        public IActionResult Adopt([FromBody] ProjectIdRequest request) => Guarded(() =>
        {
            var bad = ValidateProject(request?.ProjectPath); if (bad != null) return bad;
            var r = LanguageLibrary.Adopt(Root, request.ProjectPath, request.Id, request.Replace);
            _logger.LogInformation("[LanguageLibrary] '{Id}' v{Version} dal progetto {Project} alla libreria", r.Id, r.Version, request.ProjectPath);
            return Ok(new { id = r.Id, displayName = r.DisplayName, version = r.Version, replaced = r.Replaced });
        });

        /// <summary>Le skill dei pacchetti allineate nei motori che il progetto ha, come dopo «Importa linguaggio…».</summary>
        private static List<string> SyncSkills(string projectPath)
        {
            var log = new List<string>();
            foreach (var layout in new[] { HarnessLayout.Claude, HarnessLayout.Copilot, HarnessLayout.OpenCode })
                if (Directory.Exists(Path.Combine(projectPath, layout.SkillsFolder.Replace('/', Path.DirectorySeparatorChar))))
                    log.AddRange(LanguageSkillInstaller.Sync(projectPath, layout));
            return log;
        }

        /// <summary>Per linguaggio, le cartelle del progetto che lo hanno scelto.</summary>
        private async Task<Dictionary<string, List<string>>> UsedByAsync(string projectPath, CancellationToken ct)
        {
            var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in await _analysis.ListProgramsAsync(projectPath, ct))
                foreach (var lang in p.Languages ?? new List<string>())
                {
                    if (!map.TryGetValue(lang, out var list)) map[lang] = list = new List<string>();
                    if (!list.Contains(p.Folder)) list.Add(p.Folder);
                }
            return map;
        }

        private IActionResult Guarded(Func<IActionResult> action)
        {
            try { return action(); }
            catch (LanguagePackageExistsException ex)
            {
                return Conflict(new { error = ex.Message, id = ex.Id, existingVersion = ex.ExistingVersion, importedVersion = ex.ImportedVersion });
            }
            catch (LanguageDefinitionException ex) { return BadRequest(new { error = ex.Message }); }
            catch (Exception ex) when (ex is IOException or ArgumentException or UnauthorizedAccessException or InvalidDataException or InvalidOperationException)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private IActionResult ValidateProject(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = UiText.Say("projectPath è obbligatorio", "projectPath is required") });
            if (!Directory.Exists(projectPath))
                return BadRequest(new { error = UiText.Say($"projectPath non esiste: {projectPath}", $"projectPath does not exist: {projectPath}") });
            return null;
        }
    }
}
