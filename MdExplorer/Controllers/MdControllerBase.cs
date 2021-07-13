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
        protected readonly ISession _session;
        protected readonly ICommandRunner _commandRunner;

        public MdControllerBase(ILogger<T> logger,
            FileSystemWatcher fileSystemWatcher,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            ISession session,
            ICommandRunnerPdf commandRunner)
        {
            _logger = logger;
            _fileSystemWatcher = fileSystemWatcher;
            this._options = options;
            _hubContext = hubContext;
            _session = session;
            _commandRunner = commandRunner;
        }      


    }
}
