using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.ClaudeCode
{
    /// <summary>
    /// I modelli che Claude Code offre a QUESTO account, chiesti al CLI e non scritti nel codice.
    ///
    /// <para><b>Come.</b> Sullo stream-json il CLI accetta un <c>control_request</c> di tipo
    /// <c>initialize</c>, e la risposta contiene <c>models</c>: <c>value</c> (l'id da passare a
    /// <c>--model</c> o <c>set_model</c>), <c>displayName</c>, <c>description</c>, <c>resolvedModel</c>.
    /// Verificato il 13/09/2026 su claude 2.1.270: 0,6 s, arriva solo la <c>control_response</c>,
    /// nessun turno e quindi nessun token. È la stessa domanda che l'Agent SDK usa per
    /// <c>supportedModels()</c>.</para>
    ///
    /// <para><b>Perché non la lista di prima.</b> Era sbagliata già il giorno della prova: mancava
    /// <c>default</c>, e l'Opus dell'account è <c>opus[1m]</c>, non <c>opus</c>. Una lista scritta a
    /// mano sbaglia in silenzio a ogni rilascio.</para>
    ///
    /// <para>Nessuna ripiego: CLI assente, risposta d'errore, risposta senza modelli o nessuna
    /// risposta entro <see cref="Timeout"/> sono eccezioni col motivo dentro.</para>
    /// </summary>
    public class ClaudeCodeModelSource
    {
        /// <summary>Solo il canale bidirezionale: niente modello, niente permessi, niente MCP — non si apre un turno.</summary>
        public const string Arguments = "-p --input-format stream-json --output-format stream-json --verbose";

        public const string RequestId = "mde-list-models";

        /// <summary>Contesto dichiarato dal suffisso che il CLI stesso mette all'id (<c>opus[1m]</c>).</summary>
        public const int OneMillionContext = 1_000_000;
        public const int StandardContext = 200_000;

        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

        private readonly ILogger<ClaudeCodeModelSource> _logger;

        public ClaudeCodeModelSource(ILogger<ClaudeCodeModelSource> logger)
        {
            _logger = logger;
        }

        public async Task<List<AiProviderModel>> ListModelsAsync(CancellationToken ct = default)
        {
            // Fallisce forte, col messaggio d'installazione, se `claude` non è nel PATH.
            var psi = ClaudeCodeProcessLauncher.BuildStartInfo(Arguments);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;
            psi.StandardInputEncoding = AiCliEncoding.PromptStdin;
            // Fuori da ogni progetto: l'elenco dipende dall'account, non dalla cartella, e così il CLI
            // non carica CLAUDE.md, skill e impostazioni di un repository qualsiasi.
            psi.WorkingDirectory = Path.GetTempPath();

            var watch = Stopwatch.StartNew();
            using var process = new Process { StartInfo = psi };
            if (!process.Start())
            {
                throw new InvalidOperationException($"Avvio di `claude {Arguments}` fallito");
            }

            var stderr = new StringBuilder();
            var stderrTask = Task.Run(async () =>
            {
                try
                {
                    string errLine;
                    while ((errLine = await process.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        lock (stderr) stderr.AppendLine(errLine);
                    }
                }
                catch { /* il processo è stato chiuso */ }
            });

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(Timeout);
            try
            {
                await process.StandardInput.WriteLineAsync(BuildInitializeRequest()).ConfigureAwait(false);
                await process.StandardInput.FlushAsync().ConfigureAwait(false);

                string line;
                while ((line = await process.StandardOutput.ReadLineAsync(timeout.Token).ConfigureAwait(false)) != null)
                {
                    if (!IsControlResponseFor(line, RequestId)) continue;

                    var models = ParseInitializeResponse(line);
                    _logger.LogInformation("[ClaudeCodeModelSource] {Count} modelli dal CLI in {Ms} ms: {Ids}",
                        models.Count, watch.ElapsedMilliseconds, string.Join(", ", models.ConvertAll(m => m.Id)));
                    return models;
                }

                process.WaitForExit(2000);
                throw new InvalidOperationException(
                    "Claude Code si è chiuso senza rispondere alla richiesta dei modelli" +
                    (process.HasExited ? $" (exit {process.ExitCode})" : "") + StderrSuffix(stderr));
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"Claude Code non ha elencato i modelli entro {Timeout.TotalSeconds:0} secondi{StderrSuffix(stderr)}");
            }
            finally
            {
                try { process.StandardInput.Close(); } catch { /* già chiuso */ }
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { /* già uscito */ }
                await Task.WhenAny(stderrTask, Task.Delay(1000)).ConfigureAwait(false);
            }
        }

        /// <summary>La riga NDJSON che chiede <c>initialize</c>.</summary>
        public static string BuildInitializeRequest() => JsonSerializer.Serialize(new
        {
            type = "control_request",
            request_id = RequestId,
            request = new { subtype = "initialize" },
        });

        /// <summary>
        /// <c>true</c> solo per la <c>control_response</c> alla nostra richiesta: prima possono arrivare altre
        /// righe, e una riga che non è JSON non è un errore del protocollo, è rumore.
        /// </summary>
        public static bool IsControlResponseFor(string line, string requestId)
        {
            if (string.IsNullOrWhiteSpace(line)) return false;
            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                return root.ValueKind == JsonValueKind.Object
                    && root.TryGetProperty("type", out var type) && type.GetString() == "control_response"
                    && root.TryGetProperty("response", out var response) && response.ValueKind == JsonValueKind.Object
                    && response.TryGetProperty("request_id", out var id) && id.GetString() == requestId;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        /// <summary>I modelli di una <c>control_response</c> a <c>initialize</c>, nell'ordine del CLI.</summary>
        public static List<AiProviderModel> ParseInitializeResponse(string line)
        {
            using var doc = JsonDocument.Parse(line);
            var response = doc.RootElement.GetProperty("response");

            if (response.TryGetProperty("subtype", out var subtype) && subtype.GetString() == "error")
            {
                var error = response.TryGetProperty("error", out var e) ? e.GetString() : "(nessun motivo)";
                throw new InvalidOperationException($"Claude Code ha rifiutato la richiesta dei modelli: {error}");
            }

            if (!response.TryGetProperty("response", out var inner) || inner.ValueKind != JsonValueKind.Object
                || !inner.TryGetProperty("models", out var models) || models.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException(
                    "Claude Code ha risposto a initialize senza l'elenco dei modelli: " +
                    "questa versione del CLI non lo espone. Aggiorna Claude Code (`claude update`).");
            }

            var result = new List<AiProviderModel>();
            foreach (var m in models.EnumerateArray())
            {
                var value = Text(m, "value");
                // Senza id non c'è niente da passare a --model: la voce non è selezionabile.
                if (string.IsNullOrWhiteSpace(value)) continue;

                var resolved = Text(m, "resolvedModel");
                var description = Text(m, "description");
                if (!string.IsNullOrWhiteSpace(resolved))
                {
                    description = string.IsNullOrWhiteSpace(description) ? resolved : $"{description} ({resolved})";
                }

                var context = value.Contains("[1m]") || (resolved?.Contains("[1m]") ?? false)
                    ? OneMillionContext
                    : StandardContext;

                result.Add(new AiProviderModel
                {
                    Id = value,
                    Name = string.IsNullOrWhiteSpace(Text(m, "displayName")) ? value : Text(m, "displayName"),
                    Description = description,
                    Provider = ProviderType.ClaudeCode,
                    InputTokenLimit = context,
                    IsDeprecated = false,
                    CreatedAt = DateTime.UtcNow,
                    Capabilities = new ProviderCapabilities
                    {
                        SupportsStreaming = true,
                        // I tool li porta e li esegue Claude Code: non sono i tool di MDE.
                        SupportsFunctionCalling = false,
                        SupportsEmbeddings = false,
                        SupportsVision = true,
                        MaxInputTokens = context,
                    },
                });
            }

            if (result.Count == 0)
            {
                throw new InvalidOperationException("Claude Code ha risposto con un elenco di modelli vuoto.");
            }
            return result;
        }

        private static string Text(JsonElement element, string property) =>
            element.TryGetProperty(property, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

        private static string StderrSuffix(StringBuilder stderr)
        {
            string text;
            lock (stderr) text = stderr.ToString().Trim();
            return string.IsNullOrEmpty(text) ? "." : $". stderr: {(text.Length > 500 ? text.Substring(0, 500) : text)}";
        }
    }
}
