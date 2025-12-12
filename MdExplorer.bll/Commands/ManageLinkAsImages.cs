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
            _logger.LogInformation($"🔍 [ManageLinkAsImages] Starting image path transformation");
            _logger.LogInformation($"🔍 [ManageLinkAsImages] RequestInfo - CurrentQueryRequest: {requestInfo.CurrentQueryRequest}");
            _logger.LogInformation($"🔍 [ManageLinkAsImages] RequestInfo - CurrentRoot: {requestInfo.CurrentRoot}");
            _logger.LogInformation($"🔍 [ManageLinkAsImages] RequestInfo - AbsolutePathFile: {requestInfo.AbsolutePathFile}");
            _logger.LogInformation($"🔍 [ManageLinkAsImages] RequestInfo - BaseUrl: {requestInfo.BaseUrl}");

            var matches = GetMatches(markdown);
            _logger.LogInformation($"🔍 [ManageLinkAsImages] Found {matches.Count} image references");

            foreach (Match item in matches)
            {
                var originalImagePath = item.Groups[2].Value;
                _logger.LogInformation($"🔍 [ManageLinkAsImages] Processing image: {originalImagePath}");

                string fileName;

                // Path assoluti (iniziano con /) - rimuovi lo slash iniziale per renderli relativi alla root del progetto
                if (originalImagePath.StartsWith("/"))
                {
                    fileName = originalImagePath.TrimStart('/');
                    _logger.LogInformation($"🔍 [ManageLinkAsImages] Absolute path detected, converted to: {fileName}");
                }
                // Path relativi - componi il path aggiungendo la cartella corrente
                else
                {
                    // here you should compose the path adding missing part
                    // the missing part is the distance from the root folder and the current file
                    // you can build this using requestInfo.currentqueryrequest

                    var listOfItem = requestInfo.CurrentQueryRequest.Split(Path.DirectorySeparatorChar, options: StringSplitOptions.RemoveEmptyEntries).ToList();
                    listOfItem.RemoveAt(listOfItem.Count - 1);
                    var currentWebFolder = string.Join(Path.DirectorySeparatorChar, listOfItem.ToArray());
                    fileName = currentWebFolder + "/" + originalImagePath;
                    _logger.LogInformation($"🔍 [ManageLinkAsImages] Relative path, currentWebFolder: {currentWebFolder}");
                }

                var allElementToReplace = item.Groups[0].Value.Replace(originalImagePath, fileName);

                _logger.LogInformation($"🔍 [ManageLinkAsImages] Original: {item.Groups[0].Value}");
                _logger.LogInformation($"🔍 [ManageLinkAsImages] Transformed: {allElementToReplace}");
                _logger.LogInformation($"🔍 [ManageLinkAsImages] Final fileName: {fileName}");

                markdown = markdown.Replace(item.Groups[0].Value, allElementToReplace);
            }

            _logger.LogInformation($"🔍 [ManageLinkAsImages] Image path transformation completed");
            return markdown;

        }
    }
}
