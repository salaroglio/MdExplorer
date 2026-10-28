using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>redmine</c> (docs-internal/Sprints/2026-10-27-Redmine.md): le issue di un Redmine in lettura e scrittura.
/// Pochi strumenti orientati al compito (RM4): Jira, con 15, pesava da solo più di metà del contesto degli strumenti.
/// I controlli stanno nel servizio: permessi di Redmine prima di scrivere, rilettura dopo, solo i progetti configurati.
/// </summary>
[McpServerToolType]
public sealed class RedmineTools : McpToolsBase
{
    private const string ProjectDescription = "MdExplorer project name. Use GetProjects first to discover available project names.";

    public RedmineTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

    [McpServerTool, Description(
        "Redmine: the open issues assigned to the current user, most recently updated first, in the Redmine projects " +
        "configured for this MdExplorer project. Use it for 'what should I work on' and to pick an issue to read.")]
    public Task<string> redmine_find_my_issues(
        [Description(ProjectDescription)] string project,
        [Description("Max issues (default 25, cap 100).")] int? limit = null)
        => GetAsync("redmine_find_my_issues", project, pid => $"/api/redmine/issues?projectId={pid}&assignedToMe=true&limit={limit ?? 25}");

    [McpServerTool, Description(
        "Redmine: searches issues. 'text' matches subject, description AND notes. 'status' is 'open' (default), 'closed', " +
        "'*' for all, or a status name. 'redmineProjects' (comma-separated identifiers) overrides the configured ones, " +
        "e.g. to read a related project.")]
    public Task<string> redmine_search_issues(
        [Description(ProjectDescription)] string project,
        [Description("Text to look for in subject, description and notes (optional).")] string text = null,
        [Description("open (default), closed, * (all), or a status name.")] string status = null,
        [Description("Tracker name or id (optional).")] string tracker = null,
        [Description("Only issues assigned to me (default false).")] bool? assignedToMe = null,
        [Description("Comma-separated Redmine project identifiers (optional; default: the configured ones).")] string redmineProjects = null,
        [Description("Max issues (default 25, cap 100).")] int? limit = null)
        => GetAsync("redmine_search_issues", project, pid =>
            $"/api/redmine/issues?projectId={pid}&limit={limit ?? 25}&assignedToMe={(assignedToMe == true ? "true" : "false")}" +
            Q("text", text) + Q("status", status) + Q("tracker", tracker) + Q("projects", redmineProjects));

    [McpServerTool, Description(
        "Redmine: one issue with description, custom fields, the notes history (journals), the statuses YOU may move it to " +
        "(allowedStatuses) and 'textFormatting' (markdown or textile) — the format to use when you write to this server.")]
    public Task<string> redmine_get_issue(
        [Description(ProjectDescription)] string project,
        [Description("The issue number, e.g. 1234 (without #).")] int issueId)
        => GetAsync("redmine_get_issue", project, pid => $"/api/redmine/issue/{issueId}?projectId={pid}");

    [McpServerTool, Description(
        "Redmine: what a Redmine project accepts and what YOU may do there — trackers, statuses, priorities, members (for " +
        "assignee), custom fields, 'myPermissions' (add_issues, edit_issues, add_issue_notes…) and 'writable' (whether " +
        "MdExplorer may write there). Call it before creating or updating.")]
    public Task<string> redmine_project_options(
        [Description(ProjectDescription)] string project,
        [Description("Redmine project identifier, e.g. 'banca-esempio'.")] string redmineProject)
        => GetAsync("redmine_project_options", project, pid => $"/api/redmine/project-options?projectId={pid}" + Q("project", redmineProject));

