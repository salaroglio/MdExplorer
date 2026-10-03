using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.AI.AgenticEnvironments
{
    /// <summary>Esito di un comando breve lanciato su un CLI.</summary>
    public sealed class CliCommandResult
    {
        public int ExitCode { get; set; }
        public string Stdout { get; set; }
        public string Stderr { get; set; }
    }

    /// <summary>
    /// Lancia un comando breve su un CLI già risolto nel PATH e ne raccoglie l'uscita.
    /// <para>
    /// Su Windows npm non installa un eseguibile ma degli shim, e <see cref="Process.Start(ProcessStartInfo)"/>
    /// con <c>UseShellExecute=false</c> non sa lanciare uno shim: un <c>.cmd</c> passa da
    /// <c>cmd.exe</c>, un <c>.ps1</c> da PowerShell — la stessa regola dei lanciatori dei tre CLI.
    /// </para>
    /// </summary>
    public static class CliCommand
    {
        public static ProcessStartInfo BuildStartInfo(string resolvedPath, IReadOnlyList<string> arguments)
        {
            if (string.IsNullOrWhiteSpace(resolvedPath))
                throw new ArgumentException("Serve il percorso del CLI, già risolto nel PATH.", nameof(resolvedPath));

            var psi = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            var extension = Path.GetExtension(resolvedPath).ToLowerInvariant();
            if (OperatingSystem.IsWindows() && (extension == ".cmd" || extension == ".bat"))
            {
                var comspec = Environment.GetEnvironmentVariable("ComSpec");
                psi.FileName = string.IsNullOrEmpty(comspec) ? "cmd.exe" : comspec;
                psi.ArgumentList.Add("/d");
                psi.ArgumentList.Add("/s");
                psi.ArgumentList.Add("/c");
                psi.ArgumentList.Add(resolvedPath);
            }
            else if (OperatingSystem.IsWindows() && extension == ".ps1")
            {
                psi.FileName = "powershell.exe";
                psi.ArgumentList.Add("-NoProfile");
                psi.ArgumentList.Add("-ExecutionPolicy");
                psi.ArgumentList.Add("Bypass");
                psi.ArgumentList.Add("-File");
                psi.ArgumentList.Add(resolvedPath);
            }
            else
            {
                psi.FileName = resolvedPath;
            }

            foreach (var argument in arguments ?? Array.Empty<string>()) psi.ArgumentList.Add(argument);
            return psi;
        }

        /// <summary>
        /// Lancia il comando e aspetta che finisca. Alla cancellazione il processo viene chiuso con
        /// tutti i suoi figli, e l'<see cref="OperationCanceledException"/> risale a chi ha chiamato.
        /// </summary>
        public static async Task<CliCommandResult> RunAsync(string resolvedPath, IReadOnlyList<string> arguments, CancellationToken ct)
        {
            using var process = new Process { StartInfo = BuildStartInfo(resolvedPath, arguments) };
            if (!process.Start())
                throw new InvalidOperationException($"Il processo '{resolvedPath}' non è partito.");

            // Nessuno scrive su stdin: va chiuso, altrimenti un CLI che lo legge resta in attesa.
            process.StandardInput.Close();

            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            try
            {
                await process.WaitForExitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) { /* è uscito da solo fra il controllo e la chiusura */ }
                throw;
            }

            return new CliCommandResult
            {
                ExitCode = process.ExitCode,
                Stdout = await stdout.ConfigureAwait(false),
                Stderr = await stderr.ConfigureAwait(false),
            };
        }
    }
}
