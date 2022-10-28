using MdExplorer.Features.GIT;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("/api/gitservice/branches")]
    public class GitController : ControllerBase
    {
        private readonly IGitService _gitService;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public GitController(IGitService gitService,
            FileSystemWatcher fileSystemWatcher)
        {
            _gitService = gitService;
            _fileSystemWatcher = fileSystemWatcher;
        }
        [HttpGet("feat/GetCurrentBranch")]
        public IActionResult GetCurrentBranch()
        {
            var toReturn =_gitService.GetCurrentBranch(_fileSystemWatcher.Path);
            return Ok(new { name=toReturn });//classe branch lato angular
        }

    }
}
