using MdExplorer.Features.GIT;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MdExplorer.Service.Controllers.GIT
{
    [ApiController]
    [Route("/api/gitservice/tags")]
    public class TagsController : ControllerBase
    {
        private readonly IGitService _gitService;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public TagsController(IGitService gitService,
            FileSystemWatcher fileSystemWatcher)
        {
            _gitService = gitService;
            _fileSystemWatcher = fileSystemWatcher;
        }

        [HttpGet]
        public IActionResult GetTagList()
        {
            var toReturn = _gitService.GetTagList(_fileSystemWatcher.Path);
            return Ok(toReturn);
        }
    }
}
