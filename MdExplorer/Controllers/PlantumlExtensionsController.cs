using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Interfaces;
using MdExplorer.Features.Interfaces.ICommandsSpecificContext;
using MdExplorer.Hubs;
using MdExplorer.Service.Models;
using MdExplorer.Services.DatabaseManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("/api/plantumlextensions/{action}")]
    public class PlantumlExtensionsController : MdControllerBase<PlantumlExtensionsController>
    {
        private readonly ICommandFactoryHtml _commandFactory;

        public PlantumlExtensionsController(
            ICommandFactoryHtml commandFactory,
            ILogger<PlantumlExtensionsController> logger,
            IOptions<MdExplorerAppSettings> options,
            IHubContext<MonitorMDHub> hubContext,
            IUserSettingsDB userSettingsDB,
            IEngineDB engineDB,
            IDatabaseManager databaseManager = null)
            : base(logger, options, hubContext, userSettingsDB, engineDB,
                  databaseManager: databaseManager)
        {
            _commandFactory = commandFactory;
        }

        [HttpGet]
        public IActionResult GetPng(string pathFile, string hashFile, int step)
        {

            (var requestInfo, var markdown) = GetMarkDown(pathFile);
            var render = (IPresentationPlantuml)_commandFactory.GetCommands().Where(_ => _.Name == "FromPlantumlToPng").FirstOrDefault();
            var generatedFileName = render.GetPng(markdown, hashFile, step, requestInfo);

            return Ok(new { GeneratedFileName = generatedFileName });
        }

        private (RequestInfo, string) GetMarkDown(string pathFile)
        {
            var projectPath = GetProjectPath();
            var rootPathSystem = $"{projectPath}{Path.DirectorySeparatorChar}";
            var relativePathFileSystem = pathFile;
            var relativePathExtension = Path.GetExtension(relativePathFileSystem);
            var filePathSystem1 = string.Empty;

            if (relativePathExtension == ".md")
            {
                filePathSystem1 = string.Concat(rootPathSystem, relativePathFileSystem);
            }
            else
            {
                filePathSystem1 = string.Concat(rootPathSystem, relativePathFileSystem, ".md");
            }

            var requestInfo = new RequestInfo()
            {
                CurrentQueryRequest = relativePathFileSystem,
                CurrentRoot = projectPath,
                AbsolutePathFile = filePathSystem1,
            };

            var markdown = System.IO.File.ReadAllText(filePathSystem1);
            return (requestInfo, markdown);
        }

        [HttpGet]
        public IActionResult PresentationSVG(string pathFile, string hashFile, int step)
        {

            (var requestInfo, var markdown) = GetMarkDown(pathFile);
            var render = (IPresentationPlantuml)_commandFactory.GetCommands().Where(_ => _.Name == "FromPlantumlToPng").FirstOrDefault();
            (var generatedFileName, var totalStep ) = render.GetPresentationSvg(markdown, hashFile, step, requestInfo);

            return Ok(new { GeneratedFileName = "/api/mdexplorer/" + generatedFileName, TotalStep = totalStep });
        }

    }
}
