using MdExplorer.Abstractions.Models;
using MdExplorer.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Controllers
{
    [ApiController]
    [Route("api/MdFiles")]
    public class MdFilesController:ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;

        public MdFilesController(FileSystemWatcher fileSystemWatcher)
        {
            _fileSystemWatcher = fileSystemWatcher;
        }
        [HttpGet]
        public IActionResult GetAllMdFiles()
        {
            var currentPath = _fileSystemWatcher.Path;

            var list = new List< FileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath))
            {
                var node = CreateNodeFolder(itemFolder);
                list.Add(node);
            }

            foreach (var itemFile in Directory.GetFiles(currentPath).Where(_ => Path.GetExtension(_) == ".md"))
            {
                var node = CreateNodeMdFile(itemFile);
                list.Add(node);
            }

            return Ok( list);
        }

        private FileInfoNode CreateNodeMdFile(string itemFile)
        {
            var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
            var node = new FileInfoNode { Name = Path.GetFileName(itemFile), Path = patchedItemFile, Type = "mdFile" };
            return node;
        }

        private FileInfoNode CreateNodeFolder(string itemFolder)
        {
            var patchedItemFolfer = itemFolder.Substring(_fileSystemWatcher.Path.Length);
            var node = new FileInfoNode { Name = Path.GetFileName(itemFolder), Path = patchedItemFolfer, Type = "folder" };
            ExploreNodes(node, itemFolder);
            return node;
        }

        private void ExploreNodes(FileInfoNode fileInfoNode, string pathFile)
        {
            foreach (var itemFolder in Directory.GetDirectories(pathFile))
            {
                FileInfoNode node = CreateNodeFolder(itemFolder);
                fileInfoNode.Childrens.Add(node);
            }

            foreach (var itemFile in Directory.GetFiles(pathFile).Where(_ => Path.GetExtension(_) == ".md"))
            {
                var node = CreateNodeMdFile(itemFile);
                fileInfoNode.Childrens.Add(node);
            }
        }
    }
}
