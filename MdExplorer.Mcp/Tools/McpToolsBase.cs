using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Quello che ogni gruppo di tool ha in comune: il client HTTP verso il Service, la risoluzione
/// del progetto per nome e il log delle chiamate.
/// <para>
/// I tool stanno in una classe per <b>gruppo di funzionalità</b> — non più tutti in una classe sola —
/// perché il gruppo è l'unità che si accende e si spegne per progetto: <see cref="ToolGroups"/> registra
/// solo le classi accese, e quelle spente non entrano nemmeno in <c>tools/list</c>, quindi non costano
/// contesto. Spostare un metodo da una classe all'altra <b>non cambia il nome del tool</b> (lo fa il
/// nome del metodo), quindi la divisione è invisibile all'AI.
/// </para>
/// <para>Sprint: docs-internal/Sprints/2026-09-22-Gruppi-MCP-Per-Progetto.md.</para>
/// </summary>
public abstract class McpToolsBase
{
    protected readonly IHttpClientFactory _httpClientFactory;

    protected McpToolsBase(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected static string ProjectParam(string project, string separator = "?")
    {
        if (string.IsNullOrEmpty(project))
            return "";
        return $"{separator}project={Uri.EscapeDataString(project)}";
    }

    protected async Task LogToolCall(string toolName, string project, string request, string response)
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

    protected async Task<string> ResolveProjectIdAsync(System.Net.Http.HttpClient client, string projectName)
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
    protected static bool TryParseCustomFields(string json, out JsonElement? node, out string error)
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
}
