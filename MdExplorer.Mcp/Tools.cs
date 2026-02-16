using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

[McpServerToolType]
public class MdExplorerTools
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly McpConfig _config;

    public MdExplorerTools(IHttpClientFactory httpClientFactory, McpConfig config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    private string ProjectParam(string separator = "?")
    {
        if (string.IsNullOrEmpty(_config.ProjectName))
            return "";
        return $"{separator}project={Uri.EscapeDataString(_config.ProjectName)}";
    }

    [McpServerTool, Description(
        "IMPORTANT: Use this tool whenever the user asks about project documentation, markdown files, technical specs, " +
        "architecture, analysis, requirements, or any project-related knowledge. " +
        "This performs semantic search across all indexed markdown documents in the MdExplorer project. " +
        "Use natural language queries. Try multiple queries with different keywords if the first search returns few results. " +
        "Examples: 'GDPR data processing', 'authentication flow', 'database schema', 'API endpoints'.")]
    public async Task<string> SearchDocuments(
        [Description("The search query in natural language. Be specific and use domain terms.")] string query,
        [Description("Maximum number of results to return (default: 5, max: 20)")] int? topK = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var k = topK ?? 5;

        try
        {
            var url = $"/api/rag/search?q={Uri.EscapeDataString(query)}&topK={k}{ProjectParam("&")}";
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

            return json;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}. Ensure MdExplorer is running.";
        }
    }

    [McpServerTool, Description(
        "Retrieve the full content of a specific markdown document by its filename or path. " +
        "Use this after SearchDocuments to read the complete document when search results show relevant snippets. " +
        "Also use when the user asks to 'open', 'show', or 'read' a specific document.")]
    public async Task<string> GetDocument(
        [Description("Filename (e.g. 'README.md') or relative path of the document")] string path)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");

        try
        {
            var url = $"/api/rag/document?path={Uri.EscapeDataString(path)}{ProjectParam("&")}";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: Document not found or MdExplorer API returned {response.StatusCode}";
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(json);

            if (result.TryGetProperty("content", out var content))
            {
                var fileName = result.TryGetProperty("fileName", out var fn) ? fn.GetString() : path;
                return $"# {fileName}\n\n{content.GetString()}";
            }

            return json;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}. Ensure MdExplorer is running.";
        }
    }

    [McpServerTool, Description(
        "List all markdown files and folders in the project. " +
        "Use this to understand the project structure or when the user asks 'what documents exist', 'show me the file list', or 'what's in the project'.")]
    public async Task<string> ListDocuments()
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");

        try
        {
            var response = await client.GetAsync($"/api/mdfiles/GetShallowStructure{ProjectParam()}");

            if (!response.IsSuccessStatusCode)
            {
                return $"Error: MdExplorer API returned {response.StatusCode}. Is MdExplorer running?";
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}. Ensure MdExplorer is running.";
        }
    }

    [McpServerTool, Description(
        "Get project metadata: name, path, RAG indexing status, number of indexed chunks, and model status. " +
        "Use this to check if the project is properly configured for search.")]
    public async Task<string> GetProjectInfo()
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");

        try
        {
            var projectResponse = await client.GetAsync($"/api/MdProjects/GetProjects");
            var ragResponse = await client.GetAsync($"/api/rag/status{ProjectParam()}");

            var result = new Dictionary<string, object>();

            if (projectResponse.IsSuccessStatusCode)
            {
                var projectJson = await projectResponse.Content.ReadAsStringAsync();
                result["projects"] = JsonSerializer.Deserialize<JsonElement>(projectJson);
            }

            if (ragResponse.IsSuccessStatusCode)
            {
                var ragJson = await ragResponse.Content.ReadAsStringAsync();
                result["ragStatus"] = JsonSerializer.Deserialize<JsonElement>(ragJson);
            }

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}. Ensure MdExplorer is running.";
        }
    }
}
