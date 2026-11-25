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
    public class WorkLinkFromMarkdown : IWorkLink
    {
        /// <summary>
        /// I link (non le immagini) letti con Markdig, come la pagina: anche in prima riga, senza titolo né parentesi
        /// angolari nel percorso, mai dentro il codice; i link stile riferimento dalla loro definizione.
        /// </summary>
        public LinkDetail[] GetLinksFromMarkdown(string markdown)
            => MdExplorer.Features.Refactoring.MarkdownLinks.Read(markdown)
                .Where(l => !l.IsImage)
                .Select(l => new LinkDetail
                {
                    LinkedCommand = l.Command,
                    FullPath = l.Target,
                    HTMLTitle = l.Suffix.StartsWith("#") ? l.Suffix : string.Empty,
                })
                .ToArray();

        public LinkDetail[] GetLinksFromFile(string filepath)
        {
            var markdown = string.Empty;
            try
            {
                using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
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
            newPathFile = "/" +  newPathFile.Replace(Path.DirectorySeparatorChar, '/');
            var newCommand = relinkInfo.LinkedCommand.Replace(oldPathFile, newPathFile);
            return newCommand;
        }
    }
}
