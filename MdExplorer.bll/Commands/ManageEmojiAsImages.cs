using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Utilities;
using Microsoft.AspNetCore.Http;
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
    public class ManageEmojiAsImages : ICommand
    {


        private readonly ILogger<ManageEmojiAsImages> _logger;
        private readonly IHelper _helper;

        public ManageEmojiAsImages(ILogger<ManageEmojiAsImages> logger,
                IHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }
        public int Priority { get; set; } = 30;
        public string Name { get; set; } = "ManageEmojiAsImages";
        public bool Enabled { get; set; } = true;
        public MatchCollection GetMatches(string markdown)
        {
            // ![alt text](Icons\plus.png "Title")
            // !\[alt text\]\(([^\"]*)
            var reg = @"!\[alt text\]\(([^\""]*)";
            Regex rx = new Regex(reg,
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            // Do nothing
            return markdown;
        }

        public string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            // Do nothing
            return html;
        }

        public string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var directoryInfo = Directory.CreateDirectory(requestInfo.CurrentRoot + $"{Path.DirectorySeparatorChar}.md");
            
            var matches = GetMatches(markdown);
            foreach (Match item in matches)
            {
                var fileToCopyIntoMetadata = item.Groups[1].Value;
                //prepare directory

                var fileName = Path.GetFileName(fileToCopyIntoMetadata);
                var destination = $"{directoryInfo}{Path.DirectorySeparatorChar}{fileName}";
                File.Copy(fileToCopyIntoMetadata, destination, true);

                markdown = markdown.Replace(item.Groups[1].Value, $".md{Path.DirectorySeparatorChar}{fileName}");                
            }
            


            return markdown;
        }
    }
}
