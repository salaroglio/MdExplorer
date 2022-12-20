using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.ProjectBody
{
    public class ProjectBodyEngine
    {
       
        public FileInfoNode CreateNodeMdFile(string itemFile, string patchedItemFile)
        {
            //var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
            var fileName = Path.GetFileName(itemFile);
            var typeNode = "mdFile";
            var stringDatePattern = new Regex(@"[0-9][0-9][0-9][0-9][-][0-9][0-9][-][0-9][0-9]");
            
            if (stringDatePattern.IsMatch(fileName))
            {
                typeNode = "mdFileTimer";
            }
            
            

            var node = new FileInfoNode
            {
                Name = fileName,
                FullPath = itemFile,
                Path = patchedItemFile,
                RelativePath = patchedItemFile,
                Type = typeNode
            };
            return node;
        }

        public List<IFileInfoNode> GetPusblishDocuments(string currentPath, int currentLevel, string publishBaseFolder)
        {
            var list = new List<IFileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath))
            {
                var node = new FileInfoNode
                {
                    Name = Path.GetFileName(itemFolder),
                    FullPath = itemFolder,
                    Path = itemFolder,
                    Level = currentLevel,
                    Type = "publishFolder",
                    Expandable = Directory.GetDirectories(itemFolder).Count() > 0
                };
                list.Add(node);
            }
            foreach (var itemFolder in Directory.GetFiles(currentPath).Where(_ => _.Contains(".md")))
            {
                var relativePath = itemFolder.Replace(publishBaseFolder, string.Empty);
                var node = new FileInfoNode
                {
                    Name = Path.GetFileName(itemFolder),
                    FullPath = itemFolder,
                    Path = itemFolder,
                    Level = currentLevel,
                    Type = "mdFile",         
                    RelativePath = relativePath
                };
                list.Add(node);
            }
            

            return list;
        }

    }
}
