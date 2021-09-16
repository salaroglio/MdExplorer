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
    [Route("/api/WriteMDController/{action}")]
    public class WriteMDController : ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;

        public WriteMDController(FileSystemWatcher fileSystemWatcher)
        {
            _fileSystemWatcher = fileSystemWatcher;
        }

        [HttpGet]
        public IActionResult SetEmoji(int recurrence)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;


            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok();
        }

    }
}
