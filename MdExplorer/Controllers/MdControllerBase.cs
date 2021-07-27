using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
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

namespace MdExplorer.Service.Controllers
{
    public class MdControllerBase<T>: ControllerBase
    {
        protected readonly ILogger<T> _logger;
        protected readonly FileSystemWatcher _fileSystemWatcher;
        protected readonly IOptions<MdExplorerAppSettings> _options;
        protected readonly IHubContext<MonitorMDHub> _hubContext;
        protected readonly ISessionDB _session;
        protected readonly ICommandRunner _commandRunner;

        public MdControllerBase(ILogger<T> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB session,
            ICommandRunner commandRunner)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            this._options = options;
            _hubContext = hubContext;
            _session = session;
            _commandRunner = commandRunner;
        }

        protected string GetRelativePathFileSystem(string controllerName)
        {
            //mdexplorer
            return HttpUtility.UrlDecode(Request.Path.ToString().Replace($"/api/{controllerName}/", string.Empty).Replace('/', Path.DirectorySeparatorChar));
        }

    }
}
