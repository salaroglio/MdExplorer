using System.Net;

namespace MdExplorer.Scheduler;

/// <summary>How the hand-over of a due schedule to the Service went.</summary>
public enum FireOutcome
{
    /// <summary>The Service took the run: it executes it and writes its execution log.</summary>
    Delegated,

    /// <summary>MdExplorer is not running: nobody to hand the run to.</summary>
    ServiceOff,

    /// <summary>The Service is running the same agent already.</summary>
    Busy,

    /// <summary>The Service refused the run (schedule gone, disabled, not trusted, …).</summary>
    Refused,
}

public sealed record FireResult(FireOutcome Outcome, string Detail);

/// <summary>
/// The Scheduler hands every due cron schedule to the Service (sprint 2026-09-29-Motore-LLM-Unico, D13): the run
/// then goes as a manual launch goes — the engine of the card or of the project (Claude Code, Copilot, opencode),
/// RunToken, worktree, git identity — instead of a second, Copilot-only copy of that logic here. The Service's port
/// is the one it publishes (<c>MDEXPLORER_PORT</c>, else <c>%AppData%/MdExplorer/port.txt</c>, as MdExplorer.Mcp
/// reads it); no port, or nobody listening, means MdExplorer is off.
/// </summary>
public sealed class ServiceClient
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public static int? DiscoverPort()
    {
        var envPort = Environment.GetEnvironmentVariable("MDEXPLORER_PORT");
        if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out var port)) return port;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(appData)) return null;
        var portFile = Path.Combine(appData, "MdExplorer", "port.txt");
        if (!File.Exists(portFile)) return null;
        return int.TryParse(File.ReadAllText(portFile).Trim(), out var filePort) ? filePort : null;
    }

    public async Task<FireResult> FireAsync(Guid scheduleId, CancellationToken ct)
    {
        var port = DiscoverPort();
        if (port == null) return new FireResult(FireOutcome.ServiceOff, "porta del servizio non pubblicata");

        HttpResponseMessage response;
        try
        {
            response = await Http.PostAsync($"http://localhost:{port}/api/AgentSchedules/{scheduleId}/fire", null, ct);
        }
        catch (HttpRequestException ex)
        {
            return new FireResult(FireOutcome.ServiceOff, $"nessuno risponde sulla porta {port} ({ex.Message})");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return new FireResult(FireOutcome.ServiceOff, $"il servizio sulla porta {port} non ha risposto entro 15 s");
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            return response.StatusCode switch
            {
                HttpStatusCode.Accepted or HttpStatusCode.OK => new FireResult(FireOutcome.Delegated, body),
                HttpStatusCode.Conflict => new FireResult(FireOutcome.Busy, body),
                _ => new FireResult(FireOutcome.Refused, $"{(int)response.StatusCode}: {body}"),
            };
        }
    }
}
