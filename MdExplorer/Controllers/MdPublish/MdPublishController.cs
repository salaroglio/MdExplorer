using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ProjectBody;
using MdExplorer.Hubs;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace MdExplorer.Service.Controllers.MdPublish
{
    [ApiController]
    [Route("api/mdPublishNodes")]
    public class MdPublishController : MdControllerBase<MdPublishController>
    {
        private readonly ProjectBodyEngine _projectBodyEngine;

        public MdPublishController(
            ProjectBodyEngine projectBodyEngine,
            ILogger<MdPublishController> logger,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            IDatabaseManager databaseManager = null)
            : base(logger, options, hubContext, userSettingsDB, engineDB,
                  databaseManager: databaseManager)
        {
            _projectBodyEngine = projectBodyEngine;
        }

        [HttpGet]
        public IActionResult GetPublishDocuments([FromQuery] string path, string level)
        {
            var projectPath = GetProjectPath();
            var publishBaseFolder = $"{projectPath}{Path.DirectorySeparatorChar}mdPublish";
            Directory.CreateDirectory(publishBaseFolder);
            var currentPath = path == "root" ? publishBaseFolder : path;
            var currentLevel = Convert.ToInt32(level);

            var listToReturn = new List<IFileInfoNode>();

            if (currentLevel == 0)
            {
                var nodeempty = new FileInfoNode
                {
                    Name = "Publish",
                    FullPath = currentPath,
                    Path = currentPath,
                    RelativePath = currentPath,
                    Level = currentLevel,
                    Type = "root",
                    Expandable = false
                };
                listToReturn.Add(nodeempty);
                return Ok(listToReturn);
            }



            var list = _projectBodyEngine.GetPusblishDocuments(currentPath, currentLevel, projectPath);
            listToReturn.AddRange(list);
            return Ok(listToReturn);
        }

    }
}
