using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>core</c>: come si scopre cosa c'è dentro MdExplorer. Sempre acceso — senza questi
/// due tool l'AI non sa nemmeno quali progetti esistono, e ogni altro gruppo chiede un nome di
/// progetto che nessuno le ha detto.
/// </summary>
[McpServerToolType]
public sealed class CoreTools : McpToolsBase
{
    public CoreTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

    [McpServerTool, Description(
        "Reads a saved MarkAgent chat of the project IN FULL: every event with its whole content — the user's questions, " +
        "the answers, the reasoning, each tool call with its complete arguments and its complete result. Use it when " +
        "MdExplorer asks you to learn from an activity and the compact view it gave you is not enough (e.g. to read the " +
        "exact command that failed and the one that worked). Events are numbered (seq): ask for a range with fromSeq/toSeq. " +
        "A long range comes back in pieces: when 'nextFrom' is not null, call again with fromSeq = nextFrom.")]
    public async Task<string> chat_read(
        [Description("MdExplorer project name. Use GetProjects first to discover available project names.")] string project,
        [Description("The chat id (a GUID), as given by MdExplorer.")] string chat,
        [Description("First event to read (seq, included). Optional: from the start.")] long? fromSeq = null,
        [Description("Last event to read (seq, included). Optional: to the end.")] long? toSeq = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var projectPath = await ResolveProjectPathAsync(client, project);
        if (projectPath == null) return $"Project '{project}' not found. Use GetProjects to see the available names.";
        if (!Guid.TryParse(chat, out var id)) return $"'{chat}' is not a chat id (a GUID).";
        var url = $"/api/chats/{id}/events?maxChars=60000&projectPath={Uri.EscapeDataString(projectPath)}"
            + (fromSeq.HasValue ? $"&from={fromSeq}" : "") + (toSeq.HasValue ? $"&to={toSeq}" : "");
        try
        {
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("chat_read", project, $"chat={id},from={fromSeq},to={toSeq}", $"{(int)resp.StatusCode} {body.Length} chars");
            return resp.IsSuccessStatusCode ? body : $"chat_read failed ({(int)resp.StatusCode}): {body}";
        }
        catch (Exception ex)
        {
            return $"chat_read failed: {ex.Message}";
        }
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
}
