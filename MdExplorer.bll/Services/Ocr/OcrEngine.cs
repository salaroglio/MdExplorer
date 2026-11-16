using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.Ocr
{
    public class OcrEngineStatus
    {
        public string Version { get; set; }
        public bool IsInstalled { get; set; }
        public string Path { get; set; }
    }

    /// <summary>Ciò che il modello ha scritto per una pagina, e se si è fermato prima di finirla.</summary>
    public class OcrPageAnswer
    {
        public string Text { get; set; }
        public bool Truncated { get; set; }
    }

    public interface IOcrEngine
    {
        OcrEngineStatus GetStatus();

        /// <summary>Scarica il motore se manca. L'avanzamento ha <c>ModelId = "ocr-engine"</c>.</summary>
        Task InstallAsync(IProgress<DownloadProgress> progress, CancellationToken ct);

        /// <summary>
        /// Termina un <c>llama-server</c> rimasto acceso da una sessione di MdExplorer che non c'è
        /// più (chiusa o caduta a metà di una conversione). Da chiamare all'avvio del servizio.
        /// </summary>
        void KillOrphan();

        /// <summary>Avvia il motore col modello e resta acceso finché la sessione non viene chiusa.</summary>
        Task<IOcrEngineSession> StartAsync(string modelPath, string projectorPath, int contextLength, CancellationToken ct);
    }

    public interface IOcrEngineSession : IAsyncDisposable
    {
        Task<OcrPageAnswer> ReadPageAsync(byte[] png, string prompt, CancellationToken ct);
    }

    /// <summary>
    /// Il motore che fa girare i modelli OCR: <c>llama-server</c> di llama.cpp, come processo a parte.
    /// <para>
    /// È lo stesso llama.cpp della chat locale: stesso pacchetto, stessa versione
    /// (<c>LlamaCpp:Version</c>), stessa variante scelta dall'utente. La chat ne carica le librerie
    /// dentro il servizio e gli passa solo testo; l'OCR deve passargli anche l'immagine della pagina,
    /// e quella strada dentro il servizio oggi non regge (provato il 15/11/2026: con LLamaSharp 0.26
    /// il modello si carica e il processo cade alla codifica dell'immagine). Da processo a parte,
    /// se cade fallisce una conversione, non MdExplorer.
    /// </para>
    /// <para>
    /// Il processo nasce con la conversione di un pdf e muore con lei. Due protezioni per quando
    /// MdExplorer si ferma a metà: alla chiusura il servizio termina i processi che ha avviato; e
    /// ogni avvio annota il processo in un file, così il servizio che riparte dopo una caduta trova
    /// l'orfano e lo termina (<see cref="KillOrphan"/>).
    /// </para>
    /// </summary>
    public class OcrEngine : IOcrEngine, IDisposable
    {
        public const string ProgressId = "ocr-engine";

        private const string ProcessName = "llama-server";

        private readonly ILlamaBackendService _backend;
        private readonly ILogger<OcrEngine> _logger;
        private readonly string _pidFile;
        private readonly object _activeLock = new object();
        private readonly List<Session> _active = new List<Session>();

        public OcrEngine(ILlamaBackendService backend, ILogger<OcrEngine> logger)
        {
            _backend = backend;
            _logger = logger;
            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _pidFile = Path.Combine(userHome, "MdExplorer-Models", "llama-backends", "ocr-server.pid");
            // SIGTERM e chiusura normale del processo: i figli non devono sopravvivere al servizio.
            AppDomain.CurrentDomain.ProcessExit += (_, __) => KillActive();
        }

        public OcrEngineStatus GetStatus()
        {
            var executable = _backend.GetServerPath();
            return new OcrEngineStatus
            {
                Version = _backend.GetVersion(),
                IsInstalled = executable != null,
                Path = executable
            };
        }

        public async Task InstallAsync(IProgress<DownloadProgress> progress, CancellationToken ct)
        {
            string failure = null;
            var relay = new Relay(p =>
            {
                if (p.Status != null && p.Status.StartsWith("Error"))
                {
                    failure = p.Status;
                    return;
                }
                progress?.Report(new DownloadProgress
                {
                    ModelId = ProgressId, BytesDownloaded = p.BytesDownloaded, TotalBytes = p.TotalBytes, Status = p.Status
                });
            });
            if (!await _backend.EnsureServerAsync(relay, ct) || _backend.GetServerPath() == null)
            {
                throw new InvalidOperationException(UiText.Say(
                    "Il motore llama.cpp non si è installato" + (failure == null ? "." : ": " + failure),
                    "The llama.cpp engine could not be installed" + (failure == null ? "." : ": " + failure)));
            }
        }

        public void Dispose() => KillActive();

        private void KillActive()
        {
            Session[] sessions;
            lock (_activeLock)
            {
                sessions = _active.ToArray();
            }
            foreach (var session in sessions)
            {
                session.Kill();
            }
        }

        /// <summary>
        /// Il file annota «pid del server, pid del servizio che l'ha avviato». L'orfano è un
        /// <c>llama-server</c> ancora vivo il cui servizio non c'è più; se il servizio è vivo (un
        /// altro MdExplorer sullo stesso utente) il processo è suo e non si tocca.
        /// </summary>
        public void KillOrphan()
        {
            try
            {
                if (!File.Exists(_pidFile))
                {
                    return;
                }
                var parts = File.ReadAllText(_pidFile).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 || !int.TryParse(parts[0], out var serverPid) || !int.TryParse(parts[1], out var ownerPid))
                {
                    File.Delete(_pidFile);
                    return;
                }
                if (ownerPid != Environment.ProcessId && IsAlive(ownerPid, null))
                {
                    return;
                }
                if (IsAlive(serverPid, ProcessName))
                {
                    using (var orphan = Process.GetProcessById(serverPid))
                    {
                        orphan.Kill(entireProcessTree: true);
                        orphan.WaitForExit(5000);
                    }
                    _logger.LogWarning("[OCR] terminato un llama-server rimasto da una sessione precedente (pid {Pid})", serverPid);
                }
                File.Delete(_pidFile);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[OCR] controllo del llama-server rimasto non riuscito");
            }
        }

        /// <summary>Il processo esiste; con <paramref name="name"/>, esiste e si chiama così (un pid si riusa).</summary>
        private static bool IsAlive(int pid, string name)
        {
            try
            {
                using (var process = Process.GetProcessById(pid))
                {
                    return !process.HasExited && (name == null || process.ProcessName == name);
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private sealed class Relay : IProgress<DownloadProgress>
        {
            private readonly Action<DownloadProgress> _report;
            public Relay(Action<DownloadProgress> report) => _report = report;
            public void Report(DownloadProgress value) => _report(value);
        }

        public async Task<IOcrEngineSession> StartAsync(string modelPath, string projectorPath, int contextLength, CancellationToken ct)
        {
            var executable = _backend.GetServerPath() ?? throw new OcrNotReadyException(UiText.Say(
                "Il motore OCR non è installato: scaricalo dal passo «Conversione in markdown».",
                "The OCR engine is not installed: download it from the \"Convert to markdown\" step."));

            var port = FreePort();
            var engineDir = Path.GetDirectoryName(executable);
            var start = new ProcessStartInfo(executable)
            {
                WorkingDirectory = engineDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var argument in new[]
            {
                "-m", modelPath, "--mmproj", projectorPath,
                "--host", "127.0.0.1", "--port", port.ToString(),
                "-c", contextLength.ToString(), "--temp", "0",
                "-t", Math.Max(1, Environment.ProcessorCount * 3 / 4).ToString()
            })
            {
                start.ArgumentList.Add(argument);
            }
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Le librerie di llama.cpp stanno accanto all'eseguibile, non nei percorsi di sistema.
                var current = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
                start.Environment["LD_LIBRARY_PATH"] = string.IsNullOrEmpty(current) ? engineDir : engineDir + ":" + current;
            }

            var session = new Session(start, port, _logger, this);
            lock (_activeLock)
            {
                _active.Add(session);
            }
            File.WriteAllText(_pidFile, $"{session.ProcessId} {Environment.ProcessId}");
            try
            {
                await session.WaitUntilReadyAsync(ct);
                return session;
            }
            catch
            {
                await session.DisposeAsync();
                throw;
            }
        }

        private void Closed(Session session)
        {
            lock (_activeLock)
            {
                _active.Remove(session);
                if (_active.Count == 0 && File.Exists(_pidFile))
                {
                    File.Delete(_pidFile);
                }
            }
        }

        private static int FreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }

        private sealed class Session : IOcrEngineSession
        {
            private static readonly TimeSpan StartupLimit = TimeSpan.FromMinutes(3);

            private readonly Process _process;
            private readonly HttpClient _http;
            private readonly ILogger _logger;
            private readonly OcrEngine _owner;
            private readonly Queue<string> _lastLines = new Queue<string>();

            public int ProcessId => _process.Id;

            public Session(ProcessStartInfo start, int port, ILogger logger, OcrEngine owner)
            {
                _logger = logger;
                _owner = owner;
                // Una pagina fitta su una CPU lenta può volerci molto: il limite vero è la cancellazione.
                _http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}"), Timeout = TimeSpan.FromMinutes(30) };
                _process = new Process { StartInfo = start };
                _process.OutputDataReceived += (_, e) => Remember(e.Data);
                _process.ErrorDataReceived += (_, e) => Remember(e.Data);
                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }

            private void Remember(string line)
            {
                if (line == null)
                {
                    return;
                }
                lock (_lastLines)
                {
                    _lastLines.Enqueue(line);
                    while (_lastLines.Count > 15)
                    {
                        _lastLines.Dequeue();
                    }
                }
            }

            private string LastLines()
            {
                lock (_lastLines)
                {
                    return string.Join("\n", _lastLines);
                }
            }

            public async Task WaitUntilReadyAsync(CancellationToken ct)
            {
                var clock = Stopwatch.StartNew();
                while (clock.Elapsed < StartupLimit)
                {
                    ct.ThrowIfCancellationRequested();
                    if (_process.HasExited)
                    {
                        throw new InvalidOperationException(UiText.Say(
                            $"Il motore OCR si è chiuso all'avvio (codice {_process.ExitCode}). Ultime righe:\n{LastLines()}",
                            $"The OCR engine exited at startup (code {_process.ExitCode}). Last lines:\n{LastLines()}"));
                    }
                    try
                    {
                        using (var response = await _http.GetAsync("/health", ct))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                return;
                            }
                        }
                    }
                    catch (HttpRequestException)
                    {
                        // Non ascolta ancora: sta caricando il modello.
                    }
                    await Task.Delay(500, ct);
                }
                throw new TimeoutException(UiText.Say(
                    $"Il motore OCR non è pronto dopo {StartupLimit.TotalMinutes:0} minuti. Ultime righe:\n{LastLines()}",
                    $"The OCR engine is not ready after {StartupLimit.TotalMinutes:0} minutes. Last lines:\n{LastLines()}"));
            }

            public async Task<OcrPageAnswer> ReadPageAsync(byte[] png, string prompt, CancellationToken ct)
            {
                var body = JsonSerializer.Serialize(new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "image_url", image_url = new { url = "data:image/png;base64," + Convert.ToBase64String(png) } },
                                new { type = "text", text = prompt }
                            }
                        }
                    },
                    temperature = 0,
                    max_tokens = PdfOcrService.MaxTokensPerPage,
                    // Il modello sa anche «ragionare» prima di rispondere; per l'OCR va spento, come
                    // fa la sua scheda. Lasciato com'è, una pagina quasi vuota torna come «</think>».
                    chat_template_kwargs = new { enable_thinking = false }
                });

                using (var content = new StringContent(body, Encoding.UTF8, "application/json"))
                using (var response = await _http.PostAsync("/v1/chat/completions", content, ct))
                {
                    var answer = await response.Content.ReadAsStringAsync(ct);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(UiText.Say(
                            $"Il motore OCR ha rifiutato la pagina ({(int)response.StatusCode}): {answer}",
                            $"The OCR engine rejected the page ({(int)response.StatusCode}): {answer}"));
                    }
                    using (var json = JsonDocument.Parse(answer))
                    {
                        var choice = json.RootElement.GetProperty("choices")[0];
                        return new OcrPageAnswer
                        {
                            Text = choice.GetProperty("message").GetProperty("content").GetString(),
                            Truncated = choice.GetProperty("finish_reason").GetString() == "length"
                        };
                    }
                }
            }

            /// <summary>Termina il processo subito, senza attese: è la strada della chiusura del servizio.</summary>
            public void Kill()
            {
                try
                {
                    if (!_process.HasExited)
                    {
                        _process.Kill(entireProcessTree: true);
                    }
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
                {
                    // Già uscito, o già rilasciato: è ciò che si voleva.
                }
            }

            public async ValueTask DisposeAsync()
            {
                _http.Dispose();
                try
                {
                    if (!_process.HasExited)
                    {
                        _process.Kill(entireProcessTree: true);
                        await _process.WaitForExitAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[OCR] il motore non si è chiuso pulito");
                }
                _owner.Closed(this);
                _process.Dispose();
            }
        }
    }

    /// <summary>Manca qualcosa che l'utente può installare (motore o modello): la UI propone il download.</summary>
    public class OcrNotReadyException : Exception
    {
        public OcrNotReadyException(string message) : base(message) { }
    }
}
