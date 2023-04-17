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
        [HttpPost("pull")]
        public IActionResult Pull(PullInfo pullInfo)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            pullInfo.ProjectPath = _fileSystemWatcher.Path ;
            (bool isConnectionMissing,
                bool isAuthenticationMissing,
                bool thereAreConflicts) = _gitService.Pull(pullInfo);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new { isConnectionMissing = isConnectionMissing, 
                            isAuthenticationMissing= isAuthenticationMissing,
                            thereAreConflicts = thereAreConflicts
            });
        }
        [HttpGet("commitandpush")]
        public IActionResult CommitAndPush()
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var commitAndPushInfo = new PullInfo { ProjectPath = _fileSystemWatcher.Path };
            (bool isConnectionMissing,
               bool isAuthenticationMissing,
               bool thereAreConflicts) = _gitService.CommitAndPush(commitAndPushInfo);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new
            {
                isConnectionMissing = isConnectionMissing,
                isAuthenticationMissing = isAuthenticationMissing,
                thereAreConflicts = thereAreConflicts
            });
        }
    }
}
