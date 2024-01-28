using Ad.Tools.Dal.Abstractions.Interfaces;
using DocumentFormat.OpenXml.Presentation;
using Markdig;
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
    /// <summary>
    /// It manage SVG generation, and add to links in plantuml the SignalR connectionId
    /// </summary>
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
        { //```plantuml([^```]*)`{3}(?(?={)|)
            Regex rx = new Regex(@"```plantuml([^```]*)`{3}(?:(?={){([^{]*)}|)",
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

        public Match GetMdPlantuml(string currentTagImg)
        {
            Regex rx = new Regex(@"md-plantuml=\""([^\""]*)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(currentTagImg).FirstOrDefault();
            return matches;
        }

        public GroupCollection GetWidthGroup(string value)
        {
            Regex rx = new Regex(@"width:([^;]*);",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(value);
            return matches[0].Groups;
        }
        public GroupCollection GetHeightGroup(string value)
        {
            Regex rx = new Regex(@"height:([^;]*);",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(value);
            return matches[0].Groups;
        }
        private bool IsDynamicPlantuml(string plantuml)
        {
            Regex rx = new Regex(@"'MdExplorerAnimatedSVG",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(plantuml);
            return matches.Count>0;
        }

        protected MatchCollection GetMatchesCSS(string markdown)
        {
            Regex rx = new Regex(@"```CSSInline([^```]*)```",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        protected MatchCollection GetLinks(string html)
        {


            Regex rx = new Regex(@"<a.+?<\/a>", //lnk?
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

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
                var checkIfThereIsCSS = GetMdPlantuml(itemMatch.Groups[0].Value);
                var plantumlHasClass = checkIfThereIsCSS?.Value.Contains("linkHasClass:true;")??false;
                var linkClassHasCSS = checkIfThereIsCSS?.Value.Contains("linkClassHasCSS:true;") ?? false;
                var currentSvg = $".md{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}{stringMatchedHash}.svg";
                
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

                var nodeListLinks = doc.SelectNodes("//myns:a", m);
                foreach (XmlNode nodeLink in nodeListLinks)
                {
                    XmlAttribute itemAttribute = nodeLink.Attributes["href"];
                    if (itemAttribute.Name == "href")
                    {
                        // here we found all Href, and we have to add the SignalR connection id
                        if (itemAttribute.InnerText.Contains(".md"))
                        {
                            var indexOfSharp = itemAttribute.InnerText.IndexOf("#");
                            var firstPart = itemAttribute.InnerText;
                            var secondPart = string.Empty;
                            if (indexOfSharp > -1)
                            {
                                firstPart = itemAttribute.InnerText.Substring(0, indexOfSharp);
                                secondPart = itemAttribute.InnerText.Substring(indexOfSharp + 1);
                            }
                            
                            var stringURI = $@"{firstPart}" + "?connectionId=" + requestInfo.ConnectionId + secondPart;
                            itemAttribute.InnerText = stringURI;
                        }

                    }
                }

                var svgStyleToChange = doc.DocumentElement.Attributes["style"].Value;
                var width = GetWidthGroup(svgStyleToChange)[1].Value;
                var height = GetHeightGroup(svgStyleToChange)[1].Value;
                if (plantumlHasClass && linkClassHasCSS)
                {
                    svgStyleToChange = svgStyleToChange.Replace(width, "100%");
                    svgStyleToChange = svgStyleToChange.Replace(height, "100%");
                    doc.DocumentElement.Attributes["style"].Value = svgStyleToChange;
                }
               
                var prepareCurrentQueryRequest = requestInfo.CurrentQueryRequest.Replace(@"\",@"\\");
                var toReplace = nodeToParse.OuterXml;
                html = html.Replace(itemMatch.Groups[0].Value, toReplace);

                // insert ConnectionId of SignalR


            }
            return html;
        }

        /// <summary>
        /// La funzione chiama il server plantuml
        /// che gli restituisce una immagine SVG, 
        /// fa l'hash, lo salva come png dentro una directory .Md, 
        /// e mette riferimento in markdown al SVG.
        /// In più "comprende" se l'svg dovrà essere manipolato successivamente
        /// Questo dipende dal fatto che l'svg in questione può trovarsi dentro
        /// lo scatolotto che gestisce i menù di resize.
        /// Se non deve essere manipolato, linkHasClass è a false
        /// Altrimenti lo sarà nella funzione 
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
                var referenceUrl = "<code>error</code>";
                try
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

                    var classes = item.Groups[2].Value;
                    var markdownFilePath = $"{backPath}{Path.DirectorySeparatorChar}{textHash}.svg";
                    referenceUrl = $@"![]({markdownFilePath.Replace(Path.DirectorySeparatorChar, '/')})";
                    _logger.LogInformation(referenceUrl);
                    // i should check if there's a CSS to do
                    var linkHasClass = item.Groups[2]?.Value != null && item.Groups[2].Value != string.Empty ? "true" : "false";
                    // devo cercare dentro il CSS e se c'è qualcosa finalmente dire che linkHasClass:true
                    var linkClassHasCSS = LinkClassHasCSS(markdown, item.Groups[2]?.Value) ? "true" : "false";

                    var isDynamicPlantuml = IsDynamicPlantuml(text) ? "true" : "false";

                    referenceUrl = string.Concat(referenceUrl, "{", classes, " ", $"md-plantuml=\"dynamic:{isDynamicPlantuml};copy:true;linkHasClass:{linkHasClass};linkClassHasCSS:{linkClassHasCSS};\"", "}");
                }
                catch (Exception ex)
                {
                    referenceUrl = $"<code>{ex.Message}</code>";


                }
                

                markdown = markdown.Replace(item.Groups[0].Value, referenceUrl);
            }
            Directory.SetCurrentDirectory(Path.GetDirectoryName(requestInfo.CurrentRoot));
            return markdown;
        }

        protected MatchCollection GetMetaDataMatches(string markDown)
        {
            var reg = @"{?([^\s{}]+)}?";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(markDown);
            return matches;
        }


        private bool LinkClassHasCSS(string markdown, string cssClass)
        {
            var toReturn = false;
            var matchesCSS = GetMatchesCSS(markdown);
            if (cssClass == null ) // no class
                return false;
            var metadata = GetMetaDataMatches(cssClass);
            if (metadata.Where(_=>_.Groups[0].Value.Contains(".")).Count()>0 )
            {
                toReturn = true;
            }

            return toReturn;

        }


        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            // Do nothing
            return markdown;
        }

    }
}
