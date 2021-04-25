using MdExplorer.Abstractions.Models;
using MdExplorer.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("api/MdFiles")]
    public class MdFilesController:ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;

        public MdFilesController(FileSystemWatcher fileSystemWatcher)
        {
            _fileSystemWatcher = fileSystemWatcher;
        }
        [HttpGet]
        public IActionResult GetAllMdFiles()
        {
            var currentPath = _fileSystemWatcher.Path;
            var list = new List<MdFile>();
            foreach (var itemFile in Directory.GetFiles(currentPath).Where(_=>Path.GetExtension(_)==".md"))
            {
                var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
                list.Add(new MdFile { Name = Path.GetFileName(itemFile), Path = patchedItemFile });
            }
             
            return Ok(list);
        }
    }
}
