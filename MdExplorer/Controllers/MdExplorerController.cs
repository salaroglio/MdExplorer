using HtmlAgilityPack;
using Markdig;
using Markdig.Extensions.JiraLinks;
using Markdig.Renderers;
using MdExplorer.Abstractions.Models;
using MdExplorer.Hubs;
using MdExplorer.Implementations.Features;
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
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

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

        public MdExplorerController(ILogger<MdExplorerController> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext
            )
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            this._options = options;
            _hubContext = hubContext;
        }

        

        /// <summary>
        /// Good start for keeping html using angualar
        /// </summary>
        /// <param name="mdFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetPageAsync(FileInfoNode mdFile)
        {
            var filePath = _fileSystemWatcher.Path;
            filePath = filePath + mdFile.Path;

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UsePipeTables().UseBootstrap().Build();

            var markDownFeature = new MarkDownFeature(pipeline);
            string html = await markDownFeature.GetHtmlAsync(filePath);

            XmlDocument doc1 = new XmlDocument();
            doc1.LoadXml(html);
            var elements = doc1.FirstChild.SelectNodes("//a");
            foreach (XmlNode itemElement in elements)
            {
                var htmlClass = doc1.CreateAttribute("(click)");
                htmlClass.InnerText = "gettAlert()";
                itemElement.Attributes.Append(htmlClass);
            }

            html = doc1.InnerXml;

            return new ContentResult
            {
                ContentType = "text/html",
                Content = html,
            };
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
            var relativePath = HttpUtility.UrlDecode(Request.Path.ToString().Replace("api/mdexplorer", string.Empty).Replace("/", @"\"));
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
                Name = Path.GetFileName(filePath)
            };
            await _hubContext.Clients.All.SendAsync("markdownfileisprocessed", monitoredMd);

            string readText = System.IO.File.ReadAllText(filePath);


            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UsePipeTables()
                .UseBootstrap()
                .UseJiraLinks(new JiraLinkOptions(@"https://jira.swarco.com"))
                .UseEmojiAndSmiley()
                .Build();

            var result = Markdown.ToHtml(readText, pipeline);
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

            foreach (XmlNode item in elements)
            {

                XmlDocument doc2 = new XmlDocument();
                doc2.LoadXml(await GetSVG(item.InnerText));
                var importedNode = doc1.ImportNode(doc2.ChildNodes[1], true);

                item.ParentNode.AppendChild(importedNode);
                item.ParentNode.RemoveChild(item);
            }           

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
            body.InnerXml = resultToParse;
        }

        private static void AddLink(XmlDocument doc1, XmlElement head)
        {
            var link = doc1.CreateElement("link");
            head.AppendChild(link);
            //<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/katex@0.13.11/dist/katex.css" integrity="sha384-knaESGLxlQRSHWSJ+ZbTX6/L1bJZWBsBYGb2O+g64XHFuO7CbIj9Pkf1aaVXzIZJ" crossorigin="anonymous">
            var rel = doc1.CreateAttribute("rel");
            link.Attributes.Append(rel);
            rel.InnerText = @"stylesheet";
            var href = doc1.CreateAttribute("href");
            href.InnerText = "https://cdn.jsdelivr.net/npm/katex@0.13.11/dist/katex.css";
            link.Attributes.Append(href);
            var integrity = doc1.CreateAttribute("integrity");
            link.Attributes.Append(integrity);
            integrity.InnerText = @"sha384-knaESGLxlQRSHWSJ+ZbTX6/L1bJZWBsBYGb2O+g64XHFuO7CbIj9Pkf1aaVXzIZJ";
            var crossOrigin = doc1.CreateAttribute("crossorigin");
            link.Attributes.Append(crossOrigin);
            crossOrigin.InnerText = "anonymous";
        }

        private async Task<string> GetSVG(string plantumlCode)
        {
            var comment = plantumlCode;

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", comment),
            });

            var myHttpClient = new HttpClient();
            var response = await myHttpClient.PostAsync($"http://{_options.Value.PlantumlServer}:8080/form", formContent);//
            var content = await response.Content.ReadAsStringAsync();
            HtmlDocument mydoc = new HtmlDocument();
            mydoc.LoadHtml(content);
            var url = mydoc.DocumentNode.SelectSingleNode(@"//input[@name='url']").Attributes["value"].Value;
            var urls = mydoc.DocumentNode.SelectNodes(@"//a");
            url = urls[1].Attributes["href"].Value;

            response = await myHttpClient.GetAsync(url);
            content = await response.Content.ReadAsStringAsync();

            return content;

        }
    }


}
