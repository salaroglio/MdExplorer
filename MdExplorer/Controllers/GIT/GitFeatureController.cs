using FluentNHibernate.Utils;
using MdExplorer.Abstractions.Models.GIT;
using MdExplorer.Features.GIT;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace MdExplorer.Service.Controllers.GIT
{
    [ApiController]
    [Route("/api/gitfeatures/")]
    public class GitFeatureController:ControllerBase
    {
        private readonly IGitService _gitService;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public GitFeatureController(IGitService gitService, FileSystemWatcher fileSystemWatcher)
        {
            _gitService = gitService;
            _fileSystemWatcher = fileSystemWatcher;
        }

        [HttpPost("cloneRepository")]
        public IActionResult CloneRepository(CloneInfo request)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            _gitService.CloneRepository(request);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new { message = "done" });
        }
    }
}
