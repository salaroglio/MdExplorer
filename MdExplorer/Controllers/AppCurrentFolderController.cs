using MdExplorer.Abstractions.DB;
using MdExplorer.Hubs;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/AppCurrentFolder")]
    public class AppCurrentFolderController : MdControllerBase<AppCurrentFolderController>
    {
        public AppCurrentFolderController(
            ILogger<AppCurrentFolderController> logger,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            IDatabaseManager databaseManager = null)
            : base(logger, options, hubContext, userSettingsDB, engineDB,
                  databaseManager: databaseManager)
        {
        }

        [HttpGet]
        public IActionResult GetCurrentFolder()
        {
            var currentFolder = GetProjectPath();
            string lastFolder = Path.GetFileName(currentFolder);
            return Ok(new { currentFolder = lastFolder });
        }
    }
}
