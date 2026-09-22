using MdExplorer;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models.GIT;
using MdExplorer.Features.GIT;
using MdExplorer.Features.GIT.models;
using MdExplorer.Hubs;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.GIT;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers.GIT
{
    [ApiController]
    [Route("/api/gitservice/branches")]
    [Obsolete("This controller is deprecated. Use ModernGitToolbarController for SSH-based operations.")]
    public class BranchesController : MdControllerBase<BranchesController>
    {
        private readonly IGitService _gitService;

        public BranchesController(
            IGitService gitService,
            ILogger<BranchesController> logger,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            IDatabaseManager databaseManager = null)
            : base(logger, options, hubContext, userSettingsDB, engineDB,
                  databaseManager: databaseManager)
        {
            _gitService = gitService;
        }

        [HttpGet("feat/GetCurrentBranch")]
        [Obsolete("This endpoint is deprecated. Use ModernGitToolbar/branch-status for SSH-based operations.")]
        public IActionResult GetCurrentBranch()
        {
            var projectPath = GetProjectPath();
            var toReturn = _gitService.GetCurrentBranch(projectPath);
            var howManyFilesAreChanged = _gitService.HowManyFilesAreChanged(projectPath);

            return Ok(new
            {
                name = toReturn,
                somethingIsChangedInTheBranch = howManyFilesAreChanged > 0,
                howManyFilesAreChanged = howManyFilesAreChanged
            });
        }

        [HttpGet("feat/getdatatopull")]
        [Obsolete("This endpoint is deprecated. Use ModernGitToolbar/get-data-to-pull for SSH-based operations.")]
public async Task<IActionResult> GetDataToPull([FromServices] MdExplorer.Services.Git.Interfaces.IModernGitService modernGit)
        {
            // Rete solo via git nativo: il conteggio lo fa ModernGitService, come la toolbar moderna.
            var projectPath = GetProjectPath();
            var data = await modernGit.GetPullPushDataAsync(projectPath);
            var files = (data.FilesToPull ?? Enumerable.Empty<MdExplorer.Services.Git.Interfaces.GitFileChange>())
                .Select(f => new FileNameAndAuthor { FileName = f.FilePath, Author = f.Author }).ToList();
            return Ok(new
            {
                somethingIsToPull = data.HasDataToPull,
                howManyFilesAreToPull = files.Count,
                connectionIsActive = data.IsRemoteAvailable,
                howManyCommitAreToPush = data.CommitsAhead,
                whatFilesWillBeChanged = files
            });
        }

        [HttpPost("feat/checkoutBranch")]
        [Obsolete("This endpoint uses legacy Git service. Consider using modern Git operations.")]
        public IActionResult CheckoutBranch([FromBody] GitBranch branch)
        {
            SetFileSystemWatcherEnabled(false);
            var projectPath = GetProjectPath();
            var toReturn = _gitService.CheckoutBranch(branch, projectPath, GitCallBackForCheckout);

            return Ok(new
            {
                name = toReturn.Name,
                somethingIsChangedInTheBranch = false,
                howManyFilesAreChanged = 0,
                FullPath = projectPath,
            });
        }

        private void GitCallBackForCheckout(string path, int a, int b)
        {
            if (a == b)
            {
                SetFileSystemWatcherEnabled(true);
            }
        }

        [HttpGet]
        [Obsolete("This endpoint uses legacy Git service. Consider using modern Git operations.")]
        public IActionResult GetBranches()
        {
            var toReturn = _gitService.GetBranches(GetProjectPath());
            return Ok(toReturn);
        }
    }
}
