using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<JiraCreatedIssue> CreateIssueAsync(
            JiraConnection conn, JiraCreateIssueRequest req, CancellationToken ct = default)
        {
            Validate(conn);
            if (req == null || string.IsNullOrWhiteSpace(req.ProjectKey))
                throw new AtlassianApiException("A Jira project key is required to create an issue.");
            if (string.IsNullOrWhiteSpace(req.Summary))
                throw new AtlassianApiException("A summary is required to create an issue.");

            // Resolve the caller's accountId up-front when assigning to self, so we
            // can assign via the dedicated /assignee endpoint (unambiguous on Cloud).
            string accountId = null;
            if (req.AssignToSelf)
                accountId = (await VerifyAsync(conn, ct))?.AccountId;

            var fields = new JsonObject
            {
                ["project"] = new JsonObject { ["key"] = req.ProjectKey.Trim() },
                ["summary"] = req.Summary.Trim(),
                ["issuetype"] = new JsonObject { ["name"] = string.IsNullOrWhiteSpace(req.IssueType) ? "Task" : req.IssueType.Trim() }
            };
            if (!string.IsNullOrWhiteSpace(req.Description))
                fields["description"] = JsonNode.Parse(AdfBuilder.FromPlainText(req.Description));
            if (!string.IsNullOrWhiteSpace(req.Priority))
                fields["priority"] = new JsonObject { ["name"] = req.Priority.Trim() };
            if (!string.IsNullOrWhiteSpace(req.DueDate))
                fields["duedate"] = req.DueDate.Trim();

            var body = new JsonObject { ["fields"] = fields };
            using var doc = await SendJsonAsync(conn, HttpMethod.Post, $"{BaseUrl(conn)}/rest/api/3/issue", body, ct);

            var key = doc != null && doc.RootElement.TryGetProperty("key", out var k) ? k.GetString() : null;

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(accountId))
            {
                var assignBody = new JsonObject { ["accountId"] = accountId };
                using var _ = await SendJsonAsync(conn, HttpMethod.Put,
                    $"{BaseUrl(conn)}/rest/api/3/issue/{Uri.EscapeDataString(key)}/assignee", assignBody, ct);
            }

            return new JiraCreatedIssue
            {
                Key = key,
                Url = string.IsNullOrEmpty(key) ? null : $"{BaseUrl(conn)}/browse/{key}"
            };
        }

        public async Task<string> AddCommentAsync(JiraConnection conn, string issueKey, string body, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(issueKey)) throw new AtlassianApiException("issueKey is required.");
            if (string.IsNullOrWhiteSpace(body)) throw new AtlassianApiException("comment body is required.");

            var payload = new JsonObject { ["body"] = JsonNode.Parse(AdfBuilder.FromPlainText(body)) };
            using var doc = await SendJsonAsync(conn, HttpMethod.Post,
                $"{BaseUrl(conn)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey.Trim())}/comment", payload, ct);
            return doc != null && doc.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
        }

        public async Task UpdateIssueAsync(JiraConnection conn, string issueKey, JiraUpdateIssueRequest req, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(issueKey)) throw new AtlassianApiException("issueKey is required.");
            if (req == null) throw new AtlassianApiException("nothing to update.");

            var fields = new JsonObject();
            if (!string.IsNullOrWhiteSpace(req.Summary)) fields["summary"] = req.Summary.Trim();
            if (req.Description != null) fields["description"] = JsonNode.Parse(AdfBuilder.FromPlainText(req.Description));
            if (!string.IsNullOrWhiteSpace(req.Priority)) fields["priority"] = new JsonObject { ["name"] = req.Priority.Trim() };
            if (!string.IsNullOrWhiteSpace(req.DueDate)) fields["duedate"] = req.DueDate.Trim();
            if (fields.Count == 0)
                throw new AtlassianApiException("Nothing to update: provide at least one of summary/description/priority/dueDate.");

            var payload = new JsonObject { ["fields"] = fields };
            using var _ = await SendJsonAsync(conn, HttpMethod.Put,
                $"{BaseUrl(conn)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey.Trim())}", payload, ct);
        }

        public async Task<IReadOnlyList<JiraTransition>> GetTransitionsAsync(JiraConnection conn, string issueKey, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(issueKey)) throw new AtlassianApiException("issueKey is required.");

            using var doc = await GetJsonAsync(conn,
                $"{BaseUrl(conn)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey.Trim())}/transitions", ct);
            var list = new List<JiraTransition>();
            if (doc != null && doc.RootElement.TryGetProperty("transitions", out var ts) && ts.ValueKind == JsonValueKind.Array)
            {
                foreach (var t in ts.EnumerateArray())
                {
                    list.Add(new JiraTransition
                    {
                        Id = GetString(t, "id"),
                        Name = GetString(t, "name"),
                        ToStatus = GetNestedString(t, "to", "name")
                    });
                }
            }
            return list;
        }

        public async Task<string> TransitionIssueAsync(JiraConnection conn, string issueKey, string transition, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(transition)) throw new AtlassianApiException("transition name is required.");
            var available = await GetTransitionsAsync(conn, issueKey, ct);
            var target = transition.Trim();
            var match = available.FirstOrDefault(t =>
                string.Equals(t.ToStatus, target, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t.Name, target, StringComparison.OrdinalIgnoreCase));
            if (match == null)
            {
                var options = string.Join(", ", available.Select(t => t.ToStatus ?? t.Name));
                throw new AtlassianApiException(
                    $"No transition '{transition}' available for {issueKey}. Available: {options}.");
            }

            var payload = new JsonObject { ["transition"] = new JsonObject { ["id"] = match.Id } };
            using var _ = await SendJsonAsync(conn, HttpMethod.Post,
                $"{BaseUrl(conn)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey.Trim())}/transitions", payload, ct);
            return match.ToStatus ?? match.Name;
        }

        public async Task<IReadOnlyList<JiraProject>> ListProjectsAsync(JiraConnection conn, CancellationToken ct = default)
        {
            Validate(conn);
            using var doc = await GetJsonAsync(conn, $"{BaseUrl(conn)}/rest/api/3/project/search?maxResults=50", ct);
            var list = new List<JiraProject>();
            if (doc != null && doc.RootElement.TryGetProperty("values", out var vals) && vals.ValueKind == JsonValueKind.Array)
                foreach (var p in vals.EnumerateArray())
                    list.Add(new JiraProject { Key = GetString(p, "key"), Name = GetString(p, "name") });
            return list;
        }

        // ── HTTP plumbing ───────────────────────────────────────────

        private Task<JsonDocument> GetJsonAsync(JiraConnection conn, string url, CancellationToken ct) =>
            SendJsonAsync(conn, HttpMethod.Get, url, body: null, ct);

        /// <summary>
        /// Sends an authenticated request (GET/POST/PUT) and returns the parsed
        /// JSON body, or null on 2xx with no content (e.g. the assignee PUT
        /// returns 204). Throws <see cref="AtlassianApiException"/> on failure.
        /// </summary>
        private async Task<JsonDocument> SendJsonAsync(
            JiraConnection conn, HttpMethod method, string url, JsonNode body, CancellationToken ct)
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
                    $"Could not reach Jira at {BaseUrl(conn)}: {ex.Message}", null, ex);
            }

            var content = await resp.Content.ReadAsStringAsync(ct);
            EnsureSuccessOrThrow(resp, content, url);

            if (string.IsNullOrWhiteSpace(content)) return null;   // 2xx with no body (204)
            try
            {
                return JsonDocument.Parse(content);
            }
            catch (JsonException ex)
            {
                throw new AtlassianApiException("Jira returned a non-JSON response.", resp.StatusCode, ex);
            }
        }

        private void EnsureSuccessOrThrow(HttpResponseMessage resp, string content, string url)
        {
            if (resp.IsSuccessStatusCode) return;

            _logger.LogWarning("[JiraClient] {Status} from {Url}: {Body}", resp.StatusCode, url, Truncate(content, 500));
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var reason = ExtractJiraReason(content);
                throw new AtlassianApiException(
                    "Jira rejected the request (HTTP " + (int)resp.StatusCode + "). " +
                    (string.IsNullOrEmpty(reason) ? "" : "Jira says: " + reason + ". ") +
                    "A scoped API token does not work against the site URL — use a classic " +
                    "(non-scoped) token, or check the token has not expired and the account has Jira access. " +
                    "Manage tokens at id.atlassian.com → Project Settings → Atlassian.",
                    resp.StatusCode);
            }
            throw new AtlassianApiException(
                $"Jira returned HTTP {(int)resp.StatusCode}: {ExtractJiraReason(content) ?? Truncate(content, 300)}",
                resp.StatusCode);
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

        /// <summary>
        /// Pulls a human reason out of a Jira error body. Jira returns either
        /// { "errorMessages": [...], "errors": {...} } or { "message": "..." }.
        /// An empty body (the typical "anonymous/ignored scoped token" 403) yields
        /// null so the caller can fall back to the generic guidance.
        /// </summary>
        private static string ExtractJiraReason(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return Truncate(body, 200);

                if (root.TryGetProperty("errorMessages", out var msgs) &&
                    msgs.ValueKind == JsonValueKind.Array && msgs.GetArrayLength() > 0)
                {
                    var parts = new List<string>();
                    foreach (var m in msgs.EnumerateArray())
                        if (m.ValueKind == JsonValueKind.String) parts.Add(m.GetString());
                    if (parts.Count > 0) return string.Join("; ", parts);
                }
                if (root.TryGetProperty("message", out var single) && single.ValueKind == JsonValueKind.String)
                    return single.GetString();

                return Truncate(body, 200);
            }
            catch (JsonException)
            {
                return Truncate(body, 200);
            }
        }
    }
}
