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
using System.Security.Cryptography;
using MdExplorer.Features.Utilities;

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
        private readonly PlantumlServer _plantumlServer;
        private readonly IHelper _helper;

        public int Priority { get; set; } = 20;
        public string Name { get; set; } = "FromPlantumlToPng";
        public FromPlantumlToPng(string ServerAddress, 
                ILogger<FromPlantumlToPng> logger, 
                ISession session,
                PlantumlServer plantumlServer,
                IHelper helper)
        {
            _serverAddress = ServerAddress;
            _logger = logger;
            _session = session;
            _plantumlServer = plantumlServer;
            _helper = helper;
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


        public MatchCollection GetMatchesAfterConversion(string html)
        {
            Regex rx = new Regex(@"<img src=\""[^md]*\.md/([^\.png]*).png",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(html);
            return matches;
        }

        public virtual string TransformAfterConversion(string html, RequestInfo requestInfo)
        {            
            var matches = GetMatchesAfterConversion(html);
            string backPath = _helper.GetBackPath(requestInfo);

            foreach (Match item in matches)
            {
                var stringMatched0 = item.Groups[1].Value;
                var referenceUrl = $"<img src=\"{backPath.Replace("\\","/")}/{stringMatched0}.png";                
                html = html.Replace(item.Groups[0].Value, referenceUrl);
            }
            
            return html;
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
            var directoryInfo = Directory.CreateDirectory(requestInfo.CurrentRoot + "\\.md");
            string backPath = _helper.GetBackPath(requestInfo);

            var matches = GetMatches(markdown);
            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;
                var textHash = _helper.GetHashString(text, Encoding.UTF8);
                var filePath = $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{textHash}.png"; //text.GetHashCode()
                if (!File.Exists(filePath))
                {
                    var taskSvg = _plantumlServer.GetSVGFromJar(text);
                    taskSvg.Wait();
                    var res = taskSvg.Result;
                    File.WriteAllBytes(filePath, res);
                    _logger.LogInformation($"write file: {filePath}");
                }

                var markdownFilePath = $"{backPath}\\{textHash}.png";
                var referenceUrl = $@"![]({markdownFilePath.Replace("\\", "/")})";
                _logger.LogInformation(referenceUrl);
                markdown = markdown.Replace(item.Groups[0].Value, referenceUrl);
                //File.WriteAllText(filePath + "test.md", markdownTest);
            }
            return markdown;
        }

        

                    

        
    }
}
