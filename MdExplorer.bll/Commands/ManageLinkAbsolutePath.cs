using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands.html;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    internal class ManageLinkAbsolutePath : ICommand
    {
        private readonly ILogger<ManageLinkAbsolutePath> _logger;

        public ManageLinkAbsolutePath(ILogger<ManageLinkAbsolutePath> logger)
        {
            _logger = logger;
        }

        public int Priority { get; set; } = 50;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "ManageLinkAbsolutePath";

        public MatchCollection GetMatches(string markdown)
        {
            var reg = @"\[([^\]]*)\]\(([^\)]*)\)";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            // DO NOTHING
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            // DO NOTHING
            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var links = GetMatches(markdown);
            foreach (Match link in links)
            {
                if (link.Groups[2].Value.StartsWith("/"))
                {
                    var newlink = "/api/mdexplorer" + link.Groups[2].Value;
                    var allElementToReplace = link.Groups[0].Value.Replace(link.Groups[2].Value, newlink);
                    markdown = markdown.Replace(link.Groups[0].Value, allElementToReplace);
                }
            }
            return markdown;
        }
    }
}
