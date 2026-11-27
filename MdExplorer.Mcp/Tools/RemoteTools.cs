using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>remote</c>: i file delle radici di codice, locali e REMOTE (cartelle su un server raggiunto via SSH, sprint
/// 2026-10-21-Cartella-Remota-Via-SSH, F4). Per una radice remota i CLI non vedono il disco del server; per TUTTE le radici
/// le scritture passano da qui (sprint 2026-11-27-Modifiche-Da-Approvare-Sulle-Radici): il backend tiene l'originale di
/// ogni file cambiato, così l'utente rivede e annulla pezzo per pezzo (<c>review: true</c>). I nomi restano <c>remote_*</c>:
/// li nominano i pacchetti di linguaggio. Acceso di default nei progetti con una radice collegata su questa macchina.
/// </summary>
[McpServerToolType]
public sealed class RemoteTools : McpToolsBase
{
    public RemoteTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

    private const string PathHelp = "Logical path of a code root (local folder or remote SSH): '@alias' or '@alias/dir/file' (see code_roots).";

    [McpServerTool, Description(
        "List a folder of a code root (a local folder or one on an SSH server): subfolders first, then files, each with its " +
        "logical path '@alias/…' and whether it is text. Use the remote_* tools to CHANGE every file of any code root (your own " +
        "Edit/Write are refused there) and to read the files of a root whose code_roots entry has `remote` set.")]
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
        "Read a text file of a code root (local or remote). Returns its text with line numbers ('  12| …') for reference — the numbers are NOT " +
        "part of the file — and its real encoding when it is not UTF-8 (e.g. CP-850), with a warning when it does not match the " +
        "root's rule for that extension. Binary files are refused.")]
    public async Task<string> remote_read(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path)
    {
        var (ok, result) = await FileOpAsync("remote_read", project, new { op = "read", path });
        if (!ok) return result;
        var parsed = JsonDocument.Parse(result).RootElement;
        var text = parsed.GetProperty("text").GetString() ?? "";
        var encoding = parsed.TryGetProperty("encoding", out var enc) && enc.ValueKind == JsonValueKind.String ? enc.GetString() : null;
        var rule = parsed.TryGetProperty("ruleEncoding", out var ru) && ru.ValueKind == JsonValueKind.String ? ru.GetString() : null;
        var utf8Letters = parsed.TryGetProperty("utf8Letters", out var ul) && ul.ValueKind == JsonValueKind.Number ? ul.GetInt32() : 0;
        var numbered = new StringBuilder();
        var lines = text.Split('\n');
        var count = text.EndsWith('\n') ? lines.Length - 1 : lines.Length;
        for (var i = 0; i < count; i++) numbered.Append((i + 1).ToString().PadLeft(5)).Append("| ").Append(lines[i]).Append('\n');
        // la codifica (CP-850…) si dice: remote_edit e remote_write riscrivono in quella, e un carattere che non ha è rifiutato
        var encodingNote = encoding is null or "utf-8" ? "" : $", encoding {encoding}: write only characters it can hold";
        // la codifica VERA del file, e quando non è quella della regola della radice lo si dice: il file è già sbagliato
        if (rule != null && encoding != null && rule != encoding)
            encodingNote += $". WARNING: the root's rule says {rule} for this extension, but the file is {encoding} — it was written " +
                            "by something that ignored the rule. Edits keep the file's real encoding; tell the user, do not convert on your own";
        if (utf8Letters > 0)
            encodingNote += $". WARNING: the file is mixed: {utf8Letters} letters inside are UTF-8 (two or more bytes) and show as garbage " +
                            "in this encoding; tell the user (the MDE editor has «Repair mixed encoding»), do not rewrite them";
        return $"{path} ({count} lines{encodingNote})\n{numbered}";
    }

    [McpServerTool, Description(
        "Search a .NET regular expression in the text files under a folder (or in one file) of a code root. For a remote root the " +
        "search runs ON THE SERVER. It returns only the matching lines (path '@alias/…', line number, text); `.git` is skipped. " +
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
        "Edit a text file of a code root (local or remote) by replacing `old_text` with `new_text`: on code roots this is the way " +
        "to change a file, your own Edit/Write are refused. `old_text` must appear exactly once (add surrounding lines to make it " +
        "unique), unless `replace_all` is true. Read the file with remote_read first; copy `old_text` WITHOUT the line-number " +
        "prefix. The write is refused if the file changed in the meantime: read it again. " +
        "Your change is written to the file at once, and MDE keeps the original: the user reviews it piece by piece and can keep or undo each " +
        "piece. Tell the user which files you changed and that they are waiting for review.")]
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
        return written ? $"Edited {path}: {(replace_all == true ? count : 1)} replacement(s), waiting for the user's review." : result;
    }

    [McpServerTool, Description(
        "Write a whole text file of a code root (local or remote): creates it (with missing folders) if it does not exist, otherwise " +
        "replaces its content. " + "Your change is written to the file at once, and MDE keeps the original: the user reviews it piece by piece and can keep or undo each " +
        "piece. Tell the user which files you changed and that they are waiting for review." + " The write keeps the encoding: an existing file stays in its own " +
        "encoding (and BOM), a new file gets the encoding the root assigns to its extension (e.g. CP-850 for .cbl), UTF-8 if none; " +
        "a character the encoding cannot hold is refused. For small changes to an existing file prefer remote_edit.")]
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
            return created ? $"Created {path} (waiting for the user's review)." : result;
        }
        var stamp = JsonDocument.Parse(read).RootElement.GetProperty("stamp").GetString();
        var (written, writeResult) = await FileOpAsync("remote_write", project, new { op = "write", path, text, stamp });
        return written ? $"Wrote {path} (waiting for the user's review)." : writeResult;
    }

    [McpServerTool, Description("Create a folder (and missing parents) in a code root (local or remote).")]
    public async Task<string> remote_mkdir(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path)
    {
        var (ok, result) = await FileOpAsync("remote_mkdir", project, new { op = "mkdir", path });
        return ok ? $"Created folder {path}." : result;
    }

    [McpServerTool, Description(
        "Delete a FILE of a code root (local or remote); delete only what the user asked for. A folder is refused: delete its " +
        "files one by one, so that the user can review and undo each deletion (MDE keeps the original of every deleted file).")]
    public async Task<string> remote_delete(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path,
        [Description("Not used on code roots: folders are refused.")] bool? recursive = null)
    {
        var (ok, result) = await FileOpAsync("remote_delete", project, new { op = "delete", path, recursive = recursive ?? false });
        return ok ? $"Deleted {path} (waiting for the user's review)." : result;
    }

    [McpServerTool, Description("Rename or move a FILE inside the same code root (local or remote); the destination must not exist. A folder is refused: move its files one by one, so the user can review and undo each move.")]
    public async Task<string> remote_move(
        [Description("Project name.")] string project,
        [Description(PathHelp)] string path,
        [Description("Destination logical path in the same root, e.g. '@alias/new/name.cs'.")] string to)
    {
        var (ok, result) = await FileOpAsync("remote_move", project, new { op = "move", path, to });
        return ok ? $"Moved {path} to {to} (waiting for the user's review)." : result;
    }

    // ---- infrastruttura ----

    private async Task<(HttpClient Client, string Path, string Error)> ProjectAsync(string project)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var path = await ResolveProjectPathAsync(client, project);
        return path == null ? (client, null, $"Project '{project}' not found. Use GetProjects to list the available names.") : (client, path, null);
    }

    /// <summary>POST /api/coderoots/files: (true, corpo) se riuscita, altrimenti (false, «Error: …» con il codice di MDE Remote).</summary>
    /// <param name="tool">Il nome del tool per il log (remote_edit fa una lettura e una scrittura: nel log resta remote_edit).</param>
    private async Task<(bool Ok, string Body)> FileOpAsync(string tool, string project, object request)
    {
        var (client, projectPath, error) = await ProjectAsync(project);
        if (error != null) return (false, error);
        var payload = JsonSerializer.SerializeToElement(request);
        var fields = payload.EnumerateObject().ToDictionary(p => p.Name, p => (object)p.Value);
        fields["projectPath"] = projectPath;
        // MarkAgent: MDE tiene l'originale di ogni file che cambia, per la revisione (sprint 2026-11-27-Modifiche-Da-Approvare).
        fields["review"] = true;
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
