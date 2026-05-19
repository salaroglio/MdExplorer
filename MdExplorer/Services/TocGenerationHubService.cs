using System;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services
{
    /// <summary>
    /// Wrapper that forwards TocGenerationService progress/completion events to SignalR clients.
    /// </summary>
    public class TocGenerationHubService : ITocGenerationService
    {
        private readonly TocGenerationService _innerService;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ILogger<TocGenerationHubService> _logger;

        public event EventHandler<TocGenerationProgress> ProgressChanged
        {
            add { _innerService.ProgressChanged += value; }
            remove { _innerService.ProgressChanged -= value; }
        }

        public event EventHandler<string> GenerationCompleted
        {
            add { _innerService.GenerationCompleted += value; }
            remove { _innerService.GenerationCompleted -= value; }
        }

        public TocGenerationHubService(
            TocGenerationService innerService,
            IHubContext<MonitorMDHub> hubContext,
            ILogger<TocGenerationHubService> logger)
        {
            _innerService = innerService;
            _hubContext = hubContext;
            _logger = logger;

            _innerService.ProgressChanged += OnProgressChanged;
            _innerService.GenerationCompleted += OnGenerationCompleted;
        }

        public Task<bool> GenerateTocAsync(string directoryPath, string tocFilePath, CancellationToken ct = default)
        {
            return _innerService.GenerateTocAsync(directoryPath, tocFilePath, ct);
        }

        private async void OnProgressChanged(object sender, TocGenerationProgress e)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TocGenerationProgress", new
                {
                    directory = e.Directory,
                    processed = e.Processed,
                    total = e.Total,
                    status = e.Status,
                    percentComplete = e.PercentComplete
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending progress notification: {ex.Message}", ex);
            }
        }

        private async void OnGenerationCompleted(object sender, string directoryPath)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("TocGenerationComplete", new
                {
                    directory = directoryPath,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending completion notification: {ex.Message}", ex);
            }
        }
    }
}
