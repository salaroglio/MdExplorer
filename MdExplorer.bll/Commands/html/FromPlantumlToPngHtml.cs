using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
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

namespace MdExplorer.Features.Commands.html
{
    /// <summary>
    /// This is an empty class, it is used only to give the ICommandHtml intervace
    /// suitable for dependency injection nothing more
    /// keep as referene the inherited class FromPlantumlToPng
    /// </summary>
    public class FromPlantumlToPngHtml : FromPlantumlToPng, ICommandHtml, IPresentationPlantuml
    {
        public FromPlantumlToPngHtml(string ServerAddress, ILogger<FromPlantumlToPng> logger, IUserSettingsDB session, PlantumlServer plantumlServer, IHelperHtml helper)
            : base(ServerAddress, logger, session, plantumlServer, helper)
        {
        }

        public string GetPng(string markdown, string hashFile, int step, RequestInfo requestInfo)
        {
            var directoryInfo = Directory.CreateDirectory(requestInfo.CurrentRoot + $"{Path.DirectorySeparatorChar}.md");

            var matches = GetMatches(markdown);
            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;
                var textHash = _helper.GetHashString(text, Encoding.UTF8);
                if (textHash == hashFile)
                {
                    var filePathPng = $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{hashFile}.png";
                    if (!File.Exists(filePathPng))
                    {
                        var taskSvg = _plantumlServer.GetPngFromJar(text);
                        taskSvg.Wait();
                        var res = taskSvg.Result;
                        File.WriteAllBytes(filePathPng, res);
                        _logger.LogInformation($"write file: {filePathPng}");
                    }
                }
            }
            return $"{hashFile}.png";
        }

        public (string,int) GetPresentationSvg(string markdown, string hashFile, int step, RequestInfo requestInfo)
        {
            var hashPresentationToReturn = string.Empty;
            var directoryInfo = Directory.CreateDirectory(requestInfo.CurrentRoot + $"{Path.DirectorySeparatorChar}.md");
            var matchesPlantuml = GetMatches(markdown);
            var totalStep = 0;
            //string presentationPlantumlString = string.Empty;
            foreach (Match item in matchesPlantuml)
            {
                var textOfOneOfPossiblePlantumlImages = item.Groups[1].Value;
                var textHash = _helper.GetHashString(textOfOneOfPossiblePlantumlImages, Encoding.UTF8);
                if (textHash == hashFile) // Working on the correct selected image
                {
                    (var matchesMd, var matchesStep) = GetMatchesStep(textOfOneOfPossiblePlantumlImages);
                    var firstPartPlantuml = textOfOneOfPossiblePlantumlImages.Substring(0, matchesMd[0].Groups[0].Index);
                    var presentationPlantuml = matchesMd[0].Groups[0].Value;

                    var counter = 0;
                    totalStep = matchesStep.Count();
                    foreach (Match itemStep in matchesStep)
                    {
                        counter++;
                        if (counter == step + 1)
                        {
                            presentationPlantuml = presentationPlantuml.Substring(0, itemStep.Index);
                            // è il momento di prendere ed uscire
                            break; // exit from cicle of steps
                        }
                    }
                    var completedPlantuml = firstPartPlantuml + presentationPlantuml + "\r\n@enduml";
                    var textHashPresentation = _helper.GetHashString(completedPlantuml, Encoding.UTF8);
                    hashPresentationToReturn = textHashPresentation;
                    var filePathPng = $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{textHashPresentation}.svg";
                    if (!File.Exists(filePathPng))
                    {
                        var taskSvg = _plantumlServer.GetSvgFromJar(completedPlantuml);
                        taskSvg.Wait();
                        var res = taskSvg.Result;
                        File.WriteAllBytes(filePathPng, res);
                        XmlDocument doc = new XmlDocument();
                        doc.Load(filePathPng);
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
                        var toReplace = nodeToParse.OuterXml;
                        File.WriteAllText(filePathPng, toReplace);
                        _logger.LogInformation($"write file: {filePathPng}");
                    }
                    break;// exit from cicle of markdown
                }
            }

            return ($".md/{hashPresentationToReturn}.svg?time={DateTime.Now.Ticks}",totalStep);
        }

        private (MatchCollection, MatchCollection) GetMatchesStep(string plantuml)
        {
            Regex rx = new Regex(@"'MdExplorerAnimatedSVG(.*)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matchesMd = rx.Matches(plantuml);
            MatchCollection matchesStep = null;
            if (matchesMd.Count() > 0)
            {
                var search = @"'Step[1-9]+";
                Regex rx1 = new Regex(search, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
                matchesStep = rx1.Matches(matchesMd[0].Groups[0].Value);
            }
            return (matchesMd, matchesStep);
        }
    }
}
