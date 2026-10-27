using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.Atlassian;

namespace MdExplorer.Features.Services.Redmine
{
    public interface IRedmineClient
    {
        /// <summary>Chi è l'utente della chiave: prova della connessione.</summary>
        Task<RedmineUser> VerifyAsync(RedmineConnection conn, CancellationToken ct = default);
        Task<IReadOnlyList<RedmineIssueSummary>> SearchIssuesAsync(RedmineConnection conn, RedmineIssueQuery query, CancellationToken ct = default);
        Task<RedmineIssueDetail> GetIssueAsync(RedmineConnection conn, int id, CancellationToken ct = default);
        Task<RedmineProjectOptions> ProjectOptionsAsync(RedmineConnection conn, string project, CancellationToken ct = default);
        Task<RedmineIssueDetail> CreateIssueAsync(RedmineConnection conn, RedmineIssueChange change, CancellationToken ct = default);
        Task<RedmineIssueDetail> UpdateIssueAsync(RedmineConnection conn, int id, RedmineIssueChange change, CancellationToken ct = default);
    }

    /// <summary>
    /// Il client dell'API REST di Redmine (docs-internal/Sprints/2026-10-27-Redmine.md). I fatti su cui si regge sono stati
    /// provati su Redmine 6.0 (F0), comprese due trappole: chi non ha il permesso di creare riceve un 422 fuorviante
    /// («Progetto non può essere lasciato in bianco»), e un aggiornamento con un valore non ammesso risponde 204 senza
    /// cambiare niente. Per questo le scritture controllano i permessi prima e rileggono la issue dopo (RM8).
    /// </summary>
    public class RedmineClient : IRedmineClient
    {
        private const string ApiKeyHeader = "X-Redmine-API-Key";
        private readonly IHttpClientFactory _httpClientFactory;

        public RedmineClient(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        // ── Lettura ─────────────────────────────────────────────────

        public async Task<RedmineUser> VerifyAsync(RedmineConnection conn, CancellationToken ct = default)
        {
            using var doc = await GetJsonAsync(conn, "/users/current.json", ct);
            return MapUser(Prop(doc.RootElement, "user"));
        }

        public async Task<IReadOnlyList<RedmineIssueSummary>> SearchIssuesAsync(RedmineConnection conn, RedmineIssueQuery query, CancellationToken ct = default)
        {
            query ??= new RedmineIssueQuery();
            var limit = query.Limit <= 0 ? 25 : Math.Min(query.Limit, 100);

            var common = new List<string> { "sort=updated_on:desc", $"limit={limit}" };
            if (query.AssignedToMe) common.Add("assigned_to_id=me");
            common.Add("status_id=" + Uri.EscapeDataString(await StatusFilterAsync(conn, query.Status, ct)));
            if (!string.IsNullOrWhiteSpace(query.Tracker))
            {
                var trackers = await RefsAsync(conn, "/trackers.json", "trackers", ct);
                common.Add($"tracker_id={Match(trackers, query.Tracker, Say("tracker", "tracker")).Id}");
            }
            if (!string.IsNullOrWhiteSpace(query.Text))
                common.Add("any_searchable=" + Uri.EscapeDataString("~" + query.Text.Trim()));

            var projects = (query.Projects ?? new List<string>()).Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim()).Distinct().ToList();
            var urls = projects.Count == 0
                ? new List<string> { "/issues.json?" + string.Join("&", common) }
                : projects.Select(p => $"/issues.json?project_id={Uri.EscapeDataString(p)}&" + string.Join("&", common)).ToList();

            var result = new List<RedmineIssueSummary>();
            foreach (var url in urls)
            {
                using var doc = await GetJsonAsync(conn, url, ct);
                foreach (var el in Array(doc.RootElement, "issues"))
                    result.Add(MapSummary(conn, el, new RedmineIssueSummary()));
            }
            return result.GroupBy(i => i.Id).Select(g => g.First())
                         .OrderByDescending(i => i.UpdatedOn).Take(limit).ToList();
        }

        public async Task<RedmineIssueDetail> GetIssueAsync(RedmineConnection conn, int id, CancellationToken ct = default)
        {
            using var doc = await GetJsonAsync(conn, $"/issues/{id}.json?include=journals,attachments,allowed_statuses", ct);
            return MapDetail(conn, Prop(doc.RootElement, "issue"));
        }

