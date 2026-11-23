using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Diagnostics
{
    /// <summary>Un processo di MdExplorer in un campione: quanto processore e quanta memoria sta usando.</summary>
    public sealed class ProcessSample
    {
        public int Pid { get; set; }
        public string Name { get; set; }
        /// <summary>Percentuale di TUTTA la macchina, come la colonna «CPU» di Gestione attività.</summary>
        public double CpuPercent { get; set; }
        /// <summary>Percentuale di UN core: 100 = un core tutto occupato (un ciclo che non si ferma).</summary>
        public double CorePercent { get; set; }
        public long MemoryMb { get; set; }
    }

    public sealed class ResourceSample
    {
        public DateTime AtUtc { get; set; }
        /// <summary>Somma dei processi di MdExplorer, in percentuale della macchina.</summary>
        public double CpuPercent { get; set; }
        /// <summary>Quanta memoria della macchina è occupata, da tutti i programmi (0-100).</summary>
        public int MachineMemoryLoadPercent { get; set; }
        public List<ProcessSample> Processes { get; set; } = new List<ProcessSample>();
    }

    /// <summary>La finestra non ha risposto: quanto a lungo, e quanto ha aspettato un tasto prima di comparire.</summary>
    public sealed class UiStallReport
    {
        public DateTime AtUtc { get; set; }
        /// <summary>Quante volte la finestra si è bloccata nell'intervallo.</summary>
        public int Count { get; set; }
        public int MaxMs { get; set; }
        public int TotalMs { get; set; }
        /// <summary>Il ritardo peggiore fra un tasto premuto e il ridisegno della finestra.</summary>
        public int MaxKeyMs { get; set; }
        public int SlowKeys { get; set; }
        public int WindowSeconds { get; set; }
        /// <summary>La pagina dell'applicazione (il percorso della rotta, senza dati dell'utente).</summary>
        public string Route { get; set; }
    }

    /// <summary>
    /// Quanto processore e quanta memoria usano i processi di MdExplorer (il servizio, le finestre di Electron, Java con
    /// Fuseki, il server MCP, il motore locale), campionati ogni 15 secondi, e i blocchi che la finestra segnala da sé.
    /// Serve a «Invia diagnostica»: i registri dicono che cosa il servizio stava facendo, non chi occupava la macchina
    /// né se la finestra rispondeva (23/11/2026: un PC lento dove non si riusciva a scrivere a MarkAgent).
    /// <para>Scrive nel registro quando c'è qualcosa da dire (carico alto, finestra bloccata) e un battito ogni dieci
    /// minuti; l'ultima ora di campioni va nel pacchetto come <c>risorse.json</c>.</para>
    /// </summary>
    public sealed class ResourceMonitor : BackgroundService
    {
        public static readonly TimeSpan SampleEvery = TimeSpan.FromSeconds(15);
        private const int Kept = 240;                       // un'ora
        private const double BusyMachinePercent = 15;       // i processi di MDE insieme, sulla macchina
        private const double BusyCorePercent = 90;          // un processo che tiene occupato un core intero
        private static readonly TimeSpan BusyLogEvery = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan HeartbeatEvery = TimeSpan.FromMinutes(10);
        private static readonly string[] OtherNames = { "java", "javaw", "llama-server" };

        private readonly ILogger<ResourceMonitor> _logger;
        private readonly object _gate = new object();
        private readonly Queue<ResourceSample> _samples = new Queue<ResourceSample>();
        private readonly Queue<UiStallReport> _stalls = new Queue<UiStallReport>();
        private readonly Dictionary<int, (TimeSpan Cpu, DateTime At)> _previous = new Dictionary<int, (TimeSpan, DateTime)>();
        private DateTime _lastLog = DateTime.MinValue;

        public ResourceMonitor(ILogger<ResourceMonitor> logger) => _logger = logger;

        public static long MachineMemoryMb => GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[Risorse] macchina: {Cores} processori logici, {Ram} MB di memoria.", Environment.ProcessorCount, MachineMemoryMb);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var sample = Sample();
                    if (sample != null) Log(sample);
                }
                catch (Exception ex)
                {
                    // Misurare non deve mai rompere il servizio: lo si dice una volta ogni tanto e si continua.
                    _logger.LogDebug(ex, "[Risorse] campione non riuscito");
                }
                try { await Task.Delay(SampleEvery, stoppingToken); } catch (OperationCanceledException) { return; }
            }
        }

        /// <summary>Un campione adesso. Il primo dopo l'avvio non ha un «prima» con cui fare la differenza: torna null.</summary>
        public ResourceSample Sample()
        {
            var now = DateTime.UtcNow;
            var self = Environment.ProcessId;
            var sample = new ResourceSample { AtUtc = now, MachineMemoryLoadPercent = MemoryLoadPercent() };
            var seen = new HashSet<int>();
            var first = _previous.Count == 0;

            foreach (var process in Process.GetProcesses())
            {
                using (process)
                {
                    try
                    {
                        var name = process.ProcessName ?? string.Empty;
                        if (process.Id != self && !IsOurs(name)) continue;
                        var cpu = process.TotalProcessorTime;
                        seen.Add(process.Id);
                        if (_previous.TryGetValue(process.Id, out var before) && now > before.At)
                        {
                            var core = (cpu - before.Cpu).TotalMilliseconds / (now - before.At).TotalMilliseconds * 100;
                            sample.Processes.Add(new ProcessSample
                            {
                                Pid = process.Id,
                                Name = process.Id == self ? name + " (servizio)" : name,
                                CorePercent = Math.Round(Math.Max(0, core), 1),
                                CpuPercent = Math.Round(Math.Max(0, core) / Environment.ProcessorCount, 1),
                                MemoryMb = process.WorkingSet64 / (1024 * 1024),
                            });
                        }
                        _previous[process.Id] = (cpu, now);
                    }
                    catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is NotSupportedException)
                    {
                        // Uscito nel frattempo, o di un altro utente: non lo si può leggere, non è un errore.
                    }
                }
            }
            foreach (var gone in _previous.Keys.Where(pid => !seen.Contains(pid)).ToList()) _previous.Remove(gone);
            if (first) return null;

            sample.Processes = sample.Processes.OrderByDescending(p => p.CpuPercent).ThenByDescending(p => p.MemoryMb).ToList();
            sample.CpuPercent = Math.Round(sample.Processes.Sum(p => p.CpuPercent), 1);
            lock (_gate)
            {
                _samples.Enqueue(sample);
                while (_samples.Count > Kept) _samples.Dequeue();
            }
            return sample;
        }

        public static bool IsOurs(string processName)
            => processName.StartsWith("mdexplorer", StringComparison.OrdinalIgnoreCase)
               || OtherNames.Contains(processName, StringComparer.OrdinalIgnoreCase);

        private static int MemoryLoadPercent()
        {
            var info = GC.GetGCMemoryInfo();
            return info.TotalAvailableMemoryBytes <= 0 ? 0 : (int)Math.Round(100.0 * info.MemoryLoadBytes / info.TotalAvailableMemoryBytes);
        }

        private void Log(ResourceSample sample)
        {
            var busy = sample.CpuPercent >= BusyMachinePercent || sample.Processes.Any(p => p.CorePercent >= BusyCorePercent);
            var since = DateTime.UtcNow - _lastLog;
            if (busy ? since < BusyLogEvery : since < HeartbeatEvery) return;
            _lastLog = DateTime.UtcNow;
            _logger.Log(busy ? LogLevel.Warning : LogLevel.Information,
                "[Risorse] {What}: MdExplorer usa il {Cpu}% della macchina ({Cores} processori logici), memoria della macchina occupata al {Mem}%. {Processes}",
                busy ? "carico alto" : "a riposo", sample.CpuPercent, Environment.ProcessorCount, sample.MachineMemoryLoadPercent, Describe(sample));
        }

        /// <summary>«mdexplorer#1234 7,2% (115% di un core) 402 MB; …»: i processi che lavorano, o i cinque più pesanti.</summary>
        public static string Describe(ResourceSample sample)
        {
            var shown = sample.Processes.Where(p => p.CpuPercent >= 0.5).DefaultIfEmpty().Where(p => p != null).ToList();
            if (shown.Count == 0) shown = sample.Processes.OrderByDescending(p => p.MemoryMb).Take(5).ToList();
            return string.Join("; ", shown.Select(p => $"{p.Name}#{p.Pid} {p.CpuPercent:0.#}% ({p.CorePercent:0}% di un core) {p.MemoryMb} MB"));
        }

        /// <summary>La finestra dice di essersi bloccata: nel registro subito, con ciò che i processi facevano in quel momento.</summary>
        public void ReportUiStall(UiStallReport report)
        {
            if (report == null) return;
            report.AtUtc = DateTime.UtcNow;
            ResourceSample latest;
            lock (_gate)
            {
                _stalls.Enqueue(report);
                while (_stalls.Count > Kept) _stalls.Dequeue();
                latest = _samples.LastOrDefault();
            }
            _logger.LogWarning(
                "[Risorse] la finestra non rispondeva: {Count} blocchi negli ultimi {Seconds} s, il più lungo {Max} ms ({Total} ms in tutto); " +
                "tasti lenti a comparire: {Keys}, il peggiore {KeyMax} ms; pagina {Route}. In quel momento: {Processes}",
                report.Count, report.WindowSeconds, report.MaxMs, report.TotalMs, report.SlowKeys, report.MaxKeyMs, report.Route ?? "?",
                latest == null ? "nessun campione ancora" : $"MdExplorer al {latest.CpuPercent}% della macchina, memoria al {latest.MachineMemoryLoadPercent}%. {Describe(latest)}");
        }

        /// <summary>L'ultima ora, per il pacchetto di «Invia diagnostica» (<c>risorse.json</c>).</summary>
        public string SnapshotJson()
        {
            ResourceSample[] samples;
            UiStallReport[] stalls;
            lock (_gate) { samples = _samples.ToArray(); stalls = _stalls.ToArray(); }
            return JsonSerializer.Serialize(new
            {
                processoriLogici = Environment.ProcessorCount,
                memoriaDellaMacchinaMb = MachineMemoryMb,
                campionatoOgniSecondi = (int)SampleEvery.TotalSeconds,
                nota = "cpuPercent = percentuale della macchina (come Gestione attività); corePercent = percentuale di un core, 100 = un core tutto occupato.",
                finestraBloccata = stalls,
                campioni = samples,
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
