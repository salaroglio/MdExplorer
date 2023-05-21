//using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office.CustomUI;
using Markdig;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands.html;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    public class FromLinkToApplication : ICommand
    {
        private readonly ILogger<FromLinkToApplicationHtml> _logger;

        public FromLinkToApplication(ILogger<FromLinkToApplicationHtml> logger)
        {
            _logger = logger;
        }

        public int Priority { get; set; } = 30;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "FromLinkToApplication";

        private string[] ExtensionArrayToOpenInApplication = { "xlsx", "pdf" };

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"<a.+?<\/a>", //lnk?
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

            foreach (var extension in ExtensionArrayToOpenInApplication)
            {
                foreach (Match item in matches.Where(_ => _.Groups[0].Value.Contains($".{extension}")))
                {
                    Regex rx = new Regex(@$"(<a.+?)(href="")(.+?\.{extension})""", //lnk?
                                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

                    var matches1 = rx.Matches(item.Groups[0].Value);
                    if (matches1.Count == 0)
                    {
                        return html;
                    }
                    var item1 = matches1[0];

                    var documentRelativePath = Path.GetDirectoryName(requestInfo.RootQueryRequest);

                    var relativePath = documentRelativePath + Path.DirectorySeparatorChar + item1.Groups[3].Value.ToString();
                    var openApplication = $@"{item1.Groups[1].Value}href=""#"" onclick=""openApplication('{requestInfo.CurrentRoot + Path.DirectorySeparatorChar + relativePath}')""{item1.Groups[5].Value}".Replace(Path.DirectorySeparatorChar, '/');
                    html = html.Replace(item1.Groups[0].Value, openApplication);
                }
            }
            

            return html;

        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }
    }
}
