using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Refactoring.Work.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MdExplorer.Features.ActionLinkModifiers
{
    public class WorkLinkImgFromPlantuml : IWorkLink
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
                Regex rx1 = new Regex(@"<img:([^>]*)>");
                var matches1 = rx1.Matches(item.Groups[1].Value);
                foreach (Match match in matches1)
                {
                    var linkDetail = new LinkDetail
                    {
                        LinkedCommand = match.Groups[0].Value,
                        FullPath = match.Groups[1].Value,
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

        public string Relink(RelinkInfo relinkInfo)
        {
            var oldPathFile = relinkInfo.OldRelativePath;
            var newPathFile = Path.Combine(relinkInfo.NewRelativePath, relinkInfo.NewFileName);
            newPathFile = "/" + newPathFile.Replace(Path.DirectorySeparatorChar, '/');
            var newCommand = relinkInfo.LinkedCommand.Replace(oldPathFile, newPathFile);
            return newCommand;
        }
    }
}
