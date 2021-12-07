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

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("/api/MdExplorer/{*url}")]
    public class MdExplorerController : MdControllerBase<MdExplorerController>//ControllerBase
    {
        public MdExplorerController(ILogger<MdExplorerController> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB session,
            IEngineDB engineDB,
            ICommandRunnerHtml commandRunner
            ) : base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner)
        {
        }

        /// <summary>
        /// Get all goodies available in html
        /// It's good to get images for example
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {            
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
                RelativePath = filePathSystem1.Replace(_fileSystemWatcher.Path, string.Empty)
            };

            var readText = string.Empty;
            using (var fs = new FileStream(filePathSystem1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                readText = sr.ReadToEnd();
            }
            var requestInfo = new RequestInfo()
            {
                CurrentQueryRequest = relativePathFileSystem,
                CurrentRoot = _fileSystemWatcher.Path,
                AbsolutePathFile = filePathSystem1
            };

            readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);

            var settingDal = _session.GetDal<Setting>();
            var jiraUrl = settingDal.GetList().Where(_ => _.Name == "JiraServer").FirstOrDefault()?.ValueString;

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePipeTables()
                .UseBootstrap()
                .UseJiraLinks(new JiraLinkOptions(jiraUrl)) //@"https://jira.swarco.com"                
                .UseEmojiAndSmiley()
                .UseYamlFrontMatter()
                .Build();

            var result = Markdown.ToHtml(readText, pipeline);

            Directory.SetCurrentDirectory(_fileSystemWatcher.Path);

            result = _commandRunner.TransformAfterConversion(result, requestInfo);

            var resultToToc = $@"<div>{result}</div>";

            var docSettingDal = _session.GetDal<DocumentSetting>();
            var currentDocSetting = docSettingDal.GetList().Where(_ => _.DocumentPath == filePathSystem1).FirstOrDefault();

            var styleForToc = currentDocSetting?.ShowTOC ?? true ? @"class=""col-3""" : @"style=""display:none""" ;
            var classForMain = currentDocSetting?.ShowTOC ?? true ? @"class=""col-9""" : @"class=""col-12""";
            var resultToParse = $@"
                    <div class=""container"">
                        <div class=""row"">
                            <div id=""page"" {classForMain}>
                    {result}
                            </div>  
                            <div id=""TOC"" {styleForToc} >
                                <div class=""sticky-top"">
                                <input id=""tocInputFilter"" onkeyup=""filterToc()"" placeholder=""Search""/>
                                {CreateToc(resultToToc)} 
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    ";
            XmlDocument doc1 = new XmlDocument();
            CreateHTMLBody(resultToParse, doc1, filePathSystem1);

            var elementsA = doc1.FirstChild.SelectNodes("//a");
            foreach (XmlNode itemElement in elementsA)
            {
                var htmlClass = doc1.CreateAttribute("class");
                htmlClass.InnerText = "mdExplorerLink";
                itemElement.Attributes.Append(htmlClass);
            }
            //System.IO.File.WriteAllText(@"test.html", doc1.InnerXml);

            await _hubContext.Clients.All.SendAsync("markdownfileisprocessed", monitoredMd);

            return new ContentResult
            {
                ContentType = "text/html",
                Content = doc1.InnerXml
            };
        }

        private string CreateToc(string resultToToc)
        {
            var toReturn = string.Empty;
            var xmlDoc = new XmlDocument();
            var xmlDoc1 = new XmlDocument();
            xmlDoc.LoadXml(resultToToc);
            var hList = xmlDoc.SelectNodes("//*[starts-with(name(), 'h')]");
            toReturn += "<div class=\"bd-toc text-muted\">";
            toReturn += "<nav id=\"TableOfContents\">";
            toReturn += "<ul id=\"ulToc\" class=\"list-group\">";
            var lastLevel = 1;
            var distanceToCloseH = 0;
            foreach (XmlNode h in hList)
            {
                var currentLevel = Convert.ToInt32(h.Name.Substring(1));
                if (currentLevel > lastLevel)
                {
                    toReturn += "<ul>";
                    toReturn += "<li>";
                    toReturn += $"<a href=\"#{h.Attributes["id"].Value}\">{h.InnerText}</a>\r\n";
                    toReturn += "</li>";
                    distanceToCloseH++;
                }
                else if (lastLevel == currentLevel)
                {
                    toReturn += "<li>";
                    toReturn += $"<a href=\"#{h.Attributes["id"].Value}\">{h.InnerText}</a>\r\n";
                    toReturn += "</li>";
                }
                else if (currentLevel < lastLevel)
                {
                    for (int i = 0; i < lastLevel - currentLevel; i++)
                    {
                        toReturn += "</ul>";
                        distanceToCloseH--;
                    }
                    toReturn += "<li>";
                    toReturn += $"<a href=\"#{h.Attributes["id"].Value}\">{h.InnerText}</a>\r\n";
                    toReturn += "</li>";

                }
                lastLevel = currentLevel;
            }
            for (int i = 0; i < distanceToCloseH; i++)
            {
                toReturn += "</ul>";
            }
            toReturn += "</ul>";
            toReturn += "</nav>";
            toReturn += "</div>";
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
            AddButtonOnTopPage(doc1, body, "toggleMdCanvas()", "/assets/draw.png");
            AddButtonOnTopPage(doc1, body, $"toggleTOC('{HttpUtility.UrlEncode(filePathSystem1)}')", "/assets/TOC.png");

            body.InnerXml += resultToParse;
        }

        private static void AddButtonOnTopPage(XmlDocument doc1, XmlElement body,
            string functionJs, string image)
        {
            var a = doc1.CreateElement("a");
            var aAtt = doc1.CreateAttribute("onClick");
            var aAtt1 = doc1.CreateAttribute("href");
            aAtt1.Value = "#";
            a.Attributes.Append(aAtt1);
            a.Attributes.Append(aAtt);
            aAtt.Value = functionJs;
            var imgEl = doc1.CreateElement("img");
            a.AppendChild(imgEl);
            var srcImg = doc1.CreateAttribute("src");
            srcImg.Value = image;
            imgEl.Attributes.Append(srcImg);
            body.AppendChild(a);
        }

    }


}
