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

            // Open with selected IDE
            if (selectedIde?.ToLowerInvariant() == "intellij")
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
