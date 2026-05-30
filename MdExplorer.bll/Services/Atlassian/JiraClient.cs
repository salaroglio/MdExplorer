using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.Atlassian
{
    public class JiraClient : IJiraClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<JiraClient> _logger;

        // Fields requested for triage lists and for the planning detail view.
        private const string SummaryFields = "summary,status,priority,issuetype,duedate,assignee";
        private const string DetailFields = "summary,status,priority,issuetype,duedate,assignee,reporter,description,labels,comment,issuelinks";

        public JiraClient(IHttpClientFactory httpClientFactory, ILogger<JiraClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IReadOnlyList<JiraIssueSummary>> SearchAsync(
            JiraConnection conn, string jql, int maxResults, CancellationToken ct = default)
        {
            Validate(conn);
            if (maxResults <= 0 || maxResults > 100) maxResults = 50;

            // /rest/api/3/search (legacy) was removed; /search/jql is the current
            // endpoint. Pagination is token-based (nextPageToken) — for triage we
            // only need the first, most-urgent page so we don't follow it.
            var url = $"{BaseUrl(conn)}/rest/api/3/search/jql" +
                      $"?jql={Uri.EscapeDataString(jql)}" +
                      $"&maxResults={maxResults}" +
                      $"&fields={SummaryFields}";

            using var doc = await GetJsonAsync(conn, url, ct);
            var result = new List<JiraIssueSummary>();
            if (doc.RootElement.TryGetProperty("issues", out var issues) &&
                issues.ValueKind == JsonValueKind.Array)
            {
                foreach (var issue in issues.EnumerateArray())
                    result.Add(MapSummary(conn, issue));
            }
            return result;
        }

        public async Task<JiraIssueDetail> GetIssueAsync(
            JiraConnection conn, string issueKey, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(issueKey))
                throw new ArgumentException("issueKey is required", nameof(issueKey));

            var url = $"{BaseUrl(conn)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey.Trim())}" +
                      $"?fields={DetailFields}";

            using var doc = await GetJsonAsync(conn, url, ct);
            var root = doc.RootElement;

            var detail = new JiraIssueDetail
            {
                Key = GetString(root, "key"),
                Url = $"{BaseUrl(conn)}/browse/{GetString(root, "key")}"
            };

            if (root.TryGetProperty("fields", out var f) && f.ValueKind == JsonValueKind.Object)
            {
                detail.Summary = GetString(f, "summary");
                detail.Status = GetNestedName(f, "status");
                detail.Priority = GetNestedName(f, "priority");
                detail.IssueType = GetNestedName(f, "issuetype");
                detail.DueDate = GetString(f, "duedate");
                detail.Assignee = GetNestedString(f, "assignee", "displayName");
                detail.Reporter = GetNestedString(f, "reporter", "displayName");

                if (f.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.Object)
                    detail.Description = AdfRenderer.ToText(desc);

                if (f.TryGetProperty("labels", out var labels) && labels.ValueKind == JsonValueKind.Array)
                    foreach (var l in labels.EnumerateArray())
                        if (l.ValueKind == JsonValueKind.String) detail.Labels.Add(l.GetString());

                if (f.TryGetProperty("comment", out var commentObj) &&
                    commentObj.TryGetProperty("comments", out var comments) &&
                    comments.ValueKind == JsonValueKind.Array)
                {
                    foreach (var c in comments.EnumerateArray())
                    {
                        var comment = new JiraComment
                        {
                            Author = GetNestedString(c, "author", "displayName"),
                            Created = GetString(c, "created")
                        };
                        if (c.TryGetProperty("body", out var body) && body.ValueKind == JsonValueKind.Object)
                            comment.Body = AdfRenderer.ToText(body);
                        detail.Comments.Add(comment);
                    }
                }

                if (f.TryGetProperty("issuelinks", out var links) && links.ValueKind == JsonValueKind.Array)
                    foreach (var link in links.EnumerateArray())
                        AddLink(detail, link);
            }

            return detail;
        }

        public async Task<JiraMyself> VerifyAsync(JiraConnection conn, CancellationToken ct = default)
        {
            Validate(conn);
            var url = $"{BaseUrl(conn)}/rest/api/3/myself";
            using var doc = await GetJsonAsync(conn, url, ct);
            var root = doc.RootElement;
            return new JiraMyself
            {
                AccountId = GetString(root, "accountId"),
                DisplayName = GetString(root, "displayName"),
                EmailAddress = GetString(root, "emailAddress")
            };
        }

        // ── HTTP plumbing ───────────────────────────────────────────

        private async Task<JsonDocument> GetJsonAsync(JiraConnection conn, string url, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", BasicToken(conn));
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage resp;
            try
            {
                resp = await client.SendAsync(req, ct);
            }
            catch (HttpRequestException ex)
            {
                throw new AtlassianApiException(
                    $"Could not reach Jira at {BaseUrl(conn)}: {ex.Message}", null, ex);
            }

            var content = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new AtlassianApiException(
                        "Jira rejected the credentials (HTTP " + (int)resp.StatusCode + "). " +
                        "Your API token may be missing, expired, or lacks permission. " +
                        "Regenerate it at id.atlassian.com and re-enter it in Project Settings → Atlassian.",
                        resp.StatusCode);
                }
                _logger.LogWarning("[JiraClient] {Status} from {Url}: {Body}", resp.StatusCode, url, Truncate(content, 500));
                throw new AtlassianApiException(
                    $"Jira returned HTTP {(int)resp.StatusCode}: {Truncate(content, 300)}", resp.StatusCode);
            }

            try
            {
                return JsonDocument.Parse(content);
            }
            catch (JsonException ex)
            {
                throw new AtlassianApiException("Jira returned a non-JSON response.", resp.StatusCode, ex);
            }
        }

        private static void Validate(JiraConnection conn)
        {
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (string.IsNullOrWhiteSpace(conn.BaseUrl))
                throw new AtlassianApiException("Jira base URL is not configured (Project Settings → Atlassian).");
            if (string.IsNullOrWhiteSpace(conn.Email))
                throw new AtlassianApiException("Atlassian account email is not configured (Project Settings → Atlassian).");
            if (string.IsNullOrWhiteSpace(conn.ApiToken))
                throw new AtlassianApiException("Atlassian API token is not set (Project Settings → Atlassian).");
        }

        private static string BaseUrl(JiraConnection conn) => conn.BaseUrl.TrimEnd('/');

        private static string BasicToken(JiraConnection conn) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{conn.Email}:{conn.ApiToken}"));

        // ── JSON mapping helpers ────────────────────────────────────

        private JiraIssueSummary MapSummary(JiraConnection conn, JsonElement issue)
        {
            var s = new JiraIssueSummary
            {
                Key = GetString(issue, "key"),
                Url = $"{BaseUrl(conn)}/browse/{GetString(issue, "key")}"
            };
            if (issue.TryGetProperty("fields", out var f) && f.ValueKind == JsonValueKind.Object)
            {
                s.Summary = GetString(f, "summary");
                s.Status = GetNestedName(f, "status");
                s.Priority = GetNestedName(f, "priority");
                s.IssueType = GetNestedName(f, "issuetype");
                s.DueDate = GetString(f, "duedate");
                s.Assignee = GetNestedString(f, "assignee", "displayName");
            }
            return s;
        }

        private static void AddLink(JiraIssueDetail detail, JsonElement link)
        {
            string relation = null;
            if (link.TryGetProperty("type", out var type))
            {
                if (link.TryGetProperty("outwardIssue", out var outward))
                {
                    relation = GetString(type, "outward");
                    detail.Links.Add($"{relation} {GetString(outward, "key")}: {GetNestedString(outward, "fields", "summary")}");
                }
                else if (link.TryGetProperty("inwardIssue", out var inward))
                {
                    relation = GetString(type, "inward");
                    detail.Links.Add($"{relation} {GetString(inward, "key")}: {GetNestedString(inward, "fields", "summary")}");
                }
            }
        }

        private static string GetString(JsonElement el, string prop) =>
            el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

        private static string GetNestedName(JsonElement el, string prop) =>
            GetNestedString(el, prop, "name");

        private static string GetNestedString(JsonElement el, string prop, string innerProp)
        {
            if (el.TryGetProperty(prop, out var inner) && inner.ValueKind == JsonValueKind.Object &&
                inner.TryGetProperty(innerProp, out var v) && v.ValueKind == JsonValueKind.String)
                return v.GetString();
            return null;
        }

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";
    }
}
