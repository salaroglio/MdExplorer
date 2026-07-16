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
        // No default model constant: when the caller does not specify a model we omit
        // the --model flag entirely and let the CLI pick its own default. Hardcoding a
        // model id here breaks on CLI versions where that id does not exist (the CLI
        // exits 1 with "Model ... is not available").
        private const int MAX_COMMAND_LINE_CHARS = 30000;

        /// <summary>
        /// Working directory <b>ambientale</b> per i chiamanti legacy (chat interattiva, commit
        /// message, ecc.) che non portano identità: la impostano prima di una chiamata a
        /// <see cref="ChatAsync"/>. <b>NON usarla per i run degli agenti</b>: è stato condiviso
        /// sul singleton e non è sicura sotto concorrenza. Il path degli agenti passa la working
        /// dir (e l'ambiente col RunToken) per-chiamata via <see cref="RunHeadlessAsync"/> +
        /// <see cref="CopilotInvocation"/>, dove l'isolamento è garantito per costruzione.
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

            var output = await RunCopilotProcessAsync(prompt, modelId, streaming: false, ct: ct);

            return StripUsageMetrics(output);
        }

        /// <summary>
        /// Esegue un turno headless <b>stateless</b>: working directory e ambiente (canale del
        /// RunToken, R2) arrivano nell'<paramref name="invocation"/> e vengono usati SOLO da
        /// questa chiamata — nessuna scrittura su stato condiviso del provider. È il punto
        /// d'ingresso della "città degli agenti": due run concorrenti non possono contaminarsi
        /// l'identità perché non esiste un campo su cui competere. Fail-loud se l'invocation
        /// manca (senza contesto non c'è isolamento da garantire).
        /// </summary>
        public async Task<string> RunHeadlessAsync(string prompt, CopilotInvocation invocation, string modelId = null, CancellationToken ct = default)
        {
            if (invocation == null)
                throw new ArgumentNullException(nameof(invocation),
                    "CopilotInvocation obbligatoria: un run headless deve portare il proprio contesto (working dir + ambiente), mai ereditarlo dallo stato condiviso.");

            _logger.LogInformation("[CopilotCliProvider.RunHeadlessAsync] Starting with prompt length: {Length}", prompt?.Length ?? 0);

            if (!IsAvailable())
                throw new InvalidOperationException("Copilot CLI is not available. Make sure it is installed and authenticated.");

            var output = await RunCopilotProcessAsync(prompt, modelId, streaming: false, ct: ct, invocation: invocation);
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

            var psi = CreateProcessStartInfo(prompt, modelId, streaming: true);
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

        /// <summary>
        /// Like <see cref="ChatAsync"/> but returns the assistant's message text with
        /// byte-for-byte fidelity. The default text output mode renders markdown for the
        /// terminal (headings lose their <c>#</c>, fenced blocks lose their fences, and
        /// tool-activity traces pollute stdout), which destroys any answer that must be
        /// parsed afterwards. This method runs the CLI with <c>--output-format json</c>
        /// (JSONL) and extracts the raw <c>content</c> of the last <c>assistant.message</c>
        /// event instead.
        /// </summary>
        public async Task<string> ChatRawAsync(string prompt, string modelId = null, CancellationToken ct = default)
        {
            _logger.LogInformation("[CopilotCliProvider.ChatRawAsync] Starting with prompt length: {Length}", prompt?.Length ?? 0);

            if (!IsAvailable())
            {
                throw new InvalidOperationException("Copilot CLI is not available. Make sure it is installed and authenticated.");
            }

            var jsonl = await RunCopilotProcessAsync(prompt, modelId, streaming: false, ct: ct, outputFormatJson: true);

            string lastMessage = null;
            foreach (var line in jsonl.Split('\n'))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("{")) continue;
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(trimmed);
                    if (doc.RootElement.TryGetProperty("type", out var type)
                        && type.GetString() == "assistant.message"
                        && doc.RootElement.TryGetProperty("data", out var data)
                        && data.TryGetProperty("content", out var content))
                    {
                        var text = content.GetString();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            lastMessage = text;
                        }
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // Not a JSON line (stray CLI noise) — skip it.
                }
            }

            if (lastMessage == null)
            {
                throw new InvalidOperationException(
                    "Copilot CLI produced no assistant.message event in JSON output — cannot extract the response.");
            }

            return lastMessage;
        }

        #region Private helpers

        private async Task<string> RunCopilotProcessAsync(string prompt, string model, bool streaming, CancellationToken ct, bool outputFormatJson = false, CopilotInvocation invocation = null)
        {
            var psi = CreateProcessStartInfo(prompt, model, streaming, outputFormatJson, invocation);
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

        private ProcessStartInfo CreateProcessStartInfo(string prompt, string model, bool streaming, bool outputFormatJson = false, CopilotInvocation invocation = null)
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
            if (outputFormatJson)
            {
                // JSONL events on stdout; no --screen-reader (it renders markdown to
                // plain text) and no --stream off (irrelevant for the event stream).
                args.Append("--output-format json ");
            }
            else
            {
                args.Append("--screen-reader ");
            }
            args.Append("--allow-all-tools");
            if (!string.IsNullOrWhiteSpace(model))
            {
                args.Append($" --model {model}");
            }
            if (!streaming && !outputFormatJson)
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

            // Working directory: quella per-chiamata (run degli agenti) vince; in sua assenza,
            // il fallback ambientale per i chiamanti legacy. L'ambiente (RunToken) invece NON
            // ha fallback ambientale — vedi sotto.
            var workingDirectory = invocation?.WorkingDirectory;
            if (string.IsNullOrEmpty(workingDirectory))
                workingDirectory = WorkingDirectory;
            if (!string.IsNullOrEmpty(workingDirectory) && System.IO.Directory.Exists(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
                _logger.LogInformation("[CopilotCliProvider] Working directory set to: {WorkingDir}", workingDirectory);
            }

            // Inietta il RunToken (e le sue claim d'identità) nell'ambiente del figlio. Env preso
            // ESCLUSIVAMENTE dall'invocation per-chiamata: MAI da stato condiviso del provider —
            // è questa l'invariante che rende impossibile lo scambio d'identità tra run concorrenti.
            // psi.Environment è pre-caricato dal padre (UseShellExecute=false): il figlio eredita
            // tutto e noi sovrascriviamo solo queste chiavi. Mai loggato.
            ApplyEnvironmentOverrides(psi.Environment, invocation);

            return psi;
        }

        /// <summary>
        /// Applica gli override d'ambiente di un <see cref="CopilotInvocation"/> al set di
        /// variabili del processo da spawnare. Puro e testabile: l'ambiente proviene SOLO
        /// dall'invocation (mai da stato condiviso), garanzia strutturale dell'isolamento
        /// d'identità tra run concorrenti. Null/vuoto = nessun override.
        /// </summary>
        internal static void ApplyEnvironmentOverrides(IDictionary<string, string> target, CopilotInvocation invocation)
        {
            var overrides = invocation?.EnvironmentOverrides;
            if (target == null || overrides == null) return;
            foreach (var kv in overrides)
            {
                if (string.IsNullOrEmpty(kv.Key)) continue;
                target[kv.Key] = kv.Value ?? string.Empty;
            }
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
