using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using Ad.Tools.Dal.Extensions;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Model discovery for GitHub Copilot CLI.
    /// Reads cached models from UserDB Setting table; falls back to hardcoded list.
    /// Supports dynamic refresh via CLI invocation.
    /// </summary>
    public class CopilotCliModelDiscovery : IModelDiscoveryProvider
    {
        private readonly ILogger<CopilotCliModelDiscovery> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ProviderType ProviderType => ProviderType.CopilotCli;

        private const string DISCOVERED_MODELS_SETTING = "CopilotCli_DiscoveredModels";
        private const string COPILOT_EXECUTABLE = "copilot";
        private const int REFRESH_TIMEOUT_MS = 30000;

        private static readonly List<AiProviderModel> _fallbackModels = new List<AiProviderModel>
        {
            new AiProviderModel
            {
                Id = "claude-sonnet-4",
                Name = "Claude Sonnet 4",
                Description = "Anthropic Claude Sonnet 4 via GitHub Copilot",
                Provider = ProviderType.CopilotCli,
                InputTokenLimit = 128000,
                OutputTokenLimit = 16000,
                IsDeprecated = false,
                CreatedAt = new DateTime(2025, 5, 1),
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    SupportsFunctionCalling = false,
                    SupportsEmbeddings = false,
                    SupportsVision = false,
                    MaxInputTokens = 128000,
                    MaxOutputTokens = 16000
                }
            },
            new AiProviderModel
            {
                Id = "claude-sonnet-4.5",
                Name = "Claude Sonnet 4.5",
                Description = "Anthropic Claude Sonnet 4.5 via GitHub Copilot",
                Provider = ProviderType.CopilotCli,
                InputTokenLimit = 200000,
                OutputTokenLimit = 16000,
                IsDeprecated = false,
                CreatedAt = new DateTime(2025, 10, 1),
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    SupportsFunctionCalling = false,
                    SupportsEmbeddings = false,
                    SupportsVision = false,
                    MaxInputTokens = 200000,
                    MaxOutputTokens = 16000
                }
            },
            new AiProviderModel
            {
                Id = "claude-haiku-4.5",
                Name = "Claude Haiku 4.5",
                Description = "Anthropic Claude Haiku 4.5 (fast) via GitHub Copilot",
                Provider = ProviderType.CopilotCli,
                InputTokenLimit = 200000,
                OutputTokenLimit = 8192,
                IsDeprecated = false,
                CreatedAt = new DateTime(2025, 10, 1),
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    SupportsFunctionCalling = false,
                    SupportsEmbeddings = false,
                    SupportsVision = false,
                    MaxInputTokens = 200000,
                    MaxOutputTokens = 8192
                }
            },
            new AiProviderModel
            {
                Id = "gpt-5",
                Name = "GPT-5",
                Description = "OpenAI GPT-5 via GitHub Copilot",
                Provider = ProviderType.CopilotCli,
                InputTokenLimit = 200000,
                OutputTokenLimit = 16000,
                IsDeprecated = false,
                CreatedAt = new DateTime(2025, 6, 1),
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    SupportsFunctionCalling = false,
                    SupportsEmbeddings = false,
                    SupportsVision = false,
                    MaxInputTokens = 200000,
                    MaxOutputTokens = 16000
                }
            }
        };

        // Known uppercase prefixes for readable name generation
        private static readonly HashSet<string> _uppercasePrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "gpt", "o"
        };

        public CopilotCliModelDiscovery(
            ILogger<CopilotCliModelDiscovery> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public bool SupportsDiscovery() => true;

        public async Task<List<AiProviderModel>> GetModelsAsync()
        {
            try
            {
                var cachedModels = await LoadModelsFromDbAsync();
                if (cachedModels != null && cachedModels.Count > 0)
                {
                    _logger.LogInformation("[CopilotCliModelDiscovery] Returning {Count} cached models from DB", cachedModels.Count);
                    return cachedModels.OrderBy(m => m.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[CopilotCliModelDiscovery] Error reading cached models from DB, using fallback");
            }

            _logger.LogInformation("[CopilotCliModelDiscovery] No cached models found, returning fallback list");
            return _fallbackModels.OrderBy(m => m.Name).ToList();
        }

        /// <summary>
        /// Refreshes the model list by invoking the Copilot CLI and caching the result in UserDB.
        /// </summary>
        public async Task<List<AiProviderModel>> RefreshModelsAsync()
        {
            _logger.LogInformation("[CopilotCliModelDiscovery] Refreshing models from Copilot CLI...");

            var output = await RunCopilotDiscoveryAsync();
            if (string.IsNullOrWhiteSpace(output))
            {
                _logger.LogWarning("[CopilotCliModelDiscovery] Empty output from CLI, returning fallback");
                return _fallbackModels.OrderBy(m => m.Name).ToList();
            }

            var modelIds = ParseModelIds(output);
            if (modelIds.Count == 0)
            {
                _logger.LogWarning("[CopilotCliModelDiscovery] No model IDs parsed from CLI output, returning fallback");
                return _fallbackModels.OrderBy(m => m.Name).ToList();
            }

            _logger.LogInformation("[CopilotCliModelDiscovery] Discovered {Count} models: {Models}",
                modelIds.Count, string.Join(", ", modelIds));

            var models = modelIds.Select(id => new AiProviderModel
            {
                Id = id,
                Name = ModelIdToReadableName(id),
                Description = $"{ModelIdToReadableName(id)} via GitHub Copilot",
                Provider = ProviderType.CopilotCli,
                InputTokenLimit = 128000,
                OutputTokenLimit = 16000,
                IsDeprecated = false,
                CreatedAt = DateTime.UtcNow,
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    SupportsFunctionCalling = false,
                    SupportsEmbeddings = false,
                    SupportsVision = false,
                    MaxInputTokens = 128000,
                    MaxOutputTokens = 16000
                }
            }).ToList();

            // Save to DB
            try
            {
                await SaveModelsToDbAsync(models);
                _logger.LogInformation("[CopilotCliModelDiscovery] Saved {Count} models to DB cache", models.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CopilotCliModelDiscovery] Error saving models to DB cache");
            }

            return models.OrderBy(m => m.Name).ToList();
        }

        #region CLI execution

        private async Task<string> RunCopilotDiscoveryAsync()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = COPILOT_EXECUTABLE,
                    Arguments = "-p \"list all available model IDs, one per line, no descriptions, no headers\" --model claude-haiku-4.5 --no-color --stream off",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var process = new Process { StartInfo = psi };
                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                var completed = process.WaitForExit(REFRESH_TIMEOUT_MS);
                if (!completed)
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException($"Copilot CLI model discovery timed out after {REFRESH_TIMEOUT_MS / 1000}s");
                }

                var output = await outputTask;
                var error = await errorTask;

                _logger.LogDebug("[CopilotCliModelDiscovery] CLI output: {Output}", output);

                if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(error))
                {
                    _logger.LogError("[CopilotCliModelDiscovery] CLI error (exit {Code}): {Error}", process.ExitCode, error);
                    throw new Exception($"Copilot CLI error (exit code {process.ExitCode}): {error}");
                }

                return output;
            }
            catch (TimeoutException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CopilotCliModelDiscovery] Error running Copilot CLI for model discovery");
                throw;
            }
        }

        #endregion

        #region Parsing

        /// <summary>
        /// Parses model IDs from CLI output. Handles backtick-delimited IDs and plain text lines.
        /// Filters out usage metrics lines.
        /// </summary>
        internal static List<string> ParseModelIds(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return new List<string>();

            // Strip everything after "Total usage est:" (usage metrics)
            var usageIdx = output.IndexOf("Total usage est:", StringComparison.OrdinalIgnoreCase);
            if (usageIdx >= 0)
                output = output.Substring(0, usageIdx);

            var modelIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Strategy 1: Extract backtick-delimited model IDs (e.g. `claude-sonnet-4`)
            var backtickRegex = new Regex(@"`([a-zA-Z0-9][a-zA-Z0-9.\-]+)`");
            var matches = backtickRegex.Matches(output);
            foreach (Match match in matches)
            {
                var id = match.Groups[1].Value.Trim();
                if (IsValidModelId(id))
                    modelIds.Add(id);
            }

            // Strategy 2: If no backtick matches, try plain text lines
            if (modelIds.Count == 0)
            {
                var plainRegex = new Regex(@"^[a-z][a-z0-9.\-]+$");
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim().TrimStart('-', '*', ' ');
                    if (plainRegex.IsMatch(trimmed) && IsValidModelId(trimmed))
                        modelIds.Add(trimmed);
                }
            }

            return modelIds.OrderBy(x => x).ToList();
        }

        private static bool IsValidModelId(string id)
        {
            // Filter out common non-model strings
            if (string.IsNullOrWhiteSpace(id) || id.Length < 2 || id.Length > 60)
                return false;

            // Must contain at least one letter
            return id.Any(char.IsLetter);
        }

        /// <summary>
        /// Converts a model ID to a human-readable name.
        /// E.g. "claude-sonnet-4.5" -> "Claude Sonnet 4.5", "gpt-5.2-codex" -> "GPT-5.2 Codex"
        /// </summary>
        internal static string ModelIdToReadableName(string modelId)
        {
            if (string.IsNullOrWhiteSpace(modelId))
                return modelId;

            var segments = modelId.Split('-');
            var result = new List<string>();

            foreach (var segment in segments)
            {
                if (string.IsNullOrEmpty(segment))
                    continue;

                if (_uppercasePrefixes.Contains(segment))
                {
                    result.Add(segment.ToUpperInvariant());
                }
                else
                {
                    // Capitalize first letter, keep rest (preserves version numbers like "4.5")
                    result.Add(char.ToUpperInvariant(segment[0]) + segment.Substring(1));
                }
            }

            return string.Join(" ", result);
        }

        #endregion

        #region DB persistence

        private async Task<List<AiProviderModel>> LoadModelsFromDbAsync()
        {
            return await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    var settingsDal = session.GetDal<Setting>();
                    var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == DISCOVERED_MODELS_SETTING);

                    if (setting == null || string.IsNullOrWhiteSpace(setting.ValueString))
                        return null;

                    var cached = JsonSerializer.Deserialize<List<CachedModelEntry>>(setting.ValueString);
                    if (cached == null || cached.Count == 0)
                        return null;

                    return cached.Select(c => new AiProviderModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = $"{c.Name} via GitHub Copilot",
                        Provider = ProviderType.CopilotCli,
                        InputTokenLimit = 128000,
                        OutputTokenLimit = 16000,
                        IsDeprecated = false,
                        CreatedAt = c.DiscoveredAt,
                        Capabilities = new ProviderCapabilities
                        {
                            SupportsStreaming = true,
                            SupportsFunctionCalling = false,
                            SupportsEmbeddings = false,
                            SupportsVision = false,
                            MaxInputTokens = 128000,
                            MaxOutputTokens = 16000
                        }
                    }).ToList();
                }
            });
        }

        private async Task SaveModelsToDbAsync(List<AiProviderModel> models)
        {
            await Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    var settingsDal = session.GetDal<Setting>();

                    try
                    {
                        session.BeginTransaction();

                        var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == DISCOVERED_MODELS_SETTING);
                        if (setting == null)
                        {
                            setting = new Setting
                            {
                                Name = DISCOVERED_MODELS_SETTING,
                                Description = "Cached Copilot CLI discovered models (JSON)"
                            };
                        }

                        var cached = models.Select(m => new CachedModelEntry
                        {
                            Id = m.Id,
                            Name = m.Name,
                            DiscoveredAt = DateTime.UtcNow
                        }).ToList();

                        setting.ValueString = JsonSerializer.Serialize(cached);
                        settingsDal.Save(setting);
                        session.Commit();
                    }
                    catch
                    {
                        session.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Lightweight DTO for JSON serialization of cached model entries.
        /// </summary>
        private class CachedModelEntry
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime DiscoveredAt { get; set; }
        }

        #endregion
    }
}
