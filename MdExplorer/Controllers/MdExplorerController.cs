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
using MdExplorer.Features.Slides;
using System;
using System.Net;
using System.Collections.Generic;
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
using Microsoft.Extensions.DependencyInjection;
using MdExplorer.Services.DatabaseManager;
using MdExplorer.Features.Services.SourceMapping;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("/api/MdExplorer/{*url}")]
    public class MdExplorerController : MdControllerBase<MdExplorerController>//ControllerBase
    {
        private readonly IGoodMdRule<FileInfoNode>[] _goodRules;
        private readonly IYamlParser<MdExplorerDocumentDescriptor> _yamlDocumentDescriptor;
        private readonly IYamlDefaultGenerator _yamlDefaultGenerator;
        private readonly MarkdownSourceMapService _sourceMapService;
        private readonly MdExplorer.Services.AgentRun.IAgentWorktreeManager _worktree;

        public MdExplorerController(ILogger<MdExplorerController> logger,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB session,
            IEngineDB engineDB,
            ICommandRunnerHtml commandRunner,
            IGoodMdRule<FileInfoNode>[] GoodRules,
            IHelper helper,
            IYamlParser<MdExplorerDocumentDescriptor> yamlDocumentDescriptor,
            IYamlDefaultGenerator yamlDefaultGenerator,
            IWorkLink[] modifiers,
            MarkdownSourceMapService sourceMapService,
            MdExplorer.Services.AgentRun.IAgentWorktreeManager worktree,
            IDatabaseManager databaseManager = null
            ) : base(logger, options, hubContext, session, engineDB, commandRunner,modifiers, helper, databaseManager)
        {
            _goodRules = GoodRules;

            _yamlDocumentDescriptor = yamlDocumentDescriptor;
            _yamlDefaultGenerator = yamlDefaultGenerator;
            _sourceMapService = sourceMapService;
            _worktree = worktree;
        }

        /// <summary>
        /// Get all goodies available in html     
        /// It's good to get images for example
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var currentCultureInfo = CultureInfo.CurrentCulture;
            var test = Encoding.Default;
            
            var rootPathSystem = $"{GetProjectPath()}{Path.DirectorySeparatorChar}";
            var relativePathFile = GetRelativePathFileSystem("mdexplorer");
            var relativePathExtension = Path.GetExtension(relativePathFile);

            // Log di debug per rinominazioni
            _logger.LogInformation($"🔍 [MdExplorer] Request processing:");
            _logger.LogInformation($"🔍 [MdExplorer] rootPathSystem: {rootPathSystem}");
            _logger.LogInformation($"🔍 [MdExplorer] relativePathFile: {relativePathFile}");
            _logger.LogInformation($"🔍 [MdExplorer] relativePathExtension: {relativePathExtension}");

            // Validazione: se il path relativo è vuoto o contiene solo slash/backslash, ritorna errore
            if (string.IsNullOrWhiteSpace(relativePathFile) || 
                relativePathFile.Trim('/', '\\').Length == 0)
            {
                _logger.LogWarning($"❌ [MdExplorer] Invalid or empty relative path: '{relativePathFile}'");
                return BadRequest("Invalid file path");
            }

            // Leggere connectionId e source PRIMA per tutti i tipi di file
            var connectionId = Request.Query["ConnectionId"];
            var source = Request.Query["source"]; // "angular", "detached", or null
            var theme = Request.Query["theme"].FirstOrDefault() ?? "light";
            bool isIframeLinkClick = string.IsNullOrEmpty(source);
            // Detached standalone window: render only, no SignalR side effects so the
            // main window's navigation/state is not disturbed by a detach.
            bool isDetached = source == "detached";

            _logger.LogInformation($"🔍 [MdExplorer] Navigation source: {(isIframeLinkClick ? "iframe link click" : "Angular navigation")}");

            // A text file opened as the document (a click in the md-tree, a detached window):
            // colored source, not raw bytes. What a page asks for — an image, a link to a .json —
            // comes without "source" and stays raw, as always.
            if (!isIframeLinkClick)
            {
                var textFile = TextFileToShow(rootPathSystem, relativePathFile, relativePathExtension);
                if (textFile != null)
                {
                    return await ShowTextFile(textFile, rootPathSystem, connectionId, theme, isDetached);
                }
            }

            if (relativePathExtension != "" && relativePathExtension != ".md" && !relativePathFile.EndsWith(".md.directory"))
            {
                var responseForNotMdFile = CreateAResponseForNotMdFile(rootPathSystem,
                                                        relativePathFile,
                                                        relativePathExtension);
                if (responseForNotMdFile == null)
                {
                    return NotFound($"File not found: {relativePathFile}");
                }

                // Invia documentNavigated SOLO per file HTML (non per immagini, PDF, etc.)
                if (isIframeLinkClick && !string.IsNullOrEmpty(connectionId)
                    && (relativePathExtension == ".html" || relativePathExtension == ".htm"))
                {
                    await _hubContext.Clients.Client(connectionId: connectionId)
                        .SendAsync("documentNavigated", new {
                            fullPath = Path.Combine(rootPathSystem, relativePathFile.TrimStart('/', '\\')),
                            relativePath = relativePathFile,
                            name = Path.GetFileName(relativePathFile),
                            fullDirectoryPath = Path.GetDirectoryName(Path.Combine(rootPathSystem, relativePathFile.TrimStart('/', '\\')))
                        });
                    _logger.LogInformation($"📍 [MdExplorer] Navigation history event sent for HTML file: {relativePathFile}");
                }

                return responseForNotMdFile;
            }

            string fullPathFile = ManageIfThePathContainsExtensionMdOrNot(
                    rootPathSystem,
                    relativePathFile,
                    relativePathExtension);
            
            _logger.LogInformation($"🔍 [MdExplorer] fullPathFile: {fullPathFile}");

            // Calculate relative path properly
            var projectPath = GetProjectPath();
            var calculatedRelativePath = !string.IsNullOrEmpty(projectPath)
                ? fullPathFile.Replace(projectPath, string.Empty, StringComparison.OrdinalIgnoreCase)
                : relativePathFile; // Fallback to URL-extracted path if no project context

            // Remove leading separator if present
            calculatedRelativePath = calculatedRelativePath.TrimStart(Path.DirectorySeparatorChar, '/');
            
            _logger.LogInformation($"🔍 [MdExplorer] calculatedRelativePath: {calculatedRelativePath}");
            
            var monitoredMd = new MonitoredMDModel
            {
                Path = fullPathFile,
                Name = Path.GetFileName(fullPathFile),
                RelativePath = calculatedRelativePath,
                FullPath = fullPathFile,
                FullDirectoryPath = Path.GetDirectoryName(fullPathFile)
            };

            // Se è un file .md.directory e non esiste, crealo
            if (fullPathFile.EndsWith(".md.directory") && !System.IO.File.Exists(fullPathFile))
            {
                _logger.LogInformation($"🔍 [MdExplorer] Creating new .md.directory file: {fullPathFile}");
                
                // Estrai il nome della directory dal nome del file
                // Es: "Documentation.md.directory" -> "Documentation"
                var directoryName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fullPathFile));
                
                // Genera il contenuto iniziale con YAML front matter e titolo
                var defaultYaml = _yamlDefaultGenerator.GenerateDefaultYaml(GetProjectPath());
                var initialContent = $"{defaultYaml}# {directoryName}\n\n";
                
                try
                {
                    // Crea il file con contenuto iniziale
                    System.IO.File.WriteAllText(fullPathFile, initialContent, Encoding.UTF8);
                    _logger.LogInformation($"✅ [MdExplorer] Created .md.directory file: {fullPathFile}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ [MdExplorer] Error creating .md.directory file {fullPathFile}: {ex.Message}");
                    return StatusCode(500, $"Error creating TOC file: {ex.Message}");
                }
            }

            var markdownTxt = string.Empty;
            using (var fs = new FileStream(fullPathFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                markdownTxt = sr.ReadToEnd();
            }

            // Legge il descriptor YAML (se presente) per determinare il tipo di documento
            // NOTA: L'auto-generazione YAML avviene on-demand in GetDocumentSettings e MdExport,
            // non più alla visualizzazione del documento
            var descriptor = _yamlDocumentDescriptor.GetDescriptor(markdownTxt);

            var textHash = _helper.GetHashString(markdownTxt, Encoding.UTF8);
            var cacheName = Path.GetFileName(fullPathFile) + textHash + ".html";
            XmlDocument doc1 = null;
            string slideDeckHtml = null;
            // parse type of document. Choose between MarkdownType: slides, MarkdownType: document
            if (IsSlideDeck(descriptor))
            {
                slideDeckHtml = await ProcessAsSlideTypeDocument(
                    markdownTxt,
                    relativePathFile,
                    fullPathFile,
                    connectionId,
                    monitoredMd,
                    theme);
            }
            else
            {
                doc1 = await ProcessAsMarkdownTypeDocument(
                    markdownTxt,
                    relativePathFile,
                    fullPathFile,
                    connectionId,
                    monitoredMd,
                    theme);
            }



            //.Replace(@"\",@"\\");
            // Skip for detached windows: they render via the main window's connectionId
            // only to resolve the project, and must not push state into that window.
            if (!isDetached)
            {
                await _hubContext.Clients.Client(connectionId:connectionId).SendAsync("markdownfileisprocessed", monitoredMd);
            }

            // If navigation comes from iframe link click, notify Angular to update navigation history
            if (isIframeLinkClick)
            {
                await _hubContext.Clients.Client(connectionId: connectionId)
                    .SendAsync("documentNavigated", new {
                        fullPath = monitoredMd.FullPath,
                        relativePath = monitoredMd.RelativePath,
                        name = monitoredMd.Name,
                        fullDirectoryPath = monitoredMd.FullDirectoryPath
                    });
                _logger.LogInformation($"📍 [MdExplorer] Navigation history event sent for: {monitoredMd.RelativePath}");
            }

            // Get HTML content - check if using fallback mode
            string htmlContent;
            if (slideDeckHtml != null)
            {
                htmlContent = slideDeckHtml;
            }
            else if (doc1.DocumentElement != null &&
                doc1.DocumentElement.GetAttribute("_html_fallback") == "true")
            {
                // Using string-based fallback
                htmlContent = doc1.DocumentElement.InnerText;
            }
            else
            {
                // Using standard XML approach
                htmlContent = doc1.InnerXml;
            }

            try
            {
                System.IO.File.WriteAllText(rootPathSystem + Path.DirectorySeparatorChar + ".md" +
                                        Path.DirectorySeparatorChar + cacheName, htmlContent, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;

            }
            // Refresh database
            var engineDB = GetEngineDB();
            var relDal = engineDB.GetDal<MarkdownFile>();
            var mdFile = relDal.GetList().Where(_ => _.Path == fullPathFile).FirstOrDefault();
            engineDB.BeginTransaction();
            if (mdFile == null)
            {
                mdFile = new MarkdownFile
                {
                    FileName = Path.GetFileName(fullPathFile),
                    Path = fullPathFile,
                    FileType = "File"
                };
                relDal.Save(mdFile);
            }

            SaveLinksFromMarkdown(mdFile);
            engineDB.Commit();
            var toReturn = new ContentResult
            {
                ContentType = "text/html; charset=utf-8",
                Content = htmlContent,

            };
            return toReturn;
        }

        /// <summary>
        /// Fase 7h — elenco dei worktree degli agenti del progetto aperto (agente → path).
        /// Read-only: alimenta il sottomenu "Worktree" del toolbar.
        /// </summary>
        [HttpGet("/api/MdExplorerWorktree/list")]
        public async Task<IActionResult> ListAgentWorktrees()
        {
            var projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return Ok(new { worktrees = Array.Empty<object>() });
            // Il nome della cartella non dice più chi ci lavora: i posti si chiamano slot-1,
            // slot-2, e l'occupante lo si chiede a git (il branch in checkout).
            var list = (await _worktree.ListSlotsAsync(projectPath))
                .Where(x => x.Agent != null)
                .Select(x => new { agent = x.Agent, path = x.Path, slot = x.Index })
                .OrderBy(x => x.agent, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return Ok(new { worktrees = list });
        }

        /// <summary>
        /// Fase 7h — render READ-ONLY di un documento dal worktree di un agente (review di ciò
        /// che l'agente ha prodotto). NESSUNA scrittura: né cache <c>.md/</c>, né EngineDB, né
        /// eventi SignalR, né <c>SetCurrentDirectory</c>. La root è il worktree, risolto dal
        /// progetto aperto (connectionId); il progetto principale resta intatto.
        /// </summary>
        [HttpGet("/api/MdExplorerWorktree/render/{*url}")]
        public async Task<IActionResult> GetWorktreeReadOnlyAsync(string url)
        {
            var agent = Request.Query["agent"].ToString();
            var connectionId = Request.Query["ConnectionId"].ToString();
            var theme = Request.Query["theme"].FirstOrDefault() ?? "light";
            if (string.IsNullOrWhiteSpace(agent))
                return BadRequest("Parametro 'agent' richiesto.");

            var projectPath = GetProjectPath();
            if (string.IsNullOrEmpty(projectPath))
                return NotFound("Nessun progetto aperto per risolvere il worktree.");

            string worktreeRoot;
            try { worktreeRoot = await _worktree.FindAgentWorktreeAsync(projectPath, agent); }
            catch (ArgumentException) { return BadRequest($"Agente '{agent}' non valido."); }
            if (worktreeRoot == null || !Directory.Exists(worktreeRoot))
                return NotFound($"Nessun worktree per l'agente '{agent}'.");

            var rootPathSystem = worktreeRoot + Path.DirectorySeparatorChar;
            var relativePathFile = "/" + (url ?? string.Empty);
            var relativePathExtension = Path.GetExtension(relativePathFile);

            // Anti-traversal (§7h fix): il path risolto DEVE restare sotto la root del worktree.
            // Blocca '..%2F..' nel catch-all che altrimenti servirebbe file arbitrari dal disco.
            if (!IsUnderRoot(worktreeRoot, Path.GetFullPath(Path.Combine(rootPathSystem, relativePathFile.TrimStart('/', '\\')))))
            {
                _logger.LogWarning("❌ [MdExplorerWorktree] path traversal bloccato: '{Rel}' fuori da '{Root}'", relativePathFile, worktreeRoot);
                return BadRequest("Percorso non valido.");
            }

            // Asset non-md (immagini, ecc.): risolti dalla root del WORKTREE.
            if (relativePathExtension != "" && relativePathExtension != ".md" && !relativePathFile.EndsWith(".md.directory"))
            {
                var asset = CreateAResponseForNotMdFile(rootPathSystem, relativePathFile, relativePathExtension);
                return asset == null ? NotFound($"File non trovato nel worktree: {relativePathFile}") : (IActionResult)asset;
            }

            var fullPathFile = ManageIfThePathContainsExtensionMdOrNot(rootPathSystem, relativePathFile, relativePathExtension);
            if (!System.IO.File.Exists(fullPathFile))
                return NotFound($"'{relativePathFile}' non presente nel worktree di '{agent}'.");

            string markdownTxt;
            using (var fs = new FileStream(fullPathFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
                markdownTxt = sr.ReadToEnd();

            var monitoredMd = new MonitoredMDModel
            {
                Path = fullPathFile,
                Name = Path.GetFileName(fullPathFile),
                RelativePath = relativePathFile.TrimStart(Path.DirectorySeparatorChar, '/'),
                FullPath = fullPathFile,
                FullDirectoryPath = Path.GetDirectoryName(fullPathFile),
            };

            if (IsSlideDeck(_yamlDocumentDescriptor.GetDescriptor(markdownTxt)))
            {
                var deck = await ProcessAsSlideTypeDocument(
                    markdownTxt, relativePathFile, fullPathFile, connectionId, monitoredMd, theme,
                    explicitRoot: worktreeRoot, readOnly: true);
                return new ContentResult { ContentType = "text/html; charset=utf-8", Content = deck };
            }

            var doc1 = await ProcessAsMarkdownTypeDocument(
                markdownTxt, relativePathFile, fullPathFile, connectionId, monitoredMd, theme,
                explicitRoot: worktreeRoot, readOnly: true);

            var htmlContent = (doc1.DocumentElement != null &&
                doc1.DocumentElement.GetAttribute("_html_fallback") == "true")
                ? doc1.DocumentElement.InnerText
                : doc1.InnerXml;

            // NESSUNA scrittura cache/EngineDB, NESSUN evento SignalR: read-only puro.
            return new ContentResult { ContentType = "text/html; charset=utf-8", Content = htmlContent };
        }

        /// <summary>Il path risolto <paramref name="candidate"/> è dentro <paramref name="root"/>? (anti-traversal).</summary>
        private static bool IsUnderRoot(string root, string candidate)
        {
            var normRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, '/');
            var normCand = Path.GetFullPath(candidate);
            return string.Equals(normCand, normRoot, StringComparison.OrdinalIgnoreCase)
                || normCand.StartsWith(normRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSlideDeck(MdExplorerDocumentDescriptor descriptor)
            => descriptor?.DocumentType == "slides";

        /// <summary>
        /// A markdown slide deck as a reveal.js page (<see cref="SlideDeckRenderer"/>). The markdown
        /// goes through the document view's Markdig pipeline and MdExplorer's commands, as a document
        /// does. A deck that cannot be rendered as written shows what to change instead.
        /// </summary>
        private async Task<string> ProcessAsSlideTypeDocument(string markdownTxt,
                        string relativePathFile, string fullPathFile, string connectionId,
                        MonitoredMDModel monitoredMd, string theme,
                        string explicitRoot = null, bool readOnly = false)
        {
            var root = string.IsNullOrEmpty(explicitRoot) ? GetProjectPath() : explicitRoot;
            var requestInfo = new RequestInfo()
            {
                CurrentQueryRequest = relativePathFile,
                CurrentRoot = root,
                AbsolutePathFile = fullPathFile,
                RootQueryRequest = relativePathFile,
                ConnectionId = connectionId,
                BaseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}",
                ReadOnly = readOnly,
                SlideDeck = true,
            };
            var isPlantuml = markdownTxt.Contains("```plantuml") && !readOnly;
            if (isPlantuml)
            {
                await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("plantumlWorkStart", monitoredMd);
            }
            try
            {
                return SlideDeckRenderer.Render(markdownTxt, new SlideDeckRenderOptions
                {
                    Pipeline = BuildDocumentViewPipeline(),
                    DarkTheme = theme == "dark" || theme == "milan",
                    BeforeMarkdown = body => _commandRunner.TransformInNewMDFromMD(body, requestInfo),
                    AfterMarkdown = html =>
                    {
                        // The commands read the diagrams from ".md/" relative to the project, as for
                        // a document (and, as there, not in a read-only render).
                        if (!readOnly) Directory.SetCurrentDirectory(root);
                        return _commandRunner.TransformAfterConversion(html, requestInfo);
                    },
                });
            }
            catch (SlideDeckException ex)
            {
                _logger.LogWarning("⚠️ [Slides] {File}: {Message}", fullPathFile, ex.Message);
                return SlideDeckErrorPage(fullPathFile, ex.Message);
            }
            finally
            {
                if (isPlantuml)
                {
                    await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("plantumlWorkStop", monitoredMd);
                }
            }
        }

        private static string SlideDeckErrorPage(string fullPathFile, string message)
        {
            var file = WebUtility.HtmlEncode(Path.GetFileName(fullPathFile));
            return $@"<!DOCTYPE html>
<html><head><meta charset=""utf-8""><title>{file}</title></head>
<body style=""font-family: sans-serif; padding: 2em; line-height: 1.5"">
<h2>The slides of {file} cannot be shown</h2>
<p>{WebUtility.HtmlEncode(message)}</p>
</body></html>";
        }

        private string ManageIfThePathContainsExtensionMdOrNot(string rootPathSystem, string relativePathFile, string relativePathExtension)
        {
            // Se il file finisce con .md.directory, non aggiungere .md
            if (relativePathFile.EndsWith(".md.directory"))
            {
                return string.Concat(rootPathSystem, relativePathFile);
            }
            
            var fullPathFile = string.Concat(rootPathSystem, relativePathFile, ".md");
            if (relativePathExtension == ".md")
            {
                fullPathFile = string.Concat(rootPathSystem, relativePathFile);
            }

            return fullPathFile;
        }

        // Shown as the browser renders them, never as source: an HTML page opened from a link
        // comes back through the navigation history (source=angular) and must stay a page.
        private static readonly HashSet<string> RenderedByTheBrowser =
            new(StringComparer.OrdinalIgnoreCase) { ".html", ".htm", ".svg" };

        /// <summary>
        /// The full path of the text file to show as colored source, or null when the request is
        /// for something else: a markdown document (also asked without ".md"), a page or image the
        /// browser renders, a binary file, a path outside the project.
        /// </summary>
        private string TextFileToShow(string rootPathSystem, string relativePathFile, string extension)
        {
            if (extension == ".md" || relativePathFile.EndsWith(".md.directory") || RenderedByTheBrowser.Contains(extension))
            {
                return null;
            }

            var fullPath = Path.GetFullPath(Path.Combine(rootPathSystem, relativePathFile.TrimStart(Path.DirectorySeparatorChar, '/', '\\')));
            var root = Path.GetFullPath(rootPathSystem);
            if (!fullPath.StartsWith(root, StringComparison.Ordinal))
            {
                return null;
            }
            // Without extension the app asks for "doc" meaning "doc.md": the document wins.
            if (extension == "" && System.IO.File.Exists(fullPath + ".md"))
            {
                return null;
            }
            if (!System.IO.File.Exists(fullPath))
            {
                return null;
            }
            return TextFileView.IsText(fullPath) ? fullPath : null;
        }

        /// <summary>
        /// The document page for a text file: the same box as <c>```text(path)</c>, colored by
        /// Prism, with the app's theme. Read-only: nothing is cached, indexed or written.
        /// </summary>
        private async Task<IActionResult> ShowTextFile(string fullPath, string rootPathSystem, string connectionId, string theme, bool isDetached)
        {
            var size = new FileInfo(fullPath).Length;
            string body;
            if (size > TextFileView.MaxBytes)
            {
                body = $@"<div class=""mde-text-file-view""><p class=""mde-text-file-too-big"">{System.Web.HttpUtility.HtmlEncode(Path.GetFileName(fullPath))}: " +
                       $@"{size / 1024:N0} KB, troppo grande per mostrarlo qui (limite {TextFileView.MaxBytes / 1024} KB). Aprilo con l'editor esterno (matita nella barra).</p></div>";
            }
            else
            {
                var content = TextFileView.ReadText(fullPath);
                body = $@"<div class=""mde-text-file-view"">{TextFileView.ContainerHtml(Guid.NewGuid().ToString("N"), fullPath, TextFileView.LanguageFor(fullPath), content)}</div>";
            }

            var monitoredMd = new MonitoredMDModel
            {
                Path = fullPath,
                Name = Path.GetFileName(fullPath),
                RelativePath = Path.GetRelativePath(rootPathSystem, fullPath),
                FullPath = fullPath,
                FullDirectoryPath = Path.GetDirectoryName(fullPath)
            };
            // The document toolbar acts on the file the panel shows, whatever its kind.
            if (!isDetached)
            {
                await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("markdownfileisprocessed", monitoredMd);
            }

            var doc1 = new XmlDocument();
            CreateHTMLBody(body, doc1, fullPath, connectionId, GetProjectPath(), theme, sourceHash: "", viewKind: "text");
            var htmlContent = doc1.DocumentElement != null && doc1.DocumentElement.GetAttribute("_html_fallback") == "true"
                ? doc1.DocumentElement.InnerText
                : doc1.InnerXml;
            return new ContentResult { ContentType = "text/html; charset=utf-8", Content = htmlContent };
        }

        private FileContentResult CreateAResponseForNotMdFile(string rootPathSystem, string relativePathFile, string relativePathExtension)
        {
            // Rimuovi separatori iniziali per evitare che Path.Combine ignori il rootPath
            var cleanRelativePath = relativePathFile.TrimStart(Path.DirectorySeparatorChar, '/', '\\');
            var filePathSystem = Path.GetFullPath(Path.Combine(rootPathSystem, cleanRelativePath));

            // Se il percorso contiene .md directory (PlantUML images), cercare dalla root del progetto
            if (cleanRelativePath.Contains($"{Path.DirectorySeparatorChar}.md{Path.DirectorySeparatorChar}") ||
                cleanRelativePath.Contains("/.md/"))
            {
                // Trova la posizione di .md/ nel path
                var mdIndex = cleanRelativePath.IndexOf($"{Path.DirectorySeparatorChar}.md{Path.DirectorySeparatorChar}");
                if (mdIndex < 0) mdIndex = cleanRelativePath.IndexOf("/.md/");

                var filenameAfterMd = cleanRelativePath.Substring(mdIndex + 1); // include .md/filename
                filePathSystem = Path.GetFullPath(Path.Combine(rootPathSystem, filenameAfterMd));

                _logger.LogInformation($"🔍 [MdExplorer] PlantUML image path corrected:");
                _logger.LogInformation($"🔍 [MdExplorer] Original: {Path.Combine(rootPathSystem, relativePathFile)}");
                _logger.LogInformation($"🔍 [MdExplorer] Corrected: {filePathSystem}");
            }

            _logger.LogInformation($"🔍 [MdExplorer] CreateAResponseForNotMdFile:");
            _logger.LogInformation($"🔍 [MdExplorer]   rootPathSystem: '{rootPathSystem}'");
            _logger.LogInformation($"🔍 [MdExplorer]   relativePathFile: '{relativePathFile}'");
            _logger.LogInformation($"🔍 [MdExplorer]   cleanRelativePath: '{cleanRelativePath}'");
            _logger.LogInformation($"🔍 [MdExplorer]   filePathSystem: '{filePathSystem}'");
            _logger.LogInformation($"🔍 [MdExplorer]   File.Exists: {System.IO.File.Exists(filePathSystem)}");
            _logger.LogInformation($"🔍 [MdExplorer]   Directory.Exists: {System.IO.Directory.Exists(Path.GetDirectoryName(filePathSystem))}");

            if (!System.IO.File.Exists(filePathSystem))
            {
                _logger.LogWarning($"⚠️ [MdExplorer] File not found: '{filePathSystem}' - returning 404");
                return null; // Caller should handle null and return NotFound()
            }

            var data = System.IO.File.ReadAllBytes(filePathSystem);

            // Usa il provider standard di ASP.NET Core per i MIME types
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePathSystem, out var contentType))
            {
                // Fallback per estensioni non riconosciute
                contentType = "application/octet-stream";
            }

            var notMdFile = new FileContentResult(data, contentType);
            return notMdFile;
        }

        /// <summary>
        /// Renderizza un documento markdown in HTML. Fase 7h: <paramref name="explicitRoot"/> e
        /// <paramref name="readOnly"/> abilitano il render <b>read-only da una root esplicita</b>
        /// (worktree di un agente). Con i default (root = <see cref="MdControllerBase{T}.GetProjectPath"/>,
        /// readOnly = false) il comportamento è IDENTICO a prima. In readonly si saltano gli effetti
        /// collaterali che muterebbero lo stato della main window / del progetto aperto: eventi
        /// SignalR, valutazione Rule#1 (che è solo un evento) e <c>SetCurrentDirectory</c> (hazard
        /// globale). Le scritture EngineDB/cache restano fuori da qui (le fa il chiamante).
        /// </summary>
        private async Task<XmlDocument> ProcessAsMarkdownTypeDocument(
                string readText,
                string relativePathFileSystem,
                string fullPathFile,
                string connectionId,
                MonitoredMDModel monitoredMd,
                string theme = "light",
                string explicitRoot = null,
                bool readOnly = false)
        {
            // Impronta del file COM'È su disco, prima di qualunque trasformazione: la pagina la porta
            // (data-mde-source-hash), e un'azione che punta a "riga N" viene rifiutata se il file non
            // è più quello da cui la pagina è stata costruita — le righe punterebbero altrove.
            var sourceHash = MarkdownFileEditor.SourceHash(readText);

            // Root unica per tutto il metodo: il progetto aperto (normale) o il worktree (readonly).
            var root = string.IsNullOrEmpty(explicitRoot) ? GetProjectPath() : explicitRoot;
            var requestInfo = new RequestInfo()
            {
                CurrentQueryRequest = relativePathFileSystem,
                CurrentRoot = root,
                AbsolutePathFile = fullPathFile,
                RootQueryRequest = relativePathFileSystem,
                ConnectionId = connectionId,
                BaseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}",
                ReadOnly = readOnly,   // Fase 7h: i comandi non scrivono su disco né cambiano cwd
            };
            var isPlantuml = false;
            if (readText.Contains("```plantuml"))
            {
                isPlantuml = true;
                if (!readOnly)
                    await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("plantumlWorkStart", monitoredMd);
            }

            try
            {
            // Kept for the AI-selection source map: Markdig spans refer to the transformed
            // text, the data-mde-line-* attributes must point at the file on disk.
            var originalText = readText;
            readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);

            // Check if Rule #1 is enabled for current project.
            // Readonly (worktree review): Rule#1 produce SOLO un evento SignalR verso la main
            // window e legge il ProjectDB del progetto aperto (non del worktree) → si salta.
            var isRule1Enabled = false;
            try
            {
                // Check if Rule #1 is enabled in project settings (stored in ProjectDB)
                // Get IProjectDB from services
                var projectDB = readOnly ? null : HttpContext.RequestServices.GetService<IProjectDB>();
                if (projectDB != null)
                {
                    var projectSettingsDal = projectDB.GetDal<MdExplorer.Abstractions.Entities.ProjectDB.ProjectSetting>();
                    var rule1Setting = projectSettingsDal.GetList()
                        .FirstOrDefault(s => s.Name == "Rule1_CheckH1MatchesFilename");
                    
                    isRule1Enabled = rule1Setting?.ValueBool ?? false;
                    _logger.LogInformation($"🔍 [MdExplorer] Rule #1 enabled: {isRule1Enabled}");
                }
                else
                {
                    _logger.LogWarning("⚠️ [MdExplorer] ProjectDB not available");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Rule #1 setting");
            }

            // Apply Rule #1 only if enabled and not a .md.directory file
            if (isRule1Enabled && !fullPathFile.EndsWith(".md.directory"))
            {
                _logger.LogInformation($"✅ [MdExplorer] Applying Rule #1 check to: {fullPathFile}");
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
                    monitoredMd.Message = "It breaks Rule # 1";
                    monitoredMd.Action = "Rename the File!";
                    monitoredMd.FromFileName = Path.GetFileName(fullPathFile);
                    monitoredMd.ToFileName = theNameShouldBe;
                    monitoredMd.FullPath = Path.GetDirectoryName(fullPathFile);
                    await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("markdownbreakrule1", monitoredMd);
                }
            }
            else if (!isRule1Enabled)
            {
                _logger.LogInformation($"⏭️ [MdExplorer] Rule #1 is disabled for this project");
            }
            else if (fullPathFile.EndsWith(".md.directory"))
            {
                _logger.LogInformation($"⏭️ [MdExplorer] Skipping Rule #1 check for .md.directory file: {fullPathFile}");
            }

            // Shared with the corrections made on the page, which must read the file as it is rendered here.
            var pipeline = BuildDocumentViewPipeline();

            string result;
            try
            {
                try
                {
                    result = _sourceMapService.RenderHtmlWithSourceMap(originalText, readText, pipeline);
                }
                catch (Exception sourceMapEx)
                {
                    // The document must always render; the AI-selection feature degrades
                    // detectably (no data-mde-line-* attributes → no button).
                    _logger.LogError(sourceMapEx, "❌ [SourceMap] Source mapping failed for: {File} — rendering without source map, AI selection disabled on this document", fullPathFile);
                    result = Markdown.ToHtml(readText, pipeline);
                }
                // SetCurrentDirectory è un hazard GLOBALE (cwd di processo): in readonly si salta
                // per non corrompere render concorrenti del progetto aperto.
                if (!readOnly) Directory.SetCurrentDirectory(root);
                result = _commandRunner.TransformAfterConversion(result, requestInfo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"⚠️ [MdExplorer] Markdown rendering failed for: {fullPathFile}");
                if (!readOnly) Directory.SetCurrentDirectory(root);
                result = BuildMarkdownRenderingErrorHtml(fullPathFile, readText, ex);
            }

            var docSettingDal = _userSettingsDB.GetDal<DocumentSetting>();
            //var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == fullPathFile).FirstOrDefault();


            var btnDraw = AddButtonOnLowerBar("toggleMdCanvas(this)", "/assets/drawStatic.png","canvas");
            var btnNavBack = AddButtonOnLowerBar("navigateBack()", "/assets/nav-back.svg", "navBack", "mdeLowerBarButton mdeNavButton");
            var btnNavForward = AddButtonOnLowerBar("navigateForward()", "/assets/nav-forward.svg", "navForward", "mdeLowerBarButton mdeNavButton");
            var btnSearch = AddButtonOnLowerBar("toggleSearch()", "/assets/magnifier.svg", "searchButton", "mdeLowerBarButton mdeSearchButton");
            var btnTOC = AddButtonTextOnVerticalBar("toggleTOC()", "TOC", "btnToc");
            var btnRefs = AddButtonTextOnVerticalBar("openKnowledgeGraph()", "K.G.", "btnRefs");
            var resultToParse = $@"    
                   
                    <div  class=""mdeTocSticky-top"">                        
                        <div id=""TOC"" class=""tocNavigation"" mdeFullPathDocument=""{fullPathFile}"">
                            <div class=""mdeTocTitle"">Table of content</div>
                            <div class=""mdeNavigationMain"">
                                <div class=""tocSeparator"" onmousedown=""resizeToc()""></div>
                                <nav class=""tocNavNavigation"">
                                    <div class=""toc js-toc""></div>                                    
                                </nav>
                            </div>
                        </div>
                        <div id=""KGAnchor"" mdeFullPathDocument=""{fullPathFile}"" style=""display:none;""></div>
                        <div class=""mdeVerticalTab"">
                            <div class=""buttonTabToc"">
                                {btnTOC}                             
                            </div>
                            <div class=""buttonTabRefs"">
                                {btnRefs}
                            </div>
                        </div>
                    </div>
                    <div class=""mdeLowerBar"">
                             {btnDraw}
                             {btnNavBack}
                             {btnNavForward}
                             {btnSearch}
                             <div id=""searchContainer"" class=""mdeSearchContainer"" style=""display: none;"">
                                <input type=""text"" id=""searchInput"" class=""mdeSearchInput"" placeholder=""Cerca..."" />
                                <span id=""searchResultCount"" class=""mdeSearchResultCount""></span>
                                <button id=""searchPrev"" class=""mdeSearchNavButton"" onclick=""navigateSearchResult(-1)"">▲</button>
                                <button id=""searchNext"" class=""mdeSearchNavButton"" onclick=""navigateSearchResult(1)"">▼</button>
                                <button id=""searchClose"" class=""mdeSearchCloseButton"" onclick=""closeSearch()"">✕</button>
                             </div>
                    </div>
                    <div class=""mdeContainerIFrameApplciation"">
                        <div class=""mdeItemMainPageLeftMenu"" ></div>

                        <div class=""mdeItemMainPageCenter md-tocbot-content js-toc-content"">
                            {result}
                        </div>

                        <div class=""mdeItemMainPageRightMenu"" ></div>
                    </div>
                     
                    ";
            XmlDocument doc1 = new XmlDocument();
            CreateHTMLBody(resultToParse, doc1, fullPathFile, connectionId, root, theme, sourceHash);

            try
            {
                var elementsA = doc1.FirstChild.SelectNodes("//a");
                if (elementsA != null)
                {
                    foreach (XmlNode itemElement in elementsA)
                    {
                        var href = itemElement.Attributes["href"];
                        if (href != null && href.Value.Length > 8)
                        {
                            if (Regex.Match(href.Value, "http[s]?://(?!localhost)").Success)
                            {
                                var htmltarget = doc1.CreateAttribute("target");
                                htmltarget.InnerText = "_target";
                                itemElement.Attributes.Append(htmltarget);
                            }

                        }

                        var htmlClass = doc1.CreateAttribute("class");
                        htmlClass.InnerText = "mdExplorerLink";
                        itemElement.Attributes.Append(htmlClass);
                    }
                }
            }
            catch (Exception ex)
            {
                // If link manipulation fails (e.g., due to fallback rendering), skip it
                // The content will still be displayed, just without the link enhancements
                _logger.LogWarning($"⚠️ [MdExplorer] Could not enhance links: {ex.Message}");
            }

            return doc1;
            }
            finally
            {
                if (isPlantuml && !readOnly)
                {
                    await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("plantumlWorkStop", monitoredMd);
                }
            }
        }

        /// <param name="viewKind">"text" for a text file shown as source: the page scripts that edit a
        /// markdown document (paste an image, …) stay off there.</param>
        private static void CreateHTMLBody(string resultToParse, XmlDocument doc1, string filePathSystem1, string connectionId, string projectPath = "", string theme = "light", string sourceHash = "", string viewKind = null)
        {
            var isDark = theme == "dark" || theme == "milan";
            var html = doc1.CreateElement("html");
            var htmlStyle = doc1.CreateAttribute("style");
            htmlStyle.Value = "overflow: auto; height: auto; min-height: 100%;";
            html.Attributes.Append(htmlStyle);

            doc1.AppendChild(html);
            var head = doc1.CreateElement("head");

            var link2 = doc1.CreateElement("script");
            var link = doc1.CreateElement("link");
            var linkHref = doc1.CreateAttribute("href");
            var linkRel = doc1.CreateAttribute("rel");
            linkHref.Value = "/MdCustomCSS.css";
            linkRel.Value = "stylesheet";
            link.Attributes.Append(linkHref);
            link.Attributes.Append(linkRel);
            head.AppendChild(link);


            html.AppendChild(head);
            var body = doc1.CreateElement("body");
            var BodyId = doc1.CreateAttribute("Id");
            var ConnectionId = doc1.CreateAttribute("ConnectionId");
            var DocumentPath = doc1.CreateAttribute("DocumentPath");
            var ProjectPath = doc1.CreateAttribute("ProjectPath");
            var SourceHash = doc1.CreateAttribute("data-mde-source-hash");
            var bodyStyle = doc1.CreateAttribute("style");
            bodyStyle.Value = "overflow: visible; height: auto; min-height: 100vh; margin: 0; padding: 0;";
            BodyId.Value = "MdBody";
            ConnectionId.Value = connectionId;
            DocumentPath.Value = filePathSystem1;
            ProjectPath.Value = projectPath ?? "";
            SourceHash.Value = sourceHash ?? "";
            body.Attributes.Append(BodyId);
            body.Attributes.Append(ConnectionId);
            body.Attributes.Append(DocumentPath);
            body.Attributes.Append(ProjectPath);
            body.Attributes.Append(SourceHash);
            if (!string.IsNullOrEmpty(viewKind))
            {
                var view = doc1.CreateAttribute("data-mde-view");
                view.Value = viewKind;
                body.Attributes.Append(view);
            }
            body.Attributes.Append(bodyStyle);
            if (isDark)
            {
                var bodyClass = doc1.CreateAttribute("class");
                bodyClass.Value = "dark-theme";
                body.Attributes.Append(bodyClass);
            }
            html.AppendChild(body);

            var darkThemeLink = isDark ? @"<link rel=""stylesheet"" href=""/dark-theme.css"" />" : "";
            head.InnerXml = $@"
            <link rel=""stylesheet"" href=""/common.css"" />
            {darkThemeLink}
            <script src=""/common.js""></script>";

            try
            {
                body.InnerXml += resultToParse;
            }
            catch (XmlException ex)
            {
                // GitHub-flavored markdown may generate HTML that is not well-formed XML
                // Use string-based HTML construction as fallback
                System.Diagnostics.Debug.WriteLine($"XmlException in CreateHTMLBody: {ex.Message}");
                System.Diagnostics.Debug.WriteLine("Using string-based HTML construction as fallback");

                // Build complete HTML document as string
                var darkClass = isDark ? @" class=""dark-theme""" : "";
                var viewAttribute = string.IsNullOrEmpty(viewKind) ? "" : $@" data-mde-view=""{viewKind}""";
                var darkLink = isDark ? @"<link rel=""stylesheet"" href=""/dark-theme.css"" />" : "";
                var htmlString = $@"<html style=""overflow: auto; height: auto; min-height: 100%;"">
<head>
    <link href=""/MdCustomCSS.css"" rel=""stylesheet"" />
    <link rel=""stylesheet"" href=""/common.css"" />
    {darkLink}
    <script src=""/common.js""></script>
</head>
<body Id=""MdBody"" ConnectionId=""{connectionId}"" DocumentPath=""{filePathSystem1}"" ProjectPath=""{projectPath}"" data-mde-source-hash=""{sourceHash}""{viewAttribute}{darkClass} style=""overflow: visible; height: auto; min-height: 100vh; margin: 0; padding: 0;"">
{resultToParse}
</body>
</html>";

                // Load the complete HTML string into the XmlDocument
                // This will be used as a string, not parsed as XML
                doc1.LoadXml("<root></root>"); // Reset document
                doc1.PreserveWhitespace = true;

                // Store the HTML string in a special marker that will be handled differently
                var root = doc1.DocumentElement;
                root.SetAttribute("_html_fallback", "true");
                root.InnerText = htmlString;
            }
        }

       

        private string AddButtonOnLowerBar(string functionJs, string image, string Id, string cssClass = "mdeLowerBarButton")
        {
            try
            {
                var doc1 = new XmlDocument();
                var body = doc1.CreateElement("div");
                var a = doc1.CreateElement("a");
                var aAtt = doc1.CreateAttribute("onClick");
                var aAtt3 = doc1.CreateAttribute("class");
                aAtt3.Value = cssClass;
                body.Attributes.Append(aAtt3);
                a.Attributes.Append(aAtt);
                aAtt.Value = functionJs;
                var imgEl = doc1.CreateElement("img");
                a.AppendChild(imgEl);
                var srcImg = doc1.CreateAttribute("src");
                var id = doc1.CreateAttribute("id");
                srcImg.Value = image;
                id.Value = Id;
                imgEl.Attributes.Append(srcImg);
                imgEl.Attributes.Append(id);
                body.AppendChild(a);
                return body.OuterXml;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ [MdExplorer] Could not create button {Id}: {ex.Message}");
                // Return a simple HTML fallback
                return $"<div class=\"{cssClass}\"><a onclick=\"{functionJs}\"><img src=\"{image}\" id=\"{Id}\" /></a></div>";
            }
        }

        private string AddButtonTextOnVerticalBar(string onClickJs, string text, string Id)
        {
            try
            {
                var doc1 = new XmlDocument();
                var body = doc1.CreateElement("div");
                var a = doc1.CreateElement("div");
                a.InnerText = text;
                var attClick = doc1.CreateAttribute("onClick");
                attClick.Value = onClickJs;
                var attStyle = doc1.CreateAttribute("style");
                attStyle.Value = "cursor: pointer";
                var attId = doc1.CreateAttribute("id");
                attId.Value = Id;
                a.Attributes.Append(attClick);
                a.Attributes.Append(attStyle);
                a.Attributes.Append(attId);
                body.AppendChild(a);
                return body.OuterXml;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ [MdExplorer] Could not create text button {Id}: {ex.Message}");
                return $"<div><div onclick=\"{onClickJs}\" style=\"cursor: pointer\" id=\"{Id}\">{text}</div></div>";
            }
        }

        /// <summary>
        /// Builds an HTML error page when Markdown rendering fails.
        /// Shows the error details and a copyable AI prompt to help fix the source file.
        /// </summary>
        private static string BuildMarkdownRenderingErrorHtml(string fullPathFile, string markdownSource, Exception ex)
        {
            var fileName = Path.GetFileName(fullPathFile);
            var errorType = ex.GetType().Name;
            var errorMessage = System.Security.SecurityElement.Escape(ex.Message);

            // Identify lines that may be problematic (very long lines, or lines with complex nesting)
            var lines = markdownSource.Split('\n');
            var suspectLines = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                // Flag lines that are very long (likely complex table rows) or contain unescaped backticks inside tables
                if (line.TrimStart().StartsWith("|") && line.Length > 300)
                {
                    suspectLines.AppendLine($"  - Line {i + 1} ({line.Length} chars): table row with complex content");
                }
            }

            var suspectSection = suspectLines.Length > 0
                ? $"<h3>Suspect lines</h3><pre>{System.Security.SecurityElement.Escape(suspectLines.ToString())}</pre>"
                : "";

            // Build a copyable prompt for the AI
            var aiPrompt = $@"The file ""{fileName}"" cannot be rendered by MdExplorer due to a Markdown parsing error.

