using System.Diagnostics;
using System.Text;
using MdExplorer.Features.Services.AI.CopilotAcp;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Scheduler;

public record CopilotResult(string Status, string? OutputTail, string? Error);

/// <summary>
/// Headless one-shot Copilot CLI runner — mirror of CopilotCliProvider's spawn logic
/// (MdExplorer.bll), duplicated here because the satellite must not reference the bll.
/// Binary resolution is SHARED via the linked CopilotProcessLauncher.cs.
/// If you change the args contract in CopilotCliProvider, update this too.
/// </summary>
public class CopilotRunner
{
    private const int ProcessTimeoutMs = 5 * 60 * 1000;
    private const int MaxCommandLineChars = 30000;
    private const int OutputTailChars = 8000;

    private readonly ILogger<CopilotRunner> _logger;

    public CopilotRunner(ILogger<CopilotRunner> logger)
    {
        _logger = logger;
    }

    public bool IsCopilotAvailable() => CopilotProcessLauncher.IsResolvable();

    public async Task<CopilotResult> RunAsync(string prompt, string projectPath, CancellationToken ct)
    {
        var useStdin = prompt.Length > MaxCommandLineChars;

        var args = new StringBuilder();
        if (useStdin)
        {
            args.Append("-p - ");
        }
        else
        {
            var escaped = prompt.Replace("\"", "\\\"");
            args.Append($"-p \"{escaped}\" ");
        }
        // No --model: the CLI picks its own default (a hardcoded model id breaks on
        // CLI versions where that id does not exist).
        args.Append("--no-color --screen-reader --allow-all-tools --stream off");

        var psi = CopilotProcessLauncher.BuildStartInfo(args.ToString());
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.StandardOutputEncoding = Encoding.UTF8;
        psi.StandardErrorEncoding = Encoding.UTF8;
        psi.WorkingDirectory = projectPath;
        if (useStdin)
        {
            psi.RedirectStandardInput = true;
        }

        using var process = new Process { StartInfo = psi };
        process.Start();

        if (useStdin)
        {
            await process.StandardInput.WriteAsync(prompt);
            process.StandardInput.Close();
        }

        var outputTask = process.StandardOutput.ReadToEndAsync(ct);
        var errorTask = process.StandardError.ReadToEndAsync(ct);

        using var registration = ct.Register(() =>
        {
            try { process.Kill(entireProcessTree: true); } catch { /* already gone */ }
        });

        var exited = await Task.Run(() => process.WaitForExit(ProcessTimeoutMs), ct);
        if (!exited)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* already gone */ }
            return new CopilotResult("timeout", null, "Copilot CLI process timed out after 5 minutes");
        }

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            var message = string.IsNullOrWhiteSpace(error) ? $"Copilot CLI exit code {process.ExitCode}" : error.Trim();
            _logger.LogError("[CopilotRunner] exit={ExitCode} stderr={Error}", process.ExitCode, message);
            return new CopilotResult("error", Tail(output), message);
        }

        return new CopilotResult("success", Tail(output), null);
    }

    private static string? Tail(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var trimmed = text.Trim();
        return trimmed.Length <= OutputTailChars ? trimmed : trimmed[^OutputTailChars..];
    }
}
