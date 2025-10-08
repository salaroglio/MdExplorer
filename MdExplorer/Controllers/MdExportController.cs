using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Exports;
using MdExplorer.Features.Yaml.Models;
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
        private readonly IYamlParser<MdExplorerDocumentDescriptor> _yamlDocumentManager;
        private readonly IYamlDefaultGenerator _yamlDefaultGenerator;
        private readonly IWordTemplateService _wordTemplateService;

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
                IHelperPdf helperPdf,
                IYamlParser<MdExplorerDocumentDescriptor> yamlDocumentManager,
                IYamlDefaultGenerator yamlDefaultGenerator,
                IWorkLink[] modifiers,
                IHelper helper,
                IWordTemplateService wordTemplateService
            ) : base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner, modifiers, helper)
        {
            _helperPdf = helperPdf;
            _yamlDocumentManager = yamlDocumentManager;
            _yamlDefaultGenerator = yamlDefaultGenerator;
            _wordTemplateService = wordTemplateService;
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
                    AbsolutePathFile = filePath,
                    BaseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}",

                };

                readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);
                readText = _commandRunner.PrepareMetadataBasedOnMD(readText, requestInfo);

                // Ora il documento dovrebbe già avere YAML valido grazie al MdExplorerController
                var docDesc = _yamlDocumentManager.GetDescriptor(readText);

                // Inserisci pagine predefinite se configurate
                if (docDesc?.WordSection?.PredefinedPages != null)
                {
                    readText = await _wordTemplateService.InsertPredefinedPagesAsync(
                        readText, 
                        docDesc, 
                        _fileSystemWatcher.Path
                    );
                    
                    _logger.LogInformation("Pagine predefinite inserite per il documento {0}", filePath);
                }

                // Verifica che la directory .md esista prima di cambiare directory
                var mdTempDir = Path.Combine(_fileSystemWatcher.Path, ".md");
                
                if (!Directory.Exists(mdTempDir))
                {
                    Directory.CreateDirectory(mdTempDir);
                }
                
                Directory.SetCurrentDirectory(_fileSystemWatcher.Path);

                // TODO: Use Pandoc to create document
                string currentFilePdfPath;
                Process processStarted;

                //StartNewProcess(filePath, readText, "pdf", out currentFilePdfPath, out processStarted);
                var pandoc = new StartPandoc(new DocxPandocCommand(), _helperPdf, _yamlDocumentManager, _logger);
                pandoc.StartNewPandoc(filePath,_fileSystemWatcher.Path , readText, out currentFilePdfPath, out processStarted);

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
            catch (FileNotFoundException ex)
            {
                _logger.LogError($"Template non trovato: {ex.Message}");
                return new ContentResult
                {
                    ContentType = "application/json",
                    StatusCode = 404,
                    Content = $"{{\"error\":\"Template non trovato\",\"details\":\"{ex.Message.Replace("\"", "\\\"")}\"}}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Errore durante l'export: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                
                return new ContentResult
                {
                    ContentType = "application/json",
                    StatusCode = 500,
                    Content = $"{{\"error\":\"Errore durante l'export\",\"details\":\"{ex.Message.Replace("\"", "\\\"")}\"}}"
                };
            }
        }


        private void ProcessStarted_Exited(object? sender, EventArgs e)
        {
            if (sender is Process process)
            {
                if (process.ExitCode == 0)
                {
                    monitoredMd.Action = "Open Folder";
                    monitoredMd.StopExportTime = DateTime.Now;
                    _hubContext.Clients.Client(connectionId: monitoredMd.ConnectionId).SendAsync("pdfisready", monitoredMd).Wait();
                }
                else
                {
                    monitoredMd.Action = "Export Failed";
                    monitoredMd.Message = $"Export failed with code {process.ExitCode}";
                    _hubContext.Clients.Client(connectionId: monitoredMd.ConnectionId).SendAsync("exportfailed", monitoredMd).Wait();
                }
            }
            else
            {
                monitoredMd.Action = "Export Failed";
                monitoredMd.Message = "Unknown export error";
                _hubContext.Clients.Client(connectionId: monitoredMd.ConnectionId).SendAsync("exportfailed", monitoredMd).Wait();
            }
        }


        private class StartPandoc
        {
            private readonly ICreatePandocCommand<string, CommandParameter> _createPandocCommand;
            private readonly IHelperPdf _helperPdf;
            private readonly IYamlParser<MdExplorerDocumentDescriptor> _docSettingManager;
            private readonly ILogger<MdExportController> _logger;

            public StartPandoc(ICreatePandocCommand<string, CommandParameter> createPandocCommand,
                IHelperPdf helperPdf, IYamlParser<MdExplorerDocumentDescriptor> docSettingManager,
                ILogger<MdExportController> logger)
            {
                _createPandocCommand = createPandocCommand;
                _helperPdf = helperPdf;
                _docSettingManager = docSettingManager;
                _logger = logger;
            }

            public void StartNewPandoc(string filePath, string projectPath,
                    string readText, 
                    out string currentFilePdfPath, out Process processStarted)
            {
                var currentGuid = _helperPdf.GetHashString(readText);
                // Usa Path.Combine per essere cross-platform
                var currentFilePath = Path.Combine(".md", $"{currentGuid}.md");
                currentFilePdfPath = filePath.Replace("\\\\", "\\").Replace(".md", $".{_createPandocCommand.Extension}");
                
                System.IO.File.WriteAllText(currentFilePath, readText);
                
                
                var docDesc = _docSettingManager.GetDescriptor(readText);


                var processCommand = _createPandocCommand.CreatePandocCommand(new CommandParameter
                {
                    CurrentFilePath = currentFilePath,
                    CurrentFilePdfPath = currentFilePdfPath,
                    createTOC = docDesc.WordSection.WriteToc,
                    InheritsDocumentTemplate = docDesc.WordSection.TemplateSection.InheritFromTemplate,
                    DocumentTemplateType = docDesc.WordSection.TemplateSection.TemplateType,
                    CustomTemplate = docDesc.WordSection.TemplateSection.CustomTemplate,
                    DocumentHeader = docDesc.WordSection.DocumentHeader,
                    ProjectPath = projectPath,
                    MdFileName = filePath
                }
                );
                
                // Parsing del comando pandoc per estrarre eseguibile e argomenti
                // Il comando è nella forma: pandoc "input.md" -o "output.docx" --from markdown+implicit_figures ...
                var commandParts = ParseCommand(processCommand);
                
                // Eseguiamo pandoc direttamente (cross-platform)
                var processToStart = new ProcessStartInfo(commandParts.FileName)
                {
                    Arguments = commandParts.Arguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };
                
                processStarted = Process.Start(processToStart);
                
                if (processStarted != null)
                {
                    var output = processStarted.StandardOutput.ReadToEnd();
                    var error = processStarted.StandardError.ReadToEnd();
                    
                    if (!string.IsNullOrEmpty(error))
                    {
                        _logger.LogError($"Pandoc error: {error}");
                    }
                }
            }
            
            private (string FileName, string Arguments) ParseCommand(string command)
            {
                // Il comando inizia sempre con "pandoc", seguito dagli argomenti
                if (command.StartsWith("pandoc "))
                {
                    return ("pandoc", command.Substring(7)); // Rimuove "pandoc " dall'inizio
                }
                throw new InvalidOperationException($"Invalid pandoc command: {command}");
            }
        }

        private interface ICreatePandocCommand<R, P>
        {
            string Extension { get; }
            R CreatePandocCommand(P commandParameters);
        }

        private class CommandParameter
        {
            public string CurrentFilePath { get; set; }
            public string CurrentFilePdfPath { get; set; }
            public bool createTOC { get; set; } = true;
            public string DocumentHeader { get; set; } = "None";
            public string InheritsDocumentTemplate { get; set; }
            public string CustomTemplate { get; set; }
            public string DocumentTemplateType { get; set; }

            public string ProjectPath { get; set; }
            public string MdFileName { get; set; }
        }

        private class PdfPandoCommand : ICreatePandocCommand<string, CommandParameter>
        {
            public string Extension { get => "pdf"; }

            public string CreatePandocCommand(CommandParameter commandParam)
            {
                var setPdf = $@"--pdf-engine=pdflatex --template=.{
                    Path.DirectorySeparatorChar}.md{
                    Path.DirectorySeparatorChar}templates{
                    Path.DirectorySeparatorChar}pdf{
                    Path.DirectorySeparatorChar}eisvogel.tex";
                var processCommand = $@"pandoc ""{commandParam.CurrentFilePath}"" -o ""{commandParam.CurrentFilePdfPath}"" --from markdown+implicit_figures {setPdf}";
                return processCommand;
            }
        }

        private class DocxPandocCommand : ICreatePandocCommand<string, CommandParameter>
        {
            public string Extension => "docx";

            public string CreatePandocCommand(CommandParameter commandParam)
            {
                var createTocScriptCommand = string.Empty;
                if (commandParam.createTOC)
                {
                    createTocScriptCommand = "--toc";
                }
                var currentReferencePath = Path.Combine(".", ".md", "templates", "word", "reference.docx");
                if (commandParam.DocumentTemplateType == "inherits")
                {
                    currentReferencePath = Path.Combine(".", ".md", "templates", "word", 
                        $"{commandParam.InheritsDocumentTemplate}.docx");
                }
                if (commandParam.DocumentTemplateType == "custom")
                {
                    var mdFileDir = Path.GetDirectoryName(commandParam.MdFileName);
                    var mdFileName = Path.GetFileName(commandParam.MdFileName);
                    currentReferencePath = Path.Combine(mdFileDir, "assets", $"{mdFileName}.reference.docx");
                    
                    // Converti in path relativo rispetto al ProjectPath
                    if (currentReferencePath.StartsWith(commandParam.ProjectPath))
                    {
                        currentReferencePath = Path.Combine(".", 
                            currentReferencePath.Substring(commandParam.ProjectPath.Length).TrimStart(Path.DirectorySeparatorChar));
                    }
                }
                
                // Validazione esistenza template
                var absoluteTemplatePath = Path.IsPathRooted(currentReferencePath) 
                    ? currentReferencePath 
                    : Path.Combine(Directory.GetCurrentDirectory(), currentReferencePath.StartsWith($".{Path.DirectorySeparatorChar}") 
                        ? currentReferencePath.Substring(2) 
                        : currentReferencePath);
                    
                if (!System.IO.File.Exists(absoluteTemplatePath))
                {
                    throw new FileNotFoundException($"Template Word non trovato: {absoluteTemplatePath}. Percorso relativo: {currentReferencePath}");
                }
                  
                var processCommand = $@"pandoc ""{commandParam.CurrentFilePath}"" -o ""{commandParam.CurrentFilePdfPath}"" --from markdown+implicit_figures {createTocScriptCommand} --reference-doc ""{currentReferencePath}""";
                return processCommand;
            }
        }
    }
}
