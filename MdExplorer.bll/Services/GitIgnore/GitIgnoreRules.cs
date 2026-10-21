using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.GitIgnore
{
    /// <summary>
    /// Che cosa git ignora in un progetto (sprint 2026-10-21-Watcher-Selettivo-E-Gitignore, W2/W4): ciò che git ignora non si
    /// sorveglia, non compare nell'albero e non si indicizza. La regola la calcola git — tutti i <c>.gitignore</c> annidati,
    /// <c>.git/info/exclude</c>, il file globale — non un interprete scritto a mano. Una fotografia per progetto, in cache
    /// finché <see cref="Invalidate"/> non dice che le regole sono cambiate.
    /// </summary>
    public interface IGitIgnoreRules
    {
        /// <summary>Vero se git ignora il file o la cartella, o una cartella che lo contiene (dentro il progetto).</summary>
        bool IsIgnored(string fullPath, string projectPath);

        GitIgnoreSnapshot Snapshot(string projectPath);

        /// <summary>Le regole sono cambiate (un .gitignore modificato): la prossima domanda ricalcola.</summary>
        void Invalidate(string projectPath);
    }

    /// <summary>La fotografia: cartelle e file ignorati da git, percorsi assoluti.</summary>
    public sealed class GitIgnoreSnapshot
    {
        public string ProjectPath { get; }
        /// <summary>False per un progetto che non sta in un repository: niente regole di git.</summary>
        public bool IsGit { get; }
        public IReadOnlyCollection<string> IgnoredDirectories => _dirs;
        public IReadOnlyCollection<string> IgnoredFiles => _files;
        private readonly HashSet<string> _dirs;
        private readonly HashSet<string> _files;

        public GitIgnoreSnapshot(string projectPath, bool isGit, IEnumerable<string> dirs, IEnumerable<string> files)
        {
            ProjectPath = Trim(projectPath);
            IsGit = isGit;
            var comparer = PathContainment.Comparison == StringComparison.Ordinal ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            _dirs = new HashSet<string>(dirs.Select(Trim), comparer);
            _files = new HashSet<string>(files.Select(Trim), comparer);
        }

        public bool IsIgnored(string fullPath)
        {
            if (!IsGit || string.IsNullOrEmpty(fullPath)) return false;
            var path = Trim(Path.GetFullPath(fullPath));
            if (_files.Contains(path)) return true;
            for (var current = path; current != null && current.Length >= ProjectPath.Length; current = Path.GetDirectoryName(current))
            {
                if (_dirs.Contains(current)) return true;
                if (current.Length == ProjectPath.Length) break;
            }
            return false;
        }

        private static string Trim(string p) => p.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public sealed class GitIgnoreRules : IGitIgnoreRules
    {
        private readonly ConcurrentDictionary<string, Lazy<GitIgnoreSnapshot>> _snapshots = new(StringComparer.Ordinal);

        public bool IsIgnored(string fullPath, string projectPath)
        {
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(projectPath)) return false;
            if (!PathContainment.IsInside(projectPath, fullPath)) return false;
            return Snapshot(projectPath).IsIgnored(fullPath);
        }

        public GitIgnoreSnapshot Snapshot(string projectPath)
        {
            var key = Key(projectPath);
            var lazy = _snapshots.GetOrAdd(key, k => new Lazy<GitIgnoreSnapshot>(() => Compute(k)));
            try { return lazy.Value; }
            catch
            {
                _snapshots.TryRemove(key, out _);   // un errore non resta in cache: la prossima domanda riprova
                throw;
            }
        }

        public void Invalidate(string projectPath) => _snapshots.TryRemove(Key(projectPath), out _);

        private static string Key(string projectPath) => Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        /// <summary>
        /// <c>git ls-files --others --ignored --exclude-standard --directory</c> nella cartella del progetto (i percorsi
        /// escono relativi a essa e limitati al suo sottoalbero), poi lo stesso in ogni submodule: git non scende nei
        /// submodule, che hanno i loro .gitignore.
        /// </summary>
        public static GitIgnoreSnapshot Compute(string projectPath)
        {
            var root = Key(projectPath);
            if (!InsideRepository(root)) return new GitIgnoreSnapshot(root, false, Array.Empty<string>(), Array.Empty<string>());
            // Il progetto sta in una cartella che il repository di sopra ignora (una home sotto git con «progetti/» ignorata):
            // per git i suoi file non esistono, e ls-files lì dentro fallisce («directory entry not superset of prefix»,
            // visto il 21/10). Allora il progetto non ha regole di git.
            if (Run(root, out _, "check-ignore", "-q", ".") == 0)
                return new GitIgnoreSnapshot(root, false, Array.Empty<string>(), Array.Empty<string>());
            var dirs = new List<string>();
            var files = new List<string>();
            Collect(root, dirs, files, depth: 0);
            return new GitIgnoreSnapshot(root, true, dirs, files);
        }

        private static void Collect(string folder, List<string> dirs, List<string> files, int depth)
        {
            foreach (var entry in Git(folder, "ls-files", "--others", "--ignored", "--exclude-standard", "--directory", "-z"))
            {
                var full = Path.GetFullPath(Path.Combine(folder, entry.TrimEnd('/').Replace('/', Path.DirectorySeparatorChar)));
                (entry.EndsWith("/", StringComparison.Ordinal) ? dirs : files).Add(full);
            }
            if (depth > 8) return;
            // i submodule: voci 160000 dell'indice, se sono stati estratti (hanno il loro .git)
            foreach (var entry in Git(folder, "ls-files", "--stage", "-z"))
            {
                if (!entry.StartsWith("160000 ", StringComparison.Ordinal)) continue;
                var tab = entry.IndexOf('\t');
                if (tab < 0) continue;
                var sub = Path.GetFullPath(Path.Combine(folder, entry.Substring(tab + 1).Replace('/', Path.DirectorySeparatorChar)));
                if (Directory.Exists(sub) && (Directory.Exists(Path.Combine(sub, ".git")) || File.Exists(Path.Combine(sub, ".git"))))
                    Collect(sub, dirs, files, depth + 1);
            }
        }

        /// <summary>La cartella o una sopra ha <c>.git</c>: allora git deve rispondere, e se non può è un errore da dire.</summary>
        private static bool InsideRepository(string folder)
        {
            for (var dir = new DirectoryInfo(folder); dir != null; dir = dir.Parent)
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")) || File.Exists(Path.Combine(dir.FullName, ".git"))) return true;
            return false;
        }

        private static IEnumerable<string> Git(string folder, params string[] args)
        {
            var exit = Run(folder, out var output, args);
            if (exit != 0)
                throw new InvalidOperationException($"git {string.Join(' ', args)} in {folder}: {output.Stderr.Trim()}");
            return output.Stdout.Split('\0', StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>Lancia git e restituisce l'exit code; un git che non parte, in un repository, è un errore da dire.</summary>
        private static int Run(string folder, out (string Stdout, string Stderr) output, params string[] args)
        {
            var psi = new ProcessStartInfo("git")
            {
                WorkingDirectory = folder,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = new UTF8Encoding(false),   // Windows: senza, «Ã¨» nei nomi (vedi git_stdout_utf8_windows)
                StandardErrorEncoding = new UTF8Encoding(false),
            };
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("core.quotepath=off");
            foreach (var a in args) psi.ArgumentList.Add(a);
            Process process;
            try { process = Process.Start(psi) ?? throw new InvalidOperationException("git not started"); }
            catch (Win32Exception ex)
            {
                throw new InvalidOperationException(UiText.Say(
                    $"{folder} sta in un repository git, ma git non si avvia ({ex.Message}): senza git MDE non sa quali cartelle ignorare. Installa git o mettilo nel PATH.",
                    $"{folder} is in a git repository, but git does not start ({ex.Message}): without git MDE cannot know which folders to ignore. Install git or put it in the PATH."), ex);
            }
            using (process)
            {
                var stderr = process.StandardError.ReadToEndAsync();
                var stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                output = (stdout, stderr.Result);
                return process.ExitCode;
            }
        }
    }
}
