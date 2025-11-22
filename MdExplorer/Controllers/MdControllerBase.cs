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
using MdExplorer.Services.DatabaseManager;
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
        protected readonly IDatabaseManager _databaseManager;

        public MdControllerBase(ILogger<T> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingDB,
            IEngineDB engineDB,
            ICommandRunner commandRunner,
            IWorkLink[] getModifiers,
            IHelper helper,
            IDatabaseManager databaseManager = null)
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
            _databaseManager = databaseManager;
        }

        public IEngineDB _engineDB { get; }

        /// <summary>
        /// Gets database context for the current client based on ConnectionId.
        /// Falls back to injected _engineDB if DatabaseManager is not available.
        /// </summary>
        protected ConnectionDatabaseContext GetDatabaseContext()
        {
            if (_databaseManager == null)
            {
                // Backward compatibility: use injected _engineDB
                return null;
            }

            var connectionId = Request.Query["ConnectionId"].ToString();
            if (string.IsNullOrEmpty(connectionId))
            {
                _logger?.LogWarning("⚠️ ConnectionId not provided in request query");
                return null;
            }

            try
            {
                return _databaseManager.GetContext(connectionId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to get database context for connection {connectionId}");
                return null;
            }
        }

        /// <summary>
        /// Gets EngineDB for current client. Uses DatabaseManager if available, otherwise falls back to injected _engineDB.
        /// </summary>
        protected IEngineDB GetEngineDB()
        {
            var context = GetDatabaseContext();
            return context?.EngineDB ?? _engineDB;
        }

        /// <summary>
        /// Gets ProjectDB for current client. Only available when using DatabaseManager.
        /// </summary>
        protected IProjectDB GetProjectDB()
        {
            var context = GetDatabaseContext();
            return context?.ProjectDB;
        }

        protected string GetRelativePathFileSystem(string controllerName)
        {
            //mdexplorer
            return HttpUtility.UrlDecode(Request.Path.ToString().Replace($"/api/{controllerName}/", string.Empty).Replace('/', Path.DirectorySeparatorChar));
        }

        protected void SaveLinksFromMarkdown(MarkdownFile relationship)
        {
            if (relationship==null)
            {
                return;
            }
            var linkInsideMarkdownDal = _engineDB.GetDal<LinkInsideMarkdown>();
            foreach (var getModifier in _getModifiers)
            {
                var linksToStore = relationship.FileType == "File" ? getModifier.GetLinksFromFile(relationship.Path) : new List<LinkDetail>().ToArray();
                foreach (var singleLink in linksToStore)
                {
                    // manage relative path
                    var fullPath = Path.GetDirectoryName(relationship.Path)
                        + Path.DirectorySeparatorChar
                        + singleLink.FullPath.Replace('/', Path.DirectorySeparatorChar);

                    // manage absolute path in link
                    if (singleLink.FullPath.StartsWith("/"))
                    {
                        fullPath = _fileSystemWatcher.Path
                            + singleLink.FullPath.Replace('/', Path.DirectorySeparatorChar);
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
                            Path = singleLink.FullPath,
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
