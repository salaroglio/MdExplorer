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

namespace MdExplorer.Features.Refactoring.Work
{
    public class WorkLinkMdShowMd : IWorkLink
    {
        public LinkDetail[] GetLinksFromMarkdown(string markdown)
        {
        
            var rx = new Regex(@"mdShowMd\(([^\)]*)\)",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            var listToReturn = new List<LinkDetail>();
            foreach (Match item in matches)
            {
                var toStore = new LinkDetail
                {
                    LinkedCommand = item.Groups[0].Value,
                    FullPath = item.Groups[1].Value,
                };
                listToReturn.Add(toStore);
            }
            return listToReturn.ToArray();
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

                return  new LinkDetail[] { };
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
