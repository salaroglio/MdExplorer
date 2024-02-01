using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Refactoring.Work.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Refactoring.Work
{
    internal class WorkLinkMdShowH2 : IWorkLink
    {
        public LinkDetail[] GetLinksFromMarkdown(string markdown)
        {
            Regex rx = new Regex(@"MdShowH2\((.*?),(.*?),(.*?)(?:,(.*?))?\)", //
                                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(markdown);
            var listToReturn = new List<LinkDetail>();
            foreach (Match item in matches)
            {
                var toStore = new LinkDetail
                {
                    LinkedCommand = item.Groups[0].Value,
                    FullPath = item.Groups[1].Value,
                    MdTitle = item.Groups[2].Value,
                    HTMLTitle = item.Groups[3].Value,
                };
                listToReturn.Add(toStore);
            }
            return listToReturn.ToArray();
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
            //var markdown = File.ReadAllText(filepath);
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
