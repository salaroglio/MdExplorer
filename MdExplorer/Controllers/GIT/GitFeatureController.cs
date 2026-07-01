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
        private readonly MdExplorer.Abstractions.Services.IMarkdownFtsService _markdownFtsService;
        public GitFeatureController(IGitService gitService,
        IOptions<MdExplorerAppSettings> options,
        ILogger<GitFeatureController> logger,
        IHubContext<MonitorMDHub> hubContext,
        IUserSettingsDB session,
        ICommandRunnerHtml commandRunner,
         IHelper helper,
         IWorkLink[] modifiers,
        IEngineDB engineDB,
        MdExplorer.Abstractions.Services.IMarkdownFtsService markdownFtsService,
        IDatabaseManager databaseManager = null) : base(logger, options,
            hubContext, session, engineDB, commandRunner,
            modifiers, helper, databaseManager)
        {
            _gitService = gitService;
            _markdownFtsService = markdownFtsService;
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
            var engineDB = GetEngineDB();
            var relDal = engineDB.GetDal<MarkdownFile>();
            var projectPath = GetProjectPath();

            foreach (var item in filesToBeChanged)
            {
                var mdFile = relDal.GetList().FirstOrDefault(_ => _.Path == item.FullPath);

                // File rimosso dal pull: cancellazione completa (riga + link + chunk + FTS)
                if (!System.IO.File.Exists(item.FullPath))
                {
                    if (mdFile != null)
                    {
                        try
                        {
                            engineDB.BeginTransaction();
                            engineDB.CreateSQLQuery("DELETE FROM LinkInsideMarkdown WHERE MarkdownFileId = :id")
                                .SetParameter("id", mdFile.Id, NHibernate.NHibernateUtil.Guid).ExecuteUpdate();
                            engineDB.CreateSQLQuery("DELETE FROM DocumentChunk WHERE MarkdownFileId = :id")
                                .SetParameter("id", mdFile.Id, NHibernate.NHibernateUtil.Guid).ExecuteUpdate();
                            engineDB.CreateSQLQuery("DELETE FROM MarkdownFile WHERE Id = :id")
                                .SetParameter("id", mdFile.Id, NHibernate.NHibernateUtil.Guid).ExecuteUpdate();
                            engineDB.Commit();
                            _markdownFtsService.DeleteFileByPath(projectPath, item.FullPath);
                        }
                        catch (Exception ex)
                        {
                            try { engineDB.Rollback(); } catch { }
                            _logger.LogError(ex, "[RefreshDatabase] Delete failed for '{Path}'", item.FullPath);
                        }
                    }
                    continue;
                }

                // Contenuto + fingerprint (una sola lettura per file)
                string content = null, contentHash = null, statMtime = null;
                long? statSize = null;
                try
                {
                    var fi = new FileInfo(item.FullPath);
                    statMtime = fi.LastWriteTimeUtc.ToString("o");
                    statSize = fi.Length;
                    content = System.IO.File.ReadAllText(item.FullPath);
                    contentHash = ContentFingerprint.ComputeHash(content);
                }
                catch (Exception readEx)
                {
                    _logger.LogWarning(readEx, "[RefreshDatabase] Cannot read '{Path}'", item.FullPath);
                }

                try
                {
                    engineDB.BeginTransaction();
                    if (mdFile == null)
                    {
                        mdFile = new MarkdownFile
                        {
                            FileName = Path.GetFileName(item.FullPath),
                            Path = item.FullPath,
                            // "File" (maiuscolo): SaveLinksFromMarkdown parsa i link solo
                            // con questo valore (incoerenza storica dei FileType).
                            FileType = "File"
                        };
                    }
                    mdFile.FileName = Path.GetFileName(item.FullPath);
                    mdFile.FileLastWriteUtc = statMtime;
                    mdFile.FileSize = statSize;
                    mdFile.FileHash = contentHash;
                    relDal.Save(mdFile);
                    engineDB.Flush();

                    // Link per file NUOVI ed esistenti (prima i nuovi venivano inseriti
                    // senza alcun parse dei link).
                    SaveLinksFromMarkdown(mdFile);

                    if (content != null)
                    {
                        mdFile.Tldr = TldrExtractor.ExtractTldr(content);
                        mdFile.LinksHash = contentHash;
                        relDal.Save(mdFile);
                    }
                    engineDB.Commit();
                }
                catch (Exception ex)
                {
                    try { engineDB.Rollback(); } catch { }
                    _logger.LogError(ex, "[RefreshDatabase] Upsert failed for '{Path}'", item.FullPath);
                    continue;
                }

                // FTS side-car post-commit + marker FtsHash
                if (content != null)
                {
                    try
                    {
                        _markdownFtsService.UpsertFile(projectPath, mdFile.Id, item.FullPath, mdFile.FileName, content);
                        engineDB.BeginTransaction();
                        engineDB.CreateSQLQuery("UPDATE MarkdownFile SET FtsHash = :hash WHERE Id = :id")
                            .SetParameter("hash", contentHash)
                            .SetParameter("id", mdFile.Id, NHibernate.NHibernateUtil.Guid)
                            .ExecuteUpdate();
                        engineDB.Commit();
                    }
                    catch (Exception ftsEx)
                    {
                        try { engineDB.Rollback(); } catch { }
                        _logger.LogWarning(ftsEx, "[RefreshDatabase] FTS update failed for '{Path}'", item.FullPath);
                    }
                }
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
