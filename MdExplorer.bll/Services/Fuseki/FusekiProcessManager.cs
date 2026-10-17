using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
        /// <summary>Se 'JavaPath' punta a un Java più vecchio di <see cref="MinimumJavaMajor"/>, cercare un Java adatto in JAVA_HOME, nella scoperta di sistema e nel PATH (avvisando nel log). I test lo spengono per verificare il rifiuto.</summary>
        public bool SearchAlternativeJava { get; set; } = true;
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
        /// <summary>
        /// Chi ha avviato il Fuseki di questa cartella dati (Windows 12/10/2026: un Fuseki rimasto vivo dopo la morte del suo
        /// servizio teneva il lock del TDB2 e il nuovo usciva con exit 1). Scritto quando Fuseki è pronto, tolto all'arresto.
        /// </summary>
        public const string OwnerFileName = "fuseki.owner.json";

        public sealed class FusekiOwner
        {
            public int FusekiPid { get; set; }
            public long FusekiStartTicks { get; set; }
            public int OwnerPid { get; set; }
            public long OwnerStartTicks { get; set; }
        }

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
                throw new ArgumentException(UiText.Say("ManagedFusekiOptions.DataDirectory è obbligatorio (FUSEKI_BASE: dove stanno dati e configurazione).", "ManagedFusekiOptions.DataDirectory is required (FUSEKI_BASE: where data and configuration live)."), nameof(options));
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

                StopLeftoverOrThrow();

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
                    throw new FusekiUnavailableException(UiText.Say($"Avvio di Fuseki fallito con '{java.Path}': {ex.Message}", $"Starting Fuseki with '{java.Path}' failed: {ex.Message}"), ex);
                }
                if (proc == null)
                    throw new FusekiUnavailableException(UiText.Say($"Avvio di Fuseki fallito: Process.Start ha restituito null per '{java.Path}'.", $"Starting Fuseki failed: Process.Start returned null for '{java.Path}'."));
                // Windows: Fuseki muore con il servizio, comunque il servizio muoia (crash, chiusura forzata, aggiornamento).
                WindowsKillOnCloseJob.Assign(proc, _logger);

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
                    var tail = string.Join(" | ", lastLines);
                    var locked = tail.Contains("ProcessFileLock", StringComparison.Ordinal) || tail.Contains("Failed to get a lock", StringComparison.Ordinal);
                    throw new FusekiUnavailableException(
                        (locked
                            ? UiText.Say(
                                $"Il database del grafo in '{_options.DataDirectory}' è bloccato da un altro Fuseki: un java rimasto acceso da un avvio precedente di MdExplorer, o un'altra istanza di MdExplorer aperta. Chiudi le altre istanze; se non ce ne sono, termina i processi java di Fuseki (Gestione attività su Windows) e riprova. ",
                                $"The graph database in '{_options.DataDirectory}' is locked by another Fuseki: a java left running by an earlier start of MdExplorer, or another MdExplorer instance that is open. Close the other instances; if there are none, end Fuseki's java processes (Task Manager on Windows) and try again. ")
                            : string.Empty) +
                        UiText.Say($"{ex.Message} Ultime righe di Fuseki: {tail}", $"{ex.Message} Last lines from Fuseki: {tail}"), ex);
                }

                _process = proc;
                _baseUri = baseUri;
                WriteOwner(proc);
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
                throw new InvalidOperationException(UiText.Say($"Fuseki ha rifiutato la compattazione di '{dataset}': HTTP {(int)resp.StatusCode} {body}", $"Fuseki refused to compact '{dataset}': HTTP {(int)resp.StatusCode} {body}"));
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
                    if (DateTime.UtcNow - started > TimeSpan.FromMinutes(30)) throw new TimeoutException(UiText.Say($"La compattazione di '{dataset}' non è finita in 30 minuti.", $"Compacting '{dataset}' did not finish within 30 minutes."));
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
                    UiText.Say($"Jar di Fuseki non trovato: '{jar}'. Fa parte del pacchetto di MdExplorer (Binaries/jena-fuseki-server.jar): reinstalla MdExplorer.",
                        $"Fuseki jar not found: '{jar}'. It is part of the MdExplorer package (Binaries/jena-fuseki-server.jar): reinstall MdExplorer."));
            return jar;
        }

        private JavaProbe ResolveJavaOrThrow()
        {
            var javaPath = _javaLocator.ResolveJavaPath();
            var probe = _javaLocator.Probe(javaPath);
            if (_options.SearchAlternativeJava && probe.IsRunnable && probe.Major != null && probe.Major < _options.MinimumJavaMajor)
            {
                // L'impostazione JavaPath può puntare a un Java vecchio messo per PlantUML (Windows, 11/10/2026: un JRE 8 di
                // un altro programma, con Temurin 21 in JAVA_HOME). Fuseki cerca un Java adatto fra i candidati di sistema e
                // lo DICE nel log: l'impostazione non si tocca, la corregge l'utente.
                var other = _javaLocator.FindBest(_options.MinimumJavaMajor);
                if (other != null)
                {
                    _logger.LogWarning("[Fuseki] 'JavaPath' punta a Java {Old} ({OldPath}); per Fuseki uso Java {New} in {NewPath}. Imposta 'JavaPath' su un Java {Min} per togliere questo avviso.",
                        probe.Major, javaPath, other.Major, other.Path, _options.MinimumJavaMajor);
                    return other;
                }
            }
            if (!probe.IsRunnable)
                throw new FusekiUnavailableException(
                    UiText.Say(
                        $"java non eseguibile ('{javaPath}': {probe.Error}). Fuseki (grafo del codice e memoria) richiede Java {_options.MinimumJavaMajor}: " +
                        "installa Temurin 21 (adoptium.net) oppure imposta il percorso in 'JavaPath'.",
                        $"java cannot be run ('{javaPath}': {probe.Error}). Fuseki (code graph and memory) needs Java {_options.MinimumJavaMajor}: " +
                        "install Temurin 21 (adoptium.net) or set the path in 'JavaPath'."));
            if (probe.Major == null)
                throw new FusekiUnavailableException(
                    UiText.Say($"Versione di java non leggibile da '{javaPath}' («{probe.Raw}»). Fuseki richiede Java {_options.MinimumJavaMajor}.",
                        $"Cannot read the java version from '{javaPath}' («{probe.Raw}»). Fuseki needs Java {_options.MinimumJavaMajor}."));
            if (probe.Major < _options.MinimumJavaMajor)
                throw new FusekiUnavailableException(
                    UiText.Say(
                        $"Java {probe.Major} trovato in '{javaPath}' («{probe.Raw}»): MdExplorer richiede Java {_options.MinimumJavaMajor} per Fuseki " +
                        "(grafo del codice e memoria degli agenti). Installa Temurin 21 (adoptium.net) oppure imposta 'JavaPath' su un Java 21.",
                        $"Java {probe.Major} found in '{javaPath}' («{probe.Raw}»): MdExplorer needs Java {_options.MinimumJavaMajor} for Fuseki " +
                        "(code graph and agent memory). Install Temurin 21 (adoptium.net) or set 'JavaPath' to a Java 21."));
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
                    throw new InvalidOperationException(UiText.Say($"Fuseki è terminato durante l'avvio (exit {proc.ExitCode}).", $"Fuseki exited while starting (exit {proc.ExitCode})."));
                try
                {
                    var resp = await http.GetAsync(baseUri + "/$/ping", ct);
                    if (resp.IsSuccessStatusCode) return;
                }
                catch (HttpRequestException) { /* non ancora su */ }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested) { /* timeout del singolo ping */ }
                await Task.Delay(250, ct);
            }
            throw new TimeoutException(UiText.Say($"Fuseki non ha risposto su {baseUri}/$/ping entro {_options.StartupTimeout.TotalSeconds:0} secondi.", $"Fuseki did not answer on {baseUri}/$/ping within {_options.StartupTimeout.TotalSeconds:0} seconds."));
        }

        // ------------------------------------------------------------------ il Fuseki rimasto

        private string OwnerPath => Path.Combine(_options.DataDirectory, OwnerFileName);

        /// <summary>
        /// Prima di avviare: se nella cartella dati c'è ancora il Fuseki di un servizio morto, lo si ferma (è nostro e non
        /// serve a nessuno); se è di un altro MdExplorer ancora vivo, non si tocca e lo si dice.
        /// </summary>
        private void StopLeftoverOrThrow()
        {
            if (!File.Exists(OwnerPath)) return;
            FusekiOwner owner;
            try { owner = System.Text.Json.JsonSerializer.Deserialize<FusekiOwner>(File.ReadAllText(OwnerPath)); }
            catch (Exception ex) when (ex is System.Text.Json.JsonException or IOException)
            {
                throw new FusekiUnavailableException(UiText.Say($"File del proprietario di Fuseki illeggibile: '{OwnerPath}' ({ex.Message}). Chiudi MdExplorer, cancella il file e riapri.", $"Cannot read Fuseki's owner file: '{OwnerPath}' ({ex.Message}). Close MdExplorer, delete the file and open it again."), ex);
            }
            if (owner == null || !IsAlive(owner.FusekiPid, owner.FusekiStartTicks, out var fuseki))
            {
                File.Delete(OwnerPath);   // il Fuseki di allora non c'è più: niente da fare
                return;
            }
            using (fuseki)
            {
                var me = Process.GetCurrentProcess();
                var ownerIsMe = owner.OwnerPid == me.Id && SameStart(me, owner.OwnerStartTicks);
                if (!ownerIsMe && IsAlive(owner.OwnerPid, owner.OwnerStartTicks, out var other))
                {
                    other.Dispose();
                    throw new FusekiUnavailableException(
                        UiText.Say($"Il database del grafo in '{_options.DataDirectory}' è usato da un altro MdExplorer in esecuzione (processo {owner.OwnerPid}, Fuseki {owner.FusekiPid}). Chiudi quell'istanza e riprova.",
                            $"The graph database in '{_options.DataDirectory}' is used by another running MdExplorer (process {owner.OwnerPid}, Fuseki {owner.FusekiPid}). Close that instance and try again."));
                }
                _logger.LogWarning("[Fuseki] trovato il Fuseki {Pid} di un servizio terminato (processo {Owner}) che tiene il database: lo fermo",
                    owner.FusekiPid, owner.OwnerPid);
                try { fuseki.Kill(entireProcessTree: true); } catch (InvalidOperationException) { /* è uscito da solo intanto */ }
                if (!fuseki.WaitForExit(10_000))
                    throw new FusekiUnavailableException(UiText.Say($"Il Fuseki rimasto (processo {owner.FusekiPid}) non si ferma: terminalo a mano e riprova.", $"The leftover Fuseki (process {owner.FusekiPid}) does not stop: end it by hand and try again."));
            }
            File.Delete(OwnerPath);
        }

        private void WriteOwner(Process proc)
        {
            var me = Process.GetCurrentProcess();
            var owner = new FusekiOwner
            {
                FusekiPid = proc.Id, FusekiStartTicks = proc.StartTime.ToUniversalTime().Ticks,
                OwnerPid = me.Id, OwnerStartTicks = me.StartTime.ToUniversalTime().Ticks,
            };
            // scrittura atomica: un servizio che muore a metà non lascia un file illeggibile
            var tmp = OwnerPath + ".tmp";
            File.WriteAllText(tmp, System.Text.Json.JsonSerializer.Serialize(owner));
            File.Move(tmp, OwnerPath, overwrite: true);
        }

        private void DeleteOwnerIfMine(Process p)
        {
            try
            {
                if (!File.Exists(OwnerPath)) return;
                var owner = System.Text.Json.JsonSerializer.Deserialize<FusekiOwner>(File.ReadAllText(OwnerPath));
                if (owner?.FusekiPid == p.Id) File.Delete(OwnerPath);
            }
            catch (Exception ex) when (ex is System.Text.Json.JsonException or IOException or InvalidOperationException)
            {
                _logger.LogWarning(ex, "[Fuseki] file del proprietario non tolto: {Path}", OwnerPath);
            }
        }

        /// <summary>Vivo E lo stesso processo (stessa ora di avvio): un PID riusato da un altro programma non conta.</summary>
        private static bool IsAlive(int pid, long startTicks, out Process process)
        {
            process = null;
            Process p;
            try { p = Process.GetProcessById(pid); }
            catch (ArgumentException) { return false; }
            try
            {
                if (!p.HasExited && SameStart(p, startTicks)) { process = p; return true; }
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException) { }
            p.Dispose();
            return false;
        }

        private static bool SameStart(Process p, long startTicks)
            => Math.Abs(p.StartTime.ToUniversalTime().Ticks - startTicks) < TimeSpan.TicksPerSecond;

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
            DeleteOwnerIfMine(p);
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

    /// <summary>
    /// Windows: un Job Object con KILL_ON_JOB_CLOSE. L'handle resta aperto per tutta la vita del servizio; quando il
    /// servizio muore, in qualunque modo, Windows chiude l'handle e termina i processi del job (Fuseki). Altrove non serve:
    /// il file del proprietario ferma l'orfano all'avvio successivo.
    /// </summary>
    internal static class WindowsKillOnCloseJob
    {
        private static readonly object Gate = new();
        private static IntPtr _job = IntPtr.Zero;

        public static void Assign(Process process, ILogger logger)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
            lock (Gate)
            {
                if (_job == IntPtr.Zero)
                {
                    var job = CreateJobObject(IntPtr.Zero, null);
                    if (job == IntPtr.Zero) { logger.LogWarning("[Fuseki] CreateJobObject fallita (errore {Err}): Fuseki potrebbe sopravvivere al servizio", Marshal.GetLastWin32Error()); return; }
                    var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                    info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
                    var size = Marshal.SizeOf<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>();
                    var ptr = Marshal.AllocHGlobal(size);
                    try
                    {
                        Marshal.StructureToPtr(info, ptr, false);
                        if (!SetInformationJobObject(job, JobObjectExtendedLimitInformation, ptr, (uint)size))
                        {
                            logger.LogWarning("[Fuseki] SetInformationJobObject fallita (errore {Err}): Fuseki potrebbe sopravvivere al servizio", Marshal.GetLastWin32Error());
                            CloseHandle(job);
                            return;
                        }
                    }
                    finally { Marshal.FreeHGlobal(ptr); }
                    _job = job;
                }
                if (!AssignProcessToJobObject(_job, process.Handle))
                    logger.LogWarning("[Fuseki] AssignProcessToJobObject fallita (errore {Err}): Fuseki potrebbe sopravvivere al servizio", Marshal.GetLastWin32Error());
            }
        }

        private const int JobObjectExtendedLimitInformation = 9;
        private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount, ReadTransferCount, WriteTransferCount, OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetInformationJobObject(IntPtr hJob, int infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}
