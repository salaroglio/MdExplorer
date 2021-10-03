using Markdig;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace MdExplorer.Service.Controllers
{
    /// <summary>
    /// classe dedicata all'esportazione
    /// </summary>
    [ApiController]
    [Route("/api/mdexportemail/{*url}")]
    public class MdExportEmailController : ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly ICommandRunnerHtml _commandRunner;

        public MdExportEmailController(FileSystemWatcher fileSystemWatcher,
            ICommandRunnerHtml commandRunner)
        {
            _fileSystemWatcher = fileSystemWatcher;
            _commandRunner = commandRunner;
        }

        [HttpGet]
        public IActionResult Get(string connectionId)
        {
            try
            {
                var filePath = _fileSystemWatcher.Path;
                var relativePathFileSystem = GetRelativePathFileSystem("mdexportemail");
                var systemPathFile = string.Concat(filePath, relativePathFileSystem); // filesystem path
                Microsoft.Office.Interop.Outlook.Application oApp = new Microsoft.Office.Interop.Outlook.Application();
                Microsoft.Office.Interop.Outlook._MailItem oMailItem = 
                    (Microsoft.Office.Interop.Outlook._MailItem)oApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);

                var markdown = System.IO.File.ReadAllText(systemPathFile);

                var requestInfo = new RequestInfo()
                {
                    CurrentQueryRequest = relativePathFileSystem,
                    CurrentRoot = _fileSystemWatcher.Path,
                    AbsolutePathFile = systemPathFile
                };
                markdown = _commandRunner.TransformInNewMDFromMD(markdown, requestInfo);


                var pipeline = new MarkdownPipelineBuilder()
               .UseAdvancedExtensions()
               .UsePipeTables()
               .UseBootstrap()               
               .UseEmojiAndSmiley()
               .UseYamlFrontMatter()
               .Build();

                Directory.SetCurrentDirectory(_fileSystemWatcher.Path);

                var result = Markdown.ToHtml(markdown, pipeline);

                result = _commandRunner.TransformAfterConversion(result, requestInfo);
                

                oMailItem.HTMLBody = result;
                oMailItem.Display();
            }
            catch(Exception ex)
            {
                return Problem($@"No good:{ex.Message}" );
            }
            return Ok("Done");
        }

        protected string GetRelativePathFileSystem(string controllerName)
        {
            //mdexplorer
            return HttpUtility.UrlDecode(Request.Path.ToString().Replace($"/api/{controllerName}/", string.Empty).Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
