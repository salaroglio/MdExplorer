using Ad.Tools.Dal.Extensions;
using DocumentFormat.OpenXml.EMMA;
using MdExplorer;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Commands.FunctionParameters;
using MdExplorer.Features.Commands.html;
using MdExplorer.Features.Commands.Markdown;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.MdFiles.ModelsDto;
using MdExplorer.Service.Controllers.WriteMD.dto;
using MdExplorer.Service.Controllers.WriteMDDto;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers.WriteMDDto
{
    [ApiController]
    [Route("/api/WriteMD/{action}")]
    public class WriteMDController : MdControllerBase<WriteMDController>
    {
        private static object lockAccessToFileMD = new object();

        public WriteMDController(FileSystemWatcher fileSystemWatcher,
            ILogger<WriteMDController> logger,
            IUserSettingsDB session,
            IEngineDB engineDB,
            IOptions<MdExplorerAppSettings> options,
            ICommandRunnerMD commandRunner,
            IHubContext<MonitorMDHub> hubContext,
            IWorkLink[] modifiers,
            IHelper helper
            ) : base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner, modifiers, helper)
        {


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
                var replaceSingleItem = (IReplaceSingleItemMD<CSSSavedOnPageInfo, CSSSavedOnPageInfo>)_commandRunner.GetAllCommands()
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
            var commandSave = (ICommandSaveMD<string, string>)_commandRunner.GetAllCommands()
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
                var replaceSingleItem = (IReplaceSingleItemMD<EmojiPriorityOrderInfo, EmojiPriorityOrderInfo>)_commandRunner.GetAllCommands()
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
                var replace = (IReplaceSingleItemMD<Features.Commands.Markdown.EmojiReplaceInfo>)_commandRunner.GetAllCommands()
                        .Where(_ => _.Name == "FromEmojiToDynamicPriority").FirstOrDefault();
                markdown = replace.ReplaceSingleItem(markdown, requestInfo, new Features.Commands.Markdown.EmojiReplaceInfo { ToReplace = toReplace, Index = index });
                System.IO.File.WriteAllText(systePathFile, markdown);

                // write
            }


            var editorH1 = (IEditorH1Context)_commandRunner.GetAllCommands()
                    .Where(_ => _.Name == "EditH1").FirstOrDefault();
            var indexItemMatchArray = editorH1.RenewEditorH1Index(systePathFile);

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(indexItemMatchArray);
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
                var replace = (IReplaceSingleItemMD<Features.Commands.Markdown.EmojiReplaceInfo>)_commandRunner.GetAllCommands()
                        .Where(_ => _.Name == "FromEmojiToDynamicProcess").FirstOrDefault();
                markdown = replace
                        .ReplaceSingleItem(markdown, requestInfo, new Features.Commands.Markdown.EmojiReplaceInfo { Index = index, ToReplace = toReplace }); //toReplace, index
                System.IO.File.WriteAllText(systePathFile, markdown);

                // write
            }
            var editorH1 = (IEditorH1Context)_commandRunner.GetAllCommands()
                   .Where(_ => _.Name == "EditH1").FirstOrDefault();
            var indexItemMatchArray = editorH1.RenewEditorH1Index(systePathFile);

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(indexItemMatchArray);
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
                var replace = (IReplaceSingleItemMD<Features.Commands.Markdown.EmojiReplaceInfo>)_commandRunner.GetAllCommands()
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
            var getData = (IEditorH1Context)_commandRunner.GetAllCommands()
                       .Where(_ => _.Name == "EditH1").FirstOrDefault();
            var toReturn = getData.GetDataBy(editorH1CurrentIndex, absolutePathFile);
            return Ok(toReturn);
        }


        [HttpPost]
        async public Task<IActionResult> SetEditorH1([FromBody] SetEditorH1Request dto)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            // Sign ont RefactoringSourceAction 
            // read from LinkInsideMarkdown the list of links affected from this changes
            // then get the mdFiles you have to change in order to re-link the MdShowH2 command
            // change all links 
            Regex rxGetTitles = new Regex(@"## ([^\n])*((?!\n## )(?!\n# ).)*",
                           RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var rxGetEmoji = new Regex(":.*?:");

            var systemPathFile = dto.PathFile.Replace('/', Path.DirectorySeparatorChar);
            var markdown = System.IO.File.ReadAllText(systemPathFile);


            var editorH1Engine = (IEditorH1Context)_commandRunner.GetAllCommands()
                       .Where(_ => _.Name == "EditH1").FirstOrDefault();

            var limDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var listOfLinksTochangeFromDB = limDal.GetList()
                .Where(_ => _.FullPath == dto.PathFile && _.Source == "WorkLinkMdShowH2")                
                .Select(_=> new LinkInsideMarkdownDto { 
                    FullPath = _.FullPath,
                    HTMLTitle = _.HTMLTitle,
                    Id = _.Id,
                    LinkedCommand = _.LinkedCommand,
                    MarkdownFile = _.MarkdownFile,
                    MdContext = _.MdContext,
                    MdTitle = _.MdTitle,
                    Path = _.Path,
                    SectionIndex = _.SectionIndex,  
                    Source = _.Source,
                })
                .ToArray();

            markdown = editorH1Engine.ApplyChangesToMarkdown(markdown, dto.IndexStart, dto.IndexEnd, dto.NewMd, dto.OldMd);
            if (listOfLinksTochangeFromDB.Length > 0)// at least one link COULD be refactored
            {
                var matchesMdTitle = rxGetTitles.Matches(markdown);

                foreach (var linkDB in listOfLinksTochangeFromDB)
                {
                    // First, apply changes and calc link and isolate the broken links                  
                    foreach (Match potentialMdTitle in matchesMdTitle)
                    {
                        var calcMdtitle = string.Join(string.Empty, potentialMdTitle.Groups[1].Captures)
                                        .Replace("\r", string.Empty);
                        
                        calcMdtitle = rxGetEmoji.Replace(calcMdtitle, string.Empty).Trim();

                        if (linkDB.MdTitle == calcMdtitle)
                        {
                            linkDB.Found = true;                            
                        }
                    }
                }

                var listOfBrokenLink = listOfLinksTochangeFromDB.Where(_ => !_.Found).ToArray();
                if (listOfBrokenLink.Count() > 0)
                {
                   
                    var titleH2Matches = rxGetTitles.Matches(dto.NewMd);
                    var mdTilteNew = string.Join(string.Empty, titleH2Matches[0].Groups[1].Captures)
                                        .Replace("\r", string.Empty).Trim();
                    mdTilteNew = rxGetEmoji.Replace(mdTilteNew, string.Empty).Trim();

                    var calcMdTitleNew = rxGetEmoji.Replace(mdTilteNew, string.Empty);
                    var titleH2OldMatches = rxGetTitles.Matches(dto.OldMd);

                    foreach (var brokenLink in listOfBrokenLink)
                    {
                        
                        // Go to change the link in the files
                        var markDownOfLink = System.IO.File.ReadAllText(brokenLink.MarkdownFile.Path);
                        var newLinkedCommand = brokenLink.LinkedCommand;
                        newLinkedCommand = newLinkedCommand.Replace("," + brokenLink.MdTitle + ",",
                                        "," + mdTilteNew.Trim() + ",");
                        newLinkedCommand = newLinkedCommand.Replace("," + brokenLink.HTMLTitle ,
                            ",#" + mdTilteNew.ToLower().Replace(" ", "-") );

                        markDownOfLink = markDownOfLink.Replace(brokenLink.LinkedCommand, newLinkedCommand);

                        System.IO.File.WriteAllText(brokenLink.MarkdownFile.Path,markDownOfLink);
                    }
                }
            }
            // Get back to client eventually broken link
            // 

            System.IO.File.WriteAllText(systemPathFile, markdown);
            var relativePath = dto.PathFile.Replace(_fileSystemWatcher.Path, string.Empty).Replace(Path.DirectorySeparatorChar, '/');
            var monitoredMd = new MonitoredMDModel
            {
                Path = relativePath,
                Name = Path.GetFileName(systemPathFile),
                RelativePath = systemPathFile.Replace(_fileSystemWatcher.Path, string.Empty),
                FullPath = systemPathFile,
                FullDirectoryPath = Path.GetDirectoryName(systemPathFile)
            };
            await _hubContext.Clients.Client(connectionId: dto.connectionId).SendAsync("markdownfileischanged", monitoredMd);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok();
        }
    }


}
