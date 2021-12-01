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
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IServiceProvider _services;

        public MdProjectsController(IUserSettingsDB userSettingsDB, 
                FileSystemWatcher fileSystemWatcher,
                IServiceProvider services)
        {
            _userSettingsDB = userSettingsDB;
            _fileSystemWatcher = fileSystemWatcher;
            _services = services;
        }
        [HttpGet]
        public IActionResult GetProjects()
        {
            _userSettingsDB.BeginTransaction();
            var projectDal = _userSettingsDB.GetDal<Project>();
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
            _userSettingsDB.Commit();

            var list = projectDal.GetList().OrderByDescending(_=>_.LastUpdate).ToList();
            return Ok(list);
        }

        [HttpPost]
        public IActionResult SetFolderProject([FromBody]FolderPath folderPath)
        {
            _fileSystemWatcher.Path = folderPath.Path;
            // renew project data
            _userSettingsDB.BeginTransaction();
            var projectDal = _userSettingsDB.GetDal<Project>();
            var project = projectDal.GetList().Where(_ => _.Path == folderPath.Path).FirstOrDefault();
            if (project == null)
            {
                project = new Project
                {
                    Path = folderPath.Path,
                    Name = _fileSystemWatcher.Path.Substring(_fileSystemWatcher.Path.LastIndexOf("\\") + 1)
                };
            }
            project.LastUpdate = DateTime.Now;
            projectDal.Save(project);
            _userSettingsDB.Commit();
            ProjectsManager.SetNewProject(_services, folderPath.Path);            
            return Ok("{\"message\": \"done\"}");
        }

    }

    public class FolderPath
    {
        public string Path { get; set; }
    }
}


