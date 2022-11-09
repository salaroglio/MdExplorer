using MdExplorer;
using MdExplorer.Features.GIT;
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
        public IActionResult GetCurrentBranch()
        {
            var toReturn = _gitService.GetCurrentBranch(_fileSystemWatcher.Path);
            var howManyFilesAreChanged = _gitService.HowManyFilesAreChanged(_fileSystemWatcher.Path);

            return Ok(new { name = toReturn, 
                somethingIsChangedInTheBranch = howManyFilesAreChanged>0,
                howManyFilesAreChanged = howManyFilesAreChanged
            });//classe branch lato angular
        }

        [HttpGet]
        public IActionResult GetBranches()
        {
            var toReturn = _gitService.GetBranches(_fileSystemWatcher.Path);
            return Ok(toReturn);
        }

    }
}
