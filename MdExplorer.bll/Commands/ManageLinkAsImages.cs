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
        public List<Configuration.Models.CompatibilityMode> SupportedModes => null; // Supports all modes
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

        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            var matches = GetMatches(markdown);

            foreach (Match item in matches)
            {
                var originalImagePath = item.Groups[2].Value;
                string fileName;

                // Path assoluti (iniziano con /) - rimuovi lo slash iniziale per renderli relativi alla root del progetto
                if (originalImagePath.StartsWith("/"))
                {
                    fileName = originalImagePath.TrimStart('/');
                }
                // Path relativi - componi il path aggiungendo la cartella corrente
                else
                {
                    var listOfItem = requestInfo.CurrentQueryRequest.Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries).ToList();
                    listOfItem.RemoveAt(listOfItem.Count - 1);
                    var currentWebFolder = string.Join(Path.DirectorySeparatorChar, listOfItem.ToArray());
                    fileName = currentWebFolder + "/" + originalImagePath;
                }

                var allElementToReplace = item.Groups[0].Value.Replace(originalImagePath, fileName);
                markdown = markdown.Replace(item.Groups[0].Value, allElementToReplace);
            }

            return markdown;
        }
    }
}
