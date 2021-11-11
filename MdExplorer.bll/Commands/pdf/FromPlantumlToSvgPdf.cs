using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MdExplorer.Features.Commands.pdf
{
    public class FromPlantumlToSvgPdf : FromPlantumlToSvg, ICommandPdf
    {
        public FromPlantumlToSvgPdf(string ServerAddress, ILogger<FromPlantumlToSvg> logger, IUserSettingsDB session, PlantumlServer plantumlServer, IHelper helper)
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


                (var inchHeight, var inchWidth) = NormalizeImagesDimension(filePath);

                var inchWidthString = inchWidth.ToString(CultureInfo.InvariantCulture);
                var inchHeightString = inchHeight.ToString(CultureInfo.InvariantCulture);

                var markdownFilePath = $"{backPath}{Path.DirectorySeparatorChar}{textHash}.png";
                var referenceUrl = $@"![]({markdownFilePath.Replace(Path.DirectorySeparatorChar, '/')}){{width=""{inchWidthString}in"" height=""{inchHeightString}in"" }}";
                _logger.LogInformation(referenceUrl);
                markdown = markdown.Replace(item.Groups[0].Value, referenceUrl);
                //File.WriteAllText(filePath + "test.md", markdown);
            }
            Directory.SetCurrentDirectory(Path.GetDirectoryName(requestInfo.CurrentRoot));
            return markdown;
        }

        private (double,double) NormalizeImagesDimension(string filePath)
        {
            double inchHeight, inchWidth;
            var res1 = File.ReadAllBytes(filePath);
            MemoryStream imageStream = new MemoryStream(res1);
            Bitmap image = new Bitmap(imageStream);
            var pixelWidth = image.Width;
            var pixelHeight = image.Height;
            var ratio = Convert.ToDouble(pixelWidth) / Convert.ToDouble(pixelHeight);
            inchHeight = 0;
            inchWidth = 0;
            double conversionHeightInchPixel = 10D / 1500D;
            //double converionWidthInchPixel = 4,5 / 750;


            if (pixelHeight > 1500)
            {
                inchHeight = 9;
                if (pixelWidth < 750)
                {
                    inchWidth = inchHeight * ratio;
                }
                if (pixelWidth > 750)
                {
                    inchWidth = 6;
                }
            }
            if (pixelHeight < 1500)
            {

                if (pixelWidth < 750)
                {
                    // usa il ratio
                    inchHeight = conversionHeightInchPixel * pixelHeight;
                    inchWidth = inchHeight * ratio;
                }
                else
                {
                    inchWidth = 6; // devi tenere fisso il width a 6
                    inchHeight = 1 / ratio * inchWidth;
                }
            }
            return (inchHeight, inchWidth);
        }
    }
}
