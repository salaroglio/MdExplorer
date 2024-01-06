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

namespace MdExplorer.Service.Controllers.TabBar
{
    [ApiController]
    [Route("/api/tabcontroller/{*url}")]
    public class TabBarController : MdControllerBase<TabBarController>
    {
        private readonly ILogger<TabBarController> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IOptions<MdExplorerAppSettings> _options;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IUserSettingsDB _sessionDB;
        private readonly IEngineDB _engineDB;
        private readonly ICommandRunner _commandRunner;

        public TabBarController(ILogger<TabBarController> logger, 
                                    FileSystemWatcher fileSystemWatcher, 
                                    IOptions<MdExplorerAppSettings> options, 
                                    IHubContext<MonitorMDHub> hubContext, 
                                    IUserSettingsDB session, 
                                    IEngineDB engineDB,
                                    ICommandRunnerHtml commandRunner) : base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            _options = options;
            _hubContext = hubContext;
            _sessionDB = session;
            _engineDB = engineDB;
            _commandRunner = commandRunner;
        }

        [HttpGet]
        public IActionResult GetTOCData([FromQuery]string fullPathFile)
        {
            _session.BeginTransaction();
            var docSettingDal = _session.GetDal<DocumentSetting>();
            var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == fullPathFile).FirstOrDefault();
            if (currentDocSetting == null)
            {

                currentDocSetting = new DocumentSetting { DocumentPath = fullPathFile,ShowTOC=true };
                docSettingDal.Save(currentDocSetting);
            }
            _session.Commit();
            return Ok( currentDocSetting);
        }

        [HttpPost]
        public IActionResult SaveTOCData([FromBody] DocumentSettingDto documentSetting)
        {
            _session.BeginTransaction();
            var docSettingDal = _session.GetDal<DocumentSetting>();
            var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == documentSetting.DocumentPath).FirstOrDefault();            
            if (currentDocSetting == null)
            {
                currentDocSetting = new DocumentSetting
                {
                    ShowTOC = documentSetting.ShowTOC,
                    TocLeft = documentSetting.TocLeft,
                    TocWidth = documentSetting.TocWidth,
                    DocumentPath = documentSetting.DocumentPath,
                };
            }
            else
            {
                currentDocSetting.ShowTOC = documentSetting.ShowTOC;
                currentDocSetting.TocWidth = documentSetting.TocWidth;
                currentDocSetting.TocLeft = documentSetting.TocLeft;                

            }
            docSettingDal.Save(currentDocSetting);
            _session.Commit();
            return Ok("done");
        }

    }
}
