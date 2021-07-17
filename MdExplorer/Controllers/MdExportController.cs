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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
    [Route("/api/mdexport/{*url}")]
    public class MdExportController : MdControllerBase<MdExportController>
    {
        private readonly IHelperPdf _helperPdf;
        /// <summary>
        /// Variabile di scambio dati con l'evento di chiusura del processo Pandoc
        /// </summary>
        private MonitoredMDModel monitoredMd;
        public MdExportController(ILogger<MdExportController> logger,
                FileSystemWatcher fileSystemWatcher,
                IOptions<MdExplorerAppSettings> options,
                IHubContext<MonitorMDHub> hubContext,
                ISession session,
                ICommandRunnerPdf commandRunner,
                IHelperPdf helperPdf
            ) : base(logger, fileSystemWatcher, options, hubContext, session, commandRunner)
        {
            _helperPdf = helperPdf;
        }

        //private readonly ILogger<MdExportController> _logger;
        //private readonly FileSystemWatcher _fileSystemWatcher;
        //private readonly IOptions<MdExplorerAppSettings> _options;
        //private readonly IHubContext<MonitorMDHub> _hubContext;
        //private readonly ISession _session;
        //private readonly ICommandRunner _commandRunner;


        //private ConcurrentDictionary<int, string> concurrentProcessInfo = new ConcurrentDictionary<int, string>();

        //public MdExportController(ILogger<MdExportController> logger,
        //    FileSystemWatcher fileSystemWatcher,
        //    IOptions<MdExplorerAppSettings> options,
        //    IHubContext<MonitorMDHub> hubContext,
        //    ISession session,
        //    ICommandRunnerPdf commandRunner,
        //    IHelperPdf helperPdf
        //    )
        //{
        //    _logger = logger;
        //    _fileSystemWatcher = fileSystemWatcher;
        //    this._options = options;
        //    _hubContext = hubContext;
        //    _session = session;
        //    _commandRunner = commandRunner;

        //}
        [HttpGet]
        public async Task<IActionResult> GetAsync(string connectionId)
        {
            try
            {
                var filePath = _fileSystemWatcher.Path;
                var relativePath = GetRelativePathFileSystem("mdexport");
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
                readText = _commandRunner.PrepareMetadataBasedOnMD(readText, requestInfo);

                Directory.SetCurrentDirectory(_fileSystemWatcher.Path);


                // TODO: Use Pandoc to create document
                var currentGuid = _helperPdf.GetHashString(readText);
                var currentFilePath = $".\\.md\\{currentGuid}.md";
                var currentFilePdfPath = filePath.Replace("\\\\", "\\").Replace(".md", ".pdf");
                System.IO.File.WriteAllText(currentFilePath, readText);
                var processCommand = $@"pandoc ""{currentFilePath}"" -o ""{currentFilePdfPath}"" --from markdown --pdf-engine=xelatex --template=.\.md\eisvogel.tex --listings --toc";
                var finalCommand = $"/c {processCommand}";
                var processToStart = new ProcessStartInfo("cmd", finalCommand)
                {

                    //RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    CreateNoWindow = false
                };


                var processStarted = Process.Start(processToStart);

                processStarted.EnableRaisingEvents = true;
                monitoredMd = new MonitoredMDModel
                {
                    Path = currentFilePdfPath,
                    Name = Path.GetFileName(currentFilePdfPath),
                    RelativePath = currentFilePdfPath.Replace(_fileSystemWatcher.Path, string.Empty),
                    ConnectionId = connectionId
                };

                processStarted.Exited += ProcessStarted_Exited;

                return new ContentResult
                {
                    ContentType = "application/json",
                    Content = "{\"message\":\"done\"}"
                };
            }
            catch (Exception ex)
            {
                var ms = ex.Message;
                throw;
            }
        }



        private void ProcessStarted_Exited(object? sender, EventArgs e)
        {
            monitoredMd.Action = "Open Folder";
            _hubContext.Clients.Client(connectionId: monitoredMd.ConnectionId).SendAsync("pdfisready", monitoredMd).Wait();
        }
    }
}
