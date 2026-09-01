using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.AI.ClaudeCode;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Elenco dei modelli selezionabili per Claude Code CLI.
    ///
    /// <para><b>Perché una lista dichiarata e non un discovery.</b> Il CLI non espone un
    /// comando per elencare i modelli disponibili sull'abbonamento: inventarne uno
    /// significherebbe parsare output pensato per gli umani e sbagliare in silenzio alla prima
    /// riformulazione. <see cref="SupportsDiscovery"/> risponde quindi <c>false</c>, che è la
    /// verità, invece di far finta di interrogare qualcosa.</para>
    ///
    /// <para><b>Perché gli alias.</b> Gli id qui sono gli alias del CLI (<c>opus</c>,
    /// <c>sonnet</c>, <c>haiku</c>, <c>fable</c>), non i nomi pieni con la data: l'alias punta
    /// sempre all'ultima versione di quella famiglia e non invecchia a ogni rilascio. Il nome
    /// pieno resta accettato dal CLI per chi lo scrive a mano.</para>
    ///
    /// <para>⚠️ Da verificare (Q4 del piano): su Copilot un <c>--model</c> non disponibile viene
    /// <b>ignorato in silenzio</b> in modalità ACP. Su Claude Code non è stato provato; se si
    /// comportasse allo stesso modo, il modello davvero in uso è quello che
    /// <see cref="ClaudeCodeSession.EffectiveModel"/> legge dall'<c>init</c> — è lì che si
    /// vedrebbe la differenza tra ciò che si è chiesto e ciò che si è ottenuto.</para>
    /// </summary>
    public class ClaudeCodeModelDiscovery : IModelDiscoveryProvider
    {
        private readonly ILogger<ClaudeCodeModelDiscovery> _logger;

        public ProviderType ProviderType => ProviderType.ClaudeCode;

        public ClaudeCodeModelDiscovery(ILogger<ClaudeCodeModelDiscovery> logger)
        {
            _logger = logger;
        }

        /// <summary>Il CLI non offre un elenco interrogabile: qui non si scopre nulla, si dichiara.</summary>
        public bool SupportsDiscovery() => false;

        public Task<List<AiProviderModel>> GetModelsAsync()
        {
            if (!ClaudeCodeProcessLauncher.IsResolvable())
            {
                _logger.LogInformation(
                    "[ClaudeCodeModelDiscovery] `claude` non è nel PATH: nessun modello da offrire");
                return Task.FromResult(new List<AiProviderModel>());
            }

            return Task.FromResult(new List<AiProviderModel>(_models));
        }

        private static ProviderCapabilities Caps(int maxInput, int maxOutput) => new ProviderCapabilities
        {
            SupportsStreaming = true,
            // I tool li porta e li esegue Claude Code: non sono i tool di MDE.
            SupportsFunctionCalling = false,
            SupportsEmbeddings = false,
            SupportsVision = true,
            MaxInputTokens = maxInput,
            MaxOutputTokens = maxOutput
        };

        private static readonly List<AiProviderModel> _models = new List<AiProviderModel>
        {
            new AiProviderModel
            {
                Id = "sonnet",
                Name = "Claude Sonnet (ultimo)",
                Description = "Equilibrio fra qualità e velocità. Alias: punta sempre all'ultimo Sonnet.",
                Provider = ProviderType.ClaudeCode,
                InputTokenLimit = 200000,
                OutputTokenLimit = 64000,
                IsDeprecated = false,
                CreatedAt = new DateTime(2026, 1, 1),
                Capabilities = Caps(200000, 64000)
            },
            new AiProviderModel
            {
                Id = "opus",
                Name = "Claude Opus (ultimo)",
                Description = "Il più capace, il più caro. Alias: punta sempre all'ultimo Opus.",
                Provider = ProviderType.ClaudeCode,
                InputTokenLimit = 200000,
                OutputTokenLimit = 64000,
                IsDeprecated = false,
                CreatedAt = new DateTime(2026, 1, 1),
                Capabilities = Caps(200000, 64000)
            },
            new AiProviderModel
            {
                Id = "haiku",
                Name = "Claude Haiku (ultimo)",
                Description = "Il più rapido ed economico: adatto ai turni brevi e alle bozze.",
                Provider = ProviderType.ClaudeCode,
                InputTokenLimit = 200000,
                OutputTokenLimit = 32000,
                IsDeprecated = false,
                CreatedAt = new DateTime(2026, 1, 1),
                Capabilities = Caps(200000, 32000)
            },
            new AiProviderModel
            {
                Id = "fable",
                Name = "Claude Fable (ultimo)",
                Description = "Alias della famiglia Fable.",
                Provider = ProviderType.ClaudeCode,
                InputTokenLimit = 200000,
                OutputTokenLimit = 64000,
                IsDeprecated = false,
                CreatedAt = new DateTime(2026, 1, 1),
                Capabilities = Caps(200000, 64000)
            }
        };
    }
}
