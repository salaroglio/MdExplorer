using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentMemory
{
    /// <summary>L'addon Fuseki non è installato (manca il marker <c>current</c> o il jar). Fail-loud, actionable.</summary>
    public sealed class FusekiAddonMissingException : Exception
    {
        public FusekiAddonMissingException(string message) : base(message) { }
    }

    /// <summary>
    /// Avvia/ferma un'istanza <b>gestita</b> di Fuseki (§ roadmap Fuseki: addon on-demand,
    /// layout ordinato). L'istanza è di proprietà del Service (R4: gli agenti non vedono mai
    /// Fuseki), gira su porta random <b>loopback</b>, e ospita i dataset dei progetti creati via
    /// admin API (come il flusso esterno esistente). Layout gestito, mai a discrezione dell'utente:
    /// <code>
    /// {AppData}/MdExplorer/tools/fuseki/&lt;versione&gt;/fuseki-server.jar   (binari, upgrade side-by-side)
    /// {AppData}/MdExplorer/tools/fuseki/current                          (marker: versione attiva)
    /// {AppData}/MdExplorer/fuseki-data/                                  (FUSEKI_BASE: dati, separati dai binari)
    /// </code>
    /// Fail-loud se il marker <c>current</c> o il jar mancano (⇒ addon da scaricare).
    /// </summary>
    public interface IFusekiProcessManager
    {
        /// <summary>Assicura l'istanza gestita in esecuzione; ritorna il base URI loopback (es. http://127.0.0.1:PORT).</summary>
        Task<string> EnsureRunningAsync(CancellationToken ct = default);
    }

    public class FusekiProcessManager : IFusekiProcessManager, IDisposable
    {
        private readonly ILogger<FusekiProcessManager> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private Process _process;
        private string _baseUri;
        private bool _disposed;

        public FusekiProcessManager(ILogger<FusekiProcessManager> logger)
        {
            _logger = logger;
        }

        private static string ToolsRoot => Path.Combine(CrossPlatformPath.GetMdExplorerDataDirectory(), "tools", "fuseki");
        private static string CurrentMarker => Path.Combine(ToolsRoot, "current");
        private static string FusekiBase => Path.Combine(CrossPlatformPath.GetMdExplorerDataDirectory(), "fuseki-data");

        /// <summary>
        /// Risolve la <b>home</b> della dist Fuseki attiva dal marker <c>current</c>: la cartella
        /// <c>tools/fuseki/{version}</c> che contiene <c>fuseki-server.jar</c> <b>e</b> <c>webapp/</c>
        /// (Fuseki 4.x cerca il resourceBase in <c>FUSEKI_HOME/webapp</c>). Fail-loud actionable se
        /// manca marker/jar/webapp — l'addon non è installato o è incompleto (niente fallback muto).
        /// </summary>
        public static string ResolveHomeOrThrow()
        {
            if (!File.Exists(CurrentMarker))
                throw new FusekiAddonMissingException(
                    $"Addon Fuseki non installato: manca il marker '{CurrentMarker}'. Scarica l'addon Fuseki (Impostazioni → Dipendenze) prima di usare la memoria gestita.");

            var version = File.ReadAllText(CurrentMarker).Trim();
            if (string.IsNullOrWhiteSpace(version))
                throw new FusekiAddonMissingException($"Marker Fuseki '{CurrentMarker}' vuoto: versione attiva non definita.");

            var home = Path.Combine(ToolsRoot, version);
            var jar = Path.Combine(home, "fuseki-server.jar");
            if (!File.Exists(jar))
                throw new FusekiAddonMissingException($"Jar Fuseki non trovato per la versione '{version}': '{jar}'. Reinstalla l'addon Fuseki.");
            if (!Directory.Exists(Path.Combine(home, "webapp")))
                throw new FusekiAddonMissingException($"Dist Fuseki incompleta per la versione '{version}': manca '{Path.Combine(home, "webapp")}'. Reinstalla l'addon Fuseki (serve la distribuzione completa, non il solo jar).");
            return home;
        }

        public async Task<string> EnsureRunningAsync(CancellationToken ct = default)
        {
            if (_process != null && !_process.HasExited && _baseUri != null)
                return _baseUri;

            await _gate.WaitAsync(ct);
            try
            {
                if (_process != null && !_process.HasExited && _baseUri != null)
                    return _baseUri;

                var home = ResolveHomeOrThrow();   // fail-loud se addon assente/incompleto
                var jar = Path.Combine(home, "fuseki-server.jar");
                Directory.CreateDirectory(FusekiBase);

                var port = FreeLoopbackPort();
                var baseUri = $"http://127.0.0.1:{port}";

                var psi = new ProcessStartInfo
                {
                    FileName = "java",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    // cwd = home: Fuseki cerca ./webapp; FUSEKI_HOME lo conferma esplicitamente.
                    WorkingDirectory = home,
                };
                // Cappa l'heap: senza -Xmx la JVM prende ~1/4 della RAM fisica (GB!) per una
                // memoria di fatti che è minuscola. 512m è ampio e non affama la macchina.
                psi.ArgumentList.Add("-Xmx512m");
                psi.ArgumentList.Add("-jar");
                psi.ArgumentList.Add(jar);
                psi.ArgumentList.Add("--port");
                psi.ArgumentList.Add(port.ToString());
                psi.Environment["FUSEKI_HOME"] = home;                  // resourceBase = FUSEKI_HOME/webapp
                psi.Environment["FUSEKI_BASE"] = FusekiBase;            // dati (run/databases) sotto il layout gestito

                _logger.LogInformation("[FusekiManaged] avvio istanza gestita su {Base} (base dati {Data})", baseUri, FusekiBase);
                var proc = Process.Start(psi);
                if (proc == null)
                    throw new InvalidOperationException("Avvio del processo Fuseki fallito (java non trovato nel PATH?).");
                // Scarica gli stream per non bloccare il figlio; log a debug.
                proc.OutputDataReceived += (_, e) => { if (e.Data != null) _logger.LogDebug("[FusekiManaged] {Line}", e.Data); };
                proc.ErrorDataReceived += (_, e) => { if (e.Data != null) _logger.LogDebug("[FusekiManaged/err] {Line}", e.Data); };
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                await WaitUntilHealthyAsync(baseUri, proc, ct);

                _process = proc;
                _baseUri = baseUri;
                _logger.LogInformation("[FusekiManaged] istanza gestita pronta su {Base}", baseUri);
                return baseUri;
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task WaitUntilHealthyAsync(string baseUri, Process proc, CancellationToken ct)
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
            while (DateTime.UtcNow < deadline)
            {
                ct.ThrowIfCancellationRequested();
                if (proc.HasExited)
                    throw new InvalidOperationException($"Il processo Fuseki è terminato durante l'avvio (exit {proc.ExitCode}).");
                try
                {
                    var resp = await http.GetAsync(baseUri + "/$/ping", ct);
                    if (resp.IsSuccessStatusCode) return;
                }
                catch { /* non ancora su */ }
                await Task.Delay(500, ct);
            }
            throw new TimeoutException($"Fuseki gestito non ha risposto su {baseUri}/$/ping entro 30s.");
        }

        private static int FreeLoopbackPort()
        {
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                var p = _process;
                _process = null;
                if (p != null && !p.HasExited)
                {
                    _logger.LogInformation("[FusekiManaged] arresto istanza gestita");
                    p.Kill(entireProcessTree: true);
                    p.WaitForExit(5000);
                }
                p?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[FusekiManaged] arresto non pulito");
            }
            _gate.Dispose();
        }
    }
}
