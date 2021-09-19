using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
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
        private static object lockAccessToFileMD = new object();
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly ICommandMD[] _commands;
        private readonly ICommandRunnerHtml _commandRunner;

        public WriteMDController(FileSystemWatcher fileSystemWatcher, ICommandMD[] commands, ICommandRunnerHtml commandRunner)
        {
            _fileSystemWatcher = fileSystemWatcher;
            _commands = commands;
            _commandRunner = commandRunner;
        }

        [HttpGet]
        public IActionResult SetEmoji(int index, string pathFile, string toReplace)
        {
            
            var systePathFile = pathFile.Replace('/', Path.DirectorySeparatorChar);
            _fileSystemWatcher.EnableRaisingEvents = false;
            var emojiCommand = _commands.Where(_ => _.Name == "FromEmojiToPng").FirstOrDefault();
            lock (lockAccessToFileMD) // così evito accesso multiplo allo stesso file ma sequenzializzo
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

                markdown = emojiCommand.ReplaceSingleItem(markdown, requestInfo, toReplace, index);
                System.IO.File.WriteAllText(systePathFile, markdown);

                // write
            }

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok("done");
        }

        [HttpGet]
        public IActionResult SetCalendar(int index, string pathFile, string toReplace)
        {            
            var systePathFile = pathFile.Replace('/', Path.DirectorySeparatorChar);
            _fileSystemWatcher.EnableRaisingEvents = false;
            var emojiCommand = _commands.Where(_ => _.Name == "FromEmojiCalendarToDatepicker").FirstOrDefault();
            lock (lockAccessToFileMD) // così evito accesso multiplo allo stesso file ma sequenzializzo
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

                markdown = emojiCommand.ReplaceSingleItem(markdown, requestInfo, toReplace, index);
                System.IO.File.WriteAllText(systePathFile, markdown);

                // write
            }

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok("done");
        }
    }
}
