using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using MdExplorer;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Entities.ProjectDB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Controllers;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.ProjectBody;
using MdExplorer.Features.Refactoring;
using MdExplorer.Features.Refactoring.Analysis;
using MdExplorer.Features.Refactoring.Analysis.Interfaces;
using MdExplorer.Features.snippets;
using MdExplorer.Features.Utilities;
using MdExplorer.Features.Yaml.Interfaces;
using MdExplorer.Features.Yaml.Models;
using MdExplorer.Hubs;
using MdExplorer.Models;
using MdExplorer.Service;
using MdExplorer.Service.Controllers;
using MdExplorer.Service.Controllers.MdFiles;
using MdExplorer.Service.Controllers.MdFiles.Models;
using MdExplorer.Service.Controllers.MdFiles.ModelsDto;
using MdExplorer.Service.Utilities;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using NHibernate.Linq;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static MdExplorer.Service.Controllers.RefactoringFilesController;
using static System.Net.WebRequestMethods;

namespace MdExplorer.Service.Controllers.MdFiles
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
        private readonly ProjectBodyEngine _projectBodyEngine;
        private readonly IYamlParser<MdExplorerDocumentDescriptor> _yamlDocumentManager;
        private readonly RefactoringManager _refactoringManager;
        private readonly ProcessUtil _visualStudioCode;

        public IEngineDB _engineDB { get; }

        public MdFilesController(FileSystemWatcher fileSystemWatcher,
            IEngineDB engineDB, IWorkLink[] getModifiers, IHelper helper,
            IUserSettingsDB userSettingsDB,
            IHubContext<MonitorMDHub> hubContext,
            IGoodMdRule<FileInfoNode>[] GoodRules,
            IProjectDB projectDB,
            ISnippet<DictionarySnippetParam>[] snippets,
            ProjectBodyEngine projectBodyEngine,
            IYamlParser<MdExplorerDocumentDescriptor> yamlDocumentManager,
            RefactoringManager refactoringManager,
            ProcessUtil visualStudioCode
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
            _projectBodyEngine = projectBodyEngine;
            _yamlDocumentManager = yamlDocumentManager;
            _refactoringManager = refactoringManager;
            _visualStudioCode = visualStudioCode;
        }

        [HttpGet]
        public IActionResult GetDocumentSettings([FromQuery] string fullPath)
        {
            var text = System.IO.File.ReadAllText(fullPath);
            var document = _yamlDocumentManager.GetDescriptor(text);
            return Ok(document);
        }

        [HttpPost]
        public IActionResult SetNumberedImage([FromBody] SetNumberedImageRequest data)
        {
            return Ok(data);
        }
        /// <summary>
        /// MdLink:
        /// </summary>
        /// <param name="mdFile"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult OpenFileInApplication([FromBody] OpenFileInApplicationcs data)
        {
            var fullpath = HttpUtility.UrlDecode(data.FullPath).Replace('/', Path.DirectorySeparatorChar);
            fullpath = fullpath.Replace("\\.\\", "\\");

            var processToStart = new ProcessStartInfo("cmd.exe", $"/c \"{fullpath}\"")
            {
                CreateNoWindow = false
            };
            Process.Start(processToStart);
            return Ok(new { message = "done" });
        }

        [HttpPost]
        public IActionResult MoveMdFile([FromBody] RequestMoveMdFile requestMoveMdFile)
        {
            var projectBasePath = _fileSystemWatcher.Path;
            var fromRelativePathFileName = requestMoveMdFile.MdFile.RelativePath.Substring(1);
            var fromFullPathFileName = Path.Combine(projectBasePath, fromRelativePathFileName);

            var fileName = requestMoveMdFile.MdFile.Name;
            var relativeDestinationPath = requestMoveMdFile.DestinationPath
                                    .Replace(_fileSystemWatcher.Path, "").Substring(1);
            var toRelativePathFileName = Path.Combine(relativeDestinationPath, fileName);
            var toFullPathFileName = Path.Combine(_fileSystemWatcher.Path, toRelativePathFileName);
            var newFullPath = Path.Combine(requestMoveMdFile.DestinationPath, fileName);
            MoveFileOnFilesystem(fromFullPathFileName, toFullPathFileName);

            _engineDB.BeginTransaction();
            _refactoringManager.RenameTheMdFileIntoEngineDB(projectBasePath,
                fromRelativePathFileName, toRelativePathFileName);

            var refSourceAct = _refactoringManager
                .SaveRefactoringActionForMoveFile(fileName,
                Path.GetDirectoryName(fromFullPathFileName),
                requestMoveMdFile.DestinationPath); // Save the concept of change



            _refactoringManager.SetRefactoringInvolvedLinksActionsForMoveFile(
                //fromRelativePathFileName, 
                toRelativePathFileName,
                projectBasePath, //requestMoveMdFile.MdFile.FullPath, 
                refSourceAct);
            // After save, get back the list of links inside involved files
            _refactoringManager.UpdateAllInvolvedFilesAndReferencesToDB(refSourceAct); //newFullPath,

            _engineDB.Commit();
            return Ok("");
        }

        private void MoveFileOnFilesystem(string oldFullPath, string newFullPath)
        {

            // gestisci il rename di un file
            System.IO.File.Move(oldFullPath, newFullPath, true);
            if (_visualStudioCode.CurrentVisualStudio != null)
            {
                _visualStudioCode.ReopenVisualStudioCode(newFullPath);
            }
        }

        [HttpPost]
        public IActionResult OpenInheritingTemplateWord([FromBody] RequestOpenInheritingTemplateWord request)
        {
            var templatePath = _fileSystemWatcher.Path +
                Path.DirectorySeparatorChar +
                ".md" +
                Path.DirectorySeparatorChar +
                "templates" +
                Path.DirectorySeparatorChar +
                "word" +
                Path.DirectorySeparatorChar +
                $"{request.TemplateName}.docx";
            var processToStart = new ProcessStartInfo("cmd.exe", $"/c \"{templatePath}\"")
            {
                CreateNoWindow = false
            };
            Process.Start(processToStart);
            return Ok(new { message = "done" });
        }

        [HttpGet]
        public IActionResult getTextFromClipboard()
        {
            var textToGet = string.Empty;

            var myTask = ExtensionTask.CreateSTATask(async () =>
            {
                textToGet = Clipboard.GetText();
            });
            myTask.Wait();

            textToGet = textToGet.Contains("http") && textToGet.Contains("git") ? textToGet : string.Empty;

            return Ok(new { url = textToGet });
        }

        [HttpPost]
        public IActionResult PasteFromClipboard([FromBody] RequestPasteFromClipboard fileData)
        {
            Thread t = new Thread(new ThreadStart(() =>
            {
                //DataObject data = new DataObject();
                //data.SetData(typeof(string), "SampleText");
                //Clipboard.SetDataObject(data);
                //string text = ((DataObject)Clipboard.GetDataObject()).GetText();
                //IDataObject test = Clipboard.GetDataObject();
                if (Clipboard.ContainsImage())
                {
                    _fileSystemWatcher.EnableRaisingEvents = false;
                    var ruleReg = new Regex("(^(PRN|AUX|NUL|CON|COM[1-9]|LPT[1-9]|(\\.+)$)(\\..*)?$)|(([\\x00-\\x1f\\\\?*:\";‌​|/<>])+)|([\\. ]+)");
                    var title = ruleReg.Replace(fileData.FileName, "-").Replace(" ", "-") + ".png";

                    var imageToSave = Clipboard.GetImage();

                    var currentDirectory = string.Concat(Path.GetDirectoryName(fileData.FileInfoNode.FullPath),
                                                            Path.DirectorySeparatorChar,
                                                            "assets");
                    Directory.CreateDirectory(currentDirectory);
                    var currentImageToSave = currentDirectory + Path.DirectorySeparatorChar + title;
                    imageToSave.Save(currentImageToSave);
                    var allText = System.IO.File.ReadAllText(fileData.FileInfoNode.FullPath);
                    //We have to set an absolute path
                    var relativePathMDE = fileData.FileInfoNode.Path.Replace(fileData.FileInfoNode.Name,string.Empty).Replace("\\","/");
                    var newLineTextToAdd = @$"![{fileData.FileName}]({relativePathMDE}assets/{title})";
                    allText = string.Concat(allText, Environment.NewLine, newLineTextToAdd);
                    _fileSystemWatcher.EnableRaisingEvents = true;
                    System.IO.File.WriteAllText(fileData.FileInfoNode.FullPath, allText);
                }

            }));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            return Ok(new { message = "done" });
        }


        [HttpPost]
        public IActionResult OpenCustomWordTemplate([FromBody] FileInfoNode fileData)
        {
            // copy reference.docx
            var fromReference = _fileSystemWatcher.Path +
                Path.DirectorySeparatorChar +
                ".md" +
                Path.DirectorySeparatorChar +
                "templates" +
                Path.DirectorySeparatorChar +
                "word" +
                Path.DirectorySeparatorChar +
                "reference.docx";
            var toReference = Path.GetDirectoryName(fileData.FullPath) +
                Path.DirectorySeparatorChar +
                "assets";
            Directory.CreateDirectory(toReference);
            toReference += Path.DirectorySeparatorChar +
                    fileData.Name +
                    ".reference.docx";

            if (!System.IO.File.Exists(toReference))
            {
                System.IO.File.Copy(fromReference, toReference);
            }
            var processToStart = new ProcessStartInfo("cmd.exe", $"/c \"{toReference}\"")
            {
                CreateNoWindow = false
            };
            Process.Start(processToStart);

            //var p = new Process();
            //p.StartInfo.FileName = toReference;
            //p.StartInfo.FileName ="WinWord.exe" ;
            //p.Start();
            return Ok(new { message = "done" });
        }



        [HttpPost]
        public IActionResult SetDocumentSettings([FromBody] ChangeDocumentSettings data)
        {
            var descriptor = data.DocumentDescriptor;
            var mdFile = data.MdFile;
            var text = System.IO.File.ReadAllText(mdFile.FullPath);
            var textChanged = _yamlDocumentManager.SetDescriptor(descriptor, text);
            System.IO.File.WriteAllText(mdFile.FullPath, textChanged);
            return Ok(new { message = "done" });
        }

        [HttpGet]
        public IActionResult GetLandingPage()
        {
            var dal = _projectDB.GetDal<ProjectSetting>();
            var landingPage = dal.GetList().Where(_ => _.Name == "LandingPageFilePath").First();

            var toReturn = landingPage.LandingPages.Count() != 0 ? new FileInfoNode
            {
                Expandable = landingPage.LandingPages.First().Expandable,
                FullPath = landingPage.LandingPages.First().FullPath,
                Level = landingPage.LandingPages.First().Level,
                Name = landingPage.LandingPages.First().Name,
                Path = landingPage.LandingPages.First().Path,
                RelativePath = landingPage.LandingPages.First().RelativePath,
                Type = landingPage.LandingPages.First().Type,
                DataText = landingPage.LandingPages.First().DataText,
            } : null;
            return Ok(toReturn);
        }


        [HttpPost]
        public IActionResult OpenFolderOnFileExplorer([FromBody] FileInfoNode fileData)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = fileData.FullPath,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
            return Ok(new { message = "done" });
        }

        [HttpPost]
        public IActionResult DeleteFile([FromBody] FileInfoNode fileData)
        {
            System.IO.File.Delete(fileData.FullPath);
            return Ok(new { message = "done" });
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
            }
            else
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
            return Ok(new { message = "Done" });
        }

        [HttpGet]
        public IActionResult GetDynFoldersDocument([FromQuery] string path, string level)
        {
            var currentPath = path == "root" ? @"C:\" : path;
            currentPath = path == "project" ? _fileSystemWatcher.Path : currentPath;
            currentPath = path == "documents" ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : currentPath;

            var currentLevel = Convert.ToInt32(level);
            var list = new List<IFileInfoNode>();

            //.Where(_ => !_.Contains("Immagini") &&
            //        !_.Contains("Musica") &&
            //        !_.Contains("Video"))
            //        )
            foreach (var itemFolder in Directory.GetDirectories(currentPath))
            {
                if (!IsSymbolic(itemFolder) && !IsHidden(itemFolder))
                {
                    try
                    {
                        var node = new FileInfoNode
                        {
                            Name = Path.GetFileName(itemFolder),
                            FullPath = itemFolder,
                            Path = itemFolder,
                            Level = currentLevel,
                            Type = "folder",
                            Expandable = Directory.GetDirectories(itemFolder).Count() > 0
                        };

                        list.Add(node);
                    }
                    catch (Exception)
                    {

                        // Do nothing
                    }

                }


            }


            return Ok(list);
        }

        private bool IsSymbolic(string path)
        {
            try
            {
                FileInfo pathInfo = new FileInfo(path);
                return pathInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
            }
            catch (Exception)
            {

                return true;
            }

        }

        private bool IsHidden(string path)
        {
            try
            {
                FileInfo pathInfo = new FileInfo(path);
                return pathInfo.Attributes.HasFlag(FileAttributes.Hidden);
            }
            catch (Exception ex)
            {
                return true;

            }
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
        private string signalRConnectionId;
        [HttpGet]
        public async Task<IActionResult> GetAllMdFiles(string connectionId)
        {
            signalRConnectionId = connectionId;

            await _hubContext.Clients.Client(connectionId: connectionId).SendAsync("parsingProjectStart", "process started");

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
                _hubContext.Clients.Client(connectionId: signalRConnectionId)
                    .SendAsync("indexingFolder", itemFolder).Wait();
                (var node, var isempty) = CreateNodeFolder(itemFolder);
                if (!isempty)
                {
                    list.Add(node);
                }
            }

            foreach (var itemFile in Directory.GetFiles(currentPath).Where(_ => Path.GetExtension(_) == ".md"))
            {
                var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
                var node = _projectBodyEngine.CreateNodeMdFile(itemFile, patchedItemFile);
                list.Add(node);
            }

            _hubContext.Clients.Client(connectionId: connectionId)
                    .SendAsync("indexingFolder", "deleting database").Wait();
            // nettificazione dei folder che non contengono md            
            _engineDB.BeginTransaction();
            _engineDB.Delete("from LinkInsideMarkdown");
            _engineDB.Flush();
            _engineDB.Delete("from MarkdownFile");
            _engineDB.Flush();

            _hubContext.Clients.Client(connectionId: connectionId)
                    .SendAsync("indexingFolder", "creating database").Wait();
            SaveRealationships(list);
            _engineDB.Commit();

            GC.Collect();
            var nodeempty = new FileInfoNode
            {
                Name = "root",
                FullPath = currentPath,
                Path = currentPath,
                //Level = currentLevel,
                Type = "emptyroot",
                Expandable = false
            };

            list.Add(nodeempty);            
            await _hubContext.Clients.Client(connectionId: connectionId)
                    .SendAsync("parsingProjectStop", "process completed");
            return Ok(list);
        }

        [HttpPost]
        public IActionResult CloneTimerMd([FromBody] FileInfoNode fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;

            var allText = System.IO.File.ReadAllText(fileData.FullPath);
            string destFullPath, destRelativePath;
            (allText, destFullPath, destRelativePath) = PrepareClone(fileData, allText);
            System.IO.File.WriteAllText(destFullPath, allText);
            // Devo preparare il nuovo file di risposta
            List<IFileInfoNode> list = PrepareListToGetBack(Path.GetFileName(destFullPath), destFullPath, destRelativePath);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(list);
        }

        private (string, string, string) PrepareClone(FileInfoNode fileData, string allText)
        {
            var destFullPath = fileData.FullPath;
            var destRelativePath = fileData.RelativePath;
            var fileName = Path.GetFileName(fileData.FullPath);

            var stringDatePattern = new Regex(@"([0-9][0-9][0-9][0-9][-][0-9][0-9][-][0-9][0-9]).*");
            var matchesOnFileName = stringDatePattern.Matches(fileName);
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");

            foreach (Match item in matchesOnFileName)
            {
                destFullPath = destFullPath.Replace(item.Groups[1].Value, currentDate);
                destRelativePath = destRelativePath.Replace(item.Groups[1].Value, currentDate);
                var getTitle = new Regex(@"(#.+?)([0-9][0-9][0-9][0-9][-][0-9][0-9][-][0-9][0-9])(.*)");
                var match = getTitle.Match(allText);
                var firstPart = match.Groups[1].Value;
                var dateToReplace = match.Groups[2].Value;
                var lastPart = match.Groups[3].Value;
                var finalRowString = firstPart + currentDate + lastPart;
                allText = allText.Replace(match.Groups[0].Value, finalRowString);
            }

            return (allText, destFullPath, destRelativePath);
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
            dictParam.Add(ParameterName.DocumentType, fileData.DocumentType);
            templateContent = snippetTextDocument.GetSnippet(dictParam);


            // Additional Template 
            var snippet = _snippets.Where(_ => _.Id == fileData.documentTypeId && _.Id != 0).FirstOrDefault();
            if (snippet != null)
            {
                snippet.SetAssets(fullPath);
                var addtionalTemplateContent = snippet.GetSnippet(dictParam);
                templateContent = string.Concat(templateContent, addtionalTemplateContent);
            }

            // write content
            var relativePath = fullPath.Replace(_fileSystemWatcher.Path, string.Empty);
            System.IO.File.WriteAllText(fullPath, templateContent);


            // prepare data to raise back
            List<IFileInfoNode> list = PrepareListToGetBack(title, fullPath, relativePath);
            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(list);
        }

        private List<IFileInfoNode> PrepareListToGetBack(string title, string fullPath, string relativePath)
        {
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
            return list;
        }

        [HttpPost]
        public IActionResult RenameDirectory([FromBody] RenameDirectory fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var fullPath = fileData.DirectoryPath + Path.DirectorySeparatorChar + fileData.DirectoryName;

            var relativePath = fullPath.Replace(_fileSystemWatcher.Path, string.Empty);

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

        [HttpPost]
        public IActionResult CreateNewDirectoryEx([FromBody] NewDirectory fileData)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            var fullPath = fileData.DirectoryPath + Path.DirectorySeparatorChar + fileData.DirectoryName;
            Directory.CreateDirectory(fullPath);

            _fileSystemWatcher.EnableRaisingEvents = true;
            return Ok(fileData);
        }

        [HttpGet]
        public IActionResult GetBookmarks(string projectId)
        {
            _userSettingsDB.BeginTransaction();
            var bookmarkDal = _userSettingsDB.GetDal<Bookmark>();
            var guidProjectId = new Guid(projectId);
            var bookmarkList = bookmarkDal.GetList()
                .Where(_ => _.Project.Id == guidProjectId).Select(_ =>
                     new { _.Id, _.Name, _.FullPath, ProjectId = _.Project.Id }
                ).ToList();
            _userSettingsDB.Commit();

            return Ok(bookmarkList);
        }

        [HttpPost]
        public IActionResult ToggleBookmark([FromBody] ToggleBookmarkRequest request)
        {
            _userSettingsDB.BeginTransaction();
            var bookmarkDal = _userSettingsDB.GetDal<Bookmark>();
            var bookmarkDb= bookmarkDal.GetList().Where(_ => _.Project.Id == request.ProjectId 
                                                        && _.FullPath == request.FullPath).FirstOrDefault();
            if (bookmarkDb != null)
            {
                bookmarkDal.Delete(bookmarkDb);
            }
            else
            {
                var projectDal = _userSettingsDB.GetDal<Project>();
                var currentProject = projectDal.GetList().Where(_ => _.Id == request.ProjectId).FirstOrDefault();
                // Ok, pay attention, here we are managing TOGGLE
                var bookmark = new Bookmark { FullPath = request.FullPath, Name = request.Name, Project = currentProject };
                currentProject.Bookmarks.Add(bookmark);
                projectDal.Save(currentProject);
                
            }
            _userSettingsDB.Commit();

            return Ok(request);
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
            var relativePath = fullPath.Replace(_fileSystemWatcher.Path, string.Empty);

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
            var linkInsideMarkdownDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var relDal = _engineDB.GetDal<MarkdownFile>();

            foreach (var item in list)
            {
                var markdownFile = new MarkdownFile
                {
                    FileName = item.Name,
                    Path = item.FullPath,
                    FileType = "File"
                };
                if (item.Childrens.Count > 0)
                {
                    _hubContext.Clients.Client(connectionId: signalRConnectionId)
                    .SendAsync("indexingFolder", markdownFile.FileName).Wait();
                    markdownFile.FileType = "Folder";
                    SaveRealationships(item.Childrens, markdownFile.Id);
                }
                relDal.Save(markdownFile);
                SaveLinksFromMarkdown(item, markdownFile, linkInsideMarkdownDal);
            }

        }

        private void SaveLinksFromMarkdown(IFileInfoNode item,
            MarkdownFile relationship,
            IDAL<LinkInsideMarkdown> linkInsideMarkdownDal)
        {
            foreach (var getModifier in _getModifiers)
            {
                var linksToStore = relationship.FileType == "File" ? getModifier.GetLinksFromFile(item.FullPath) : new List<LinkDetail>().ToArray();
                foreach (var singleLink in linksToStore)
                {
                    // manage relative path
                    var fullPath = Path.GetDirectoryName(item.FullPath)
                        + Path.DirectorySeparatorChar
                        + singleLink.LinkPath.Replace('/', Path.DirectorySeparatorChar);

                    // manage absolute path in link
                    if (singleLink.LinkPath.StartsWith("/"))
                    {
                        fullPath = _fileSystemWatcher.Path
                            + singleLink.LinkPath.Replace('/', Path.DirectorySeparatorChar);
                    }

                    var normalizedFullPath = _helper.NormalizePath(fullPath);
                    
                    var context = Path.GetDirectoryName(relationship.Path)
                        .Replace(_fileSystemWatcher.Path, string.Empty)
                        .Replace(Path.DirectorySeparatorChar, '/');
                    var linkToStore = new LinkInsideMarkdown
                    {
                        FullPath = normalizedFullPath,
                        Path = singleLink.LinkPath,
                        MdTitle = singleLink.MdTitle,
                        HTMLTitle = singleLink.HTMLTitle,
                        Source = getModifier.GetType().Name,
                        LinkedCommand = singleLink.LinkedCommand,
                        SectionIndex = singleLink.SectionIndex,
                        MarkdownFile = relationship,
                        MdContext = context,
                    };
                    linkInsideMarkdownDal.Save(linkToStore);
                }
            }

        }

        //private FileInfoNode CreateNodeMdFile(string itemFile)
        //{
        //    var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
        //    var node = new FileInfoNode
        //    {
        //        Name = Path.GetFileName(itemFile),
        //        FullPath = itemFile,
        //        Path = patchedItemFile,
        //        RelativePath = patchedItemFile,
        //        Type = "mdFile"
        //    };
        //    return node;
        //}

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
                var accessControlList = new DirectoryInfo(pathFile).GetAccessControl();
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
                var patchedItemFile = itemFile.Substring(_fileSystemWatcher.Path.Length);
                var node = _projectBodyEngine.CreateNodeMdFile(itemFile, patchedItemFile);
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
    public string DocumentType { get; set; }
}

public class NewDirectory
{
    public string DirectoryName { get; set; }
    public string DirectoryPath { get; set; }
    public int DirectoryLevel { get; set; }
}

public class RenameDirectory
{
    public string DirectoryName { get; set; }
    public string DirectoryPath { get; set; }
    public int DirectoryLevel { get; set; }
}