using System;
using System.Diagnostics;
using System.IO;

namespace MdExplorer.Features.Services.AI.OpenCode
{
    /// <summary>
    /// Risolve il binario <c>opencode</c> e costruisce il <see cref="ProcessStartInfo"/> per
    /// avviarne il server.
    /// <para>
    /// Stessa forma di <c>ClaudeCodeProcessLauncher</c>, e per la stessa ragione: la scansione
    /// del PATH è vera su tutte le piattaforme, così «opencode non è installato» si scopre
    /// subito e con quel nome, invece di diventare un errore di avvio processo mille righe più
    /// in là.
    /// </para>
    /// <para>
    /// ⚠️ <b>Windows non è stato verificato.</b> L'ordine dei candidati è quello di Claude Code
    /// e Copilot — <c>.exe</c>, poi lo shim <c>.cmd</c> di npm, poi <c>.ps1</c> — perché
    /// <see cref="Process.Start(ProcessStartInfo)"/> con <c>UseShellExecute=false</c> NON onora
    /// PATHEXT. Misurato su Linux (opencode 1.18.30 installato con npm): il binario è
    /// <c>~/.nvm/versions/node/&lt;v&gt;/bin/opencode</c>.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F3.</para>
    /// </summary>
    public static class OpenCodeProcessLauncher
    {
        /// <summary>Nome dell'eseguibile, senza estensione.</summary>
        public const string ExecutableName = "opencode";

        /// <summary>
        /// La variabile d'ambiente con cui il server chiede una password. Misurato il
        /// 21/09/2026 su opencode 1.18.30: quando è valorizzata ogni richiesta senza
        /// credenziali riceve <c>401</c>.
        /// </summary>
        public const string PasswordVariable = "OPENCODE_SERVER_PASSWORD";

        /// <summary>
        /// L'utente dell'autenticazione Basic. ⚠️ Misurato: **deve** essere esattamente
        /// <c>opencode</c> — con <c>x</c>, <c>admin</c> o l'utente vuoto la stessa password
        /// riceve <c>401</c>. Un <c>Authorization: Bearer &lt;password&gt;</c> pure.
        /// </summary>
        public const string BasicUser = "opencode";

        /// <summary>
        /// Percorso assoluto del binario risolto, oppure <c>null</c> se non è nel PATH. Pura
        /// ispezione del filesystem: nessun processo lanciato, nessun timeout.
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
                    catch (ArgumentException) { continue; } // PATH con caratteri illegali
                    if (File.Exists(full)) return full;
                }
            }
            return null;
        }

        /// <summary><c>true</c> se il CLI è risolvibile nel PATH.</summary>
        public static bool IsResolvable() => ResolvePath() != null;

        /// <summary>
        /// Start info per <c>opencode serve</c>. La porta è <c>0</c>: la sceglie il sistema e il
        /// server la <b>dichiara</b> sulla prima riga di stdout, quindi non c'è nessuna finestra
        /// fra «ho trovato una porta libera» e «l'ho occupata» in cui qualcun altro possa
        /// prendersela.
        /// </summary>
        /// <param name="password">Valore per <see cref="PasswordVariable"/>.</param>
        public static ProcessStartInfo BuildServeStartInfo(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Il server opencode va avviato con una password.", nameof(password));

            var resolved = ResolvePath();
            if (resolved == null)
            {
                throw new InvalidOperationException(
                    "opencode non è installato su questa macchina, o non è nel PATH del servizio. " +
                    $"Cercato '{ExecutableName}' in ogni cartella del PATH. Installalo con " +
                    "`npm i -g opencode-ai`, oppure scegli un altro motore per MarkAgent nelle " +
                    "impostazioni del progetto.");
            }

            var psi = new ProcessStartInfo
            {
                FileName = resolved,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("serve");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add("0");
            psi.ArgumentList.Add("--hostname");
            psi.ArgumentList.Add("127.0.0.1");   // mai esposto fuori dalla macchina
            psi.Environment[PasswordVariable] = password;
            return psi;
        }
    }
}
