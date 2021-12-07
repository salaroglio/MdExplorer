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
    [Route("api/MdFiles/{action}")]
    public class MdFilesController : ControllerBase
    {
        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly IWorkLink[] _getModifiers;
        private readonly IHelper _helper;
        private readonly IUserSettingsDB _userSettingsDB;

        public IEngineDB _engineDB { get; }

        public MdFilesController(FileSystemWatcher fileSystemWatcher,
            IEngineDB engineDB, IWorkLink[] getModifiers, IHelper helper, 
            IUserSettingsDB userSettingsDB)
        {
            _fileSystemWatcher = fileSystemWatcher;
            _engineDB = engineDB;
            _getModifiers = getModifiers;
            _helper = helper;
            _userSettingsDB = userSettingsDB;
        }

        [HttpGet]
        public IActionResult GetFoldersDocument()
        {
            var currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); ;

            var list = new List<IFileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath)
                    .Where(_=>!_.Contains("Immagini") && 
                    !_.Contains("Musica") &&
                    !_.Contains("Video"))
                    )
            {
                var node = CreateNodeFolderOnly(itemFolder);
                list.Add(node);

            }

            return Ok(list);
        }

        [HttpGet]
        public IActionResult GetAllMdFiles()
        {
            var list = new List<IFileInfoNode>();
            var currentPath = _fileSystemWatcher.Path;
            if (currentPath == Directory.GetCurrentDirectory())
            {
                return Ok(list);
            }

            _userSettingsDB.BeginTransaction();
            var projectDal = _userSettingsDB.GetDal<Project>();
            if (_fileSystemWatcher.Path != string.Empty)
            {
                var currentProject = projectDal.GetList().Where(_ => _.Path == _fileSystemWatcher.Path).FirstOrDefault();
                if (currentProject == null)
                {
                    var projectName = _fileSystemWatcher.Path.Substring(_fileSystemWatcher.Path.LastIndexOf("\\") + 1);
                    currentProject = new Project { Name = projectName, Path = _fileSystemWatcher.Path };
                    projectDal.Save(currentProject);
                }
            }
            _userSettingsDB.Commit();


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
                    var fullPath = Path.GetDirectoryName(item.FullPath) + Path.DirectorySeparatorChar + singleLink.LinkPath.Replace('/', Path.DirectorySeparatorChar);
                    var linkToStore = new LinkInsideMarkdown
                    {
                        FullPath = _helper.NormalizePath(fullPath),
                        Path = singleLink.LinkPath,
                        Source = getModifier.GetType().Name,
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

        private FileInfoNode CreateNodeFolderOnly(string itemFolder)
        {
            var patchedItemFolfer = itemFolder;
            var node = new FileInfoNode { Name = Path.GetFileName(itemFolder), FullPath = itemFolder, Path = patchedItemFolfer, Type = "folder" };
            ExploreNodesFolderOnly(node, itemFolder);
            return node;
        }

        private void ExploreNodesFolderOnly(FileInfoNode fileInfoNode, string pathFile)
        {
            try
            {
                var accessControlList = FileSystemAclExtensions.GetAccessControl(new DirectoryInfo(pathFile));
                if (accessControlList.AreAccessRulesProtected)
                    return;

                if (!Directory.Exists(pathFile)) // jump directories where access is denied
                {
                    return;
                }
                foreach (var itemFolder in Directory.GetDirectories(pathFile))
                {
                    FileInfoNode node = CreateNodeFolderOnly(itemFolder);
                    fileInfoNode.Childrens.Add(node);
                }
            }
            catch (Exception ex)
            {

                
            }
            
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
