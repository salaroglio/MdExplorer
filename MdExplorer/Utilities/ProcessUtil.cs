using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Utilities
{
    public class ProcessUtil
    {
        private Process _currentVisualStudio;
        private string _lastDocumentOpened;
        private string _editorPath;
        private string _lastProjectPath;
        public Process CurrentVisualStudio { get { return _currentVisualStudio; } }
        public bool IKilled { get; set; }

        public ProcessUtil()
        {
        }

        public void OpenFileWithVisualStudioCode(string path, string editorPath, string projectPath)
        {
            _editorPath = editorPath;
            _lastDocumentOpened = path;
            _lastProjectPath = projectPath;

            // Clean up the path
            var currentPath = path.Replace(@"\\", System.IO.Path.DirectorySeparatorChar.ToString())
                                  .Replace(@"\", System.IO.Path.DirectorySeparatorChar.ToString());

            // Create ProcessStartInfo for cross-platform compatibility
            // --reuse-window: Reuse existing window instead of opening a new one
            // First parameter: Open project root folder as workspace
            // --goto <file:line>: Open file at specific line (line 1)
            var startInfo = new ProcessStartInfo
            {
                FileName = editorPath,
                Arguments = $"--reuse-window \"{projectPath}\" --goto \"{currentPath}:1\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                if (_currentVisualStudio == null || _currentVisualStudio.HasExited)
                {
                    _currentVisualStudio = Process.Start(startInfo);
                }
                else
                {
                    Process.Start(startInfo);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                Console.WriteLine($"Error opening file with VS Code: {ex.Message}");
                Console.WriteLine($"Editor path: {editorPath}");
                Console.WriteLine($"File path: {currentPath}");
                Console.WriteLine($"Project path: {projectPath}");
            }
        }

        public void OpenFileWithIntelliJ(string path, string intellijPath)
        {
            // Clean up the path
            var currentPath = path.Replace(@"\\", System.IO.Path.DirectorySeparatorChar.ToString())
                                  .Replace(@"\", System.IO.Path.DirectorySeparatorChar.ToString());

            // IntelliJ IDEA command line arguments:
            // --line <number> : go to the specified line
            // Opening only the file (not the project) to avoid multiple windows
            var startInfo = new ProcessStartInfo
            {
                FileName = intellijPath,
                Arguments = $"--line 1 \"{currentPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                Console.WriteLine($"Error opening file with IntelliJ IDEA: {ex.Message}");
                Console.WriteLine($"IntelliJ path: {intellijPath}");
                Console.WriteLine($"File path: {currentPath}");
            }
        }

        /// <summary>
        /// Apre il CLI agentico del progetto (<c>copilot</c>, <c>claude</c>, <c>opencode</c>) in un
        /// terminale visibile, con la cartella di lavoro sulla radice del progetto.
        /// <para>
        /// A differenza di VS Code e IntelliJ, che sono processi grafici lanciati con
        /// <c>CreateNoWindow=true</c>, questi sono programmi da terminale: serve una finestra vera.
        /// </para>
        /// <para>
        /// Quale comando lanciare lo decide il chiamante, che lo prende dall'ambiente agentico del
        /// progetto, e controlla prima che sia installato — così «non è installato» si legge come
        /// tale, e non come una finestra che sbatte e si chiude.
        /// </para>
        /// </summary>
        /// <param name="projectPath">Radice del progetto: è lì che il CLI deve trovarsi.</param>
        /// <param name="command">Nome dell'eseguibile, senza estensione.</param>
        public void OpenFolderWithAgentCli(string projectPath, string command)
        {
            var startInfo = BuildAgentCliStartInfo(projectPath, command);
            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nell'apertura di {command}: {ex.Message}");
                Console.WriteLine($"Cartella del progetto: {projectPath}");
                throw;
            }
        }

        /// <summary>
        /// Come si apre una finestra di terminale, su questa piattaforma, già posizionata nel
        /// progetto e con il comando dentro.
        /// <list type="bullet">
        /// <item><description><b>Windows</b>: <c>powershell.exe -NoExit</c> con
        /// <c>UseShellExecute=true</c>, che è ciò che dà alla finestra una vita propria. La
        /// risoluzione dello shim la fa PowerShell, che onora PATHEXT.</description></item>
        /// <item><description><b>Linux</b>: un emulatore di terminale vero, cercato nel PATH.
        /// ⚠️ Qui <c>UseShellExecute=true</c> NON apre un terminale: su Linux .NET lo traduce in
        /// <c>xdg-open</c>, che aprirebbe il <i>file</i> <c>pwsh</c> con l'applicazione predefinita.
        /// È il motivo per cui questa strada esiste.</description></item>
        /// </list>
        /// Metodo a sé, e statico, per poterlo verificare senza aprire finestre addosso a nessuno.
        /// </summary>
        /// <exception cref="ArgumentException">Comando vuoto o con caratteri che un terminale interpreterebbe.</exception>
        /// <exception cref="InvalidOperationException">Su Linux non c'è nessun emulatore di terminale conosciuto.</exception>
        public static ProcessStartInfo BuildAgentCliStartInfo(string projectPath, string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentException("Nessun comando da aprire.", nameof(command));

            // Solo un nome di comando: qui non deve poter entrare niente che il terminale
            // interpreti come altro (l'ambiente arriva da un file del repository).
            foreach (var c in command)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                    throw new ArgumentException($"Nome di comando non ammesso: '{command}'.", nameof(command));
            }

            var path = projectPath ?? string.Empty;

            if (OperatingSystem.IsWindows())
            {
                // Apici singoli raddoppiati: è così che PowerShell li vuole dentro una 'literal'.
                var safePath = path.Replace("'", "''");
                return new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -Command \"Set-Location -LiteralPath '{safePath}'; {command}\"",
                    WorkingDirectory = path,
                    UseShellExecute = true   // la finestra del terminale deve essere sua e visibile
                };
            }

            if (!OperatingSystem.IsLinux())
            {
                throw new InvalidOperationException(
                    "L'apertura del CLI agentico in un terminale è realizzata per Windows e Linux. " +
                    $"Su {Environment.OSVersion.Platform} apri tu un terminale nella cartella del progetto ed esegui '{command}'.");
            }

            // `exec bash` alla fine: quando il CLI esce la finestra resta, come -NoExit su Windows —
            // altrimenti un errore all'avvio sparirebbe prima che qualcuno riesca a leggerlo.
            var script = $"cd {QuoteForShell(path)} && {command}; exec bash";

            foreach (var terminal in LinuxTerminals)
            {
                var resolved = ResolveInPath(terminal.Command);
                if (resolved == null) continue;

                var startInfo = new ProcessStartInfo
                {
                    FileName = resolved,
                    WorkingDirectory = path,
                    UseShellExecute = false
                };
                foreach (var arg in terminal.BuildArguments(path, script)) startInfo.ArgumentList.Add(arg);
                return startInfo;
            }

            throw new InvalidOperationException(
                "Nessun emulatore di terminale trovato su questa macchina (cercati: "
                + string.Join(", ", LinuxTerminals.Select(t => t.Command))
                + $"). Installane uno, oppure apri tu un terminale nel progetto ed esegui '{command}'.");
        }

        /// <summary>
        /// Gli emulatori di terminale che sappiamo pilotare, in ordine di preferenza: ognuno vuole
        /// i suoi argomenti, e sbagliarli vuol dire una finestra che non si apre.
        /// </summary>
        private static readonly (string Command, Func<string, string, string[]> BuildArguments)[] LinuxTerminals =
        {
            ("gnome-terminal", (dir, script) => new[] { "--working-directory=" + dir, "--", "bash", "-lc", script }),
            ("konsole",        (dir, script) => new[] { "--workdir", dir, "-e", "bash", "-lc", script }),
            ("xfce4-terminal", (dir, script) => new[] { "--working-directory=" + dir, "-x", "bash", "-lc", script }),
            ("x-terminal-emulator", (dir, script) => new[] { "-e", "bash", "-lc", script }),
            ("xterm",          (dir, script) => new[] { "-e", "bash", "-lc", script }),
        };

        /// <summary>
        /// Il comando è raggiungibile dal PATH di QUESTO processo? Ispezione del filesystem,
        /// nessun processo lanciato.
        /// <para>
        /// Su Windows si provano anche gli shim di npm (<c>.exe</c>, <c>.cmd</c>, <c>.ps1</c>),
        /// perché <see cref="Process.Start(ProcessStartInfo)"/> con <c>UseShellExecute=false</c>
        /// non onora PATHEXT.
        /// </para>
        /// </summary>
        public static bool IsCommandInPath(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return false;
            var candidates = OperatingSystem.IsWindows()
                ? new[] { command + ".exe", command + ".cmd", command + ".ps1" }
                : new[] { command };
            return candidates.Any(c => ResolveInPath(c) != null);
        }

        /// <summary>Percorso assoluto di un eseguibile nel PATH, o <c>null</c>. Nessun processo lanciato.</summary>
        private static string ResolveInPath(string executable)
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var raw in pathEnv.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                string full;
                try { full = Path.Combine(raw.Trim().Trim('"'), executable); }
                catch (ArgumentException) { continue; }
                if (File.Exists(full)) return full;
            }
            return null;
        }

        /// <summary>Un percorso dentro apici singoli, alla maniera di POSIX: <c>'</c> → <c>'\''</c>.</summary>
        private static string QuoteForShell(string value)
            => "'" + (value ?? string.Empty).Replace("'", "'\\''") + "'";

        public void KillVisualStudioCode()
        {
            if (_currentVisualStudio != null && !_currentVisualStudio.HasExited)
            {
                _currentVisualStudio.Kill();
                IKilled = true;
                //_currentVisualStudio.Dispose();
                //_currentVisualStudio = null;
            }
        }


        public void ReopenVisualStudioCode(string newDocument)
        {
            if (!string.IsNullOrEmpty(newDocument) && !string.IsNullOrEmpty(_lastProjectPath))
            {
                OpenFileWithVisualStudioCode(newDocument, _editorPath, _lastProjectPath);
            }
        }
    }
}
