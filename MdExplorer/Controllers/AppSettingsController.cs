using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Service.Utilities;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MdExplorer.Utilities;
using MdExplorer.Service.Models;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Features.Services.AI.CopilotAcp;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MdExplorer.Services.DatabaseManager;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/AppSettings/{action}")] //AppCurrentFolder
    public class AppSettingsController : MdControllerBase<AppSettingsController>
    {
        private readonly IUserSettingsDB _session;
        private readonly ProcessUtil _processUtil;

        public AppSettingsController(
                ILogger<AppSettingsController> logger,
                IOptions<MdExplorerAppSettings> options,
                IHubContext<MonitorMDHub> hubContext,
                IUserSettingsDB userSettingDB,
                IEngineDB engineDB,
                ProcessUtil processUtil,
                IDatabaseManager databaseManager = null)
            : base(logger, options, hubContext, userSettingDB, engineDB, databaseManager: databaseManager)
        {
            _session = userSettingDB;
            _processUtil = processUtil;
        }

        [HttpGet]
        public IActionResult GetCurrentFolder()
        {
            try
            {
                var currentFolder = GetProjectPath();
                // Use Path.GetFileName to get the last part of the path, cross-platform compatible
                string lastFolder = Path.GetFileName(currentFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                // If Path.GetFileName returns empty (e.g. for root paths), use the full path
                if (string.IsNullOrEmpty(lastFolder))
                {
                    lastFolder = currentFolder;
                }
                return Ok(new { currentFolder = lastFolder });
            }
            catch (InvalidOperationException)
            {
                // No project is open yet (e.g. on the Projects page at startup)
                return Ok(new { currentFolder = "" });
            }
        }

        [HttpGet]
        public IActionResult GetSettings()
        {
            var settingsDal = _session.GetDal<Setting>();
            var settings = settingsDal.GetList();
            return Ok(new { settings = settings });
        }

        [HttpPost]
        public IActionResult SetSettings([FromBody] Settings settings)
        {
            _logger.LogWarning("***** SETSETTINGS CHIAMATO *****");
            _logger.LogInformation($"[SetSettings] Received {settings?.settings?.Length ?? 0} settings to save");

            var settingsDal = _session.GetDal<Setting>();
            var allDbSettings = settingsDal.GetList().ToList();
            // Evict all loaded Setting entities to prevent NHibernate dirty-check issues
            // (migration-seeded GUIDs use text format which conflicts with NHibernate's binary GUID parameters)
            foreach (var s in allDbSettings)
                _session.Evict(s);

            _session.BeginTransaction(System.Data.IsolationLevel.Unspecified);
            foreach (var item in settings.settings)
            {
                _logger.LogInformation($"[SetSettings] Processing: Id={item.id}, Name={item.name}, ValueString={item.valueString}");

                var dbItem = allDbSettings.FirstOrDefault(_ => _.Id == item.id);

                // Fallback: search by Name if Id not found (e.g. new settings added from frontend)
                if (dbItem == null && !string.IsNullOrEmpty(item.name))
                {
                    dbItem = allDbSettings.FirstOrDefault(_ => _.Name == item.name);
                }

                if (dbItem != null)
                {
                    _logger.LogInformation($"[SetSettings] Found in DB: Id={dbItem.Id}, Name={dbItem.Name}, OldValue={dbItem.ValueString}");
                    // Use raw SQL to bypass NHibernate GUID format mismatch on migration-seeded entities
                    _session.CreateSQLQuery("UPDATE Setting SET ValueString = :val, ValueInt = :vi, ValueDecimal = :vd, ValueDateTime = :vdt WHERE Name = :name")
                        .SetParameter("val", item.valueString ?? "")
                        .SetParameter("vi", item.valueInt, NHibernateUtil.Int32)
                        .SetParameter("vd", item.valueDecimal, NHibernateUtil.Decimal)
                        .SetParameter("vdt", item.valueDateTime, NHibernateUtil.DateTime)
                        .SetParameter("name", dbItem.Name)
                        .ExecuteUpdate();
                    _logger.LogInformation($"[SetSettings] Updated: Name={dbItem.Name}, NewValue={item.valueString}");
                }
                else if (!string.IsNullOrEmpty(item.name))
                {
                    _logger.LogInformation($"[SetSettings] Creating new setting: Name={item.name}, Value={item.valueString}");
                    var newSetting = new Setting
                    {
                        Name = item.name,
                        ValueString = item.valueString,
                        ValueInt = item.valueInt,
                        ValueDecimal = item.valueDecimal,
                        ValueDateTime = item.valueDateTime
                    };
                    settingsDal.Save(newSetting); // GuidComb generates Id in correct binary format
                    _logger.LogInformation($"[SetSettings] Created: Name={newSetting.Name}, Value={newSetting.ValueString}");
                }
                else
                {
                    _logger.LogWarning($"[SetSettings] Setting not found in DB and no name provided: Id={item.id}");
                    continue;
                }
            }
            _session.Commit();
            _logger.LogInformation("[SetSettings] Transaction committed");
            return Ok(new { response = "settings saved" });
        }

        [HttpGet]
        public IActionResult OpenFile(string path)
        {
            var settingDal = _session.GetDal<Setting>();
            var projectDal = _session.GetDal<Project>();
            var projectPath = GetProjectPath();

            // Debug logging
            var connectionId = Request.Query["ConnectionId"].ToString();
            _logger.LogInformation($"[OpenFile] ConnectionId: '{connectionId}', ProjectPath: '{projectPath}', FilePath: '{path}'");

            // Read IDE selection from Project database
            string selectedIde = "vscode"; // Default to VS Code
            var project = projectDal.GetList().FirstOrDefault(p => p.Path == projectPath);

            if (project != null && !string.IsNullOrWhiteSpace(project.SelectedIde))
            {
                selectedIde = project.SelectedIde;
            }

            // --- Docker headless mode -------------------------------------------------
            // The backend runs inside a container; there is no host-side editor
            // process to spawn. Instead we translate the container path back to the
            // host path and return a "vscode://file/..." URL that the user's browser
            // will hand off to the OS, which launches VS Code natively on the host.
            // Gated by MDE_DOCKER=1 so non-container deployments are unaffected.
            if (Environment.GetEnvironmentVariable("MDE_DOCKER") == "1")
            {
                if (IsAgentCli(selectedIde))
                {
                    // Un CLI agentico ha bisogno di un terminale sulla macchina dell'utente, e non
                    // esiste uno schema di URL da passare al browser come per vscode:// e jetbrains://.
                    return BadRequest(new { error = "Il CLI dell'ambiente agentico non si può aprire in modalità Docker: serve un terminale sulla macchina." });
                }

                var hostUrl = TryBuildHostEditorUrl(path, selectedIde);
                if (hostUrl != null)
                {
                    _logger.LogInformation("[OpenFile] Docker mode → returning host URL: {Url}", hostUrl);
                    return Ok(new { openUrl = hostUrl });
                }
                // Fall through with a clear error rather than silently spawning a
                // process in the container that no one would ever see.
                return BadRequest(new { error = "Docker mode: could not translate container path to host path. Check MDE_HOST_WORKSPACE / MDE_CONTAINER_WORKSPACE env vars." });
            }

            // Open with selected IDE
            if (IsAgentCli(selectedIde))
            {
                // Quale CLI aprire NON è una scelta a sé: è l'ambiente agentico del progetto, lo
                // stesso che decide dove stanno skill e prompt e con chi parla MarkAgent. Prima
                // questa voce era "GitHub Copilot" fissa, e su un progetto Claude o opencode
                // apriva il CLI sbagliato.
                MarkAgentEngine engine;
                try
                {
                    engine = MarkAgentEngines.Resolve(project?.MarkAgentEngine, projectPath, out _);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { error = ex.Message });
                }

                if (engine == MarkAgentEngine.None)
                {
                    return BadRequest(new { error = "Questo progetto non ha un ambiente agentico: scegline uno in Impostazioni → AI & RAG, oppure seleziona un altro editor." });
                }

                var command = MarkAgentEngines.CommandOf(engine);
                if (!IsAgentCliInstalled(engine))
                {
                    return BadRequest(new { error = $"'{command}' non è installato su questa macchina, o non è nel PATH del servizio: è il CLI dell'ambiente agentico di questo progetto." });
                }

                // È un programma interattivo: si apre un terminale sulla RADICE del progetto, e il
                // file specifico non c'entra — quello che conta qui è la cartella.
                _processUtil.OpenFolderWithAgentCli(projectPath, command);
                _logger.LogInformation("[OpenFile] aperto {Command} sulla radice di {ProjectPath}", command, projectPath);
                return Ok(new { message = $"opened {command} on project root" });
            }
            else if (selectedIde?.ToLowerInvariant() == "intellij")
            {
                var intellijPath = settingDal.GetList().Where(_ => _.Name == "IntelliJPath").FirstOrDefault()?.ValueString;

                if (string.IsNullOrEmpty(intellijPath))
                {
                    return BadRequest(new { error = "IntelliJ IDEA not found. Please configure IntelliJ path in settings or run auto-discovery." });
                }

                _processUtil.OpenFileWithIntelliJ(path, intellijPath);
                return Ok(new { message = "opened with IntelliJ IDEA" });
            }
            else
            {
                var editorPath = settingDal.GetList().Where(_ => _.Name == "EditorPath").FirstOrDefault()?.ValueString;

                if (string.IsNullOrEmpty(editorPath))
                {
                    return BadRequest(new { error = "VS Code not found. Please configure the editor path in settings." });
                }

                _processUtil.OpenFileWithVisualStudioCode(path, editorPath, projectPath);
                return Ok(new { message = "opened with VS Code" });
            }
        }

        /// <summary>
        /// Valore di <c>Project.SelectedIde</c> che vuol dire «il CLI dell'ambiente agentico».
        /// </summary>
        public const string AgentCliIde = "agent-cli";

        /// <summary>
        /// ⚠️ Valore storico: fino al 21/09/2026 questa voce era «GitHub Copilot», fissa. La
        /// migrazione M2026_09_21_003 lo converte, ma un client vecchio potrebbe ancora mandarlo:
        /// lo si accetta come sinonimo, invece di far finta di non capirlo.
        /// </summary>
        private const string LegacyCopilotIde = "copilot";

        private static bool IsAgentCli(string selectedIde)
        {
            var value = selectedIde?.Trim().ToLowerInvariant();
            return value == AgentCliIde || value == LegacyCopilotIde;
        }

        /// <summary>
        /// Il CLI di questo motore è raggiungibile dal PATH del servizio?
        /// <para>
        /// ⚠️ Una scansione sola per tutti e tre, e <b>non</b>
        /// <see cref="CopilotProcessLauncher.IsResolvable"/>: quello, fuori da Windows, risponde
        /// <c>true</c> per scelta, lasciando fallire il lancio più avanti. Va bene dove il
        /// fallimento si legge; qui no — misurato il 21/09/2026 su Linux con un PATH senza
        /// <c>copilot</c>: invece del messaggio «non è installato» si apriva una finestra di
        /// terminale che diceva «command not found» e restava lì.
        /// </para>
        /// </summary>
        private static bool IsAgentCliInstalled(MarkAgentEngine engine)
            => engine != MarkAgentEngine.None
               && ProcessUtil.IsCommandInPath(MarkAgentEngines.CommandOf(engine));

        /// <summary>
        /// Docker-only: translate a container-side file path (e.g. "/workspace/test/foo.md")
        /// into a vscode:// or jetbrains:// URL pointing at the host-side equivalent
        /// (e.g. "C:/sviluppo/.../docker/workspace/test/foo.md"). Returns null if the
        /// env vars aren't set or the path doesn't start with the container prefix.
        /// </summary>
        private static string TryBuildHostEditorUrl(string containerPath, string selectedIde)
        {
            if (string.IsNullOrWhiteSpace(containerPath)) return null;

            var containerPrefix = Environment.GetEnvironmentVariable("MDE_CONTAINER_WORKSPACE") ?? "/workspace";
            var hostPrefix = Environment.GetEnvironmentVariable("MDE_HOST_WORKSPACE");
            if (string.IsNullOrWhiteSpace(hostPrefix)) return null;

            // Normalize separators on the container side (it's always forward slashes
            // inside the container, but be defensive).
            var normalized = containerPath.Replace('\\', '/');
            if (!normalized.StartsWith(containerPrefix.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(normalized, containerPrefix.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var suffix = normalized.Substring(containerPrefix.TrimEnd('/').Length); // includes leading '/'
            var hostFs = hostPrefix.Replace('\\', '/').TrimEnd('/') + suffix;       // e.g. "C:/sviluppo/.../foo.md"

            // VS Code expects "vscode://file/<absolute-path>" with forward slashes.
            // We percent-encode each segment but leave the slashes intact.
            var encodedSegments = hostFs.Split('/')
                .Select(seg => Uri.EscapeDataString(seg))
                .ToArray();
            var encoded = string.Join('/', encodedSegments);

            var ide = (selectedIde ?? "vscode").ToLowerInvariant();
            return ide switch
            {
                "intellij" => $"jetbrains://idea/navigate/reference?project=&path={Uri.EscapeDataString(hostFs)}",
                _ => $"vscode://file/{encoded}",
            };
        }

        

        [HttpGet]
        public IActionResult OpenFolder(string path)
        {
            var pathToOpen = Path.GetDirectoryName(path);
            CrossPlatformProcess.OpenFolder(pathToOpen);
            return Ok(new { message = "opened" });
        }

        [HttpGet]
        public IActionResult OpenChromePdf(string path)
        {
            // Open PDF with default application
            CrossPlatformProcess.OpenFile(path);
            
            return Ok(new { message = "opened" });
        }

        public class Settings
        {
            public SettingDto[] settings { get; set; }
        }

        public class SettingDto
        {
            public Guid id { get; set; }
            public string name { get; set; }
            public string? valueString { get; set; }
            public int? valueInt { get; set; }
            public DateTime? valueDateTime { get; set; }
            public decimal? valueDecimal { get; set; }
        }

        [HttpGet]
        public IActionResult KillServer()
        {
            Environment.Exit(0);
            return Ok(new { message = "self-destruction activated" });
        }

        [HttpGet]
        public IActionResult ShowToc(string documentPathEncoded, bool showToc)
        {
            var docPathDecoded = HttpUtility.UrlDecode(documentPathEncoded);
            
            var docSettDal = _session.GetDal<DocumentSetting>();
            var docSett = docSettDal.GetList().Where(_ => _.DocumentPath == docPathDecoded)
                .FirstOrDefault() ?? new DocumentSetting { DocumentPath = docPathDecoded};
            docSett.ShowTOC = showToc;

            _session.BeginTransaction();
            docSettDal.Save(docSett);
            _session.Commit();
            return Ok(new { message = "done" });
        }


    }
}
