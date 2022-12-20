//using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office.CustomUI;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands.html;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    public class FromLinkToapplication : ICommand
    {
        private readonly ILogger<FromLinkToApplicationHtml> _logger;

        public FromLinkToapplication(ILogger<FromLinkToApplicationHtml> logger)
        {
            _logger = logger;
        }

        public int Priority { get; set; } = 30;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "FromLinkToApplication";

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"(<a.+?)(href="")(.+?\.XLSX)""",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            var matches = GetMatches(html);
            foreach (Match item in matches)
            {
                var documentRelativePath = Path.GetDirectoryName(requestInfo.RootQueryRequest);

                var relativePath = documentRelativePath+ Path.DirectorySeparatorChar + item.Groups[3].Value.ToString();
                var openApplication = $@"{item.Groups[1].Value }href=""#"" onclick=""openApplication('{
                    requestInfo.CurrentRoot + Path.DirectorySeparatorChar + relativePath}')""{
                    item.Groups[5].Value}".Replace(Path.DirectorySeparatorChar, '/');
                html = html.Replace(item.Groups[0].Value, openApplication);
            }

            return html;

        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }
    }
}
