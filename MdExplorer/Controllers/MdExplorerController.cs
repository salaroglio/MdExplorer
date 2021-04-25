using HtmlAgilityPack;
using Markdig;
using Markdig.Renderers;
using MdExplorer.Abstractions.Models;
using MdExplorer.Implementations.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("/api/MdExplorer/{*url}")]
    public class MdExplorerController : ControllerBase
    {
        private readonly ILogger<MdExplorerController> _logger;
        private readonly FileSystemWatcher _fileSystemWatcher;

        public MdExplorerController(ILogger<MdExplorerController> logger, FileSystemWatcher fileSystemWatcher)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
        }


        /// <summary>
        /// Good start for keeping html using angualar
        /// </summary>
        /// <param name="mdFile"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetPageAsync(MdFile mdFile)
        {
            var filePath = _fileSystemWatcher.Path;
            filePath = filePath + mdFile.Path;

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UsePipeTables().UseBootstrap().Build();

            var markDownFeature = new MarkDownFeature(pipeline);
            string html = await markDownFeature.GetHtmlAsync(filePath);                         

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
            var relativePathExtension = string.Empty;
            var relativePath = string.Empty;
            
            relativePath = Request.Path.ToString().Replace("api/mdexplorer", string.Empty).Replace("/", @"\");
            
            relativePathExtension = Path.GetExtension(relativePath);

            if (relativePathExtension != "" && relativePathExtension != ".md")
            {
                filePath = string.Concat(filePath, relativePath);
                var data = System.IO.File.ReadAllBytes(filePath);
                var test = new FileContentResult(data, "image/" + relativePathExtension);
                return test;
            }

            
            if (relativePathExtension == ".md" )
            {
                filePath = string.Concat(filePath, relativePath);
            }
            else
            {
                filePath = string.Concat(filePath, relativePath, ".md");
            }

                       

            string readText = System.IO.File.ReadAllText(filePath);

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UsePipeTables().UseBootstrap().Build();

            var result = Markdown.ToHtml(readText, pipeline);
            StringWriter tw = new StringWriter();
            var markDownDocument = Markdown.ToHtml(readText, tw, pipeline);


            var resultToParse = "<MainHTML>" + result + "</MainHTML>";
            XmlDocument doc1 = new XmlDocument();
            doc1.LoadXml(resultToParse);
            var elements = doc1.FirstChild.SelectNodes(@"//pre/code[@class='language-plantuml']");

            foreach (XmlNode item in elements)
            {
                
                XmlDocument doc2 = new XmlDocument();
                doc2.LoadXml(await GetSVG(item.InnerText));
                var importedNode = doc1.ImportNode(doc2.ChildNodes[1],true);

                item.ParentNode.AppendChild(importedNode);
                item.ParentNode.RemoveChild(item);
            }

            var anchors = doc1.FirstChild.SelectNodes(@"//a");

            return new ContentResult
            {
                ContentType = "text/html",
                Content = doc1.InnerXml
            };
        }

        private async Task<string> GetSVG(string plantumlCode)
        {
            var comment = plantumlCode;

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", comment),
            });

            var myHttpClient = new HttpClient();
            var response = await myHttpClient.PostAsync("http://172.25.74.116:8080/form", formContent);//
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
