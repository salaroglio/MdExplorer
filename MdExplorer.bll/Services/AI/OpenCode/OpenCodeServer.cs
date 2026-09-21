using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.OpenCode
{
    /// <summary>
    /// Il server <c>opencode serve</c> di questa istanza di MdExplorer: uno solo, avviato alla
    /// prima richiesta e chiuso insieme all'applicazione.
    /// <para>
    /// <b>Uno solo, non uno per progetto</b>: ogni chiamata porta la propria cartella nel
    /// parametro <c>directory</c>, quindi un processo basta per tutti i progetti aperti.
    /// </para>
    /// <para>
    /// <b>Alla prima richiesta</b>, non all'avvio: chi non usa opencode non deve pagare un
    /// processo acceso a vuoto.
    /// </para>
    /// <para>
    /// <b>Con una password casuale</b> (<c>OPENCODE_SERVER_PASSWORD</c>): la porta è solo
    /// locale, ma su una macchina condivisa "solo locale" non vuol dire "solo noi". Misurato il
    /// 21/09/2026 su opencode 1.18.30: la password si presenta come HTTP Basic con utente
    /// <c>opencode</c>; Bearer e ogni altro utente ricevono 401.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F3.</para>
    /// </summary>
    public sealed class OpenCodeServer : IHostedService, IDisposable
    {
        /// <summary>La riga con cui il server dichiara la porta scelta. Misurata, non dedotta.</summary>
        private static readonly Regex ListeningLine =
            new(@"listening on\s+(?<url>https?://\S+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Quanto si aspetta quella riga. Generoso: il primo avvio di opencode carica i plugin e
        /// la configurazione. Superato il tempo si fallisce, dicendo cosa si era letto.
        /// </summary>
        private static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(60);

        private readonly ILogger<OpenCodeServer> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private Process _process;
        private Uri _baseAddress;
        private string _password;
        private readonly StringBuilder _startupLog = new();

        public OpenCodeServer(ILogger<OpenCodeServer> logger)
        {
            _logger = logger;
        }

        /// <summary>Il server è acceso adesso? Non lo avvia: è una domanda, non un ordine.</summary>
        public bool IsRunning => _process != null && !_process.HasExited && _baseAddress != null;

        /// <summary>L'indirizzo del server acceso, oppure <c>null</c>.</summary>
        public Uri BaseAddress => IsRunning ? _baseAddress : null;

        /// <summary>
        /// Avvia il server se serve e restituisce il suo indirizzo. Chiamate in parallelo si
        /// mettono in fila sulla stessa partenza: due <c>opencode serve</c> per la stessa
        /// istanza sarebbero due verità.
        /// </summary>
        public async Task<Uri> EnsureRunningAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning) return _baseAddress;

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (IsRunning) return _baseAddress;

                // Il processo c'era e se n'è andato: si dice, e si riparte. Un server morto in
                // silenzio diventerebbe "la chat non risponde più" senza spiegazione.
                if (_process != null && _process.HasExited)
                {
                    _logger.LogWarning(
                        "[opencode] il server era uscito con codice {Code}: lo riavvio", SafeExitCode(_process));
                    Cleanup();
                }

                await StartAsync_Internal(cancellationToken).ConfigureAwait(false);
                return _baseAddress;
            }
            finally
            {
                _gate.Release();
            }
        }

        /// <summary>
        /// Un <see cref="HttpClient"/> già puntato al server e già autenticato. Chi chiama non
        /// deve sapere né la porta né la password: sono dettagli di questo oggetto.
        /// </summary>
        public async Task<HttpClient> CreateClientAsync(CancellationToken cancellationToken = default)
        {
            var baseAddress = await EnsureRunningAsync(cancellationToken).ConfigureAwait(false);
            var client = new HttpClient { BaseAddress = baseAddress, Timeout = TimeSpan.FromMinutes(10) };
            client.DefaultRequestHeaders.Authorization = BuildAuthentication(_password);
            return client;
        }

        /// <summary>
        /// HTTP Basic <c>opencode:&lt;password&gt;</c>. In un metodo suo perché è un fatto
        /// misurato, e va provato dai test senza accendere un server.
        /// </summary>
        public static AuthenticationHeaderValue BuildAuthentication(string password)
        {
            var raw = $"{OpenCodeProcessLauncher.BasicUser}:{password}";
            return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(raw)));
        }

        /// <summary>
        /// Il server risponde davvero? Una chiamata vera, non «il processo è vivo»: un processo
        /// acceso che non serve ancora richieste è la stessa cosa di un server assente, per chi
        /// aspetta una risposta.
        /// </summary>
        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var client = await CreateClientAsync(cancellationToken).ConfigureAwait(false);
                client.Timeout = TimeSpan.FromSeconds(15);
                using var res = await client.GetAsync("/config/providers", cancellationToken).ConfigureAwait(false);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[opencode] controllo di salute fallito");
                return false;
            }
        }

        private async Task StartAsync_Internal(CancellationToken cancellationToken)
        {
            _password = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
            _startupLog.Clear();

            var psi = OpenCodeProcessLauncher.BuildServeStartInfo(_password);
            var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

            var listening = new TaskCompletionSource<Uri>(TaskCreationOptions.RunContinuationsAsynchronously);

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                lock (_startupLog) { if (_startupLog.Length < 8000) _startupLog.AppendLine(e.Data); }
                var m = ListeningLine.Match(e.Data);
                if (m.Success && Uri.TryCreate(m.Groups["url"].Value, UriKind.Absolute, out var uri))
                {
                    listening.TrySetResult(uri);
                }
            };
            // Anche stderr va letto, sempre: una pipe che nessuno svuota si riempie e blocca il
            // processo figlio. Qui serve pure a spiegare un avvio fallito.
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null) return;
                lock (_startupLog) { if (_startupLog.Length < 8000) _startupLog.AppendLine("[stderr] " + e.Data); }
            };
            process.Exited += (_, _) =>
                listening.TrySetException(new InvalidOperationException(
                    $"opencode serve è uscito subito (codice {SafeExitCode(process)}). Output:\n{StartupLog()}"));

            _logger.LogInformation("[opencode] avvio {File} serve --port 0", psi.FileName);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            _process = process;

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(StartTimeout);
            try
            {
                _baseAddress = await listening.Task.WaitAsync(timeout.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                Cleanup();
                throw new TimeoutException(
                    $"opencode serve non ha dichiarato la sua porta entro {StartTimeout.TotalSeconds:N0} s. " +
                    $"Output letto finora:\n{StartupLog()}");
            }
            catch
            {
                Cleanup();
                throw;
            }

            _logger.LogInformation("[opencode] server pronto su {Address}", _baseAddress);
        }

        private string StartupLog()
        {
            lock (_startupLog) { return _startupLog.ToString(); }
        }

        private static int SafeExitCode(Process p)
        {
            try { return p.ExitCode; } catch { return -1; }
        }

        private void Cleanup()
        {
            var process = _process;
            _process = null;
            _baseAddress = null;
            if (process == null) return;

            try
            {
                if (!process.HasExited)
                {
                    // L'albero intero: opencode è un binario npm, e il processo che vediamo può
                    // esserne solo l'avvio.
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[opencode] non sono riuscito a chiudere il server");
            }
            finally
            {
                process.Dispose();
            }
        }

        /// <summary>Niente all'avvio: il server nasce alla prima richiesta.</summary>
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>
        /// Alla chiusura di MdExplorer il server si spegne. Senza questo resterebbe acceso
        /// dopo l'uscita dell'app — è già successo con l'MCP su Linux.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_process != null) _logger.LogInformation("[opencode] chiudo il server");
            Cleanup();
            return Task.CompletedTask;
        }

        public void Dispose() => Cleanup();
    }
}
