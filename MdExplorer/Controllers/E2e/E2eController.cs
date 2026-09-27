using MdExplorer.Features.E2e;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.E2e
{
    /// <summary>
    /// End-to-end tests written in markdown (sprint docs-internal/Sprints/2026-09-26-Test-E2E-Da-Markdown.md).
    /// F3: what a run needs on this computer, and the installation of what is missing, asked by the
    /// secondary wizard only when the user runs a test (D7).
    /// </summary>
    [ApiController]
    [Route("api/e2e")]
    public class E2eController : ControllerBase
    {
        // One installation at a time: two wizards (two windows) must not write the same folder.
        private static readonly SemaphoreSlim InstallLock = new(1, 1);

        private readonly E2eEnvironment _environment;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<E2eController> _logger;

        public E2eController(E2eEnvironment environment, IHttpClientFactory httpClientFactory, ILogger<E2eController> logger)
        {
            _environment = environment;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("prerequisites")]
        public async Task<ActionResult<E2ePrerequisitesReport>> GetPrerequisites(CancellationToken ct) =>
            await _environment.CheckAsync(ct);

        [HttpPost("prerequisites/playwright-mcp")]
        public Task<IActionResult> InstallPlaywrightMcp(CancellationToken ct) =>
            Install("@playwright/mcp", async () =>
            {
                var http = _httpClientFactory.CreateClient();
                http.Timeout = TimeSpan.FromMinutes(10);
                var installed = await _environment.InstallPlaywrightMcpAsync(http, ct);
                return string.Join(", ", installed);
            });

        [HttpPost("prerequisites/chromium")]
        public Task<IActionResult> InstallChromium(CancellationToken ct) =>
            Install("Chromium", () => _environment.InstallChromiumAsync(ct));

        private async Task<IActionResult> Install(string what, Func<Task<string>> install)
        {
            if (!await InstallLock.WaitAsync(0))
                return Conflict(new { error = "C'è già un'installazione in corso: aspetta che finisca." });
            try
            {
                _logger.LogInformation("[E2e] Installing {What}", what);
                var detail = await install();
                _logger.LogInformation("[E2e] Installed {What}: {Detail}", what, detail);
                return Ok(new { detail, report = await _environment.CheckAsync() });
            }
            catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException)
            {
                _logger.LogError(ex, "[E2e] Installing {What} failed", what);
                return StatusCode(502, new { error = ex.Message });
            }
            finally
            {
                InstallLock.Release();
            }
        }
    }
}
