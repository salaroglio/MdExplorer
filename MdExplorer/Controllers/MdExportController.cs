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
    [ApiController]
    [Route("/api/mdexport/{*url}")]
    public class MdExportController : ControllerBase
    {
        private readonly ILogger<MdExportController> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IOptions<MdExplorerAppSettings> _options;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ISession _session;
        private readonly ICommandRunner _commandRunner;
        private readonly IHelperPdf _helperPdf;

        public MdExportController(ILogger<MdExportController> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            ISession session,
            ICommandRunnerPdf commandRunner,
            IHelperPdf helperPdf
            )
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            this._options = options;
            _hubContext = hubContext;
            _session = session;
            _commandRunner = commandRunner;
            _helperPdf = helperPdf;
        }
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var filePath = _fileSystemWatcher.Path;
            var relativePath = HttpUtility.UrlDecode(Request.Path.ToString().Replace("api/mdexport//", string.Empty).Replace("/", @"\"));
            relativePath = HttpUtility.UrlDecode(Request.Path.ToString().Replace("api/mdexport/", string.Empty).Replace("/", @"\"));
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
                CurrentRoot = _fileSystemWatcher.Path
            };

            Directory.SetCurrentDirectory(Path.GetDirectoryName(filePath));            

            readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);

            Directory.SetCurrentDirectory(_fileSystemWatcher.Path);

            //pandoc - N--template = template.tex--variable mainfont = "Palatino Linotype"--variable sansfont = "Lucida Sans"--variable monofont = "Arial"--variable fontsize = 8pt--variable version = 2.0 __test.md--pdf - engine = pdflatex--toc - o example14pdf.tex
            // TODO: Use Pandoc to create document
            var currentGuid = _helperPdf.GetHashString(readText);

            System.IO.File.WriteAllText($".\\.md\\{currentGuid}.md", readText);

            return new ContentResult
            {
                ContentType = "text/html",
                Content = ""
            };
        }

    }
}
