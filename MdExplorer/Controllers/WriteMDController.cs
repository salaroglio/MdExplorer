using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.FunctionParameters;
using MdExplorer.Features.Commands.Markdown;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
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
        private readonly ICommandRunnerMD _commandRunner;

        public WriteMDController(FileSystemWatcher fileSystemWatcher, ICommandRunnerMD commandRunner)
        {
            _fileSystemWatcher = fileSystemWatcher;

            _commandRunner = commandRunner;
        }

        [HttpGet]
        public IActionResult ActivateSaveCopy(string pathFile)
        {
            var systemPathFile = pathFile.Replace('/', Path.DirectorySeparatorChar);
            var markdown = System.IO.File.ReadAllText(systemPathFile);
            var commandSave = (ICommandSaveMD<string, string>)_commandRunner.Commands
                        .Where(_ => _.Name == "FromEmojiFloppyDiskToSaveFile").FirstOrDefault();
            var folder = Path.GetDirectoryName(systemPathFile);
            var fileName = string.Empty;
            (markdown, fileName) = commandSave.GetMDAndFileNameToSave(markdown, "test");
            fileName = folder + Path.DirectorySeparatorChar + fileName;

            System.IO.File.WriteAllText(fileName, markdown);

            return Ok("Done");
        }

        [HttpGet]
        public IActionResult SetEmojiOrderPriority(int currentNodeIndex,
                    int? previousNodeIndex, int? nextNodeIndex,
                    string pathFile,
                    int tableGameIndex)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var systePathFile = pathFile.Replace('/', Path.DirectorySeparatorChar);

            lock (lockAccessToFileMD) // così evito accesso multiplo allo stesso file ma sequenzializzo
            {
                // load Md
                var markdown = System.IO.File.ReadAllText(systePathFile);
                var requestInfo = new RequestInfo
                {
                    AbsolutePathFile = pathFile,
                    CurrentRoot = _fileSystemWatcher.Path,
                };

                var param = new EmojiPriorityOrderInfo
                {
                    CurrentNodeIndex = currentNodeIndex,
                    FilePath = pathFile,
                    NextNodeIndex = nextNodeIndex,
                    PreviousNodeIndex = previousNodeIndex,
                    TableGameIndex = tableGameIndex
                };
                // transform
                var replaceSingleItem = (IReplaceSingleItemMD<EmojiPriorityOrderInfo>)_commandRunner.Commands
                        .Where(_ => _.Name == "FromEmojiToDynamicPriority").FirstOrDefault();
                markdown = replaceSingleItem
                        .ReplaceSingleItem(markdown, requestInfo, param);
                System.IO.File.WriteAllText(systePathFile, markdown);

                // write
            }
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok("Done");
        }

        [HttpGet]
        public IActionResult SetEmojiPriority(int index, string pathFile, string toReplace)
        {

            var systePathFile = pathFile.Replace('/', Path.DirectorySeparatorChar);
            _fileSystemWatcher.EnableRaisingEvents = false;
            //var emojiCommand = _commands.Where(_ => _.Name == "FromEmojiToPng").FirstOrDefault();
            lock (lockAccessToFileMD) // così evito accesso multiplo allo stesso file ma sequenzializzo
            {
                // load Md
                var markdown = System.IO.File.ReadAllText(systePathFile);
                var requestInfo = new RequestInfo
                {
                    AbsolutePathFile = pathFile,
                    CurrentRoot = _fileSystemWatcher.Path,
                };
                // transform
                var replace = (IReplaceSingleItemMD<Features.Commands.Markdown.EmojiReplaceInfo>)_commandRunner.Commands
                        .Where(_ => _.Name == "FromEmojiToDynamicPriority").FirstOrDefault();
                markdown = replace.ReplaceSingleItem(markdown, requestInfo, new Features.Commands.Markdown.EmojiReplaceInfo { ToReplace = toReplace, Index = index });
                System.IO.File.WriteAllText(systePathFile, markdown);

                // write
            }

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok("done");
        }

        [HttpGet]
        public IActionResult SetEmojiProcess(int index, string pathFile, string toReplace)
        {

            var systePathFile = pathFile.Replace('/', Path.DirectorySeparatorChar);
            _fileSystemWatcher.EnableRaisingEvents = false;
            //var emojiCommand = _commands.Where(_ => _.Name == "FromEmojiToPng").FirstOrDefault();
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
                var replace = (IReplaceSingleItemMD<Features.Commands.Markdown.EmojiReplaceInfo>)_commandRunner.Commands
                        .Where(_ => _.Name == "FromEmojiToDynamicProcess").FirstOrDefault();
                markdown = replace
                        .ReplaceSingleItem(markdown, requestInfo, new Features.Commands.Markdown.EmojiReplaceInfo { Index = index, ToReplace = toReplace }); //toReplace, index
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
            //var emojiCommand = _commands.Where(_ => _.Name == "FromEmojiCalendarToDatepicker").FirstOrDefault();
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
                var replace = (IReplaceSingleItemMD<Features.Commands.Markdown.EmojiReplaceInfo>)_commandRunner.Commands
                        .Where(_ => _.Name == "FromEmojiCalendarToDatepicker").FirstOrDefault();
                markdown = replace
                        .ReplaceSingleItem(markdown, requestInfo, new Features.Commands.Markdown.EmojiReplaceInfo { ToReplace = toReplace, Index = index } ); //toReplace, index
                System.IO.File.WriteAllText(systePathFile, markdown);

                // write
            }

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok("done");
        }
    }
}
