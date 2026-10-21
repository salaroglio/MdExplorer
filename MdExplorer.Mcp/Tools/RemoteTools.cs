using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>remote</c>: i file delle radici di codice REMOTE, cartelle su un server raggiunto via SSH (sprint
/// 2026-10-21-Cartella-Remota-Via-SSH, F4). I CLI vedono solo il disco locale: per una radice remota leggono, cercano e
/// scrivono con questi tool, che passano dal backend e dal serverino sul server (dentro la radice, niente link, impronta
/// per scrivere). Acceso di default nei progetti con una radice remota collegata su questa macchina.
/// </summary>
[McpServerToolType]
public sealed class RemoteTools : McpToolsBase
{
    public RemoteTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

    private const string PathHelp = "Logical path of a REMOTE code root: '@alias' or '@alias/dir/file' (see code_roots: roots with a `remote` location).";

    [McpServerTool, Description(
        "List a folder of a REMOTE code root (a folder on an SSH server, not on this disk): subfolders first, then files, " +
        "each with its logical path '@alias/…' and whether it is text. Use the remote_* tools for every file of a root whose " +
        "code_roots entry has `remote` set: your own file tools cannot reach it.")]
    public async Task<string> remote_list(
        [Description("Project name (see GetProjects).")] string project,
        [Description(PathHelp)] string path)
    {
        var (client, projectPath, error) = await ProjectAsync(project);
        if (error != null) return error;
        var url = $"/api/coderoots/browse?projectPath={Uri.EscapeDataString(projectPath)}&path={Uri.EscapeDataString(path)}";
        try
        {
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) return Error(resp, body);
            await LogToolCall("remote_list", project, path, body);
            return body;
        }
        catch (HttpRequestException ex) { return $"Error connecting to MdExplorer: {ex.Message}"; }
    }

    [McpServerTool, Description(
        "Read a text file of a REMOTE code root. Returns its text with line numbers ('  12| …') for reference — the numbers are NOT " +
        "part of the file. Binary and non-UTF-8 files are refused.")]
    public async Task<string> remote_read(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path)
    {
        var (ok, result) = await FileOpAsync("remote_read", project, new { op = "read", path });
        if (!ok) return result;
        var text = JsonDocument.Parse(result).RootElement.GetProperty("text").GetString() ?? "";
        var numbered = new StringBuilder();
        var lines = text.Split('\n');
        var count = text.EndsWith('\n') ? lines.Length - 1 : lines.Length;
        for (var i = 0; i < count; i++) numbered.Append((i + 1).ToString().PadLeft(5)).Append("| ").Append(lines[i]).Append('\n');
        return $"{path} ({count} lines)\n{numbered}";
    }

    [McpServerTool, Description(
        "Search a .NET regular expression in the text files under a folder (or in one file) of a REMOTE code root. The search " +
        "runs ON THE SERVER and returns only the matching lines (path '@alias/…', line number, text); `.git` is skipped. " +
        "Prefer it to reading many files.")]
    public async Task<string> remote_grep(
        [Description("Project name.")] string project,
        [Description(PathHelp + " A folder searches everything under it.")] string path,
        [Description(".NET regular expression, e.g. 'SaveOrder\\s*\\('.")] string pattern,
        [Description("Case-insensitive (default false).")] bool? ignoreCase = null)
    {
        var (ok, result) = await FileOpAsync("remote_grep", project, new { op = "grep", path, pattern, ignoreCase = ignoreCase ?? false });
        if (!ok) return result;
        // una riga per risultato, «@alias/percorso:riga: testo», come grep: niente campi vuoti nel contesto
        var root = JsonDocument.Parse(result).RootElement;
        var hits = root.GetProperty("hits").EnumerateArray()
            .Select(h => $"{h.GetProperty("path").GetString()}:{h.GetProperty("line").GetInt32()}: {h.GetProperty("text").GetString()}")
            .ToList();
        if (hits.Count == 0) return $"No matches for '{pattern}' under {path}.";
        var truncated = root.TryGetProperty("truncated", out var t) && t.ValueKind == JsonValueKind.True;
        return string.Join("\n", hits) + (truncated ? $"\n… more matches not shown (first {hits.Count}): narrow the path or the pattern." : "");
    }

    [McpServerTool, Description(
        "Edit a text file of a REMOTE code root by replacing `old_text` with `new_text`. `old_text` must appear exactly once (add " +
        "surrounding lines to make it unique), unless `replace_all` is true. Read the file with remote_read first; copy `old_text` " +
        "WITHOUT the line-number prefix. The write is refused if the file changed on the server in the meantime: read it again.")]
    public async Task<string> remote_edit(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path,
        [Description("Exact text to replace (no line-number prefixes).")] string old_text,
        [Description("Replacement text.")] string new_text,
        [Description("Replace every occurrence (default false).")] bool? replace_all = null)
    {
        if (string.IsNullOrEmpty(old_text)) return "Error: old_text is empty.";
        var (ok, read) = await FileOpAsync("remote_edit", project, new { op = "read", path });
        if (!ok) return read;
        var root = JsonDocument.Parse(read).RootElement;
        var text = root.GetProperty("text").GetString() ?? "";
        var stamp = root.GetProperty("stamp").GetString();
        var count = Occurrences(text, old_text);
        if (count == 0) return $"Error: old_text not found in {path}. Read the file again (remote_read) and copy the text exactly, without line numbers.";
        if (count > 1 && replace_all != true) return $"Error: old_text appears {count} times in {path}: add surrounding lines to make it unique, or set replace_all.";
        var updated = replace_all == true ? text.Replace(old_text, new_text, StringComparison.Ordinal) : ReplaceFirst(text, old_text, new_text);
        var (written, result) = await FileOpAsync("remote_edit", project, new { op = "write", path, text = updated, stamp });
        return written ? $"Edited {path}: {(replace_all == true ? count : 1)} replacement(s)." : result;
    }

    [McpServerTool, Description(
        "Write a whole text file of a REMOTE code root: creates it (with missing folders) if it does not exist, otherwise replaces " +
        "its content (UTF-8; an existing BOM is kept). For small changes to an existing file prefer remote_edit.")]
    public async Task<string> remote_write(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path,
        [Description("The full new content.")] string text)
    {
        var (exists, read) = await FileOpAsync("remote_write", project, new { op = "read", path });
        if (!exists)
        {
            if (!IsCode(read, "not_found")) return read;
            var (created, result) = await FileOpAsync("remote_write", project, new { op = "create", path, text });
            return created ? $"Created {path}." : result;
        }
        var stamp = JsonDocument.Parse(read).RootElement.GetProperty("stamp").GetString();
        var (written, writeResult) = await FileOpAsync("remote_write", project, new { op = "write", path, text, stamp });
        return written ? $"Wrote {path}." : writeResult;
    }

    [McpServerTool, Description("Create a folder (and missing parents) in a REMOTE code root.")]
    public async Task<string> remote_mkdir(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path)
    {
        var (ok, result) = await FileOpAsync("remote_mkdir", project, new { op = "mkdir", path });
        return ok ? $"Created folder {path}." : result;
    }

    [McpServerTool, Description(
        "Delete a file or a folder of a REMOTE code root. A non-empty folder needs `recursive` true. It cannot be undone: " +
        "delete only what the user asked for.")]
    public async Task<string> remote_delete(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path,
        [Description("Delete a non-empty folder with everything in it (default false).")] bool? recursive = null)
    {
        var (ok, result) = await FileOpAsync("remote_delete", project, new { op = "delete", path, recursive = recursive ?? false });
        return ok ? $"Deleted {path}." : result;
    }

    [McpServerTool, Description("Rename or move a file or folder inside the same REMOTE code root; the destination must not exist.")]
    public async Task<string> remote_move(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path,
        [Description("Destination logical path in the same root, e.g. '@alias/new/name.cs'.")] string to)
    {
        var (ok, result) = await FileOpAsync("remote_move", project, new { op = "move", path, to });
        return ok ? $"Moved {path} to {to}." : result;
    }

    // ---- infrastruttura ----

    private async Task<(HttpClient Client, string Path, string Error)> ProjectAsync(string project)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var path = await ResolveProjectPathAsync(client, project);
        return path == null ? (client, null, $"Project '{project}' not found. Use GetProjects to list the available names.") : (client, path, null);
    }

    /// <summary>POST /api/coderoots/files: (true, corpo) se riuscita, altrimenti (false, «Error: …» con il codice del serverino).</summary>
    /// <param name="tool">Il nome del tool per il log (remote_edit fa una lettura e una scrittura: nel log resta remote_edit).</param>
    private async Task<(bool Ok, string Body)> FileOpAsync(string tool, string project, object request)
    {
        var (client, projectPath, error) = await ProjectAsync(project);
        if (error != null) return (false, error);
        var payload = JsonSerializer.SerializeToElement(request);
        var fields = payload.EnumerateObject().ToDictionary(p => p.Name, p => (object)p.Value);
        fields["projectPath"] = projectPath;
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(fields), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/coderoots/files", content);
            var body = await resp.Content.ReadAsStringAsync();
            var op = fields.TryGetValue("op", out var o) ? o.ToString() : "?";
            await LogToolCall(tool, project, op + " " + (fields.TryGetValue("path", out var p) ? p.ToString() : ""), resp.IsSuccessStatusCode ? "ok" : body);
            return resp.IsSuccessStatusCode ? (true, body) : (false, Error(resp, body));
        }
        catch (HttpRequestException ex) { return (false, $"Error connecting to MdExplorer: {ex.Message}"); }
    }

    private static string Error(HttpResponseMessage resp, string body)
    {
        try
        {
            var root = JsonDocument.Parse(body).RootElement;
            var message = root.TryGetProperty("error", out var e) ? e.GetString() : body;
            var code = root.TryGetProperty("code", out var c) ? c.GetString() : null;
            return code != null ? $"Error ({code}): {message}" : $"Error: {message}";
        }
        catch (JsonException) { return $"Error: MdExplorer answered {(int)resp.StatusCode}: {body}"; }
    }

    private static bool IsCode(string error, string code) => error.StartsWith($"Error ({code})", StringComparison.Ordinal);

    private static int Occurrences(string text, string what)
    {
        var count = 0;
        for (var i = text.IndexOf(what, StringComparison.Ordinal); i >= 0; i = text.IndexOf(what, i + what.Length, StringComparison.Ordinal)) count++;
        return count;
    }

    private static string ReplaceFirst(string text, string what, string with)
    {
        var i = text.IndexOf(what, StringComparison.Ordinal);
        return text.Substring(0, i) + with + text.Substring(i + what.Length);
    }
}
