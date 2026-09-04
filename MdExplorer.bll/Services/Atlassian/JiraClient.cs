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

        // Priority names, same cache shape and lifetime. The site's priority scheme is
        // about as stable as its field catalog, and we read it on every write that sets
        // a priority.
        private static readonly ConcurrentDictionary<string, CachedPriorities> _priorityCache = new();
        private sealed class CachedPriorities
        {
            public DateTime FetchedUtc;
            public IReadOnlyList<string> Names;
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

            // Resolve the accountId up-front so we can assign via the dedicated
            // /assignee endpoint (unambiguous on Cloud). An explicit assignee wins
            // over assign-to-self; the caller has already turned a name into an id.
            string accountId = null;
            if (!string.IsNullOrWhiteSpace(req.AssigneeAccountId))
                accountId = req.AssigneeAccountId.Trim();
            else if (req.AssignToSelf)
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
                fields["priority"] = new JsonObject { ["name"] = await ResolvePriorityAsync(conn, req.Priority, ct) };
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
            if (!string.IsNullOrWhiteSpace(req.Priority)) fields["priority"] = new JsonObject { ["name"] = await ResolvePriorityAsync(conn, req.Priority, ct) };
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

        /// <summary>
        /// Resolves a free-text name/email to candidate Jira users. Tries the literal
        /// query first; only if that finds nothing does it fall back to the individual
        /// name tokens and the reversed order (handles "Mario Rossi" vs "Rossi Mario",
        /// partials, comma forms). Keeps only active, real ("atlassian") accounts and
        /// de-duplicates by accountId. Returning every candidate rather than picking one
        /// is deliberate: assigning work to the wrong person is not a guess worth making,
        /// so the caller disambiguates.
        /// </summary>
        public async Task<IReadOnlyList<JiraUser>> ResolveUsersAsync(
            JiraConnection conn, string query, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(query))
                throw new AtlassianApiException("A name or email is required to resolve a user.");

            var seen = new Dictionary<string, JiraUser>(StringComparer.OrdinalIgnoreCase);

            async Task AddMatches(string q)
            {
                if (string.IsNullOrWhiteSpace(q)) return;
                var found = await SearchUsersAsync(conn, q.Trim(), 20, ct);
                foreach (var u in found)
                {
                    if (string.IsNullOrEmpty(u.AccountId) || !u.Active) continue;
                    if (!string.IsNullOrEmpty(u.AccountType) &&
                        !string.Equals(u.AccountType, "atlassian", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!seen.ContainsKey(u.AccountId)) seen[u.AccountId] = u;
                }
            }

            var norm = Regex.Replace(query.Trim(), @"\s+", " ");
            await AddMatches(norm);

            if (seen.Count == 0)
            {
                var tokens = norm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length > 1)
                {
                    await AddMatches(tokens[tokens.Length - 1]);          // surname
                    await AddMatches(tokens[0]);                          // first name
                    await AddMatches(string.Join(" ", tokens.Reverse())); // reversed order
                }
            }

            return seen.Values.ToList();
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

        // ── Create screen & project versions ────────────────────────

        /// <summary>
        /// Reads the fields the CREATE screen of a project + issue type actually exposes
        /// (/issue/createmeta/{project}/issuetypes/{id}). The site field catalog cannot
        /// answer this: it lists every field that exists somewhere, so a caller picking
        /// from it can choose one Jira will then refuse as "not on the appropriate screen".
        /// </summary>
        public async Task<JiraCreateFieldsResult> GetCreateFieldsAsync(
            JiraConnection conn, string projectKey, string issueType, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(projectKey)) throw new AtlassianApiException("projectKey is required.");
            if (string.IsNullOrWhiteSpace(issueType)) throw new AtlassianApiException("issueType is required.");

            var project = Uri.EscapeDataString(projectKey.Trim());
            var wanted = issueType.Trim();

            // 1. Issue type name -> id. Type names are localised per site, so match the
            //    untranslated name too: "Epic" has to work on an Italian site as well.
            string typeId = null, typeName = null, untranslated = null;
            var available = new List<string>();
            using (var types = await GetJsonAsync(conn,
                $"{BaseUrl(conn)}/rest/api/3/issue/createmeta/{project}/issuetypes?maxResults={CreateMetaPageSize}", ct))
            {
                if (types != null && types.RootElement.TryGetProperty("values", out var vals) &&
                    vals.ValueKind == JsonValueKind.Array)
                {
                    foreach (var t in vals.EnumerateArray())
                    {
                        var n = GetString(t, "name");
                        var u = GetString(t, "untranslatedName");
                        available.Add(string.IsNullOrEmpty(u) || string.Equals(u, n, StringComparison.Ordinal) ? n : $"{n} ({u})");
                        if (typeId == null &&
                            (string.Equals(n, wanted, StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(u, wanted, StringComparison.OrdinalIgnoreCase)))
                        {
                            typeId = GetString(t, "id");
                            typeName = n;
                            untranslated = u;
                        }
                    }
                }
            }

            if (typeId == null)
                throw new AtlassianApiException(
                    $"Issue type '{wanted}' cannot be created in project '{projectKey.Trim()}'. " +
                    $"Creatable types here: {(available.Count == 0 ? "(none — check the project key and your permissions)" : string.Join(", ", available))}.");

            var result = new JiraCreateFieldsResult
            {
                ProjectKey = projectKey.Trim(),
                IssueType = typeName,
                IssueTypeId = typeId,
                IssueTypeUntranslated = string.Equals(untranslated, typeName, StringComparison.Ordinal) ? null : untranslated
            };

            // 2. The screen itself. Paginated — a busy create screen runs past one page.
            var startAt = 0;
            while (true)
            {
                using var page = await GetJsonAsync(conn,
                    $"{BaseUrl(conn)}/rest/api/3/issue/createmeta/{project}/issuetypes/{Uri.EscapeDataString(typeId)}" +
                    $"?startAt={startAt}&maxResults={CreateMetaPageSize}", ct);
                if (page == null || !page.RootElement.TryGetProperty("values", out var fields) ||
                    fields.ValueKind != JsonValueKind.Array)
                    break;

                var read = 0;
                foreach (var f in fields.EnumerateArray()) { result.Fields.Add(MapCreateField(f)); read++; }
                startAt += read;

                var isLast = page.RootElement.TryGetProperty("isLast", out var l) && l.ValueKind == JsonValueKind.True;
                var total = page.RootElement.TryGetProperty("total", out var t) && t.ValueKind == JsonValueKind.Number
                    ? t.GetInt32() : startAt;
                if (read == 0 || isLast || startAt >= total) break;
            }

            return result;
        }

        /// <summary>Maps one createmeta field entry, reusing the write-side value hint.</summary>
        private static JiraCreateField MapCreateField(JsonElement f)
        {
            var meta = new JiraFieldMeta
            {
                Id = GetString(f, "fieldId") ?? GetString(f, "key"),
                Name = GetString(f, "name")
            };
            if (f.TryGetProperty("schema", out var sc) && sc.ValueKind == JsonValueKind.Object)
            {
                meta.SchemaType = GetString(sc, "type");
                meta.ItemsType = GetString(sc, "items");
            }
            meta.IsCustom = meta.Id != null && meta.Id.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase);

            var field = new JiraCreateField
            {
                Id = meta.Id,
                Name = meta.Name,
                Required = f.TryGetProperty("required", out var r) && r.ValueKind == JsonValueKind.True,
                SchemaType = meta.SchemaType,
                ItemsType = meta.ItemsType,
                ValueHint = DescribeAcceptedValue(meta)
            };

            if (f.TryGetProperty("allowedValues", out var allowed) && allowed.ValueKind == JsonValueKind.Array)
            {
                var labels = new List<string>();
                foreach (var v in allowed.EnumerateArray())
                {
                    if (labels.Count >= AllowedValuesMax) { field.AllowedValuesTruncated = true; break; }
                    var label = v.ValueKind == JsonValueKind.String
                        ? v.GetString()
                        : GetString(v, "name") ?? GetString(v, "value") ?? GetString(v, "id");
                    if (!string.IsNullOrEmpty(label)) labels.Add(label);
                }
                if (labels.Count > 0) field.AllowedValues = labels;
            }
            return field;
        }

        /// <summary>
        /// Lists a project's versions (/project/{key}/versions) — the values a
        /// fixVersions/versions field will accept. Checking here beats discovering
        /// from a 400 that the release name was spelled differently.
        /// </summary>
        public async Task<IReadOnlyList<JiraVersion>> GetProjectVersionsAsync(
            JiraConnection conn, string projectKey, CancellationToken ct = default)
        {
            Validate(conn);
            if (string.IsNullOrWhiteSpace(projectKey)) throw new AtlassianApiException("projectKey is required.");

            using var doc = await GetJsonAsync(conn,
                $"{BaseUrl(conn)}/rest/api/3/project/{Uri.EscapeDataString(projectKey.Trim())}/versions", ct);
            var list = new List<JiraVersion>();
            if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Array)
                foreach (var v in doc.RootElement.EnumerateArray())
                    list.Add(new JiraVersion
                    {
                        Id = GetString(v, "id"),
                        Name = GetString(v, "name"),
                        Released = v.TryGetProperty("released", out var rel) && rel.ValueKind == JsonValueKind.True,
                        Archived = v.TryGetProperty("archived", out var arc) && arc.ValueKind == JsonValueKind.True,
                        ReleaseDate = GetString(v, "releaseDate")
                    });
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
                    AtlassianNetworkDiagnostics.DescribeUnreachable("Jira", BaseUrl(conn), ex), null, ex);
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
                case "user":     return "scalar — the person's name, email or accountId; a name is looked up " +
                                        "and wrapped as {\"accountId\": ...}.";
                case "version":
                case "component":
                case "group":
                case "priority": return $"scalar — the {meta.SchemaType} name; it is wrapped as {{\"name\": ...}}.";
                case "array":
                    switch (meta.ItemsType)
                    {
                        case "string": return "scalar (wrapped into a 1-element array) or a JSON array of strings.";
                        case "option": return "scalar option label (wrapped as [{\"value\": ...}]) or a JSON array.";
                        case "user":   return "scalar name/email/accountId (looked up, then wrapped as " +
                                              "[{\"accountId\": ...}]) or a JSON array.";
                        case "version":
                        case "component":
                        case "group":  return $"scalar {meta.ItemsType} name (wrapped as [{{\"name\": ...}}]) or a JSON array.";
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
        /// Resolves each caller-supplied field (by id or human name — custom fields and
        /// system fields such as fixVersions/reporter alike), shapes its value from the
        /// field schema, and writes it into <paramref name="fields"/>. Throws (never
        /// guesses) when a key is unknown or ambiguous — a scoped write must be
        /// deterministic.
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

                // A scalar string for a user field ("Enrico Torrelli") or for priority
                // ("Bassa") is resolved here, before the shaping: both are cases where
                // the caller knows a human label and Jira wants something else, and both
                // must fail BEFORE anything is written rather than as a 400 afterwards.
                var value = kvp.Value;
                var scalar = value as JsonValue;
                if (scalar != null && scalar.GetValueKind() == JsonValueKind.String)
                {
                    var raw = scalar.GetValue<string>();
                    if (IsUserField(target))
                        value = JsonValue.Create(await ResolveUserScalarAsync(conn, target, raw, ct));
                    else if (string.Equals(target.Id, "priority", StringComparison.OrdinalIgnoreCase))
                        value = JsonValue.Create(await ResolvePriorityAsync(conn, raw, ct));
                }

                fields[target.Id] = CoerceCustomValue(key, value, target);
            }
        }

        /// <summary>A field Jira wants an accountId for — a single user or a list of them.</summary>
        private static bool IsUserField(JiraFieldMeta meta) =>
            string.Equals(meta.SchemaType, "user", StringComparison.OrdinalIgnoreCase) ||
            (string.Equals(meta.SchemaType, "array", StringComparison.OrdinalIgnoreCase) &&
             string.Equals(meta.ItemsType, "user", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Turns what a caller wrote for a user field into the accountId Jira needs. An
        /// accountId is passed through; anything else is looked up by name/email, and 0
        /// or >1 matches throw with the candidates rather than writing the wrong person.
        /// </summary>
        private async Task<string> ResolveUserScalarAsync(
            JiraConnection conn, JiraFieldMeta target, string raw, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            if (LooksLikeAccountId(raw)) return raw.Trim();

            var label = target.Name ?? target.Id;
            var candidates = await ResolveUsersAsync(conn, raw, ct);

            if (candidates.Count == 0)
                throw new AtlassianApiException(
                    $"Field '{label}': no Jira user matches '{raw.Trim()}' — nothing was written. Try a " +
                    "different spelling, just the surname, or an email; or pass the accountId, or the " +
                    "value already shaped as {\"accountId\": \"...\"}. Note that setting a reporter also " +
                    "needs the \"Modify Reporter\" permission on your account.");

            if (candidates.Count > 1)
                throw new AtlassianApiException(
                    $"Field '{label}': {candidates.Count} Jira users match '{raw.Trim()}' — nothing was " +
                    $"written. Candidates: {string.Join("; ", candidates.Select(u => $"{u.DisplayName} <{u.EmailAddress}> = {u.AccountId}"))}. " +
                    "Ask the user which one, then pass that accountId.");

            return candidates[0].AccountId;
        }

        /// <summary>
        /// Tells an accountId from a person's name. Jira Cloud ids are either 24 chars of
        /// [0-9a-z] (the legacy form) or namespaced with a colon ("557058:uuid",
        /// "qm:uuid:uuid"); none of them contains whitespace or '@'. Anything else is
        /// taken to be a name to look up — and when that guess is wrong the caller still
        /// has the explicit shape, {"accountId": "..."}, which is passed through untouched.
        /// </summary>
        private static bool LooksLikeAccountId(string value)
        {
            var v = value.Trim();
            if (v.Length == 0) return false;
            if (v.Any(char.IsWhiteSpace) || v.IndexOf('@') >= 0) return false;
            if (v.IndexOf(':') >= 0) return true;
            return Regex.IsMatch(v, "^[0-9a-zA-Z]{24}$");
        }

        /// <summary>
        /// Validates a priority name against the site's own list and returns it with the
        /// stored casing. Jira localises priority labels in the UI but the API only takes
        /// the stored name, so "Bassa" is rejected with a 400 that says nothing useful —
        /// this turns that into an error naming the valid values, before anything is written.
        /// </summary>
        private async Task<string> ResolvePriorityAsync(JiraConnection conn, string priority, CancellationToken ct)
        {
            var wanted = priority.Trim();
            var names = await GetPriorityNamesAsync(conn, ct);

            // No list (an old site, or a token without the permission to read it): leave
            // the value alone and let Jira be the authority, exactly as before.
            if (names.Count == 0) return wanted;

            var match = names.FirstOrDefault(n => string.Equals(n, wanted, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;

            throw new AtlassianApiException(
                $"Priority '{wanted}' does not exist on this Jira site. Valid priorities: {string.Join(", ", names)}. " +
                "Jira wants the name it stores, which stays English on a localised site (use 'Low', not 'Bassa'). " +
                "A project can also accept only some of these — JiraGetCreateFields lists the ones valid for the " +
                "issue type you are creating.");
        }

        /// <summary>Site priority names (/rest/api/3/priority), cached per site.</summary>
        private async Task<IReadOnlyList<string>> GetPriorityNamesAsync(JiraConnection conn, CancellationToken ct)
        {
            var siteKey = BaseUrl(conn);
            if (_priorityCache.TryGetValue(siteKey, out var cached) &&
                (DateTime.UtcNow - cached.FetchedUtc) < FieldCacheTtl)
                return cached.Names;

            var names = new List<string>();
            try
            {
                using var doc = await GetJsonAsync(conn, $"{siteKey}/rest/api/3/priority", ct);
                if (doc != null && doc.RootElement.ValueKind == JsonValueKind.Array)
                    foreach (var pr in doc.RootElement.EnumerateArray())
                    {
                        var n = GetString(pr, "name");
                        if (!string.IsNullOrEmpty(n)) names.Add(n);
                    }
            }
            catch (AtlassianApiException ex)
            {
                // Not being able to READ the priority list must not block a WRITE that
                // might well be valid: fall back to letting Jira judge the value.
                _logger.LogWarning(ex, "[JiraClient] Could not read the priority list from {Site}", siteKey);
            }

            _priorityCache[siteKey] = new CachedPriorities { FetchedUtc = DateTime.UtcNow, Names = names };
            return names;
        }

        /// <summary>
        /// Maps a caller-supplied key — a field id or a human field name — to its field
        /// definition. Any id in the site catalog resolves, system fields included
        /// ("fixVersions", "reporter", …), so a caller is never forced through the
        /// localised name; an unknown customfield_ id resolves to an id-only stub (that
        /// path then requires a structured value). Names are matched case-insensitively.
        /// A key that matches nothing, or more than one field, throws (never guesses).
        /// </summary>
        private static JiraFieldMeta ResolveFieldByKey(IReadOnlyList<JiraFieldMeta> meta, string key)
        {
            // Id first: ids are stable and language-independent, names are localised per
            // site, so an exact id is the one unambiguous way to name a field.
            var byId = meta.FirstOrDefault(m => string.Equals(m.Id, key, StringComparison.OrdinalIgnoreCase));
            if (byId != null) return byId;

            if (Regex.IsMatch(key, @"^customfield_\d+$", RegexOptions.IgnoreCase))
                return new JiraFieldMeta { Id = key, IsCustom = true };

            var matches = meta.Where(m => string.Equals(m.Name, key, StringComparison.OrdinalIgnoreCase)).ToList();
            if (matches.Count == 0)
                throw new AtlassianApiException(
                    $"Field '{key}' not found on this Jira site. {DescribeNearMatches(meta, key)} " +
                    "Field names are localised, ids are not: pass the name exactly as Jira spells it, " +
                    "or the id — a system id such as 'fixVersions'/'reporter'/'components', or " +
                    "'customfield_XXXXX'. Call JiraListFields with customOnly=false and a nameFilter " +
                    "to look the field up.");
            if (matches.Count > 1)
            {
                var ids = string.Join(", ", matches.Select(m => $"{m.Name} ({m.Id})"));
                throw new AtlassianApiException(
                    $"Field name '{key}' is ambiguous — it matches: {ids}. Pass the exact field id instead.");
            }
            return matches[0];
        }

        /// <summary>
        /// Builds the "did you mean" half of an unknown-field error: the fields whose name
        /// or id contains the key (or vice versa), system fields included. Listing only the
        /// custom fields — as this used to — pushes a caller towards a same-named custom
        /// field when the field it actually wants is a system one.
        /// </summary>
        private static string DescribeNearMatches(IReadOnlyList<JiraFieldMeta> meta, string key)
        {
            bool Touches(string candidate) =>
                !string.IsNullOrEmpty(candidate) &&
                (candidate.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 key.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0);

            var near = meta.Where(m => Touches(m.Name) || Touches(m.Id))
                           .Select(m => $"{m.Name ?? "(unnamed)"} ({m.Id})")
                           .Distinct()
                           .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                           .Take(15)
                           .ToList();

            return near.Count == 0
                ? "No field on this site has a similar name or id."
                : $"Closest matches: {string.Join(", ", near)}.";
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
                // Named entities — the caller knows them by their label ("REL. Q4 2026",
                // "High"), and Jira takes that label in a {"name": ...} envelope.
                case "version":
                case "component":
                case "group":
                case "priority":
                    return new JsonObject { ["name"] = value.DeepClone() };
                case "array":
                    switch (meta.ItemsType)
                    {
                        case "string": return new JsonArray(value.DeepClone());
                        case "option": return new JsonArray(new JsonObject { ["value"] = value.DeepClone() });
                        case "user":   return new JsonArray(new JsonObject { ["accountId"] = value.DeepClone() });
                        case "version":
                        case "component":
                        case "group":
                            return new JsonArray(new JsonObject { ["name"] = value.DeepClone() });
                        default:
                            throw new AtlassianApiException(
                                $"Field '{meta.Name ?? meta.Id}' is an array of '{meta.ItemsType}' — pass a JSON array in Jira's shape.");
                    }
                default:
                    throw new AtlassianApiException(
                        $"Field '{meta.Name ?? meta.Id}' has schema type '{meta.SchemaType ?? "(unknown)"}', which " +
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
                    AtlassianNetworkDiagnostics.DescribeUnreachable("Jira", BaseUrl(conn), ex), null, ex);
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

        // createmeta is paginated; 100 covers a create screen in one round-trip in
        // practice, and the loop still follows isLast/total when it does not.
        private const int CreateMetaPageSize = 100;

        // A select can carry hundreds of options; past a few dozen the list stops being
        // useful to read and starts crowding out the rest of the answer.
        private const int AllowedValuesMax = 50;

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