        public async Task<RedmineProjectOptions> ProjectOptionsAsync(RedmineConnection conn, string project, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new RedmineApiException(Say("Manca il progetto (identificativo o id).", "The project (identifier or id) is missing."));
            using var doc = await GetJsonAsync(conn, $"/projects/{Uri.EscapeDataString(project.Trim())}.json?include=trackers,issue_custom_fields", ct);
            var p = Prop(doc.RootElement, "project");
            var options = new RedmineProjectOptions
            {
                Id = Int(p, "id") ?? 0,
                Identifier = Str(p, "identifier"),
                Name = Str(p, "name"),
                Trackers = Array(p, "trackers").Select(MapRef).ToList(),
                CustomFields = Array(p, "issue_custom_fields").Select(MapRef).ToList(),
            };
            options.Statuses = await RefsAsync(conn, "/issue_statuses.json", "issue_statuses", ct);
            options.Priorities = await RefsAsync(conn, "/enumerations/issue_priorities.json", "issue_priorities", ct);
            options.Members = await MembersAsync(conn, options.Id, ct);
            options.MyPermissions = (await PermissionsAsync(conn, options.Id, ct)).OrderBy(x => x, StringComparer.Ordinal).ToList();
            return options;
        }

        // ── Scrittura ───────────────────────────────────────────────

        public async Task<RedmineIssueDetail> CreateIssueAsync(RedmineConnection conn, RedmineIssueChange change, CancellationToken ct = default)
        {
            if (change == null || string.IsNullOrWhiteSpace(change.Subject))
                throw new RedmineApiException(Say("Per creare una issue serve l'oggetto (subject).", "Creating an issue needs a subject."));
            var options = await ProjectOptionsAsync(conn, change.Project, ct);
            RequirePermission(options, "add_issues", Say("creare issue", "create issues"));

            RedmineRef tracker;
            if (!string.IsNullOrWhiteSpace(change.Tracker)) tracker = Match(options.Trackers, change.Tracker, "tracker");
            else if (options.Trackers.Count == 1) tracker = options.Trackers[0];
            else throw new RedmineApiException(Say(
                $"Il progetto «{options.Name}» ha più tracker: scegli quale ({Names(options.Trackers)}).",
                $"The project «{options.Name}» has several trackers: choose one ({Names(options.Trackers)})."));

            var issue = new JsonObject
            {
                ["project_id"] = options.Id,
                ["tracker_id"] = tracker.Id,
                ["subject"] = change.Subject.Trim(),
            };
            if (change.Description != null) issue["description"] = change.Description;
            await FillFieldsAsync(conn, options, change, issue, statuses: null, ct);

            using var doc = await SendJsonAsync(conn, HttpMethod.Post, "/issues.json", new JsonObject { ["issue"] = issue }, ct);
            var id = Int(Prop(doc.RootElement, "issue"), "id")
                     ?? throw new RedmineApiException(Say("Redmine ha risposto senza l'id della issue creata.", "Redmine answered without the id of the created issue."));
            var created = await GetIssueAsync(conn, id, ct);
            var missing = Unapplied(change, created, options, includeNotes: false);
            if (missing.Count > 0)
                throw new RedmineApiException(Say(
                    $"La issue #{id} è stata creata, ma Redmine non ha applicato: {string.Join("; ", missing)}.",
                    $"Issue #{id} was created, but Redmine did not apply: {string.Join("; ", missing)}."));
            return created;
        }

