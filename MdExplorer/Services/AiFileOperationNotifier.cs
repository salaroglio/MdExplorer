using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Services;
using MdExplorer.Hubs;

namespace MdExplorer.Services
{
    /// <summary>
    /// Sends real-time SignalR notifications about AI file operations to specific clients.
    /// </summary>
    public class AiFileOperationNotifier : IAiFileOperationNotifier
    {
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly ILogger<AiFileOperationNotifier> _logger;

        public AiFileOperationNotifier(
            IHubContext<MonitorMDHub> hubContext,
            ILogger<AiFileOperationNotifier> logger)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendNotificationAsync(string connectionId, string operationType, string filePath, bool success, string message)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                _logger.LogDebug("No connectionId provided for AI file operation notification");
                return;
            }

            try
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("aiFileOperation", new
                {
                    operationType = operationType,
                    filePath = filePath,
                    success = success,
                    message = message,
                    timestamp = DateTime.UtcNow
                });

                _logger.LogDebug("Sent AI file operation notification to {ConnectionId}: {OperationType} {FilePath} Success={Success}",
                    connectionId, operationType, filePath, success);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send AI file operation notification to {ConnectionId}", connectionId);
            }
        }
    }
}
