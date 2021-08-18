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
using MdExplorer.Abstractions.DB;

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
            var relDal = _engineDB.GetDal<MarkdownFile>();
            var list = relDal.GetList().ToList();


            var rootPathSystem = $"{_fileSystemWatcher.Path}{Path.DirectorySeparatorChar}";
            string relativePathFileSystem = GetRelativePathFileSystem("mdexplorer");
            var relativePathExtension = Path.GetExtension(relativePathFileSystem);


            if (relativePathExtension != "" && relativePathExtension != ".md")
            {
                var filePathSystem = string.Concat(rootPathSystem, relativePathFileSystem);
                var data = System.IO.File.ReadAllBytes(filePathSystem);
                var currentContetType = $"image/{relativePathExtension.Replace(".",string.Empty)}";
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
                    <div class=""container"">
                        <div class=""row"">
                            <div class=""col-3"">
                                <div class=""sticky-top"">
                                {CreateToc(resultToToc)} 
                                </div>
                            </div>
                            <div class=""col-9"">
                    {result}
                            </div>                            
                        </div>
                    </div>
                    
                    ";
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

            //var elements = doc1.FirstChild.SelectNodes(@"//pre/code[@class='language-plantuml']");


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
            toReturn += "<ul class=\"list-group\">";
            var lastLevel = 1;
            var distanceToCloseH = 0;
            foreach (XmlNode h in hList)
            {
                var currentLevel = Convert.ToInt32(h.Name.Substring(1));
                if (currentLevel >lastLevel )
                {
                    toReturn += "<ul>";
                    toReturn += "<li>";
                    toReturn += $"<a href=\"#{h.Attributes["id"].Value}\">{h.InnerText}</a>\r\n";
                    toReturn += "</li>";
                    distanceToCloseH++;
                }
                else if(lastLevel == currentLevel)
                {
                    toReturn += "<li>";
                    toReturn += $"<a href=\"#{h.Attributes["id"].Value}\">{h.InnerText}</a>\r\n";
                    toReturn += "</li>";
                }
                else if (currentLevel < lastLevel)
                {
                    for (int i = 0; i < lastLevel-currentLevel; i++)
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

        private static void CreateHTMLBody(string resultToParse, XmlDocument doc1)
        {
            var html = doc1.CreateElement("html");
            
            doc1.AppendChild(html);
            var head = doc1.CreateElement("head");
           
            

            html.AppendChild(head);
            //AddLink(doc1, head);
            var body = doc1.CreateElement("body");
            html.AppendChild(body);

            var script = doc1.CreateElement("script");
            script.InnerText = @"
            
            function toggleMdCanvas(){
                if (window.toggleCanvas == 'undefined')
                {
                    window.toggleCanvas = false;
                }
                
                if(window.toggleCanvas){
                    window.canvas.remove();
                    window.toggleCanvas = !window.toggleCanvas;
                   return;
                }
                window.toggleCanvas = !window.toggleCanvas;
                window.canvas = document.createElement('canvas');
                window.canvas.setAttribute('id','writeCanvas');
                document.body.appendChild(canvas);

                // some hotfixes... ( ≖_≖)
                //document.body.style.margin = 0;
                canvas.style.position = 'absolute';           
                canvas.style.top = 40;
                canvas.style.left = 0;

                // get canvas 2D context and set him correct size
                window.ctx = canvas.getContext('2d');
                resize();

                // last known position
                window.shiftY = -40;
                window.pos = { x: 0, y: 0 };
                window.scrollPos = { x: 0, y: window.shiftY};

                window.addEventListener('resize', resize);
                document.addEventListener('mousemove', draw);
                document.addEventListener('mousedown', setPosition);
                document.addEventListener('mouseenter', setPosition);
                document.addEventListener('scroll', scrollPosition);
            }            
            function scrollPosition(e){                
                scrollPos.x = window.scrollX;
                scrollPos.y = window.scrollY + window.shiftY;                
            }
            // new position from mouse event
            function setPosition(e) {
              pos.x = scrollPos.x + e.clientX;
              pos.y = scrollPos.y + e.clientY;
            }

            // resize canvas
            function resize() {
              ctx.canvas.width = window.innerWidth;               
              ctx.canvas.height = document.documentElement.scrollHeight;
            }

            function draw(e) {
              // mouse left button must be pressed
              if (e.buttons !== 1) return;

              ctx.beginPath(); // begin

              ctx.lineWidth = 5;
              ctx.lineCap = 'round';
              ctx.strokeStyle = '#2bc02d';

              ctx.moveTo(pos.x, pos.y); // from
              setPosition(e);
              ctx.lineTo(pos.x, pos.y); // to

              ctx.stroke(); // draw it!
            }
            //toggleMdCanvas();
            ";
            //head.AppendChild(script);

            
            head.AppendChild(script);

            var bootStrap = doc1.CreateElement("link");
            head.AppendChild(bootStrap);
            var bootAttRel = doc1.CreateAttribute("rel");
            bootAttRel.Value = "stylesheet";
            bootStrap.Attributes.Append(bootAttRel);
            var bootAttHref = doc1.CreateAttribute("href");
            bootAttHref.Value = @"/bootstrap/css/bootstrap.css";
            bootStrap.Attributes.Append(bootAttHref);

            // jquery 3.6.0
            var jqueryScript = doc1.CreateElement("script");
            var jqueryScriptsrc = doc1.CreateAttribute("src");
            jqueryScriptsrc.Value = @"/bootstrap/jquery-3.6.0.js";
            jqueryScript.Attributes.Append(jqueryScriptsrc);

            // bootstrap js
            var bootScript = doc1.CreateElement("script");
            var bootScriptsrc = doc1.CreateAttribute("src");
            bootScriptsrc.Value = @"/bootstrap/js/bootstrap.bundle.js";
            bootScript.Attributes.Append(bootScriptsrc);





            var a = doc1.CreateElement("a");
            var aAtt = doc1.CreateAttribute("onClick");
            var aAtt1 = doc1.CreateAttribute("href");
            aAtt1.Value = "#";
            a.Attributes.Append(aAtt1);
            a.Attributes.Append(aAtt);
            aAtt.Value = "toggleMdCanvas()";
            var imgEl = doc1.CreateElement("img");
            a.AppendChild(imgEl);
            var srcImg = doc1.CreateAttribute("src");
            srcImg.Value = "/assets/draw.png";
            imgEl.Attributes.Append(srcImg);
            body.AppendChild(a);
            body.InnerXml += resultToParse;
        }

       
    }


}
