using FluentMigrator.Infrastructure;
using FluentNHibernate.Utils;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Models.GIT;
using MdExplorer.Features.GIT;
using Microsoft.AspNetCore.Mvc;
using System;
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

using System.Collections.Generic;
using MdExplorer.Features.GIT.models;
using MdExplorer.Service.Controllers.GIT.models;
using MdExplorer.Abstractions.Models;
using MdExplorer.Services.DatabaseManager;

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
        IEngineDB engineDB,
        IDatabaseManager databaseManager = null) : base(logger, fileSystemWatcher, options,
            hubContext, session, engineDB, commandRunner,
            modifiers, helper, databaseManager)
        {
            _gitService = gitService;            
        }

        [HttpPost("cloneRepository")]
        [Obsolete("This endpoint is deprecated. Use ModernGitToolbar for SSH-based operations.")]
        public IActionResult CloneRepository(CloneInfo request)
        {
            SetFileSystemWatcherEnabled(false);
            var areCredentialsCorrect = _gitService.CloneRepository(request);
            SetFileSystemWatcherEnabled(true);
            return Ok(new { areCredentialsCorrect = areCredentialsCorrect, message = "done" });
        }

        [HttpPost("pull")]
        [Obsolete("This endpoint is deprecated. Use ModernGitToolbar/pull for SSH-based operations.")]
        public IActionResult Pull(PullInfo pullInfo)
        {
            SetFileSystemWatcherEnabled(false);


            var filesToBeChanged = _gitService.CheckExistenceAccountAndGetFilesAndAuthorsToBeChanged(GetProjectPath(), pullInfo);
            var filesMdo = filesToBeChanged.Select(_ => {
                var myData = new FilesAndAuthorsChangedMdo
                {
                    Author = _.Author,
                    FileName = _.FileName,
                    FullPath = _.FullPath,
                    RelativePath = _.RelativePath,
                    Status = _.Status
                };
                return myData;
                
            }).ToList();

            foreach (var fileMdo in filesMdo)
            {
                var splittedFullPath = fileMdo.FullPath.Replace(GetProjectPath(), string.Empty, StringComparison.OrdinalIgnoreCase).Split("\\", System.StringSplitOptions.RemoveEmptyEntries).ToList();
                var currentPathName = string.Empty;
                var currentLevel = 0;
                foreach (var item in splittedFullPath)
                {
                    currentPathName += "\\" + item;
                    var myNewMd = new FileInfoNode
                    {
                        Name = item,
                        FullPath = GetProjectPath() + currentPathName,
                        Level = currentLevel,
                        Path = currentPathName,
                        RelativePath = currentPathName,
                        Expandable = true,

                    };
                    if (item != splittedFullPath.Last())
                    {
                        myNewMd.Type = "folder";
                    }
                    else
                    {
                        myNewMd.Type = "mdFile";
                    }

                    fileMdo.MdFiles.Add(myNewMd);
                    currentLevel++;
                }
            }
            

            

            // prepare multiple data for client


            pullInfo.ProjectPath = GetProjectPath();

            var pullResult = _gitService.Pull(pullInfo);
            RefreshDatabase(filesToBeChanged);            

            SetFileSystemWatcherEnabled(true);
            return Ok(new
            {
                isConnectionMissing = pullResult.IsConnectionMissing,
                isAuthenticationMissing = pullResult.IsAuthenticationMissing,
                thereAreConflicts = pullResult.ThereAreConflicts,
                errorMessage = pullResult.ErrorMessage,
                whatFilesWillBeChanged = filesMdo
            });
        }

       

 


        private void RefreshDatabase(IEnumerable<FileNameAndAuthor> filesToBeChanged)
        {
            var relDal = GetEngineDB().GetDal<MarkdownFile>();
            foreach (var item in filesToBeChanged)
            {
                var mdFile = relDal.GetList().FirstOrDefault(_ => _.Path == item.FullPath);
                GetEngineDB().BeginTransaction();
                if (mdFile == null)
                {
                    relDal.Save(new MarkdownFile
                    {
                        FileName = Path.GetFileName(item.FullPath),
                        Path = item.FullPath,
                        FileType = "File"
                    });
                }
                else
                {
                    SaveLinksFromMarkdown(mdFile);
                }
                GetEngineDB().Commit();
            }
        }

        


       


        [HttpPost("commitandpush")]
        [Obsolete("This endpoint is deprecated. Use ModernGitToolbar/commit-and-push for SSH-based operations.")]
        public IActionResult CommitAndPush(PullInfo pullInfo)
        {
            SetFileSystemWatcherEnabled(false);
             pullInfo.ProjectPath = GetProjectPath();
            (bool isConnectionMissing,
               bool isAuthenticationMissing,
               bool thereAreConflicts,
               string errorMessage) = _gitService.CommitAndPush(pullInfo);
            SetFileSystemWatcherEnabled(true);
            return Ok(new
            {
                isConnectionMissing = isConnectionMissing,
                isAuthenticationMissing = isAuthenticationMissing,
                thereAreConflicts = thereAreConflicts,
                errorMessage = errorMessage
            });
        }

        [HttpPost("commit")]
        [Obsolete("This endpoint is deprecated. Use ModernGitToolbar/commit for SSH-based operations.")]
        public IActionResult Commit(PullInfo pullInfo)
        {
            SetFileSystemWatcherEnabled(false);
            pullInfo.ProjectPath = GetProjectPath();
            (bool isConnectionMissing,
               bool isAuthenticationMissing,
               bool thereAreConflicts,
               string errorMessage) = _gitService.Commit(pullInfo);
            SetFileSystemWatcherEnabled(true);
            return Ok(new
            {
                isConnectionMissing = isConnectionMissing,
                isAuthenticationMissing = isAuthenticationMissing,
                thereAreConflicts = thereAreConflicts,
                errorMessage = errorMessage
            });
        }

        [HttpPost("push")]
        [Obsolete("This endpoint is deprecated. Use ModernGitToolbar/push for SSH-based operations.")]
        public IActionResult Push(PullInfo pullInfo)
        {
            SetFileSystemWatcherEnabled(false);
            pullInfo.ProjectPath = GetProjectPath();
            (bool isConnectionMissing,
               bool isAuthenticationMissing,
               bool thereAreConflicts,
               string errorMessage) = _gitService.Push(pullInfo);
            SetFileSystemWatcherEnabled(true);
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
