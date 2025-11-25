using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Controllers;
using MdExplorer.Features.Commands;
using MdExplorer.Hubs;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using System.Linq;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Service.Controllers.TabBar.Automapper;
using System.Collections.Generic;
using AutoMapper;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Utilities;
using MdExplorer.Services.DatabaseManager;

namespace MdExplorer.Service.Controllers.TabBar
{
    [ApiController]
    [Route("/api/tabcontroller/{action}")]
    public class TabBarController : MdControllerBase<TabBarController>
    {
        private readonly ILogger<TabBarController> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IMapper _mapper;
        private readonly IOptions<MdExplorerAppSettings> _options;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IUserSettingsDB _sessionDB;
        private readonly ICommandRunner _commandRunner;

        public TabBarController(ILogger<TabBarController> logger,
                                    FileSystemWatcher fileSystemWatcher,
                                    IMapper mapper,
                                    IOptions<MdExplorerAppSettings> options,
                                    IHubContext<MonitorMDHub> hubContext,
                                    IUserSettingsDB session,
                                    IEngineDB engineDB,
                                    IWorkLink[] modifiers,
                                    IHelper helper,
                                    ICommandRunnerHtml commandRunner,
                                    IDatabaseManager databaseManager = null) : base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner,modifiers,helper, databaseManager)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            _mapper = mapper;
            _options = options;
            _hubContext = hubContext;
            _sessionDB = session;
            _commandRunner = commandRunner;
        }

        [HttpGet]
        public IActionResult GetTOCData([FromQuery]string fullPathFile)
        {
            _userSettingsDB.BeginTransaction();
            var docSettingDal = _userSettingsDB.GetDal<DocumentSetting>();
            var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == fullPathFile).FirstOrDefault();
            if (currentDocSetting == null)
            {

                currentDocSetting = new DocumentSetting { DocumentPath = fullPathFile,ShowTOC=true };
                docSettingDal.Save(currentDocSetting);
            }
            _userSettingsDB.Commit();
            return Ok( currentDocSetting);
        }

        [HttpPost]
        public IActionResult SaveTOCData([FromBody] DocumentSettingDto documentSetting)
        {
            _userSettingsDB.BeginTransaction();
            var docSettingDal = _userSettingsDB.GetDal<DocumentSetting>();
            var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == documentSetting.DocumentPath).FirstOrDefault();            
            if (currentDocSetting == null)
            {
                currentDocSetting = new DocumentSetting
                {
                    ShowTOC = documentSetting.ShowTOC,
                    RefsWidth = documentSetting.RefsWidth,
                    TocWidth = documentSetting.TocWidth,
                    DocumentPath = documentSetting.DocumentPath,
                    ShowRefs = documentSetting.ShowRefs,
                };
            }
            else
            {
                currentDocSetting.ShowTOC = documentSetting.ShowTOC;
                currentDocSetting.TocWidth = documentSetting.TocWidth;
                currentDocSetting.RefsWidth = documentSetting.RefsWidth;
                currentDocSetting.ShowRefs = documentSetting.ShowRefs;

            }
            docSettingDal.Save(currentDocSetting);
            _userSettingsDB.Commit();
            return Ok("done");
        }

      

        [HttpGet]
        public IActionResult GetRefsData([FromQuery] string fullPathFile)
        {
            //_session.BeginTransaction();
            _logger.LogInformation($"[GetRefsData] Searching references for: '{fullPathFile}'");

            // Normalize the path to remove double backslashes
            var normalizedPath = fullPathFile.Replace("\\\\", "\\");
            _logger.LogInformation($"[GetRefsData] Normalized path: '{normalizedPath}'");

            var docLinkInsideMarkdownDal = GetEngineDB().GetDal<LinkInsideMarkdown>();
            var allLinks = docLinkInsideMarkdownDal.GetList().ToList();

            _logger.LogInformation($"[GetRefsData] Total links in database: {allLinks.Count}");

            // Log PlantUML links specifically
            var plantumlLinks = allLinks.Where(_ => _.Source == "WorkLinkFromPlantuml").ToList();
            _logger.LogInformation($"[GetRefsData] PlantUML links in database: {plantumlLinks.Count}");
            foreach (var link in plantumlLinks)
            {
                _logger.LogInformation($"[GetRefsData] PlantUML link - FullPath: '{link.FullPath}', From file: '{link.MarkdownFile?.Path}'");
            }

            // Log first 5 links for debugging
            foreach (var link in allLinks.Take(5))
            {
                _logger.LogInformation($"[GetRefsData] Sample link - FullPath: '{link.FullPath}', Source: '{link.Source}'");
            }

            var links = allLinks.Where(_ => _.FullPath.Contains(normalizedPath)).ToList();

            _logger.LogInformation($"[GetRefsData] Found {links.Count} matching references");
            foreach (var link in links)
            {
                _logger.LogInformation($"[GetRefsData] Match - FullPath: '{link.FullPath}', Source: '{link.Source}', File: '{link.MarkdownFile?.FileName}'");
            }

            var linkDtoList = _mapper.Map<List<LinkInsideMarkdownDto>>(links);
            //_session.Commit();
            return Ok(linkDtoList);
        }

    }
}