        public async Task<RedmineIssueDetail> UpdateIssueAsync(RedmineConnection conn, int id, RedmineIssueChange change, CancellationToken ct = default)
        {
            if (change == null) throw new RedmineApiException(Say("Niente da aggiornare.", "Nothing to update."));
            var before = await GetIssueAsync(conn, id, ct);
            var options = await ProjectOptionsAsync(conn, before.Project.Id.ToString(CultureInfo.InvariantCulture), ct);
            var me = await VerifyAsync(conn, ct);

            var changesFields = change.Subject != null || change.Description != null || change.Status != null || change.Priority != null
                                || change.AssignedTo != null || (change.CustomFields?.Count ?? 0) > 0;
            var canEdit = Has(options, "edit_issues") || (Has(options, "edit_own_issues") && before.Author?.Id == me.Id);
            if (changesFields && !canEdit)
                RequirePermission(options, "edit_issues", Say("modificare le issue", "edit issues"));
            if (!string.IsNullOrWhiteSpace(change.Notes) && !canEdit && !Has(options, "add_issue_notes"))
                RequirePermission(options, "add_issue_notes", Say("aggiungere note", "add notes"));
            if (!changesFields && string.IsNullOrWhiteSpace(change.Notes))
                throw new RedmineApiException(Say("Niente da aggiornare: nessun campo e nessuna nota.", "Nothing to update: no field and no note."));

            var issue = new JsonObject();
            if (change.Subject != null) issue["subject"] = change.Subject.Trim();
            if (change.Description != null) issue["description"] = change.Description;
            if (!string.IsNullOrWhiteSpace(change.Notes)) issue["notes"] = change.Notes;
            // Lo stato si sceglie fra quelli che QUESTO utente può dare a QUESTA issue: un altro Redmine lo ignora in silenzio.
            await FillFieldsAsync(conn, options, change, issue, statuses: before.AllowedStatuses.Count > 0 ? before.AllowedStatuses : options.Statuses, ct);

            using (await SendJsonAsync(conn, HttpMethod.Put, $"/issues/{id}.json", new JsonObject { ["issue"] = issue }, ct)) { }

            var after = await GetIssueAsync(conn, id, ct);
            var missing = Unapplied(change, after, options, includeNotes: true, journalsBefore: before.Journals.Count);
            if (missing.Count > 0)
                throw new RedmineApiException(Say(
                    $"Redmine ha accettato l'aggiornamento della issue #{id} ma non ha applicato: {string.Join("; ", missing)}.",
                    $"Redmine accepted the update of issue #{id} but did not apply: {string.Join("; ", missing)}."));
            return after;
        }

        // ── Campi, permessi e verifica ──────────────────────────────

        private async Task FillFieldsAsync(RedmineConnection conn, RedmineProjectOptions options, RedmineIssueChange change, JsonObject issue,
            List<RedmineRef> statuses, CancellationToken ct)
        {
            if (change.Status != null)
            {
                if (statuses == null)
                    throw new RedmineApiException(Say("Lo stato si cambia aggiornando la issue, non creandola.", "The status is changed by updating the issue, not by creating it."));
                issue["status_id"] = Match(statuses, change.Status, Say("stato consentito", "allowed status")).Id;
            }
            if (change.Priority != null) issue["priority_id"] = Match(options.Priorities, change.Priority, Say("priorità", "priority")).Id;
            if (change.AssignedTo != null)
            {
                var who = change.AssignedTo.Trim();
                if (string.Equals(who, "none", StringComparison.OrdinalIgnoreCase)) issue["assigned_to_id"] = "";
                else if (string.Equals(who, "me", StringComparison.OrdinalIgnoreCase)) issue["assigned_to_id"] = (await VerifyAsync(conn, ct)).Id;
                else issue["assigned_to_id"] = Match(options.Members, who, Say("membro del progetto", "project member")).Id;
            }
            if (change.CustomFields?.Count > 0)
            {
                var array = new JsonArray();
                foreach (var (field, value) in change.CustomFields)
                    array.Add(new JsonObject { ["id"] = Match(options.CustomFields, field, Say("campo personalizzato", "custom field")).Id, ["value"] = value ?? "" });
                issue["custom_fields"] = array;
            }
        }

        /// <summary>Che cosa è stato chiesto e non c'è nella issue riletta (la trappola del 204 che non cambia niente).</summary>
        private static List<string> Unapplied(RedmineIssueChange change, RedmineIssueDetail issue, RedmineProjectOptions options, bool includeNotes, int journalsBefore = 0)
        {
            var missing = new List<string>();
            if (change.Subject != null && !SameText(issue.Subject, change.Subject.Trim())) missing.Add("subject");
            if (change.Description != null && !SameText(issue.Description, change.Description)) missing.Add("description");
            if (change.Status != null && !Matches(issue.Status, change.Status)) missing.Add(Say($"stato «{change.Status}» (è «{issue.Status?.Name}»)", $"status «{change.Status}» (it is «{issue.Status?.Name}»)"));
            if (change.Priority != null && !Matches(issue.Priority, change.Priority)) missing.Add(Say($"priorità «{change.Priority}»", $"priority «{change.Priority}»"));
            if (change.AssignedTo != null)
            {
                var who = change.AssignedTo.Trim();
                var cleared = string.Equals(who, "none", StringComparison.OrdinalIgnoreCase);
                var ok = cleared ? issue.AssignedTo == null
                       : string.Equals(who, "me", StringComparison.OrdinalIgnoreCase) ? issue.AssignedTo != null
                       : Matches(issue.AssignedTo, who);
                if (!ok) missing.Add(Say($"assegnatario «{who}»", $"assignee «{who}»"));
            }
            foreach (var (field, value) in change.CustomFields ?? new Dictionary<string, string>())
            {
                var cf = issue.CustomFields.FirstOrDefault(c => c.Id.ToString(CultureInfo.InvariantCulture) == field.Trim()
                                                                || string.Equals(c.Name, field.Trim(), StringComparison.OrdinalIgnoreCase));
                if (cf == null || !SameText(cf.Value, value ?? "")) missing.Add(Say($"campo «{field}»", $"field «{field}»"));
            }
            if (includeNotes && !string.IsNullOrWhiteSpace(change.Notes)
                && !issue.Journals.Skip(journalsBefore).Any(j => SameText(j.Notes, change.Notes)))
                missing.Add(Say("la nota", "the note"));
            return missing;
        }

