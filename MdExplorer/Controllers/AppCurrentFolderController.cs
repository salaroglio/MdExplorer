using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("api/AppCurrentFolder")]
    public class AppCurrentFolderController:ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;

        public AppCurrentFolderController(FileSystemWatcher fileSystemWatcher)
        {
            _fileSystemWatcher = fileSystemWatcher;            
        }

        [HttpGet]
        public IActionResult GetCurrentFolder()
        {
            var currentFolder = _fileSystemWatcher.Path;
            string lastFolder = Path.GetFileName(currentFolder);
            return Ok(new { currentFolder = lastFolder });
        }
    }
}
