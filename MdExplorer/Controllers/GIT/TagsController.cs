using MdExplorer.Abstractions.DB;
using MdExplorer.Features.GIT;
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
using System.Text;

namespace MdExplorer.Service.Controllers.GIT
{
    [ApiController]
    [Route("/api/gitservice/tags")]
    public class TagsController : MdControllerBase<TagsController>
    {
        private readonly IGitService _gitService;

        public TagsController(
            IGitService gitService,
            ILogger<TagsController> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            IDatabaseManager databaseManager = null)
            : base(logger, fileSystemWatcher, options, hubContext, userSettingsDB, engineDB,
                  databaseManager: databaseManager)
        {
            _gitService = gitService;
        }

        [HttpGet]
        public IActionResult GetTagList()
        {
            var toReturn = _gitService.GetTagList(GetProjectPath());
            return Ok(toReturn);
        }
    }
}
