using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class FusekiClient : IFusekiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FusekiClient> _logger;

        public FusekiClient(IHttpClientFactory httpClientFactory, ILogger<FusekiClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string SanitizeDatasetName(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName)) return "default";
            // Replace any char that's not [A-Za-z0-9_-] with '_'.
            var sanitized = Regex.Replace(projectName.Trim(), @"[^A-Za-z0-9_\-]", "_");
            // Collapse multiple underscores in a row.
            sanitized = Regex.Replace(sanitized, @"_+", "_");
            // Trim leading/trailing punctuation.
            sanitized = sanitized.Trim('_', '-');
            return string.IsNullOrEmpty(sanitized) ? "default" : sanitized;
        }

        public async Task<FusekiTestResult> TestAsync(string baseUri, string dataset, string username, string passwordPlain)
        {
            var result = new FusekiTestResult();
            var sw = Stopwatch.StartNew();
            try
            {
                using var http = CreateClient(username, passwordPlain);
                var basePing = TrimSlash(baseUri) + "/$/ping";
                using var pingResp = await http.GetAsync(basePing);
                result.ServerReachable = pingResp.IsSuccessStatusCode;

                if (!result.ServerReachable)
                {
                    result.Error = $"Server not reachable: HTTP {(int)pingResp.StatusCode} on {basePing}";
                    sw.Stop();
                    result.LatencyMs = sw.ElapsedMilliseconds;
                    return result;
                }

                if (!string.IsNullOrWhiteSpace(dataset))
                {
                    var datasetUrl = TrimSlash(baseUri) + "/$/datasets/" + Uri.EscapeDataString(dataset);
                    using var dsResp = await http.GetAsync(datasetUrl);
                    result.DatasetExists = dsResp.IsSuccessStatusCode;
                    if (!result.DatasetExists && dsResp.StatusCode != HttpStatusCode.NotFound)
                    {
                        result.Error = $"Dataset check failed: HTTP {(int)dsResp.StatusCode}";
                    }
                }

                result.Success = result.ServerReachable && (string.IsNullOrWhiteSpace(dataset) || result.DatasetExists);
                sw.Stop();
                result.LatencyMs = sw.ElapsedMilliseconds;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[FusekiClient] TestAsync failed for {Uri}", baseUri);
                sw.Stop();
                result.LatencyMs = sw.ElapsedMilliseconds;
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
        }

        public async Task<bool> EnsureDatasetAsync(string baseUri, string dataset, string username, string passwordPlain)
        {
            if (string.IsNullOrWhiteSpace(dataset))
                throw new ArgumentException("Dataset name is required", nameof(dataset));

            try
            {
                using var http = CreateClient(username, passwordPlain);

                // Check if it already exists
                var checkUrl = TrimSlash(baseUri) + "/$/datasets/" + Uri.EscapeDataString(dataset);
                using var checkResp = await http.GetAsync(checkUrl);
                if (checkResp.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[FusekiClient] Dataset '{Dataset}' already exists on {Uri}", dataset, baseUri);
                    return true;
                }

                // Create it
                var createUrl = TrimSlash(baseUri)
                    + "/$/datasets?dbName=" + Uri.EscapeDataString(dataset)
                    + "&dbType=tdb2";
                using var createResp = await http.PostAsync(createUrl, new StringContent(string.Empty));
                if (createResp.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[FusekiClient] Created dataset '{Dataset}' on {Uri}", dataset, baseUri);
                    return true;
                }

                var body = await createResp.Content.ReadAsStringAsync();
                _logger.LogWarning("[FusekiClient] Create dataset failed: HTTP {Status} — {Body}", (int)createResp.StatusCode, body);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FusekiClient] EnsureDatasetAsync failed for {Dataset} at {Uri}", dataset, baseUri);
                return false;
            }
        }

        private HttpClient CreateClient(string username, string passwordPlain)
        {
            var http = _httpClientFactory.CreateClient();
            http.Timeout = TimeSpan.FromSeconds(15);
            if (!string.IsNullOrWhiteSpace(username))
            {
                var basicToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{passwordPlain ?? string.Empty}"));
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicToken);
            }
            return http;
        }

        private static string TrimSlash(string s) => string.IsNullOrEmpty(s) ? s : s.TrimEnd('/');
    }
}
