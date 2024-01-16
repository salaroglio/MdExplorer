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
        private readonly IEngineDB _engineDB;
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
                                    ICommandRunnerHtml commandRunner) : base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner,modifiers,helper)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            _mapper = mapper;
            _options = options;
            _hubContext = hubContext;
            _sessionDB = session;
            _engineDB = engineDB;
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
                    TocLeft = documentSetting.TocLeft,
                    TocWidth = documentSetting.TocWidth,
                    DocumentPath = documentSetting.DocumentPath,
                    ShowRefs = documentSetting.ShowRefs,
                };
            }
            else
            {
                currentDocSetting.ShowTOC = documentSetting.ShowTOC;
                currentDocSetting.TocWidth = documentSetting.TocWidth;
                currentDocSetting.TocLeft = documentSetting.TocLeft;
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
            var docLinkInsideMarkdownDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var links = docLinkInsideMarkdownDal.GetList().Where(_ => _.FullPath.Contains(fullPathFile)).ToList();
            
            var linkDtoList = _mapper.Map<List<LinkInsideMarkdownDto>>(links);            
            //_session.Commit();
            return Ok(linkDtoList);
        }

    }
}
