using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ad.Tools.Dal;
using MdExplorer.Service.Utilities;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.MdProjects;
using AutoMapper;
using MdExplorer.Service.Controllers.MdProjects.dto;

namespace MdExplorer.Service.Controllers.MdProjects
{
    [ApiController]
    [Route("api/MdProjects/{action}")]
    public class MdProjectsController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IServiceProvider _services;
        private readonly ProcessUtil _processUtil;
        private readonly IMapper _mapper;

        public MdProjectsController(IUserSettingsDB userSettingsDB,
                FileSystemWatcher fileSystemWatcher,
                IServiceProvider services,
                ProcessUtil processUtil,
                IMapper mapper)
        {
            _userSettingsDB = userSettingsDB;
            _fileSystemWatcher = fileSystemWatcher;
            _services = services;
            _processUtil = processUtil;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetProjects()
        {

            // check if folder exists in project table

            var projectDal = _userSettingsDB.GetDal<Project>();
            var list = projectDal.GetList().OrderByDescending(_ => _.LastUpdate).ToList();
            var listToReturn = _mapper.Map<IEnumerable<ProjectWithoutBookmarks>>(list);


            return Ok(listToReturn);
        }

        [HttpPost]
        public IActionResult DeleteProject([FromBody] Project project)
        {
            _userSettingsDB.BeginTransaction();
            var projectDal = _userSettingsDB.GetDal<Project>();
            var projectFromDb = projectDal.GetList().Where(_ => _.Id == project.Id).FirstOrDefault();
            projectDal.Delete(projectFromDb);
            _userSettingsDB.Commit();
            return Ok(new { message = "done!" });
        }

        [HttpPost]
        public IActionResult SetFolderProject([FromBody] FolderPath folderPath)
        {
            // Prima di cambiare il percorso, disabilita temporaneamente il FileSystemWatcher
            bool wasEnabled = _fileSystemWatcher.EnableRaisingEvents;
            _fileSystemWatcher.EnableRaisingEvents = false;
            
            try
            {
                // Aggiorna il percorso del FileSystemWatcher
                _fileSystemWatcher.Path = folderPath.Path;
                
                // Log del cambio percorso
                var logger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<MdProjectsController>>();
                logger?.LogInformation($"🔄 FileSystemWatcher path changed to: {folderPath.Path}");
                
                // renew project data
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                var project = projectDal.GetList().Where(_ => _.Path == folderPath.Path).FirstOrDefault();
                if (project == null)
                {
                    project = new Project
                    {
                        Path = folderPath.Path,
                        Name = System.IO.Path.GetFileName(_fileSystemWatcher.Path)
                    };
                }
                project.LastUpdate = DateTime.Now;
                projectDal.Save(project);
                _userSettingsDB.Commit();
                
                // Configura i database per il nuovo progetto
                ProjectsManager.SetNewProject(_services, folderPath.Path);
                
                // Riabilita il FileSystemWatcher se era abilitato prima
                if (wasEnabled)
                {
                    _fileSystemWatcher.EnableRaisingEvents = true;
                    logger?.LogInformation($"✅ FileSystemWatcher re-enabled for path: {folderPath.Path}");
                }
                
                return Ok(new { id = project.Id, name = project.Name, path = project.Path, sidenavWidth= project.SidenavWidth });
            }
            catch (Exception ex)
            {
                // In caso di errore, riabilita il FileSystemWatcher con il vecchio percorso
                if (wasEnabled)
                {
                    _fileSystemWatcher.EnableRaisingEvents = true;
                }
                throw;
            }
        }

        [HttpPost]
        public IActionResult SetSideNavWidth([FromBody] Project project)
        {
            _userSettingsDB.BeginTransaction();
            var projectDal = _userSettingsDB.GetDal<Project>();
            var projectDB = projectDal.GetList().Where(_=>_.Id == project.Id).FirstOrDefault();
            if (projectDB != null)
            {
                projectDB.SidenavWidth = project.SidenavWidth;
            }
            _userSettingsDB.Commit();
            return Ok();
        }

        [HttpPost]
        public IActionResult InitializeProjectTemplates([FromBody] FolderPath folderPath)
        {
            try
            {
                var logger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<MdProjectsController>>();
                logger?.LogInformation($"🔧 [TemplateInit] Initializing templates for: {folderPath.Path}");
                
                // Chiama ConfigTemplates per creare la struttura template
                ProjectsManager.ConfigTemplates(folderPath.Path);
                
                logger?.LogInformation($"✅ [TemplateInit] Templates initialized successfully for: {folderPath.Path}");
                
                return Ok(new { message = "Templates initialized successfully", path = folderPath.Path });
            }
            catch (Exception ex)
            {
                var logger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<MdProjectsController>>();
                logger?.LogError($"❌ [TemplateInit] Error initializing templates: {ex.Message}");
                
                return StatusCode(500, new { error = "Failed to initialize templates", details = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SetFolderProjectQuickNotes([FromBody] FolderPath folderPath)
        {
            // Prima di cambiare il percorso, disabilita temporaneamente il FileSystemWatcher
            bool wasEnabled = _fileSystemWatcher.EnableRaisingEvents;
            _fileSystemWatcher.EnableRaisingEvents = false;
            
            try
            {
                // Aggiorna il percorso del FileSystemWatcher
                _fileSystemWatcher.Path = folderPath.Path;
                
                // Log del cambio percorso
                var logger = HttpContext.RequestServices.GetService<Microsoft.Extensions.Logging.ILogger<MdProjectsController>>();
                logger?.LogInformation($"🔄 FileSystemWatcher path changed to: {folderPath.Path} (Quick Notes)");
                
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
                        Name = System.IO.Path.GetFileName(_fileSystemWatcher.Path)
                    };
                }

                project.LastUpdate = DateTime.Now;
                projectDal.Save(project);
                _userSettingsDB.Commit();
                
                // Configura i database per il nuovo progetto
                ProjectsManager.SetNewProject(_services, folderPath.Path);
                
                // Riabilita il FileSystemWatcher se era abilitato prima
                if (wasEnabled)
                {
                    _fileSystemWatcher.EnableRaisingEvents = true;
                    logger?.LogInformation($"✅ FileSystemWatcher re-enabled for path: {folderPath.Path}");
                }
                
                var settingDal = _userSettingsDB.GetDal<Setting>();
                var editorPath = settingDal.GetList().Where(_ => _.Name == "EditorPath").FirstOrDefault()?.ValueString
                    ?? @"C:\Users\Carlo\AppData\Local\Programs\Microsoft VS Code\Code.exe";
                _processUtil.OpenFileWithVisualStudioCode(currentIdNotes, editorPath);
                
                return Ok(new { message = "done", currentNote = currentIdNotes });
            }
            catch (Exception ex)
            {
                // In caso di errore, riabilita il FileSystemWatcher con il vecchio percorso
                if (wasEnabled)
                {
                    _fileSystemWatcher.EnableRaisingEvents = true;
                }
                throw;
            }
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


