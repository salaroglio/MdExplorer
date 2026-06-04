using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Read-only Confluence Cloud client. Mirrors <see cref="JiraClient"/>'s auth
    /// and error handling (Basic auth per call, actionable <see cref="AtlassianApiException"/>,
    /// no silent fallbacks) but talks to Confluence: v2 for spaces/pages, v1 CQL
    /// for search. Page bodies are requested as ADF and flattened with AdfRenderer.
    /// </summary>
    public class ConfluenceClient : IConfluenceClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ConfluenceClient> _logger;

        public ConfluenceClient(IHttpClientFactory httpClientFactory, ILogger<ConfluenceClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ConfluenceSpace>> ListSpacesAsync(
            ConfluenceConnection conn, int limit = 50, CancellationToken ct = default)
        {
            Validate(conn);
            if (limit <= 0 || limit > 250) limit = 50;

            var url = $"{BaseUrl(conn)}/api/v2/spaces?limit={limit}";
            using var doc = await GetJsonAsync(conn, url, ct);
            var result = new List<ConfluenceSpace>();
            if (doc != null && doc.RootElement.TryGetProperty("results", out var results) &&
                results.ValueKind == JsonValueKind.Array)
            {
                foreach (var s in results.EnumerateArray())
                    result.Add(new ConfluenceSpace
                    {
                        Id = GetString(s, "id"),
                        Key = GetString(s, "key"),
                        Name = GetString(s, "name"),
                        Type = GetString(s, "type")
                    });
            }
            return result;
        }

        public async Task<IReadOnlyList<ConfluenceSearchHit>> SearchAsync(
            ConfluenceConnection conn, string cql, int limit = 20, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(cql))
                throw new AtlassianApiException("A CQL query is required to search Confluence.");
            if (limit <= 0 || limit > 100) limit = 25;

            // v2 has no search API — CQL search stays on the v1 endpoint. We expand
            // content.space so each hit carries its space key without a second call.
            var url = $"{BaseUrl(conn)}/rest/api/search" +
                      $"?cql={Uri.EscapeDataString(cql)}" +
                      $"&limit={limit}" +
                      $"&expand=content.space";

            using var doc = await GetJsonAsync(conn, url, ct);
            var result = new List<ConfluenceSearchHit>();
            if (doc != null && doc.RootElement.TryGetProperty("results", out var results) &&
                results.ValueKind == JsonValueKind.Array)
            {
                foreach (var r in results.EnumerateArray())
                    result.Add(MapSearchHit(conn, r));
            }
            return result;
        }

        public async Task<ConfluencePage> GetPageAsync(
            ConfluenceConnection conn, string pageId, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(pageId))
                throw new AtlassianApiException("A page id is required.");

            // Ask for ADF (atlas_doc_format) so we can reuse AdfRenderer; the value
            // is a JSON-encoded ADF document string.
            var url = $"{BaseUrl(conn)}/api/v2/pages/{Uri.EscapeDataString(pageId.Trim())}" +
                      $"?body-format=atlas_doc_format";

            using var doc = await GetJsonAsync(conn, url, ct);
            var root = doc.RootElement;

            var page = new ConfluencePage
            {
                Id = GetString(root, "id"),
                Title = GetString(root, "title"),
                SpaceId = GetString(root, "spaceId"),
                Status = GetString(root, "status")
            };

            if (root.TryGetProperty("version", out var ver) && ver.ValueKind == JsonValueKind.Object &&
                ver.TryGetProperty("number", out var num) && num.ValueKind == JsonValueKind.Number)
                page.Version = num.GetInt32();

            // body.atlas_doc_format.value is a STRING containing the ADF JSON.
            if (root.TryGetProperty("body", out var body) && body.ValueKind == JsonValueKind.Object &&
                body.TryGetProperty("atlas_doc_format", out var adf) && adf.ValueKind == JsonValueKind.Object)
            {
                var value = GetString(adf, "value");
                page.Body = AdfRenderer.ToText(value);
            }

            page.Url = BuildPageUrl(conn, root, page.Id);
            return page;
        }

        public async Task<ConfluenceWriteResult> CreatePageAsync(
            ConfluenceConnection conn, ConfluenceCreatePageRequest req, CancellationToken ct = default)
        {
            Validate(conn);
            if (req == null) throw new AtlassianApiException("A create request is required.");
            if (string.IsNullOrWhiteSpace(req.Title))
                throw new AtlassianApiException("A page title is required.");

            var spaceId = req.SpaceId?.Trim();
            if (string.IsNullOrEmpty(spaceId))
            {
                if (string.IsNullOrWhiteSpace(req.SpaceKey))
                    throw new AtlassianApiException("A space (key or id) is required to create a page.");
                spaceId = await ResolveSpaceIdAsync(conn, req.SpaceKey.Trim(), ct);
            }

            var adf = MarkdownToAdf.ToAdfJson(req.MarkdownBody ?? string.Empty);
            var payload = new JsonObject
            {
                ["spaceId"] = spaceId,
                ["status"] = "current",
                ["title"] = req.Title.Trim(),
                ["body"] = new JsonObject
                {
                    ["representation"] = "atlas_doc_format",
                    ["value"] = adf
                }
            };
            if (!string.IsNullOrWhiteSpace(req.ParentId))
                payload["parentId"] = req.ParentId.Trim();

            using var doc = await SendJsonAsync(conn, HttpMethod.Post, $"{BaseUrl(conn)}/api/v2/pages", payload, ct);
            return MapWriteResult(conn, doc);
        }

        public async Task<ConfluenceWriteResult> UpdatePageAsync(
            ConfluenceConnection conn, ConfluenceUpdatePageRequest req, CancellationToken ct = default)
        {
            Validate(conn);
            if (req == null || string.IsNullOrWhiteSpace(req.PageId))
                throw new AtlassianApiException("A page id is required to update a page.");

            // Read the current page to get the version (optimistic lock) and the
            // existing title/status when the caller does not override them.
            var pageId = req.PageId.Trim();
            using var current = await GetJsonAsync(conn, $"{BaseUrl(conn)}/api/v2/pages/{Uri.EscapeDataString(pageId)}", ct);
            var croot = current.RootElement;
            var currentVersion = 0;
            if (croot.TryGetProperty("version", out var ver) && ver.ValueKind == JsonValueKind.Object &&
                ver.TryGetProperty("number", out var num) && num.ValueKind == JsonValueKind.Number)
                currentVersion = num.GetInt32();
            var status = GetString(croot, "status") ?? "current";
            var title = string.IsNullOrWhiteSpace(req.Title) ? GetString(croot, "title") : req.Title.Trim();

            var adf = MarkdownToAdf.ToAdfJson(req.MarkdownBody ?? string.Empty);
            var version = new JsonObject { ["number"] = currentVersion + 1 };
            if (!string.IsNullOrWhiteSpace(req.VersionMessage))
                version["message"] = req.VersionMessage.Trim();

            var payload = new JsonObject
            {
                ["id"] = pageId,
                ["status"] = status,
                ["title"] = title,
                ["body"] = new JsonObject
                {
                    ["representation"] = "atlas_doc_format",
                    ["value"] = adf
                },
                ["version"] = version
            };

            using var doc = await SendJsonAsync(conn, HttpMethod.Put,
                $"{BaseUrl(conn)}/api/v2/pages/{Uri.EscapeDataString(pageId)}", payload, ct);
            return MapWriteResult(conn, doc);
        }

        /// <summary>Resolves a space key (e.g. "DEV") to its numeric id via v2.</summary>
        private async Task<string> ResolveSpaceIdAsync(ConfluenceConnection conn, string spaceKey, CancellationToken ct)
        {
            using var doc = await GetJsonAsync(conn,
                $"{BaseUrl(conn)}/api/v2/spaces?keys={Uri.EscapeDataString(spaceKey)}&limit=1", ct);
            if (doc != null && doc.RootElement.TryGetProperty("results", out var results) &&
                results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
            {
                var id = GetString(results[0], "id");
                if (!string.IsNullOrEmpty(id)) return id;
            }
            throw new AtlassianApiException(
                $"Confluence space '{spaceKey}' not found (or you lack access). Use ConfluenceListSpaces to find valid keys.");
        }

        private static ConfluenceWriteResult MapWriteResult(ConfluenceConnection conn, JsonDocument doc)
        {
            var root = doc.RootElement;
            var result = new ConfluenceWriteResult
            {
                Id = GetString(root, "id"),
                Title = GetString(root, "title")
            };
            if (root.TryGetProperty("version", out var ver) && ver.ValueKind == JsonValueKind.Object &&
                ver.TryGetProperty("number", out var num) && num.ValueKind == JsonValueKind.Number)
                result.Version = num.GetInt32();
            result.Url = BuildPageUrl(conn, root, result.Id);
            return result;
        }

        // ── mapping ─────────────────────────────────────────────────

        private ConfluenceSearchHit MapSearchHit(ConfluenceConnection conn, JsonElement r)
        {
            var hit = new ConfluenceSearchHit
            {
                Title = GetString(r, "title"),
                Excerpt = StripHighlight(GetString(r, "excerpt"))
            };

            if (r.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Object)
            {
                hit.Id = GetString(content, "id");
                hit.Type = GetString(content, "type");
                if (content.TryGetProperty("space", out var space) && space.ValueKind == JsonValueKind.Object)
                    hit.SpaceKey = GetString(space, "key");
            }

            // The v1 search "url" is relative to the Confluence context (/wiki),
            // which is exactly our BaseUrl — so prefix it directly.
            var rel = GetString(r, "url");
            if (!string.IsNullOrEmpty(rel))
                hit.Url = BaseUrl(conn) + (rel.StartsWith("/") ? rel : "/" + rel);
            else if (!string.IsNullOrEmpty(hit.Id))
                hit.Url = $"{BaseUrl(conn)}/pages/{hit.Id}";

            return hit;
        }

        private static string BuildPageUrl(ConfluenceConnection conn, JsonElement root, string id)
        {
            if (root.TryGetProperty("_links", out var links) && links.ValueKind == JsonValueKind.Object)
            {
                var webui = GetString(links, "webui");
                if (!string.IsNullOrEmpty(webui))
                    return BaseUrl(conn) + (webui.StartsWith("/") ? webui : "/" + webui);
            }
            return string.IsNullOrEmpty(id) ? null : $"{BaseUrl(conn)}/pages/{id}";
        }

        /// <summary>
        /// CQL "highlight" excerpts wrap matched terms in @@@hl@@@…@@@endhl@@@.
        /// Strip the markers so the snippet is clean text.
        /// </summary>
        private static string StripHighlight(string s) =>
            string.IsNullOrEmpty(s) ? s : s.Replace("@@@hl@@@", "").Replace("@@@endhl@@@", "");

        // ── HTTP plumbing (mirrors JiraClient) ──────────────────────

        private Task<JsonDocument> GetJsonAsync(ConfluenceConnection conn, string url, CancellationToken ct) =>
            SendJsonAsync(conn, HttpMethod.Get, url, body: null, ct);

        private async Task<JsonDocument> SendJsonAsync(
            ConfluenceConnection conn, HttpMethod method, string url, JsonNode body, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(method, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", BasicToken(conn));
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (body != null)
                req.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

            HttpResponseMessage resp;
            try
            {
                resp = await client.SendAsync(req, ct);
            }
            catch (HttpRequestException ex)
            {
                throw new AtlassianApiException(
                    $"Could not reach Confluence at {BaseUrl(conn)}: {ex.Message}", null, ex);
            }

            var content = await resp.Content.ReadAsStringAsync(ct);
            EnsureSuccessOrThrow(resp, content, url);

            if (string.IsNullOrWhiteSpace(content)) return null;
            try
            {
                return JsonDocument.Parse(content);
            }
            catch (JsonException ex)
            {
                throw new AtlassianApiException("Confluence returned a non-JSON response.", resp.StatusCode, ex);
            }
        }

        private void EnsureSuccessOrThrow(HttpResponseMessage resp, string content, string url)
        {
            if (resp.IsSuccessStatusCode) return;

            _logger.LogWarning("[ConfluenceClient] {Status} from {Url}: {Body}", resp.StatusCode, url, Truncate(content, 500));
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var reason = ExtractReason(content);
                throw new AtlassianApiException(
                    "Confluence rejected the request (HTTP " + (int)resp.StatusCode + "). " +
                    (string.IsNullOrEmpty(reason) ? "" : "Confluence says: " + reason + ". ") +
                    "Use a classic (non-scoped) API token, check it has not expired, and make sure " +
                    "the account has access to Confluence (the space must be shared with you). " +
                    "Manage tokens at id.atlassian.com.",
                    resp.StatusCode);
            }
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new AtlassianApiException(
                    "Confluence returned HTTP 404: the page/space was not found, or Confluence is not " +
                    "enabled on this site. " + (ExtractReason(content) ?? ""),
                    resp.StatusCode);
            }
            throw new AtlassianApiException(
                $"Confluence returned HTTP {(int)resp.StatusCode}: {ExtractReason(content) ?? Truncate(content, 300)}",
                resp.StatusCode);
        }

        private static void Validate(ConfluenceConnection conn)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (string.IsNullOrWhiteSpace(conn.BaseUrl))
                throw new AtlassianApiException("Confluence base URL is not configured (Project Settings → Atlassian).");
            if (string.IsNullOrWhiteSpace(conn.Email))
                throw new AtlassianApiException("Atlassian account email is not configured (Project Settings → Atlassian).");
            if (string.IsNullOrWhiteSpace(conn.ApiToken))
                throw new AtlassianApiException("Atlassian API token is not set (Project Settings → Atlassian).");
        }

        private static string BaseUrl(ConfluenceConnection conn) => conn.BaseUrl.TrimEnd('/');

        private static string BasicToken(ConfluenceConnection conn) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{conn.Email}:{conn.ApiToken}"));

        private static string GetString(JsonElement el, string prop) =>
            el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";

        /// <summary>
        /// Pulls a human reason out of a Confluence error body. Confluence Cloud
        /// returns { "message": "..." } (v2) or { "message": {...} } / errorMessages.
        /// </summary>
        private static string ExtractReason(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return Truncate(body, 200);

                if (root.TryGetProperty("message", out var msg))
                {
                    if (msg.ValueKind == JsonValueKind.String) return msg.GetString();
                    if (msg.ValueKind == JsonValueKind.Object && msg.TryGetProperty("message", out var inner) &&
                        inner.ValueKind == JsonValueKind.String)
                        return inner.GetString();
                }
                if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Array &&
                    errs.GetArrayLength() > 0)
                {
                    var first = errs[0];
                    if (first.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                        return title.GetString();
                }
                return Truncate(body, 200);
            }
            catch (JsonException)
            {
                return Truncate(body, 200);
            }
        }
    }
}
