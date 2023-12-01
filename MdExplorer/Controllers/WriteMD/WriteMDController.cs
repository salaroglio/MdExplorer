using MdExplorer;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.FunctionParameters;
using MdExplorer.Features.Commands.Markdown;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.WriteMD.dto;
using MdExplorer.Service.Controllers.WriteMDDto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers.WriteMDDto
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

        /// <summary>
        /// Thi function is called when an Image is resized
        /// It's getting width, height... and ohter csslike info
        /// in order to store that data into markdown
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveImgPositionAndSize([FromBody] SaveImgPostionAndSizeDto dto)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            CSSSavedOnPageInfo cssInfo;
            var systePathFile = dto.PathFile.Replace('/', Path.DirectorySeparatorChar);
            lock (lockAccessToFileMD) // così evito accesso multiplo allo stesso file ma sequenzializzo
            {
                // load Md
                var markdown = System.IO.File.ReadAllText(systePathFile);
                var requestInfo = new RequestInfo
                {
                    AbsolutePathFile = dto.PathFile,
                    CurrentRoot = _fileSystemWatcher.Path,
                    CurrentQueryRequest = dto.CurrentQueryRequest
                };
                var param = new CSSSavedOnPageInfo
                {
                    ClientX = dto.ClientX,
                    ClientY = dto.ClientY, // magic number to fit another magic number in jqueryForFirstPage.js in the function move(e)
                    CSSHash = dto.CSSHash,
                    Height = dto.Height,
                    LinkHash = dto.LinkHash,
                    Width = dto.Width,
                    Position = dto.Position
                };
                // transform
                var replaceSingleItem = (IReplaceSingleItemMD<CSSSavedOnPageInfo, CSSSavedOnPageInfo>)_commandRunner.Commands
                        .Where(_ => _.Name == "CSSSavedOnPage").FirstOrDefault();

                (markdown, cssInfo) = replaceSingleItem
                        .ReplaceSingleItem(markdown, requestInfo, param);
                System.IO.File.WriteAllText(systePathFile, markdown);

            }
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(new { Message = "Done", cssInfo.CSSHash });
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
            EmojiPriorityOrderInfo info = null;

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
                var replaceSingleItem = (IReplaceSingleItemMD<EmojiPriorityOrderInfo, EmojiPriorityOrderInfo>)_commandRunner.Commands
                        .Where(_ => _.Name == "FromEmojiToDynamicPriority").FirstOrDefault();
                (markdown, info) = replaceSingleItem
                        .ReplaceSingleItem(markdown, requestInfo, param);
                System.IO.File.WriteAllText(systePathFile, markdown);

                // write
            }
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(info);
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
                        .ReplaceSingleItem(markdown, requestInfo, new Features.Commands.Markdown.EmojiReplaceInfo { ToReplace = toReplace, Index = index }); //toReplace, index
                System.IO.File.WriteAllText(systePathFile, markdown);

                // write
            }

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok("done");
        }

        [HttpGet]
        public IActionResult GetEditorH1(string editorH1CurrentIndex, string absolutePathFile)
        {
            var getData = (IEditorH1Context)_commandRunner.Commands
                       .Where(_ => _.Name == "EditH1").FirstOrDefault();
            var toReturn = getData.GetDataBy(editorH1CurrentIndex,absolutePathFile);
            return Ok(toReturn);
        }


        [HttpPost]
        public IActionResult SetEditorH1([FromBody] SetEditorH1Request dto)
        {
            var systePathFile = dto.PathFile.Replace('/', Path.DirectorySeparatorChar);
            var markdown = System.IO.File.ReadAllText(systePathFile);
            var replace = (IEditorH1Context)_commandRunner.Commands
                       .Where(_ => _.Name == "EditH1").FirstOrDefault();

            markdown = replace.SaveNewMarkdown(markdown, dto.IndexStart, dto.IndexEnd, dto.NewMd, dto.OldMd);
            System.IO.File.WriteAllText(systePathFile, markdown);

            return Ok();
        }
    }


}