    [McpServerTool, Description(
        "Redmine: CREATES an issue — a WRITE: only when the user explicitly asks, and never retry after a success (it would " +
        "make a duplicate). Write description in the server's 'textFormatting' (see redmine_project_options). Tracker, " +
        "priority and assignee take a name or id; assignee 'me' for yourself. Custom fields: a JSON object {\"Name\": \"value\"}. " +
        "Errors say what is missing (a required custom field, a permission) and nothing is created.")]
    public Task<string> redmine_create_issue(
        [Description(ProjectDescription)] string project,
        [Description("Redmine project identifier, e.g. 'banca-esempio'.")] string redmineProject,
        [Description("Subject (title).")] string subject,
        [Description("Tracker name or id; required when the project has more than one.")] string tracker = null,
        [Description("Description, in the server's textFormatting (optional).")] string description = null,
        [Description("Priority name or id (optional).")] string priority = null,
        [Description("Assignee: member name or id, or 'me' (optional).")] string assignedTo = null,
        [Description("Custom fields as a JSON object, e.g. {\"Ambiente\": \"Collaudo\"} (optional).")] string customFields = null)
        => WriteAsync("redmine_create_issue", project, HttpMethod.Post, "/api/redmine/issue",
            new { project = redmineProject, subject, tracker, description, priority, assignedTo }, customFields);

    [McpServerTool, Description(
        "Redmine: UPDATES an issue and/or adds a note — a WRITE: only when the user explicitly asks. Pass only what changes. " +
        "Status must be one of the issue's allowedStatuses (redmine_get_issue). Notes and description in the server's " +
        "textFormatting. Assignee: member name or id, 'me', or 'none' to clear. After the update the issue is read back: " +
        "anything Redmine silently ignored comes back as an error.")]
    public Task<string> redmine_update_issue(
        [Description(ProjectDescription)] string project,
        [Description("The issue number, e.g. 1234.")] int issueId,
        [Description("A note to add (optional).")] string notes = null,
        [Description("New status name or id (optional).")] string status = null,
        [Description("New assignee: name, id, 'me' or 'none' (optional).")] string assignedTo = null,
        [Description("New priority name or id (optional).")] string priority = null,
        [Description("New subject (optional).")] string subject = null,
        [Description("New description, replacing the old one (optional).")] string description = null,
        [Description("Custom fields as a JSON object, e.g. {\"Ambiente\": \"Produzione\"} (optional).")] string customFields = null)
        => WriteAsync("redmine_update_issue", project, HttpMethod.Put, $"/api/redmine/issue/{issueId}",
            new { notes, status, assignedTo, priority, subject, description }, customFields);

    // ── Helper ──────────────────────────────────────────────────

    private static string Q(string name, string value)
        => string.IsNullOrWhiteSpace(value) ? "" : $"&{name}={Uri.EscapeDataString(value.Trim())}";

    private async Task<string> GetAsync(string tool, string project, Func<string, string> url)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        try
        {
            var address = url(pid);
            var body = await (await client.GetAsync(address)).Content.ReadAsStringAsync();
            await LogToolCall(tool, project, address, body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    private async Task<string> WriteAsync(string tool, string project, HttpMethod method, string path, object fields, string customFields)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";

        var change = JsonSerializer.SerializeToNode(fields)!.AsObject();
        foreach (var key in change.Where(p => p.Value == null).Select(p => p.Key).ToList()) change.Remove(key);
        if (!string.IsNullOrWhiteSpace(customFields))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(customFields);
                change["customFields"] = JsonSerializer.SerializeToNode(parsed!.ToDictionary(kv => kv.Key, kv => kv.Value.ToString()));
            }
            catch (JsonException)
            {
                return "customFields must be a JSON object, e.g. {\"Ambiente\": \"Collaudo\"}. Nothing was sent.";
            }
        }

        try
        {
            using var request = new HttpRequestMessage(method, path)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { projectId = pid, change }), System.Text.Encoding.UTF8, "application/json"),
            };
            var body = await (await client.SendAsync(request)).Content.ReadAsStringAsync();
            await LogToolCall(tool, project, $"{method} {path} {change.ToJsonString()}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }
}
