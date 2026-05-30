using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

[McpServerToolType]
public class MdExplorerTools
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MdExplorerTools(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private static string ProjectParam(string project, string separator = "?")
    {
        if (string.IsNullOrEmpty(project))
            return "";
        return $"{separator}project={Uri.EscapeDataString(project)}";
    }

    private async Task LogToolCall(string toolName, string project, string request, string response)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("MdExplorer");
            var projectsJson = await client.GetStringAsync("/api/MdProjects/GetProjects");
            var projects = JsonSerializer.Deserialize<JsonElement>(projectsJson);

            string logDir = null;
            foreach (var p in projects.EnumerateArray())
            {
                if (p.TryGetProperty("name", out var name) &&
                    name.GetString()?.Equals(project, StringComparison.OrdinalIgnoreCase) == true &&
                    p.TryGetProperty("path", out var path))
                {
                    logDir = Path.Combine(path.GetString(), ".md");
                    break;
                }
            }

            if (logDir == null) return;
            Directory.CreateDirectory(logDir);

            var logFile = Path.Combine(logDir, "mcp-tools.log");
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {toolName}\nREQUEST: {request}\nRESPONSE: {response}\n{"".PadRight(80, '-')}\n";
            await File.AppendAllTextAsync(logFile, entry);
        }
        catch { /* logging should never break tool execution */ }
    }

    [McpServerTool, Description(
        "List all MdExplorer projects with their names and paths. " +
        "Use this FIRST to discover available projects before calling SearchDocuments. " +
        "The project name from the results must be passed to SearchDocuments via the 'project' parameter.")]
    public async Task<string> GetProjects()
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");

        try
        {
            var projectResponse = await client.GetAsync("/api/MdProjects/GetProjects");

            if (!projectResponse.IsSuccessStatusCode)
            {
                return $"Error: MdExplorer API returned {projectResponse.StatusCode}. Is MdExplorer running?";
            }

            return await projectResponse.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}. Ensure MdExplorer is running.";
        }
    }

    [McpServerTool, Description(
        "Semantic search across all indexed markdown documents in an MdExplorer project. " +
        "Returns the most relevant text passages. " +
        "You MUST answer ONLY based on the returned results. Do NOT use other tools to read files or search the filesystem. " +
        "If the results are insufficient, try a different query with different keywords. " +
        "Use natural language queries with domain-specific terms.")]
    public async Task<string> SearchDocuments(
        [Description("The search query in natural language. Be specific and use domain terms.")] string query,
        [Description("Project name to search in. Use GetProjects first to discover available project names.")] string project,
        [Description("Maximum number of results to return (default: 10, max: 20)")] int? topK = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var k = topK ?? 10;

        try
        {
            var url = $"/api/rag/search?q={Uri.EscapeDataString(query)}&topK={k}{ProjectParam(project, "&")}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: MdExplorer API returned {response.StatusCode}. Is MdExplorer running?";
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(json);

            if (result.TryGetProperty("enabled", out var enabled) && !enabled.GetBoolean())
            {
                return "RAG is not enabled for this project. Enable it in MdExplorer Settings > RAG.";
            }

            await LogToolCall("SearchDocuments", project, $"query={query}, topK={k}", json);
            return json;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}. Ensure MdExplorer is running.";
        }
    }

    // ============================================================
    //   Knowledge Graph (Neo4j) tools
    // ============================================================

    private async Task<string> ResolveProjectIdAsync(System.Net.Http.HttpClient client, string projectName)
    {
        if (string.IsNullOrWhiteSpace(projectName)) return null;
        try
        {
            var json = await client.GetStringAsync("/api/MdProjects/GetProjects");
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            foreach (var p in doc.EnumerateArray())
            {
                if (!p.TryGetProperty("name", out var nameEl)) continue;
                if (string.Equals(nameEl.GetString(), projectName, StringComparison.OrdinalIgnoreCase) &&
                    p.TryGetProperty("id", out var idEl))
                {
                    return idEl.GetString();
                }
            }
        }
        catch { }
        return null;
    }

    [McpServerTool, Description(
        "Lists the knowledge-graph namespaces configured for an MdExplorer project (each namespace " +
        "is a logical graph, e.g. 'cobol-domain' or 'impl-plan'). Each entry includes the concept " +
        "count currently stored in Neo4j. Use this FIRST to discover what graphs exist before " +
        "introducing concept names — a concept defined in another namespace MUST be reused verbatim " +
        "from there (cross-graph rule of the mde-doc skill).")]
    public async Task<string> GetGraphNamespaces(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        try
        {
            var resp = await client.GetAsync($"/api/kg/query/namespaces/{pid}");
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("GetGraphNamespaces", project, $"projectId={pid}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Returns the schema of a single knowledge-graph namespace: total concept count, breakdown of " +
        "relationship types, and the top 10 concepts by degree (most connected). Use this to get a " +
        "high-level sense of a graph before drilling in.")]
    public async Task<string> GetGraphSchema(
        [Description("Project name.")] string project,
        [Description("Namespace name (returned by GetGraphNamespaces).")] string graphNamespace)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        try
        {
            var resp = await client.GetAsync($"/api/kg/query/schema/{pid}?ns={Uri.EscapeDataString(graphNamespace ?? string.Empty)}");
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("GetGraphSchema", project, $"ns={graphNamespace}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Searches concept names across the whole project knowledge graph (or one namespace) via case-" +
        "insensitive substring match. " +
        "Use this BEFORE introducing a concept name in a new .kg.cypher: if a concept with that name " +
        "already exists in another namespace, reuse it verbatim so the graphs link up. " +
        "Returns each match with its graph namespace and the source documents that declared it.")]
    public async Task<string> FindConcepts(
        [Description("Substring to search for in concept names (case-insensitive).")] string query,
        [Description("Project name.")] string project,
        [Description("Optional namespace to limit search to.")] string graphNamespace = null,
        [Description("Max results (default 20, cap 200).")] int? limit = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        var lim = limit ?? 20;
        var url = $"/api/kg/query/concepts/search?projectId={pid}&q={Uri.EscapeDataString(query ?? string.Empty)}&limit={lim}";
        if (!string.IsNullOrWhiteSpace(graphNamespace))
            url += $"&ns={Uri.EscapeDataString(graphNamespace)}";
        try
        {
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("FindConcepts", project, $"q={query}, ns={graphNamespace}, limit={lim}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Returns concepts directly related to a named concept up to N hops away (default 1). Useful to " +
        "understand a concept's neighborhood before writing about it. Distance is reported per concept.")]
    public async Task<string> GetRelatedConcepts(
        [Description("Concept name (exact, case-sensitive).")] string name,
        [Description("Project name.")] string project,
        [Description("Optional namespace to scope the origin concept.")] string graphNamespace = null,
        [Description("Traversal depth (1-3, default 1).")] int? depth = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        var d = depth ?? 1;
        if (d < 1) d = 1;
        if (d > 3) d = 3;
        var url = $"/api/kg/query/concepts/{Uri.EscapeDataString(name ?? string.Empty)}/related?projectId={pid}&depth={d}";
        if (!string.IsNullOrWhiteSpace(graphNamespace))
            url += $"&ns={Uri.EscapeDataString(graphNamespace)}";
        try
        {
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("GetRelatedConcepts", project, $"name={name}, depth={d}, ns={graphNamespace}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Runs a READ-ONLY Cypher query against the project's Neo4j knowledge graph. The endpoint " +
        "automatically scopes the query to the project (the parameter $pid is injected with the " +
        "current projectId). Use this only when the other tools (FindConcepts / GetRelatedConcepts / " +
        "GetGraphSchema) are not enough. Write keywords (CREATE/MERGE/DELETE/SET/REMOVE/DETACH/DROP) " +
        "are rejected.")]
    public async Task<string> RunCypher(
        [Description("The Cypher query. Use $pid for the projectId (injected automatically).")] string query,
        [Description("Project name.")] string project)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        try
        {
            var payload = new { projectId = pid, query };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/kg/query/cypher", content);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("RunCypher", project, $"query={query}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    // ============================================================
    //   Atlassian (Jira) tools — read-only triage
    // ============================================================

    [McpServerTool, Description(
        "Lists the Jira issues assigned to the current user that are still open " +
        "(not Done), most urgent first (priority desc, due date asc). Use this to " +
        "answer 'what should I work on next' and to pick the top issue to plan. " +
        "The response also includes 'planningFolder' — the project-relative folder " +
        "where the generated plan markdown should be written. Requires the project " +
        "to have the Atlassian integration enabled and a token configured in " +
        "MdExplorer (Project Settings → Atlassian).")]
    public async Task<string> JiraFindMyIssues(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Max issues to return (default 10, cap 50).")] int? maxResults = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        var k = maxResults ?? 10;
        try
        {
            var resp = await client.GetAsync($"/api/atlassian/jira/my-issues?projectId={pid}&maxResults={k}");
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
        "Creates a Jira issue and assigns it to the current user. This is a WRITE " +
        "operation — only use it when the user explicitly asks to create/open an " +
        "issue (e.g. to seed test issues). Returns the created issue key and URL. " +
        "projectKey defaults to the project's configured key; issueType defaults to " +
        "'Task'. priority (e.g. 'High') and dueDate ('yyyy-MM-dd') are optional.")]
    public async Task<string> JiraCreateIssue(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Issue summary (title).")] string summary,
        [Description("Issue description in plain text (optional).")] string description = null,
        [Description("Issue type, default 'Task'.")] string issueType = null,
        [Description("Priority name, e.g. 'Highest'/'High'/'Medium'/'Low' (optional).")] string priority = null,
        [Description("Due date 'yyyy-MM-dd' (optional).")] string dueDate = null,
        [Description("Jira project key, e.g. 'BCO' (optional — defaults to the configured key).")] string projectKey = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(summary)) return "summary is required.";
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
                assignToSelf = true
            };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/atlassian/jira/issue", content);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("JiraCreateIssue", project, $"summary={summary}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }
}
