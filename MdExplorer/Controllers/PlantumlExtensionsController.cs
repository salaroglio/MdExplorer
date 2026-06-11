using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Features.Commands;
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
using MdExplorer.Utilities;
using System.Web;

namespace MdExplorer.Service.Controllers
{
    [ApiController]
    [Route("/api/plantumlextensions/{action}")]
    public class PlantumlExtensionsController : MdControllerBase<PlantumlExtensionsController>
    {
        private readonly ICommandFactoryHtml _commandFactory;
        private readonly PlantumlServer _plantumlServer;

        public PlantumlExtensionsController(
            ICommandFactoryHtml commandFactory,
            PlantumlServer plantumlServer,
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
            _plantumlServer = plantumlServer;
        }

        [HttpGet]
        public IActionResult GetPng(string pathFile, string hashFile, int step)
        {

            (var requestInfo, var markdown) = GetMarkDown(pathFile);
            var render = (IPresentationPlantuml)_commandFactory.GetCommands().Where(_ => _.Name == "FromPlantumlToPng").FirstOrDefault();
            var generatedFileName = render.GetPng(markdown, hashFile, step, requestInfo);

            return Ok(new { GeneratedFileName = generatedFileName });
        }

        [HttpGet]
        public async Task<IActionResult> CopyPngToClipboard(string pathFile, string hashFile, int step)
        {
            try
            {
                (var requestInfo, var markdown) = GetMarkDown(pathFile);
                var render = (IPresentationPlantuml)_commandFactory.GetCommands().Where(_ => _.Name == "FromPlantumlToPng").FirstOrDefault();
                var generatedFileName = render.GetPng(markdown, hashFile, step, requestInfo);

                var pngPath = Path.Combine(requestInfo.CurrentRoot, ".md", generatedFileName);
                if (!System.IO.File.Exists(pngPath))
                    return NotFound(new { error = "PNG file not found" });

                var pngData = await System.IO.File.ReadAllBytesAsync(pngPath);
                var result = await CrossPlatformClipboard.SetImageAsync(pngData);

                if (result.Success)
                    return Ok(new { message = "Image copied to clipboard" });

                return StatusCode(500, new { error = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CopyPngToClipboard] Error");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CopyImageToClipboard(string imagePath)
        {
            try
            {
                var projectPath = GetProjectPath();

                // Strip /api/mdexplorer/ prefix if present (image URLs go through the API)
                var cleanPath = imagePath;
                var apiPrefix = "/api/mdexplorer/";
                if (cleanPath.StartsWith(apiPrefix, StringComparison.OrdinalIgnoreCase))
                    cleanPath = cleanPath.Substring(apiPrefix.Length);
                // Strip query string if present
                var qsIndex = cleanPath.IndexOf('?');
                if (qsIndex >= 0)
                    cleanPath = cleanPath.Substring(0, qsIndex);

                var fullPath = Path.Combine(projectPath, cleanPath.Replace('/', Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(fullPath))
                    return NotFound(new { error = "Image file not found" });

                var ext = Path.GetExtension(fullPath).ToLowerInvariant();
                byte[] pngData;

                if (ext == ".svg")
                {
                    // SVG → PNG via Svg.Skia + SkiaSharp (cross-platform)
                    using (var svg = new Svg.Skia.SKSvg())
                    {
                        svg.Load(fullPath);
                        if (svg.Picture == null)
                            return StatusCode(500, new { error = "Failed to parse SVG" });

                        var bounds = svg.Picture.CullRect;
                        var info = new SkiaSharp.SKImageInfo((int)bounds.Width, (int)bounds.Height);
                        using (var surface = SkiaSharp.SKSurface.Create(info))
                        {
                            surface.Canvas.Clear(SkiaSharp.SKColors.White);
                            surface.Canvas.DrawPicture(svg.Picture);
                            surface.Canvas.Flush();
                            using (var image = surface.Snapshot())
                            using (var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                            {
                                pngData = data.ToArray();
                            }
                        }
                    }
                }
                else if (ext == ".png")
                {
                    pngData = await System.IO.File.ReadAllBytesAsync(fullPath);
                }
                else
                {
                    // JPG, BMP, etc. → PNG via System.Drawing (Windows) or SkiaSharp
                    var imageData = await System.IO.File.ReadAllBytesAsync(fullPath);
                    using (var inputBitmap = SkiaSharp.SKBitmap.Decode(imageData))
                    using (var outputMs = new MemoryStream())
                    {
                        inputBitmap.Encode(outputMs, SkiaSharp.SKEncodedImageFormat.Png, 100);
                        pngData = outputMs.ToArray();
                    }
                }

                var result = await CrossPlatformClipboard.SetImageAsync(pngData);

                if (result.Success)
                    return Ok(new { message = "Image copied to clipboard" });

                return StatusCode(500, new { error = result.ErrorMessage });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CopyImageToClipboard] Error");
                return StatusCode(500, new { error = ex.Message });
            }
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

        /// <summary>
        /// Renders PlantUML code to SVG via the PlantUML server.
        /// Used by PromptLab to render Sequence/Workflow diagrams.
        /// If SavePath is provided, also saves the SVG to disk relative to the template file's directory.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RenderSvg([FromBody] RenderSvgRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.PlantUmlCode))
                    return BadRequest(new { error = "PlantUML code is required" });

                var svgBytes = await _plantumlServer.GetSvgFromJar(request.PlantUmlCode);
                if (svgBytes == null || svgBytes.Length == 0)
                    return StatusCode(500, new { error = "PlantUML server returned empty result" });

                var svg = Encoding.UTF8.GetString(svgBytes);

                // Optionally save SVG to disk
                if (!string.IsNullOrWhiteSpace(request.SavePath))
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(request.SavePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        await System.IO.File.WriteAllBytesAsync(request.SavePath, svgBytes);
                        _logger.LogInformation("[RenderSvg] SVG saved to: {Path}", request.SavePath);
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogWarning(saveEx, "[RenderSvg] Failed to save SVG to disk, returning SVG anyway");
                    }
                }

                return Ok(new { svg });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RenderSvg] Error rendering PlantUML");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class RenderSvgRequest
        {
            public string PlantUmlCode { get; set; }
            /// <summary>
            /// Optional absolute path where the SVG file should be saved.
            /// </summary>
            public string SavePath { get; set; }
        }

    }
}
