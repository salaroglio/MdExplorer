using FluentMigrator.Infrastructure;
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
            var areCredentialsCorrect = _gitService.CloneRepository(request);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new { areCredentialsCorrect = areCredentialsCorrect, message = "done" });
        }
        [HttpPost("pull")]
        public IActionResult Pull(PullInfo pullInfo)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            pullInfo.ProjectPath = _fileSystemWatcher.Path ;
            (bool isConnectionMissing,
                bool isAuthenticationMissing,
                bool thereAreConflicts,
                string errorMessage) = _gitService.Pull(pullInfo);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new { isConnectionMissing = isConnectionMissing, 
                            isAuthenticationMissing= isAuthenticationMissing,
                            thereAreConflicts = thereAreConflicts,
                            errorMessage = errorMessage
            });
        }
        [HttpPost("commitandpush")]
        public IActionResult CommitAndPush(PullInfo pullInfo)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
             pullInfo.ProjectPath = _fileSystemWatcher.Path;
            (bool isConnectionMissing,
               bool isAuthenticationMissing,
               bool thereAreConflicts,
               string errorMessage) = _gitService.CommitAndPush(pullInfo);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new
            {
                isConnectionMissing = isConnectionMissing,
                isAuthenticationMissing = isAuthenticationMissing,
                thereAreConflicts = thereAreConflicts,
                errorMessage = errorMessage
            });
        }

        [HttpPost("commit")]
        public IActionResult Commit(PullInfo pullInfo)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            pullInfo.ProjectPath = _fileSystemWatcher.Path;
            (bool isConnectionMissing,
               bool isAuthenticationMissing,
               bool thereAreConflicts,
               string errorMessage) = _gitService.Commit(pullInfo);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new
            {
                isConnectionMissing = isConnectionMissing,
                isAuthenticationMissing = isAuthenticationMissing,
                thereAreConflicts = thereAreConflicts,
                errorMessage = errorMessage
            });
        }

        [HttpPost("push")]
        public IActionResult Push(PullInfo pullInfo)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            pullInfo.ProjectPath = _fileSystemWatcher.Path;
            (bool isConnectionMissing,
               bool isAuthenticationMissing,
               bool thereAreConflicts,
               string errorMessage) = _gitService.Push(pullInfo);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new
            {
                isConnectionMissing = isConnectionMissing,
                isAuthenticationMissing = isAuthenticationMissing,
                thereAreConflicts = thereAreConflicts,
                errorMessage = errorMessage
            });
        }
    }
}
