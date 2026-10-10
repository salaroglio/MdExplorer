using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.Fuseki
{
    /// <summary>
    /// Fuseki non può partire: jar assente, java assente o troppo vecchio, avvio fallito.
    /// Il messaggio dice che cosa manca, dove si è guardato e che cosa fare (fail-loud, actionable).
    /// </summary>
    public sealed class FusekiUnavailableException : Exception
    {
        public FusekiUnavailableException(string message) : base(message) { }
        public FusekiUnavailableException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>Dove stanno jar e dati del Fuseki gestito; i default sono quelli del pacchetto.</summary>
    public sealed class ManagedFusekiOptions
    {
        /// <summary>Il jar nel pacchetto, accanto a plantuml.jar (D19/D20).</summary>
        public string JarPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "Binaries", "jena-fuseki-server.jar");

        /// <summary><c>FUSEKI_BASE</c>: dati TDB2, indici Lucene, configurazione. Separati dal pacchetto. Obbligatorio:
        /// lo imposta chi compone il servizio (<c>%AppData%/MdExplorer/fuseki-data</c>) o il test (cartella temporanea).</summary>
        public string DataDirectory { get; set; }

        /// <summary>Jena 6 richiede Java 21.</summary>
        public int MinimumJavaMajor { get; set; } = 21;

        /// <summary>Senza -Xmx la JVM prende ~1/4 della RAM fisica per dati che sono piccoli.</summary>
        public int HeapMegabytes { get; set; } = 512;

        public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Avvia e ferma l'UNICO Fuseki del servizio (D19): il jar del pacchetto, su una porta libera di
    /// loopback, con <c>--admin=localhost</c> (la memoria degli agenti crea i suoi dataset TDB2 a caldo)
    /// e il dataset <c>code</c> del grafo del codice descritto da <see cref="FusekiManagedConfig"/>
    /// in <c>FUSEKI_BASE/configuration/</c>, perché un dataset con jena-text non si crea a caldo (provato il 10/10).
    /// Parte alla prima richiesta, non all'avvio del servizio; riparte da solo se il processo è morto.
    /// </summary>
    public interface IFusekiProcessManager
    {
        /// <summary>Assicura l'istanza in esecuzione; ritorna il base URI loopback (es. http://127.0.0.1:PORT).</summary>
        Task<string> EnsureRunningAsync(CancellationToken ct = default);

        /// <summary>Base URI se l'istanza è su, altrimenti <c>null</c>: per lo stato, senza avviare niente.</summary>
        string RunningBaseUri { get; }

        /// <summary>Ferma l'istanza (se c'è). Il servizio lo fa in Dispose; i test a fine prova.</summary>
        Task StopAsync();

        /// <summary>
        /// Compatta un dataset TDB2 (<c>POST /$/compact/{dataset}?deleteOld=true</c>): TDB2 è copy-on-write e i blocchi
        /// vecchi restano su disco finché non si compatta. Aspetta la fine del task; un rifiuto è un'eccezione, non un silenzio.
        /// </summary>
        Task CompactAsync(string dataset, CancellationToken ct = default);
    }

    public class FusekiProcessManager : IFusekiProcessManager, IDisposable
    {
        private readonly ManagedFusekiOptions _options;
        private readonly IJavaLocator _javaLocator;
        private readonly ILogger<FusekiProcessManager> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private Process _process;
        private string _baseUri;
        private bool _disposed;

        public FusekiProcessManager(ManagedFusekiOptions options, IJavaLocator javaLocator, ILogger<FusekiProcessManager> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.DataDirectory))
                throw new ArgumentException("ManagedFusekiOptions.DataDirectory è obbligatorio (FUSEKI_BASE: dove stanno dati e configurazione).", nameof(options));
            _javaLocator = javaLocator ?? throw new ArgumentNullException(nameof(javaLocator));
            _logger = logger;
        }

        public string RunningBaseUri => _process != null && !_process.HasExited ? _baseUri : null;

        public async Task<string> EnsureRunningAsync(CancellationToken ct = default)
        {
            var running = RunningBaseUri;
            if (running != null) return running;

            await _gate.WaitAsync(ct);
            try
            {
                running = RunningBaseUri;
                if (running != null) return running;

                if (_process != null)
                {
                    _logger.LogWarning("[Fuseki] il processo precedente è terminato (exit {Code}); riavvio", SafeExitCode(_process));
                    _process.Dispose();
                    _process = null;
                    _baseUri = null;
                }

                var jar = ResolveJarOrThrow();
                var java = ResolveJavaOrThrow();
                Directory.CreateDirectory(_options.DataDirectory);
                var config = FusekiManagedConfig.WriteCodeDataset(_options.DataDirectory);
                if (config.Changed)
                    _logger.LogInformation("[Fuseki] configurazione del dataset '{Dataset}' scritta in {Path} (versione {Version})",
                        FusekiManagedConfig.CodeDataset, config.Path, FusekiManagedConfig.ConfigVersion);

                var port = FreeLoopbackPort();
                var baseUri = $"http://127.0.0.1:{port}";

                var psi = new ProcessStartInfo
                {
                    FileName = java.Path,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _options.DataDirectory,
                };
                psi.ArgumentList.Add($"-Xmx{_options.HeapMegabytes}m");
                // Senza questi due la JVM 21 avvisa a ogni avvio (accesso nativo di Lucene; Vector API per l'indice).
                psi.ArgumentList.Add("--enable-native-access=ALL-UNNAMED");
                psi.ArgumentList.Add("--add-modules");
                psi.ArgumentList.Add("jdk.incubator.vector");
                psi.ArgumentList.Add("-jar");
                psi.ArgumentList.Add(jar);
                psi.ArgumentList.Add("--port");
                psi.ArgumentList.Add(port.ToString());
                psi.ArgumentList.Add("--localhost");
                psi.ArgumentList.Add("--ping");
                // Admin senza password, raggiungibile solo da loopback: il servizio crea i dataset della memoria a caldo.
                psi.ArgumentList.Add("--admin=localhost");
                psi.ArgumentList.Add("--compact");
                psi.Environment["FUSEKI_BASE"] = _options.DataDirectory;

                _logger.LogInformation("[Fuseki] avvio {Jar} con {Java} ({JavaVersion}) su {Base}, dati in {Data}",
                    jar, java.Path, java.Raw, baseUri, _options.DataDirectory);

                Process proc;
                try
                {
                    proc = Process.Start(psi);
                }
                catch (Exception ex)
                {
                    throw new FusekiUnavailableException($"Avvio di Fuseki fallito con '{java.Path}': {ex.Message}", ex);
                }
                if (proc == null)
                    throw new FusekiUnavailableException($"Avvio di Fuseki fallito: Process.Start ha restituito null per '{java.Path}'.");

                var lastLines = new System.Collections.Concurrent.ConcurrentQueue<string>();
                void Keep(string line)
                {
                    if (line == null) return;
                    lastLines.Enqueue(line);
                    while (lastLines.Count > 20 && lastLines.TryDequeue(out _)) { }
                }
                proc.OutputDataReceived += (_, e) => { Keep(e.Data); if (e.Data != null) _logger.LogDebug("[Fuseki] {Line}", e.Data); };
                proc.ErrorDataReceived += (_, e) => { Keep(e.Data); if (e.Data != null) _logger.LogDebug("[Fuseki/err] {Line}", e.Data); };
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                try
                {
                    await WaitUntilHealthyAsync(baseUri, proc, ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    try { if (!proc.HasExited) proc.Kill(true); } catch { }
                    proc.Dispose();
                    throw new FusekiUnavailableException(
                        $"{ex.Message} Ultime righe di Fuseki: {string.Join(" | ", lastLines)}", ex);
                }

                _process = proc;
                _baseUri = baseUri;
                _logger.LogInformation("[Fuseki] pronto su {Base}", baseUri);
                return baseUri;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task CompactAsync(string dataset, CancellationToken ct = default)
        {
            var baseUri = await EnsureRunningAsync(ct);
            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            var started = DateTime.UtcNow;
            using var resp = await http.PostAsync($"{baseUri}/$/compact/{Uri.EscapeDataString(dataset)}?deleteOld=true", new StringContent(string.Empty), ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"Fuseki ha rifiutato la compattazione di '{dataset}': HTTP {(int)resp.StatusCode} {body}");
            // La risposta è un task asincrono: {"taskId":"…"}; si aspetta che finisca guardando /$/tasks/{id}.
            string taskId = null;
            try { using var doc = System.Text.Json.JsonDocument.Parse(body); if (doc.RootElement.TryGetProperty("taskId", out var t)) taskId = t.GetString(); } catch (System.Text.Json.JsonException) { }
            if (taskId != null)
            {
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    using var st = await http.GetAsync($"{baseUri}/$/tasks/{taskId}", ct);
                    var stBody = await st.Content.ReadAsStringAsync(ct);
                    if (stBody.Contains("\"finished\"", StringComparison.Ordinal) && !stBody.Contains("\"finished\":null", StringComparison.Ordinal)) break;
                    if (DateTime.UtcNow - started > TimeSpan.FromMinutes(30)) throw new TimeoutException($"La compattazione di '{dataset}' non è finita in 30 minuti.");
                    await Task.Delay(500, ct);
                }
            }
            _logger.LogInformation("[Fuseki] dataset '{Dataset}' compattato in {Seconds:0.0} s", dataset, (DateTime.UtcNow - started).TotalSeconds);
        }

        private string ResolveJarOrThrow()
        {
            var jar = _options.JarPath;
            if (string.IsNullOrWhiteSpace(jar) || !File.Exists(jar))
                throw new FusekiUnavailableException(
                    $"Jar di Fuseki non trovato: '{jar}'. Fa parte del pacchetto di MdExplorer (Binaries/jena-fuseki-server.jar): reinstalla MdExplorer.");
            return jar;
        }

        private JavaProbe ResolveJavaOrThrow()
        {
            var javaPath = _javaLocator.ResolveJavaPath();
            var probe = _javaLocator.Probe(javaPath);
            if (!probe.IsRunnable)
                throw new FusekiUnavailableException(
                    $"java non eseguibile ('{javaPath}': {probe.Error}). Fuseki (grafo del codice e memoria) richiede Java {_options.MinimumJavaMajor}: " +
                    "installa Temurin 21 (adoptium.net) oppure imposta il percorso in 'JavaPath'.");
            if (probe.Major == null)
                throw new FusekiUnavailableException(
                    $"Versione di java non leggibile da '{javaPath}' («{probe.Raw}»). Fuseki richiede Java {_options.MinimumJavaMajor}.");
            if (probe.Major < _options.MinimumJavaMajor)
                throw new FusekiUnavailableException(
                    $"Java {probe.Major} trovato in '{javaPath}' («{probe.Raw}»): MdExplorer richiede Java {_options.MinimumJavaMajor} per Fuseki " +
                    "(grafo del codice e memoria degli agenti). Installa Temurin 21 (adoptium.net) oppure imposta 'JavaPath' su un Java 21.");
            return probe;
        }

        private async Task WaitUntilHealthyAsync(string baseUri, Process proc, CancellationToken ct)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var deadline = DateTime.UtcNow + _options.StartupTimeout;
            while (DateTime.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();
                if (proc.HasExited)
                    throw new InvalidOperationException($"Fuseki è terminato durante l'avvio (exit {proc.ExitCode}).");
                try
                {
                    var resp = await http.GetAsync(baseUri + "/$/ping", ct);
                    if (resp.IsSuccessStatusCode) return;
                }
                catch (HttpRequestException) { /* non ancora su */ }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested) { /* timeout del singolo ping */ }
                await Task.Delay(250, ct);
            }
            throw new TimeoutException($"Fuseki non ha risposto su {baseUri}/$/ping entro {_options.StartupTimeout.TotalSeconds:0} secondi.");
        }

        private static int FreeLoopbackPort()
        {
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static string SafeExitCode(Process p)
        {
            try { return p.HasExited ? p.ExitCode.ToString() : "?"; } catch { return "?"; }
        }

        public async Task StopAsync()
        {
            await _gate.WaitAsync();
            try { StopCore(); }
            finally { _gate.Release(); }
        }

        private void StopCore()
        {
            var p = _process;
            _process = null;
            _baseUri = null;
            if (p == null) return;
            try
            {
                if (!p.HasExited)
                {
                    _logger.LogInformation("[Fuseki] arresto");
                    p.Kill(entireProcessTree: true);
                    p.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Fuseki] arresto non pulito");
            }
            finally
            {
                p.Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopCore();
            _gate.Dispose();
        }
    }
}
