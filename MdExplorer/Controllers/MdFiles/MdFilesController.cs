using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using MdExplorer;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Entities.ProjectDB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Controllers;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.ProjectBody;
using MdExplorer.Features.Refactoring;
using MdExplorer.Features.Refactoring.Analysis;
using MdExplorer.Features.Refactoring.Analysis.Interfaces;
using MdExplorer.Features.snippets;
using MdExplorer.Features.Utilities;
using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.MdFiles;
using MdExplorer.Service.Controllers.MdFiles.Models;
using MdExplorer.Service.Controllers.MdFiles.ModelsDto;
using MdExplorer.Service.Models;
using MdExplorer.Service.Utilities;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlX.XDevAPI;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using NHibernate.Linq;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
#if WINDOWS_FORMS_AVAILABLE
using System.Windows.Forms;
#endif
using MdExplorer.Utilities;
using static MdExplorer.Service.Controllers.RefactoringFilesController;
using static System.Net.WebRequestMethods;
using MdExplorer.Abstractions.Services;
using MdExplorer.Service.Services;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Service.Controllers.MdFiles
{
    [ApiController]
    [Route("api/mdfiles/{action}")]
    public class MdFilesController : MdControllerBase<MdFilesController>
    {
        
        
        private readonly IHelper _helper;

        private readonly IGoodMdRule<FileInfoNode>[] _goodRules;
        private readonly IProjectDB _projectDB;
        private readonly ISnippet<DictionarySnippetParam>[] _snippets;
        private readonly ProjectBodyEngine _projectBodyEngine;
        private readonly IYamlParser<MdExplorerDocumentDescriptor> _yamlDocumentManager;
        private readonly RefactoringManager _refactoringManager;
        private readonly ProcessUtil _visualStudioCode;
        private readonly IMdIgnoreService _mdIgnoreService;
        private readonly FoldersIgnoreService _foldersIgnoreService;
        

        public MdFilesController(FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            ILogger<MdFilesController> logger,
            IEngineDB engineDB, 
            IWorkLink[] getModifiers, 
            IHelper helper,
            IUserSettingsDB userSettingsDB,
            IHubContext<MonitorMDHub> hubContext,
            IGoodMdRule<FileInfoNode>[] GoodRules,
            IProjectDB projectDB,
            ICommandRunnerHtml commandRunner,

            ISnippet<DictionarySnippetParam>[] snippets,
            ProjectBodyEngine projectBodyEngine,
            IYamlParser<MdExplorerDocumentDescriptor> yamlDocumentManager,
        RefactoringManager refactoringManager,
        ProcessUtil visualStudioCode,
        IMdIgnoreService mdIgnoreService,
        FoldersIgnoreService foldersIgnoreService
            ) : base(logger, fileSystemWatcher, options, hubContext, userSettingsDB, engineDB, commandRunner, getModifiers, helper)
        {

            _goodRules = GoodRules;
            _projectDB = projectDB;
            _snippets = snippets;
            _projectBodyEngine = projectBodyEngine;
            _yamlDocumentManager = yamlDocumentManager;
            _refactoringManager = refactoringManager;
            _visualStudioCode = visualStudioCode;
            _mdIgnoreService = mdIgnoreService;
            _foldersIgnoreService = foldersIgnoreService;
        }

        [HttpGet]
        public IActionResult GetDocumentSettings([FromQuery] string fullPath)
        {
            var text = System.IO.File.ReadAllText(fullPath);
            var document = _yamlDocumentManager.GetDescriptor(text);
            return Ok(document);
        }

        [HttpPost]
        public IActionResult SetNumberedImage([FromBody] SetNumberedImageRequest data)
        {
            return Ok(data);
        }

        [HttpPost]
        public IActionResult CreateSnapshot([FromBody] CreateSnapshtoRequest data)
        {
            return Ok(data);
        }

        /// <summary>
        /// MdLink:
        /// </summary>
        /// <param name="mdFile"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OpenFileInApplication([FromBody] OpenFileInApplicationcs data)
        {
            var fullpath = HttpUtility.UrlDecode(data.FullPath).Replace('/', Path.DirectorySeparatorChar);
            fullpath = CrossPlatformPath.NormalizePath(fullpath);

            // Open file with default application
            CrossPlatformProcess.OpenFile(fullpath);
            _hubContext.Clients.Client(connectionId: data.ConnectionId).SendAsync("openingApplication", Path.GetFileName(fullpath)).Wait();            
            return Ok(new { message = "done" });
        }

        [HttpPost]
        public IActionResult MoveMdFile([FromBody] RequestMoveMdFile requestMoveMdFile)
        {
            _logger.LogInformation("[MoveMdFile] Method called");
            _logger.LogInformation($"[MoveMdFile] MdFile is null: {requestMoveMdFile?.MdFile == null}");
            _logger.LogInformation($"[MoveMdFile] DestinationPath: {requestMoveMdFile?.DestinationPath}");

            if (requestMoveMdFile?.MdFile == null || string.IsNullOrEmpty(requestMoveMdFile.DestinationPath))
            {
                _logger.LogError("[MoveMdFile] Invalid request data");
                return BadRequest(new { error = "Invalid request data" });
            }

            try
            {
                var projectBasePath = _fileSystemWatcher.Path;
                var fromRelativePathFileName = requestMoveMdFile.MdFile.RelativePath.Substring(1);
                var fromFullPathFileName = Path.Combine(projectBasePath, fromRelativePathFileName);

                _logger.LogInformation($"[MoveMdFile] From: {fromFullPathFileName}");

                var fileName = requestMoveMdFile.MdFile.Name;
                var relativeDestinationPath = requestMoveMdFile.DestinationPath
                                        .Replace(_fileSystemWatcher.Path, "").Substring(1);
                var toRelativePathFileName = Path.Combine(relativeDestinationPath, fileName);
                var toFullPathFileName = Path.Combine(_fileSystemWatcher.Path, toRelativePathFileName);

                _logger.LogInformation($"[MoveMdFile] To: {toFullPathFileName}");

                MoveFileOnFilesystem(fromFullPathFileName, toFullPathFileName);

                _engineDB.BeginTransaction();
                _refactoringManager.RenameTheMdFileIntoEngineDB(projectBasePath,
                    fromRelativePathFileName, toRelativePathFileName);

                var refSourceAct = _refactoringManager
                    .SaveRefactoringActionForMoveFile(fileName,
                    Path.GetDirectoryName(fromFullPathFileName),
                    requestMoveMdFile.DestinationPath); // Save the concept of change


                _refactoringManager.SetExternalLinks(
                   toRelativePathFileName,
                   refSourceAct);

                _refactoringManager.SetInternalLinks(
                    toRelativePathFileName,
                    projectBasePath,
                    refSourceAct);
                // After save, get back the list of links inside involved files
                _refactoringManager.UpdateAllInvolvedFilesAndReferencesToDB(refSourceAct); //newFullPath,

                _engineDB.Commit();
                _logger.LogInformation("[MoveMdFile] Completed successfully");
                return Ok(new { message = "done" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MoveMdFile] Error during move operation");
                _engineDB.Rollback();
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private void MoveFileOnFilesystem(string oldFullPath, string newFullPath)
        {

            // gestisci il rename di un file
            System.IO.File.Move(oldFullPath, newFullPath, true);
            if (_visualStudioCode.CurrentVisualStudio != null)
            {
                _visualStudioCode.ReopenVisualStudioCode(newFullPath);
            }
        }

        [HttpPost]
        public IActionResult OpenInheritingTemplateWord([FromBody] RequestOpenInheritingTemplateWord request)
        {
            var templatePath = Path.Combine(_fileSystemWatcher.Path, ".md", "templates", "word",
                $"{request.TemplateName}.docx");
            
            // Open Word template with default application
            CrossPlatformProcess.OpenFile(templatePath);
            return Ok(new { message = "done" });
        }

        [HttpGet]
        public IActionResult getTextFromClipboard()
        {
#if WINDOWS_FORMS_AVAILABLE
            var textToGet = string.Empty;

            var myTask = ExtensionTask.CreateSTATask(async () =>
            {
                textToGet = Clipboard.GetText();
            });
            myTask.Wait();

            textToGet = textToGet.Contains("http") && textToGet.Contains("git") ? textToGet : string.Empty;
#else
            // Clipboard functionality not available on Linux without GUI
            var textToGet = string.Empty;
#endif

            return Ok(new { url = textToGet });
        }

        [HttpPost]
        public IActionResult AddExistingFileToMDEProject([FromBody] RequestAddExistingFileToMdeproject request)
        {
            // Generate a function that copy the file from request.FullPath into request.MdFIle.FullPath adding the "\assets" folder,
            // creating the folder if not exists
            var folderContainintMdFile = Path.GetDirectoryName(request.MdFile.FullPath);
            var destinationDirectory = Directory.CreateDirectory(folderContainintMdFile + Path.DirectorySeparatorChar + "assets");
            
            var trueFileName = Path.GetFileNameWithoutExtension(request.FullPath.Replace(" ", "-")) + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(request.FullPath);
            var fullPathFileName = destinationDirectory.FullName 
                + Path.DirectorySeparatorChar + trueFileName
                ;
            _fileSystemWatcher.EnableRaisingEvents = false;
            try
            {
                System.IO.File.Copy(request.FullPath, fullPathFileName);
            }
            catch (IOException ex)
            {
                _logger.LogInformation(ex.Message);
            }
           
            _fileSystemWatcher.EnableRaisingEvents = true;
            var allText = System.IO.File.ReadAllText(request.MdFile.FullPath);
            //We have to set an absolute path
            var relativePathMDE = fullPathFileName.Replace(_fileSystemWatcher.Path, string.Empty).Replace("\\", "/");
            var newLineTextToAdd = @$"[{Path.GetFileName(request.FullPath)}]({relativePathMDE})";
            allText = string.Concat(allText, Environment.NewLine, newLineTextToAdd);
            System.IO.File.WriteAllText(request.MdFile.FullPath, allText);
            return  Ok(new { message = "done" });

        }

        [HttpPost]
        public async Task<IActionResult> PasteFromClipboard([FromBody] RequestPasteFromClipboard fileData)
        {
            _logger.LogInformation("PasteFromClipboard called with fileName: {FileName}, filePath: {FilePath}", 
                fileData?.FileName, fileData?.FileInfoNode?.FullPath);
            
            try
            {
                // Use the new cross-platform clipboard utility
                var clipboardResult = await MdExplorer.Utilities.CrossPlatformClipboard.GetImageAsync();
                
                if (!clipboardResult.Success)
                {
                    _logger.LogWarning($"PasteFromClipboard failed: {clipboardResult.ErrorMessage}");
                    return BadRequest(new 
                    { 
                        message = clipboardResult.ErrorMessage,
                        platformHint = clipboardResult.PlatformHint 
                    });
                }
                
                // Common code for all platforms to save the image
                _fileSystemWatcher.EnableRaisingEvents = false;
                
                try
                {
                    // Sanitize filename
                    var ruleReg = new Regex("(^(PRN|AUX|NUL|CON|COM[1-9]|LPT[1-9]|(\\.+)$)(\\..*)?$)|(([\\x00-\\x1f\\\\?*:\";‌​|/<>])+)|([\\. ]+)");
                    var sanitizedTitle = ruleReg.Replace(fileData.FileName, "-").Replace(" ", "-") + ".png";
                    
                    // Create assets directory using Path.Combine for cross-platform compatibility
                    var assetsDirectory = Path.Combine(
                        Path.GetDirectoryName(fileData.FileInfoNode.FullPath),
                        "assets"
                    );
                    Directory.CreateDirectory(assetsDirectory);
                    
                    // Save image to file system
                    var imagePath = Path.Combine(assetsDirectory, sanitizedTitle);
                    await System.IO.File.WriteAllBytesAsync(imagePath, clipboardResult.ImageData);
                    
                    // Update Markdown file
                    var mdContent = await System.IO.File.ReadAllTextAsync(fileData.FileInfoNode.FullPath);
                    
                    // Use forward slashes for the markdown path (compatible with all platforms)
                    var relativePath = fileData.FileInfoNode.Path
                        .Replace(fileData.FileInfoNode.Name, string.Empty)
                        .Replace("\\", "/");
                    var imageMarkdown = $"![{fileData.FileName}]({relativePath}assets/{sanitizedTitle})";
                    
                    mdContent = string.Concat(mdContent, Environment.NewLine, imageMarkdown);
                    await System.IO.File.WriteAllTextAsync(fileData.FileInfoNode.FullPath, mdContent);
                    
                    _logger.LogInformation($"Successfully pasted image from clipboard: {sanitizedTitle}");
                    
                    // Send SignalR notification to refresh the document
                    try
                    {
                        var monitoredMd = new MonitoredMDModel
                        {
                            Path = fileData.FileInfoNode.Path.Replace("\\", "/"),
                            Name = fileData.FileInfoNode.Name,
                            RelativePath = fileData.FileInfoNode.Path,
                            FullPath = fileData.FileInfoNode.FullPath,
                            FullDirectoryPath = Path.GetDirectoryName(fileData.FileInfoNode.FullPath)
                        };
                        
                        await _hubContext.Clients.All.SendAsync("markdownfileischanged", monitoredMd);
                        _logger.LogInformation($"SignalR notification 'markdownfileischanged' sent for: {fileData.FileInfoNode.Path}");
                    }
                    catch (Exception signalrEx)
                    {
                        _logger.LogWarning($"Failed to send SignalR notification: {signalrEx.Message}");
                    }
                    
                    return Ok(new { message = "Image pasted successfully", fileName = sanitizedTitle });
                }
                finally
                {
                    _fileSystemWatcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PasteFromClipboard");
                return StatusCode(500, new { message = "Internal error while pasting image", error = ex.Message });
            }
        }


        [HttpPost]
        public IActionResult OpenCustomWordTemplate([FromBody] FileInfoNode fileData)
        {
            // copy reference.docx
            var fromReference = _fileSystemWatcher.Path +
                Path.DirectorySeparatorChar +
                ".md" +
                Path.DirectorySeparatorChar +
                "templates" +
                Path.DirectorySeparatorChar +
                "word" +
                Path.DirectorySeparatorChar +
                "reference.docx";
            var toReference = Path.GetDirectoryName(fileData.FullPath) +
                Path.DirectorySeparatorChar +
                "assets";
            Directory.CreateDirectory(toReference);
            toReference += Path.DirectorySeparatorChar +
                    fileData.Name +
                    ".reference.docx";

            if (!System.IO.File.Exists(toReference))
            {
                System.IO.File.Copy(fromReference, toReference);
            }
            
            // Open Word reference template with default application
            CrossPlatformProcess.OpenFile(toReference);

            //var p = new Process();
            //p.StartInfo.FileName = toReference;
            //p.StartInfo.FileName ="WinWord.exe" ;
            //p.Start();
            return Ok(new { message = "done" });
        }



        [HttpPost]
        public IActionResult SetDocumentSettings([FromBody] ChangeDocumentSettings data)
        {
            var descriptor = data.DocumentDescriptor;
            var mdFile = data.MdFile;
            var text = System.IO.File.ReadAllText(mdFile.FullPath);
            var textChanged = _yamlDocumentManager.SetDescriptor(descriptor, text);
            System.IO.File.WriteAllText(mdFile.FullPath, textChanged);
            return Ok(new { message = "done" });
        }

        [HttpGet]
        public IActionResult GetLandingPage()
        {
            var dal = _projectDB.GetDal<ProjectSetting>();
            var landingPage = dal.GetList().Where(_ => _.Name == "LandingPageFilePath").First();

            var toReturn = landingPage.LandingPages.Count() != 0 ? new FileInfoNode
            {
                Expandable = landingPage.LandingPages.First().Expandable,
                FullPath = landingPage.LandingPages.First().FullPath,
                Level = landingPage.LandingPages.First().Level,
                Name = landingPage.LandingPages.First().Name,
                Path = landingPage.LandingPages.First().Path,
                RelativePath = landingPage.LandingPages.First().RelativePath,
                Type = landingPage.LandingPages.First().Type,
                DataText = landingPage.LandingPages.First().DataText,
            } : null;
            return Ok(toReturn);
        }


        [HttpPost]
        public async Task<IActionResult> OpenFolderOnFileExplorer()
        {
            Console.WriteLine("[MdFilesController] OpenFolderOnFileExplorer called - NO PARAMS VERSION");
            
            try
            {
                // Leggi il body manualmente
                using (var reader = new System.IO.StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    Console.WriteLine($"[MdFilesController] Raw body: {body}");
                    
                    // Deserializza manualmente
                    var fileData = System.Text.Json.JsonSerializer.Deserialize<FileInfoNode>(body);
                    Console.WriteLine($"[MdFilesController] Deserialized fileData.FullPath: {fileData?.FullPath}");
                    
                    if (fileData == null || string.IsNullOrEmpty(fileData.FullPath))
                    {
                        Console.WriteLine("[MdFilesController] ERROR: fileData or FullPath is null/empty");
                        return BadRequest(new { error = "Invalid file data or path" });
                    }
            
                    // Open folder containing the file
                    var folderPath = Path.GetDirectoryName(fileData.FullPath);
                    Console.WriteLine($"[MdFilesController] Extracted folderPath: {folderPath}");
                    
                    var result = CrossPlatformProcess.OpenFolder(folderPath);
                    Console.WriteLine($"[MdFilesController] CrossPlatformProcess.OpenFolder returned: {result}");
                    
                    if (result)
                    {
                        return Ok(new { message = "done", success = true });
                    }
                    else
                    {
                        return StatusCode(500, new { error = "Failed to open folder", path = folderPath });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MdFilesController] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Console.WriteLine($"[MdFilesController] Stack: {ex.StackTrace}");
                return StatusCode(500, new { error = "Exception occurred", message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile()
        {
            Console.WriteLine("[MdFilesController] DeleteFile called");
            
            try
            {
                // Leggi il body manualmente
                using (var reader = new System.IO.StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    Console.WriteLine($"[MdFilesController] DeleteFile raw body: {body}");
                    
                    // Deserializza manualmente
                    var fileData = System.Text.Json.JsonSerializer.Deserialize<FileInfoNode>(body);
                    Console.WriteLine($"[MdFilesController] DeleteFile fileData.FullPath: {fileData?.FullPath}");
                    
                    if (fileData == null || string.IsNullOrEmpty(fileData.FullPath))
                    {
                        Console.WriteLine("[MdFilesController] DeleteFile ERROR: fileData or FullPath is null/empty");
                        return BadRequest(new { error = "Invalid file data or path" });
                    }
                    
                    _fileSystemWatcher.EnableRaisingEvents = false;
                    System.IO.File.Delete(fileData.FullPath);
                    _fileSystemWatcher.EnableRaisingEvents = true;
                    Console.WriteLine($"[MdFilesController] File deleted successfully: {fileData.FullPath}");
                    return Ok(new { message = "done" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MdFilesController] DeleteFile EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                // In caso di errore, assicurati di riabilitare il FileSystemWatcher
                _fileSystemWatcher.EnableRaisingEvents = true;
                return StatusCode(500, new { message = $"Error deleting file: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetLandingPage()
        {
            Console.WriteLine("[MdFilesController] SetLandingPage called");
            
            try
            {
                // Leggi il body manualmente
                using (var reader = new System.IO.StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    Console.WriteLine($"[MdFilesController] SetLandingPage raw body: {body}");
                    
                    // Deserializza manualmente
                    var fileData = System.Text.Json.JsonSerializer.Deserialize<FileInfoNode>(body);
                    Console.WriteLine($"[MdFilesController] SetLandingPage fileData.FullPath: {fileData?.FullPath}");
                    
                    if (fileData == null || string.IsNullOrEmpty(fileData.FullPath))
                    {
                        Console.WriteLine("[MdFilesController] SetLandingPage ERROR: fileData or FullPath is null/empty");
                        return BadRequest(new { error = "Invalid file data or path" });
                    }
                    
                    _projectDB.BeginTransaction();

                    var dal = _projectDB.GetDal<ProjectSetting>();
                    var landingPage = dal.GetList().Where(_ => _.Name == "LandingPageFilePath").First();
                    landingPage.ValueString = fileData.FullPath;
                    dal.Save(landingPage);
                    var dalDetails = _projectDB.GetDal<ProjectFileInfoNode>();
                    var landingPageDetails = dalDetails.GetList().Where(_ => _.ProjectSetting == landingPage)
                        .FirstOrDefault();
                    if (landingPageDetails == null)
                    {
                        landingPageDetails = new ProjectFileInfoNode
                        {
                            ProjectSetting = landingPage,
                            Level = fileData.Level,
                            Expandable = fileData.Expandable,
                            Name = fileData.Name,
                            FullPath = fileData.FullPath,
                            Path = fileData.Path,
                            RelativePath = fileData.RelativePath,
                            Type = fileData.Type,
                            DataText = fileData.DataText,
                        };
                    }
                    else
                    {
                        landingPageDetails.Level = fileData.Level;
                        landingPageDetails.Expandable = fileData.Expandable;
                        landingPageDetails.Name = fileData.Name;
                        landingPageDetails.FullPath = fileData.FullPath;
                        landingPageDetails.Path = fileData.Path;
                        landingPageDetails.RelativePath = fileData.RelativePath;
                        landingPageDetails.Type = fileData.Type;
                        landingPageDetails.DataText = fileData.DataText;
                    }



                    dalDetails.Save(landingPageDetails);

                    _projectDB.Commit();
                    Console.WriteLine($"[MdFilesController] SetLandingPage completed successfully");
                    return Ok(new { message = "Done" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MdFilesController] SetLandingPage EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                _projectDB.Rollback();
                return StatusCode(500, new { message = $"Error setting landing page: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult SetDevelopmentTags([FromBody] SetDevelopmentTagsRequest request)
        {
            Console.WriteLine("[MdFilesController] SetDevelopmentTags called");
            try
            {
                Console.WriteLine($"[MdFilesController] SetDevelopmentTags request: FolderPath={request?.FolderPath}, Tags={string.Join(",", request?.Tags ?? new List<string>())}");

                if (request == null || string.IsNullOrEmpty(request.FolderPath) || string.IsNullOrEmpty(request.ProjectRoot))
                {
                    Console.WriteLine("[MdFilesController] SetDevelopmentTags ERROR: Invalid request data");
                    return BadRequest(new { error = "Invalid request data - missing FolderPath or ProjectRoot" });
                }

                // Calculate relative path from project root
                var relativePath = request.FolderPath.Replace(request.ProjectRoot, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Console.WriteLine($"[MdFilesController] Relative path: {relativePath}");

                // Path to .development.yml in project root
                var devConfigPath = Path.Combine(request.ProjectRoot, ".development.yml");
                Console.WriteLine($"[MdFilesController] Config file path: {devConfigPath}");

                // Load existing config or create new one
                DevelopmentConfig config;
                if (System.IO.File.Exists(devConfigPath))
                {
                    var yamlContent = System.IO.File.ReadAllText(devConfigPath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    config = deserializer.Deserialize<DevelopmentConfig>(yamlContent) ?? new DevelopmentConfig();
                }
                else
                {
                    config = new DevelopmentConfig();
                }

                // Update or add folder entry
                var existingFolder = config.Folders.FirstOrDefault(f => f.Path == relativePath);
                if (existingFolder != null)
                {
                    existingFolder.Tags = request.Tags ?? new List<string>();
                }
                else
                {
                    config.Folders.Add(new DevelopmentFolder
                    {
                        Path = relativePath,
                        Tags = request.Tags ?? new List<string>()
                    });
                }

                // Remove folders with no tags
                config.Folders.RemoveAll(f => f.Tags == null || f.Tags.Count == 0);

                // Save config
                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var newYamlContent = serializer.Serialize(config);
                System.IO.File.WriteAllText(devConfigPath, newYamlContent);

                _logger.LogInformation($"Development tags saved for folder: {relativePath}");
                Console.WriteLine($"[MdFilesController] SetDevelopmentTags completed successfully");
                return Ok(new { message = "Development tags saved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting development tags");
                Console.WriteLine($"[MdFilesController] SetDevelopmentTags EXCEPTION: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class SetDevelopmentTagsRequest
        {
            public string FolderPath { get; set; }
            public string ProjectRoot { get; set; }
            public List<string> Tags { get; set; }
        }

        private string GetSystemRootPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, return C:\ or the drive where the app is running
                return Path.GetPathRoot(Directory.GetCurrentDirectory()) ?? @"C:\";
            }
            else
            {
                // On Linux/Mac, return root /
                return "/";
            }
        }

        [HttpGet]
        public IActionResult GetDynFoldersDocument([FromQuery] string path, string level)
        {
            var currentPath = path == "root" ? GetSystemRootPath() : path;
            currentPath = path == "project" ? _fileSystemWatcher.Path : currentPath;
            currentPath = path == "documents" ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : currentPath;

            var currentLevel = Convert.ToInt32(level);
            var list = new List<IFileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath).Where(_=>!Path.GetFileName(_).StartsWith(".")))
            {
                // Check if folder should be ignored based on .mdFoldersIgnore configuration
                if (_foldersIgnoreService.ShouldIgnoreFolder(itemFolder))
                {
                    continue;
                }

                if (!IsSymbolic(itemFolder) && !IsHidden(itemFolder))
                {
                    try
                    {
                        var node = new FileInfoNode
                        {
                            Name = Path.GetFileName(itemFolder),
                            FullPath = itemFolder,
                            Path = itemFolder,
                            Level = currentLevel,
                            Type = "folder",
                            Expandable = Directory.GetDirectories(itemFolder).Count() > 0
                        };

                        list.Add(node);
                    }
                    catch (Exception)
                    {

                        // Do nothing
                    }

                }


            }


            return Ok(list);
        }

        [HttpGet]
        public IActionResult GetDynFoldersAndFilesDocument([FromQuery] string path, string level)
        {
            var currentPath = path == "root" ? GetSystemRootPath() : path;
            currentPath = path == "project" ? _fileSystemWatcher.Path : currentPath;
            currentPath = path == "documents" ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : currentPath;

            var currentLevel = Convert.ToInt32(level);
            var list = new List<IFileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath).Where(_ => !Path.GetFileName(_).StartsWith(".")))
            {
                // Check if folder should be ignored based on .mdFoldersIgnore configuration
                if (_foldersIgnoreService.ShouldIgnoreFolder(itemFolder))
                {
                    continue;
                }

                if (!IsSymbolic(itemFolder) && !IsHidden(itemFolder))
                {
                    try
                    {
                        var node = new FileInfoNode
                        {
                            Name = Path.GetFileName(itemFolder),
                            FullPath = itemFolder,
                            Path = itemFolder,
                            Level = currentLevel,
                            Type = "folder",
                            Expandable = true //Directory.GetDirectories(itemFolder).Count() > 0
                        };

                        list.Add(node);
                    }
                    catch (Exception)
                    {

                        // Do nothing
                    }

                }


            }// End foreach for folders

            foreach (var itemFile in Directory.GetFiles(currentPath).Where(
                _ => Path.GetExtension(_) != ".dll" &&
                    Path.GetExtension(_) != ".exe" &&
                    Path.GetExtension(_) != ".sys" &&
                    Path.GetExtension(_) != ".tmp" ))
            {
                var node = new FileInfoNode
                {
                    Name = Path.GetFileName(itemFile),
                    FullPath = itemFile,
                    Path = itemFile,
                    Level = currentLevel,
                    Expandable = false,
                    Type = "file"
                };


                list.Add(node);
            }

            _logger.LogWarning($"[MdFilesController.GetDynFoldersAndFilesDocument] 🏁 END - Returning {list.Count} items");
            return Ok(list);
        }

        private bool IsSymbolic(string path)
        {
            try
            {
                FileInfo pathInfo = new FileInfo(path);
                return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }
            catch (Exception)
            {

                return true;
            }

        }

        private bool IsHidden(string path)
        {
            try
            {
                FileInfo pathInfo = new FileInfo(path);
                return pathInfo.Attributes.HasFlag(FileAttributes.Hidden);
            }
            catch (Exception ex)
            {
                return true;

            }
        }




        [HttpGet]
        public IActionResult GetSpecialFolders()
        {
            try
            {
                var folders = new List<object>();

                // Desktop
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (Directory.Exists(desktopPath))
                {
                    folders.Add(new { 
                        name = "Desktop", 
                        path = desktopPath,
                        icon = "desktop_windows"
                    });
                }

                // Documents
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (Directory.Exists(documentsPath))
                {
                    folders.Add(new { 
                        name = "Documents", 
                        path = documentsPath,
                        icon = "folder_shared"
                    });
                }

                // Downloads (try multiple common locations)
                var downloadsPaths = new[]
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Descargas"), // Spanish
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Téléchargements") // French
                };

                foreach (var downloadsPath in downloadsPaths)
                {
                    if (Directory.Exists(downloadsPath))
                    {
                        folders.Add(new { 
                            name = "Downloads", 
                            path = downloadsPath,
                            icon = "cloud_download"
                        });
                        break;
                    }
                }

                // Project folder
                if (Directory.Exists(_fileSystemWatcher.Path))
                {
                    folders.Add(new { 
                        name = "Project", 
                        path = _fileSystemWatcher.Path,
                        icon = "work"
                    });
                }

                return Ok(folders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting special folders");
                return Ok(new object[0]); // Return empty array on error
            }
        }

        [HttpGet]
        public IActionResult GetDrives()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && (d.DriveType == DriveType.Fixed || d.DriveType == DriveType.Network))
                    .Select(d => new {
                        name = d.Name.TrimEnd(Path.DirectorySeparatorChar),
                        path = d.RootDirectory.FullName,
                        icon = d.DriveType == DriveType.Network ? "lan" : "storage",
                        label = string.IsNullOrEmpty(d.VolumeLabel) ? d.Name : d.VolumeLabel,
                        totalSize = d.TotalSize,
                        freeSpace = d.AvailableFreeSpace,
                        driveType = d.DriveType.ToString()
                    });
                return Ok(drives);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drives");
                return Ok(new object[0]); // Return empty array on error
            }
        }

        [HttpGet]
        public IActionResult GetNetworkShares()
        {
            try
            {
                var networkShares = new List<object>();
                
                // Get all network drives (mapped drives on Windows)
                var networkDrives = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Network && d.IsReady);
                
                foreach (var drive in networkDrives)
                {
                    try
                    {
                        // Get the UNC path for mapped drives if possible
                        var driveLetter = drive.Name.TrimEnd(Path.DirectorySeparatorChar);
                        
                        networkShares.Add(new
                        {
                            name = string.IsNullOrEmpty(drive.VolumeLabel) ? driveLetter : $"{drive.VolumeLabel} ({driveLetter})",
                            path = drive.RootDirectory.FullName,
                            icon = "folder_shared",
                            isNetworkDrive = true,
                            driveLetter = driveLetter
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Could not access network drive {drive.Name}");
                    }
                }
                
                // Check for Linux mount points (typically in /mnt or /media)
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) ||
                    System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    // Check common mount directories
                    var mountDirs = new[] { "/mnt", "/media", "/Volumes" }; // /Volumes for macOS
                    
                    foreach (var mountDir in mountDirs)
                    {
                        if (Directory.Exists(mountDir))
                        {
                            try
                            {
                                var subdirs = Directory.GetDirectories(mountDir);
                                foreach (var dir in subdirs)
                                {
                                    var dirInfo = new DirectoryInfo(dir);
                                    // Add mounted directories as network shares
                                    // Typically network mounts on Linux are in /mnt or /media
                                    networkShares.Add(new
                                    {
                                        name = $"{dirInfo.Name} (mount)",
                                        path = dir,
                                        icon = "folder_shared",
                                        isNetworkDrive = true,
                                        mountPoint = mountDir
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"Could not access mount directory {mountDir}");
                            }
                        }
                    }
                }
                // Also check for /mnt on Windows (WSL scenarios)
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    // Check if /mnt exists (could be WSL)
                    var wslMountPath = Path.Combine(Path.GetPathRoot(Environment.SystemDirectory), "mnt");
                    if (Directory.Exists(wslMountPath))
                    {
                        try
                        {
                            var subdirs = Directory.GetDirectories(wslMountPath);
                            foreach (var dir in subdirs)
                            {
                                var dirInfo = new DirectoryInfo(dir);
                                networkShares.Add(new
                                {
                                    name = $"{dirInfo.Name} (WSL mount)",
                                    path = dir,
                                    icon = "folder_shared",
                                    isNetworkDrive = true,
                                    isWslMount = true
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not access WSL mount directory");
                        }
                    }
                }
                
                return Ok(networkShares);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting network shares");
                return Ok(new object[0]); // Return empty array on error
            }
        }

        [HttpGet]
        public IActionResult GetFoldersDocument()
        {
            var currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); ;

            var list = new List<IFileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath)
                    .Where(_ => !_.Contains("Immagini") &&
                    !_.Contains("Musica") &&
                    !_.Contains("Video"))
                    )
            {
                var node = CreateNodeFolderOnly(itemFolder);
                list.Add(node);

            }

            return Ok(list);
        }
        private string signalRConnectionId;
        
        private void IndexAllMarkdownFiles()
        {
            try
            {
                _logger.LogInformation("[IndexAllMarkdownFiles] Starting initial indexing of all markdown files");
                
                var currentPath = _fileSystemWatcher.Path;
                if (string.IsNullOrEmpty(currentPath) || currentPath == AppDomain.CurrentDomain.BaseDirectory)
                {
                    _logger.LogWarning("[IndexAllMarkdownFiles] Invalid path, skipping indexing");
                    return;
                }
                
                _engineDB.BeginTransaction();
                var markdownFileDal = _engineDB.GetDal<MarkdownFile>();
                
                // Trova tutti i file .md ricorsivamente
                var allMdFiles = Directory.GetFiles(currentPath, "*.md", SearchOption.AllDirectories)
                    .Where(f => !f.Contains(Path.DirectorySeparatorChar + ".md" + Path.DirectorySeparatorChar)) // Esclude la cartella .md
                    .ToList();
                
                _logger.LogInformation($"[IndexAllMarkdownFiles] Found {allMdFiles.Count} markdown files to index");
                
                foreach (var filePath in allMdFiles)
                {
                    try
                    {
                        // Crea il record per ogni file markdown
                        var markdownFile = new MarkdownFile
                        {
                            FileName = Path.GetFileName(filePath),
                            Path = filePath,  // Usa il path completo assoluto
                            FileType = "file"
                        };
                        
                        markdownFileDal.Save(markdownFile);
                        _logger.LogDebug($"[IndexAllMarkdownFiles] Indexed: {filePath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[IndexAllMarkdownFiles] Error indexing file: {filePath}");
                    }
                }
                
                _engineDB.Commit();
                _logger.LogInformation($"[IndexAllMarkdownFiles] Initial indexing completed - {allMdFiles.Count} files indexed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[IndexAllMarkdownFiles] Error during initial indexing - rolling back");
                _engineDB.Rollback();
                throw;
            }
        }
        
        private void CleanupDatabaseDuplicates()
        {
            try
            {
                _logger.LogInformation("[CleanupDatabase] Starting complete database cleanup - removing ALL records");
                
                _engineDB.BeginTransaction();
                
                // IMPORTANTE: Cancella TUTTO il contenuto delle due tabelle
                // Prima i link (hanno foreign key verso MarkdownFile)
                _logger.LogInformation("[CleanupDatabase] Step 1: Deleting all LinkInsideMarkdown records");
                _engineDB.Delete("from LinkInsideMarkdown");
                _engineDB.Flush();
                
                // Poi i file (dopo che i link sono stati eliminati)
                _logger.LogInformation("[CleanupDatabase] Step 2: Deleting all MarkdownFile records");
                _engineDB.Delete("from MarkdownFile");
                _engineDB.Flush();
                
                _engineDB.Commit();
                
                _logger.LogInformation("[CleanupDatabase] Database cleanup completed - both tables are now empty");
                _logger.LogInformation("[CleanupDatabase] Ready for fresh indexing from filesystem");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CleanupDatabase] Error during database cleanup - rolling back");
                _engineDB.Rollback();
                throw; // Rilancia l'eccezione per fermare il processo
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShallowStructure(string connectionId)
        {
            signalRConnectionId = connectionId;
            
            var list = new List<IFileInfoNode>();
            var currentPath = _fileSystemWatcher.Path;
            
            if (currentPath == AppDomain.CurrentDomain.BaseDirectory)
            {
                return Ok(list);
            }
            
            // PULIZIA E REINDICIZZAZIONE DEL DATABASE
            // Prima pulisce completamente, poi reindicizza tutti i file
            CleanupDatabaseDuplicates();
            IndexAllMarkdownFiles();
            
            // Carica solo primo livello di cartelle che contengono file markdown
            foreach (var itemFolder in Directory.GetDirectories(currentPath).Where(_ => !_.Contains(".md")))
            {
                // Usa FoldersIgnoreService per filtrare le cartelle da nascondere nell'UI
                if (_foldersIgnoreService.ShouldIgnoreFolder(itemFolder))
                {
                    _logger.LogWarning($"[GetShallowStructure] ❌ IGNORING folder: '{itemFolder}'");
                    continue;
                }
                
                // Usa la logica esistente per determinare se la cartella contiene file markdown
                var result = await CreateNodeFolder(itemFolder);
                var node = result.Item1;
                var isEmpty = result.Item2;
                if (!isEmpty)
                {
                    // Imposta le proprietà per il caricamento incrementale
                    node.Level = 0;
                    node.IsIndexed = false;
                    node.IndexingStatus = "idle";
                    
                    // Marca tutti i file children come non indicizzati
                    MarkChildrenAsNotIndexed(node);
                    
                    list.Add(node);
                }
            }
            
            // File nella root
            foreach (var itemFile in Directory.GetFiles(currentPath).Where(_ => Path.GetExtension(_) == ".md"))
            {
                if (_mdIgnoreService.ShouldIgnorePath(itemFile, _fileSystemWatcher.Path))
                {
                    continue;
                }
                
                var relativePath = itemFile.Substring(_fileSystemWatcher.Path.Length);
                var nodeFile = _projectBodyEngine.CreateNodeMdFile(itemFile, relativePath);
                nodeFile.IsIndexed = false;
                nodeFile.IndexingStatus = "idle";
                list.Add(nodeFile);
            }
            
            // Aggiungi nodo empty root
            var nodeempty = new FileInfoNode
            {
                Name = "root",
                FullPath = currentPath,
                Path = currentPath,
                Type = "emptyroot",
                Expandable = false,
                IsIndexed = true
            };
            list.Add(nodeempty);
            
            // Avvia indicizzazione in background senza modificare la struttura visibile
            // Avvia indicizzazione in background
            _ = Task.Run(async () => 
            {
                try 
                {
                    await IndexLinksInBackground(connectionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during background indexing");
                }
            });
            
            return Ok(list);
        }

        /// <summary>
        /// DEPRECATED: Questo metodo non è più utilizzato dai client attuali.
        /// Usare GetShallowStructure invece.
        /// Mantenuto per compatibilità con vecchie versioni.
        /// </summary>
        [HttpGet]
        [Obsolete("Use GetShallowStructure instead. This method is no longer maintained.")]
        public async Task<IActionResult> GetAllMdFiles(string connectionId)
        {
            signalRConnectionId = connectionId;

            await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("parsingProjectStart", "process started");

            var list = new List<IFileInfoNode>();
            var currentPath = _fileSystemWatcher.Path;
            if (currentPath == AppDomain.CurrentDomain.BaseDirectory)
            {
                return Ok(list);
            }

            // IMPORTANTE: Disabilita il FileSystemWatcher per prevenire duplicati durante l'indicizzazione
            bool wasWatcherEnabled = _fileSystemWatcher.EnableRaisingEvents;
            _fileSystemWatcher.EnableRaisingEvents = false;
            _logger.LogInformation($"[GetAllMdFiles] FileSystemWatcher disabled for indexing. Was enabled: {wasWatcherEnabled}");

            try
            {
                _userSettingsDB.BeginTransaction();
                var projectDal = _userSettingsDB.GetDal<Project>();
                if (_fileSystemWatcher.Path != string.Empty)
                {
                    var currentProject = projectDal.GetList().Where(_ => _.Path == _fileSystemWatcher.Path).FirstOrDefault();
                    if (currentProject == null)
                    {
                        var projectName = System.IO.Path.GetFileName(_fileSystemWatcher.Path);
                        currentProject = new Project { Name = projectName, Path = _fileSystemWatcher.Path };
                        projectDal.Save(currentProject);
                    }
                }
                _userSettingsDB.Commit();


                foreach (var itemFolder in Directory.GetDirectories(currentPath).Where(_ => !_.Contains(".md")))
                {
                    // Check if folder should be ignored
                    if (_mdIgnoreService.ShouldIgnorePath(itemFolder, _fileSystemWatcher.Path))
                    {
                        continue;
                    }
                    
                    await _hubContext.Clients.Client(connectionId: signalRConnectionId)
                        .SendAsync("indexingFolder", itemFolder);
                    var result = await CreateNodeFolder(itemFolder);
                    var node = result.Item1;
                    var isempty = result.Item2;
                    if (!isempty)
                    {
                        list.Add(node);
                    }
                }

                foreach (var itemFile in Directory.GetFiles(currentPath).Where(_ => Path.GetExtension(_) == ".md"))
                {
                    var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
                    var node = _projectBodyEngine.CreateNodeMdFile(itemFile, patchedItemFile);
                    list.Add(node);
                }

                _hubContext.Clients.Client(connectionId: connectionId)
                        .SendAsync("indexingFolder", "deleting database").Wait();
                // nettificazione dei folder che non contengono md            
                _engineDB.BeginTransaction();
                _engineDB.Delete("from LinkInsideMarkdown");
                _engineDB.Flush();
                _engineDB.Delete("from MarkdownFile");
                _engineDB.Flush();

                _hubContext.Clients.Client(connectionId: connectionId)
                        .SendAsync("indexingFolder", "creating database").Wait();
                SaveRealationships(list);
                _engineDB.Commit();

                GC.Collect();
                var nodeempty = new FileInfoNode
                {
                    Name = "root",
                    FullPath = currentPath,
                    Path = currentPath,
                    //Level = currentLevel,
                    Type = "emptyroot",
                    Expandable = false
                };

                list.Add(nodeempty);
                await _hubContext.Clients.Client(connectionId: connectionId)
                        .SendAsync("parsingProjectStop", "process completed");
                
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetAllMdFiles] Error during indexing");
                throw;
            }
            finally
            {
                // IMPORTANTE: Riabilita sempre il FileSystemWatcher alla fine
                if (wasWatcherEnabled)
                {
                    _fileSystemWatcher.EnableRaisingEvents = true;
                    _logger.LogInformation("[GetAllMdFiles] FileSystemWatcher re-enabled after indexing");
                }
            }
        }

        [HttpPost]
        public IActionResult CloneTimerMd([FromBody] FileInfoNode fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;

            var allText = System.IO.File.ReadAllText(fileData.FullPath);
            string destFullPath, destRelativePath;
            (allText, destFullPath, destRelativePath) = PrepareClone(fileData, allText);
            System.IO.File.WriteAllText(destFullPath, allText);
            // Devo preparare il nuovo file di risposta
            List<IFileInfoNode> list = PrepareListToGetBack(Path.GetFileName(destFullPath), destFullPath, destRelativePath);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(list);
        }

        private (string, string, string) PrepareClone(FileInfoNode fileData, string allText)
        {
            var destFullPath = fileData.FullPath;
            var destRelativePath = fileData.RelativePath;
            var fileName = Path.GetFileName(fileData.FullPath);

            var stringDatePattern = new Regex(@"([0-9][0-9][0-9][0-9][-][0-9][0-9][-][0-9][0-9]).*");
            var matchesOnFileName = stringDatePattern.Matches(fileName);
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            foreach (Match item in matchesOnFileName)
            {
                destFullPath = destFullPath.Replace(item.Groups[1].Value, currentDate);
                destRelativePath = destRelativePath.Replace(item.Groups[1].Value, currentDate);
                var getTitle = new Regex(@"(#.+?)([0-9][0-9][0-9][0-9][-][0-9][0-9][-][0-9][0-9])(.*)");
                var match = getTitle.Match(allText);
                var firstPart = match.Groups[1].Value;
                var dateToReplace = match.Groups[2].Value;
                var lastPart = match.Groups[3].Value;
                var finalRowString = firstPart + currentDate + lastPart;
                allText = allText.Replace(match.Groups[0].Value, finalRowString);
            }

            return (allText, destFullPath, destRelativePath);
        }

        /// <summary>
        /// It helps to create new Markdown when on client:right click and then: create new markdow
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateNewMd([FromBody] NewFile fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var goodMdRuleFileNameShouldBeSameAsTitle =
                    _goodRules.First(_ => _.GetType() ==
                        typeof(GoodMdRuleFileNameShouldBeSameAsTitle)) as GoodMdRuleFileNameShouldBeSameAsTitle;

            var title = goodMdRuleFileNameShouldBeSameAsTitle.GetTitle(fileData.Title) + ".md";
            var fullPath = fileData.DirectoryPath + Path.DirectorySeparatorChar + title;
            if (fileData.DirectoryLevel == 0 && fileData.DirectoryPath == "root")
            {
                fullPath = _fileSystemWatcher.Path + Path.DirectorySeparatorChar + title;
            }

            // Text Document Management            
            var templateContent = string.Empty;
            var snippetTextDocument = _snippets.Where(_ => _.Id == 0).FirstOrDefault();
            var dictParam = new DictionarySnippetParam();
            dictParam.Add(ParameterName.StringDocumentTitle, fileData.Title);
            dictParam.Add(ParameterName.ProjectPath, _fileSystemWatcher.Path);
            dictParam.Add(ParameterName.DocumentType, fileData.DocumentType);
            templateContent = snippetTextDocument.GetSnippet(dictParam);


            // Additional Template 
            var snippet = _snippets.Where(_ => _.Id == fileData.documentTypeId && _.Id != 0).FirstOrDefault();
            if (snippet != null)
            {
                snippet.SetAssets(fullPath);
                var addtionalTemplateContent = snippet.GetSnippet(dictParam);
                templateContent = string.Concat(templateContent, addtionalTemplateContent);
            }

            // write content
            var relativePath = fullPath.Replace(_fileSystemWatcher.Path, string.Empty);
            System.IO.File.WriteAllText(fullPath, templateContent);


            // prepare data to raise back
            List<IFileInfoNode> list = PrepareListToGetBack(title, fullPath, relativePath);
            _fileSystemWatcher.EnableRaisingEvents = true;
            
            // Invia evento SignalR per il nuovo file creato
            var newFileNode = new
            {
                Name = title,
                FullPath = fullPath,
                Path = relativePath,
                RelativePath = relativePath,
                Type = "mdFile",
                Level = CalculateFileLevel(relativePath),
                Expandable = false,
                IsIndexed = true,
                IndexingStatus = "completed"
            };
            
            _ = Task.Run(async () =>
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync("markdownFileCreated", newFileNode);
                    _logger.LogInformation($"📡 SignalR event 'markdownFileCreated' sent for: {title}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ Error sending SignalR event for: {title}");
                }
            });
            
            return Ok(list);
        }

        private int CalculateFileLevel(string relativePath)
        {
            // Rimuovi il separatore iniziale se presente
            var cleanPath = relativePath.TrimStart(Path.DirectorySeparatorChar);
            
            if (string.IsNullOrEmpty(cleanPath))
            {
                return 0; // File nella root
            }
            
            // Conta il numero di separatori per determinare il livello
            return cleanPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Length - 1;
        }

        private List<IFileInfoNode> PrepareListToGetBack(string title, string fullPath, string relativePath)
        {
            var list = new List<IFileInfoNode>();
            var relativeSplitted = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).SkipLast(1);

            var dynamicRelativePath = string.Empty;
            var currentLevel = 0;
            foreach (var item in relativeSplitted)
            {
                dynamicRelativePath =
                        relativeSplitted.First() == item ? string.Empty : dynamicRelativePath + Path.DirectorySeparatorChar.ToString();
                dynamicRelativePath += item;

                var node = new FileInfoNode
                {
                    Name = item,
                    FullPath = _fileSystemWatcher.Path + Path.DirectorySeparatorChar + dynamicRelativePath,
                    RelativePath = dynamicRelativePath,
                    Path = dynamicRelativePath,
                    Type = "folder",
                    Level = currentLevel,
                };
                currentLevel++;
                list.Add(node);
            }
            var nodeFile = new FileInfoNode
            {
                Name = title,
                FullPath = fullPath,
                Path = relativePath,
                RelativePath = relativePath,
                Type = "mdFile",
                Level = currentLevel,
            };
            list.Add(nodeFile);
            return list;
        }

        [HttpPost]
        public IActionResult RenameDirectory([FromBody] RenameDirectory fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var fullPath = fileData.DirectoryPath + Path.DirectorySeparatorChar + fileData.DirectoryName;

            var relativePath = fullPath.Replace(_fileSystemWatcher.Path, string.Empty);

            var list = new List<IFileInfoNode>();
            var relativeSplitted = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).SkipLast(1);

            var dynamicRelativePath = string.Empty;
            var currentLevel = 0;
            foreach (var item in relativeSplitted)
            {
                dynamicRelativePath =
                        relativeSplitted.First() == item ? string.Empty : dynamicRelativePath + Path.DirectorySeparatorChar.ToString();
                dynamicRelativePath += item;

                var node = new FileInfoNode
                {
                    Name = item,
                    FullPath = _fileSystemWatcher.Path + Path.DirectorySeparatorChar + dynamicRelativePath,
                    RelativePath = dynamicRelativePath,
                    Path = dynamicRelativePath,
                    Type = "folder",
                    Level = currentLevel,
                };
                currentLevel++;
                list.Add(node);
            }
            var nodeFile = new FileInfoNode
            {
                Name = fileData.DirectoryName,
                FullPath = fullPath,
                Path = relativePath,
                RelativePath = relativePath,
                Type = "folder",
                Level = currentLevel,
            };
            list.Add(nodeFile);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(list);
        }

        [HttpPost]
        public IActionResult CreateNewDirectoryEx([FromBody] NewDirectory fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var fullPath = fileData.DirectoryPath + Path.DirectorySeparatorChar + 
                fileData.DirectoryName.Replace(" ","-");
            Directory.CreateDirectory(fullPath);

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(fileData);
        }

        [HttpGet]
        public IActionResult GetBookmarks(string projectId)
        {
            _userSettingsDB.BeginTransaction();
            var bookmarkDal = _userSettingsDB.GetDal<Bookmark>();
            var guidProjectId = new Guid(projectId);
            var bookmarkList = bookmarkDal.GetList()
                .Where(_ => _.Project.Id == guidProjectId).Select(_ =>
                     new { _.Id, _.Name, _.FullPath, ProjectId = _.Project.Id }
                ).ToList();
            _userSettingsDB.Commit();

            return Ok(bookmarkList);
        }

        [HttpPost]
        public IActionResult ToggleBookmark([FromBody] ToggleBookmarkRequest request)
        {
            _userSettingsDB.BeginTransaction();
            var bookmarkDal = _userSettingsDB.GetDal<Bookmark>();
            var bookmarkDb= bookmarkDal.GetList().Where(_ => _.Project.Id == request.ProjectId 
                                                        && _.FullPath == request.FullPath).FirstOrDefault();
            if (bookmarkDb != null)
            {
                bookmarkDal.Delete(bookmarkDb);
            }
            else
            {
                var projectDal = _userSettingsDB.GetDal<Project>();
                var currentProject = projectDal.GetList().Where(_ => _.Id == request.ProjectId).FirstOrDefault();
                // Ok, pay attention, here we are managing TOGGLE
                var bookmark = new Bookmark { FullPath = request.FullPath, Name = request.Name, Project = currentProject };
                currentProject.Bookmarks.Add(bookmark);
                projectDal.Save(currentProject);
                
            }
            _userSettingsDB.Commit();

            return Ok(request);
        }

        [HttpPost]
        public IActionResult CreateNewDirectory([FromBody] NewDirectory fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;

            var fullPath = fileData.DirectoryPath + Path.DirectorySeparatorChar + 
                fileData.DirectoryName.Replace(" ", "-"); ;
            if (fileData.DirectoryLevel == 0 && fileData.DirectoryPath == "root")
            {
                fullPath = _fileSystemWatcher.Path + Path.DirectorySeparatorChar + fileData.DirectoryName;
            }

            Directory.CreateDirectory(fullPath);
            var relativePath = fullPath.Replace(_fileSystemWatcher.Path, string.Empty);

            var list = new List<IFileInfoNode>();
            var relativeSplitted = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).SkipLast(1);

            var dynamicRelativePath = string.Empty;
            var currentLevel = 0;
            foreach (var item in relativeSplitted)
            {
                dynamicRelativePath =
                        relativeSplitted.First() == item ? "\\" : dynamicRelativePath + Path.DirectorySeparatorChar.ToString();
                dynamicRelativePath += item;

                var node = new FileInfoNode
                {
                    Name = item,
                    FullPath = _fileSystemWatcher.Path + dynamicRelativePath,
                    RelativePath =  dynamicRelativePath,
                    Path = dynamicRelativePath,
                    Type = "folder",
                    Level = currentLevel,
                };
                currentLevel++;
                list.Add(node);
            }
            var nodeFile = new FileInfoNode
            {
                Name = fileData.DirectoryName.Replace(" ", "-"),
                FullPath = fullPath,
                Path =  relativePath,
                RelativePath = relativePath,
                Type = "folder",
                Level = currentLevel,
            };
            list.Add(nodeFile);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(list);
        }

        private void SaveRealationships(IList<IFileInfoNode> list, Guid? parentId = null)
        {
            // clean all data
           
            var relDal = _engineDB.GetDal<MarkdownFile>();

            foreach (var item in list)
            {
                var markdownFile = new MarkdownFile
                {
                    FileName = item.Name,
                    Path = item.FullPath,
                    FileType = "File"
                };
                if (item.Childrens.Count > 0)
                {
                    _hubContext.Clients.Client(connectionId: signalRConnectionId)
                    .SendAsync("indexingFolder", markdownFile.FileName).Wait();
                    markdownFile.FileType = "Folder";
                    SaveRealationships(item.Childrens, markdownFile.Id);
                }
                relDal.Save(markdownFile);
                SaveLinksFromMarkdown( markdownFile);
            }

        }

        //private void SaveLinksFromMarkdown(IFileInfoNode item,
        //    MarkdownFile relationship)
        //{
        //    var linkInsideMarkdownDal = _engineDB.GetDal<LinkInsideMarkdown>();
        //    foreach (var getModifier in _getModifiers)
        //    {
        //        var linksToStore = relationship.FileType == "File" ? getModifier.GetLinksFromFile(item.FullPath) : new List<LinkDetail>().ToArray();
        //        foreach (var singleLink in linksToStore)
        //        {
        //            // manage relative path
        //            var fullPath = Path.GetDirectoryName(item.FullPath)
        //                + Path.DirectorySeparatorChar
        //                + singleLink.LinkPath.Replace('/', Path.DirectorySeparatorChar);

        //            // manage absolute path in link
        //            if (singleLink.LinkPath.StartsWith("/"))
        //            {
        //                fullPath = _fileSystemWatcher.Path
        //                    + singleLink.LinkPath.Replace('/', Path.DirectorySeparatorChar);
        //            }

        //            var normalizedFullPath = _helper.NormalizePath(fullPath);
                    
        //            var context = Path.GetDirectoryName(relationship.Path)
        //                .Replace(_fileSystemWatcher.Path, string.Empty)
        //                .Replace(Path.DirectorySeparatorChar, '/');
        //            var linkToStore = new LinkInsideMarkdown
        //            {
        //                FullPath = normalizedFullPath,
        //                Path = singleLink.LinkPath,
        //                MdTitle = singleLink.MdTitle,
        //                HTMLTitle = singleLink.HTMLTitle,
        //                Source = getModifier.GetType().Name,
        //                LinkedCommand = singleLink.LinkedCommand,
        //                SectionIndex = singleLink.SectionIndex,
        //                MarkdownFile = relationship,
        //                MdContext = context,
        //            };
        //            linkInsideMarkdownDal.Save(linkToStore);
        //        }
        //    }

        //}

        //private FileInfoNode CreateNodeMdFile(string itemFile)
        //{
        //    var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
        //    var node = new FileInfoNode
        //    {
        //        Name = Path.GetFileName(itemFile),
        //        FullPath = itemFile,
        //        Path = patchedItemFile,
        //        RelativePath = patchedItemFile,
        //        Type = "mdFile"
        //    };
        //    return node;
        //}

        private async Task<(FileInfoNode, bool)> CreateNodeFolder(string itemFolder)
        {

            var patchedItemFolfer = itemFolder.Substring(_fileSystemWatcher.Path.Length);
            var node = new FileInfoNode
            {
                Name = Path.GetFileName(itemFolder),
                FullPath = itemFolder,
                RelativePath =
                    patchedItemFolfer,
                Path = patchedItemFolfer,
                Type = "folder",
                DevelopmentTags = LoadDevelopmentTags(itemFolder)
            };
            var isEmpty = await ExploreNodes(node, itemFolder);
            return (node, isEmpty);
        }

        private FileInfoNode CreateNodeFolderOnly(string itemFolder)
        {
            var patchedItemFolfer = itemFolder;
            var node = new FileInfoNode
            {
                Name = Path.GetFileName(itemFolder),
                FullPath = itemFolder,
                Path = patchedItemFolfer,
                Type = "folder",
                DevelopmentTags = LoadDevelopmentTags(itemFolder)
            };
            ExploreNodesFolderOnly(node, itemFolder);
            return node;
        }

        /// <summary>
        /// Loads development tags from .development.yml file in the specified folder
        /// </summary>
        /// <param name="folderPath">Path to the folder to check for .development.yml</param>
        /// <returns>List of development tags, empty if file doesn't exist or parsing fails</returns>
        private List<string> LoadDevelopmentTags(string folderPath)
        {
            try
            {
                // Find project root by looking for .development.yml file going up the directory tree
                var projectRoot = FindProjectRoot(folderPath);
                if (string.IsNullOrEmpty(projectRoot))
                {
                    return new List<string>();
                }

                var devConfigPath = Path.Combine(projectRoot, ".development.yml");
                if (!System.IO.File.Exists(devConfigPath))
                {
                    return new List<string>();
                }

                var yamlContent = System.IO.File.ReadAllText(devConfigPath);

                // Use YamlDotNet for parsing
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var config = deserializer.Deserialize<DevelopmentConfig>(yamlContent);
                if (config == null || config.Folders == null)
                {
                    return new List<string>();
                }

                // Calculate relative path and find matching folder
                var relativePath = folderPath.Replace(projectRoot, "").TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var folderEntry = config.Folders.FirstOrDefault(f => f.Path == relativePath);

                return folderEntry?.Tags ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoadDevelopmentTags] ERROR: {ex.Message}");
                Console.WriteLine($"[LoadDevelopmentTags] Stack trace: {ex.StackTrace}");
                _logger.LogWarning(ex, $"Error reading development tags for {folderPath}");
                return new List<string>();
            }
        }

        private string FindProjectRoot(string startPath)
        {
            try
            {
                var currentDir = new DirectoryInfo(startPath);
                while (currentDir != null)
                {
                    // Look for .development.yml in current directory
                    var configPath = Path.Combine(currentDir.FullName, ".development.yml");
                    if (System.IO.File.Exists(configPath))
                    {
                        return currentDir.FullName;
                    }

                    // Also look for other project markers like .git, .mdexplorer, etc.
                    if (Directory.Exists(Path.Combine(currentDir.FullName, ".git")) ||
                        System.IO.File.Exists(Path.Combine(currentDir.FullName, ".mdexplorer")))
                    {
                        return currentDir.FullName;
                    }

                    currentDir = currentDir.Parent;
                }
                // If no project root found, return the start path
                return startPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FindProjectRoot] ERROR: {ex.Message}");
                _logger.LogWarning(ex, $"Error finding project root for {startPath}");
                return startPath;
            }
        }

        private void ExploreNodesFolderOnly(FileInfoNode fileInfoNode, string pathFile)
        {
            try
            {
                var accessControlList = new DirectoryInfo(pathFile).GetAccessControl();
                if (accessControlList.AreAccessRulesProtected)
                    return;

                if (!Directory.Exists(pathFile)) // jump directories where access is denied
                {
                    return;
                }
                foreach (var itemFolder in Directory.GetDirectories(pathFile))
                {
                    FileInfoNode node = CreateNodeFolderOnly(itemFolder);
                    fileInfoNode.Childrens.Add(node);
                }
            }
            catch (Exception ex)
            {


            }

        }

        private async Task<bool> ExploreNodes(FileInfoNode fileInfoNode, string pathFile)
        {
            var isEmpty = true;

            foreach (var itemFolder in Directory.GetDirectories(pathFile).Where(_ => !_.Contains(".md")))
            {
                // Check if folder should be ignored using FoldersIgnoreService
                if (_foldersIgnoreService.ShouldIgnoreFolder(itemFolder))
                {
                    _logger.LogWarning($"[ExploreNodes] ❌ IGNORING subfolder: '{itemFolder}'");
                    continue;
                }
                
                // Send folderIndexingStart event for subfolder
                await _hubContext.Clients.Client(signalRConnectionId)
                    .SendAsync("folderIndexingStart", new { path = itemFolder, status = "indexing" });
                
                var result = await CreateNodeFolder(itemFolder);
                var node = result.Item1;
                var isempty = result.Item2;
                if (!isempty)
                {
                    fileInfoNode.Childrens.Add(node);
                    
                    // Send folderIndexingComplete event for subfolder
                    await _hubContext.Clients.Client(signalRConnectionId)
                        .SendAsync("folderIndexingComplete", new { path = itemFolder, status = "completed" });
                        
                    // Small delay to make progress visible
                    await Task.Delay(50);
                }
                isEmpty = isEmpty && isempty;
            }

            foreach (var itemFile in Directory.GetFiles(pathFile).Where(_ => Path.GetExtension(_) == ".md"))
            {
                // Check if file should be ignored
                if (_mdIgnoreService.ShouldIgnorePath(itemFile, _fileSystemWatcher.Path))
                {
                    continue;
                }
                
                var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
                var node = _projectBodyEngine.CreateNodeMdFile(itemFile, patchedItemFile);
                fileInfoNode.Childrens.Add(node);
                isEmpty = false;
            }
            return isEmpty;
        }

        private async Task IndexLinksInBackground(string connectionId)
        {
            // Per questa implementazione, saltiamo la gestione del database
            // e ci concentriamo solo sulle notifiche SignalR per il feedback visivo
            
            // Carica la struttura completa per l'indicizzazione
            var fullStructure = new List<IFileInfoNode>();
            var currentPath = _fileSystemWatcher.Path;
            
            foreach (var itemFolder in Directory.GetDirectories(currentPath).Where(_ => !_.Contains(".md")))
            {
                if (_mdIgnoreService.ShouldIgnorePath(itemFolder, _fileSystemWatcher.Path))
                    continue;
                
                // Notifica inizio indicizzazione cartella
                await _hubContext.Clients.Client(connectionId)
                    .SendAsync("folderIndexingStart", new { path = itemFolder, status = "indexing" });
                    
                var result = await CreateNodeFolder(itemFolder);
                var folderNode = result.Item1;
                var isEmpty = result.Item2;
                if (!isEmpty)
                {
                    fullStructure.Add(folderNode);
                    
                    // Notifica fine indicizzazione cartella
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("folderIndexingComplete", new { path = itemFolder, status = "completed" });
                }
            }
            
            // File nella root
            foreach (var itemFile in Directory.GetFiles(currentPath).Where(_ => Path.GetExtension(_) == ".md"))
            {
                if (_mdIgnoreService.ShouldIgnorePath(itemFile, _fileSystemWatcher.Path))
                    continue;
                    
                var nodeFile = _projectBodyEngine.CreateNodeMdFile(itemFile, currentPath);
                fullStructure.Add(nodeFile);
            }
            
            // TODO: Implementare salvataggio relazioni nel database con sessione dedicata
            
            // Notifica che i file sono stati indicizzati
            await NotifyFilesIndexed(fullStructure, connectionId);
        }

        private async Task NotifyFilesIndexed(List<IFileInfoNode> structure, string connectionId)
        {
            // Notifica ricorsivamente tutti i file markdown come indicizzati
            foreach (var node in structure)
            {
                if (node.Type == "mdFile" || node.Type == "mdFileTimer")
                {
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("fileIndexed", new { 
                            path = node.FullPath, 
                            isIndexed = true 
                        });
                }
                
                if (node.Childrens != null && node.Childrens.Count > 0)
                {
                    await NotifyFilesIndexed(node.Childrens.ToList(), connectionId);
                }
            }
        }

        private void MarkChildrenAsNotIndexed(IFileInfoNode node)
        {
            if (node.Type == "mdFile" || node.Type == "mdFileTimer")
            {
                node.IsIndexed = false;
                node.IndexingStatus = "idle";
            }
            
            if (node.Childrens != null)
            {
                foreach (var child in node.Childrens)
                {
                    MarkChildrenAsNotIndexed(child);
                }
            }
        }
    }
}


public class NewFile
{
    public string Title { get; set; }
    public string DirectoryPath { get; set; }
    public int DirectoryLevel { get; set; }
    public int documentTypeId { get; set; }
    public string DocumentType { get; set; }
}

public class NewDirectory
{
    public string DirectoryName { get; set; }
    public string DirectoryPath { get; set; }
    public int DirectoryLevel { get; set; }
}

public class RenameDirectory
{
    public string DirectoryName { get; set; }
    public string DirectoryPath { get; set; }
    public int DirectoryLevel { get; set; }
}