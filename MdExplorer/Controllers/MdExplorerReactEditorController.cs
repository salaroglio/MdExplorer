using Markdig;
using Markdig.Extensions.JiraLinks;
using MdExplorer.Abstractions.Models;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Features.Commands;
using MdExplorer.Service.Controllers;
using MdExplorer.Abstractions.DB;
using System.Web;
using System.Net.Http;
using System.Text.RegularExpressions;
using MdExplorer.Features.Refactoring.Analysis.Interfaces;
using MdExplorer.Features.Refactoring.Analysis;
using System.Globalization;
using System.Net.Http.Headers;
using MdExplorer.Features.Utilities;
using MdExplorer.Features.Yaml.Models;
using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.AspNetCore.Http;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using DocumentFormat.OpenXml.Wordprocessing;
using MdExplorer.Abstractions.Entities.EngineDB;
using System.Collections.Generic;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("/api/MdExplorerEditorReact/{*url}")]
    public class MdExplorerReactEditorController : MdControllerBase<MdExplorerReactEditorController>//ControllerBase
    {
        private readonly IGoodMdRule<FileInfoNode>[] _goodRules;        
        // private readonly IYamlParser<MdExplorerDocumentDescriptor> _yamlDocumentDescriptor; // Field removed

        public MdExplorerReactEditorController(ILogger<MdExplorerReactEditorController> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IUserSettingsDB session,
            ICommandRunnerHtml commandRunner,
            IGoodMdRule<FileInfoNode>[] GoodRules,
            IHelper helper,
            IWorkLink[] modifiers            
            ) : base(logger, fileSystemWatcher, options, null, session, null, commandRunner, modifiers, helper) // hubContext and engineDB passed as null to base
        {
            _goodRules = GoodRules;
            // _yamlDocumentDescriptor assignment removed
        }

        /// <summary>
        /// Get all goodies available in html     
        /// It's good to get images for example
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync(string url) // Added url parameter from route
        {
            // The 'url' parameter is automatically bound from the {*url} part of the route.
            // ASP.NET Core handles URL decoding for route parameters.
            var requestedPathFromClient = url; 
            
            if (string.IsNullOrEmpty(requestedPathFromClient))
            {
                _logger.LogWarning("Requested path from client is null or empty.");
                return BadRequest("Requested path cannot be empty.");
            }

            // Sanitize or validate the path if necessary, though GetRelativePathFileSystem might already do some of this.
            // For now, assume requestedPathFromClient is the intended absolute file path.

            var fileExtension = Path.GetExtension(requestedPathFromClient);
            string filePathToAccessOnServer = requestedPathFromClient;

            if (!System.IO.File.Exists(filePathToAccessOnServer))
            {
                // If the direct path doesn't exist, and it had no extension, try adding .md
                if (string.IsNullOrEmpty(fileExtension))
                {
                    filePathToAccessOnServer = requestedPathFromClient + ".md";
                    if (!System.IO.File.Exists(filePathToAccessOnServer))
                    {
                        _logger.LogWarning($"File not found: {requestedPathFromClient} or {filePathToAccessOnServer}");
                        return NotFound($"File not found: {requestedPathFromClient}");
                    }
                    fileExtension = ".md"; // Update extension if we found it with .md
                }
                else
                {
                    _logger.LogWarning($"File not found: {filePathToAccessOnServer}");
                    return NotFound($"File not found: {filePathToAccessOnServer}");
                }
            }
            
            // Handle non-markdown files
            if (!string.IsNullOrEmpty(fileExtension) && fileExtension.ToLowerInvariant() != ".md")
            {
                try
                {
                    var fileBytes = await System.IO.File.ReadAllBytesAsync(filePathToAccessOnServer);
                    var contentType = GetContentType(filePathToAccessOnServer);
                    _logger.LogInformation($"Serving non-markdown file: {filePathToAccessOnServer} with content type: {contentType}");
                    return File(fileBytes, contentType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error reading non-markdown file: {filePathToAccessOnServer}");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error reading file.");
                }
            }

            // Handle markdown files
            try
            {
                var markdownTxt = await System.IO.File.ReadAllTextAsync(filePathToAccessOnServer, Encoding.UTF8);

                string contentWithoutYaml = markdownTxt;
                const string yamlSeparator = "---";
                string[] newLines = { Environment.NewLine, "\n" };

                if (markdownTxt.StartsWith(yamlSeparator + newLines[0]) || markdownTxt.StartsWith(yamlSeparator + newLines[1]))
                {
                    var firstSeparatorEndIndex = markdownTxt.IndexOf(yamlSeparator, yamlSeparator.Length); // Trova la fine del primo "---"
                    if (firstSeparatorEndIndex > 0) // Assicura che ci sia qualcosa dopo il primo "---"
                    {
                        // Cerca l'inizio del secondo separatore "---" seguito da newline
                        // partendo da dopo il primo blocco separatore completo (incluso il newline dopo il primo "---")
                        int searchStartIndexForSecondSeparator = -1;
                        if (markdownTxt.Substring(yamlSeparator.Length).StartsWith(newLines[0])) {
                            searchStartIndexForSecondSeparator = yamlSeparator.Length + newLines[0].Length;
                        } else if (markdownTxt.Substring(yamlSeparator.Length).StartsWith(newLines[1])) {
                            searchStartIndexForSecondSeparator = yamlSeparator.Length + newLines[1].Length;
                        }
                        
                        if(searchStartIndexForSecondSeparator > 0)
                        {
                            var secondSeparatorActualStartIndex = -1;
                            // Cerchiamo "---" seguito da newline
                            foreach (var nl in newLines)
                            {
                                secondSeparatorActualStartIndex = markdownTxt.IndexOf(yamlSeparator + nl, searchStartIndexForSecondSeparator);
                                if (secondSeparatorActualStartIndex > 0) break;
                            }

                            if (secondSeparatorActualStartIndex > 0)
                            {
                                // Trovato un blocco YAML valido
                                // Il contenuto inizia dopo il secondo separatore e il suo newline
                                int contentActualStartIndex = secondSeparatorActualStartIndex + yamlSeparator.Length;
                                // Aggiungiamo la lunghezza del newline specifico trovato
                                if (markdownTxt.Substring(contentActualStartIndex).StartsWith(newLines[0])) {
                                    contentActualStartIndex += newLines[0].Length;
                                } else if (markdownTxt.Substring(contentActualStartIndex).StartsWith(newLines[1])) {
                                     contentActualStartIndex += newLines[1].Length;
                                }

                                contentWithoutYaml = markdownTxt.Substring(contentActualStartIndex);
                                // Non è necessario TrimStart perché partiamo esattamente dopo il newline
                                _logger.LogInformation($"YAML front matter removed from file: {filePathToAccessOnServer}");
                            }
                            else
                            {
                                _logger.LogInformation($"YAML front matter started but not properly closed in file: {filePathToAccessOnServer}. Serving full content.");
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"File starts with '---' but no content or closing YAML separator found. Serving full content for: {filePathToAccessOnServer}");
                        }
                    }
                    else // Questo caso è improbabile se StartsWith ha avuto successo, ma per sicurezza
                    {
                         _logger.LogInformation($"File starts with '---' but structure is unusual. Serving full content for: {filePathToAccessOnServer}");
                    }
                }


                // Relative path for ProcessMarkdownToMarkdownAsync might need adjustment
                // For now, we pass the full path as relativePathFile as ProcessMarkdownToMarkdownAsync uses it for RequestInfo
                // This might need further refinement if ProcessMarkdownToMarkdownAsync strictly expects a path relative to _fileSystemWatcher.Path
                string relativePathForProcessing = filePathToAccessOnServer;
                if (filePathToAccessOnServer.StartsWith(_fileSystemWatcher.Path, StringComparison.OrdinalIgnoreCase))
                {
                    relativePathForProcessing = filePathToAccessOnServer.Substring(_fileSystemWatcher.Path.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                }


                var monitoredMd = new MonitoredMDModel
                {
                    Path = filePathToAccessOnServer, // Corrected to use the actual path being accessed
                    Name = Path.GetFileName(filePathToAccessOnServer),
                    RelativePath = relativePathForProcessing, // Use the calculated relative path
                    FullPath = filePathToAccessOnServer,
                    FullDirectoryPath = Path.GetDirectoryName(filePathToAccessOnServer)
                };

                string processedMarkdownText = await ProcessMarkdownToMarkdownAsync(
                    contentWithoutYaml, // Usa il contenuto senza YAML
                    relativePathForProcessing, // Pass the potentially adjusted relative path
                    filePathToAccessOnServer,  // Pass the absolute path
                    monitoredMd);
                
                _logger.LogInformation($"Serving markdown file: {filePathToAccessOnServer}");
                return Content(processedMarkdownText, "text/plain; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing markdown file: {filePathToAccessOnServer}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error processing markdown file.");
            }
        }

        // private async Task<XmlDocument> ProcessAsSlideTypeDocument(...) // Method removed as per plan
        // ManageIfThePathContainsExtensionMdOrNot is no longer needed as path logic is handled in GetAsync
        // CreateAResponseForNotMdFile is replaced by direct File() result and GetContentType helper

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (types.ContainsKey(ext))
            {
                return types[ext];
            }
            return "application/octet-stream"; // Default MIME type
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            // Common MIME types, can be expanded
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".svg", "image/svg+xml"}
            };
        }

        private async Task<string> ProcessMarkdownToMarkdownAsync(
                string readText,
                string relativePathFileSystem,
                string fullPathFile,
                //string connectionId, // Removed
                MonitoredMDModel monitoredMd) // Kept for now, usage reduced
        {
            var requestInfo = new RequestInfo()
            {
                CurrentQueryRequest = relativePathFileSystem,
                CurrentRoot = _fileSystemWatcher.Path,
                AbsolutePathFile = fullPathFile,
                RootQueryRequest = relativePathFileSystem,
                //ConnectionId = connectionId, // Removed
            };
            //var isPlantuml = false; // Not needed for SignalR
            //if (readText.Contains("```plantuml")) // PlantUML handling should be within TransformInNewMDFromMD
            //{
            //    isPlantuml = true;
            //    // await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("plantumlWorkStart", monitoredMd); // Removed
            //}

            // This is the core Markdown-to-Markdown transformation.
            // It's assumed that _commandRunner.TransformInNewMDFromMD handles PlantUML blocks
            // by preserving them as code blocks, not converting them to images/links.
            //readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo); // Decommentare se necessario

            var goodMdRuleFileNameShouldBeSameAsTitle =
                    _goodRules.First(_ => _.GetType() ==
                        typeof(GoodMdRuleFileNameShouldBeSameAsTitle));

            var fileNode = new FileInfoNode
            {
                FullPath = fullPathFile,
                Name = Path.GetFileName(fullPathFile),
                DataText = readText
            };
            
            (var isBroken, var theNameShouldBe) = goodMdRuleFileNameShouldBeSameAsTitle.ItBreakTheRule(fileNode);
            if (isBroken)
            {
                // SignalR notification removed. Log or handle internally if needed.
                // monitoredMd.Message = "It breaks Rule # 1";
                // monitoredMd.Action = "Rename the File!";
                // monitoredMd.FromFileName = Path.GetFileName(fullPathFile);
                // monitoredMd.ToFileName = theNameShouldBe;
                // monitoredMd.FullPath = Path.GetDirectoryName(fullPathFile);
                // await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("markdownbreakrule1", monitoredMd); // Removed
                _logger.LogWarning($"File '{fullPathFile}' breaks rule 'FileNameShouldBeSameAsTitle'. Suggested name: '{theNameShouldBe}'");
            }

            // HTML Pipeline, HTML Conversion, Post-HTML Conversion, and UI Element creation removed.
            // Directory.SetCurrentDirectory, cache logic, and HTML specific transformations removed.
            
            //if (isPlantuml)
            //{                 
            //    // await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("plantumlWorkStop", monitoredMd); // Removed
            //}
            return readText; // Return the processed Markdown text
        }

        // private static void CreateHTMLBody(...) // Method removed
        // private string AddButtonOnLowerBar(...) // Method removed
        // private string AddButtonTextOnVerticalBar(...) // Method removed
    }
}
