using Ad.Tools.Dal.Extensions;
using Markdig;
using Markdig.Extensions.JiraLinks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Controllers;
using MdExplorer.Features.Commands;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace MdExplorer.Service.Controllers
{

    [ApiController]
    [Route("/api/mdexplorer2/{*url}")]
    public class MdExplorer2Controller : MdControllerBase<MdExplorer2Controller>
    {
        public MdExplorer2Controller(ILogger<MdExplorer2Controller> logger, 
            FileSystemWatcher fileSystemWatcher, 
            IOptions<MdExplorerAppSettings> options, 
            IHubContext<MonitorMDHub> hubContext, 
            IUserSettingsDB session, 
            IEngineDB engineDB,
            ICommandRunnerHtml commandRunner) : 
            base(logger, fileSystemWatcher, options, hubContext, session, engineDB, commandRunner)
        {
        }

        [HttpGet]
        public IActionResult GetAsync()
        {
            var rootPathSystem = $"{_fileSystemWatcher.Path}{Path.DirectorySeparatorChar}";
            string relativePathFileSystem = GetRelativePathFileSystem("mdexplorer2");
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


            StringWriter tw = new StringWriter();
            var markDownDocument = Markdown.ToHtml(readText, tw, pipeline);

            // <a onClick=""toggleMdCanvas()"" href=""#""><img src=""/assets/draw.png"" /></a>

            var resultToToc = $@"<div>{result}</div>";


            var resultToParse = $@"
                    <div>
                    {result}
                    </div>
                    ";
            XmlDocument doc1 = new XmlDocument();
            //CreateHTMLBody(resultToParse, doc1);

            doc1.LoadXml(resultToParse);
            var elementsA = doc1.FirstChild.SelectNodes("//a");
            foreach (XmlNode itemElement in elementsA)
            {
                var href = itemElement.Attributes["href"];
                if(Regex.Match(href.Value, "http[s]?://(?!localhost)").Success)
                {
                    var htmltarget = doc1.CreateAttribute("target");
                    htmltarget.InnerText = "_target";
                    itemElement.Attributes.Append(htmltarget);
                }
                var htmlClass = doc1.CreateAttribute("class");
                htmlClass.InnerText = "mdExplorerLink";
                itemElement.Attributes.Append(htmlClass);
            }           

            return new ContentResult
            {
                ContentType = "text/html",
                Content = doc1.InnerXml
            };
        }
    }
}
