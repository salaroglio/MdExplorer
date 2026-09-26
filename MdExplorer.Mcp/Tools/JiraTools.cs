using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>jira</c>: i 15 tool Jira. Da soli sono il <b>54%</b> del peso di tutti i tool MCP
/// (~4.354 token su ~8.009, misurati il 22/09/2026): su un progetto senza Jira sono il risparmio
/// più grosso che questo sprint possa fare.
/// </summary>
[McpServerToolType]
public sealed class JiraTools : McpToolsBase
{
    public JiraTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

    [McpServerTool, Description(
        "Lists the Jira issues assigned to the current user that are still open " +
        "(not Done), most urgent first (priority desc, due date asc). Use this to " +
        "answer 'what should I work on next' and to pick the top issue to plan. " +
        "Requires the project to have the Atlassian integration enabled and a token " +
        "configured in MdExplorer (Project Settings → Atlassian).")]
    public async Task<string> JiraFindMyIssues(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Max issues to return (default 10, cap 50).")] int? maxResults = null,
        [Description("Custom fields to include per issue, comma-separated field names or customfield_ ids " +
                     "(optional), e.g. 'Story Points,Severity'. Omit to include all populated custom fields.")] string customFields = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        var k = maxResults ?? 10;
        try
        {
            var url = $"/api/atlassian/jira/my-issues?projectId={pid}&maxResults={k}";
            if (!string.IsNullOrWhiteSpace(customFields)) url += $"&customFields={Uri.EscapeDataString(customFields.Trim())}";
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraFindMyIssues", project, $"maxResults={k}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Searches Jira issues with a free-form JQL query (read-only) — use this for " +
        "ANY filter the user asks for. Translate natural language into JQL. Each " +
        "result includes a short description snippet. Useful JQL building blocks: " +
        "assignee = currentUser(); statusCategory != Done; priority >= High; " +
        "date functions startOfDay()/endOfDay()/startOfWeek() with offsets like " +
        "startOfDay(\"+1\"). Examples — due today: 'assignee = currentUser() AND " +
        "duedate >= startOfDay() AND duedate <= endOfDay() AND statusCategory != Done'; " +
        "due tomorrow: 'duedate >= startOfDay(\"+1\") AND duedate <= endOfDay(\"+1\")'; " +
        "overdue: 'duedate < startOfDay() AND statusCategory != Done'. Scope to a " +
        "project with 'project = SCRUM' (use JiraListProjects to find keys). For the " +
        "common 'my urgent issues' case prefer JiraFindMyIssues.")]
    public async Task<string> JiraSearch(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("The JQL query, e.g. 'assignee = currentUser() AND duedate <= endOfDay()'.")] string jql,
        [Description("Max results (default 20, cap 50).")] int? maxResults = null,
        [Description("Custom fields to include per issue, comma-separated field names or customfield_ ids " +
                     "(optional), e.g. 'Story Points,Severity'. Omit to include all populated custom fields.")] string customFields = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(jql)) return "jql is required.";
        var k = maxResults ?? 20;
        try
        {
            var url = $"/api/atlassian/jira/search?projectId={pid}&jql={Uri.EscapeDataString(jql)}&maxResults={k}";
            if (!string.IsNullOrWhiteSpace(customFields)) url += $"&customFields={Uri.EscapeDataString(customFields.Trim())}";
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraSearch", project, $"jql={jql}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Fetches the full details of a single Jira issue (summary, description, " +
        "acceptance criteria text, labels, recent comments, and linked issues) so " +
        "you can write an implementation plan. Call JiraFindMyIssues first to get " +
        "the issue key. Rich text (description/comments) is flattened to markdown.")]
    public async Task<string> JiraGetIssue(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("The Jira issue key, e.g. 'BCO-123'.")] string issueKey)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(issueKey)) return "issueKey is required.";
        try
        {
            var resp = await client.GetAsync($"/api/atlassian/jira/issue/{Uri.EscapeDataString(issueKey.Trim())}?projectId={pid}");
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraGetIssue", project, $"issueKey={issueKey}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Creates a Jira issue. By default it is assigned to the current user; pass " +
        "'assignee' (a name or email) to assign it to someone else in the same call — " +
        "the tool resolves the person to their Jira accountId itself. Read the JSON 'ok' " +
        "field: ok=true -> created (the issue key, URL and resolved assignee are echoed " +
        "back). notFound=true or ambiguous=true -> NOTHING WAS CREATED, because the " +
        "assignee could not be pinned down; for ambiguous, 'candidates' lists each " +
        "accountId + name + email — show them to the user, get the choice, then call this " +
        "tool again with that exact 'assigneeAccountId'. Do NOT retry blindly: a second " +
        "call after a successful create makes a duplicate issue. This is a WRITE " +
        "operation — only use it when the user explicitly asks to create/open an " +
        "issue (e.g. to seed test issues). Returns the created issue key and URL. " +
        "projectKey defaults to the project's configured key; issueType defaults to " +
        "'Task'. priority (e.g. 'High') and dueDate ('yyyy-MM-dd') are optional. " +
        "Any other field — including system fields with no argument here, such as " +
        "fixVersions, components, labels or reporter — goes in 'customFields'. " +
        "priority takes the name Jira stores (usually English: 'Low', not 'Bassa'). " +
        "Setting anything beyond summary/description? Call JiraGetCreateFields first: " +
        "it lists what this project + issue type actually accepts on creation.")]
    public async Task<string> JiraCreateIssue(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Issue summary (title).")] string summary,
        [Description("Issue description in plain text (optional).")] string description = null,
        [Description("Issue type, default 'Task'.")] string issueType = null,
        [Description("Priority name, e.g. 'Highest'/'High'/'Medium'/'Low' (optional). Must be the name " +
                     "Jira stores, which stays English on a localised site; a wrong one is rejected with " +
                     "the list of valid names, and nothing is created.")] string priority = null,
        [Description("Due date 'yyyy-MM-dd' (optional).")] string dueDate = null,
        [Description("Jira project key, e.g. 'BCO' (optional — defaults to the configured key).")] string projectKey = null,
        [Description("Who to assign the new issue to: a person's name or email, resolved to an accountId " +
                     "by the tool (optional — omitted means assign to the current user).")] string assignee = null,
        [Description("The exact Jira accountId of the assignee, when already known (e.g. after " +
                     "disambiguating an 'ambiguous' result). Skips the name lookup.")] string assigneeAccountId = null,
        [Description("Parent issue key to link this issue to — typically the EPIC a story belongs to " +
                     "(the 'Parent'/'Principale' field), e.g. 'BCE-1694' (optional).")] string parentKey = null,
        [Description("Any other field to set, as a JSON object keyed by field id or exact field name " +
                     "(optional). Takes custom fields AND system fields with no argument of their own, e.g. " +
                     "{\"Story Points\": 5, \"fixVersions\": \"REL. Q4 2026\", \"reporter\": \"Enrico Torrelli\"}. " +
                     "User fields (reporter, and any custom people field) take a name or an email and are " +
                     "resolved to the accountId for you; if the name matches nobody or several people, " +
                     "NOTHING is written and the error lists the candidates. " +
                     "Ids are language-independent, names are localised per site — prefer the id, and call " +
                     "JiraListFields with customOnly=false to discover both. Scalars are shaped from the field's " +
                     "schema; pass a structured JSON value for types that need one. If Jira answers that a field " +
                     "'cannot be set / is not on the appropriate screen', it is missing from the CREATE screen: " +
                     "create the issue without it, then set it with JiraUpdateIssue.")] string customFields = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(summary)) return "summary is required.";
        if (!TryParseCustomFields(customFields, out var customFieldsNode, out var cfError)) return cfError;
        try
        {
            var payload = new
            {
                projectId = pid,
                summary,
                description,
                issueType,
                priority,
                dueDate,
                projectKey,
                // Assign-to-self only when no one else was named: an explicit assignee
                // takes over, and saying both would be contradictory.
                assignToSelf = string.IsNullOrWhiteSpace(assignee) && string.IsNullOrWhiteSpace(assigneeAccountId),
                assignee,
                assigneeAccountId,
                parentKey,
                customFields = customFieldsNode
            };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/atlassian/jira/issue", content);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraCreateIssue", project, $"summary={summary}, assignee={assignee}, assigneeAccountId={assigneeAccountId}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Lists the Jira projects the user can access (key + name). Use this to find " +
        "the right project key before creating an issue, or when the user is unsure " +
        "which project key to use.")]
    public async Task<string> JiraListProjects(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        try
        {
            var resp = await client.GetAsync($"/api/atlassian/jira/projects?projectId={pid}");
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraListProjects", project, "", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Attaches a file to a Jira issue. This is a WRITE operation that uploads the file's " +
        "contents to Jira, where everyone with access to the issue can read them — only use " +
        "it on a file the user asked you to attach. The path may be absolute (e.g. " +
        "'/var/log/app.log') or relative to the MdExplorer project root (e.g. " +
        "'docs/report.pdf'). Use it for a document, a log or a rendered image (a chart cannot " +
        "be interactive on an issue: render it to an image first, then attach it).")]
    public async Task<string> JiraAttachFile(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("The Jira issue key, e.g. 'BCO-123'.")] string issueKey,
        [Description("Path of the file to attach: absolute, or relative to the project root.")] string filePath,
        [Description("Name to give the attachment in Jira (optional — defaults to the file's own name).")] string fileName = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(issueKey)) return "issueKey is required.";
        if (string.IsNullOrWhiteSpace(filePath)) return "filePath is required.";
        try
        {
            var payload = new { projectId = pid, filePath, fileName };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(
                $"/api/atlassian/jira/issue/{Uri.EscapeDataString(issueKey.Trim())}/attachment", content);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraAttachFile", project, $"issueKey={issueKey},filePath={filePath}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Lists the Jira fields available on the site, with the exact field name, its id " +
        "(a system id such as 'fixVersions', or 'customfield_XXXXX'), the schema type and " +
        "— in valueHint — the value shape to pass. Call this BEFORE setting fields on " +
        "JiraCreateIssue/JiraUpdateIssue: names must match Jira exactly and are localised " +
        "per site, so a wrong or ambiguous name is rejected rather than guessed. Pass " +
        "customOnly=false whenever the field might be a built-in one (fix version, " +
        "component, reporter, labels…) — with the default true you only see custom fields " +
        "and risk picking a same-named custom field instead of the system one. Use " +
        "nameFilter to look up a single field (e.g. 'points', 'version').")]
    public async Task<string> JiraListFields(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Return only custom fields (default true). Pass false to include system fields too.")] bool customOnly = true,
        [Description("Case-insensitive substring to filter by field name or id (optional).")] string nameFilter = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        try
        {
            var url = $"/api/atlassian/jira/fields?projectId={pid}&customOnly={(customOnly ? "true" : "false")}";
            if (!string.IsNullOrWhiteSpace(nameFilter))
                url += $"&nameFilter={Uri.EscapeDataString(nameFilter.Trim())}";
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraListFields", project, $"customOnly={customOnly},nameFilter={nameFilter}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Lists the fields you can actually set while CREATING a given issue type in a " +
        "given Jira project — the create screen — with each field's id, name, whether it " +
        "is required, its schema and, in valueHint, the value shape to pass; allowedValues " +
        "lists the accepted labels when the field has a closed set. This is NOT the same " +
        "as JiraListFields: that one lists every field defined on the site, and a field can " +
        "exist there and still be absent from this screen, in which case Jira rejects the " +
        "create with 'cannot be set. It is not on the appropriate screen, or unknown'. Call " +
        "this BEFORE JiraCreateIssue whenever you intend to set anything beyond summary/" +
        "description, and pick field ids from what it returns. A field you need that is " +
        "missing here may still be editable after creation — create the issue, then set it " +
        "with JiraUpdateIssue.")]
    public async Task<string> JiraGetCreateFields(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Issue type to create, e.g. 'Task', 'Bug', 'Epic'. The localised name works too.")] string issueType,
        [Description("Jira project key, e.g. 'BCE' (optional — defaults to the configured key).")] string projectKey = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(issueType)) return "issueType is required (e.g. 'Task', 'Epic').";
        try
        {
            var url = $"/api/atlassian/jira/createfields?projectId={pid}&issueType={Uri.EscapeDataString(issueType.Trim())}";
            if (!string.IsNullOrWhiteSpace(projectKey))
                url += $"&projectKey={Uri.EscapeDataString(projectKey.Trim())}";
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraGetCreateFields", project, $"issueType={issueType},projectKey={projectKey}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Lists a Jira project's versions (releases) — name, id, released/archived and " +
        "release date. These are the only values a fix-version field accepts: check the " +
        "release name here before passing it to JiraCreateIssue/JiraUpdateIssue, e.g. " +
        "{\"fixVersions\": \"REL. Q4 2026\"}, rather than discovering from a 400 that it " +
        "is spelled differently or does not exist in this project.")]
    public async Task<string> JiraGetProjectVersions(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Jira project key, e.g. 'BCE' (optional — defaults to the configured key).")] string projectKey = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        try
        {
            var url = $"/api/atlassian/jira/versions?projectId={pid}";
            if (!string.IsNullOrWhiteSpace(projectKey))
                url += $"&projectKey={Uri.EscapeDataString(projectKey.Trim())}";
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraGetProjectVersions", project, $"projectKey={projectKey}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Discovers a Jira project's workflow: the statuses (stages) per issue type, " +
        "each tagged with its category (To Do / In Progress / Done). Use this to " +
        "understand the process so you can suggest the next step. To know exactly " +
        "which moves are valid from an issue's CURRENT status, also call " +
        "JiraListTransitions for that issue; combine the two to recommend what to do " +
        "next and (with JiraTransitionIssue) to do it.")]
    public async Task<string> JiraGetWorkflow(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Jira project key, e.g. 'SCRUM' (optional — defaults to the configured key).")] string projectKey = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        try
        {
            var url = $"/api/atlassian/jira/statuses?projectId={pid}";
            if (!string.IsNullOrWhiteSpace(projectKey)) url += $"&projectKey={Uri.EscapeDataString(projectKey.Trim())}";
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraGetWorkflow", project, $"projectKey={projectKey}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Adds a comment to a Jira issue (WRITE). Use when the user asks to comment on " +
        "an issue, e.g. to note that a plan was produced. Body is plain text.")]
    public async Task<string> JiraAddComment(
        [Description("Project name.")] string project,
        [Description("Issue key, e.g. 'SCRUM-5'.")] string issueKey,
        [Description("Comment text (plain text).")] string body)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(issueKey)) return "issueKey is required.";
        try
        {
            var payload = new { projectId = pid, body };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync($"/api/atlassian/jira/issue/{Uri.EscapeDataString(issueKey.Trim())}/comment", content);
            var respBody = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraAddComment", project, $"issueKey={issueKey}", respBody);
            return respBody;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Edits fields of an existing Jira issue (WRITE): summary, description, " +
        "priority and/or due date — and, via 'customFields', any other field, custom or " +
        "system (fixVersions, components, labels, reporter…). Only the arguments you " +
        "pass are changed. Use only when the user explicitly asks to modify an issue. " +
        "Also the way to set a field that JiraCreateIssue could not, because it is on " +
        "the edit screen but not the create screen.")]
    public async Task<string> JiraUpdateIssue(
        [Description("Project name.")] string project,
        [Description("Issue key, e.g. 'SCRUM-5'.")] string issueKey,
        [Description("New summary (optional).")] string summary = null,
        [Description("New description, plain text (optional).")] string description = null,
        [Description("New priority, e.g. 'High' (optional).")] string priority = null,
        [Description("New due date 'yyyy-MM-dd' (optional).")] string dueDate = null,
        [Description("Parent issue key — link this issue to an EPIC or parent (the 'Parent'/'Principale' " +
                     "field), e.g. 'BCE-1694' (optional).")] string parentKey = null,
        [Description("Any other field to change, as a JSON object keyed by field id or exact field name " +
                     "(optional). Takes custom fields AND system fields with no argument of their own, e.g. " +
                     "{\"Story Points\": 8, \"fixVersions\": \"REL. Q4 2026\", \"reporter\": \"Enrico Torrelli\"}. " +
                     "User fields take a name or email and are resolved to the accountId for you. " +
                     "Ids are language-independent, " +
                     "names are localised per site — prefer the id (JiraListFields with customOnly=false lists " +
                     "both). A JSON null clears a field.")] string customFields = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(issueKey)) return "issueKey is required.";
        if (!TryParseCustomFields(customFields, out var customFieldsNode, out var cfError)) return cfError;
        try
        {
            var payload = new { projectId = pid, summary, description, priority, dueDate, parentKey, customFields = customFieldsNode };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Put,
                $"/api/atlassian/jira/issue/{Uri.EscapeDataString(issueKey.Trim())}") { Content = content };
            var resp = await client.SendAsync(req);
            var respBody = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraUpdateIssue", project, $"issueKey={issueKey}", respBody);
            return respBody;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Reassigns a Jira issue to another person (WRITE). Pass the assignee's name (or " +
        "email) in 'assignee': the tool looks the person up in Jira and resolves the " +
        "internal accountId itself before reassigning. Outcomes (read the JSON 'ok' " +
        "field): ok=true -> reassigned (the resolved user is echoed back). notFound=true " +
        "-> no user matched 'assignee'; tell the user and try a different spelling/surname/" +
        "email. ambiguous=true -> SEVERAL users matched and NOTHING was changed; the JSON " +
        "'candidates' lists each accountId + name + email — show them to the user, get the " +
        "choice, then call this tool again passing that exact 'accountId'. To clear the " +
        "assignee pass unassign=true. To assign to yourself, pass your own name in 'assignee'.")]
    public async Task<string> JiraAssignIssue(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Issue key, e.g. 'SCRUM-5'.")] string issueKey,
        [Description("The assignee's name or email to look up. Omit when passing accountId, or when unassign=true.")] string assignee = null,
        [Description("The exact Jira accountId, when already known (e.g. after disambiguating an 'ambiguous' result). Skips the name lookup.")] string accountId = null,
        [Description("Set true to remove the current assignee (leave assignee/accountId empty).")] bool unassign = false)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(issueKey)) return "issueKey is required.";
        if (!unassign && string.IsNullOrWhiteSpace(assignee) && string.IsNullOrWhiteSpace(accountId))
            return "Provide 'assignee' (a name/email to look up), 'accountId', or set unassign=true.";
        try
        {
            var payload = new { projectId = pid, query = assignee, accountId, unassign };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Put,
                $"/api/atlassian/jira/issue/{Uri.EscapeDataString(issueKey.Trim())}/assignee") { Content = content };
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraAssignIssue", project, $"issueKey={issueKey}, assignee={assignee}, accountId={accountId}, unassign={unassign}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Lists the workflow transitions currently available for a Jira issue " +
        "(e.g. 'In Progress', 'Done'). Call this before JiraTransitionIssue to know " +
        "the valid target states.")]
    public async Task<string> JiraListTransitions(
        [Description("Project name.")] string project,
        [Description("Issue key, e.g. 'SCRUM-5'.")] string issueKey)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(issueKey)) return "issueKey is required.";
        try
        {
            var resp = await client.GetAsync($"/api/atlassian/jira/issue/{Uri.EscapeDataString(issueKey.Trim())}/transitions?projectId={pid}");
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraListTransitions", project, $"issueKey={issueKey}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Moves a Jira issue to a new workflow state (WRITE), e.g. 'In Progress' or " +
        "'Done'. Accepts the target status name or transition name (case-insensitive). " +
        "If unsure of valid values, call JiraListTransitions first.")]
    public async Task<string> JiraTransitionIssue(
        [Description("Project name.")] string project,
        [Description("Issue key, e.g. 'SCRUM-5'.")] string issueKey,
        [Description("Target status or transition name, e.g. 'In Progress' / 'Done'.")] string transition)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(issueKey)) return "issueKey is required.";
        if (string.IsNullOrWhiteSpace(transition)) return "transition is required.";
        try
        {
            var payload = new { projectId = pid, transition };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync($"/api/atlassian/jira/issue/{Uri.EscapeDataString(issueKey.Trim())}/transition", content);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraTransitionIssue", project, $"issueKey={issueKey}, to={transition}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }
}
