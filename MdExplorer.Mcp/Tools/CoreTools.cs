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

    // ---- documenti in markdown (27/11/2026): lo stesso motore del wizard «Aggiungi file esistente come collegamento» ----

    [McpServerTool, Description(
        "Convert a PDF (with MdExplorer's local OCR model) or a Word .docx that is ALREADY in the project into a markdown copy, " +
        "with the same engine as the tree's «Add existing file as link» wizard. Returns at once with a conversion id: follow it " +
        "with document_conversion_status (a PDF takes about 3-30 seconds per page). The markdown is created next to the source, " +
        "or next to `document` when given (and a link to it is added there); a free name, never overwriting. RULES: the PDF/Word " +
        "stays the authoritative source, the markdown is a working copy — check tables, numbering and page references against " +
        "the source before relying on them, and tell the user about truncated pages. Do not convert without the user's consent " +
        "when the document is long.")]
    public async Task<string> document_convert(
        [Description("Project name (see GetProjects).")] string project,
        [Description("The .pdf or .docx, path from the project root (e.g. 'docs/assets/syllabus.pdf').")] string source,
        [Description("Optional: the markdown document that gets a link to the converted copy (path from the project root).")] string? document = null,
        [Description("Optional: the OCR model id; without it, the first downloaded one.")] string? modelId = null)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var projectPath = await ResolveProjectPathAsync(client, project);
        if (projectPath == null) return $"Project '{project}' not found. Use GetProjects to list the available names.";
        try
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new { projectPath, source, document, modelId });
            var resp = await client.PostAsync("/api/DocumentConversion/convert",
                new StringContent(payload, System.Text.Encoding.UTF8, "application/json"));
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("document_convert", project, source, body);
            if (!resp.IsSuccessStatusCode) return ConversionError(resp, body);
            var id = System.Text.Json.JsonDocument.Parse(body).RootElement.GetProperty("id").GetString();
            return $"Conversion started (id {id}). Follow it with document_conversion_status.";
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}. Ensure MdExplorer is running.";
        }
    }

    [McpServerTool, Description(
        "The state of a conversion started with document_convert: queued, running (page N of M), done (with the markdown path " +
        "from the project root, truncated pages and warnings) or error. Conversions run one at a time.")]
    public async Task<string> document_conversion_status(
        [Description("The conversion id returned by document_convert.")] string id)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        try
        {
            var resp = await client.GetAsync("/api/DocumentConversion/jobs/" + Uri.EscapeDataString(id));
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) return ConversionError(resp, body);
            var job = System.Text.Json.JsonDocument.Parse(body).RootElement;
            string Str(string name) => job.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.String ? v.GetString() : null;
            int Int(string name) => job.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.Number ? v.GetInt32() : 0;
            List<string> List(string name) => job.TryGetProperty(name, out var v) && v.ValueKind == System.Text.Json.JsonValueKind.Array
                ? v.EnumerateArray().Select(e => e.ToString()).ToList() : new List<string>();
            var file = Str("fileName");
            switch (Str("status"))
            {
                case "queued": return $"{file}: waiting (another conversion is running).";
                case "running": return Int("pages") > 0 ? $"{file}: page {Int("page")} of {Int("pages")}." : $"{file}: running.";
                case "error": return $"{file}: failed — {Str("error")}";
                case "done":
                    var sb = new System.Text.StringBuilder($"{file}: done → {Str("outputRelativePath")}.");
                    var truncated = List("truncatedPages");
                    if (truncated.Count > 0) sb.Append($" WARNING: pages {string.Join(", ", truncated)} were not finished by the model: check them in the source and tell the user.");
                    var warnings = List("warnings");
                    if (warnings.Count > 0) sb.Append(" Warnings: ").Append(string.Join("; ", warnings)).Append('.');
                    var follow = List("followUpJobIds");
                    if (follow.Count > 0) sb.Append($" Attached PDFs are being converted too: {string.Join(", ", follow)}.");
                    sb.Append(" The source stays authoritative: verify tables, numbering and page references before relying on the copy.");
                    return sb.ToString();
                default: return body;
            }
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}. Ensure MdExplorer is running.";
        }
    }

    private static string ConversionError(HttpResponseMessage resp, string body)
    {
        try
        {
            var root = System.Text.Json.JsonDocument.Parse(body).RootElement;
            var message = root.TryGetProperty("error", out var e) ? e.GetString() : body;
            // needsModel: il messaggio del servizio dice già all'utente dove si scarica il modello.
            return root.TryGetProperty("needsModel", out var n) && n.ValueKind == System.Text.Json.JsonValueKind.True
                ? $"Error: {message} Tell the user, do not try another way."
                : $"Error: {message}";
        }
        catch (System.Text.Json.JsonException) { return $"Error: MdExplorer answered {(int)resp.StatusCode}: {body}"; }
    }
}
