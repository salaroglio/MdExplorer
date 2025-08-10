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
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB session,
            IEngineDB engineDB,
            ICommandRunnerHtml commandRunner,
            IGoodMdRule<FileInfoNode>[] GoodRules,
            IHelper helper,
            IYamlParser<MdExplorerDocumentDescriptor> yamlDocumentDescriptor,
            IYamlDefaultGenerator yamlDefaultGenerator,
            IWorkLink[] modifiers            
            ) : base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner,modifiers, helper)
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
            
            var rootPathSystem = $"{_fileSystemWatcher.Path}{Path.DirectorySeparatorChar}";
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

            if (relativePathExtension != "" && relativePathExtension != ".md")
            {
                var responseForNotMdFile = CreateAResponseForNotMdFile(rootPathSystem,
                                                        relativePathFile,
                                                        relativePathExtension);
                return responseForNotMdFile;
            }
            var connectionId = Request.Query["ConnectionId"];
            string fullPathFile = ManageIfThePathContainsExtensionMdOrNot(
                    rootPathSystem, 
                    relativePathFile, 
                    relativePathExtension);
            
            _logger.LogInformation($"🔍 [MdExplorer] fullPathFile: {fullPathFile}");

            // Calculate relative path properly
            var calculatedRelativePath = fullPathFile.Replace(_fileSystemWatcher.Path, string.Empty);
            if (calculatedRelativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
            {
                calculatedRelativePath = calculatedRelativePath.Substring(1);
            }
            
            _logger.LogInformation($"🔍 [MdExplorer] calculatedRelativePath: {calculatedRelativePath}");
            
            var monitoredMd = new MonitoredMDModel
            {
                Path = fullPathFile,
                Name = Path.GetFileName(fullPathFile),
                RelativePath = calculatedRelativePath,
                FullPath = fullPathFile,
                FullDirectoryPath = Path.GetDirectoryName(fullPathFile)
            };

            var markdownTxt = string.Empty;
            using (var fs = new FileStream(fullPathFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                markdownTxt = sr.ReadToEnd();
            }

            // Verifica se il documento ha YAML front matter e aggiungilo se mancante
            var descriptor = _yamlDocumentDescriptor.GetDescriptor(markdownTxt);
            if (descriptor == null || descriptor.WordSection == null)
            {
                _logger.LogInformation($"🔍 [MdExplorer] YAML front matter mancante per {fullPathFile}, aggiunta automatica...");
                
                // YAML mancante o invalido - aggiungiamolo automaticamente
                var defaultYaml = _yamlDefaultGenerator.GenerateDefaultYaml(_fileSystemWatcher.Path);
                var updatedContent = defaultYaml + markdownTxt;
                
                // Salva il file evitando il trigger del FileSystemWatcher
                _fileSystemWatcher.EnableRaisingEvents = false;
                try
                {
                    System.IO.File.WriteAllText(fullPathFile, updatedContent);
                    
                    // Notifica l'utente via SignalR
                    await _hubContext.Clients.Client(connectionId: connectionId)
                        .SendAsync("yamlAutoGenerated", new {
                            message = "Metadati YAML aggiunti automaticamente al documento",
                            filePath = fullPathFile
                        });
                    
                    _logger.LogInformation($"✅ [MdExplorer] YAML front matter aggiunto con successo a {fullPathFile}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ [MdExplorer] Errore durante l'aggiunta dello YAML a {fullPathFile}: {ex.Message}");
                }
                finally
                {
                    _fileSystemWatcher.EnableRaisingEvents = true;
                }
                
                // Usa il contenuto aggiornato per il processing
                markdownTxt = updatedContent;
                // Ricarica il descriptor con il nuovo contenuto
                descriptor = _yamlDocumentDescriptor.GetDescriptor(markdownTxt);
            }

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

            try
            {
                System.IO.File.WriteAllText(rootPathSystem + Path.DirectorySeparatorChar + ".md" +
                                        Path.DirectorySeparatorChar + cacheName, doc1.InnerXml, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;

            }
            // Refresh database
            var relDal = _engineDB.GetDal<MarkdownFile>();
            var mdFile = relDal.GetList().Where(_ => _.Path == fullPathFile).FirstOrDefault();
            _engineDB.BeginTransaction();
            if (mdFile == null)
            {
                var markdownFile = new MarkdownFile
                {
                    FileName = Path.GetFileName(fullPathFile),
                    Path = fullPathFile,                    
                    FileType = "File"
                };
                relDal.Save(markdownFile);
            }
            
            SaveLinksFromMarkdown(mdFile);
            _engineDB.Commit();
            var toReturn = new ContentResult
            {
                ContentType = "text/html; charset=utf-8",
                Content = doc1.InnerXml,

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
            var fullPathFile = string.Concat(rootPathSystem, relativePathFile, ".md");
            if (relativePathExtension == ".md")
            {
                fullPathFile = string.Concat(rootPathSystem, relativePathFile);
            }

            return fullPathFile;
        }

        private FileContentResult CreateAResponseForNotMdFile(string rootPathSystem, string relativePathFile, string relativePathExtension)
        {
            var filePathSystem = string.Concat(rootPathSystem, relativePathFile);
            
            // Se il percorso contiene .md directory (PlantUML images), cercare dalla root del progetto
            if (relativePathFile.Contains($"{Path.DirectorySeparatorChar}.md{Path.DirectorySeparatorChar}"))
            {
                var mdIndex = relativePathFile.IndexOf($"{Path.DirectorySeparatorChar}.md{Path.DirectorySeparatorChar}");
                var filenameAfterMd = relativePathFile.Substring(mdIndex + 1); // include .md/filename
                filePathSystem = Path.Combine(rootPathSystem, filenameAfterMd);
                
                _logger.LogInformation($"🔍 [MdExplorer] PlantUML image path corrected:");
                _logger.LogInformation($"🔍 [MdExplorer] Original: {string.Concat(rootPathSystem, relativePathFile)}");
                _logger.LogInformation($"🔍 [MdExplorer] Corrected: {filePathSystem}");
            }
            
            var data = System.IO.File.ReadAllBytes(filePathSystem);
            var currentContetType = $"image/{relativePathExtension.Replace(".", string.Empty)}";
            if (relativePathExtension == ".pdf")
            {
                currentContetType = $"application/{relativePathExtension}";
            }
            if (relativePathExtension == ".svg")
            {
                currentContetType = $"image/svg+xml";
            }
            var notMdFile = new FileContentResult(data, currentContetType);
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
                CurrentRoot = _fileSystemWatcher.Path,
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

            var goodMdRuleFileNameShouldBeSameAsTitle =
                    _goodRules.First(_ => _.GetType() ==
                        typeof(GoodMdRuleFileNameShouldBeSameAsTitle));

            var fileNode = new FileInfoNode
            {
                FullPath = fullPathFile,
                Name = Path.GetFileName(fullPathFile),
                DataText = readText
            };
            //bool isBroken;
            //string theNameShouldBe;
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
            Directory.SetCurrentDirectory(_fileSystemWatcher.Path);

            

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



            //Directory.SetCurrentDirectory(_fileSystemWatcher.Path);
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
                        
                    </div>
                     
                    ";
            XmlDocument doc1 = new XmlDocument();
            CreateHTMLBody(resultToParse, doc1, fullPathFile, connectionId);

            var elementsA = doc1.FirstChild.SelectNodes("//a");
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

            if (isPlantuml)
            {                 
                await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("plantumlWorkStop", monitoredMd);
            }
            return doc1;
        }

        private static void CreateHTMLBody(string resultToParse, XmlDocument doc1, string filePathSystem1, string connectionId)
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
            var bodyStyle = doc1.CreateAttribute("style");
            // IFRAME SCROLLING FIX: Permetti scrolling naturale nel body
            bodyStyle.Value = "overflow: visible; height: auto; min-height: 100vh; margin: 0; padding: 0;";
            BodyId.Value = "MdBody";
            ConnectionId.Value = connectionId;
            body.Attributes.Append(BodyId);
            body.Attributes.Append(ConnectionId);
            body.Attributes.Append(bodyStyle);
            html.AppendChild(body);


            head.InnerXml = $@"
            <link rel=""stylesheet"" href=""/common.css"" />            
            <script src=""/common.js""></script>";

            body.InnerXml += resultToParse;
        }

       

        private string AddButtonOnLowerBar(string functionJs, string image, string Id, string cssClass = "mdeLowerBarButton")
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

        private string AddButtonTextOnVerticalBar(string functionJs, string text, string Id)
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
            body.AppendChild(a);
            return body.OuterXml;
        }

    }


}
