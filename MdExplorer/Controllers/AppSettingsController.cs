using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Service.Utilities;
using Microsoft.AspNetCore.Mvc;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/AppSettings/{action}")] //AppCurrentFolder
    public class AppSettingsController : ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IUserSettingsDB _session;
        private readonly ProcessUtil _processUtil;

        public AppSettingsController(FileSystemWatcher fileSystemWatcher, 
                IUserSettingsDB session,
                ProcessUtil processUtil)
        {
            _fileSystemWatcher = fileSystemWatcher;
            _session = session;
            _processUtil = processUtil;
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
        public IActionResult SetSettings(Settings settings)
        {
            var settingsDal = _session.GetDal<Setting>();
            _session.BeginTransaction(System.Data.IsolationLevel.Unspecified);
            foreach (var item in settings.settings)
            {
                var dbItem = settingsDal.GetList().Where(_ => _.Id == item.Id).FirstOrDefault();
                dbItem.ValueDateTime = item.ValueDateTime;
                dbItem.ValueDecimal = item.ValueDecimal;
                dbItem.ValueInt = item.ValueInt;
                dbItem.ValueString = item.ValueString;
                settingsDal.Save(dbItem);
            }
            _session.Commit();
            return Ok(new { response = "settings saved" });
        }

        [HttpGet]
        public IActionResult OpenFile(string path)
        {
            _processUtil.OpenFileWithVisualStudioCode(path);
            return Ok(new { message = "opened" });
        }

        

        [HttpGet]
        public IActionResult OpenFolder(string path)
        {
            var pathToOpen = Path.GetDirectoryName(path);
            Process.Start("explorer.exe", pathToOpen);
            return Ok(new { message = "opened" });
        }

        [HttpGet]
        public IActionResult OpenChromePdf(string path)
        {
            var processToStart = new ProcessStartInfo("cmd.exe", $"/c \"{path}\"") { 
                
                CreateNoWindow = false };
            Process.Start(processToStart);
            
            return Ok(new { message = "opened" });
        }

        public class Settings
        {
            public Setting[] settings { get; set; }
        }

        [HttpGet]
        public IActionResult KillServer()
        {
            //Environment.Exit(0);
            return Ok(new { message = "self-destruction activated" });
        }

        [HttpGet]
        public IActionResult ShowToc(string documentPathEncoded, bool showToc)
        {
            var docPathDecoded = HttpUtility.UrlDecode(documentPathEncoded);
            
            var docSettDal = _session.GetDal<DocumentSetting>();
            var docSett = docSettDal.GetList().Where(_ => _.DocumentPath == docPathDecoded)
                .FirstOrDefault() ?? new DocumentSetting { DocumentPath = docPathDecoded};
            docSett.ShowTOC = showToc;

            _session.BeginTransaction();
            docSettDal.Save(docSett);
            _session.Commit();
            return Ok(new { message = "done" });
        }


    }
}
