using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MdExplorer.Abstractions.Exceptions;
using MdExplorer.Abstractions.Models;
using MdExplorer.Abstractions.Services;

namespace MdExplorer.AI.Stubs.Services
{
    /// <summary>
    /// Implementazione stub che mostra "Premium Required".
    /// Sostituita dall'implementazione reale se submodule premium è presente.
    /// </summary>
    public class AiChatServiceStub : IAiChatService
    {
        private readonly ILogger<AiChatServiceStub> _logger;

        public AiChatServiceStub(ILogger<AiChatServiceStub> logger)
        {
            _logger = logger;
            _logger.LogInformation("ℹ️  AI Chat Service stub initialized - Premium features not available");
        }

        public Task<bool> IsLicensedAsync() => Task.FromResult(false);

        public Task<LicenseStatus> GetLicenseStatusAsync()
            => Task.FromResult(new LicenseStatus
            {
                IsValid = false,
                Type = LicenseType.Free,
                Message = "AI features require MdExplorer AI Premium addon.\n" +
                         "Visit https://mdexplorer.net/ai-premium for more information.",
                EnabledFeatures = Array.Empty<string>()
            });

        public Task<string> ChatAsync(string prompt)
        {
            _logger.LogWarning("Chat requested but AI Premium not available");
            throw new LicenseRequiredException();
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            string prompt,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            _logger.LogWarning("Stream chat requested but AI Premium not available");

            yield return "⚠️ **AI Premium Addon Not Installed**\n\n";
            await Task.Delay(100, ct);

            yield return "To use AI features, you need **MdExplorer AI Premium**.\n\n";
            await Task.Delay(100, ct);

            yield return "**Features included:**\n";
            await Task.Delay(50, ct);

            yield return "- 🤖 Local AI models (no cloud required)\n";
            await Task.Delay(50, ct);

            yield return "- 📄 Document RAG for contextual answers\n";
            await Task.Delay(50, ct);

            yield return "- ⚡ GPU acceleration support\n";
            await Task.Delay(50, ct);

            yield return "- 🛠️ Tool system and agents (coming soon)\n\n";
            await Task.Delay(100, ct);

            yield return "Visit [mdexplorer.net/ai-premium](https://mdexplorer.net/ai-premium) to learn more.";
        }

        public bool IsModelLoaded()
        {
            return false;
        }

        public Task<bool> LoadModelAsync(string modelPath, string modelId = null)
        {
            _logger.LogWarning($"Model load requested for {modelPath} but AI Premium not available");
            throw new LicenseRequiredException();
        }

        public string GetCurrentModelName() => "None (Premium Required)";

        public string GetCurrentModelId() => "None";

        public Task SetSystemPromptAsync(string systemPrompt)
        {
            throw new LicenseRequiredException();
        }

        public string GetSystemPrompt() => string.Empty;

        public GpuInfo GetGpuInfo() => new GpuInfo
        {
            Status = "Premium Required",
            Name = "N/A",
            FormattedMemory = "N/A",
            CudaVersion = 0,
            DriverVersion = "N/A"
        };

        public bool IsGpuEnabled() => false;

        public int GetGpuLayerCount() => 0;
    }
}
