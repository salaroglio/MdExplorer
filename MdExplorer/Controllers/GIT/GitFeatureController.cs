using FluentMigrator.Infrastructure;
using FluentNHibernate.Utils;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Models.GIT;
using MdExplorer.Features.GIT;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Ad.Tools.Dal.Extensions;
using System.Linq;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;
using MdExplorer.Service.Models;
using Microsoft.Extensions.Options;
using MdExplorer.Controllers;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Service.Controllers.GIT.models;
using System.Collections.Generic;

namespace MdExplorer.Service.Controllers.GIT
{
    [ApiController]
    [Route("/api/gitfeatures/")]
    public class GitFeatureController:MdControllerBase<GitFeatureController>
    {
        private readonly IGitService _gitService;                     
        
        
        
        
        public GitFeatureController(IGitService gitService,
        FileSystemWatcher fileSystemWatcher,        
        IOptions<MdExplorerAppSettings> options,
        ILogger<GitFeatureController> logger,
        IHubContext<MonitorMDHub> hubContext,
        IUserSettingsDB session,
        ICommandRunnerHtml commandRunner,
         IHelper helper,
         IWorkLink[] modifiers,
        IEngineDB engineDB) : base(logger, fileSystemWatcher, options, 
            hubContext, session, engineDB, commandRunner, 
            modifiers, helper)
        {
            _gitService = gitService;            
            
            
           
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

            // Check if there are files to be changed
            var whatFilesWillBeChanged = _gitService.GetFilesAndAuthorsToBeChanged(_fileSystemWatcher.Path);

            pullInfo.ProjectPath = _fileSystemWatcher.Path;

            // PULL
            (bool isConnectionMissing,
                bool isAuthenticationMissing,
                bool thereAreConflicts,
                string errorMessage) = _gitService.Pull(pullInfo);

            // Refresh the database
            var relDal = _engineDB.GetDal<MarkdownFile>();
            foreach (var item in whatFilesWillBeChanged)
            {
                // Refresh database
                
                var mdFile = relDal.GetList().Where(_ => _.Path == item.FullPath).FirstOrDefault();
                _engineDB.BeginTransaction();
                if (mdFile == null)
                {
                    var markdownFile = new MarkdownFile
                    {
                        FileName = Path.GetFileName(item.FullPath),
                        Path = item.FullPath,
                        FileType = "File"
                    };
                    relDal.Save(markdownFile);
                }

                SaveLinksFromMarkdown(mdFile);
                _engineDB.Commit();
            }

            // generate the new array of data
            var filesToBeChangedMdo = new List<FileToBeChangedMdo>();
            foreach (var item in whatFilesWillBeChanged)
            {
                var fileToFullPath = new FileToBeChangedMdo
                {
                    Author = item.Author,
                    FileName = item.FileName,
                    FullPath = item.FullPath,
                    Status = item.Status,                    
                };

                filesToBeChangedMdo.Add(fileToFullPath);
            }

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new { isConnectionMissing = isConnectionMissing, 
                            isAuthenticationMissing= isAuthenticationMissing,
                            thereAreConflicts = thereAreConflicts,
                            errorMessage = errorMessage,
                            whatFilesWillBeChanged = filesToBeChangedMdo
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
