using MdExplorer.DataStorage;
using MdExplorer.HubConfig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("MdConfig")]
    public class MdConfigController : ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IHubContext<ChartHub> _hub;

        public MdConfigController(FileSystemWatcher fileSystemWatcher, IHubContext<ChartHub> hub)
        {
            _fileSystemWatcher = fileSystemWatcher;
            _hub = hub;
        }
        [HttpGet]
        public IActionResult SetFolderSourceAsync(string folderPath)
        {
            var filePath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (folderPath != null)
            {
                filePath = folderPath;
            }

            _fileSystemWatcher.Path = filePath;
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            _fileSystemWatcher.Filter = "*.md";
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Changed += ChangeWithLove;
            return Ok("Done");
        }

        private void ChangeWithLove(object sender, FileSystemEventArgs e)
        {
            _hub.Clients.All.SendAsync("transferchartdata", DataManager.GetData());
            
        }
    }
}
