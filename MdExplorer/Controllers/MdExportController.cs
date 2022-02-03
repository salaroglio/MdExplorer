using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
                IUserSettingsDB session,
                IEngineDB engineDB,
                ICommandRunnerPdf commandRunner,
                IHelperPdf helperPdf
            ) : base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner)
        {
            _helperPdf = helperPdf;
        }

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
                    CurrentRoot = _fileSystemWatcher.Path,
                    AbsolutePathFile = filePath
                    
                };

                readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);
                readText = _commandRunner.PrepareMetadataBasedOnMD(readText, requestInfo);

                Directory.SetCurrentDirectory(_fileSystemWatcher.Path);


                // TODO: Use Pandoc to create document
                string currentFilePdfPath;
                Process processStarted;

                //StartNewProcess(filePath, readText, "pdf", out currentFilePdfPath, out processStarted);
                var pandoc = new StartPandoc(new DocxPandocCommand(), _helperPdf);
                pandoc.StartNewPandoc(filePath, readText, out currentFilePdfPath, out processStarted);

                processStarted.EnableRaisingEvents = true;
                monitoredMd = new MonitoredMDModel
                {
                    Path = currentFilePdfPath,
                    Name = Path.GetFileName(currentFilePdfPath),
                    RelativePath = currentFilePdfPath.Replace(_fileSystemWatcher.Path, string.Empty),
                    ConnectionId = connectionId,
                    StartExportTime = DateTime.Now
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
            monitoredMd.StopExportTime = DateTime.Now;
            _hubContext.Clients.Client(connectionId: monitoredMd.ConnectionId).SendAsync("pdfisready", monitoredMd).Wait();
        }


        private class StartPandoc
        {
            private readonly ICreatePandocCommand<string, CommandParameter> _createPandocCommand;
            private readonly IHelperPdf _helperPdf;

            public StartPandoc(ICreatePandocCommand<string, CommandParameter> createPandocCommand, IHelperPdf helperPdf)
            {
                _createPandocCommand = createPandocCommand;
                _helperPdf = helperPdf;
            }

            public void StartNewPandoc(string filePath, string readText, out string currentFilePdfPath, out Process processStarted)
            {
                var currentGuid = _helperPdf.GetHashString(readText);
                var currentFilePath = $".\\.md\\{currentGuid}.md";
                currentFilePdfPath = filePath.Replace("\\\\", "\\").Replace(".md", $".{_createPandocCommand.Extension}");
                System.IO.File.WriteAllText(currentFilePath, readText);

                var processCommand = _createPandocCommand.CreatePandocCommand(new CommandParameter { CurrentFilePath = currentFilePath, CurrentFilePdfPath = currentFilePdfPath } );
                var finalCommand = $"/c {processCommand}";
                var processToStart = new ProcessStartInfo("cmd", finalCommand)
                {
                    CreateNoWindow = false
                };
                processStarted = Process.Start(processToStart);
            }
        }

        private interface ICreatePandocCommand<R,P>
        {
            string Extension { get; }
            R CreatePandocCommand(P commandParameters);
        }

        private class CommandParameter
        {
            public string CurrentFilePath { get; set; }
            public string CurrentFilePdfPath { get; set; }            
        }

        private class PdfPandoCommand : ICreatePandocCommand<string,CommandParameter>
        {
            public string Extension { get => "pdf"; }

            public string CreatePandocCommand(CommandParameter commandParam)
            {
                var setPdf = $@"--pdf-engine=pdflatex --template=.\.md\eisvogel.tex";
                var processCommand = $@"pandoc ""{commandParam.CurrentFilePath}"" -o ""{commandParam.CurrentFilePdfPath}"" --from markdown+implicit_figures {setPdf}";
                return processCommand;
            }
        }

        private class DocxPandocCommand : ICreatePandocCommand<string, CommandParameter>
        {
            public string Extension => "docx";

            public string CreatePandocCommand(CommandParameter commandParam)
            {
                var currentReferencePath = $".\\.md\\templates\\reference.docx";
                var processCommand = $@"pandoc ""{commandParam.CurrentFilePath}"" -o ""{commandParam.CurrentFilePdfPath}"" --from markdown+implicit_figures --reference-doc {currentReferencePath}";
                return processCommand;
            }
        }
    }
}
