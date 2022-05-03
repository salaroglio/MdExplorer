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
using MdExplorer.Service.Utilities;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/MdProjects/{action}")]
    public class MdProjectsController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IServiceProvider _services;
        private readonly ProcessUtil _processUtil;

        public MdProjectsController(IUserSettingsDB userSettingsDB,
                FileSystemWatcher fileSystemWatcher,
                IServiceProvider services,
                ProcessUtil processUtil)
        {
            _userSettingsDB = userSettingsDB;
            _fileSystemWatcher = fileSystemWatcher;
            _services = services;
            _processUtil = processUtil;
        }
        [HttpGet]
        public IActionResult GetProjects()
        {

            // check if folder exists in project table
            
            var projectDal = _userSettingsDB.GetDal<Project>();
            

            var list = projectDal.GetList().OrderByDescending(_ => _.LastUpdate).ToList();
            return Ok(list);
        }

        [HttpPost]
        public IActionResult DeleteProject([FromBody]Project project)
        {
            _userSettingsDB.BeginTransaction();
            var projectDal = _userSettingsDB.GetDal<Project>();
            var projectFromDb = projectDal.GetList().Where(_ => _.Id == project.Id).FirstOrDefault();
            projectDal.Delete(projectFromDb);
            _userSettingsDB.Commit();
            return Ok(new { message = "done!" });
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

        [HttpPost]
        public IActionResult SetFolderProjectQuickNotes([FromBody] FolderPath folderPath)
        {
            _fileSystemWatcher.Path = folderPath.Path;
            string currentIdNotes = CreateQuickNote();

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
            var settingDal = _userSettingsDB.GetDal<Setting>();
            var editorPath = settingDal.GetList().Where(_ => _.Name == "EditorPath").FirstOrDefault()?.ValueString
                ?? @"C:\Users\Carlo\AppData\Local\Programs\Microsoft VS Code\Code.exe";
            _processUtil.OpenFileWithVisualStudioCode(currentIdNotes, editorPath);
            return Ok(new { message = "done", currentNote = currentIdNotes });
        }

        private string CreateQuickNote()
        {
            var quickNotes = _fileSystemWatcher.Path + Path.DirectorySeparatorChar + "Quick-Notes";
            if (!Directory.Exists(quickNotes))
                Directory.CreateDirectory(quickNotes);
            var currentDate = DateTime.Now.ToString("yyyy_MM_dd");
            var currentIdNotes = quickNotes + Path.DirectorySeparatorChar + currentDate + ".md";
            if (!System.IO.File.Exists(currentIdNotes))
            {
                System.IO.File.WriteAllText(currentIdNotes, $"# {currentDate}");
            }
            
            return currentIdNotes;
        }
    }

    public class FolderPath
    {
        public string Path { get; set; }
    }
}


