using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MdExplorer.Features.ProjectBody
{
    public class ProjectBodyEngine
    {
        public FileInfoNode CreateNodeMdFile(string itemFile)
        {
            var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
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
    }
}
