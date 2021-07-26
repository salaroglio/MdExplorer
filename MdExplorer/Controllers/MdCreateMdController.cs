using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Interfaces;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate;
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
    /// Usata dal command PDF per estrarre i dati senza generare HTML, ma rimanendo solo in MarkDown
    /// </summary>
    [ApiController]
    [Route("/api/mdcreatemd/{*url}")]
    public class MdCreateMdController : MdControllerBase<MdCreateMdController>
    {
        public MdCreateMdController(ILogger<MdCreateMdController> logger, 
            FileSystemWatcher fileSystemWatcher, IOptions<MdExplorerAppSettings> options, 
            IHubContext<MonitorMDHub> hubContext, ISession session, 
            ICommandRunnerPdf commandRunner
            ) : base(logger, fileSystemWatcher, options, hubContext, session, commandRunner)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var filePath = _fileSystemWatcher.Path + Path.DirectorySeparatorChar;            
            var relativePath = GetRelativePathFileSystem("mdcreatemd");
            var relativePathExtension = Path.GetExtension(relativePath);

            if (relativePathExtension != "" && relativePathExtension != ".md")
            {
                filePath = string.Concat(filePath, relativePath);
                var data = System.IO.File.ReadAllBytes(filePath);
                var notMdFile = new FileContentResult(data, "image/" + relativePathExtension);                
                return notMdFile;
            }

            if (relativePathExtension == ".md")
            {
                filePath = string.Concat(filePath, relativePath);
            }
            else
            {
                filePath = string.Concat(filePath, relativePath, ".md");
            }            

            var readText = string.Empty;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                readText = sr.ReadToEnd();
            }

            var requestInfo = new RequestInfo()
            {
                CurrentQueryRequest = relativePath,
                CurrentRoot = _fileSystemWatcher.Path,
                AbsolutePathFile = filePath
            };

            readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);
            readText = _commandRunner.PrepareMetadataBasedOnMD(readText, requestInfo);

            return new ContentResult
            {
                ContentType = "text/html",
                Content = readText
            };
        }
    }
}
