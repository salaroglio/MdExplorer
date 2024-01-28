using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
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
        public LinkDetail[] GetLinksFromMarkdown(string markdown)
        {
            var toReturn = new List<LinkDetail>();
            // Devo prima isolare la quota parte di plantuml
            Regex rx = new Regex(@"```plantuml([^```]*)```",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);

            var counter = 0;
            foreach (Match item in matches)
            {
                Regex rx1 = new Regex(@"\[\[([^\]]*)\]\]");
                var matches1 = rx1.Matches(item.Groups[1].Value);
                foreach (Match match in matches1)
                {
                    // i have to parse the filename from the link
                    var toParse = match.Groups[1].Value;
                    Regex rx2 = new Regex(@"(.*\.md)(?:(#.*?))?");
                    var matches2 = rx2.Matches(toParse.ToLower());

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
            return toReturn.ToArray();
        }

        public LinkDetail[] GetLinksFromFile(string filepath)
        {
            var markdown = string.Empty;
            using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream))
                {
                    markdown = reader.ReadToEnd();
                }
            }
            return GetLinksFromMarkdown(markdown);
        }

        public void SetLinkIntoFile(string filepath, string oldLink, string newLink)
        {
            var markdown = File.ReadAllText(filepath);
            markdown = markdown.Replace(oldLink, newLink);
            File.WriteAllText(filepath, markdown);
        }
    }
}
