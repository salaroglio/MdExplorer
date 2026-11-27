using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.AgenticEnvironments;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.SetupGuide
{
    /// <summary>Lo stato di un'installazione lanciata da «Che cosa installare».</summary>
    public sealed class InstallJobStatus
    {
        public string Tool { get; set; }
        public string PackageId { get; set; }
        /// <summary><c>running</c>, <c>succeeded</c>, <c>failed</c>.</summary>
        public string State { get; set; }
        /// <summary>L'ultima riga utile di winget, per far vedere a che punto è.</summary>
        public string LastLine { get; set; }
        public int? ExitCode { get; set; }
        /// <summary>Il pacchetto c'era già (winget lo dice con un codice suo).</summary>
        public bool AlreadyInstalled { get; set; }
        /// <summary>Il motivo, quando non è andata.</summary>
        public string Error { get; set; }
        /// <summary>Le ultime righe di winget, per capire che cosa è successo.</summary>
        public List<string> Tail { get; set; } = new();
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }

    /// <summary>
    /// «Installa adesso» della pagina «Che cosa installare» (27/11/2026, solo Windows): MDE lancia
    /// <c>winget install --id &lt;pacchetto&gt; -e --silent</c>, che scarica la versione giusta dal sito ufficiale, ne controlla
    /// l'impronta e la installa. Per git e Java Windows chiede l'autorizzazione (UAC): la deve dare l'utente.
    /// Una sola installazione per programma alla volta; lo stato si chiede con <see cref="Status"/>.
    /// </summary>
    public sealed class WingetInstaller
    {
        /// <summary>I pacchetti winget: la versione che serve a MDE (Java 21).</summary>
        public static readonly IReadOnlyDictionary<string, string> Packages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["git"] = "Git.Git",
            ["java"] = "EclipseAdoptium.Temurin.21.JDK",
            ["pandoc"] = "JohnMacFarlane.Pandoc",
        };

        /// <summary>winget: «il pacchetto è già installato» (APPINSTALLER_CLI_ERROR_PACKAGE_ALREADY_INSTALLED, 0x8A150061).</summary>
        public const int AlreadyInstalledExit = unchecked((int)0x8A150061);

        public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(20);
        private const int TailLines = 15;

        private readonly ConcurrentDictionary<string, InstallJobStatus> _jobs = new(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<WingetInstaller> _logger;

        public WingetInstaller(ILogger<WingetInstaller> logger) => _logger = logger;

        /// <summary>Gli argomenti di winget per un pacchetto. Pura: si prova senza Windows.</summary>
        public static IReadOnlyList<string> Arguments(string packageId) => new[]
        {
            "install", "--id", packageId, "-e", "--silent",
            "--accept-package-agreements", "--accept-source-agreements",
        };

        /// <summary>
        /// Dove sta winget: nel PATH del servizio, oppure nella cartella dove Windows mette i suoi alias delle app
        /// (<c>%LOCALAPPDATA%\Microsoft\WindowsApps</c>, che è dove winget vive davvero). Null se non c'è.
        /// </summary>
        public static string FindWinget()
        {
            if (!OperatingSystem.IsWindows()) return null;
            var onPath = ExecutableProbe.FindOnPath(Environment.GetEnvironmentVariable("PATH"), "winget.exe");
            if (onPath != null) return onPath;
            var local = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            if (string.IsNullOrEmpty(local)) return null;
            var alias = Path.Combine(local, "Microsoft", "WindowsApps", "winget.exe");
            return File.Exists(alias) ? alias : null;
        }

        /// <summary>
        /// Una riga di winget che vale la pena mostrare: niente righe vuote, niente spinner («-», «\», «|», «/»). Le barre di
        /// avanzamento («██████  45.0 MB / 60.0 MB») restano: dicono a che punto è il download. Pura.
        /// </summary>
        public static string Meaningful(string line)
        {
            var t = (line ?? string.Empty).Trim();
            if (t.Length == 0) return null;
            if (t.All(c => c == '-' || c == '\\' || c == '|' || c == '/' || char.IsWhiteSpace(c))) return null;
            return t;
        }

        public InstallJobStatus Status(string tool) => tool != null && _jobs.TryGetValue(tool, out var s) ? Snapshot(s) : null;

        /// <summary>
        /// Fa partire l'installazione e torna subito. Se per quel programma ce n'è già una in corso, restituisce quella.
        /// Fuori da Windows, senza winget o con un programma sconosciuto: <see cref="InvalidOperationException"/> col motivo.
        /// </summary>
        public InstallJobStatus Start(string tool)
        {
            if (!OperatingSystem.IsWindows())
                throw new InvalidOperationException(UiText.Say("L'installazione con un clic c'è solo su Windows: qui usa il comando della scheda.",
                    "One-click install is only on Windows: here use the command on the card."));
            if (tool == null || !Packages.TryGetValue(tool, out var packageId))
                throw new InvalidOperationException(UiText.Say($"Programma sconosciuto: «{tool}».", $"Unknown program: «{tool}»."));
            var winget = FindWinget() ?? throw new InvalidOperationException(UiText.Say(
                "winget non c'è su questo Windows (arriva con «Programma di installazione app» dal Microsoft Store): scarica il programma dal link della scheda.",
                "winget is not on this Windows (it comes with «App Installer» from the Microsoft Store): download the program from the card's link."));

            var job = new InstallJobStatus { Tool = tool.ToLowerInvariant(), PackageId = packageId, State = "running", StartedAt = DateTime.Now };
            var current = _jobs.AddOrUpdate(job.Tool, job, (_, old) => old.State == "running" ? old : job);
            if (!ReferenceEquals(current, job)) return Snapshot(current);

            _logger.LogInformation("[SetupGuide] installo {Tool} con winget ({Package}) da {Winget}", tool, packageId, winget);
            _ = Task.Run(() => RunAsync(job, winget));
            return Snapshot(job);
        }

        private async Task RunAsync(InstallJobStatus job, string winget)
        {
            try
            {
                var psi = CliCommand.BuildStartInfo(winget, Arguments(job.PackageId));
                psi.StandardOutputEncoding = Encoding.UTF8;
                psi.StandardErrorEncoding = Encoding.UTF8;
                using var process = new Process { StartInfo = psi };
                if (!process.Start()) throw new InvalidOperationException(UiText.Say("winget non è partito.", "winget did not start."));
                process.StandardInput.Close();
                using var cts = new CancellationTokenSource(Timeout);
                var readOut = ReadLinesAsync(process.StandardOutput, job);
                var readErr = ReadLinesAsync(process.StandardError, job);
                try
                {
                    await process.WaitForExitAsync(cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
                    Finish(job, null, UiText.Say($"winget non ha finito entro {Timeout.TotalMinutes:0} minuti: l'ho fermato.",
                        $"winget did not finish within {Timeout.TotalMinutes:0} minutes: I stopped it."));
                    return;
                }
                await Task.WhenAll(readOut, readErr).ConfigureAwait(false);
                var code = process.ExitCode;
                if (code == 0 || code == AlreadyInstalledExit)
                {
                    lock (job) job.AlreadyInstalled = code == AlreadyInstalledExit;
                    Finish(job, code, null);
                }
                else
                {
                    Finish(job, code, UiText.Say($"winget ha risposto con codice 0x{code:X8}: {job.LastLine ?? "nessuna uscita"}",
                        $"winget answered with code 0x{code:X8}: {job.LastLine ?? "no output"}"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SetupGuide] installazione di {Tool} fallita", job.Tool);
                Finish(job, null, ex.Message);
            }
        }

        private static async Task ReadLinesAsync(StreamReader reader, InstallJobStatus job)
        {
            // winget ridisegna la barra di avanzamento con «\r»: ogni pezzo fra \r e \n è uno stato.
            var buffer = new char[1024];
            var line = new StringBuilder();
            int n;
            while ((n = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                for (var i = 0; i < n; i++)
                {
                    var c = buffer[i];
                    if (c == '\r' || c == '\n') { Push(job, line.ToString(), c == '\n'); line.Clear(); }
                    else line.Append(c);
                }
            }
            Push(job, line.ToString(), true);
        }

        private static void Push(InstallJobStatus job, string raw, bool endOfLine)
        {
            var text = Meaningful(raw);
            if (text == null) return;
            lock (job)
            {
                job.LastLine = text;
                // Nella coda solo le righe vere: i ridisegni della barra la riempirebbero.
                if (endOfLine && (job.Tail.Count == 0 || job.Tail[^1] != text))
                {
                    job.Tail.Add(text);
                    if (job.Tail.Count > TailLines) job.Tail.RemoveAt(0);
                }
            }
        }

        private void Finish(InstallJobStatus job, int? exitCode, string error)
        {
            lock (job)
            {
                job.ExitCode = exitCode;
                job.Error = error;
                job.State = error == null ? "succeeded" : "failed";
                job.FinishedAt = DateTime.Now;
            }
            _logger.LogInformation("[SetupGuide] installazione di {Tool}: {State} (codice {Code}) {Error}", job.Tool, job.State, exitCode, error);
        }

        private static InstallJobStatus Snapshot(InstallJobStatus s)
        {
            lock (s)
                return new InstallJobStatus
                {
                    Tool = s.Tool, PackageId = s.PackageId, State = s.State, LastLine = s.LastLine, ExitCode = s.ExitCode,
                    AlreadyInstalled = s.AlreadyInstalled, Error = s.Error, Tail = s.Tail.ToList(), StartedAt = s.StartedAt, FinishedAt = s.FinishedAt,
                };
        }
    }
}
