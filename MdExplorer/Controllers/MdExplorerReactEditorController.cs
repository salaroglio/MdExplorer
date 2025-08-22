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
    [Route("/api/MdExplorerEditorReact")] // Rimosso {*url} da qui
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
        [HttpGet("{*url}")] // Aggiunto {*url} specificamente a HttpGet
        public async Task<IActionResult> GetAsync(string url)
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

            // Handle non-markdown files (but treat .md.directory as markdown)
            if (!string.IsNullOrEmpty(fileExtension) && 
                fileExtension.ToLowerInvariant() != ".md" && 
                !requestedPathFromClient.EndsWith(".md.directory", StringComparison.OrdinalIgnoreCase))
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
                // Su Linux, Environment.NewLine è "\n", quindi meglio usare un array distinto
                string[] newLines = { "\r\n", "\n" };
                
                _logger.LogDebug($"[YAML Debug] File: {filePathToAccessOnServer}");
                _logger.LogDebug($"[YAML Debug] Environment.NewLine: '{Environment.NewLine}' (length: {Environment.NewLine.Length})");
                _logger.LogDebug($"[YAML Debug] First 50 chars: '{(markdownTxt.Length > 50 ? markdownTxt.Substring(0, 50).Replace("\r", "\\r").Replace("\n", "\\n") : markdownTxt.Replace("\r", "\\r").Replace("\n", "\\n"))}'");
                _logger.LogDebug($"[YAML Debug] Starts with '---\\r\\n': {markdownTxt.StartsWith(yamlSeparator + "\r\n")}");
                _logger.LogDebug($"[YAML Debug] Starts with '---\\n': {markdownTxt.StartsWith(yamlSeparator + "\n")}");

                bool startsWithYaml = false;
                string detectedNewLine = null;
                foreach (var nl in newLines)
                {
                    if (markdownTxt.StartsWith(yamlSeparator + nl))
                    {
                        startsWithYaml = true;
                        detectedNewLine = nl;
                        break;
                    }
                }

                if (startsWithYaml)
                {
                    _logger.LogDebug($"[YAML Debug] YAML separator detected at start of file with newline: '{detectedNewLine.Replace("\r", "\\r").Replace("\n", "\\n")}'");
                    
                    // Cerca il secondo separatore YAML
                    int searchStartIndexForSecondSeparator = yamlSeparator.Length + detectedNewLine.Length;
                    int secondSeparatorActualStartIndex = -1;
                    string secondNewLine = null;
                    
                    // Cerchiamo "---" seguito da newline
                    foreach (var nl in newLines)
                    {
                        int idx = markdownTxt.IndexOf(yamlSeparator + nl, searchStartIndexForSecondSeparator);
                        if (idx > 0)
                        {
                            secondSeparatorActualStartIndex = idx;
                            secondNewLine = nl;
                            _logger.LogDebug($"[YAML Debug] Found second separator at position {idx} with newline: '{nl.Replace("\r", "\\r").Replace("\n", "\\n")}'");
                            break;
                        }
                    }

                    if (secondSeparatorActualStartIndex > 0)
                    {
                        // Trovato un blocco YAML valido
                        // Il contenuto inizia dopo il secondo separatore e il suo newline
                        int contentActualStartIndex = secondSeparatorActualStartIndex + yamlSeparator.Length + secondNewLine.Length;
                        
                        contentWithoutYaml = markdownTxt.Substring(contentActualStartIndex);
                        _logger.LogDebug($"[YAML Debug] YAML block found from 0 to {contentActualStartIndex}");
                        _logger.LogDebug($"[YAML Debug] Content without YAML starts at position {contentActualStartIndex}");
                        _logger.LogDebug($"[YAML Debug] First 50 chars of content: '{(contentWithoutYaml.Length > 50 ? contentWithoutYaml.Substring(0, 50).Replace("\r", "\\r").Replace("\n", "\\n") : contentWithoutYaml.Replace("\r", "\\r").Replace("\n", "\\n"))}'");
                        _logger.LogInformation($"YAML front matter removed from file: {filePathToAccessOnServer}");
                    }
                    else
                    {
                        _logger.LogInformation($"YAML front matter started but not properly closed in file: {filePathToAccessOnServer}. Serving full content.");
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

        public class UpdateMarkdownRequest
        {
            public string FilePath { get; set; }
            public string MarkdownContent { get; set; }
        }

        [HttpPost("UpdateMarkdown")]
        public async Task<IActionResult> UpdateMarkdownAsync([FromBody] UpdateMarkdownRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.FilePath) || request.MarkdownContent == null)
            {
                _logger.LogWarning("UpdateMarkdownAsync: Richiesta non valida o campi mancanti.");
                return BadRequest("Richiesta non valida o campi mancanti.");
            }

            var filePathToAccessOnServer = request.FilePath;

            if (!System.IO.File.Exists(filePathToAccessOnServer))
            {
                // Prova ad aggiungere .md se non esiste e non ha estensione
                if (string.IsNullOrEmpty(Path.GetExtension(filePathToAccessOnServer)))
                {
                    filePathToAccessOnServer = request.FilePath + ".md";
                    if (!System.IO.File.Exists(filePathToAccessOnServer))
                    {
                        _logger.LogWarning($"UpdateMarkdownAsync: File non trovato: {request.FilePath} o {filePathToAccessOnServer}");
                        return NotFound($"File non trovato: {request.FilePath}");
                    }
                }
                else
                {
                    _logger.LogWarning($"UpdateMarkdownAsync: File non trovato: {filePathToAccessOnServer}");
                    return NotFound($"File non trovato: {filePathToAccessOnServer}");
                }
            }

            try
            {
                var originalFullContent = await System.IO.File.ReadAllTextAsync(filePathToAccessOnServer, Encoding.UTF8);
                string originalYamlBlock = "";
                const string yamlSeparator = "---";
                string[] newLines = { "\r\n", "\n" }; // Windows e Unix/Linux newlines

                // Verifica se il contenuto inizia con il separatore YAML
                bool startsWithYaml = false;
                string firstNewLine = "";
                foreach (var nl in newLines)
                {
                    if (originalFullContent.StartsWith(yamlSeparator + nl))
                    {
                        startsWithYaml = true;
                        firstNewLine = nl;
                        _logger.LogDebug($"[UpdateMarkdown YAML Debug] File starts with YAML, newline: '{nl.Replace("\r", "\\r").Replace("\n", "\\n")}'");
                        break;
                    }
                }

                if (startsWithYaml)
                {
                    // Trovato l'inizio del blocco YAML. Cerchiamo la fine.
                    // L'indice di partenza per la ricerca del secondo separatore è dopo il primo separatore e il suo newline.
                    int searchStartIndexForSecondSeparator = yamlSeparator.Length + firstNewLine.Length;

                    int secondSeparatorActualStartIndex = -1;
                    string secondNewLine = "";

                    foreach (var nl in newLines)
                    {
                        // Cerchiamo "---" seguito da un newline
                        secondSeparatorActualStartIndex = originalFullContent.IndexOf(yamlSeparator + nl, searchStartIndexForSecondSeparator);
                        if (secondSeparatorActualStartIndex != -1) // Trovato
                        {
                            secondNewLine = nl;
                            break;
                        }
                    }

                    if (secondSeparatorActualStartIndex != -1)
                    {
                        // Trovato il secondo separatore. Il blocco YAML va dall'inizio fino alla fine del secondo separatore + newline.
                        int endOfYamlBlockIndex = secondSeparatorActualStartIndex + yamlSeparator.Length + secondNewLine.Length;
                        originalYamlBlock = originalFullContent.Substring(0, endOfYamlBlockIndex);
                        _logger.LogInformation($"UpdateMarkdownAsync: Blocco YAML originale estratto per {filePathToAccessOnServer}");
                    }
                    else
                    {
                        // Iniziato con YAML ma non trovato un secondo separatore valido.
                        _logger.LogInformation($"UpdateMarkdownAsync: YAML front matter iniziato ma non chiuso correttamente in {filePathToAccessOnServer}. Nessun blocco YAML preservato.");
                        // In questo caso, originalYamlBlock rimane vuoto, quindi l'intero contenuto originale non viene considerato YAML.
                    }
                }
                else
                {
                    _logger.LogInformation($"UpdateMarkdownAsync: Nessun YAML front matter trovato in {filePathToAccessOnServer}.");
                }

                // Combina l'YAML originale (se presente) con il nuovo contenuto Markdown.
                // Assicurati che non ci siano doppi newline se originalYamlBlock è vuoto e MarkdownContent inizia con newline,
                // o se originalYamlBlock finisce con newline e MarkdownContent inizia con newline.
                // Se originalYamlBlock è vuoto, usiamo direttamente request.MarkdownContent.
                // Se originalYamlBlock non è vuoto, termina con un newline. request.MarkdownContent dovrebbe iniziare senza newline.
                string finalContentToWrite;

                // Normalizza il contenuto Markdown ricevuto dal client
                string contentFromClient = request.MarkdownContent;
                // Primo: normalizza tutti i tipi di newline a \n
                string normalizedToLfContent = contentFromClient.Replace("\r\n", "\n");
                // Secondo: converti \n nel newline specifico della piattaforma
                string platformSpecificContent = normalizedToLfContent.Replace("\n", Environment.NewLine);

                if (!string.IsNullOrEmpty(originalYamlBlock))
                {
                    // originalYamlBlock dovrebbe già finire con il newline corretto (estratto dal file originale)
                    // platformSpecificContent ora ha i newlines corretti per la piattaforma corrente.
                    // TrimStart è per evitare doppi newlines se platformSpecificContent iniziasse con uno
                    // e originalYamlBlock finisse già con uno.
                    finalContentToWrite = originalYamlBlock + platformSpecificContent.TrimStart('\r', '\n');
                }
                else
                {
                    finalContentToWrite = platformSpecificContent;
                }

                // Disabilita temporaneamente il FileSystemWatcher per evitare doppi eventi
                _fileSystemWatcher.EnableRaisingEvents = false;
                try
                {
                    await System.IO.File.WriteAllTextAsync(filePathToAccessOnServer, finalContentToWrite, Encoding.UTF8);
                    _logger.LogInformation($"UpdateMarkdownAsync: File aggiornato con successo: {filePathToAccessOnServer}");
                    return NoContent(); // O Ok() se si preferisce
                }
                finally
                {
                    // Riabilita sempre il FileSystemWatcher
                    _fileSystemWatcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateMarkdownAsync: Errore durante l'aggiornamento del file: {filePathToAccessOnServer}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Errore durante l'aggiornamento del file.");
            }
        }
    }
}
