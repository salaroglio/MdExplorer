using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Models;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PlantUml.Net;

namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Questa clase comando, trasforma il testo contenuto dentro il markdown che si riferisce
    /// al codice plantuml, 
    /// Devo fare la gestione della session, in particolare la dispose, perché altrimenti non mi cancello la sessione
    /// </summary>
    public class FromPlantumlToPng : ICommand, IDisposable
    {
        private readonly string _serverAddress;
        private readonly ILogger<FromPlantumlToPng> _logger;
        private readonly ISession _session;

        public FromPlantumlToPng(string ServerAddress, ILogger<FromPlantumlToPng> logger, ISession session)
        {
            _serverAddress = ServerAddress;
            _logger = logger;
            _session = session;
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"```plantuml([^```]*)```",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public virtual string TransformAfterConversion(string text, RequestInfo requestInfo)
        {
            // Nothing to do
            return text;
        }

        /// <summary>
        /// La funzione chiama il server plantuml
        /// che gli restituisce una immagine png, 
        /// fa l'hash, lo salva come png dentro una directory .Md, 
        /// e mette riferimento in markdown al png
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {            
            var directoryInfo = Directory.CreateDirectory(".md");
            var level = requestInfo.CurrentQueryRequest.Split("\\").Count()-2;

            var backPath = string.Empty;
            for (int i = 0; i<level;i++)
            {
                if (i == 0)
                {
                    backPath += "..";
                }
                else
                {
                    backPath += "\\..";
                }
                
            }

            var matches = GetMatches(markdown);
            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;
                var taskSvg = GetSVGFromJar(text);
                taskSvg.Wait();
                var res = taskSvg.Result;
                var filePath = $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{text.GetHashCode()}.png";
                var markdownFilePath = $"{backPath}\\.md\\{text.GetHashCode()}.png";
                _logger.LogInformation($"preparing temporary file: {filePath}");
                if (!File.Exists(filePath))
                    File.WriteAllBytes(filePath, res);
                var referenceUrl =  $@"![]({markdownFilePath.Replace("\\", "/")})";
                markdown = markdown.Replace(item.Groups[0].Value, referenceUrl);
                //File.WriteAllText(filePath + "test.md", markdownTest);
            }
            return markdown;
        }

        private async Task<byte[]> GetSVGFromJar(string plantumlcode)
        {
            var factory = new RendererFactory();
            var settingDal = _session.GetDal<Setting>();
            var plantumlSetting = settingDal.GetList().Where(_ => _.Name == "PlantumlLocalPath").FirstOrDefault()?.ValueString;
            var renderer = factory.CreateRenderer(new PlantUmlSettings() {
                JavaPath = @"C:\Program Files\Java\jre1.8.0_291\bin\javaw.exe",
                //LocalGraphvizDotPath = @"E:\Sviluppo\MdExplorer\InstallBinaries\Graphviz\bin\dot.exe",
                RenderingMode = RenderingMode.Local,
                LocalPlantUmlPath= plantumlSetting,//@"E:\Sviluppo\MdExplorer\InstallBinaries\plantuml.jar"
            });

            var bytes = await renderer.RenderAsync(plantumlcode, OutputFormat.Png);
            return bytes;
        }

        private async Task<byte[]> GetSVG(string plantumlCode)
        {
            var comment = plantumlCode;

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("text", comment),
            });

            var myHttpClient = new HttpClient();

            var settingDal = _session.GetDal<Setting>();
            var plantumlServer = settingDal.GetList().Where(_ => _.Name == "PlantumlServer").FirstOrDefault()?.ValueString;

            var response = await myHttpClient.PostAsync($"http://{plantumlServer}:8080/form", formContent);//_options.Value.PlantumlServer
            var content = await response.Content.ReadAsStringAsync();
            HtmlDocument mydoc = new HtmlDocument();
            mydoc.LoadHtml(content);
            var url = mydoc.DocumentNode.SelectSingleNode(@"//input[@name='url']").Attributes["value"].Value;
            //var urls = mydoc.DocumentNode.SelectNodes(@"//a");
            //url = urls[0].Attributes["href"].Value;

            response = await myHttpClient.GetAsync(url);
            content = await response.Content.ReadAsStringAsync();            
            
            return await response.Content.ReadAsByteArrayAsync();

        }
    }
}
