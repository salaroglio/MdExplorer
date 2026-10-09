using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>kg</c>: il knowledge graph su Neo4j — namespace, schema, concetti, vicinato e Cypher.
/// </summary>
[McpServerToolType]
public sealed class KnowledgeGraphTools : McpToolsBase
{
    public KnowledgeGraphTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

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
}
