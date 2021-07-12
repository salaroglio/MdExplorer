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

        public MdExportController(ILogger<MdExportController> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            ISession session,
            ICommandRunnerPdf commandRunner)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            this._options = options;
            _hubContext = hubContext;
            _session = session;
            _commandRunner = commandRunner;
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

            var monitoredMd = new MonitoredMDModel
            {
                Path = filePath,
                Name = Path.GetFileName(filePath)
            };

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

            return new ContentResult
            {
                ContentType = "text/html",
                Content = ""
            };
        }

    }
}
