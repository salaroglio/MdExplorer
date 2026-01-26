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
using Microsoft.Extensions.DependencyInjection;
using MdExplorer.Services.DatabaseManager;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("/api/MdExplorer/{*url}")]
    public class MdExplorerController : MdControllerBase<MdExplorerController>//ControllerBase
    {
        private readonly IGoodMdRule<FileInfoNode>[] _goodRules;        
        private readonly IYamlParser<MdExplorerDocumentDescriptor> _yamlDocumentDescriptor;
        private readonly IYamlDefaultGenerator _yamlDefaultGenerator;

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
            IDatabaseManager databaseManager = null
            ) : base(logger, options, hubContext, session, engineDB, commandRunner,modifiers, helper, databaseManager)
        {
            _goodRules = GoodRules;
            
            _yamlDocumentDescriptor = yamlDocumentDescriptor;
            _yamlDefaultGenerator = yamlDefaultGenerator;
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
            var source = Request.Query["source"]; // "angular" or null
            bool isIframeLinkClick = string.IsNullOrEmpty(source);

            _logger.LogInformation($"🔍 [MdExplorer] Navigation source: {(isIframeLinkClick ? "iframe link click" : "Angular navigation")}");

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
            // parse type of document. Choose between MarkdownType: slides, MarkdownType: document
            if (descriptor!= null &&  descriptor.DocumentType == "slides")
            {
                doc1 = await ProcessAsSlideTypeDocument(
                    markdownTxt,
                    relativePathFile,
                    fullPathFile,
                    monitoredMd);
            }
            else
            {
                doc1 = await ProcessAsMarkdownTypeDocument(
                    markdownTxt,
                    relativePathFile,
                    fullPathFile,
                    connectionId,
                    monitoredMd);
            }



            //.Replace(@"\",@"\\");
            await _hubContext.Clients.Client(connectionId:connectionId).SendAsync("markdownfileisprocessed", monitoredMd);

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
            if (doc1.DocumentElement != null &&
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

        private async Task<XmlDocument> ProcessAsSlideTypeDocument(string markdownTxt, 
                        string relativePathFile, string fullPathFile, MonitoredMDModel monitoredMd)
        {

            Regex rx = new Regex(@"-{3}([^-{3}]*)-{3}(.*)",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdownTxt);

            var innerXML = matches[0].Groups[2].Value;

            var doc1 = new XmlDocument();
            var html = doc1.CreateElement("html");
            doc1.AppendChild(html);
            var head = doc1.CreateElement("head");
            html.AppendChild(head);
            var body = doc1.CreateElement("body");
            html.AppendChild(body);

            head.InnerXml = $@"
            <link rel=""stylesheet"" href=""/commonSlide.css"" />            
            "; //<script src=""/commonSlide.js""></script>

            // add final div and script

            var finalExecutionScript = @"
                <script src=""/reveal/dist/reveal.js""></script>
                <script src =""/reveal/plugin/zoom/zoom.js""></script>
                <script src =""/reveal/plugin/notes/notes.js""></script>
                <script src =""/reveal/plugin/search/search.js""></script>
                <script src =""/reveal/plugin/markdown/markdown.js""></script>
                <script src =""/reveal/plugin/highlight/highlight.js""></script>
                ";

            var execScript = @"
            <script>
			// Also available as an ES module, see:
			// https://revealjs.com/initialization/
			Reveal.initialize({
				controls: true,
				progress: true,
				center: true,
				hash: true,

				// Learn about plugins: https://revealjs.com/plugins/
				plugins: [ RevealZoom, RevealNotes, RevealSearch, RevealMarkdown, RevealHighlight ]
			});

            </script>
            ";

            var xmlForBody = string.Concat(innerXML, finalExecutionScript, execScript);
            body.InnerXml += xmlForBody;

            return doc1;            
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

        private async Task<XmlDocument> ProcessAsMarkdownTypeDocument(
                string readText,
                string relativePathFileSystem,
                string fullPathFile,
                string connectionId,
                MonitoredMDModel monitoredMd)
        {
            var requestInfo = new RequestInfo()
            {
                CurrentQueryRequest = relativePathFileSystem,
                CurrentRoot = GetProjectPath(),
                AbsolutePathFile = fullPathFile,
                RootQueryRequest = relativePathFileSystem,
                ConnectionId = connectionId,
                BaseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}",
            };
            var isPlantuml = false;
            if (readText.Contains("```plantuml"))
            {
                isPlantuml = true;
                await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("plantumlWorkStart", monitoredMd);
            }

            readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);

            // Check if Rule #1 is enabled for current project
            var isRule1Enabled = false;
            try
            {
                // Check if Rule #1 is enabled in project settings (stored in ProjectDB)
                // Get IProjectDB from services
                var projectDB = HttpContext.RequestServices.GetService<IProjectDB>();
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

            var settingDal = _userSettingsDB.GetDal<Setting>();
            var jiraUrl = settingDal.GetList().Where(_ => _.Name == "JiraServer").FirstOrDefault()?.ValueString;

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseDiagrams()
                .UsePipeTables()
                .UseBootstrap()
                .UseJiraLinks(new JiraLinkOptions(jiraUrl)) //@"https://jira.swarco.com"                
                .UseEmojiAndSmiley()
                .UseYamlFrontMatter()
                .UseGenericAttributes()
                
                .Build();

            var result = Markdown.ToHtml(readText, pipeline);
            Directory.SetCurrentDirectory(GetProjectPath());

            

            //try
            //{
            //    if (System.IO.File.Exists(rootPathSystem + Path.DirectorySeparatorChar + ".md" +
            //                            Path.DirectorySeparatorChar + cacheName))
            //    {

            //        var currentHtml = System.IO.File.ReadAllText(rootPathSystem + Path.DirectorySeparatorChar + ".md" +
            //                               Path.DirectorySeparatorChar + cacheName);
            //        if (currentHtml != String.Empty)
            //        {
            //            var myurl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            //            var regularExpression = @$"{this.Request.Scheme}://localhost([^/]*)";
            //            Regex rx = new Regex(regularExpression,
            //                       RegexOptions.Compiled | RegexOptions.IgnoreCase);
            //            var matches = rx.Matches(currentHtml);
            //            currentHtml = Regex.Replace(currentHtml, regularExpression, myurl);

            //            await _hubContext.Clients.All.SendAsync("markdownfileisprocessed", monitoredMd);
            //            var toQuickReturn = new ContentResult
            //            {
            //                ContentType = "text/html; charset=utf-8",
            //                Content = currentHtml,

            //            };
            //            return toQuickReturn;
            //        }

            //    }
            //}
            //catch (Exception ex)
            //{
            //    var msg = ex.Message;

            //}



            //Directory.SetCurrentDirectory(GetProjectPath());
            result = _commandRunner.TransformAfterConversion(result, requestInfo);

            var docSettingDal = _userSettingsDB.GetDal<DocumentSetting>();
            //var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == fullPathFile).FirstOrDefault();


            var btnDraw = AddButtonOnLowerBar("toggleMdCanvas(this)", "/assets/drawStatic.png","canvas");
            var btnNavBack = AddButtonOnLowerBar("navigateBack()", "/assets/nav-back.svg", "navBack", "mdeLowerBarButton mdeNavButton");
            var btnNavForward = AddButtonOnLowerBar("navigateForward()", "/assets/nav-forward.svg", "navForward", "mdeLowerBarButton mdeNavButton");
            var btnSearch = AddButtonOnLowerBar("toggleSearch()", "/assets/magnifier.svg", "searchButton", "mdeLowerBarButton mdeSearchButton");
            var btnTOC = AddButtonTextOnVerticalBar($"toggleTOC('{HttpUtility.UrlEncode(fullPathFile)}')", "TOC", "toc");
            var btnRefs = AddButtonTextOnVerticalBar($"toggleReferences('{HttpUtility.UrlEncode(fullPathFile)}')", "Refs", "toc");
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
                        <div id=""Refs"" class=""refsNavigation"" mdeFullPathDocument=""{fullPathFile}"">
                            <div class=""mdeRefsTitle"">References</div>
                            <div class=""mdeNavigationMain"">
                                <div class=""tocSeparator"" onmousedown=""resizeRefs()""></div>
                                <nav class=""refsNavNavigation"">
                                    <div id=""references"" class=""refsMain""></div>                                    
                                </nav>
                            </div>
                        </div>
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
            var projectPath = GetProjectPath();
            CreateHTMLBody(resultToParse, doc1, fullPathFile, connectionId, projectPath);

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

            if (isPlantuml)
            {                 
                await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("plantumlWorkStop", monitoredMd);
            }
            return doc1;
        }

        private static void CreateHTMLBody(string resultToParse, XmlDocument doc1, string filePathSystem1, string connectionId, string projectPath = "")
        {
            var html = doc1.CreateElement("html");
            // IFRAME SCROLLING FIX: Permetti scrolling naturale nell'iframe
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
            var bodyStyle = doc1.CreateAttribute("style");
            // IFRAME SCROLLING FIX: Permetti scrolling naturale nel body
            bodyStyle.Value = "overflow: visible; height: auto; min-height: 100vh; margin: 0; padding: 0;";
            BodyId.Value = "MdBody";
            ConnectionId.Value = connectionId;
            DocumentPath.Value = filePathSystem1;
            ProjectPath.Value = projectPath ?? "";
            body.Attributes.Append(BodyId);
            body.Attributes.Append(ConnectionId);
            body.Attributes.Append(DocumentPath);
            body.Attributes.Append(ProjectPath);
            body.Attributes.Append(bodyStyle);
            html.AppendChild(body);


            head.InnerXml = $@"
            <link rel=""stylesheet"" href=""/common.css"" />
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
                var htmlString = $@"<html style=""overflow: auto; height: auto; min-height: 100%;"">
<head>
    <link href=""/MdCustomCSS.css"" rel=""stylesheet"" />
    <link rel=""stylesheet"" href=""/common.css"" />
    <script src=""/common.js""></script>
</head>
<body Id=""MdBody"" ConnectionId=""{connectionId}"" DocumentPath=""{filePathSystem1}"" ProjectPath=""{projectPath}"" style=""overflow: visible; height: auto; min-height: 100vh; margin: 0; padding: 0;"">
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

        private string AddButtonTextOnVerticalBar(string functionJs, string text, string Id)
        {
            try
            {
                var doc1 = new XmlDocument();
                var body = doc1.CreateElement("div");
                var a = doc1.CreateElement("div");
                a.InnerText = text;
                var aAtt = doc1.CreateAttribute("onClick");
                var att2 = doc1.CreateAttribute("style");
                att2.Value = "cursor: pointer";
                a.Attributes.Append(aAtt);
                a.Attributes.Append(att2);
                aAtt.Value = functionJs;
                var id = doc1.CreateAttribute("id");
                id.Value = Id;
                body.AppendChild(a);
                return body.OuterXml;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ [MdExplorer] Could not create text button {Id}: {ex.Message}");
                // Return a simple HTML fallback
                return $"<div><div onclick=\"{functionJs}\" style=\"cursor: pointer\" id=\"{Id}\">{text}</div></div>";
            }
        }

    }


}
