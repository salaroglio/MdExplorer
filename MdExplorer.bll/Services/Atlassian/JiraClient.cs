using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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
        private const string SummaryFields = "summary,status,priority,issuetype,duedate,assignee,description";
        private const int SummaryDescriptionMax = 200;
        private const string DetailFields = "summary,status,priority,issuetype,duedate,assignee,reporter,description,labels,comment,issuelinks,parent";

        // Field-metadata cache, keyed by site base URL. The custom-field catalog changes
        // rarely, so caching it for a few minutes avoids a /field round-trip on every
        // create/update/get. JiraClient is a singleton (see Startup), so this survives
        // across requests.
        private static readonly ConcurrentDictionary<string, CachedFields> _fieldCache = new();
        private static readonly TimeSpan FieldCacheTtl = TimeSpan.FromMinutes(10);
        private sealed class CachedFields
        {
            public DateTime FetchedUtc;
            public IReadOnlyList<JiraFieldMeta> Fields;
        }

        public JiraClient(IHttpClientFactory httpClientFactory, ILogger<JiraClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IReadOnlyList<JiraIssueSummary>> SearchAsync(
            JiraConnection conn, string jql, int maxResults,
            IReadOnlyList<string> customFieldSelect = null, CancellationToken ct = default)
        {
            Validate(conn);
            if (maxResults <= 0 || maxResults > 100) maxResults = 50;

            // Custom fields to surface on each row: an explicit selection (resolved to ids),
            // or — when the caller passes none — all of the site's custom fields (only the
            // populated ones come back and get mapped). meta is also used to label ids.
            var meta = await GetFieldMetaAsync(conn, ct);
            List<string> customIds;
            if (customFieldSelect != null && customFieldSelect.Count > 0)
                customIds = customFieldSelect.Select(s => s?.Trim())
                                             .Where(s => !string.IsNullOrEmpty(s))
                                             .Select(s => ResolveFieldByKey(meta, s).Id)
                                             .Distinct().ToList();
            else
                customIds = meta.Where(m => m.IsCustom && !string.IsNullOrEmpty(m.Id)).Select(m => m.Id).ToList();

            var requestedFields = customIds.Count == 0 ? SummaryFields : SummaryFields + "," + string.Join(",", customIds);

            // /rest/api/3/search (legacy) was removed; /search/jql is the current
            // endpoint. Pagination is token-based (nextPageToken) — for triage we
            // only need the first, most-urgent page so we don't follow it.
            var url = $"{BaseUrl(conn)}/rest/api/3/search/jql" +
                      $"?jql={Uri.EscapeDataString(jql)}" +
                      $"&maxResults={maxResults}" +
                      $"&fields={requestedFields}";

            using var doc = await GetJsonAsync(conn, url, ct);
            var result = new List<JiraIssueSummary>();
            if (doc.RootElement.TryGetProperty("issues", out var issues) &&
                issues.ValueKind == JsonValueKind.Array)
            {
                foreach (var issue in issues.EnumerateArray())
                {
                    var s = MapSummary(conn, issue);
                    if (issue.TryGetProperty("fields", out var f) && f.ValueKind == JsonValueKind.Object)
                        MapCustomFields(meta, f, s);
                    result.Add(s);
                }
            }
            return result;
        }

        public async Task<JiraIssueDetail> GetIssueAsync(
            JiraConnection conn, string issueKey, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(issueKey))
                throw new ArgumentException("issueKey is required", nameof(issueKey));

            // Fetch the field catalog up-front so we can (a) also request the custom
            // fields by id and (b) label them by name when mapping the response.
            var meta = await GetFieldMetaAsync(conn, ct);
            var customIds = meta.Where(m => m.IsCustom && !string.IsNullOrEmpty(m.Id)).Select(m => m.Id).ToList();
            var requestedFields = customIds.Count == 0 ? DetailFields : DetailFields + "," + string.Join(",", customIds);

            var url = $"{BaseUrl(conn)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey.Trim())}" +
                      $"?fields={requestedFields}";

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
                detail.StatusCategory = GetStatusCategory(f);
                detail.Priority = GetNestedName(f, "priority");
                detail.IssueType = GetNestedName(f, "issuetype");
                detail.DueDate = GetString(f, "duedate");
                detail.Assignee = GetNestedString(f, "assignee", "displayName");
                detail.Reporter = GetNestedString(f, "reporter", "displayName");
                detail.Parent = GetNestedString(f, "parent", "key");

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

                MapCustomFields(meta, f, detail);
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
            if (!string.IsNullOrWhiteSpace(req.ParentKey))
                fields["parent"] = new JsonObject { ["key"] = req.ParentKey.Trim() };

            await ApplyCustomFieldsAsync(conn, fields, req.CustomFields, ct);

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
            if (!string.IsNullOrWhiteSpace(req.ParentKey)) fields["parent"] = new JsonObject { ["key"] = req.ParentKey.Trim() };

            await ApplyCustomFieldsAsync(conn, fields, req.CustomFields, ct);

            if (fields.Count == 0)
                throw new AtlassianApiException("Nothing to update: provide at least one of summary/description/priority/dueDate/parentKey/customFields.");

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

        public async Task<IReadOnlyList<JiraIssueTypeStatuses>> GetProjectStatusesAsync(JiraConnection conn, string projectKey, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(projectKey)) throw new AtlassianApiException("projectKey is required.");
            using var doc = await GetJsonAsync(conn,
                $"{BaseUrl(conn)}/rest/api/3/project/{Uri.EscapeDataString(projectKey.Trim())}/statuses", ct);
            var result = new List<JiraIssueTypeStatuses>();
            if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var it in doc.RootElement.EnumerateArray())
                {
                    var entry = new JiraIssueTypeStatuses { IssueType = GetString(it, "name") };
                    if (it.TryGetProperty("statuses", out var sts) && sts.ValueKind == JsonValueKind.Array)
                        foreach (var s in sts.EnumerateArray())
                            entry.Statuses.Add(new JiraStatus
                            {
                                Name = GetString(s, "name"),
                                Category = GetNestedString(s, "statusCategory", "name")
                            });
                    result.Add(entry);
                }
            }
            return result;
        }

        public async Task<IReadOnlyList<JiraUser>> SearchUsersAsync(
            JiraConnection conn, string query, int maxResults, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(query))
                throw new AtlassianApiException("A name or email is required to search users.");
            if (maxResults <= 0 || maxResults > 50) maxResults = 20;

            // /user/search matches on display name and email (partial, case-insensitive).
            var url = $"{BaseUrl(conn)}/rest/api/3/user/search" +
                      $"?query={Uri.EscapeDataString(query.Trim())}&maxResults={maxResults}";
            using var doc = await GetJsonAsync(conn, url, ct);
            var list = new List<JiraUser>();
            if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var u in doc.RootElement.EnumerateArray())
                {
                    list.Add(new JiraUser
                    {
                        AccountId = GetString(u, "accountId"),
                        DisplayName = GetString(u, "displayName"),
                        EmailAddress = GetString(u, "emailAddress"),
                        AccountType = GetString(u, "accountType"),
                        Active = u.TryGetProperty("active", out var a) && a.ValueKind == JsonValueKind.True
                    });
                }
            }
            return list;
        }

        public async Task AssignIssueAsync(JiraConnection conn, string issueKey, string accountId, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(issueKey)) throw new AtlassianApiException("issueKey is required.");

            // A non-null string sets the assignee; a null value serializes as JSON null,
            // which is how Jira Cloud clears the assignee (unassign). Same endpoint used
            // by CreateIssueAsync to assign-to-self.
            var body = new JsonObject { ["accountId"] = string.IsNullOrWhiteSpace(accountId) ? null : accountId.Trim() };
            using var _ = await SendJsonAsync(conn, HttpMethod.Put,
                $"{BaseUrl(conn)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey.Trim())}/assignee", body, ct);
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

        // ── Attachments ─────────────────────────────────────────────

        public async Task<IReadOnlyList<JiraAttachment>> AttachFileAsync(
            JiraConnection conn, string issueKey, System.IO.Stream content, string fileName,
            CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(issueKey)) throw new AtlassianApiException("issueKey is required.");
            if (content == null) throw new AtlassianApiException("File content is required.");
            if (string.IsNullOrWhiteSpace(fileName)) throw new AtlassianApiException("fileName is required.");

            var url = $"{BaseUrl(conn)}/rest/api/3/issue/{Uri.EscapeDataString(issueKey.Trim())}/attachments";

            // Multipart, quindi fuori dal giro di SendJsonAsync (che manda JSON): qui si
            // riusano solo l'autenticazione e la traduzione degli errori.
            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", BasicToken(conn));
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // Difesa XSRF di Atlassian: senza questo header l'upload viene rifiutato. Non è
            // opzionale e non ha equivalenti negli altri endpoint.
            req.Headers.Add("X-Atlassian-Token", "no-check");

            using var form = new MultipartFormDataContent();
            var part = new StreamContent(content);           // stream, non ReadAllBytes: un allegato
            part.Headers.ContentType =                        // grande non deve finire tutto in RAM
                new MediaTypeHeaderValue("application/octet-stream");
            form.Add(part, "file", fileName);                 // il nome del campo DEVE essere "file"
            req.Content = form;

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

            var body = await resp.Content.ReadAsStringAsync(ct);
            EnsureSuccessOrThrow(resp, body, url);

            // La risposta è un ARRAY: Jira accetta più file per richiesta, noi ne mandiamo uno.
            var list = new List<JiraAttachment>();
            if (!string.IsNullOrWhiteSpace(body))
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        list.Add(new JiraAttachment
                        {
                            Id = GetString(el, "id"),
                            FileName = GetString(el, "filename"),
                            MimeType = GetString(el, "mimeType"),
                            ContentUrl = GetString(el, "content"),
                            Size = el.TryGetProperty("size", out var s) && s.TryGetInt64(out var n) ? n : 0,
                        });
                    }
                }
            }

            if (list.Count == 0)
                throw new AtlassianApiException(
                    "Jira accepted the upload but returned no attachment: the file may have been " +
                    "stripped by an attachment policy, or attachments may be disabled on this project.");

            return list;
        }

        // ── Custom fields ───────────────────────────────────────────

        public async Task<IReadOnlyList<JiraFieldMeta>> ListFieldsAsync(
            JiraConnection conn, bool customOnly = true, string nameFilter = null, CancellationToken ct = default)
        {
            Validate(conn);
            var meta = await GetFieldMetaAsync(conn, ct);

            IEnumerable<JiraFieldMeta> q = meta;
            if (customOnly) q = q.Where(m => m.IsCustom);
            if (!string.IsNullOrWhiteSpace(nameFilter))
            {
                var needle = nameFilter.Trim();
                q = q.Where(m => (m.Name ?? string.Empty).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0
                              || (m.Id ?? string.Empty).IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Project into fresh instances: GetFieldMetaAsync hands back the cached
            // objects, and ValueHint must not be written onto shared state.
            return q.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(m => new JiraFieldMeta
                    {
                        Id = m.Id,
                        Name = m.Name,
                        IsCustom = m.IsCustom,
                        SchemaType = m.SchemaType,
                        ItemsType = m.ItemsType,
                        ValueHint = DescribeAcceptedValue(m)
                    })
                    .ToList();
        }

        /// <summary>
        /// Describes what a caller may pass for a field — the read-side mirror of
        /// <see cref="CoerceCustomValue"/>. Keep the two in step: every schema type the
        /// coercion accepts as a scalar must say so here, and every type it rejects must
        /// be reported as needing a structured value.
        /// </summary>
        private static string DescribeAcceptedValue(JiraFieldMeta meta)
        {
            switch (meta.SchemaType)
            {
                case "string":   return "scalar — a string, e.g. \"some text\".";
                case "number":   return "scalar — a number, e.g. 5.";
                case "date":     return "scalar — a date \"yyyy-MM-dd\".";
                case "datetime": return "scalar — an ISO-8601 timestamp.";
                case "any":      return "scalar — passed through unchanged.";
                case "option":   return "scalar — the option label; it is wrapped as {\"value\": ...}.";
                case "user":     return "scalar — the user's accountId; it is wrapped as {\"accountId\": ...}.";
                case "array":
                    switch (meta.ItemsType)
                    {
                        case "string": return "scalar (wrapped into a 1-element array) or a JSON array of strings.";
                        case "option": return "scalar option label (wrapped as [{\"value\": ...}]) or a JSON array.";
                        case "user":   return "scalar accountId (wrapped as [{\"accountId\": ...}]) or a JSON array.";
                        default:       return $"structured — a JSON array of '{meta.ItemsType ?? "?"}' in Jira's own shape.";
                    }
                default:
                    return $"structured — schema type '{meta.SchemaType ?? "(unknown)"}' needs the JSON object/array Jira expects.";
            }
        }

        /// <summary>
        /// Fetches the site's field catalog (/rest/api/3/field), cached per site for a
        /// few minutes. Custom fields expose the id ("customfield_10016"), the human
        /// name and the schema we need to shape a value on write.
        /// </summary>
        private async Task<IReadOnlyList<JiraFieldMeta>> GetFieldMetaAsync(JiraConnection conn, CancellationToken ct)
        {
            var siteKey = BaseUrl(conn);
            if (_fieldCache.TryGetValue(siteKey, out var cached) &&
                (DateTime.UtcNow - cached.FetchedUtc) < FieldCacheTtl)
                return cached.Fields;

            using var doc = await GetJsonAsync(conn, $"{siteKey}/rest/api/3/field", ct);
            var list = new List<JiraFieldMeta>();
            if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    var meta = new JiraFieldMeta
                    {
                        Id = GetString(el, "id"),
                        Name = GetString(el, "name"),
                        IsCustom = el.TryGetProperty("custom", out var c) && c.ValueKind == JsonValueKind.True
                    };
                    if (el.TryGetProperty("schema", out var sc) && sc.ValueKind == JsonValueKind.Object)
                    {
                        meta.SchemaType = GetString(sc, "type");
                        meta.ItemsType = GetString(sc, "items");
                    }
                    if (!string.IsNullOrEmpty(meta.Id)) list.Add(meta);
                }
            }
            _fieldCache[siteKey] = new CachedFields { FetchedUtc = DateTime.UtcNow, Fields = list };
            return list;
        }

        /// <summary>
        /// Resolves each caller-supplied custom field (by human name or raw customfield_
        /// id), shapes its value from the field schema, and writes it into <paramref name="fields"/>.
        /// Throws (never guesses) when a name is unknown or ambiguous — a scoped write
        /// must be deterministic.
        /// </summary>
        private async Task ApplyCustomFieldsAsync(
            JiraConnection conn, JsonObject fields, JsonObject customFields, CancellationToken ct)
        {
            if (customFields == null || customFields.Count == 0) return;

            var meta = await GetFieldMetaAsync(conn, ct);

            foreach (var kvp in customFields)
            {
                var key = kvp.Key?.Trim();
                if (string.IsNullOrEmpty(key)) continue;
                var target = ResolveFieldByKey(meta, key);
                fields[target.Id] = CoerceCustomValue(key, kvp.Value, target);
            }
        }

        /// <summary>
        /// Maps a caller-supplied key — a human field name or a raw customfield_ id — to its
        /// field definition. An explicit id resolves to the known schema, or to an id-only stub
        /// when the field is unknown (that path then requires a structured value). A name that
        /// matches nothing, or matches more than one field, throws (never guesses).
        /// </summary>
        private static JiraFieldMeta ResolveFieldByKey(IReadOnlyList<JiraFieldMeta> meta, string key)
        {
            if (Regex.IsMatch(key, @"^customfield_\d+$", RegexOptions.IgnoreCase))
                return meta.FirstOrDefault(m => string.Equals(m.Id, key, StringComparison.OrdinalIgnoreCase))
                       ?? new JiraFieldMeta { Id = key, IsCustom = true };

            var matches = meta.Where(m => string.Equals(m.Name, key, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0)
            {
                var available = string.Join(", ", meta.Where(m => m.IsCustom && !string.IsNullOrEmpty(m.Name))
                                                        .Select(m => m.Name).Distinct().OrderBy(n => n));
                throw new AtlassianApiException(
                    $"Custom field '{key}' not found on this Jira site. Available custom fields: {available}.");
            }
            if (matches.Count > 1)
            {
                var ids = string.Join(", ", matches.Select(m => $"{m.Name} ({m.Id})"));
                throw new AtlassianApiException(
                    $"Custom field name '{key}' is ambiguous — it matches: {ids}. Pass the exact customfield_ id instead.");
            }
            return matches[0];
        }

        /// <summary>
        /// Shapes a caller value into the JSON Jira expects for the field's schema type.
        /// A structured value (object/array) is trusted as-is — the explicit escape hatch
        /// for field types we don't coerce. A JSON null clears the field. A scalar for an
        /// unknown/unsupported schema type throws (rather than sending a shape Jira will reject).
        /// </summary>
        private static JsonNode CoerceCustomValue(string keyForError, JsonNode value, JiraFieldMeta meta)
        {
            if (value == null) return null;                       // JSON null → clear the field
            if (value is JsonObject || value is JsonArray)
                return value.DeepClone();                         // caller took control of the shape

            switch (meta.SchemaType)
            {
                case "string":
                case "number":
                case "date":
                case "datetime":
                case "any":
                    return value.DeepClone();
                case "option":
                    return new JsonObject { ["value"] = value.DeepClone() };
                case "user":
                    return new JsonObject { ["accountId"] = value.DeepClone() };
                case "array":
                    switch (meta.ItemsType)
                    {
                        case "string": return new JsonArray(value.DeepClone());
                        case "option": return new JsonArray(new JsonObject { ["value"] = value.DeepClone() });
                        case "user":   return new JsonArray(new JsonObject { ["accountId"] = value.DeepClone() });
                        default:
                            throw new AtlassianApiException(
                                $"Custom field '{meta.Name}' is an array of '{meta.ItemsType}' — pass a JSON array in Jira's shape.");
                    }
                default:
                    throw new AtlassianApiException(
                        $"Custom field '{meta.Name ?? meta.Id}' has schema type '{meta.SchemaType ?? "(unknown)"}', which " +
                        "needs a structured value. Pass it already in Jira's JSON shape (object/array).");
            }
        }

        /// <summary>
        /// Reads back the custom fields that carry a value, keyed by their human name and
        /// flattened to a readable scalar/list. Uses the field catalog already fetched by
        /// the caller to label ids.
        /// </summary>
        private static void MapCustomFields(
            IReadOnlyList<JiraFieldMeta> meta, JsonElement fields, JiraIssueSummary detail)
        {
            var nameById = meta.Where(m => m.IsCustom && !string.IsNullOrEmpty(m.Id))
                               .GroupBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
                               .ToDictionary(g => g.Key,
                                             g => string.IsNullOrEmpty(g.First().Name) ? g.Key : g.First().Name,
                                             StringComparer.OrdinalIgnoreCase);

            foreach (var prop in fields.EnumerateObject())
            {
                if (!prop.Name.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase)) continue;
                if (prop.Value.ValueKind == JsonValueKind.Null) continue;

                var flat = FlattenCustomValue(prop.Value);
                if (flat == null) continue;

                var label = nameById.TryGetValue(prop.Name, out var n) ? n : prop.Name;
                detail.CustomFields[label] = flat;
            }
        }

        /// <summary>Flattens a raw custom-field JSON value to a readable scalar/list for display.</summary>
        private static object FlattenCustomValue(JsonElement v)
        {
            switch (v.ValueKind)
            {
                case JsonValueKind.String: return v.GetString();
                case JsonValueKind.Number: return v.TryGetInt64(out var l) ? (object)l : v.GetDouble();
                case JsonValueKind.True: return true;
                case JsonValueKind.False: return false;
                case JsonValueKind.Array:
                    var items = new List<object>();
                    foreach (var el in v.EnumerateArray())
                    {
                        var fx = FlattenCustomValue(el);
                        if (fx != null) items.Add(fx);
                    }
                    return items.Count == 0 ? null : items;
                case JsonValueKind.Object:
                    // ADF rich text
                    if (v.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String && t.GetString() == "doc")
                        return AdfRenderer.ToText(v);
                    // select / option
                    if (v.TryGetProperty("value", out var val) && val.ValueKind == JsonValueKind.String)
                        return val.GetString();
                    // user
                    if (v.TryGetProperty("displayName", out var dn) && dn.ValueKind == JsonValueKind.String)
                        return dn.GetString();
                    // priority / version / component / …
                    if (v.TryGetProperty("name", out var nm) && nm.ValueKind == JsonValueKind.String)
                        return nm.GetString();
                    return v.GetRawText();
                default: return null;
            }
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
                s.StatusCategory = GetStatusCategory(f);
                if (f.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.Object)
                    s.Description = Truncate(AdfRenderer.ToText(desc), SummaryDescriptionMax);
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

        // fields.status.statusCategory.name — three levels deep.
        private static string GetStatusCategory(JsonElement fields)
        {
            if (fields.TryGetProperty("status", out var st) && st.ValueKind == JsonValueKind.Object &&
                st.TryGetProperty("statusCategory", out var sc) && sc.ValueKind == JsonValueKind.Object &&
                sc.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String)
                return n.GetString();
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
