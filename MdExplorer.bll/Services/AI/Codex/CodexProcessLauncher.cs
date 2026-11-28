using System;
using System.Diagnostics;
using System.IO;

namespace MdExplorer.Features.Services.AI.Codex
{
    /// <summary>
    /// Risolve il binario <c>codex</c> e costruisce il <see cref="ProcessStartInfo"/> di <c>codex app-server</c>.
    /// <para>
    /// Stessa forma di <c>ClaudeCodeProcessLauncher</c> e <c>OpenCodeProcessLauncher</c>: la scansione del PATH è vera su
    /// tutte le piattaforme, così «Codex non è installato» si scopre subito e con quel nome. Su Windows l'ordine dei
    /// candidati è <c>.exe</c>, lo shim <c>.cmd</c> di npm, poi <c>.ps1</c>, perché <see cref="Process.Start(ProcessStartInfo)"/>
    /// con <c>UseShellExecute=false</c> non onora PATHEXT (⚠️ Windows non ancora provato: F8).
    /// </para>
    /// <para>Misurato il 28/11/2026 su Linux: <c>npm i -g @openai/codex@0.172.0</c> mette il binario in
    /// <c>~/.nvm/versions/node/&lt;v&gt;/bin/codex</c>.</para>
    /// <para>Sprint: docs-internal/Sprints/2026-11-28-Motore-Codex.md, fase F1.</para>
    /// </summary>
    public static class CodexProcessLauncher
    {
        /// <summary>Nome dell'eseguibile, senza estensione.</summary>
        public const string ExecutableName = "codex";

        /// <summary>
        /// La cartella in cui Codex tiene accesso, configurazione e conversazioni (predefinita <c>~/.codex</c>).
        /// Il servizio di prova la isola come fa con <c>HOME</c> (D9).
        /// </summary>
        public const string HomeVariable = "CODEX_HOME";

        /// <summary>Il comando con cui si installa: lo mostrano gli errori e la guida d'installazione.</summary>
        public const string InstallCommand = "npm install -g @openai/codex";

        /// <summary>Percorso assoluto del binario risolto, oppure <c>null</c>. Nessun processo lanciato.</summary>
        public static string ResolvePath()
        {
            var candidates = OperatingSystem.IsWindows()
                ? new[] { ExecutableName + ".exe", ExecutableName + ".cmd", ExecutableName + ".ps1" }
                : new[] { ExecutableName };

            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var raw in pathEnv.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var dir = raw.Trim().Trim('"');
                if (dir.Length == 0) continue;

                foreach (var candidate in candidates)
                {
                    string full;
                    try { full = Path.Combine(dir, candidate); }
                    catch (ArgumentException) { continue; } // PATH con caratteri illegali
                    if (File.Exists(full)) return full;
                }
            }
            return null;
        }

        /// <summary><c>true</c> se il CLI è risolvibile nel PATH. Non dice niente sull'accesso.</summary>
        public static bool IsResolvable() => ResolvePath() != null;

        /// <summary>L'errore di «Codex non trovato», lo stesso per tutti i chiamanti.</summary>
        public static InvalidOperationException NotInstalled() => new InvalidOperationException(UiText.Say(
            $"Codex non è installato su questa macchina, o non è nel PATH del servizio. Cercato '{ExecutableName}' in ogni " +
            $"cartella del PATH. Installalo con `{InstallCommand}` e accedi con `codex login`, oppure scegli un altro motore " +
            "per MarkAgent nelle impostazioni del progetto.",
            $"Codex is not installed on this machine, or it is not in the service PATH. Looked for '{ExecutableName}' in every " +
            $"PATH folder. Install it with `{InstallCommand}` and sign in with `codex login`, or choose another MarkAgent " +
            "engine in the project settings."));

        /// <summary>
        /// Start info per <c>codex</c> con gli argomenti dati (per esempio <c>app-server</c>, oppure
        /// <c>login status</c>). stdin/stdout/stderr li redirige il chiamante.
        /// </summary>
        public static ProcessStartInfo BuildStartInfo(params string[] arguments)
        {
            var resolved = ResolvePath() ?? throw NotInstalled();

            if (OperatingSystem.IsWindows())
            {
                var ext = Path.GetExtension(resolved).ToLowerInvariant();
                var joined = string.Join(" ", arguments);
                if (ext == ".cmd" || ext == ".bat")
                {
                    // cmd.exe è obbligatorio: Process.Start non sa eseguire uno script batch.
                    return new ProcessStartInfo { FileName = "cmd.exe", Arguments = $"/d /s /c \"\"{resolved}\" {joined}\"" };
                }
                if (ext == ".ps1")
                {
                    return new ProcessStartInfo { FileName = "powershell.exe", Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{resolved}\" {joined}" };
                }
            }

            var psi = new ProcessStartInfo { FileName = resolved };
            foreach (var argument in arguments) psi.ArgumentList.Add(argument);
            return psi;
        }
    }
}