        private async Task<HashSet<string>> PermissionsAsync(RedmineConnection conn, int projectId, CancellationToken ct)
        {
            using var doc = await GetJsonAsync(conn, "/users/current.json?include=memberships", ct);
            var user = Prop(doc.RootElement, "user");
            var permissions = new HashSet<string>(StringComparer.Ordinal);
            if (Bool(user, "admin")) permissions.Add("admin");
            var roles = Array(user, "memberships")
                .Where(m => Int(Prop(m, "project"), "id") == projectId)
                .SelectMany(m => Array(m, "roles")).Select(r => Int(r, "id")).Where(r => r.HasValue).Select(r => r.Value).Distinct().ToList();
            foreach (var role in roles)
            {
                using var r = await GetJsonAsync(conn, $"/roles/{role}.json", ct);
                foreach (var p in Array(Prop(r.RootElement, "role"), "permissions"))
                    if (p.ValueKind == JsonValueKind.String) permissions.Add(p.GetString());
            }
            return permissions;
        }

        private static bool Has(RedmineProjectOptions options, string permission)
            => options.MyPermissions.Contains("admin") || options.MyPermissions.Contains(permission);

        private static void RequirePermission(RedmineProjectOptions options, string permission, string what)
        {
            if (Has(options, permission)) return;
            var mine = options.MyPermissions.Count == 0 ? Say("nessuno", "none") : string.Join(", ", options.MyPermissions);
            throw new RedmineApiException(Say(
                $"Non hai il permesso di {what} nel progetto «{options.Name}» (serve «{permission}»; i tuoi: {mine}). Non ho mandato niente a Redmine.",
                $"You are not allowed to {what} in the project «{options.Name}» (it needs «{permission}»; yours: {mine}). Nothing was sent to Redmine."),
                HttpStatusCode.Forbidden);
        }

        private async Task<List<RedmineRef>> MembersAsync(RedmineConnection conn, int projectId, CancellationToken ct)
        {
            var members = new List<RedmineRef>();
            for (var offset = 0; ; offset += 100)
            {
                using var doc = await GetJsonAsync(conn, $"/projects/{projectId}/memberships.json?limit=100&offset={offset}", ct);
                var page = Array(doc.RootElement, "memberships").ToList();
                foreach (var m in page)
                {
                    var principal = Prop(m, "user");
                    if (principal.ValueKind != JsonValueKind.Object) principal = Prop(m, "group");
                    if (principal.ValueKind == JsonValueKind.Object) members.Add(MapRef(principal));
                }
                var total = Int(doc.RootElement, "total_count") ?? 0;
                if (page.Count == 0 || offset + page.Count >= total) break;
            }
            return members.GroupBy(m => m.Id).Select(g => g.First()).ToList();
        }

        private async Task<string> StatusFilterAsync(RedmineConnection conn, string status, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(status)) return "open";
            var s = status.Trim();
            if (s == "*" || s.Equals("open", StringComparison.OrdinalIgnoreCase) || s.Equals("closed", StringComparison.OrdinalIgnoreCase))
                return s.ToLowerInvariant();
            if (s.Equals("all", StringComparison.OrdinalIgnoreCase)) return "*";
            var statuses = await RefsAsync(conn, "/issue_statuses.json", "issue_statuses", ct);
            return Match(statuses, s, Say("stato", "status")).Id.ToString(CultureInfo.InvariantCulture);
        }

