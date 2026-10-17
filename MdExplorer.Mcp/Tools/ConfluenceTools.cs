using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>confluence</c>: spazi, ricerca, lettura e scrittura di pagine Confluence.
/// </summary>
[McpServerToolType]
public sealed class ConfluenceTools : McpToolsBase
{
    public ConfluenceTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

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
