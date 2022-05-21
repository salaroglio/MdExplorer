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

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("/api/MdExplorer/{*url}")]
    public class MdExplorerController : MdControllerBase<MdExplorerController>//ControllerBase
    {
        private readonly IGoodMdRule<FileInfoNode>[] _goodRules;
        private readonly IHelper _helper;

        public MdExplorerController(ILogger<MdExplorerController> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB session,
            IEngineDB engineDB,
            ICommandRunnerHtml commandRunner,
            IGoodMdRule<FileInfoNode>[] GoodRules,
            IHelper helper
            ) : base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner)
        {
            _goodRules = GoodRules;
            _helper = helper;
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
            string relativePathFileSystem = GetRelativePathFileSystem("mdexplorer");
            var relativePathExtension = Path.GetExtension(relativePathFileSystem);


            if (relativePathExtension != "" && relativePathExtension != ".md")
            {
                var filePathSystem = string.Concat(rootPathSystem, relativePathFileSystem);
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

            var filePathSystem1 = string.Empty;
            if (relativePathExtension == ".md")
            {
                filePathSystem1 = string.Concat(rootPathSystem, relativePathFileSystem);
            }
            else
            {
                filePathSystem1 = string.Concat(rootPathSystem, relativePathFileSystem, ".md");
            }

            var monitoredMd = new MonitoredMDModel
            {
                Path = filePathSystem1,
                Name = Path.GetFileName(filePathSystem1),                
                RelativePath = filePathSystem1.Replace(_fileSystemWatcher.Path, string.Empty),
                FullPath = filePathSystem1
        };

            var readText = string.Empty;
            using (var fs = new FileStream(filePathSystem1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                readText = sr.ReadToEnd();
            }
            var requestInfo = new RequestInfo()
            {
                CurrentQueryRequest = relativePathFileSystem,
                CurrentRoot = _fileSystemWatcher.Path,
                AbsolutePathFile = filePathSystem1,
                RootQueryRequest = relativePathFileSystem,
            };

            readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);

            var goodMdRuleFileNameShouldBeSameAsTitle = 
                    _goodRules.First(_ => _.GetType() == 
                        typeof(GoodMdRuleFileNameShouldBeSameAsTitle));

            var fileNode = new FileInfoNode { 
                FullPath = filePathSystem1,
                Name = Path.GetFileName(filePathSystem1),                
                DataText = readText
            };
            //bool isBroken;
            //string theNameShouldBe;
            (var isBroken, var  theNameShouldBe) = goodMdRuleFileNameShouldBeSameAsTitle.ItBreakTheRule(fileNode);
            if (isBroken)
            {
                monitoredMd.Message = "It breaks Rule # 1";
                monitoredMd.Action = "Rename the File!";
                monitoredMd.FromFileName  = Path.GetFileName(filePathSystem1);
                monitoredMd.ToFileName = theNameShouldBe;
                monitoredMd.FullPath = Path.GetDirectoryName(filePathSystem1);
                await _hubContext.Clients.All.SendAsync("markdownbreakrule1", monitoredMd);
            }

            var settingDal = _session.GetDal<Setting>();
            var jiraUrl = settingDal.GetList().Where(_ => _.Name == "JiraServer").FirstOrDefault()?.ValueString;

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()

                .UsePipeTables()
                .UseBootstrap()
                .UseJiraLinks(new JiraLinkOptions(jiraUrl)) //@"https://jira.swarco.com"                
                .UseEmojiAndSmiley()
                .UseYamlFrontMatter()
                .UseGenericAttributes()
                .Build();

            var result = Markdown.ToHtml(readText, pipeline);
            Directory.SetCurrentDirectory(_fileSystemWatcher.Path);

            var textHash = _helper.GetHashString(readText, Encoding.UTF8);
            var cacheName = Path.GetFileName(filePathSystem1) + textHash + ".html";

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

            var docSettingDal = _session.GetDal<DocumentSetting>();
            var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == filePathSystem1).FirstOrDefault();

            var styleForToc = currentDocSetting?.ShowTOC ?? true ? @"class=""col-3""" : @"style=""display:none""";
            var classForMain = currentDocSetting?.ShowTOC ?? true ? @"class=""col-9""" : @"class=""col-12""";

            var button1 = AddButtonOnTopPage("toggleMdCanvas()", "/assets/draw.png");
            var button2 = AddButtonOnTopPage($"toggleTOC('{HttpUtility.UrlEncode(filePathSystem1)}')", "/assets/TOC.png");

            var resultToParse = $@"
                    <div class=""container-fluid"">
                        <div class=""row"">                            
                            <div id=""page"" {classForMain}>
                        <div class=""container "">
                            <div class=""row"">
                                <div  class=""col-1"">
                                    <div class=""sticky-top"">
                                    {button1}
                                    {button2}
                                    </div>
                                </div>
                                <div class=""col-11 md-tocbot-content js-toc-content"">
                    {result}
                                </div>
                            </div>
                        </div>
                            </div>  
                            <nav id=""TOC"" {styleForToc} >
                                <div class=""sticky-top"">
                                    <div class=""toc js-toc is-position-fixed""></div>                                    
                                </div>
                            </nav>
                        </div>
                    </div>
                    
                    ";
            XmlDocument doc1 = new XmlDocument();
            CreateHTMLBody(resultToParse, doc1, filePathSystem1);

            var elementsA = doc1.FirstChild.SelectNodes("//a");
            foreach (XmlNode itemElement in elementsA)
            {
                var href = itemElement.Attributes["href"];
                if (href!= null && href.Value.Length > 8)
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
            //.Replace(@"\",@"\\");
            await _hubContext.Clients.All.SendAsync("markdownfileisprocessed", monitoredMd);

            try
            {
                System.IO.File.WriteAllText(rootPathSystem + Path.DirectorySeparatorChar + ".md" +
                                        Path.DirectorySeparatorChar + cacheName, doc1.InnerXml, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                
            }
            

            var toReturn = new ContentResult
            {
                ContentType = "text/html; charset=utf-8",
                Content = doc1.InnerXml,

            };
            return toReturn;
        }

        private static void CreateHTMLBody(string resultToParse, XmlDocument doc1, string filePathSystem1)
        {
            var html = doc1.CreateElement("html");

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
            html.AppendChild(body);

            head.InnerXml = $@"
            <link rel=""stylesheet"" href=""/common.css"" />            
            <script src=""/common.js""></script>";

            body.InnerXml += resultToParse;
        }

        private string AddButtonOnTopPage(string functionJs, string image)
        {
            var doc1 = new XmlDocument();
            var body = doc1.CreateElement("div");

            var a = doc1.CreateElement("a");
            var aAtt = doc1.CreateAttribute("onClick");
            var att2 = doc1.CreateAttribute("style");
            att2.Value = "cursor: pointer";
            a.Attributes.Append(aAtt);
            a.Attributes.Append(att2);
            aAtt.Value = functionJs;
            var imgEl = doc1.CreateElement("img");
            a.AppendChild(imgEl);
            var srcImg = doc1.CreateAttribute("src");
            srcImg.Value = image;
            imgEl.Attributes.Append(srcImg);
            body.AppendChild(a);
            return body.OuterXml;
        }

    }


}
