using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
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
    [Route("/api/WriteMD/{action}")]
    public class WriteMDController : ControllerBase
    {
        private static object lockSetEmohi = new object();
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly ICommandMD[] _commands;

        public WriteMDController(FileSystemWatcher fileSystemWatcher,ICommandMD[] commands)
        {
            _fileSystemWatcher = fileSystemWatcher;
            _commands = commands;
        }

        [HttpGet]
        public IActionResult SetEmoji(int index, string pathFile,  string toReplace)
        {
            var newIndex = 0;
            var systePathFile = pathFile.Replace('/', Path.DirectorySeparatorChar);
            _fileSystemWatcher.EnableRaisingEvents = false;
            var emojiCommand = _commands.Where(_ => _.Name == "FromEmojiToPng").FirstOrDefault();
            lock (lockSetEmohi) // così evito accesso multiplo allo stesso file ma sequenzializzo
            {
                // load Md
                var markdown = System.IO.File.ReadAllText(systePathFile);
                var requestInfo = new RequestInfo
                {
                    AbsolutePathFile = pathFile,
                    CurrentRoot = _fileSystemWatcher.Path,
                    CurrentQueryRequest = ""
                };
                // transform
                
                markdown = emojiCommand.ReplaceSingleItem(markdown, requestInfo,toReplace,index);
                System.IO.File.WriteAllText(systePathFile, markdown);
                
                // write
            }


            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(newIndex);
        }

    }
}
