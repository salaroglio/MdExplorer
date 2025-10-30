using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Refactoring.Work.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.ActionLinkModifiers
{
    public class WorkLinkFromPlantuml : IWorkLink
    {
        private readonly ILogger<WorkLinkFromPlantuml> _logger;

        public WorkLinkFromPlantuml(ILogger<WorkLinkFromPlantuml> logger)
        {
            _logger = logger;
        }

        public LinkDetail[] GetLinksFromMarkdown(string markdown)
        {
            _logger.LogInformation($"🔍 [PlantUML Parser] START - Analyzing markdown (length: {markdown?.Length ?? 0} chars)");

            var toReturn = new List<LinkDetail>();
            // Devo prima isolare la quota parte di plantuml
            Regex rx = new Regex(@"```plantuml([^```]*)```",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);

            _logger.LogInformation($"🔍 [PlantUML Parser] Found {matches.Count} PlantUML blocks");

            var counter = 0;
            foreach (Match item in matches)
            {
                Regex rx1 = new Regex(@"\[\[([^\]]*)\]\]");
                var matches1 = rx1.Matches(item.Groups[1].Value);
                foreach (Match match in matches1)
                {
                    // Parse [[path label]] or [[path#section label]]
                    // The path and label are separated by space in PlantUML syntax
                    var toParse = match.Groups[1].Value.Trim();
                    _logger.LogInformation($"🔍 [PlantUML Parser] Raw content: '{toParse}'");

                    // Split by space: first part is path, rest is label
                    // This prevents greedy regex from including label text containing ".md"
                    var parts = toParse.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                    var pathPart = parts.Length > 0 ? parts[0] : toParse;
                    _logger.LogInformation($"🔍 [PlantUML Parser] Extracted path: '{pathPart}'");

                    // Extract .md file and optional #section from path only
                    Regex rx2 = new Regex(@"(.*\.md)(?:(#.*?))?");
                    var matches2 = rx2.Matches(pathPart.ToLower());

                    foreach (Match match2 in matches2)
                    {
                        var linkDetail = new LinkDetail
                        {
                            LinkedCommand = match.Groups[0].Value,
                            FullPath = match2.Groups[1].Value,
                            HTMLTitle = match2.Groups[2]?.Value,
                            SectionIndex = counter
                        };
                        toReturn.Add(linkDetail);
                    }

                }
                
                counter++;
            }
            // devo poi andare a cercare i link
            // infine segnare i link ed immettere a quale index sono stati trovati
            _logger.LogInformation($"🔍 [PlantUML Parser] END - Extracted {toReturn.Count} links");
            return toReturn.ToArray();
        }

        public LinkDetail[] GetLinksFromFile(string filepath)
        {
            var markdown = string.Empty;
            try
            {
                using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        markdown = reader.ReadToEnd();
                    }
                }

            }
            catch (Exception)
            {

               return new LinkDetail[] { }; 
            }
            
            return GetLinksFromMarkdown(markdown);
        }

        public void SetLinkIntoFile(string filepath, string oldLink, string newLink)
        {
            var markdown = File.ReadAllText(filepath);
            markdown = markdown.Replace(oldLink, newLink);
            File.WriteAllText(filepath, markdown);
        }

        public string Relink(RelinkInfo relinkInfo)
        {
            var oldPathFile = relinkInfo.OldRelativePath;
            var newPathFile = Path.Combine(relinkInfo.NewRelativePath, relinkInfo.NewFileName);
            newPathFile ="/" +  newPathFile.Replace(Path.DirectorySeparatorChar, '/');

            Regex rx = new Regex(oldPathFile, //([^:^ ]*) //replace all emoji from markdown
                             RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var commandToReplace = rx.Replace(relinkInfo.LinkedCommand, newPathFile);

            var newCommand = commandToReplace;// relinkInfo.LinkedCommand.Replace(oldPathFile, newPathFile);
            return newCommand;
        }
    }
}
