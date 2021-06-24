using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/AppSettings/{action}")] //AppCurrentFolder
    public class AppSettingsController:ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly ISession _session;

        public AppSettingsController(FileSystemWatcher fileSystemWatcher, ISession session)
        {
            _fileSystemWatcher = fileSystemWatcher;
            _session = session;
        }

        [HttpGet]
        public IActionResult GetCurrentFolder()
        {
            var currentFolder = _fileSystemWatcher.Path;
            string lastFolder = currentFolder.Substring(currentFolder.LastIndexOf("\\") + 1);
            return Ok(new { currentFolder = lastFolder });
        }

        [HttpGet]
        public IActionResult GetSettings()
        {
            var settingsDal = _session.GetDal<Setting>();
            var settings = settingsDal.GetList();
            return Ok(new { settings = settings });
        }

        [HttpPost]
        public IActionResult SetSettings(Setting[] settings)
        {
            var settingsDal = _session.GetDal<Setting>();
            foreach (var item in settings)
            {
                settingsDal.Save(item);
            }
            return Ok(new { response = "done" });
        }

    }
}
