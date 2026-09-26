using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Copilot;
using MdExplorer.Abstractions.Models.AI;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Elenco dei modelli di GitHub Copilot, chiesto al CLI con l'SDK ufficiale
    /// (<c>GitHub.Copilot.SDK</c>, RPC <c>models.list</c>).
    ///
    /// <para>
    /// Sostituisce il metodo precedente, che <b>chiedeva all'LLM a parole</b> di elencare
    /// i propri modelli («list all available model IDs, one per line…») e poi faceva il
    /// parsing della prosa con tre espressioni regolari. Quel giro aveva tre difetti che
    /// si sommavano: costava un'inferenza vera (crediti e secondi), passava
    /// <c>--model claude-haiku-4.5</c> — un id che su molte installazioni non esiste, e
    /// quindi falliva prima ancora di cominciare — e quando falliva restituiva una lista
    /// di modelli <b>cablata nel codice</b>. Il risultato era che l'utente poteva
    /// selezionare un modello inesistente e scoprirlo solo alla prima domanda.
    /// </para>
    ///
    /// <para>
    /// Qui invece la domanda è tipizzata e non passa dal modello: nessuna inferenza,
    /// nessun credito, nessun parsing. Misurato il 04/09/2026 su Copilot CLI 1.0.82:
    /// avvio ~500 ms, <c>ListModelsAsync</c> ~1,4 s.
    /// </para>
    /// </summary>
    public class CopilotSdkModelSource
    {
        private readonly ILogger<CopilotSdkModelSource> _logger;

        /// <summary>
        /// Il CLI risponde in un paio di secondi; oltre questo qualcosa è bloccato e
        /// vale più la pena dirlo che continuare ad aspettare.
        /// </summary>
        private const int TimeoutMs = 30000;

        public CopilotSdkModelSource(ILogger<CopilotSdkModelSource> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Modelli realmente disponibili per l'utente collegato.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Se il CLI non risponde. Volutamente un'eccezione e non una lista vuota o di
        /// ripiego: chi chiama deve poter dire all'utente <i>perché</i> non ci sono modelli,
        /// invece di mostrargliene di inventati.
        /// </exception>
        public async Task<List<AiProviderModel>> ListModelsAsync(CancellationToken ct = default)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeoutMs);

            try
            {
                // Si usa il CLI installato dall'utente, non quello incluso nel pacchetto
                // NuGet dell'SDK. Due motivi, entrambi concreti: il CLI dell'utente porta
                // la sua autenticazione (quello incluso chiederebbe un altro login), e il
                // download automatico del pacchetto pesa oltre 300 MB nell'output di build
                // — disattivato con <CopilotSkipCliDownload>true</CopilotSkipCliDownload>.
                var (cliPath, prefixArgs) = CopilotAcp.CopilotProcessLauncher.ResolveStdioTarget();

                await using var client = new CopilotClient(new CopilotClientOptions
                {
                    Connection = RuntimeConnection.ForStdio(cliPath, prefixArgs),
                    UseLoggedInUser = true,
                });

                await client.StartAsync(timeout.Token).ConfigureAwait(false);
                var models = await client.ListModelsAsync(timeout.Token).ConfigureAwait(false);

                var mapped = models.Select(Map).ToList();
                _logger.LogInformation("[CopilotSdkModelSource] {Count} modelli: {Ids}",
                    mapped.Count, string.Join(", ", mapped.Select(m => m.Id)));

                await client.StopAsync().ConfigureAwait(false);
                return mapped;
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new InvalidOperationException(
                    $"Copilot CLI non ha risposto entro {TimeoutMs / 1000} secondi alla richiesta dei modelli.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Non è stato possibile leggere i modelli da Copilot CLI: {ex.Message}", ex);
            }
        }

        private static AiProviderModel Map(GitHub.Copilot.ModelInfo m)
        {
            // I limiti arrivano dal CLI. Quando mancano restano a zero invece di essere
            // rimpiazzati da un numero inventato: uno zero si riconosce come "non lo so",
            // un 128000 finto no.
            var maxContext = m.Capabilities?.Limits?.MaxContextWindowTokens ?? 0;
            var maxPrompt = m.Capabilities?.Limits?.MaxPromptTokens ?? maxContext;

            return new AiProviderModel
            {
                Id = m.Id,
                Name = string.IsNullOrWhiteSpace(m.Name) ? m.Id : m.Name,
                Description = $"{(string.IsNullOrWhiteSpace(m.Name) ? m.Id : m.Name)} via GitHub Copilot",
                Provider = ProviderType.CopilotCli,
                InputTokenLimit = maxPrompt,
                OutputTokenLimit = 0,
                IsDeprecated = false,
                CreatedAt = DateTime.UtcNow,
                Capabilities = new ProviderCapabilities
                {
                    SupportsStreaming = true,
                    // Il CLI non dichiara il tool calling per modello: lo gestisce lui,
                    // ed è sempre attivo. Non lo si deduce da un campo che non esiste.
                    SupportsFunctionCalling = true,
                    SupportsEmbeddings = false,
                    SupportsVision = m.Capabilities?.Supports?.Vision ?? false,
                    MaxInputTokens = maxPrompt,
                    // Il CLI non espone un limite di output per modello: resta zero,
                    // che si legge come "non dichiarato".
                    MaxOutputTokens = 0,
                }
            };
        }
    }
}
