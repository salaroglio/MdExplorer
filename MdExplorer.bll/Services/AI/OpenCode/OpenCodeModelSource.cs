using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.OpenCode
{
    /// <summary>
    /// I modelli che opencode offre <b>su questa installazione</b>: quelli dei provider
    /// collegati, letti da <c>GET /config/providers</c> del server.
    /// <para>
    /// Nessuna lista scritta a mano: quali modelli esistano dipende da come l'utente ha
    /// configurato opencode (misurato il 21/09/2026 su un'installazione senza credenziali:
    /// provider <c>opencode</c>, 7 modelli gratuiti, default <c>big-pickle</c>).
    /// </para>
    /// <para>
    /// L'identificativo è <c>provider/modello</c> — la coppia che va poi in ogni messaggio.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F5.</para>
    /// </summary>
    public class OpenCodeModelSource
    {
        private readonly ILogger<OpenCodeModelSource> _logger;
        private readonly OpenCodeServer _server;

        public OpenCodeModelSource(ILogger<OpenCodeModelSource> logger, OpenCodeServer server)
        {
            _logger = logger;
            _server = server;
        }

        /// <summary>
        /// Il modello di default dichiarato dal server per un provider, nella forma
        /// <c>provider/modello</c>, o <c>null</c>. Letto insieme all'elenco.
        /// </summary>
        public string DefaultModelId { get; private set; }

        /// <summary>
        /// Chiede l'elenco al server, accendendolo se serve.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Il server risponde ma non con un elenco leggibile: si dice, invece di restituire una
        /// lista vuota che sembrerebbe «nessun modello disponibile».
        /// </exception>
        public async Task<List<AiProviderModel>> ListModelsAsync(CancellationToken ct = default)
        {
            using var client = await _server.CreateClientAsync(ct).ConfigureAwait(false);
            using var res = await client.GetAsync("/config/providers", ct).ConfigureAwait(false);
            var payload = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"opencode non ha restituito i provider ({(int)res.StatusCode}): {Truncate(payload, 300)}");
            }

            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            if (!root.TryGetProperty("providers", out var providers) || providers.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException(
                    $"La risposta di /config/providers non ha l'elenco 'providers': {Truncate(payload, 300)}");
            }

            var result = new List<AiProviderModel>();
            foreach (var provider in providers.EnumerateArray())
            {
                var providerId = provider.TryGetProperty("id", out var pid) ? pid.GetString() : null;
                if (string.IsNullOrWhiteSpace(providerId)) continue;
                var providerName = provider.TryGetProperty("name", out var pn) ? pn.GetString() : providerId;

                if (!provider.TryGetProperty("models", out var models) || models.ValueKind != JsonValueKind.Object) continue;
                foreach (var model in models.EnumerateObject())
                {
                    var modelName = model.Value.TryGetProperty("name", out var mn) ? mn.GetString() : model.Name;
                    result.Add(new AiProviderModel
                    {
                        Id = $"{providerId}/{model.Name}",
                        Name = modelName,
                        // La descrizione dice da quale provider arriva: con più provider
                        // collegati due modelli possono chiamarsi allo stesso modo.
                        Description = providerName,
                        Provider = ProviderType.OpenCode,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
            }

            DefaultModelId = ReadDefault(root);
            _logger.LogInformation("[opencode] {Count} modelli dichiarati dal server (default {Default})",
                result.Count, DefaultModelId ?? "(nessuno)");
            return result.OrderBy(m => m.Id, StringComparer.OrdinalIgnoreCase).ToList();
        }

        /// <summary>
        /// <c>"default": { "opencode": "big-pickle" }</c> → <c>opencode/big-pickle</c>. Con più
        /// provider si prende il primo: il server non dice quale preferisce fra loro, e
        /// inventare una preferenza qui sarebbe peggio che non averne.
        /// </summary>
        private static string ReadDefault(JsonElement root)
        {
            if (!root.TryGetProperty("default", out var def) || def.ValueKind != JsonValueKind.Object) return null;
            foreach (var entry in def.EnumerateObject())
            {
                var value = entry.Value.GetString();
                if (!string.IsNullOrWhiteSpace(value)) return $"{entry.Name}/{value}";
            }
            return null;
        }

        private static string Truncate(string s, int max)
            => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";
    }
}
