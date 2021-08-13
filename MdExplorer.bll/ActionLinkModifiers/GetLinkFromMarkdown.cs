using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.LinkModifiers
{
    public class GetLinkFromMarkdown: IGetModifier
    {
        public LinkDetail[] GetLinks(string mardown)
        {
            var rx = new Regex(@"!\[[^\]]*\]\(([^\)]*)\)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(mardown);
            var listToReturn = new List<LinkDetail>();
            foreach (Match item in matches) 
            {

                var toStore = new LinkDetail {
                    LinkedCommand = item.Groups[0].Value,
                    LinkPath = item.Groups[1].Value,
                } ;
                listToReturn.Add(toStore);
            }
            return listToReturn.ToArray();
        }

        public LinkDetail[] GetLinksFromFile(string filepath)
        {
            var markdown = File.ReadAllText(filepath);
            return GetLinks(markdown);
        }
    }
}
