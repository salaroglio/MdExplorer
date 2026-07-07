using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using Ad.Tools.Dal.Extensions;
using MdExplorer.bll.Models.AI;
using MdExplorer.Features.Services.AI.CopilotAcp;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Provider per GitHub Copilot CLI (copilot -p).
    /// Uses the locally installed copilot CLI tool instead of HTTP APIs.
    /// No API key required - uses GitHub OAuth authentication.
    /// </summary>
    public class CopilotCliProvider : IAiProvider
    {
        private readonly ILogger<CopilotCliProvider> _logger;
        private readonly IServiceProvider _serviceProvider;
        private string _systemPrompt;

        private const string USAGE_SEPARATOR = "Total usage est:";
        private const int PROCESS_TIMEOUT_MS = 300000; // 5 minutes
        private const int AVAILABILITY_CHECK_TIMEOUT_MS = 5000;
        private const string SYSTEM_PROMPT_SETTING = "CopilotCli_SystemPrompt";
        private const string DEFAULT_MODEL = "claude-sonnet-5";
        private const int MAX_COMMAND_LINE_CHARS = 30000;

        /// <summary>
        /// Working directory for the copilot process.
        /// Should be set to the current MdExplorer project path before each call.
        /// </summary>
        public string WorkingDirectory { get; set; }

        // Availability cache
        private bool? _cachedAvailability;
        private DateTime _availabilityCacheExpiry = DateTime.MinValue;
        private static readonly TimeSpan AvailabilityCacheDuration = TimeSpan.FromMinutes(5);

        public CopilotCliProvider(
            ILogger<CopilotCliProvider> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public string GetName() => "GitHub Copilot CLI";

        public ProviderType GetProviderType() => ProviderType.CopilotCli;

        /// <summary>
        /// Returns the cached availability if fresh, null if cache is cold/expired.
        /// Non-blocking: never spawns a subprocess. Callers on hot paths should use
        /// this and fire-and-forget <see cref="IsAvailable"/> in the background to
        /// warm the cache for next time.
        /// </summary>
        public bool? TryGetCachedAvailability()
        {
            if (_cachedAvailability.HasValue && DateTime.UtcNow < _availabilityCacheExpiry)
            {
                return _cachedAvailability.Value;
            }
            return null;
        }

        /// <summary>
        /// Deterministic installation check: PATH scan for copilot.exe/.cmd/.ps1.
        /// Single-digit ms, no process spawn, no timeout.
        /// <para>
        /// Old behaviour was a <c>copilot --version</c> probe with 5-second timeout. That
        /// conflated "installed" (file exists in PATH) with "starts within 5s" (runtime
        /// performance). Symptom: Copilot CLI 1.0.51 cold-starts in ~8.5s while it runs
        /// its own update check; MDE killed the process at 5s and concluded "not installed"
        /// on a perfectly installed system. Verified 2026-05-24.
        /// </para>
        /// <para>
        /// We don't probe the version. If the file is there, it's installed. If a real
        /// launch later fails (corrupt binary, permission denied, ...) that's a separate
        /// concern surfaced at use time with the real error, not as a silent "not available".
        /// </para>
        /// </summary>
        public bool IsAvailable()
        {
            if (_cachedAvailability.HasValue && DateTime.UtcNow < _availabilityCacheExpiry)
            {
                return _cachedAvailability.Value;
            }

            var resolvable = CopilotProcessLauncher.IsResolvable();
            _cachedAvailability = resolvable;
            _availabilityCacheExpiry = DateTime.UtcNow + AvailabilityCacheDuration;
            if (!resolvable)
            {
                _logger.LogInformation(
                    "[CopilotCliProvider.IsAvailable] copilot.exe/.cmd/.ps1 not found in PATH — Copilot CLI not installed");
            }
            return resolvable;
        }

        public ProviderCapabilities GetCapabilities()
        {
            return new ProviderCapabilities
            {
                SupportsStreaming = true,
                SupportsFunctionCalling = false,
                SupportsEmbeddings = false,
                SupportsVision = false,
                MaxInputTokens = 128000,
                MaxOutputTokens = 16000,
                AvailableModels = null
            };
        }

        public async Task<string> ChatAsync(string prompt, string modelId = null, CancellationToken ct = default)
        {
            _logger.LogInformation("[CopilotCliProvider.ChatAsync] Starting with prompt length: {Length}", prompt?.Length ?? 0);

            if (!IsAvailable())
            {
                throw new InvalidOperationException("Copilot CLI is not available. Make sure it is installed and authenticated.");
            }

            var model = modelId ?? DEFAULT_MODEL;
            var output = await RunCopilotProcessAsync(prompt, model, streaming: false, ct: ct);

            return StripUsageMetrics(output);
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            string prompt,
            string modelId = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            _logger.LogInformation("[CopilotCliProvider.StreamChatAsync] Starting with prompt length: {Length}", prompt?.Length ?? 0);

            if (!IsAvailable())
            {
                throw new InvalidOperationException("Copilot CLI is not available. Make sure it is installed and authenticated.");
            }

            var model = modelId ?? DEFAULT_MODEL;

            var psi = CreateProcessStartInfo(prompt, model, streaming: true);
            using var process = new Process { StartInfo = psi };

            process.Start();

            // If prompt is too long, write to stdin
            if (ShouldUseStdin(prompt))
            {
                await WritePromptToStdinAsync(process, prompt);
            }

            var buffer = new char[256];
            var usageFound = false;
            var pendingText = new StringBuilder();

            while (!ct.IsCancellationRequested)
            {
                int bytesRead;
                try
                {
                    bytesRead = await process.StandardOutput.ReadAsync(buffer, 0, buffer.Length);
                }
                catch (Exception)
                {
                    break;
                }

                if (bytesRead == 0) break;

                var chunk = new string(buffer, 0, bytesRead);

                // Check if usage metrics have started
                if (chunk.Contains(USAGE_SEPARATOR))
                {
                    // Output only the part before the usage separator
                    var idx = chunk.IndexOf(USAGE_SEPARATOR, StringComparison.Ordinal);
                    var beforeUsage = chunk.Substring(0, idx).TrimEnd();
                    if (!string.IsNullOrEmpty(beforeUsage))
                    {
                        yield return beforeUsage;
                    }
                    usageFound = true;
                    break;
                }

                if (!usageFound)
                {
                    yield return chunk;
                }
            }

            if (ct.IsCancellationRequested)
            {
                try { process.Kill(); } catch { }
            }
            else
            {
                // Wait for process to finish
                try
                {
                    if (!process.HasExited)
                    {
                        process.WaitForExit(5000);
                    }
                    // Log stderr if there was no output (helps diagnose CopilotCli issues)
                    var stderr = await process.StandardError.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(stderr))
                    {
                        _logger.LogWarning("[CopilotCliProvider.StreamChatAsync] stderr: {StdErr}", stderr.Length > 500 ? stderr.Substring(0, 500) : stderr);
                    }
                    _logger.LogInformation("[CopilotCliProvider.StreamChatAsync] Process exit code: {ExitCode}", process.ExitCode);
                }
                catch { }
            }
        }

        public async Task<string> ChatWithToolsAsync(
            string prompt,
            List<object> tools,
            Func<string, dynamic, Task<object>> toolExecutor,
            string modelId = null,
            string currentDocumentPath = null,
            List<object> conversationHistory = null,
            CancellationToken ct = default)
        {
            _logger.LogInformation("[CopilotCliProvider.ChatWithToolsAsync] Starting with prompt length: {Length}", prompt?.Length ?? 0);

            // Load system prompt if needed
            if (string.IsNullOrEmpty(_systemPrompt))
            {
                _systemPrompt = await GetSystemPromptAsync();
            }

            // Build composite prompt with conversation history
            var compositePrompt = new StringBuilder();

            // System instructions
            if (!string.IsNullOrEmpty(_systemPrompt))
            {
                compositePrompt.AppendLine("System instructions:");
                compositePrompt.AppendLine(_systemPrompt);
                compositePrompt.AppendLine();
            }

            // Current document context
            if (!string.IsNullOrEmpty(currentDocumentPath))
            {
                compositePrompt.AppendLine($"Current document: {currentDocumentPath}");
                compositePrompt.AppendLine();
            }

            // Conversation history
            if (conversationHistory != null && conversationHistory.Count > 0)
            {
                compositePrompt.AppendLine("Previous conversation:");
                foreach (var msgObj in conversationHistory)
                {
                    if (msgObj is ConversationMessage msg)
                    {
                        var role = msg.Role == "model" ? "Assistant" : "User";
                        compositePrompt.AppendLine($"{role}: {msg.Content}");
                    }
                }
                compositePrompt.AppendLine();
            }

            // Current question
            compositePrompt.AppendLine("Current question:");
            compositePrompt.AppendLine(prompt);

            return await ChatAsync(compositePrompt.ToString(), modelId, ct);
        }

        public async Task SetSystemPromptAsync(string systemPrompt)
        {
            await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();

                    try
                    {
                        var settingsDal = session.GetDal<Setting>();
                        session.BeginTransaction();

                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == SYSTEM_PROMPT_SETTING);
                        if (setting == null)
                        {
                            setting = new Setting
                            {
                                Name = SYSTEM_PROMPT_SETTING,
                                Description = "System prompt for Copilot CLI"
                            };
                        }

                        setting.ValueString = systemPrompt;
                        settingsDal.Save(setting);
                        session.Commit();
                        _systemPrompt = systemPrompt;
                        _logger.LogInformation("Copilot CLI system prompt saved successfully");
                    }
                    catch (Exception ex)
                    {
                        session.Rollback();
                        _logger.LogError(ex, "Error saving Copilot CLI system prompt");
                        throw;
                    }
                }
            });
        }

        public async Task<string> GetSystemPromptAsync()
        {
            return await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(_systemPrompt))
                    return _systemPrompt;

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                        var settingsDal = session.GetDal<Setting>();
                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == SYSTEM_PROMPT_SETTING);

                        if (setting != null && !string.IsNullOrEmpty(setting.ValueString))
                        {
                            _systemPrompt = setting.ValueString;
                            return _systemPrompt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading Copilot CLI system prompt");
                }

                return @"You are a helpful AI assistant specialized in markdown editing and document management.