        private async Task<List<RedmineRef>> RefsAsync(RedmineConnection conn, string path, string array, CancellationToken ct)
        {
            using var doc = await GetJsonAsync(conn, path, ct);
            return Array(doc.RootElement, array).Select(MapRef).ToList();
        }

        /// <summary>Un valore dato per id o per nome: mai indovinato. Niente corrispondenza → errore con le scelte possibili.</summary>
        private static RedmineRef Match(IReadOnlyList<RedmineRef> options, string value, string what)
        {
            var v = (value ?? "").Trim();
            var found = int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                ? options.FirstOrDefault(o => o.Id == id)
                : options.FirstOrDefault(o => string.Equals(o.Name, v, StringComparison.OrdinalIgnoreCase));
            return found ?? throw new RedmineApiException(Say(
                $"«{v}» non è un {what} valido. Possibili: {Names(options)}.",
                $"«{v}» is not a valid {what}. Possible: {Names(options)}."));
        }

        private static bool Matches(RedmineRef actual, string wanted)
        {
            if (actual == null) return false;
            var v = wanted.Trim();
            return int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id)
                ? actual.Id == id
                : string.Equals(actual.Name, v, StringComparison.OrdinalIgnoreCase);
        }

        private static bool SameText(string a, string b)
            => string.Equals(Normalize(a), Normalize(b), StringComparison.Ordinal);

        private static string Normalize(string s) => (s ?? "").Replace("\r\n", "\n").Trim();

        private static string Names(IEnumerable<RedmineRef> refs)
        {
            var list = refs.Select(r => $"{r.Name} ({r.Id})").ToList();
            return list.Count == 0 ? "—" : string.Join(", ", list);
        }

        // ── HTTP ────────────────────────────────────────────────────

        private Task<JsonDocument> GetJsonAsync(RedmineConnection conn, string pathAndQuery, CancellationToken ct)
            => SendJsonAsync(conn, HttpMethod.Get, pathAndQuery, null, ct);

        private async Task<JsonDocument> SendJsonAsync(RedmineConnection conn, HttpMethod method, string pathAndQuery, JsonNode body, CancellationToken ct)
        {
            Validate(conn);
            var url = BaseUrl(conn) + pathAndQuery;
            using var req = new HttpRequestMessage(method, url);
            req.Headers.Add(ApiKeyHeader, conn.ApiKey);
            req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (body != null) req.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

            HttpResponseMessage resp;
            try
            {
                resp = await _httpClientFactory.CreateClient().SendAsync(req, ct);
            }
            catch (HttpRequestException ex)
            {
                throw new RedmineApiException(AtlassianNetworkDiagnostics.DescribeUnreachable("Redmine", BaseUrl(conn), ex), null, ex);
            }

            using (resp)
            {
                var text = await resp.Content.ReadAsStringAsync(ct);
                var isHtml = text.TrimStart().StartsWith("<", StringComparison.Ordinal)
                             || resp.Content.Headers.ContentType?.MediaType?.Contains("html", StringComparison.OrdinalIgnoreCase) == true;
                if (isHtml) throw HtmlAnswer(conn, resp.StatusCode, text);
                if (!resp.IsSuccessStatusCode) throw Failure(conn, method, pathAndQuery, resp.StatusCode, text);
                // 204 (aggiornamento) e risposte vuote: un documento vuoto, chi chiama non lo legge.
                if (string.IsNullOrWhiteSpace(text)) return JsonDocument.Parse("{}");
                try { return JsonDocument.Parse(text); }
                catch (JsonException ex)
                {
                    throw new RedmineApiException(Say(
                        $"Redmine ha risposto a {pathAndQuery} con un testo che non è JSON (HTTP {(int)resp.StatusCode}).",
                        $"Redmine answered {pathAndQuery} with text that is not JSON (HTTP {(int)resp.StatusCode})."), resp.StatusCode, ex);
                }
            }
        }

        /// <summary>
        /// Una pagina HTML al posto dell'API. Visto il 27/10 su un Redmine reale: un firewall davanti al server risponde ai
        /// programmi con una sfida JavaScript (HTTP 200) e poi con «The URL you requested has been blocked» (HTTP 500), e la
        /// chiave non arriva nemmeno a Redmine. Lo si dice con il titolo della pagina, perché l'utente lo giri agli
        /// amministratori; non si prova ad aggirarlo.
        /// </summary>
        private static RedmineApiException HtmlAnswer(RedmineConnection conn, HttpStatusCode status, string html)
        {
            var title = Regex.Match(html ?? "", @"<title[^>]*>\s*([^<]*?)\s*</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
            var reference = Regex.Match(html ?? "", @"(Message ID|Support ID|Incident ID|Request ID|Ray ID)\s*:?\s*([A-Za-z0-9-]+)", RegexOptions.IgnoreCase);
            var detail = string.IsNullOrWhiteSpace(title) ? "" : $" «{title}»";
            var refText = reference.Success ? $" ({reference.Groups[1].Value}: {reference.Groups[2].Value})" : "";
            return new RedmineApiException(Say(
                $"{BaseUrl(conn)} ha risposto con una pagina web{detail}{refText}, HTTP {(int)status}, invece che con l'API di Redmine. " +
                "Di solito è un firewall o un proxy davanti a Redmine che non lascia passare i programmi: chiedi agli amministratori " +
                "di consentire l'API REST con la chiave personale (header X-Redmine-API-Key), citando il riferimento. Controlla anche " +
                "l'indirizzo: deve essere la radice di Redmine.",
                $"{BaseUrl(conn)} answered with a web page{detail}{refText}, HTTP {(int)status}, instead of Redmine's API. " +
                "Usually a firewall or proxy in front of Redmine does not let programs through: ask the administrators to allow the " +
                "REST API with the personal key (X-Redmine-API-Key header), quoting the reference. Also check the address: it must be " +
                "Redmine's root."), status);
        }

        private static RedmineApiException Failure(RedmineConnection conn, HttpMethod method, string pathAndQuery, HttpStatusCode status, string body)
        {
            var path = pathAndQuery.Split('?')[0];
            switch (status)
            {
                case HttpStatusCode.Unauthorized:
                    return new RedmineApiException(Say(
                        $"Redmine ({BaseUrl(conn)}) non accetta la chiave API: rigenerala in «Il mio account» → «Chiave di accesso API» e salvala di nuovo nelle impostazioni del progetto. Se la chiave è giusta, l'API REST potrebbe essere spenta sul server.",
                        $"Redmine ({BaseUrl(conn)}) does not accept the API key: regenerate it in «My account» → «API access key» and save it again in the project settings. If the key is right, the REST API may be disabled on the server."), status);
                case HttpStatusCode.Forbidden:
                    return new RedmineApiException(Say(
                        $"Redmine nega l'accesso a {path}: il tuo utente non ha il permesso, o il progetto non è visibile per te.",
                        $"Redmine denies access to {path}: your user lacks the permission, or the project is not visible to you."), status);
                case HttpStatusCode.NotFound:
                    return new RedmineApiException(Say(
                        $"Redmine non trova {path}: l'id o l'identificativo non esiste, oppure non è visibile per te.",
                        $"Redmine does not find {path}: the id or identifier does not exist, or is not visible to you."), status);
                case (HttpStatusCode)422:
                    var errors = new List<string>();
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        errors.AddRange(Array(doc.RootElement, "errors").Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()));
                    }
                    catch (JsonException) { }
                    return new RedmineApiException(Say(
                        $"Redmine ha rifiutato i dati: {(errors.Count > 0 ? string.Join("; ", errors) : body)}.",
                        $"Redmine rejected the data: {(errors.Count > 0 ? string.Join("; ", errors) : body)}."), status);
                default:
                    var excerpt = body == null ? "" : body.Length > 300 ? body[..300] + "…" : body;
                    return new RedmineApiException(Say(
                        $"{method} {path} su Redmine è fallito con HTTP {(int)status}: {excerpt}",
                        $"{method} {path} on Redmine failed with HTTP {(int)status}: {excerpt}"), status);
            }
        }

        private static void Validate(RedmineConnection conn)
        {
            if (conn == null || string.IsNullOrWhiteSpace(conn.BaseUrl))
                throw new RedmineApiException(Say("Manca l'indirizzo di Redmine (Impostazioni del progetto → Redmine).", "The Redmine address is missing (Project settings → Redmine)."));
            if (!Uri.TryCreate(conn.BaseUrl.Trim(), UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                throw new RedmineApiException(Say($"«{conn.BaseUrl}» non è un indirizzo http(s) valido.", $"«{conn.BaseUrl}» is not a valid http(s) address."));
            if (string.IsNullOrWhiteSpace(conn.ApiKey))
                throw new RedmineApiException(Say("Manca la chiave API di Redmine (Impostazioni del progetto → Redmine).", "The Redmine API key is missing (Project settings → Redmine)."));
        }

        private static string BaseUrl(RedmineConnection conn) => conn.BaseUrl.Trim().TrimEnd('/');

        private static string Say(string it, string en) => UiText.Say(it, en);

        // ── Mappatura ───────────────────────────────────────────────

        private static RedmineUser MapUser(JsonElement u) => new RedmineUser
        {
            Id = Int(u, "id") ?? 0,
            Login = Str(u, "login"),
            Name = $"{Str(u, "firstname")} {Str(u, "lastname")}".Trim(),
            Admin = Bool(u, "admin"),
        };

        private static RedmineRef MapRef(JsonElement e)
            => e.ValueKind == JsonValueKind.Object ? new RedmineRef { Id = Int(e, "id") ?? 0, Name = Str(e, "name") } : null;

        private static T MapSummary<T>(RedmineConnection conn, JsonElement i, T s) where T : RedmineIssueSummary
        {
            s.Id = Int(i, "id") ?? 0;
            s.Url = $"{BaseUrl(conn)}/issues/{s.Id}";
            s.Project = MapRef(Prop(i, "project"));
            s.Tracker = MapRef(Prop(i, "tracker"));
            s.Status = MapRef(Prop(i, "status"));
            s.StatusIsClosed = Bool(Prop(i, "status"), "is_closed");
            s.Priority = MapRef(Prop(i, "priority"));
            s.Author = MapRef(Prop(i, "author"));
            s.AssignedTo = MapRef(Prop(i, "assigned_to"));
            s.Subject = Str(i, "subject");
            s.StartDate = Str(i, "start_date");
            s.DueDate = Str(i, "due_date");
            s.UpdatedOn = Date(i, "updated_on");
            return s;
        }

        private static RedmineIssueDetail MapDetail(RedmineConnection conn, JsonElement i)
        {
            var d = MapSummary(conn, i, new RedmineIssueDetail());
            d.Description = Str(i, "description");
            d.DoneRatio = Int(i, "done_ratio");
            d.CreatedOn = Date(i, "created_on");
            d.CustomFields = Array(i, "custom_fields").Select(c => new RedmineCustomFieldValue
            {
                Id = Int(c, "id") ?? 0,
                Name = Str(c, "name"),
                Value = Prop(c, "value") is var v && v.ValueKind == JsonValueKind.Array
                    ? string.Join(", ", v.EnumerateArray().Select(x => x.ToString()))
                    : Str(c, "value"),
            }).ToList();
            d.Journals = Array(i, "journals").Select(j => new RedmineJournal
            {
                Id = Int(j, "id") ?? 0,
                User = MapRef(Prop(j, "user")),
                CreatedOn = Date(j, "created_on"),
                Notes = Str(j, "notes"),
                Changes = Array(j, "details").Select(x => $"{Str(x, "name")}: {Str(x, "old_value") ?? "—"} → {Str(x, "new_value") ?? "—"}").ToList(),
            }).ToList();
            d.AllowedStatuses = Array(i, "allowed_statuses").Select(MapRef).ToList();
            d.Attachments = Array(i, "attachments").Select(a => Str(a, "filename")).Where(n => n != null).ToList();
            return d;
        }

        private static JsonElement Prop(JsonElement e, string name)
            => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) ? v : default;

        private static IEnumerable<JsonElement> Array(JsonElement e, string name)
            => Prop(e, name) is var a && a.ValueKind == JsonValueKind.Array ? a.EnumerateArray() : Enumerable.Empty<JsonElement>();

        private static string Str(JsonElement e, string name)
        {
            var v = Prop(e, name);
            return v.ValueKind switch
            {
                JsonValueKind.String => v.GetString(),
                JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => v.ToString(),
                _ => null,
            };
        }

        private static int? Int(JsonElement e, string name)
            => Prop(e, name) is var v && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n) ? n : null;

        private static bool Bool(JsonElement e, string name) => Prop(e, name).ValueKind == JsonValueKind.True;

        private static DateTime? Date(JsonElement e, string name)
            => DateTime.TryParse(Str(e, name), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var d) ? d : null;
    }
}
