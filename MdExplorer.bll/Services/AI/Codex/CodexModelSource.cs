using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.Codex
{
    /// <summary>
    /// I modelli che Codex offre <b>a questo account</b>: <c>model/list</c> di un <c>codex app-server</c> acceso apposta e
    /// spento subito. Nessuna lista scritta a mano: dipendono dal piano (misurato il 28/11/2026 su un account free:
    /// <c>gpt-6-luna</c> predefinito, <c>gpt-5.6-terra</c>, <c>gpt-5.6-luna</c>).
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    public class CodexModelSource
    {
        private readonly ILogger<CodexModelSource> _logger;

        public CodexModelSource(ILogger<CodexModelSource> logger) => _logger = logger;

        /// <summary>Il modello che Codex dichiara predefinito (<c>isDefault</c>), letto insieme all'elenco; null se nessuno.</summary>
        public string DefaultModelId { get; private set; }

        /// <summary>Chiede l'elenco (tutte le pagine, modelli nascosti esclusi). Fallisce forte, col motivo.</summary>
        public async Task<List<AiProviderModel>> ListModelsAsync(CancellationToken ct = default)
        {
            if (!CodexProcessLauncher.IsResolvable()) throw CodexProcessLauncher.NotInstalled();

            await using var server = new CodexAppServer(_logger, Path.GetTempPath());
            await server.StartAsync(ct).ConfigureAwait(false);

            var result = new List<AiProviderModel>();
            string defaultId = null;
            string cursor = null;
            do
            {
                var page = await server.RequestAsync("model/list",
                    cursor == null ? new { limit = 100 } : (object)new { limit = 100, cursor }, ct).ConfigureAwait(false);
                if (!page.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
                    throw new InvalidOperationException(UiText.Say($"La risposta di model/list non ha l'elenco 'data': {page.GetRawText()}",
                        $"The model/list answer has no 'data' list: {page.GetRawText()}"));

                foreach (var model in data.EnumerateArray())
                {
                    if (model.TryGetProperty("hidden", out var hidden) && hidden.ValueKind == JsonValueKind.True) continue;
                    var id = model.TryGetProperty("id", out var i) ? i.GetString() : null;
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (model.TryGetProperty("isDefault", out var isDefault) && isDefault.ValueKind == JsonValueKind.True) defaultId = id;
                    result.Add(new AiProviderModel
                    {
                        Id = id,
                        Name = model.TryGetProperty("displayName", out var name) && name.ValueKind == JsonValueKind.String ? name.GetString() : id,
                        Description = model.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() : null,
                        Provider = ProviderType.Codex,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
                cursor = page.TryGetProperty("nextCursor", out var next) && next.ValueKind == JsonValueKind.String ? next.GetString() : null;
            } while (cursor != null);

            DefaultModelId = defaultId;
            _logger.LogInformation("[Codex] {Count} modelli dichiarati (predefinito {Default})", result.Count, defaultId ?? "(nessuno)");
            return result;
        }
    }
}
