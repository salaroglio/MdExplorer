using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Execution;
using MdExplorer.Features.Services.AI;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Singleton executor for <c>*.agent.md</c> agents. Fire-and-forget, one run per agent
    /// file; progress is streamed via the <c>agentJobProgress</c> event on MonitorMDHub
    /// (same plumbing the Mark folder job uses).
    /// </summary>
    public class AgentRunJobService : IAgentRunJobService
    {
        private const string ProgressEvent = "agentJobProgress";
        private const int OutputTailChars = 2000;

        private readonly ILogger<AgentRunJobService> _logger;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IEnumerable<IAiProvider> _aiProviders;

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();

        public AgentRunJobService(
            ILogger<AgentRunJobService> logger,
            IHubContext<MonitorMDHub> hubContext,
            IEnumerable<IAiProvider> aiProviders)
        {
            _logger = logger;
            _hubContext = hubContext;
            _aiProviders = aiProviders;
        }

        // NOTE: deliberately NOT async — the registry check must run synchronously so the
        // controller can catch the "already running" exception before the request returns.
        public Task RunAsync(AgentRunRequestModel request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ProjectPath) || !Directory.Exists(request.ProjectPath))
                throw new ArgumentException($"Project path does not exist: '{request.ProjectPath}'");
            if (string.IsNullOrWhiteSpace(request.AgentFilePath) || !File.Exists(request.AgentFilePath))
                throw new ArgumentException($"Agent file does not exist: '{request.AgentFilePath}'");
            if (string.IsNullOrWhiteSpace(request.PreparedPrompt))
                throw new ArgumentException("Prepared prompt is empty");

            var key = RunKey(request.AgentFilePath);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (!_running.TryAdd(key, cts))
            {
                cts.Dispose();
                throw new InvalidOperationException(
                    $"Agent '{Path.GetFileName(request.AgentFilePath)}' is already running.");
            }

            return RunInternalAsync(request, cts);
        }

        public void Cancel(string agentFilePath)
        {
            if (!string.IsNullOrWhiteSpace(agentFilePath)
                && _running.TryGetValue(RunKey(agentFilePath), out var cts))
            {
                try { cts.Cancel(); }
                catch (ObjectDisposedException) { /* job already finished */ }
            }
        }

        public bool IsRunning(string agentFilePath)
        {
            return !string.IsNullOrWhiteSpace(agentFilePath)
                && _running.ContainsKey(RunKey(agentFilePath));
        }

        private async Task RunInternalAsync(AgentRunRequestModel request, CancellationTokenSource cts)
        {
            var agentName = Path.GetFileName(request.AgentFilePath);
            try
            {
                _logger.LogInformation(
                    "[AgentRun] STARTED agent='{Agent}' trigger='{Trigger}' runId={RunId}",
                    agentName, request.TriggerSource, request.RunId);

                await SendAsync(request, new
                {
                    runId = request.RunId,
                    scheduleId = request.ScheduleId,
                    agentName,
                    agentFilePath = request.AgentFilePath,
                    triggerSource = request.TriggerSource,
                    phase = "started"
                });

                var unresolved = AgentPromptComposer.FindUnresolvedPlaceholders(request.PreparedPrompt);
                if (unresolved.Count > 0)
                {
                    throw new InvalidOperationException(
                        $"Prompt still contains unresolved placeholders: {string.Join(", ", unresolved)}");
                }

                var copilot = _aiProviders?
                    .FirstOrDefault(p => p.GetProviderType() == ProviderType.CopilotCli) as CopilotCliProvider;
                if (copilot == null || !copilot.IsAvailable())
                {
                    throw new InvalidOperationException(
                        "Copilot CLI is not installed or not authenticated. Install it and run 'copilot' once to log in.");
                }

                var agentContent = await File.ReadAllTextAsync(request.AgentFilePath, cts.Token);
                var composedPrompt = AgentPromptComposer.ComposeRunPrompt(agentContent, request.PreparedPrompt);

                copilot.WorkingDirectory = request.ProjectPath;
                var output = await copilot.ChatAsync(composedPrompt, ct: cts.Token);

                _logger.LogInformation(
                    "[AgentRun] COMPLETED agent='{Agent}' runId={RunId} outputChars={Chars}",
                    agentName, request.RunId, output?.Length ?? 0);

                await SendAsync(request, new
                {
                    runId = request.RunId,
                    scheduleId = request.ScheduleId,
                    agentName,
                    agentFilePath = request.AgentFilePath,
                    triggerSource = request.TriggerSource,
                    phase = "completed",
                    outputTail = Tail(output)
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[AgentRun] CANCELLED agent='{Agent}' runId={RunId}", agentName, request.RunId);
                await SendAsync(request, new
                {
                    runId = request.RunId,
                    scheduleId = request.ScheduleId,
                    agentName,
                    agentFilePath = request.AgentFilePath,
                    triggerSource = request.TriggerSource,
                    phase = "cancelled"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AgentRun] FAILED agent='{Agent}' runId={RunId}", agentName, request.RunId);
                await SendAsync(request, new
                {
                    runId = request.RunId,
                    scheduleId = request.ScheduleId,
                    agentName,
                    agentFilePath = request.AgentFilePath,
                    triggerSource = request.TriggerSource,
                    phase = "failed",
                    error = ex.Message
                });
            }
            finally
            {
                _running.TryRemove(RunKey(request.AgentFilePath), out _);
                cts.Dispose();
            }
        }

        private static string RunKey(string agentFilePath)
        {
            return Path.GetFullPath(agentFilePath).ToLowerInvariant();
        }

        private static string Tail(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= OutputTailChars ? text : text.Substring(text.Length - OutputTailChars);
        }

        private async Task SendAsync(AgentRunRequestModel request, object payload)
        {
            try
            {
                // Always broadcast: scheduled/hook runs have no originating connection, and
                // for manual runs every open window should see the outcome toast. Clients
                // that care about a specific run filter by runId.
                await _hubContext.Clients.All.SendAsync(ProgressEvent, payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AgentRun] SignalR send failed");
            }
        }
    }
}
