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
    //   Agent-to-agent messaging (città degli agenti, §7)
    // ============================================================

    // Il RunToken viaggia NELL'AMBIENTE del processo (mai nel prompt): il Service lo
    // conia al risveglio, lo mette nell'env del processo Copilot, che lo eredita a questo
    // MCP. Presentandolo, il Service certifica l'identità del mittente (anti-spoofing R2).
    private const string RunTokenEnvVar = "MDE_RUN_TOKEN";
    private const string RunTokenHeader = "X-MDE-Run-Token";

    private static string? RunToken()
    {
        var t = Environment.GetEnvironmentVariable(RunTokenEnvVar);
        return string.IsNullOrWhiteSpace(t) ? null : t;
    }

    [McpServerTool, Description(
        "Request the intervention of an agent that belongs to ANOTHER member's city (federation). " +
        "You give a SCOPE (an area from the project's ownership document) and a message; the harness " +
        "deterministically resolves who owns that scope and which agent should act, then routes the " +
        "request to that member's machine — where THEIR human must explicitly authorize it before any " +
        "agent runs. Only available to an agent woken by a message (identity from the run token). This " +
        "does NOT return an answer: it returns a routing receipt. Use only when the work belongs to a " +
        "different owner (see the '# Ownership del progetto' section of your prompt).")]
    public async Task<string> RequestIntervention(
        [Description("The ownership scope the work belongs to (exact name from the ownership table).")] string scope,
        [Description("The request body for the remote agent. Plain text; state clearly what you need.")] string message,
        [Description("Optional preferred agent name (must be one listed for that scope).")] string preferredAgent = null,
        [Description("Optional topics/tags, comma-separated (context only).")] string topics = null)
    {
        var token = RunToken();
        if (token == null)
            return "Error: RequestIntervention is only available to an agent woken by a message (no run token in the environment).";
        if (string.IsNullOrWhiteSpace(scope)) return "Error: scope is required.";
        if (string.IsNullOrWhiteSpace(message)) return "Error: message is required.";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var payload = new
            {
                scope = scope.Trim(),
                message,
                preferredAgent = string.IsNullOrWhiteSpace(preferredAgent) ? null : preferredAgent.Trim(),
                topics = string.IsNullOrWhiteSpace(topics)
                    ? new List<string>()
                    : topics.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
            };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/A2A/messages/request-intervention") { Content = content };
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Request refused ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Send a message to ANOTHER agent that lives in the same MdExplorer project (the " +
        "'city of agents'). Only available to an agent that was itself woken by a message: " +
        "your identity as the sender is taken from the run environment and cannot be forged. " +
        "Use ListAgents first to see who you may contact. The recipient must trust you " +
        "(its 'accepts_messages_from' must include your name or '*'). Delivery is asynchronous: " +
        "the message is queued and the recipient is woken by the harness. This does NOT return " +
        "the recipient's answer — it returns a queue receipt (taskId).")]
    public async Task<string> SendAgentMessage(
        [Description("The recipient agent's name (kebab-case), as shown by ListAgents.")] string toAgent,
        [Description("The message body. Plain text; state your intent clearly.")] string message,
        [Description("Optional topics/tags describing the message, comma-separated (context only).")] string topics = null)
    {
        var token = RunToken();
        if (token == null)
            return "Error: SendAgentMessage is only available to an agent woken by a message (no run token in the environment).";
        if (string.IsNullOrWhiteSpace(toAgent)) return "Error: toAgent is required.";
        if (string.IsNullOrWhiteSpace(message)) return "Error: message is required.";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var payload = new
            {
                toAgent = toAgent.Trim(),
                message,
                topics = string.IsNullOrWhiteSpace(topics)
                    ? new List<string>()
                    : topics.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
            };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/A2A/messages/send") { Content = content };
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Send refused ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Remember a fact you learned, so your FUTURE selves recall it across wake-ups (semantic " +
        "memory). Only available to an agent woken by a message: the fact is stored in YOUR private " +
        "memory graph, keyed to your stable identity from the run environment — you cannot write to " +
        "another agent's memory. Admit ONLY facts that are specific, verifiable and operational " +
        "(e.g. 'the payments batch runs at 02:00 UTC'), never chit-chat or restatements of the " +
        "prompt. Give 'about' tags that match the topics of the conversation so retrieval finds it " +
        "later. Provenance (which run, which conversation) is recorded automatically.")]
    public async Task<string> AssertLearnedFact(
        [Description("The fact, as a short declarative statement. Specific, verifiable, operational.")] string statement,
        [Description("Topic tags this fact is about, comma-separated (align them with the message topics).")] string about = null,
        [Description("Confidence 0..1 (default 0.7). Use ~1.0 only for facts a human confirmed.")] double confidence = 0.7)
    {
        var token = RunToken();
        if (token == null)
            return "Error: AssertLearnedFact is only available to an agent woken by a message (no run token in the environment).";
        if (string.IsNullOrWhiteSpace(statement)) return "Error: statement is required.";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var payload = new { statement = statement.Trim(), about, confidence };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/A2A/memory/assert") { Content = content };
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Assert refused ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Recall what you (and the shared city memory) already know about some topics, before you " +
        "start working. Only available to an agent woken by a message: you see ONLY your own memory " +
        "graph plus the shared one — never another agent's. Pass the topics you care about; you get " +
        "back the relevant facts with their confidence. Use this to avoid re-deriving what a past " +
        "run already established.")]
    public async Task<string> QueryAgentMemory(
        [Description("Topics/tags to recall facts about, comma-separated. Empty = your most confident facts.")] string topics = null,
        [Description("Max facts to return (default 20).")] int limit = 20)
    {
        var token = RunToken();
        if (token == null)
            return "Error: QueryAgentMemory is only available to an agent woken by a message (no run token in the environment).";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var payload = new
            {
                topics = string.IsNullOrWhiteSpace(topics)
                    ? new List<string>()
                    : topics.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList(),
                limit,
            };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/A2A/memory/query") { Content = content };
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Query refused ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "List the other trusted agents you may contact in your MdExplorer project (name, role, " +
        "skills), plus whether each currently accepts messages from you. Only available to an " +
        "agent woken by a message: the project and your identity come from the run environment. " +
        "Call this before SendAgentMessage.")]
    public async Task<string> ListAgents()
    {
        var token = RunToken();
        if (token == null)
            return "Error: ListAgents is only available to an agent woken by a message (no run token in the environment).";

        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, "/api/A2A/messages/roster");
            req.Headers.Add(RunTokenHeader, token);
            var resp = await client.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return $"Error ({(int)resp.StatusCode}): {body}";
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
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

    /// <summary>
    /// Parses the optional customFields JSON-object argument. Empty → success with a null
    /// node (nothing sent). A non-object or invalid JSON → failure with an actionable message.
    /// </summary>
    private static bool TryParseCustomFields(string json, out JsonElement? node, out string error)
    {
        node = null;
        error = null;
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            var el = JsonSerializer.Deserialize<JsonElement>(json);
            if (el.ValueKind != JsonValueKind.Object)
            {
                error = "customFields must be a JSON object, e.g. {\"Story Points\": 5}.";
                return false;
            }
            node = el;
            return true;
        }
        catch (JsonException)
        {
            error = "customFields is not valid JSON. Pass a JSON object, e.g. {\"Story Points\": 5}.";
            return false;
        }
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
        [Description("Jira project key, e.g. 'BCO' (optional — defaults to the configured key).")] string projectKey = null,
        [Description("Parent issue key to link this issue to — typically the EPIC a story belongs to " +
                     "(the 'Parent'/'Principale' field), e.g. 'BCE-1694' (optional).")] string parentKey = null,
        [Description("Custom fields as a JSON object keyed by field name or customfield_ id (optional), " +
                     "e.g. {\"Story Points\": 5, \"Severity\": \"High\"}. Scalars are shaped from the field's " +
                     "schema; pass a structured JSON value for types that need one.")] string customFields = null)
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
                assignToSelf = true,
                parentKey,
                customFields = customFieldsNode
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
        "Lists the Jira fields available on the site, with the exact field name, its " +
        "'customfield_XXXXX' id, the schema type and — in valueHint — the value shape to " +
        "pass. Call this BEFORE setting customFields on JiraCreateIssue/JiraUpdateIssue: " +
        "field names must match Jira exactly, and a wrong or ambiguous name is rejected " +
        "rather than guessed. Use nameFilter to look up one field (e.g. 'points'); set " +
        "customOnly=false to also see the built-in system fields.")]
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
        "priority and/or due date. Only the arguments you pass are changed. Use only " +
        "when the user explicitly asks to modify an issue.")]
    public async Task<string> JiraUpdateIssue(
        [Description("Project name.")] string project,
        [Description("Issue key, e.g. 'SCRUM-5'.")] string issueKey,
        [Description("New summary (optional).")] string summary = null,
        [Description("New description, plain text (optional).")] string description = null,
        [Description("New priority, e.g. 'High' (optional).")] string priority = null,
        [Description("New due date 'yyyy-MM-dd' (optional).")] string dueDate = null,
        [Description("Parent issue key — link this issue to an EPIC or parent (the 'Parent'/'Principale' " +
                     "field), e.g. 'BCE-1694' (optional).")] string parentKey = null,
        [Description("Custom fields to change, as a JSON object keyed by field name or customfield_ id " +
                     "(optional), e.g. {\"Story Points\": 8}. A JSON null clears a field.")] string customFields = null)
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

    // ============================================================
    //   Confluence (read-only) tools
    // ============================================================

    [McpServerTool, Description(
        "Lists the Confluence spaces the current user can access (id + key + name). " +
        "A 'space' is a top-level container of pages, like a wiki section. Use this " +
        "FIRST to discover space keys before scoping a ConfluenceSearch with " +
        "'space = KEY'. Confluence lives on the same Atlassian site as Jira; the " +
        "integration must be enabled and a token configured in MdExplorer " +
        "(Project Settings → Atlassian).")]
    public async Task<string> ConfluenceListSpaces(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Max spaces to return (default 50, cap 250).")] int? limit = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        var k = limit ?? 50;
        try
        {
            var resp = await client.GetAsync($"/api/atlassian/confluence/spaces?projectId={pid}&limit={k}");
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("ConfluenceListSpaces", project, $"limit={k}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Searches Confluence pages with a free-form CQL query (read-only) — translate " +
        "the user's natural language into CQL. Each result returns the page id, title, " +
        "space key, a short matched excerpt, and the page URL. Use ConfluenceGetPage " +
        "with the returned id to read the full content. Useful CQL building blocks: " +
        "text ~ \"some phrase\" (full-text); type = page; space = DEV (use " +
        "ConfluenceListSpaces to find keys); title ~ \"onboarding\"; lastmodified >= " +
        "now(\"-7d\"). Combine with AND/OR, e.g. 'space = DEV AND text ~ \"deployment\" " +
        "AND type = page'. Order with 'ORDER BY lastmodified DESC'.")]
    public async Task<string> ConfluenceSearch(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("The CQL query, e.g. 'space = DEV AND text ~ \"deployment\"'.")] string cql,
        [Description("Max results (default 20, cap 100).")] int? limit = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(cql)) return "cql is required.";
        var k = limit ?? 20;
        try
        {
            var resp = await client.GetAsync($"/api/atlassian/confluence/search?projectId={pid}&cql={Uri.EscapeDataString(cql)}&limit={k}");
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("ConfluenceSearch", project, $"cql={cql}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Fetches the full content of a single Confluence page by its id (title + body " +
        "flattened to markdown, plus space id, version and URL). Call ConfluenceSearch " +
        "first to get the page id. Use this to read documentation/specs as context " +
        "before planning or writing.")]
    public async Task<string> ConfluenceGetPage(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("The Confluence page id, e.g. '123456'.")] string pageId)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(pageId)) return "pageId is required.";
        try
        {
            var resp = await client.GetAsync($"/api/atlassian/confluence/page/{Uri.EscapeDataString(pageId.Trim())}?projectId={pid}");
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("ConfluenceGetPage", project, $"pageId={pageId}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Creates a Confluence page from MARKDOWN content (WRITE). The markdown is " +
        "converted to Confluence's rich format (headings, bold/italic, lists, code " +
        "blocks, links and tables are supported). Only use when the user explicitly " +
        "asks to publish/create a page. Provide spaceKey (use ConfluenceListSpaces to " +
        "find it; defaults to the project's configured space) and a title. Optionally " +
        "parentId to nest under an existing page. Returns the new page id and URL.")]
    public async Task<string> ConfluenceCreatePage(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("Page title.")] string title,
        [Description("Page body in markdown.")] string markdownBody,
        [Description("Confluence space key, e.g. 'DEV' (optional — defaults to the configured space).")] string spaceKey = null,
        [Description("Parent page id to nest under (optional).")] string parentId = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(title)) return "title is required.";
        try
        {
            var payload = new { projectId = pid, title, markdownBody, spaceKey, parentId };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/atlassian/confluence/page", content);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("ConfluenceCreatePage", project, $"title={title}, spaceKey={spaceKey}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Updates an existing Confluence page's content from MARKDOWN (WRITE). The " +
        "markdown replaces the page body (converted to Confluence's rich format). The " +
        "page version is bumped automatically. Only use when the user explicitly asks " +
        "to edit/update a page. Get the page id from ConfluenceSearch. Title is " +
        "optional (keeps the existing one when omitted).")]
    public async Task<string> ConfluenceUpdatePage(
        [Description("Project name. Use GetProjects first to discover available project names.")] string project,
        [Description("The Confluence page id to update, e.g. '294914'.")] string pageId,
        [Description("New page body in markdown (replaces the current body).")] string markdownBody,
        [Description("New title (optional — keeps the existing title when omitted).")] string title = null,
        [Description("Short note describing the change (optional).")] string versionMessage = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var pid = await ResolveProjectIdAsync(client, project);
        if (pid == null) return $"Project '{project}' not found.";
        if (string.IsNullOrWhiteSpace(pageId)) return "pageId is required.";
        try
        {
            var payload = new { projectId = pid, title, markdownBody, versionMessage };
            var content = new System.Net.Http.StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PutAsync($"/api/atlassian/confluence/page/{Uri.EscapeDataString(pageId.Trim())}", content);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("ConfluenceUpdatePage", project, $"pageId={pageId}", body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }
}
