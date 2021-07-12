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
    public class MdFilesController : ControllerBase
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

            var list = new List<FileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath).Where(_=>!_.Contains(".md")))
            {
                (var node, var isempty) = CreateNodeFolder(itemFolder);
                if (!isempty)
                {
                    list.Add(node);
                }                
            }

            foreach (var itemFile in Directory.GetFiles(currentPath).Where(_ => Path.GetExtension(_) == ".md"))
            {
                var node = CreateNodeMdFile(itemFile);
                list.Add(node);
            }

            // nettificazione dei folder che non contengono md


            return Ok(list);
        }

        private FileInfoNode CreateNodeMdFile(string itemFile)
        {
            var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
            var node = new FileInfoNode { Name = Path.GetFileName(itemFile), Path = patchedItemFile, Type = "mdFile" };
            return node;
        }

        private (FileInfoNode,bool) CreateNodeFolder(string itemFolder)
        {
            var patchedItemFolfer = itemFolder.Substring(_fileSystemWatcher.Path.Length);
            var node = new FileInfoNode { Name = Path.GetFileName(itemFolder), Path = patchedItemFolfer, Type = "folder" };
            var isEmpty = ExploreNodes(node, itemFolder);            
            return (node, isEmpty);
        }

        private bool ExploreNodes(FileInfoNode fileInfoNode, string pathFile)
        {
            var isEmpty = true;

            foreach (var itemFolder in Directory.GetDirectories(pathFile).Where(_=>!_.Contains(".md")))
            {
                (FileInfoNode node, bool isempty )= CreateNodeFolder(itemFolder);
                if (!isempty)
                {
                    fileInfoNode.Childrens.Add(node);
                }                
                isEmpty = isempty;
            }

            foreach (var itemFile in Directory.GetFiles(pathFile).Where(_ => Path.GetExtension(_) == ".md"))
            {
                var node = CreateNodeMdFile(itemFile);
                fileInfoNode.Childrens.Add(node);
                isEmpty = false;
            }
            return isEmpty;
        }
    }
}
