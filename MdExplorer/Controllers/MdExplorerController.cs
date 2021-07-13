using Ad.Tools.Dal.Abstractions.Interfaces;
using HtmlAgilityPack;
using Markdig;
using Markdig.Extensions.JiraLinks;
using Markdig.Renderers;
using MdExplorer.Abstractions.Models;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Features.Commands;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("/api/MdExplorer/{*url}")]
    public class MdExplorerController : ControllerBase
    {
        private readonly ILogger<MdExplorerController> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IOptions<MdExplorerAppSettings> _options;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ISession _session;
        private readonly ICommandRunner _commandRunner;

        public MdExplorerController(ILogger<MdExplorerController> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            ISession session,
            ICommandRunnerHtml commandRunner
            )
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            this._options = options;
            _hubContext = hubContext;
            _session = session;
            _commandRunner = commandRunner;
        }



        /// <summary>
        /// Get all goodies available in html
        /// It's good to get images for example
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            
            var filePath = _fileSystemWatcher.Path;
            var relativePath = HttpUtility.UrlDecode(Request.Path.ToString().Replace("api/mdexplorer//", string.Empty).Replace("/", @"\"));
            relativePath = HttpUtility.UrlDecode(Request.Path.ToString().Replace("api/mdexplorer/", string.Empty).Replace("/", @"\"));
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

            var monitoredMd = new MonitoredMDModel
            {
                Path = filePath,
                Name = Path.GetFileName(filePath),
                RelativePath = filePath.Replace(_fileSystemWatcher.Path, string.Empty)
            };
            
            var readText = string.Empty;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                readText = sr.ReadToEnd();
            }
            var requestInfo = new RequestInfo()
            {
                CurrentQueryRequest = relativePath,
                CurrentRoot = _fileSystemWatcher.Path
            };

            Directory.SetCurrentDirectory(Path.GetDirectoryName(filePath));

            readText  = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);

            Directory.SetCurrentDirectory(_fileSystemWatcher.Path);

            var settingDal = _session.GetDal<Setting>();
            var jiraUrl = settingDal.GetList().Where(_ => _.Name == "JiraServer").FirstOrDefault()?.ValueString;

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePipeTables()
                .UseBootstrap()
                .UseJiraLinks(new JiraLinkOptions(jiraUrl)) //@"https://jira.swarco.com"
                .UseEmojiAndSmiley()
                .Build();

            var result = Markdown.ToHtml(readText, pipeline);
            result = _commandRunner.TransformAfterConversion(result, requestInfo);
            StringWriter tw = new StringWriter();
            var markDownDocument = Markdown.ToHtml(readText, tw, pipeline);


            var resultToParse = "<MainHTML>" + result + "</MainHTML>";
            XmlDocument doc1 = new XmlDocument();
            CreateHTMLBody(resultToParse, doc1);

            //doc1.LoadXml(resultToParse);
            var elementsA = doc1.FirstChild.SelectNodes("//a");
            foreach (XmlNode itemElement in elementsA)
            {
                var htmlClass = doc1.CreateAttribute("class");
                htmlClass.InnerText = "mdExplorerLink";
                itemElement.Attributes.Append(htmlClass);
            }

            var elements = doc1.FirstChild.SelectNodes(@"//pre/code[@class='language-plantuml']");

            //foreach (XmlNode item in elements)
            //{

            //    XmlDocument doc2 = new XmlDocument();
            //    doc2.LoadXml(await GetSVG(item.InnerText));
            //    var importedNode = doc1.ImportNode(doc2.ChildNodes[1], true);

            //    item.ParentNode.AppendChild(importedNode);
            //    item.ParentNode.RemoveChild(item);
            //}
            await _hubContext.Clients.All.SendAsync("markdownfileisprocessed", monitoredMd);

            return new ContentResult
            {
                ContentType = "text/html",
                Content = doc1.InnerXml
            };
        }

        private static void CreateHTMLBody(string resultToParse, XmlDocument doc1)
        {
            var html = doc1.CreateElement("html");
            doc1.AppendChild(html);
            var head = doc1.CreateElement("head");
            html.AppendChild(head);
            //AddLink(doc1, head);
            var body = doc1.CreateElement("body");
            html.AppendChild(body);
            //resultToParse = resultToParse.Replace("--&gt;</g>", "--&gt</g>")
            body.InnerXml = resultToParse;
        }

       
    }


}
