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
}
