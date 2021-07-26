using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
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

namespace MdExplorer.Features.Commands.pdf
{
    public class FromPlantumlToSvgPdf : FromPlantumlToSvg, ICommandPdf
    {
        public FromPlantumlToSvgPdf(string ServerAddress, ILogger<FromPlantumlToSvg> logger, ISessionDB session, PlantumlServer plantumlServer, IHelper helper)
            : base(ServerAddress, logger, session, plantumlServer, helper)
        {
        }

        public override string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            var matches = GetMatchesAfterConversion(html);
            string backPath = _helper.GetBackPath(requestInfo);

            foreach (Match item in matches)
            {
                var stringMatched0 = item.Groups[1].Value;
                var referenceUrl = $"<img src=\"{backPath.Replace("\\", "/")}/{stringMatched0}.png";
                html = html.Replace(item.Groups[0].Value, referenceUrl);
            }

            return html;
        }

        public override string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var directoryInfo = Directory.CreateDirectory(requestInfo.CurrentRoot + $"{Path.DirectorySeparatorChar}.md");
            string backPath = _helper.GetBackPath(requestInfo);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(requestInfo.AbsolutePathFile));

            var matches = GetMatches(markdown);
            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;
                var textHash = _helper.GetHashString(text, Encoding.UTF8);
                var filePath = $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{textHash}.png"; //text.GetHashCode()
                if (!File.Exists(filePath))
                {
                    var taskSvg = _plantumlServer.GetPngFromJar(text);
                    taskSvg.Wait();
                    var res = taskSvg.Result;
                    File.WriteAllBytes(filePath, res);
                    _logger.LogInformation($"write file: {filePath}");
                }

                var markdownFilePath = $"{backPath}{Path.DirectorySeparatorChar}{textHash}.png";
                var referenceUrl = $@"![]({markdownFilePath.Replace(Path.DirectorySeparatorChar, '/')})";
                _logger.LogInformation(referenceUrl);
                markdown = markdown.Replace(item.Groups[0].Value, referenceUrl);
                //File.WriteAllText(filePath + "test.md", markdown);
            }
            Directory.SetCurrentDirectory(Path.GetDirectoryName(requestInfo.CurrentRoot));
            return markdown;
        }


    }
}
