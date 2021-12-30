using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;


namespace MdExplorer.Features.Commands
{
    public class FromPlantumlToSvg : ICommand, IDisposable
    {
        protected readonly string _serverAddress;
        protected readonly ILogger<FromPlantumlToSvg> _logger;
        protected readonly IUserSettingsDB _session;
        protected readonly PlantumlServer _plantumlServer;
        protected readonly IHelper _helper;

        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 20;
        public string Name { get; set; } = "FromPlantumlToSvg";
        public FromPlantumlToSvg(string ServerAddress,
                ILogger<FromPlantumlToSvg> logger,
                IUserSettingsDB session,
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
            Regex rx = new Regex(@"<img src=\""[^md]*\.md/([^\.svg]*).svg[^/>]*/>",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(html);
            return matches;
        }

        public virtual string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            var matches = GetMatchesAfterConversion(html);
            string backPath = _helper.GetBackPath(requestInfo);
            foreach (Match itemMatch in matches)
            {

                XmlDocument doc = new XmlDocument();

                var stringMatchedHash = itemMatch.Groups[1].Value;
                //var referenceUrl = $"<img src=\"{backPath.Replace("\\", "/")}/{stringMatched0}.svg";
                var currentSvg = $".md{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}{stringMatchedHash}.svg";
                var currentPng = $".md{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}{stringMatchedHash}.png";
                doc.Load(currentSvg);
                var nodeToParse = doc.LastChild;
                XmlNamespaceManager m = new XmlNamespaceManager(doc.NameTable);
                m.AddNamespace("myns", "http://www.w3.org/2000/svg");

                var nodeList = nodeToParse.SelectNodes("//myns:a", m);
                foreach (XmlNode itemNode in nodeList)
                {
                    for (int i = itemNode.Attributes.Count - 1; i > 0; i--)
                    {
                        XmlAttribute itemAttribute = itemNode.Attributes[i];
                        if (itemAttribute.Name != "href")
                        {
                            itemNode.Attributes.Remove(itemAttribute);
                        }
                    }
                }
                var listOfComment = nodeToParse.SelectNodes("//comment()");
                var filteredCommentThatContainAnimation = listOfComment.Cast<XmlComment>().Where(_ => _.Data.Contains("MdExplorerAnimatedSVG"));
                var stringToPutInside = string.Empty;
                var prepareCurrentQueryRequest = requestInfo.CurrentQueryRequest.Replace(@"\",@"\\");
                
                var toReplace = nodeToParse.OuterXml;
                if (filteredCommentThatContainAnimation.Count() > 0)
                {
                    stringToPutInside = $@"<button id=""forwardArrow{stringMatchedHash}"" data-step=""1"" onclick=""presentationSVG('{prepareCurrentQueryRequest}','{stringMatchedHash}')"" class=""btn btn-md btn-primary-outline""><img src=""/assets/green_right_arrow.png""/></button>";
                }
                var orribleHTMLBefore = $@"<div id=""{stringMatchedHash}"" class=""img-wrapper"">";
                var orribleHTMLAfter = $@"</div>";                
                var buttonsArray = $@"<div><button alt=""copy into clipboard"" onclick = ""copyToClipboard('/api/mdexplorer/{currentPng}', '{prepareCurrentQueryRequest}', '{stringMatchedHash}', 0)"" ><img src = ""/assets/clipboard.png""/></button>{stringToPutInside}</div>";
                toReplace = string.Concat(buttonsArray, toReplace);
                toReplace = string.Concat(orribleHTMLBefore,toReplace, orribleHTMLAfter);


                html = html.Replace(itemMatch.Groups[0].Value, toReplace);
            }
            return html;
        }

        /// <summary>
        /// La funzione chiama il server plantuml
        /// che gli restituisce una immagine SVG, 
        /// fa l'hash, lo salva come png dentro una directory .Md, 
        /// e mette riferimento in markdown al SVG
        /// </summary>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var directoryInfo = Directory.CreateDirectory(requestInfo.CurrentRoot + $"{Path.DirectorySeparatorChar}.md");
            string backPath = _helper.GetBackPath(requestInfo);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(requestInfo.AbsolutePathFile));

            var matches = GetMatches(markdown);
            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;
                var textHash = _helper.GetHashString(text, Encoding.UTF8);
                var filePathSvg = $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{textHash}.svg"; //text.GetHashCode()
                if (!File.Exists(filePathSvg))
                {
                    var taskSvg = _plantumlServer.GetSvgFromJar(text);
                    taskSvg.Wait();
                    var res = taskSvg.Result;
                    File.WriteAllBytes(filePathSvg, res);
                    _logger.LogInformation($"write file: {filePathSvg}");
                }

                var markdownFilePath = $"{backPath}{Path.DirectorySeparatorChar}{textHash}.svg";
                var referenceUrl = $@"![]({markdownFilePath.Replace(Path.DirectorySeparatorChar, '/')})";
                _logger.LogInformation(referenceUrl);
                markdown = markdown.Replace(item.Groups[0].Value, referenceUrl);
            }
            Directory.SetCurrentDirectory(Path.GetDirectoryName(requestInfo.CurrentRoot));
            return markdown;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            // Do nothing
            return markdown;
        }

    }
}
