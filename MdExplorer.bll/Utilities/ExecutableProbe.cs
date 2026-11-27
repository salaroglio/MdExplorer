using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI.AgenticEnvironments;

namespace MdExplorer.Features.Utilities
{
    /// <summary>
    /// Che cosa sa MDE di un programma che lancia per nome dal PATH, come Pandoc e git (pagina «Che cosa installare»,
    /// 19/11/2026; git dal 27/11/2026).
    /// </summary>
    public sealed class ExecutableStatus
    {
        /// <summary>Il servizio lo trova nel suo PATH e risponde a <c>--version</c>.</summary>
        public bool Usable { get; set; }
        /// <summary>
        /// Installato DOPO l'avvio di MDE: sta nel PATH di Windows di adesso, non in quello che il servizio ha ricevuto
        /// all'avvio. MDE lo lancia per nome, quindi lo troverà solo dal prossimo avvio.
        /// </summary>
        public bool NeedsRestart { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        /// <summary>Il motivo, quando c'è ma non risponde come dovrebbe.</summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// La sonda di un programma che MDE lancia per nome (<c>pandoc …</c>, <c>git …</c>): conta il PATH del servizio. Lo cerca lì,
    /// su Windows anche nel PATH di adesso (registro), e gli chiede <c>--version</c> entro <see cref="Timeout"/>.
    /// </summary>
    public static class ExecutableProbe
    {
        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        /// <summary>Il primo <paramref name="executableName"/> nelle cartelle di <paramref name="pathValue"/>, o null. Pura: si prova senza il programma.</summary>
        public static string FindOnPath(string pathValue, string executableName)
        {
            foreach (var raw in (pathValue ?? string.Empty).Split(System.IO.Path.PathSeparator))
            {
                var dir = raw.Trim().Trim('"');
                if (dir.Length == 0) continue;
                string full;
                try { full = System.IO.Path.Combine(dir, executableName); }
                catch (ArgumentException) { continue; } // caratteri non ammessi in una voce del PATH
                if (File.Exists(full)) return full;
            }
            return null;
        }

        public static async Task<ExecutableStatus> ProbeAsync(string executableName, Func<string, string> parseVersion, CancellationToken ct)
        {
            var path = FindOnPath(Environment.GetEnvironmentVariable("PATH"), executableName);
            if (path == null)
            {
                // Su Windows il PATH di adesso sta nel registro (computer + utente): gli installer lo aggiornano lì, e il
                // servizio, partito prima, non lo vede.
                if (OperatingSystem.IsWindows())
                {
                    var fresh = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) + System.IO.Path.PathSeparator
                              + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                    var later = FindOnPath(fresh, executableName);
                    if (later != null) return new ExecutableStatus { NeedsRestart = true, Path = later };
                }
                return new ExecutableStatus();
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(Timeout);
            try
            {
                var result = await CliCommand.RunAsync(path, new[] { "--version" }, timeout.Token).ConfigureAwait(false);
                var version = parseVersion(result.Stdout);
                if (result.ExitCode == 0 && version != null)
                    return new ExecutableStatus { Usable = true, Path = path, Version = version };
                return new ExecutableStatus
                {
                    Path = path,
                    Error = UiText.Say(
                        $"'{path} --version' ha risposto con codice {result.ExitCode}: {FirstLine(result.Stderr) ?? FirstLine(result.Stdout) ?? "nessuna uscita"}",
                        $"'{path} --version' answered with code {result.ExitCode}: {FirstLine(result.Stderr) ?? FirstLine(result.Stdout) ?? "no output"}"),
                };
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                return new ExecutableStatus
                {
                    Path = path,
                    Error = UiText.Say($"'{path} --version' non ha risposto entro {Timeout.TotalSeconds:0} secondi.", $"'{path} --version' did not answer within {Timeout.TotalSeconds:0} seconds."),
                };
            }
        }

        private static string FirstLine(string text)
        {
            var line = (text ?? string.Empty).Trim().Split('\n')[0].Trim();
            return line.Length == 0 ? null : line;
        }
    }

    /// <summary>
    /// Pandoc fa gli export in Word: MDE lo lancia per nome (<c>pandoc …</c>), quindi conta il PATH del servizio.
    /// </summary>
    public static class PandocProbe
    {
        private static readonly Regex VersionLine = new Regex(@"^pandoc(?:\.exe)?\s+(\d+(?:\.\d+)+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        /// <summary>Il nome dell'eseguibile sul sistema in uso.</summary>
        public static string ExecutableName => OperatingSystem.IsWindows() ? "pandoc.exe" : "pandoc";

        /// <inheritdoc cref="ExecutableProbe.FindOnPath"/>
        public static string FindOnPath(string pathValue, string executableName) => ExecutableProbe.FindOnPath(pathValue, executableName);

        /// <summary>La versione dalla prima riga di <c>pandoc --version</c> («pandoc 3.1.11»), o null. Pura.</summary>
        public static string ParseVersion(string stdout)
        {
            var m = VersionLine.Match(stdout ?? string.Empty);
            return m.Success ? m.Groups[1].Value : null;
        }

        public static Task<ExecutableStatus> ProbeAsync(CancellationToken ct) => ExecutableProbe.ProbeAsync(ExecutableName, ParseVersion, ct);
    }

    /// <summary>
    /// git: MDE lo lancia per nome (<c>NativeGitRunner</c>, FileName «git») per sincronizzare, registrare e pubblicare, quindi
    /// conta il PATH del servizio, come per Pandoc.
    /// </summary>
    public static class GitProbe
    {
        // «git version 2.43.0», «git version 2.45.1.windows.1», «git version 2.39.3 (Apple Git-145)»
        private static readonly Regex VersionLine = new Regex(@"^git version\s+(\d+(?:\.\d+)+)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        public static string ExecutableName => OperatingSystem.IsWindows() ? "git.exe" : "git";

        /// <summary>La versione da <c>git --version</c>, o null. Pura.</summary>
        public static string ParseVersion(string stdout)
        {
            var m = VersionLine.Match(stdout ?? string.Empty);
            return m.Success ? m.Groups[1].Value : null;
        }

        public static Task<ExecutableStatus> ProbeAsync(CancellationToken ct) => ExecutableProbe.ProbeAsync(ExecutableName, ParseVersion, ct);
    }
}
