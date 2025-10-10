using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Commands.GitHub
{
    /// <summary>
    /// GitHub-compatible version of PlantUML command
    /// In GitHub mode, PlantUML blocks are converted to standard code blocks
    /// or optionally to PNG images stored in .md/ folder with relative links
    /// </summary>
    public class FromPlantumlToPngGitHub : ICommand, IDisposable
    {
        protected readonly ILogger<FromPlantumlToPngGitHub> _logger;
        private readonly IUserSettingsDB _session;
        protected readonly PlantumlServer _plantumlServer;
        protected readonly IHelper _helper;

        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 20;
        public string Name { get; set; } = "FromPlantumlToPngGitHub";

        public FromPlantumlToPngGitHub(
            ILogger<FromPlantumlToPngGitHub> logger,
            IUserSettingsDB session,
            PlantumlServer plantumlServer,
            IHelper helper)
        {
            _logger = logger;
            _session = session;
            _plantumlServer = plantumlServer;
            _helper = helper;
        }

        public void Dispose()
        {
            _session.Dispose();
        }

        public MatchCollection GetMatches(string markdown)
        {
            Regex rx = new Regex(@"```plantuml([^```]*)```",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = rx.Matches(markdown);
            return matches;
        }

        public MatchCollection GetMatchesAfterConversion(string html)
        {
            // Not used in GitHub mode
            return null;
        }

        public virtual string TransformAfterConversion(string html, RequestInfo requestInfo)
        {
            // No transformation needed in HTML for GitHub mode
            return html;
        }

        /// <summary>
        /// In GitHub mode, converts PlantUML blocks to:
        /// 1. Generate PNG file in .md/ folder
        /// 2. Replace with standard markdown image link (GitHub compatible)
        /// </summary>
        public virtual string TransformInNewMDFromMD(string markdown, RequestInfo requestInfo)
        {
            _logger.LogInformation($"[PlantUML GitHub] TransformInNewMDFromMD called - GitHub compatibility mode");
            var directoryInfo = Directory.CreateDirectory(requestInfo.CurrentRoot + $"{Path.DirectorySeparatorChar}.md");
            string backPath = _helper.GetBackPath(requestInfo);

            _logger.LogDebug($"[PlantUML GitHub] CurrentRoot: '{requestInfo.CurrentRoot}'");
            _logger.LogDebug($"[PlantUML GitHub] BackPath: '{backPath}'");

            // Fix backPath for subdirectories
            var pathSegments = requestInfo.CurrentQueryRequest.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            var fileDepth = pathSegments.Length - 1;

            if (fileDepth > 0 && backPath.StartsWith($".{Path.DirectorySeparatorChar}"))
            {
                var upLevels = string.Join(Path.DirectorySeparatorChar.ToString(), Enumerable.Repeat("..", fileDepth));
                backPath = $"{upLevels}{Path.DirectorySeparatorChar}.md";
                _logger.LogDebug($"[PlantUML GitHub] Corrected backPath: '{backPath}'");
            }

            Directory.SetCurrentDirectory(Path.GetDirectoryName(requestInfo.AbsolutePathFile));

            var matches = GetMatches(markdown);
            foreach (Match item in matches)
            {
                var text = item.Groups[1].Value;
                var textHash = _helper.GetHashString(text, Encoding.UTF8);
                var filePath = $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{textHash}.png";

                if (!File.Exists(filePath))
                {
                    try
                    {
                        var taskPng = _plantumlServer.GetPngFromJar(text);
                        taskPng.Wait();
                        var res = taskPng.Result;
                        File.WriteAllBytes(filePath, res);
                        _logger.LogInformation($"[PlantUML GitHub] Generated PNG: {filePath}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[PlantUML GitHub] Failed to generate PNG for diagram");
                        // If PNG generation fails, keep the PlantUML code block
                        continue;
                    }
                }

                // Create GitHub-compatible markdown image link
                var markdownFilePath = $"{backPath}{Path.DirectorySeparatorChar}{textHash}.png";
                var referenceUrl = $@"![PlantUML Diagram]({markdownFilePath.Replace(Path.DirectorySeparatorChar, '/')})";

                _logger.LogInformation($"[PlantUML GitHub] Replacing with GitHub-compatible image: {referenceUrl}");
                markdown = markdown.Replace(item.Groups[0].Value, referenceUrl);
            }

            Directory.SetCurrentDirectory(Path.GetDirectoryName(requestInfo.CurrentRoot));

            return markdown;
        }

        public string PrepareMetadataBasedOnMD(string markdown, RequestInfo requestInfo)
        {
            // Do nothing
            return markdown;
        }
    }
}
