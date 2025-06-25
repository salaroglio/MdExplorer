using MdExplorer;
using MdExplorer.Abstractions.Models.GIT;
using MdExplorer.Features.GIT;
using MdExplorer.Features.GIT.models;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.GIT;
using Microsoft.AspNetCore.Mvc;
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
    public class BranchesController : ControllerBase
    {
        private readonly IGitService _gitService;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public BranchesController(IGitService gitService,
            FileSystemWatcher fileSystemWatcher)
        {
            _gitService = gitService;
            _fileSystemWatcher = fileSystemWatcher;
        }
        [HttpGet("feat/GetCurrentBranch")]
        [Obsolete("This endpoint is deprecated. Use ModernGitToolbar/branch-status for SSH-based operations.")]
        public IActionResult GetCurrentBranch()
        {
            var toReturn = _gitService.GetCurrentBranch(_fileSystemWatcher.Path);
            var howManyFilesAreChanged = _gitService.HowManyFilesAreChanged(_fileSystemWatcher.Path);

            return Ok(new
            {
                name = toReturn,
                somethingIsChangedInTheBranch = howManyFilesAreChanged > 0,
                howManyFilesAreChanged = howManyFilesAreChanged
            });//classe branch lato angular
        }
        [HttpGet("feat/getdatatopull")]
        [Obsolete("This endpoint is deprecated. Use ModernGitToolbar/get-data-to-pull for SSH-based operations.")]
        public IActionResult GetDataToPull()
        {
            var howManyFilesAreToPull = 0;
            var howManyCommitAreToPush = 0;
            var connectionIsActive = true;
            IList<FileNameAndAuthor> whatFilesAreChanged = new List<FileNameAndAuthor>();
            try
            {
                howManyFilesAreToPull = _gitService.HowManyFilesAreToPull(_fileSystemWatcher.Path);
                howManyCommitAreToPush = _gitService.CountCommitsBehindTrackedBranch(_fileSystemWatcher.Path);
                whatFilesAreChanged = _gitService.GetFilesAndAuthorsToBeChanged(_fileSystemWatcher.Path);
            }
            catch (Exception ex)
            {
                connectionIsActive = false;                
            }            
            return Ok(new
            {
                somethingIsToPull = howManyFilesAreToPull > 0,
                howManyFilesAreToPull = howManyFilesAreToPull,
                connectionIsActive = connectionIsActive,
                howManyCommitAreToPush = howManyCommitAreToPush,
                whatFilesWillBeChanged = whatFilesAreChanged
            });
        }

        [HttpPost("feat/checkoutBranch")]
        [Obsolete("This endpoint uses legacy Git service. Consider using modern Git operations.")]
        public IActionResult CheckoutBranch([FromBody] GitBranch branch)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var toReturn = _gitService.CheckoutBranch(branch, _fileSystemWatcher.Path, GitCallBackForCheckout);

            return Ok(new
            {
                name = toReturn.Name,
                somethingIsChangedInTheBranch = false,
                howManyFilesAreChanged = 0,
                FullPath = _fileSystemWatcher.Path,
            });//classe branch lato angular
        }

        private void GitCallBackForCheckout(string path, int a, int b)
        {
            if (a == b)
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }

        }


        [HttpGet]
        [Obsolete("This endpoint uses legacy Git service. Consider using modern Git operations.")]
        public IActionResult GetBranches()
        {
            var toReturn = _gitService.GetBranches(_fileSystemWatcher.Path);
            return Ok(toReturn);
        }

    }
}
