using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.Refactoring.Work
{
    public class WorkLinkMdShowMd : IWorkLink
    {
        public LinkDetail[] GetLinks(string markdown)
        {
        
            var rx = new Regex(@"M↓ShowMd\(([^\)]*)\)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            var listToReturn = new List<LinkDetail>();
            foreach (Match item in matches)
            {
                var toStore = new LinkDetail
                {
                    LinkedCommand = item.Groups[0].Value,
                    LinkPath = item.Groups[1].Value,
                };
                listToReturn.Add(toStore);
            }
            return listToReturn.ToArray();
        }

        public LinkDetail[] GetLinksFromFile(string filepath)
        {
            var markdown = File.ReadAllText(filepath);
            return GetLinks(markdown);
        }

        public void SetLinkIntoFile(string filepath, string oldLink, string newLink)
        {
            var markdown = File.ReadAllText(filepath);
            markdown = markdown.Replace(oldLink, newLink);
            File.WriteAllText(filepath, markdown);
        }
    }
}
