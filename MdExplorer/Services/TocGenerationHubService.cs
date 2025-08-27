using System;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services
{
    /// <summary>
    /// Wrapper service that bridges TocGenerationService events with SignalR notifications
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

            // Subscribe to events and forward to SignalR
            _innerService.ProgressChanged += OnProgressChanged;
            _innerService.GenerationCompleted += OnGenerationCompleted;
        }

        public Task<bool> GenerateTocWithAIAsync(string directoryPath, string tocFilePath, CancellationToken ct = default)
        {
            return _innerService.GenerateTocWithAIAsync(directoryPath, tocFilePath, ct);
        }

        public Task<bool> RefreshTocAsync(string tocFilePath, CancellationToken ct = default)
        {
            return _innerService.RefreshTocAsync(tocFilePath, ct);
        }

        public Task<string> GenerateQuickTocAsync(string directoryPath, string tocFilePath)
        {
            return _innerService.GenerateQuickTocAsync(directoryPath, tocFilePath);
        }
        
        public void SetAiMode(bool useGemini, string geminiModel = null)
        {
            _logger.LogInformation($"[TocGenerationHubService] SetAiMode called - forwarding to inner service");
            _innerService.SetAiMode(useGemini, geminiModel);
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