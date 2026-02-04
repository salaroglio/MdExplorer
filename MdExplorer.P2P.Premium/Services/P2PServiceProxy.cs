using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MdExplorer.P2P.Premium.Models;

namespace MdExplorer.P2P.Premium.Services
{
    /// <summary>
    /// P2P Service proxy that communicates with the Electron P2P plugin
    /// via its HTTP API on localhost:48124
    /// </summary>
    public class P2PServiceProxy : IP2PService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<P2PServiceProxy> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string DEFAULT_BASE_URL = "http://127.0.0.1:48124";

        public P2PServiceProxy(HttpClient httpClient, ILogger<P2PServiceProxy> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Configure JSON options to match Electron's API
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Set default base address if not already set
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(DEFAULT_BASE_URL);
            }

            // Set reasonable timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "P2P service not available");
                return false;
            }
        }

        public async Task<HealthResponse?> GetHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HealthResponse>(_jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting P2P health status");
                return null;
            }
        }

        public async Task<P2PStats?> GetStatsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/stats");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<P2PStats>(_jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting P2P stats");
                return null;
            }
        }

        public async Task<List<TransferInfo>> GetTransfersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/transfers");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<TransferInfo>>(_jsonOptions);
                    return result ?? new List<TransferInfo>();
                }
                return new List<TransferInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting P2P transfers");
                return new List<TransferInfo>();
            }
        }

        public async Task<TransferInfo?> GetTransferAsync(string infoHash)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/transfers/{infoHash}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TransferInfo>(_jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting P2P transfer {InfoHash}", infoHash);
                return null;
            }
        }

        public async Task<ShareResult?> ShareFileAsync(string filePath, string? name = null)
        {
            try
            {
                var request = new { filePath, name };
                var response = await _httpClient.PostAsJsonAsync("/share", request, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ShareResult>(_jsonOptions);
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error sharing file: {Error}", error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing file {FilePath}", filePath);
                return null;
            }
        }

        public async Task<ShareResult?> DownloadAsync(string magnetUri, string? destPath = null)
        {
            try
            {
                var request = new { magnetUri, destPath };
                var response = await _httpClient.PostAsJsonAsync("/download", request, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ShareResult>(_jsonOptions);
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error downloading: {Error}", error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading from magnet");
                return null;
            }
        }

        public async Task<bool> PauseTransferAsync(string infoHash)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/transfers/{infoHash}/pause", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing transfer {InfoHash}", infoHash);
                return false;
            }
        }

        public async Task<bool> ResumeTransferAsync(string infoHash)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/transfers/{infoHash}/resume", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming transfer {InfoHash}", infoHash);
                return false;
            }
        }

        public async Task<bool> StopTransferAsync(string infoHash, bool deleteFiles = false)
        {
            try
            {
                var url = $"/transfers/{infoHash}?deleteFiles={deleteFiles.ToString().ToLower()}";
                var response = await _httpClient.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping transfer {InfoHash}", infoHash);
                return false;
            }
        }

        public async Task<object?> ParseMagnetAsync(string magnetUri)
        {
            try
            {
                var request = new { magnetUri };
                var response = await _httpClient.PostAsJsonAsync("/parse-magnet", request, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<object>(_jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing magnet URI");
                return null;
            }
        }

        public async Task<TrackerStatusResponse?> GetTrackerStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/tracker-status");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TrackerStatusResponse>(_jsonOptions);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tracker status");
                return null;
            }
        }

        public async Task<AutoRestoreAllResult?> AutoRestoreAllAsync(List<ProjectInfo> projects)
        {
            try
            {
                var request = new { projects };
                var response = await _httpClient.PostAsJsonAsync("/auto-restore-all", request, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<AutoRestoreAllResult>(_jsonOptions);
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error auto-restoring all: {Error}", error);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-restoring all projects");
                return null;
            }
        }

        /// <summary>
        /// Subscribe to real-time P2P events via Server-Sent Events (SSE).
        /// Events include: peer-connected, upload-activity
        /// </summary>
        /// <param name="onEvent">Callback for each event received</param>
        /// <param name="ct">Cancellation token to stop subscription</param>
        public async Task SubscribeToEventsAsync(Action<string, JsonElement> onEvent, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("[P2P] Subscribing to SSE events...");

                using var request = new HttpRequestMessage(HttpMethod.Get, "/events");
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var reader = new StreamReader(stream);

                string? eventType = null;

                while (!ct.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(ct);

                    if (line == null)
                    {
                        // Stream ended
                        break;
                    }

                    if (line.StartsWith("event: "))
                    {
                        eventType = line.Substring(7);
                    }
                    else if (line.StartsWith("data: "))
                    {
                        var json = line.Substring(6);
                        if (!string.IsNullOrEmpty(eventType) && !string.IsNullOrWhiteSpace(json))
                        {
                            try
                            {
                                var data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
                                _logger.LogDebug("[P2P Event] {EventType}: {Json}", eventType, json);
                                onEvent(eventType, data);
                            }
                            catch (JsonException ex)
                            {
                                _logger.LogWarning(ex, "[P2P Event] Failed to parse JSON: {Json}", json);
                            }
                        }
                        eventType = null;
                    }
                    else if (line.StartsWith(": keepalive"))
                    {
                        // Ignore keepalive comments
                    }
                }

                _logger.LogInformation("[P2P] SSE subscription ended");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[P2P] SSE subscription cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[P2P] SSE subscription error");
                throw;
            }
        }
    }
}
