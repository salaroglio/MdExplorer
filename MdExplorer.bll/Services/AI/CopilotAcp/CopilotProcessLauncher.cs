using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MdExplorer.Features.Services.AI.CopilotAcp
{
    /// <summary>
    /// Builds a <see cref="ProcessStartInfo"/> that invokes the locally installed
    /// <c>copilot</c> CLI. Centralises the Windows-specific shim resolution because
    /// <see cref="Process.Start(ProcessStartInfo)"/> with <c>UseShellExecute=false</c>
    /// does NOT honor PATHEXT — without this helper, a bare <c>"copilot"</c> filename
    /// fails on Windows installations where the shim is <c>copilot.cmd</c> or
    /// <c>copilot.ps1</c> (typical of npm-installed CLIs).
    /// <para>
    /// Resolution order, on Windows, scanning <c>PATH</c>:
    /// <list type="number">
    /// <item><description><c>copilot.exe</c> — used directly.</description></item>
    /// <item><description><c>copilot.cmd</c> — wrapped via <c>cmd.exe /d /s /c</c>.</description></item>
    /// <item><description><c>copilot.ps1</c> — wrapped via <c>powershell.exe</c> with
    ///   <c>-NoProfile -ExecutionPolicy Bypass -File</c>.</description></item>
    /// <item><description>Fallback: bare <c>"copilot"</c> handed to the OS.</description></item>
    /// </list>
    /// On non-Windows platforms, returns <c>FileName = "copilot"</c> unchanged
    /// (POSIX shells honor the shebang of an extension-less script).
    /// </para>
    /// </summary>
    public static class CopilotProcessLauncher
    {
        /// <summary>
        /// Returns <c>true</c> if a <c>copilot.exe</c>, <c>copilot.cmd</c> or <c>copilot.ps1</c>
        /// exists in <c>PATH</c>. Pure filesystem check, no process spawn — deterministic and
        /// instant (single-digit ms).
        /// <para>
        /// Used by <see cref="CopilotCliProvider.IsAvailable"/> in place of the previous
        /// <c>copilot --version</c> probe with 5-second timeout: that probe conflated
        /// "installed" with "starts within 5s" — Copilot CLI's cold start sometimes exceeds
        /// the timeout (e.g. when the CLI does its own update check) and MDE would conclude
        /// "not installed" on a perfectly installed system. The path-presence check is the
        /// answer to "is it installed?" — runtime startup time is a different concern.
        /// </para>
        /// </summary>
        public static bool IsResolvable()
        {
            if (!OperatingSystem.IsWindows())
            {
                // POSIX: rely on which-style probe? We don't have one here without spawn.
                // For now assume non-Windows path resolution is handled by the shell at
                // launch time. The bool answer for "is installed" returns true and lets
                // the actual launcher fail later if absent. Refine if a Linux user reports
                // a false positive.
                return true;
            }

            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var raw in pathEnv.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var dir = raw.Trim().Trim('"');
                if (dir.Length == 0) continue;
                if (File.Exists(Path.Combine(dir, "copilot.exe"))) return true;
                if (File.Exists(Path.Combine(dir, "copilot.cmd"))) return true;
                if (File.Exists(Path.Combine(dir, "copilot.ps1"))) return true;
            }
            return false;
        }

        /// <summary>
        /// Risolve il CLI nella forma che vuole l'SDK ufficiale: un eseguibile lanciabile
        /// direttamente più gli argomenti che devono precedere quelli dell'SDK
        /// (<c>RuntimeConnection.ForStdio(path, prefixArgs)</c> — verificato il 04/09/2026:
        /// quegli argomenti sono un <b>prefisso</b>, l'SDK accoda i propri).
        ///
        /// <para>
        /// Esiste separata da <see cref="BuildStartInfo"/> perché l'SDK non accetta una
        /// riga di comando ma una coppia percorso + argomenti; la <i>logica di ricerca</i>
        /// però è la stessa e resta qui, in un posto solo. Su Windows npm non installa un
        /// <c>copilot.exe</c> ma degli shim, e uno shim non si lancia direttamente: per
        /// quelli si passa da <c>cmd.exe /c</c> o da PowerShell, esattamente come fa
        /// <see cref="BuildStartInfo"/>.
        /// </para>
        ///
        /// <para>
        /// Volutamente <b>non</b> si ripiega sul CLI incluso nel pacchetto NuGet dell'SDK:
        /// MdExplorer ha sempre usato l'installazione dell'utente, che porta con sé la sua
        /// autenticazione. Usarne un'altra vorrebbe dire chiedergli di rifare il login per
        /// una funzione che prima andava.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">Se il CLI non si trova nel PATH.</exception>
        public static (string Path, List<string> PrefixArgs) ResolveStdioTarget()
        {
            if (!OperatingSystem.IsWindows())
            {
                // POSIX: "copilot" nel PATH è un eseguibile vero (loader npm con shebang).
                // Si verifica che ci sia davvero, invece di restituirlo alla cieca: così
                // l'errore dice "installalo" invece di un "No such file or directory" che
                // costringe chi legge a indovinare di cosa si parla.
                var posixPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                foreach (var raw in posixPath.Split(Path.PathSeparator))
                {
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    var candidate = Path.Combine(raw.Trim(), "copilot");
                    if (File.Exists(candidate)) return (candidate, new List<string>());
                }
                throw new InvalidOperationException(
                    "Copilot CLI non trovato nel PATH. Installalo con 'npm install -g @github/copilot'.");
            }

            string exeMatch = null, cmdMatch = null, ps1Match = null;
            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var raw in pathEnv.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var dir = raw.Trim().Trim('"');
                if (dir.Length == 0) continue;
                if (exeMatch == null && File.Exists(Path.Combine(dir, "copilot.exe"))) exeMatch = Path.Combine(dir, "copilot.exe");
                if (cmdMatch == null && File.Exists(Path.Combine(dir, "copilot.cmd"))) cmdMatch = Path.Combine(dir, "copilot.cmd");
                if (ps1Match == null && File.Exists(Path.Combine(dir, "copilot.ps1"))) ps1Match = Path.Combine(dir, "copilot.ps1");
                if (exeMatch != null) break; // l'eseguibile vero vince
            }

            if (exeMatch != null) return (exeMatch, new List<string>());

            if (cmdMatch != null)
            {
                var comspec = Environment.GetEnvironmentVariable("ComSpec");
                if (string.IsNullOrEmpty(comspec)) comspec = "cmd.exe";
                return (comspec, new List<string> { "/d", "/s", "/c", cmdMatch });
            }

            if (ps1Match != null)
            {
                return ("powershell.exe",
                    new List<string> { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", ps1Match });
            }

            throw new InvalidOperationException(
                "Copilot CLI executable not found in PATH. Install it via 'winget install GitHub.Copilot' " +
                "or 'npm install -g @github/copilot'. Looked for copilot.exe, copilot.cmd, copilot.ps1.");
        }

        /// <summary>
        /// Builds a <see cref="ProcessStartInfo"/> that runs <c>copilot</c> with the given args.
        /// Caller is responsible for additional settings (RedirectStandardOutput, WorkingDirectory, ...).
        /// </summary>
        public static ProcessStartInfo BuildStartInfo(string copilotArgs)
        {
            if (!OperatingSystem.IsWindows())
            {
                return new ProcessStartInfo { FileName = "copilot", Arguments = copilotArgs ?? string.Empty };
            }

            string exeMatch = null;
            string cmdMatch = null;
            string ps1Match = null;
            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var raw in pathEnv.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var dir = raw.Trim().Trim('"');
                if (dir.Length == 0) continue;
                if (exeMatch == null)
                {
                    var p = Path.Combine(dir, "copilot.exe");
                    if (File.Exists(p)) exeMatch = p;
                }
                if (cmdMatch == null)
                {
                    var p = Path.Combine(dir, "copilot.cmd");
                    if (File.Exists(p)) cmdMatch = p;
                }
                if (ps1Match == null)
                {
                    var p = Path.Combine(dir, "copilot.ps1");
                    if (File.Exists(p)) ps1Match = p;
                }
                if (exeMatch != null) break; // .exe wins; stop scanning
            }

            var args = copilotArgs ?? string.Empty;

            if (exeMatch != null)
            {
                return new ProcessStartInfo { FileName = exeMatch, Arguments = args };
            }

            if (cmdMatch != null)
            {
                // cmd.exe /d /s /c "<argstring>" — with /s, the first and last quote of
                // the argstring are the delimiters, so we wrap the whole "<cmdPath> <args>"
                // in one set of outer quotes and double-quote the path within.
                var comspec = Environment.GetEnvironmentVariable("ComSpec");
                if (string.IsNullOrEmpty(comspec)) comspec = "cmd.exe";
                var inner = string.IsNullOrEmpty(args) ? $"\"{cmdMatch}\"" : $"\"{cmdMatch}\" {args}";
                return new ProcessStartInfo
                {
                    FileName = comspec,
                    Arguments = $"/d /s /c \"{inner}\""
                };
            }

            if (ps1Match != null)
            {
                // powershell.exe -NoProfile -ExecutionPolicy Bypass -File <ps1Path> <args>
                // -File treats the rest of the line as script arguments; quote the path.
                var pwshArgs = string.IsNullOrEmpty(args)
                    ? $"-NoProfile -ExecutionPolicy Bypass -File \"{ps1Match}\""
                    : $"-NoProfile -ExecutionPolicy Bypass -File \"{ps1Match}\" {args}";
                return new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = pwshArgs
                };
            }

            throw new InvalidOperationException(
                "Copilot CLI executable not found in PATH. Install it via 'winget install GitHub.Copilot' " +
                "or 'npm install -g @github/copilot'. Looked for copilot.exe, copilot.cmd, copilot.ps1.");
        }
    }
}
