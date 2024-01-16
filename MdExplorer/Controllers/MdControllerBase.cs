using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Interfaces;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Interfaces;
using MdExplorer.Hubs;
using MdExplorer.Service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NHibernate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Features.Utilities;
using NHibernate.Util;

namespace MdExplorer.Service.Controllers
{
    public class MdControllerBase<T>: ControllerBase
    {
        protected readonly ILogger<T> _logger;
        protected readonly FileSystemWatcher _fileSystemWatcher;
        protected readonly IOptions<MdExplorerAppSettings> _options;
        protected readonly IHubContext<MonitorMDHub> _hubContext;
        protected readonly IUserSettingsDB _userSettingsDB;
        protected readonly ICommandRunner _commandRunner;
        protected readonly IWorkLink[] _getModifiers;
        protected readonly IHelper _helper;

        public MdControllerBase(ILogger<T> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingDB,
            IEngineDB engineDB,
            ICommandRunner commandRunner,
            IWorkLink[] getModifiers,
            IHelper helper)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            this._options = options;
            _hubContext = hubContext;
            _userSettingsDB = userSettingDB;
            _engineDB = engineDB;
            _commandRunner = commandRunner;
            _getModifiers = getModifiers;
            _helper = helper;
        }

        public IEngineDB _engineDB { get; }

        protected string GetRelativePathFileSystem(string controllerName)
        {
            //mdexplorer
            return HttpUtility.UrlDecode(Request.Path.ToString().Replace($"/api/{controllerName}/", string.Empty).Replace('/', Path.DirectorySeparatorChar));
        }

        protected void SaveLinksFromMarkdown(MarkdownFile relationship)
        {
            var linkInsideMarkdownDal = _engineDB.GetDal<LinkInsideMarkdown>();
            foreach (var getModifier in _getModifiers)
            {
                var linksToStore = relationship.FileType == "File" ? getModifier.GetLinksFromFile(relationship.Path) : new List<LinkDetail>().ToArray();
                foreach (var singleLink in linksToStore)
                {
                    // manage relative path
                    var fullPath = Path.GetDirectoryName(relationship.Path)
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
                    LinkInsideMarkdown linkToStore = linkInsideMarkdownDal.GetList()
                        .Where(_=>_.FullPath == normalizedFullPath 
                            && _.Source == getModifier.GetType().Name 
                            && _.HTMLTitle == singleLink.HTMLTitle
                            && _.MdTitle == singleLink.MdTitle 
                            && _.LinkedCommand == singleLink.LinkedCommand
                            && _.MarkdownFile.Id == relationship.Id).FirstOrDefault();
                    if (linkToStore == null)
                    {
                        linkToStore = new LinkInsideMarkdown
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
                    }
                    else // This is update, changing only what is not key
                    {
                        linkToStore.MdTitle = singleLink.MdTitle;
                        linkToStore.HTMLTitle = singleLink.HTMLTitle;
                        linkToStore.LinkedCommand = singleLink.LinkedCommand;
                        linkToStore.SectionIndex = singleLink.SectionIndex;
                    }
                     
                    linkInsideMarkdownDal.Save(linkToStore);
                }
            }

        }

    }
}
