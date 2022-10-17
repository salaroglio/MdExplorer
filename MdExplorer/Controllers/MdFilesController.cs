using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using Markdig;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Entities.ProjectDB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Refactoring.Analysis;
using MdExplorer.Features.Refactoring.Analysis.Interfaces;
using MdExplorer.Features.snippets;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NHibernate.Linq;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IGoodMdRule<FileInfoNode>[] _goodRules;
        private readonly IProjectDB _projectDB;
        private readonly ISnippet<DictionarySnippetParam>[] _snippets;

        public IEngineDB _engineDB { get; }

        public MdFilesController(FileSystemWatcher fileSystemWatcher,
            IEngineDB engineDB, IWorkLink[] getModifiers, IHelper helper,
            IUserSettingsDB userSettingsDB,
            IHubContext<MonitorMDHub> hubContext,
            IGoodMdRule<FileInfoNode>[] GoodRules,
            IProjectDB projectDB,
            ISnippet<DictionarySnippetParam>[] snippets
            )
        {
            _fileSystemWatcher = fileSystemWatcher;
            _engineDB = engineDB;
            _getModifiers = getModifiers;
            _helper = helper;
            _userSettingsDB = userSettingsDB;
            _hubContext = hubContext;
            _goodRules = GoodRules;
            _projectDB = projectDB;
            _snippets = snippets;
        }


        [HttpGet]
        public IActionResult GetLandingPage()
        {
            var dal = _projectDB.GetDal<ProjectSetting>();
            var landingPage = dal.GetList().Where(_ => _.Name == "LandingPageFilePath").First();

            var toReturn = landingPage.LandingPages.Count() != 0 ? new FileInfoNode
            {
                Expandable  = landingPage.LandingPages.First().Expandable,
                FullPath = landingPage.LandingPages.First().FullPath,
                Level = landingPage.LandingPages.First().Level,
                Name = landingPage.LandingPages.First().Name,
                Path = landingPage.LandingPages.First().Path,
                RelativePath = landingPage.LandingPages.First().RelativePath,
                Type = landingPage.LandingPages.First().Type,
                DataText = landingPage.LandingPages.First().DataText,                
            }: null;
            return Ok(toReturn);
        }

        public IActionResult OpenFolderOnFileExplorer([FromBody] FileInfoNode fileData)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = fileData.FullPath,
                FileName = "explorer.exe"
             };

            Process.Start(startInfo);
            return Ok(new { message="done" });
        }

        [HttpPost]
        public IActionResult SetLandingPage([FromBody] FileInfoNode fileData)
        {
            _projectDB.BeginTransaction();

            var dal = _projectDB.GetDal<ProjectSetting>();
            var landingPage = dal.GetList().Where(_ => _.Name == "LandingPageFilePath").First();
            landingPage.ValueString = fileData.FullPath;
            dal.Save(landingPage);
            var dalDetails = _projectDB.GetDal<ProjectFileInfoNode>();
            var landingPageDetails = dalDetails.GetList().Where(_ => _.ProjectSetting == landingPage)
                .FirstOrDefault();
            if (landingPageDetails == null)
            {
                landingPageDetails = new ProjectFileInfoNode
                {
                    ProjectSetting = landingPage,
                    Level = fileData.Level,
                    Expandable = fileData.Expandable,
                    Name = fileData.Name,
                    FullPath = fileData.FullPath,
                    Path = fileData.Path,
                    RelativePath = fileData.RelativePath,
                    Type = fileData.Type,
                    DataText = fileData.DataText,
                };
            }else
            {
                landingPageDetails.Level = fileData.Level;
                landingPageDetails.Expandable = fileData.Expandable;
                landingPageDetails.Name = fileData.Name;
                landingPageDetails.FullPath = fileData.FullPath;
                landingPageDetails.Path = fileData.Path;
                landingPageDetails.RelativePath = fileData.RelativePath;
                landingPageDetails.Type = fileData.Type;
                landingPageDetails.DataText = fileData.DataText;
            }
               
               
            
            dalDetails.Save(landingPageDetails);

            _projectDB.Commit();
            return Ok(new {message = "Done" });
        }



        [HttpGet]
        public IActionResult GetDynFoldersDocument([FromQuery] string path, string level)
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var currentPath = path == "root" ? basePath : path;
            var currentLevel = Convert.ToInt32(level);
            var list = new List<IFileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath)
                    .Where(_ => !_.Contains("Immagini") &&
                    !_.Contains("Musica") &&
                    !_.Contains("Video"))
                    )
            {
                var node = new FileInfoNode
                {
                    Name = Path.GetFileName(itemFolder),
                    FullPath = itemFolder,
                    Path = itemFolder,
                    Level = currentLevel,
                    Type = "folder4",
                    Expandable = Directory.GetDirectories(itemFolder).Count() > 0
                };

                list.Add(node);

            }

            return Ok(list);
        }

        [HttpGet]
        public IActionResult GetFoldersDocument()
        {
            var currentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); ;

            var list = new List<IFileInfoNode>();

            foreach (var itemFolder in Directory.GetDirectories(currentPath)
                    .Where(_ => !_.Contains("Immagini") &&
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
        public async Task<IActionResult> GetAllMdFiles()
        {
            await _hubContext.Clients.All.SendAsync("parsingProjectStart", "process started");
            var list = new List<IFileInfoNode>();
            var currentPath = _fileSystemWatcher.Path;
            if (currentPath == AppDomain.CurrentDomain.BaseDirectory)
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
            await _hubContext.Clients.All.SendAsync("parsingProjectStop", "process completed");
            return Ok(list);
        }

        /// <summary>
        /// It helps to create new Markdown when on client:right click and then: create new markdow
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateNewMd([FromBody] NewFile fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var goodMdRuleFileNameShouldBeSameAsTitle =
                    _goodRules.First(_ => _.GetType() ==
                        typeof(GoodMdRuleFileNameShouldBeSameAsTitle)) as GoodMdRuleFileNameShouldBeSameAsTitle;

            var title = goodMdRuleFileNameShouldBeSameAsTitle.GetTitle(fileData.Title) + ".md";
            var fullPath = fileData.DirectoryPath + Path.DirectorySeparatorChar + title;
            if (fileData.DirectoryLevel == 0 && fileData.DirectoryPath == "root")
            {
                fullPath = _fileSystemWatcher.Path + Path.DirectorySeparatorChar + title;
            }

            // Text Document Management
            var templateContent = string.Empty;
            var snippetTextDocument = _snippets.Where(_ => _.Id == 0).FirstOrDefault();
            var dictParam = new DictionarySnippetParam();
            dictParam.Add(ParameterName.StringDocumentTitle, fileData.Title);
            dictParam.Add(ParameterName.ProjectPath, _fileSystemWatcher.Path);
            templateContent = snippetTextDocument.GetSnippet(dictParam);
            

            // Additional Template 
            var snippet =_snippets.Where(_ => _.Id == fileData.documentTypeId && _.Id != 0).FirstOrDefault();
            if (snippet != null)
            {
                snippet.SetAssets(fullPath);
                var addtionalTemplateContent = snippet.GetSnippet(dictParam);
                templateContent = string.Concat(templateContent, addtionalTemplateContent);
            }
            
            
            var relativePath = fullPath.Replace(_fileSystemWatcher.Path, String.Empty);
            System.IO.File.WriteAllText(fullPath, templateContent);


            var list = new List<IFileInfoNode>();
            var relativeSplitted = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).SkipLast(1);

            var dynamicRelativePath = string.Empty;
            var currentLevel = 0;
            foreach (var item in relativeSplitted)
            {
                dynamicRelativePath =
                        relativeSplitted.First() == item ? string.Empty : dynamicRelativePath + Path.DirectorySeparatorChar.ToString();
                dynamicRelativePath += item;

                var node = new FileInfoNode
                {
                    Name = item,
                    FullPath = _fileSystemWatcher.Path + Path.DirectorySeparatorChar + dynamicRelativePath,
                    RelativePath = dynamicRelativePath,
                    Path = dynamicRelativePath,
                    Type = "folder",
                    Level = currentLevel,
                };
                currentLevel++;
                list.Add(node);
            }
            var nodeFile = new FileInfoNode
            {
                Name = title,
                FullPath = fullPath,
                Path = relativePath,
                RelativePath = relativePath,
                Type = "mdFile",
                Level = currentLevel,
            };
            list.Add(nodeFile);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(list);
        }

        [HttpPost]
        public IActionResult CreateNewDirectory([FromBody] NewDirectory fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;

            var fullPath = fileData.DirectoryPath + Path.DirectorySeparatorChar + fileData.DirectoryName;
            if (fileData.DirectoryLevel == 0 && fileData.DirectoryPath == "root")
            {
                fullPath = _fileSystemWatcher.Path + Path.DirectorySeparatorChar + fileData.DirectoryName;
            }

            Directory.CreateDirectory(fullPath);
            var relativePath = fullPath.Replace(_fileSystemWatcher.Path, String.Empty);

            var list = new List<IFileInfoNode>();
            var relativeSplitted = relativePath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).SkipLast(1);

            var dynamicRelativePath = string.Empty;
            var currentLevel = 0;
            foreach (var item in relativeSplitted)
            {
                dynamicRelativePath =
                        relativeSplitted.First() == item ? string.Empty : dynamicRelativePath + Path.DirectorySeparatorChar.ToString();
                dynamicRelativePath += item;

                var node = new FileInfoNode
                {
                    Name = item,
                    FullPath = _fileSystemWatcher.Path + Path.DirectorySeparatorChar + dynamicRelativePath,
                    RelativePath = dynamicRelativePath,
                    Path = dynamicRelativePath,
                    Type = "folder",
                    Level = currentLevel,
                };
                currentLevel++;
                list.Add(node);
            }
            var nodeFile = new FileInfoNode
            {
                Name = fileData.DirectoryName,
                FullPath = fullPath,
                Path = relativePath,
                RelativePath = relativePath,
                Type = "folder",
                Level = currentLevel,
            };
            list.Add(nodeFile);
            _fileSystemWatcher.EnableRaisingEvents = true;
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

        private (FileInfoNode, bool) CreateNodeFolder(string itemFolder)
        {
            var patchedItemFolfer = itemFolder.Substring(_fileSystemWatcher.Path.Length);
            var node = new FileInfoNode
            {
                Name = Path.GetFileName(itemFolder),
                FullPath = itemFolder,
                RelativePath =
                    patchedItemFolfer,
                Path = patchedItemFolfer,
                Type = "folder"
            };
            var isEmpty = ExploreNodes(node, itemFolder);
            return (node, isEmpty);
        }

        private FileInfoNode CreateNodeFolderOnly(string itemFolder)
        {
            var patchedItemFolfer = itemFolder;
            var node = new FileInfoNode
            {
                Name = Path.GetFileName(itemFolder),
                FullPath = itemFolder,
                Path = patchedItemFolfer,
                Type = "folder"
            };
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


public class NewFile
{
    public string Title { get; set; }
    public string DirectoryPath { get; set; }
    public int DirectoryLevel { get; set; }
    public int documentTypeId { get; set; }
}

public class NewDirectory
{
    public string DirectoryName { get; set; }
    public string DirectoryPath { get; set; }
    public int DirectoryLevel { get; set; }
}