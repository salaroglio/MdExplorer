using System;
using System.Diagnostics;
using System.IO;

namespace MdExplorer.Features.Services.AI.ClaudeCode
{
    /// <summary>
    /// Risolve il binario <c>claude</c> (Claude Code CLI) e costruisce il
    /// <see cref="ProcessStartInfo"/> per lanciarlo.
    /// <para>
    /// Gemello di <c>CopilotProcessLauncher</c> ma <b>non</b> una sua copia: dove quello, su
    /// Linux, restituisce <c>true</c> a scatola chiusa e lascia fallire il lancio, qui la
    /// risoluzione è una scansione vera del PATH su <b>tutte</b> le piattaforme. Il motivo è
    /// la regola "niente fallback silenziosi": un `IsResolvable()` che risponde sempre sì
    /// trasforma "Claude Code non è installato" in un errore di avvio processo generico,
    /// mille righe più in là.
    /// </para>
    /// <para>
    /// ⚠️ <b>Windows non è stato verificato</b> (fase F0 del piano
    /// <c>docs-internal/Sprints/2026-08-28-Claude-Code-Come-Motore-Chat.md</c>): l'ordine di
    /// risoluzione qui sotto è modellato su quello di Copilot — <c>.exe</c>, poi lo shim
    /// <c>.cmd</c> di npm, poi <c>.ps1</c> — perché <see cref="Process.Start(ProcessStartInfo)"/>
    /// con <c>UseShellExecute=false</c> NON onora PATHEXT. Se su Windows Claude Code si
    /// installa in un modo che questa scansione non copre, il metodo <b>fallisce forte</b> con
    /// un messaggio che dice cosa cercare, invece di provare <c>"claude"</c> e sperare.
    /// </para>
    /// </summary>
    public static class ClaudeCodeProcessLauncher
    {
        /// <summary>Nome dell'eseguibile, senza estensione.</summary>
        public const string ExecutableName = "claude";

        /// <summary>
        /// Percorso assoluto del binario risolto, oppure <c>null</c> se non è nel PATH.
        /// Pura ispezione del filesystem: nessun processo lanciato, nessun timeout —
        /// "è installato?" e "parte entro N secondi?" sono due domande diverse, e
        /// confonderle è già costato una diagnosi sbagliata sul lato Copilot.
        /// </summary>
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
                    catch (ArgumentException) { continue; } // path con caratteri illegali nel PATH
                    if (File.Exists(full)) return full;
                }
            }
            return null;
        }

        /// <summary>
        /// <c>true</c> se il CLI è risolvibile nel PATH. Non dice nulla sull'autenticazione:
        /// quella si scopre al primo turno (il CLI non emette nulla finché non riceve un
        /// messaggio — verificato).
        /// </summary>
        public static bool IsResolvable() => ResolvePath() != null;

        /// <summary>
        /// Costruisce lo start info per <c>claude</c> con gli argomenti dati.
        /// Fallisce forte se il binario non è nel PATH.
        /// </summary>
        /// <param name="arguments">Argomenti già formattati (spazio-separati).</param>
        public static ProcessStartInfo BuildStartInfo(string arguments)
        {
            var resolved = ResolvePath();
            if (resolved == null)
            {
                throw new InvalidOperationException(
                    "Claude Code CLI non trovato nel PATH. Attesi: " +
                    (OperatingSystem.IsWindows()
                        ? "claude.exe, claude.cmd o claude.ps1"
                        : "claude") +
                    ". Installalo con `npm i -g @anthropic-ai/claude-code` e verifica con `claude --version`.");
            }

            if (OperatingSystem.IsWindows())
            {
                var ext = Path.GetExtension(resolved).ToLowerInvariant();
                if (ext == ".cmd" || ext == ".bat")
                {
                    // cmd.exe è obbligatorio: Process.Start non sa eseguire uno script batch.
                    return new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/d /s /c \"\"{resolved}\" {arguments}\""
                    };
                }
                if (ext == ".ps1")
                {
                    return new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{resolved}\" {arguments}"
                    };
                }
            }

            return new ProcessStartInfo
            {
                FileName = resolved,
                Arguments = arguments
            };
        }
    }
}
