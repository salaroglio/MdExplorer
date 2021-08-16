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
    public class GetLinkImgFromPlantuml : IManageLink
    {
        public LinkDetail[] GetLinks(string markdown)
        {
            var toReturn = new List<LinkDetail>();
            // Devo prima isolare la quota parte di plantuml
            Regex rx = new Regex(@"```plantuml([^```]*)```",
                               RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);

            var counter = 0;
            foreach (Match item in matches)
            {
                Regex rx1 = new Regex(@"<img:([^>]*)>");
                var matches1 = rx1.Matches(item.Groups[1].Value);
                foreach (Match match in matches1)
                {
                    var linkDetail = new LinkDetail
                    {
                        LinkedCommand = match.Groups[0].Value,
                        LinkPath = match.Groups[1].Value,
                        SectionIndex = counter
                    };
                    toReturn.Add(linkDetail);
                }
                counter++;
            }
            // devo poi andare a cercare i link
            // infine segnare i link ed immettere a quale index sono stati trovati
            return toReturn.ToArray();
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
