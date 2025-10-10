using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Configuration;
using MdExplorer.Features.Configuration.Models;
using MdExplorer.Features.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class CommandRunner : ICommandRunner
    {
        private readonly ICommand[] _commands;
        private readonly ICompatibilityModeService _compatibilityService;
        private readonly ILogger<CommandRunner> _logger;

        public CommandRunner(
            ICommand[] commands,
            ICompatibilityModeService compatibilityService,
            ILogger<CommandRunner> logger)
        {
            _commands = commands;
            _compatibilityService = compatibilityService;
            _logger = logger;
        }

        private bool IsCommandCompatible(ICommand command)
        {
            // If command has no SupportedModes specified (null or empty), it supports all modes
            if (command.SupportedModes == null || command.SupportedModes.Count == 0)
            {
                return true;
            }

            var currentMode = _compatibilityService.GetMode();
            return command.SupportedModes.Contains(currentMode);
        }

        private IEnumerable<ICommand> GetCompatibleCommands()
        {
            var currentMode = _compatibilityService.GetMode();

            return _commands
                .OrderBy(_ => _.Priority)
                .Where(_ => _.Enabled)
                .Where(cmd =>
                {
                    var isCompatible = IsCommandCompatible(cmd);
                    if (!isCompatible)
                    {
                        _logger.LogDebug($"⏭️ [CommandRunner] Skipping command '{cmd.Name}' - not compatible with {currentMode} mode");
                    }
                    return isCompatible;
                });
        }

        public string TransformInNewMDFromMD(string markdownText, RequestInfo requestInfo)
        {
            foreach (var item in GetCompatibleCommands())
            {
                try
                {
                    markdownText = item.TransformInNewMDFromMD(markdownText, requestInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"⚠️ [CommandRunner] Command '{item.Name}' failed in TransformInNewMDFromMD - continuing with next command");
                }
            }
            return markdownText;
        }

        public ICommand[] GetAllCommands()
        {
            return _commands;
        }

        public string TransformAfterConversion(string markdownText, RequestInfo requestInfo)
        {
            foreach (var item in GetCompatibleCommands())
            {
                try
                {
                    markdownText = item.TransformAfterConversion(markdownText, requestInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"⚠️ [CommandRunner] Command '{item.Name}' failed in TransformAfterConversion - continuing with next command");
                }
            }
            return markdownText;
        }

        public string PrepareMetadataBasedOnMD(string markdownText, RequestInfo requestInfo)
        {
            foreach (var item in GetCompatibleCommands())
            {
                try
                {
                    markdownText = item.PrepareMetadataBasedOnMD(markdownText, requestInfo);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"⚠️ [CommandRunner] Command '{item.Name}' failed in PrepareMetadataBasedOnMD - continuing with next command");
                }
            }
            return markdownText;
        }



    }
}