Error: {ex.Message}

{(suspectLines.Length > 0 ? "Suspect lines:\n" + suspectLines.ToString() : "")}
Please fix the Markdown source so that it can be parsed correctly. Common causes:
- Triple backticks (```) inside table cells create deeply nested inline elements. Escape them as \`\`\` or replace with descriptive text like ""triple backtick"".
- Very long table rows with mixed formatting (bold, code, links) can exceed parser nesting limits.
- Unbalanced formatting markers (*, **, `) inside table cells.

Keep the content and meaning identical — only fix the Markdown syntax.";

            var escapedPrompt = System.Security.SecurityElement.Escape(aiPrompt)
                .Replace("\n", "&#10;")
                .Replace("\r", "");

            var escapedPromptForJs = aiPrompt
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "");

            return $@"
<div style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            max-width: 800px; margin: 40px auto; padding: 20px;"">

    <div style=""background: #fff3cd; border: 1px solid #ffc107; border-radius: 8px; padding: 20px; margin-bottom: 20px;"">
        <h2 style=""margin-top: 0; color: #856404;"">&#9888; Markdown rendering failed</h2>
        <p style=""color: #856404; margin-bottom: 0;"">
            The file <strong>{System.Security.SecurityElement.Escape(fileName)}</strong> could not be rendered.
        </p>
    </div>

    <div style=""background: #f8f9fa; border: 1px solid #dee2e6; border-radius: 8px; padding: 20px; margin-bottom: 20px;"">
        <h3 style=""margin-top: 0;"">Error details</h3>
        <p><strong>Type:</strong> {errorType}</p>
        <p><strong>Message:</strong> {errorMessage}</p>
        <p><strong>File:</strong> <code>{System.Security.SecurityElement.Escape(fullPathFile)}</code></p>
        {suspectSection}
    </div>

    <div style=""background: #e7f3ff; border: 1px solid #b6d4fe; border-radius: 8px; padding: 20px; margin-bottom: 20px;"">
        <h3 style=""margin-top: 0; color: #0a58ca;"">&#129302; AI Fix Prompt</h3>
        <p style=""color: #0a58ca;"">Copy the text below and paste it to your AI assistant to get the file fixed:</p>
        <textarea id=""aiPromptText"" readonly
                  style=""width: 100%; height: 200px; font-family: monospace; font-size: 13px;
                         padding: 12px; border: 1px solid #b6d4fe; border-radius: 4px;
                         background: #ffffff; resize: vertical;""
        >{escapedPrompt}</textarea>
        <br />
        <button onclick=""navigator.clipboard.writeText(document.getElementById('aiPromptText').value).then(function(){{ var b=event.target; b.textContent='Copied!'; setTimeout(function(){{ b.textContent='Copy to clipboard'; }}, 2000); }});""
                style=""margin-top: 10px; padding: 8px 20px; background: #0a58ca; color: white;
                       border: none; border-radius: 4px; cursor: pointer; font-size: 14px;"">
            Copy to clipboard
        </button>
    </div>

</div>";
        }

    }


}
