using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Utilities;
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
        private readonly IManageLink[] _getModifiers;
        private readonly IHelper _helper;

        public IEngineDB _engineDB { get; }

        public MdFilesController(FileSystemWatcher fileSystemWatcher,
            IEngineDB engineDB, IManageLink[] getModifiers, IHelper helper )
        {
            _fileSystemWatcher = fileSystemWatcher;
            _engineDB = engineDB;
            _getModifiers = getModifiers;
            _helper = helper;
        }

        [HttpGet]
        public IActionResult GetAllMdFiles()
        {
            var currentPath = _fileSystemWatcher.Path;

            var list = new List<IFileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath).Where(_ => !_.Contains(".md")))
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
            _engineDB.BeginTransaction();
            _engineDB.Delete("from LinkInsideMarkdown");
            _engineDB.Flush();
            _engineDB.Delete("from MarkdownFile");
            _engineDB.Flush();
            
            SaveRealationships(list);
            _engineDB.Commit();
            return Ok(list);
        }

        private void SaveRealationships(IList<IFileInfoNode> list, Guid? parentId = null)
        {
            // clean all data

            foreach (var item in list)
            {
                var markdownFile = new MarkdownFile
                {
                    FileName = item.Name,
                    LinkPath = item.Path,
                    Path = item.FullPath,
                    FileType = "File"
                };

                var relDal = _engineDB.GetDal<MarkdownFile>();
                if (item.Childrens.Count > 0)
                {
                    markdownFile.FileType = "Folder";
                    SaveRealationships(item.Childrens, markdownFile.Id);
                }


                relDal.Save(markdownFile);

                var linkInsideMarkdownDal = _engineDB.GetDal<LinkInsideMarkdown>();
                SaveLinksFromMarkdown(item, markdownFile, linkInsideMarkdownDal);  
                

            }

        }

        private void SaveLinksFromMarkdown(IFileInfoNode item, MarkdownFile relationship, IDAL<LinkInsideMarkdown> linkInsideMarkdownDal)
        {
            foreach (var getModifier in _getModifiers)
            {
                var linksToStore = relationship.FileType == "File" ? getModifier.GetLinksFromFile(item.FullPath) : new List<LinkDetail>().ToArray();
                foreach (var singleLink in linksToStore)
                {
                    var fullPath = Path.GetDirectoryName(item.FullPath) + Path.DirectorySeparatorChar + singleLink.LinkPath.Replace('/',Path.DirectorySeparatorChar);
                    var linkToStore = new LinkInsideMarkdown
                    {
                        FullPath = _helper.NormalizePath(fullPath),
                        Path = singleLink.LinkPath,
                        Source =  getModifier.GetType().Name,
                        LinkedCommand = singleLink.LinkedCommand,
                        SectionIndex = singleLink.SectionIndex,
                        MarkdownFile = relationship
                    };
                    linkInsideMarkdownDal.Save(linkToStore);
                }
            }
            
        }

        private FileInfoNode CreateNodeMdFile(string itemFile)
        {
            var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
            var node = new FileInfoNode { Name = Path.GetFileName(itemFile), FullPath = itemFile, Path = patchedItemFile, Type = "mdFile" };
            return node;
        }

        private (FileInfoNode, bool) CreateNodeFolder(string itemFolder)
        {
            var patchedItemFolfer = itemFolder.Substring(_fileSystemWatcher.Path.Length);
            var node = new FileInfoNode { Name = Path.GetFileName(itemFolder), FullPath = itemFolder, Path = patchedItemFolfer, Type = "folder" };
            var isEmpty = ExploreNodes(node, itemFolder);
            return (node, isEmpty);
        }

        private bool ExploreNodes(FileInfoNode fileInfoNode, string pathFile)
        {
            var isEmpty = true;

            foreach (var itemFolder in Directory.GetDirectories(pathFile).Where(_ => !_.Contains(".md")))
            {
                (FileInfoNode node, bool isempty) = CreateNodeFolder(itemFolder);
                if (!isempty)
                {
                    fileInfoNode.Childrens.Add(node);
                }
                isEmpty = isEmpty && isempty;
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
