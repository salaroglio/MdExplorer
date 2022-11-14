using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace MdExplorer.Service.Controllers.MdPublish
{
    [ApiController]
    [Route("api/MdPublish")]
    public class MdPublishController:ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;

        public MdPublishController(
            FileSystemWatcher fileSystemWatcher
            )
        {
            _fileSystemWatcher = fileSystemWatcher;
        }

        [HttpGet]
        public IActionResult GetPublishDocuments([FromQuery] string path, string level)
        {
            var publishBaseFolder =  $"{_fileSystemWatcher.Path}{Path.DirectorySeparatorChar}mdPublish";
            Directory.CreateDirectory(publishBaseFolder);

            var currentPath = path == "root" ? publishBaseFolder : path;
            var currentLevel = Convert.ToInt32(level);
            var list = new List<IFileInfoNode>();                        

            foreach (var itemFolder in Directory.GetDirectories(currentPath))
            {
                var node = new FileInfoNode
                {
                    Name = Path.GetFileName(itemFolder),
                    FullPath = itemFolder,
                    Path = itemFolder,
                    Level = currentLevel,
                    Type = "publishFolder",
                    Expandable = Directory.GetDirectories(itemFolder).Count() > 0
                };
                list.Add(node);
            }
            foreach (var itemFolder in Directory.GetFiles(currentPath))
            {
                var node = new FileInfoNode
                {
                    Name = Path.GetFileName(itemFolder),
                    FullPath = itemFolder,
                    Path = itemFolder,
                    Level = currentLevel,
                    Type = "mdFile",
                    Expandable = Directory.GetDirectories(itemFolder).Count() > 0
                };
                list.Add(node);
            }

            return Ok(list);
        }

    }
}
