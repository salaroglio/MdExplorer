using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ad.Tools.Dal;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/MdProjects/{action}")]
    public class MdProjectsController : ControllerBase
    {
        private readonly IUserSettingsDB _session;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IServiceProvider _services;

        public MdProjectsController(IUserSettingsDB session, 
                FileSystemWatcher fileSystemWatcher,
                IServiceProvider services)
        {
            _session = session;
            _fileSystemWatcher = fileSystemWatcher;
            _services = services;
        }
        [HttpGet]
        public IActionResult GetProjects()
        {
            _session.BeginTransaction();
            var projectDal = _session.GetDal<Project>();
            // check if folder exists in project table

            if (_fileSystemWatcher.Path != string.Empty)
            {
                var currentProject = projectDal.GetList().Where(_ => _.Path == _fileSystemWatcher.Path).FirstOrDefault();
                if (currentProject == null)
                {
                    var projectName = _fileSystemWatcher.Path.Substring(_fileSystemWatcher.Path.LastIndexOf("\\") + 1);
                    currentProject = new Project { Name = projectName, Path = _fileSystemWatcher.Path };
                    projectDal.Save(currentProject);
                }
            }
            _session.Commit();

            var list = projectDal.GetList().ToList();
            return Ok(list);
        }

        [HttpPost]
        public IActionResult SetFolderProject([FromBody]FolderPath folderPath)
        {
            _fileSystemWatcher.Path = folderPath.Path;
            ProjectsManager.SetNewProject(_services, folderPath.Path);            
            return Ok("{\"message\": \"done\"}");
        }

    }

    public class FolderPath
    {
        public string Path { get; set; }
    }
}


