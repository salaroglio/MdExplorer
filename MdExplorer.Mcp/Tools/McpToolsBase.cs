using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Protocol;
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

    /// <summary>Il percorso del progetto dal suo nome: le API del grafo del codice lavorano per percorso, come l'FTS.</summary>
    protected async Task<string> ResolveProjectPathAsync(System.Net.Http.HttpClient client, string projectName)
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
                    p.TryGetProperty("path", out var pathEl))
                {
                    return pathEl.GetString();
                }
            }
        }
        catch { }
        return null;
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

    // Formati che i modelli leggono come immagine; gli altri (svg, bmp, tiff…) restano
    // solo sul disco e si aprono da lì.
    private static readonly HashSet<string> InlineImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/gif", "image/webp",
    };

    // Oltre questa soglia l'immagine pesa troppo nel contesto: la si lascia sul disco.
    private const long InlineImageMaxBytes = 5 * 1024 * 1024;

    /// <summary>
    /// La risposta di un download di allegato: il JSON del Service (dove sta il file) e,
    /// se è un'immagine di formato e dimensione adatti, l'immagine stessa, così l'LLM la
    /// vede subito senza dover aprire il file.
    /// </summary>
    protected static CallToolResult AttachmentDownloadResult(System.Net.Http.HttpResponseMessage resp, string body)
    {
        var result = new CallToolResult { Content = new List<ContentBlock> { new TextContentBlock { Text = body } } };
        if (!resp.IsSuccessStatusCode)
        {
            result.IsError = true;
            return result;
        }

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var path = root.TryGetProperty("path", out var p) ? p.GetString() : null;
            var mime = root.TryGetProperty("mimeType", out var m) ? m.GetString() : null;
            if (string.IsNullOrEmpty(path) || mime == null || !InlineImageTypes.Contains(mime))
                return result;

            var info = new FileInfo(path);
            if (!info.Exists)
            {
                // Il Service gira altrove (container, macchina remota): il file c'è, ma non qui.
                ((TextContentBlock)result.Content[0]).Text = body +
                    "\n(The image was saved by MdExplorer but is not readable from this process, so it is not shown inline.)";
                return result;
            }
            if (info.Length > InlineImageMaxBytes)
            {
                ((TextContentBlock)result.Content[0]).Text = body +
                    $"\n(The image is {info.Length / (1024 * 1024)} MB, too large to show inline: open it from 'path'.)";
                return result;
            }
            result.Content.Add(ImageContentBlock.FromBytes(File.ReadAllBytes(path), mime));
        }
        catch (JsonException) { /* corpo non JSON: resta il testo così com'è */ }
        return result;
    }
}
