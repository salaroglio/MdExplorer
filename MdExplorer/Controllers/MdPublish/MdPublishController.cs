using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using MdExplorer.Features.ProjectBody;

namespace MdExplorer.Service.Controllers.MdPublish
{
    [ApiController]
    [Route("api/mdPublishNodes")]
    public class MdPublishController:ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly ProjectBodyEngine _projectBodyEngine;

        public MdPublishController(
            FileSystemWatcher fileSystemWatcher,
            ProjectBodyEngine projectBodyEngine
            )
        {
            _fileSystemWatcher = fileSystemWatcher;
            _projectBodyEngine = projectBodyEngine;
        }

        [HttpGet]
        public IActionResult GetPublishDocuments([FromQuery] string path, string level)
        {
            var publishBaseFolder =  $"{_fileSystemWatcher.Path}{Path.DirectorySeparatorChar}mdPublish";
            Directory.CreateDirectory(publishBaseFolder);

            var currentPath = path == "root" ? publishBaseFolder : path;
            var currentLevel = Convert.ToInt32(level);
            var list = _projectBodyEngine.GetPusblishDocuments(currentPath, currentLevel);

            return Ok(list);
        }

    }
}
