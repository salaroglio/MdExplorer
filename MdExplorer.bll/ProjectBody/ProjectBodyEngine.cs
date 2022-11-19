using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.ProjectBody
{
    public class ProjectBodyEngine
    {
       
        public FileInfoNode CreateNodeMdFile(string itemFile, string patchedItemFile)
        {
            //var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
            var node = new FileInfoNode
            {
                Name = Path.GetFileName(itemFile),
                FullPath = itemFile,
                Path = patchedItemFile,
                RelativePath = patchedItemFile,
                Type = "mdFile"
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
            foreach (var itemFolder in Directory.GetFiles(currentPath))
            {
                var relativePath = currentPath.Replace(publishBaseFolder, string.Empty);
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
