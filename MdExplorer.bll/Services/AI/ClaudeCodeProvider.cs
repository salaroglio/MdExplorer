using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.bll.Models.AI;
using MdExplorer.Features.Services.AI.ClaudeCode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Provider per <b>Claude Code CLI</b> (<c>claude</c>), affiancato a quello di Copilot e
    /// del tutto indipendente da esso.
    ///
    /// <para>La chat interattiva <b>non passa da qui</b>: quella usa la sessione persistente
    /// (<see cref="ClaudeCodeSessionPool"/> → <see cref="ClaudeCodeSession"/>), che conserva la
    /// memoria conversazionale tra un turno e l'altro. Questa classe copre i chiamanti
    /// "una domanda, una risposta" che il resto di MDE fa passare per <see cref="IAiProvider"/>
    /// (messaggio di commit, riassunti, ...) e la presenza del provider nel picker.</para>
    ///
    /// <para>Nessuna API key: Claude Code gira sull'abbonamento di chi ha fatto login col CLI
    /// (verificato: <c>apiKeySource: "none"</c>). MDE non custodisce segreti per questo provider.</para>
    /// </summary>
    public class ClaudeCodeProvider : IAiProvider
    {
        private readonly ILogger<ClaudeCodeProvider> _logger;
        private readonly IServiceProvider _serviceProvider;
        private string _systemPrompt;

        private const string SYSTEM_PROMPT_SETTING = "ClaudeCode_SystemPrompt";
        private const int ONESHOT_TIMEOUT_MS = 300000; // 5 minuti
        private static readonly TimeSpan AvailabilityCacheDuration = TimeSpan.FromMinutes(5);

        private bool? _cachedAvailability;
        private DateTime _availabilityCacheExpiry = DateTime.MinValue;

        /// <summary>
        /// Working directory per i chiamanti "una domanda, una risposta". Claude Code va
        /// lanciato dentro il progetto: senza, leggerebbe il filesystem sbagliato.
        /// </summary>
        public string WorkingDirectory { get; set; }

        public ClaudeCodeProvider(ILogger<ClaudeCodeProvider> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public string GetName() => "Claude Code CLI";

        public ProviderType GetProviderType() => ProviderType.ClaudeCode;

        /// <summary>
        /// Controllo di installazione deterministico: scansione del PATH, nessun processo
        /// lanciato. Non dice nulla sull'autenticazione — quella emerge al primo turno,
        /// perché il CLI resta muto finché non riceve un messaggio.
        /// </summary>
        public bool IsAvailable()
        {
            if (_cachedAvailability.HasValue && DateTime.UtcNow < _availabilityCacheExpiry)
            {
                return _cachedAvailability.Value;
            }

            var resolvable = ClaudeCodeProcessLauncher.IsResolvable();
            _cachedAvailability = resolvable;
            _availabilityCacheExpiry = DateTime.UtcNow + AvailabilityCacheDuration;
            if (!resolvable)
            {
                _logger.LogInformation(
                    "[ClaudeCodeProvider.IsAvailable] `claude` non trovato nel PATH — Claude Code CLI non installato");
            }
            return resolvable;
        }

        /// <summary>Come <see cref="IsAvailable"/> ma senza mai calcolare: solo la cache calda.</summary>
        public bool? TryGetCachedAvailability() =>
            _cachedAvailability.HasValue && DateTime.UtcNow < _availabilityCacheExpiry
                ? _cachedAvailability.Value
                : (bool?)null;

        public ProviderCapabilities GetCapabilities()
        {
            return new ProviderCapabilities
            {
                SupportsStreaming = true,
                // Function calling nel senso di MDE (tool definiti da noi ed eseguiti dal
                // ToolExecutor) NON è supportato: Claude Code porta i propri tool e li esegue
                // da sé. Dichiararlo true farebbe passare a questo provider una lista di tool
                // che verrebbe ignorata.
                SupportsFunctionCalling = false,
                SupportsEmbeddings = false,
                SupportsVision = false,
                MaxInputTokens = 200000,
                MaxOutputTokens = 64000,
                AvailableModels = null
            };
        }

        public async Task<string> ChatAsync(string prompt, string modelId = null, CancellationToken ct = default)
        {
            if (!IsAvailable())
            {
                throw new InvalidOperationException(
                    "Claude Code CLI non disponibile: `claude` non è nel PATH. " +
                    "Installalo con `npm i -g @anthropic-ai/claude-code` e fai il login.");
            }

            var args = new StringBuilder("-p --output-format json --permission-mode dontAsk");
            if (!string.IsNullOrWhiteSpace(modelId) && modelId != "auto")
            {
                args.Append(" --model ").Append(modelId.IndexOf(' ') >= 0 ? "\"" + modelId + "\"" : modelId);
            }

            var psi = ClaudeCodeProcessLauncher.BuildStartInfo(args.ToString());
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.StandardOutputEncoding = Encoding.UTF8;
            psi.StandardErrorEncoding = Encoding.UTF8;
            if (!string.IsNullOrEmpty(WorkingDirectory) && System.IO.Directory.Exists(WorkingDirectory))
            {
                psi.WorkingDirectory = WorkingDirectory;
            }

            using var process = new Process { StartInfo = psi };
            if (!process.Start())
            {
                throw new InvalidOperationException("Avvio di `claude -p` fallito");
            }

            // Il prompt viaggia su stdin, non sulla riga di comando: nessun limite di
            // lunghezza da aggirare e nessun problema di quoting.
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            await process.StandardInput.WriteAsync(prompt.AsMemory(), ct).ConfigureAwait(false);
            process.StandardInput.Close();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(ONESHOT_TIMEOUT_MS);
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                throw new TimeoutException($"Claude Code non ha risposto entro {ONESHOT_TIMEOUT_MS / 1000}s");
            }

            var stdout = await stdoutTask.ConfigureAwait(false);
            var stderr = await stderrTask.ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Claude Code è uscito con codice {process.ExitCode}. " +
                    (string.IsNullOrWhiteSpace(stderr) ? "(stderr vuoto)" : stderr.Trim()));
            }

            return ExtractResultText(stdout, stderr);
        }

        /// <summary>
        /// Estrae il testo dal JSON di <c>--output-format json</c>. Fallisce forte se il JSON
        /// non ha la forma attesa: restituire l'output grezzo "così almeno qualcosa arriva"
        /// significherebbe mostrare in chat un blob di telemetria spacciato per risposta.
        /// </summary>
        private static string ExtractResultText(string stdout, string stderr)
        {
            if (string.IsNullOrWhiteSpace(stdout))
            {
                throw new InvalidOperationException(
                    "Claude Code non ha prodotto output. " +
                    (string.IsNullOrWhiteSpace(stderr) ? "(stderr vuoto)" : stderr.Trim()));
            }

            using var doc = JsonDocument.Parse(stdout);
            var root = doc.RootElement;

            if (root.TryGetProperty("is_error", out var isError) && isError.ValueKind == JsonValueKind.True)
            {
                var subtype = root.TryGetProperty("subtype", out var st) ? st.GetString() : "sconosciuto";
                var detail = root.TryGetProperty("result", out var r) ? r.GetString() : null;
                throw new InvalidOperationException($"Claude Code ha risposto in errore ({subtype}): {detail}");
            }

            if (!root.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    "La risposta di Claude Code non contiene il campo 'result': forma del JSON inattesa.");
            }

            return result.GetString();
        }

        /// <summary>
        /// Streaming "una domanda, una risposta": apre una sessione usa-e-getta, la consuma e
        /// la chiude. Chi vuole conservare la conversazione tra i turni deve passare dal
        /// <see cref="ClaudeCodeSessionPool"/>, non da qui.
        /// </summary>
        public async IAsyncEnumerable<string> StreamChatAsync(
            string prompt,
            string modelId = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (!IsAvailable())
            {
                throw new InvalidOperationException(
                    "Claude Code CLI non disponibile: `claude` non è nel PATH.");
            }

            var session = new ClaudeCodeSession(_logger, WorkingDirectory, modelId);
            try
            {
                await session.StartAsync(ct).ConfigureAwait(false);
                await foreach (var chunk in session.PromptAsync(prompt, ct).ConfigureAwait(false))
                {
                    // Solo il testo visibile: ragionamento e attività sui tool hanno canali
                    // propri e qui non avrebbero dove andare.
                    if (chunk.Kind == ClaudeCodeChunk.KindMessage)
                    {
                        yield return chunk.Text;
                    }
                }
            }
            finally
            {
                await session.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// ⚠️ I <paramref name="tools"/> di MDE e il <paramref name="toolExecutor"/> <b>non</b>
        /// vengono usati: Claude Code porta i propri tool e li esegue in proprio, quindi non
        /// esiste un punto dove innestare i nostri. Il metodo esiste perché fa parte di
        /// <see cref="IAiProvider"/>, e si comporta come <see cref="ChatAsync"/> con il
        /// contesto (system prompt, documento corrente, storico) impacchettato nel prompt.
        /// <see cref="GetCapabilities"/> dichiara <c>SupportsFunctionCalling = false</c>
        /// proprio per questo.
        /// </summary>
        public async Task<string> ChatWithToolsAsync(
            string prompt,
            List<object> tools,
            Func<string, dynamic, Task<object>> toolExecutor,
            string modelId = null,
            string currentDocumentPath = null,
            List<object> conversationHistory = null,
            CancellationToken ct = default)
        {
            if (tools != null && tools.Count > 0)
            {
                _logger.LogInformation(
                    "[ClaudeCodeProvider.ChatWithToolsAsync] {Count} tool di MDE ignorati: Claude Code usa i propri.",
                    tools.Count);
            }

            if (string.IsNullOrEmpty(_systemPrompt))
            {
                _systemPrompt = await GetSystemPromptAsync();
            }

            var composite = new StringBuilder();
            if (!string.IsNullOrEmpty(_systemPrompt))
            {
                composite.AppendLine("Istruzioni di sistema:");
                composite.AppendLine(_systemPrompt);
                composite.AppendLine();
            }
            if (!string.IsNullOrEmpty(currentDocumentPath))
            {
                composite.AppendLine($"Documento corrente: {currentDocumentPath}");
                composite.AppendLine();
            }
            if (conversationHistory != null && conversationHistory.Count > 0)
            {
                composite.AppendLine("Conversazione precedente:");
                foreach (var msgObj in conversationHistory)
                {
                    if (msgObj is ConversationMessage msg)
                    {
                        composite.AppendLine($"{(msg.Role == "model" ? "Assistant" : "User")}: {msg.Content}");
                    }
                }
                composite.AppendLine();
            }
            composite.AppendLine("Domanda corrente:");
            composite.AppendLine(prompt);

            return await ChatAsync(composite.ToString(), modelId, ct);
        }

        public async Task SetSystemPromptAsync(string systemPrompt)
        {
            await Task.Run(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                try
                {
                    var settingsDal = session.GetDal<Setting>();
                    session.BeginTransaction();

                    var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == SYSTEM_PROMPT_SETTING)
                        ?? new Setting
                        {
                            Name = SYSTEM_PROMPT_SETTING,
                            Description = "System prompt per Claude Code CLI"
                        };

                    setting.ValueString = systemPrompt;
                    settingsDal.Save(setting);
                    session.Commit();
                    _systemPrompt = systemPrompt;
                    _logger.LogInformation("System prompt di Claude Code salvato");
                }
                catch (Exception ex)
                {
                    session.Rollback();
                    _logger.LogError(ex, "Errore nel salvataggio del system prompt di Claude Code");
                    throw;
                }
            });
        }

        public async Task<string> GetSystemPromptAsync()
        {
            return await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(_systemPrompt)) return _systemPrompt;

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var session = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                    var settingsDal = session.GetDal<Setting>();
                    var setting = settingsDal.GetList().FirstOrDefault(s => s.Name == SYSTEM_PROMPT_SETTING);
                    if (setting != null && !string.IsNullOrEmpty(setting.ValueString))
                    {
                        _systemPrompt = setting.ValueString;
                        return _systemPrompt;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore nel caricamento del system prompt di Claude Code");
                }

                return @"Sei un assistente specializzato in documentazione markdown.
Rispondi in modo chiaro e conciso, usando markdown quando aiuta la leggibilità.";
            });
        }

        /// <summary>Claude Code usa l'abbonamento dell'utente via login del CLI: nessuna API key.</summary>
        public Task<string> GetApiKeyAsync() => Task.FromResult("not-required");

        public Task SaveApiKeyAsync(string apiKey) => Task.CompletedTask;

        public Task<bool> TestApiKeyAsync(string apiKey) => Task.FromResult(IsAvailable());

        /// <summary>Versione del CLI installato, oppure <c>null</c> se non è risolvibile.</summary>
        public async Task<string> GetVersionAsync()
        {
            if (!IsAvailable()) return null;
            try
            {
                var psi = ClaudeCodeProcessLauncher.BuildStartInfo("--version");
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                using var process = Process.Start(psi);
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                return output?.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClaudeCodeProvider] lettura della versione fallita");
                return null;
            }
        }
    }
}
