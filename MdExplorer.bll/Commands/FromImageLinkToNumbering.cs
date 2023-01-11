using Markdig;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands
{
    internal class FromImageLinkToNumbering : ICommand
    {
        public int Priority { get; set; } = 40;
        public bool Enabled { get ; set ; }= true;
        public string Name { get; set; } = "FromImageLinkToNumbering";

        public MatchCollection GetMatches(string markdown)
        {
            var reg = @"!\[([^\]]*)\]\((.*)\)";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase);
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
            // Prepare layer

            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            // DO NOTHING
            return markdown;
        }
    }
}
