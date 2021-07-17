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
using MdExplorer.Service.Controllers;
using MdExplorer.Features.Interfaces;

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
            ISession session, 
            ICommandRunnerHtml commandRunner
            ) : base(logger, fileSystemWatcher, options, hubContext, session, commandRunner)
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
                var currentContetType = $"image/{relativePathExtension}";
                if (relativePathExtension == "pdf")
                {
                    currentContetType = $"application/{relativePathExtension}";
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
                CurrentRoot = _fileSystemWatcher.Path
            };

            Directory.SetCurrentDirectory(Path.GetDirectoryName(filePathSystem1));

            readText = _commandRunner.TransformInNewMDFromMD(readText, requestInfo);

            Directory.SetCurrentDirectory(_fileSystemWatcher.Path);

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