You excel at creating well-structured markdown documents, generating summaries, and helping with technical documentation.
Always provide clear, concise, and well-formatted responses using proper markdown syntax when appropriate.";
            });
        }

        public Task<string> GetApiKeyAsync()
        {
            // Copilot CLI uses GitHub OAuth, no API key needed
            return Task.FromResult("not-required");
        }

        public Task SaveApiKeyAsync(string apiKey)
        {
            // No-op: Copilot CLI uses GitHub OAuth
            return Task.CompletedTask;
        }

        public Task<bool> TestApiKeyAsync(string apiKey)
        {
            return Task.FromResult(IsAvailable());
        }

        /// <summary>
        /// Gets the version of the installed Copilot CLI.
        /// </summary>
        public async Task<string> GetVersionAsync()
        {
            try
            {
                var psi = CopilotProcessLauncher.BuildStartInfo("--version");
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                using var process = Process.Start(psi);
                if (process == null) return null;

                var output = await process.StandardOutput.ReadToEndAsync();
                process.WaitForExit(AVAILABILITY_CHECK_TIMEOUT_MS);

                return output?.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Copilot CLI version");
                return null;
            }
        }

        #region Private helpers

        private async Task<string> RunCopilotProcessAsync(string prompt, string model, bool streaming, CancellationToken ct)
        {
            var psi = CreateProcessStartInfo(prompt, model, streaming);
            using var process = new Process { StartInfo = psi };

            process.Start();

            // If prompt is too long, write to stdin
            if (ShouldUseStdin(prompt))
            {
                await WritePromptToStdinAsync(process, prompt);
            }

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            using var registration = ct.Register(() =>
            {
                try { process.Kill(); } catch { }
            });

            var completed = process.WaitForExit(PROCESS_TIMEOUT_MS);
            if (!completed)
            {
                try { process.Kill(); } catch { }
                throw new TimeoutException("Copilot CLI process timed out after 5 minutes");
            }

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
            {
                _logger.LogError("[CopilotCliProvider] Process exited with code {ExitCode}: {Error}", process.ExitCode, error);
                throw new Exception($"Copilot CLI error (exit code {process.ExitCode}): {error}");
            }

            return output;
        }

        private ProcessStartInfo CreateProcessStartInfo(string prompt, string model, bool streaming)
        {
            var useStdin = ShouldUseStdin(prompt);

            // Build the argument string first; CopilotProcessLauncher.BuildStartInfo will splice it
            // into the right wrapper (direct copilot.exe, cmd.exe /c copilot.cmd, or powershell -File copilot.ps1).
            var args = new StringBuilder();
            if (useStdin)
            {
                args.Append("-p - "); // Read prompt from stdin
            }
            else
            {
                var escapedPrompt = prompt.Replace("\"", "\\\"");
                args.Append($"-p \"{escapedPrompt}\" ");
            }
            args.Append("--no-color ");
            args.Append("--screen-reader ");
            args.Append("--allow-all-tools ");
            args.Append($"--model {model}");
            if (!streaming)
            {
                args.Append(" --stream off");
            }

            var psi = CopilotProcessLauncher.BuildStartInfo(args.ToString());
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;
            if (useStdin)
            {
                psi.RedirectStandardInput = true;
            }

            if (!string.IsNullOrEmpty(WorkingDirectory) && System.IO.Directory.Exists(WorkingDirectory))
            {
                psi.WorkingDirectory = WorkingDirectory;
                _logger.LogInformation("[CopilotCliProvider] Working directory set to: {WorkingDir}", WorkingDirectory);
            }

            return psi;
        }

        private bool ShouldUseStdin(string prompt)
        {
            return prompt != null && prompt.Length > MAX_COMMAND_LINE_CHARS;
        }

        private async Task WritePromptToStdinAsync(Process process, string prompt)
        {
            await process.StandardInput.WriteAsync(prompt);
            process.StandardInput.Close();
        }

        private string StripUsageMetrics(string output)
        {
            if (string.IsNullOrEmpty(output)) return output;

            var idx = output.LastIndexOf(USAGE_SEPARATOR, StringComparison.Ordinal);
            return idx >= 0 ? output.Substring(0, idx).TrimEnd() : output.TrimEnd();
        }

        #endregion
    }
}
