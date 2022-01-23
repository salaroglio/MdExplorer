using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class ManageLinkAsImages : ICommand
    {
        public int Priority { get; set; } = 5;
        public bool Enabled { get; set; } = true;
        public string Name { get; set; } = "ManageLinkAsImages";
        private readonly ILogger<ManageLinkAsImages> _logger;
        private readonly IHelper _helper;

        public ManageLinkAsImages(ILogger<ManageLinkAsImages> logger,
               IHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public MatchCollection GetMatches(string markdown)
        {
            var reg = @"!\[([^\]]*)\]\(([^\)]*)\)";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var matches = GetMatches(markdown);

            foreach (Match item in matches)
            {
                // here you should compose the path adding missing part
                // the missing part is the distance from the root folder and the current file
                // you can build this using requestInfo.currentqueryrequest
                var listOfItem = requestInfo.CurrentQueryRequest.Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries).ToList();
                listOfItem.RemoveAt(listOfItem.Count - 1);
                var currentWebFolder = string.Empty;
                foreach (var item1 in listOfItem)
                {
                    if (item1 == listOfItem.First())
                    {
                        currentWebFolder = item1;
                    }
                    else
                    {
                        currentWebFolder += "/" + item1;
                    }
                }
                currentWebFolder = string.Join(Path.DirectorySeparatorChar, listOfItem.ToArray());
                var fileName = currentWebFolder + "/" + item.Groups[2].Value;
                var allElementToReplace = item.Groups[0].Value.Replace(item.Groups[2].Value, fileName);
                markdown = markdown.Replace(item.Groups[0].Value, allElementToReplace);
            }

            return markdown;

        }
    }
}
