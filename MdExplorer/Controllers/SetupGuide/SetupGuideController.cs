using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI.AgenticEnvironments;
using MdExplorer.Features.Services.AI.CopilotSdk;
using MdExplorer.Features.Utilities;
using MdExplorer.Services.Java;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.SetupGuide
{
    /// <summary>
    /// La pagina «Che cosa installare» (19/11/2026): alla prima installazione, prima del tour di Mark, dice se ci sono le tre
    /// cose che MDE non porta con sé — un motore per MarkAgent, Java 21, Pandoc — e come installarle.
    /// </summary>
    [ApiController]
    [Route("api/SetupGuide/{action}")]
    public class SetupGuideController : ControllerBase
    {
        private const string ShownSettingName = "SetupGuideShown";

        private readonly IUserSettingsDB _session;
        private readonly IAgenticEnvironmentProbe _harnessProbe;
        private readonly JavaHealthService _javaHealth;
        private readonly ILogger<SetupGuideController> _logger;

        public SetupGuideController(IUserSettingsDB session, IAgenticEnvironmentProbe harnessProbe, JavaHealthService javaHealth, ILogger<SetupGuideController> logger)
        {
            _session = session;
            _harnessProbe = harnessProbe;
            _javaHealth = javaHealth;
            _logger = logger;
        }

        /// <summary>GET api/SetupGuide/Shown — {shown}: se la pagina si è già aperta da sola. Veloce: nessun controllo.</summary>
        [HttpGet]
        public IActionResult Shown()
        {
            _session.BeginTransaction();
            try
            {
                var shown = _session.GetDal<Setting>().GetList().FirstOrDefault(x => x.Name == ShownSettingName)?.ValueInt == 1;
                _session.Commit();
                return Ok(new { shown });
            }
            catch
            {
                _session.Rollback();
                throw;
            }
        }

        /// <summary>POST api/SetupGuide/MarkShown — la pagina si è aperta da sola ed è stata chiusa: non si riapre più da sola.</summary>
        [HttpPost]
        public IActionResult MarkShown()
        {
            _session.BeginTransaction();
            try
            {
                var dal = _session.GetDal<Setting>();
                var existing = dal.GetList().FirstOrDefault(x => x.Name == ShownSettingName);
                if (existing != null)
                {
                    _session.Evict(existing);
                    // SQL per nome, come le altre righe di Setting: un Id scritto da una migrazione ha un altro formato di GUID.
                    _session.CreateSQLQuery("UPDATE Setting SET ValueInt = 1 WHERE Name = :name").SetParameter("name", ShownSettingName).ExecuteUpdate();
                }
                else
                {
                    dal.Save(new Setting { Name = ShownSettingName, Description = "La pagina «Che cosa installare» si è già aperta da sola", ValueInt = 1 });
                }
                _session.Commit();
            }
            catch
            {
                _session.Rollback();
                throw;
            }
            return Ok(new { shown = true });
        }

        /// <summary>
        /// GET api/SetupGuide/Status — i tre controlli insieme: motori di MarkAgent, Java, Pandoc. Qualche secondo (il probe dei
        /// motori aspetta fino a 20 s un CLI lento). Il controllo di Java è quello di salute: un Java 21 appena installato
        /// prende il posto di un JavaPath automatico non adatto.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Status(CancellationToken ct)
        {
            var harnessTask = _harnessProbe.ProbeAsync(ct);
            var pandocTask = PandocProbe.ProbeAsync(ct);
            var javaTask = Task.Run(() => _javaHealth.Run(), ct);

            object harness, java, pandoc;
            try
            {
                var environments = await harnessTask;
                harness = new
                {
                    environments = environments.Select(e => new { e.Id, e.Installed, e.Usable, e.Reason, e.Detail }),
                    usable = environments.Where(e => e.Usable).Select(e => e.Id).ToArray(),
                };
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SetupGuide] controllo dei motori fallito");
                harness = new { error = UiText.Say($"Il controllo dei motori non è riuscito: {ex.Message}", $"Checking the engines failed: {ex.Message}") };
            }

            try
            {
                var r = await javaTask;
                java = new
                {
                    ok = r.Outcome == JavaHealthOutcome.Ok || r.Outcome == JavaHealthOutcome.Replaced,
                    outcome = r.Outcome.ToString(),
                    path = r.EffectivePath,
                    major = r.Outcome == JavaHealthOutcome.Replaced ? r.BestMajor : r.ConfiguredMajor,
                    r.BestPath,
                    r.BestMajor,
                    r.ConfiguredPath,
                    minimum = r.MinimumMajor,
                };
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SetupGuide] controllo di Java fallito");
                java = new { error = UiText.Say($"Il controllo di Java non è riuscito: {ex.Message}", $"Checking Java failed: {ex.Message}"), minimum = JavaHealthService.MinimumJavaMajor };
            }

            try
            {
                pandoc = await pandocTask;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SetupGuide] controllo di Pandoc fallito");
                pandoc = new PandocStatus { Error = UiText.Say($"Il controllo di Pandoc non è riuscito: {ex.Message}", $"Checking Pandoc failed: {ex.Message}") };
            }

            var os = OperatingSystem.IsWindows() ? "windows" : OperatingSystem.IsMacOS() ? "macos" : "linux";
            return Ok(new { os, copilotCliVersion = CopilotCliCompatibility.ExpectedCliVersion, harness, java, pandoc });
        }
    }
}
