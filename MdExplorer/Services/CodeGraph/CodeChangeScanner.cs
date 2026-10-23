using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Services.Git;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.CodeGraph
{
    public sealed class CodeScanResult
    {
        /// <summary><c>git</c> oppure <c>hash</c>: come si è deciso che cosa guardare. Dichiarato, mai un fallback muto.</summary>
        public string Mode { get; set; }
        public string RepositoryRoot { get; set; }
        public string HeadCommit { get; set; }
        public string PreviousCommit { get; set; }
        /// <summary>Tutti i file del perimetro che hanno un linguaggio in catalogo (dopo gli ignore).</summary>
        public List<CodeSourceFile> AllFiles { get; } = new();
        /// <summary>I file che git (o la prima volta: tutti) indica come cambiati; l'hash nell'intestazione conferma.</summary>
        public List<CodeSourceFile> Candidates { get; } = new();
        /// <summary>Perché si è scelto il modo (es. «nessun repository git», «prima analisi»).</summary>
        public string Reason { get; set; }
    }

    public interface ICodeChangeScanner
    {
        /// <param name="toLogical">Dal percorso della macchina al percorso logico (relativo al progetto, o <c>@alias/…</c> per una
        /// radice esterna); null = relativo al progetto.</param>
        /// <param name="languages">I linguaggi che contano; null = quelli del progetto (<see cref="CodeLanguageCatalog.ForProject"/>).</param>
        Task<CodeScanResult> ScanAsync(string projectPath, string folderAbsolute, string previousCommit, CancellationToken ct = default, Func<string, string> toLogical = null, CodeLanguageCatalog languages = null);
    }

    /// <summary>
    /// Lo scanner delle modifiche (D14): chiede a git che cosa è cambiato dall'ultimo commit analizzato e lascia all'hash
    /// la conferma. <c>git ls-files --cached --others --exclude-standard</c> dà i file del perimetro rispettando
    /// <c>.gitignore</c>; <c>git diff --name-status -z prev HEAD</c> più <c>git status --porcelain -z</c> danno i candidati.
    /// Fuori da un repository: cammina l'albero con una lista di esclusioni (bin, obj, node_modules, …) e tutti i file
    /// sono candidati; il modo è <c>hash</c> e si dichiara.
    /// </summary>
    public sealed class CodeChangeScanner : ICodeChangeScanner
    {
        private static readonly HashSet<string> IgnoredDirectories = new(StringComparer.OrdinalIgnoreCase)
        {
            ".git", ".md", ".mde-doc", ".mde-code", "bin", "obj", "node_modules", "dist", ".angular", ".vs", ".idea",
            "packages", "TestResults", ".worktrees", "__pycache__", "target", "build", ".gradle", "vendor"
        };

        private readonly INativeGitRunner _git;
        private readonly ILogger<CodeChangeScanner> _logger;
        private readonly MdExplorer.Features.Services.IUiLanguage _language;

        public CodeChangeScanner(INativeGitRunner git, ILogger<CodeChangeScanner> logger, MdExplorer.Features.Services.IUiLanguage language)
        {
            _git = git;
            _logger = logger;
            _language = language;
        }

        /// <summary>Il motivo, che la finestra «Ambiente di sviluppo» mostra così com'è, nella lingua dell'interfaccia.</summary>
        private string L(string italian, string english) => _language.Current == "it" ? italian : english;

        public async Task<CodeScanResult> ScanAsync(string projectPath, string folderAbsolute, string previousCommit, CancellationToken ct = default, Func<string, string> toLogical = null, CodeLanguageCatalog languages = null)
        {
            toLogical ??= abs => CodeGraphPaths.ToProjectRelative(projectPath, abs);
            var result = new CodeScanResult();
            folderAbsolute = Path.GetFullPath(folderAbsolute);
            languages ??= CodeLanguageCatalog.ForProject(projectPath);   // anche i linguaggi definiti nel progetto

            var top = await _git.RunAsync(folderAbsolute, new[] { "rev-parse", "--show-toplevel" }, ct);
            if (!top.Ok)
            {
                result.Mode = "hash";
                result.Reason = top.ExitCode == NativeGitRunner.GitNotFoundExit ? L("git non installato", "git is not installed") : L("la cartella non sta in un repository git", "the folder is not in a git repository");
                WalkWithoutGit(toLogical, folderAbsolute, result, languages);
                result.Candidates.AddRange(result.AllFiles);
                return result;
            }

            result.Mode = "git";
            result.RepositoryRoot = Path.GetFullPath(top.Stdout.Trim());
            var head = await _git.RunAsync(folderAbsolute, new[] { "rev-parse", "HEAD" }, ct);
            result.HeadCommit = head.Ok ? head.Stdout.Trim() : null;   // repository senza commit: HEAD non c'è
            result.PreviousCommit = previousCommit;

            // Il perimetro: tracciati + non tracciati non ignorati, relativi alla radice del repository.
            var ls = await _git.RunAsync(result.RepositoryRoot, new[] { "ls-files", "-z", "--cached", "--others", "--exclude-standard", "--", folderAbsolute }, ct);
            if (!ls.Ok) throw new InvalidOperationException($"git ls-files fallito in '{result.RepositoryRoot}': {ls.Describe()}");
            foreach (var rel in SplitZ(ls.Stdout))
            {
                var abs = Path.GetFullPath(Path.Combine(result.RepositoryRoot, rel));
                if (languages.ForFile(abs) == null || !File.Exists(abs)) continue;
                result.AllFiles.Add(new CodeSourceFile(abs, toLogical(abs)));
            }

            if (string.IsNullOrEmpty(previousCommit) || string.IsNullOrEmpty(result.HeadCommit))
            {
                result.Reason = string.IsNullOrEmpty(previousCommit) ? L("prima analisi", "first analysis") : L("repository senza commit", "repository with no commits");
                result.Candidates.AddRange(result.AllFiles);
                return result;
            }

            var candidates = new HashSet<string>(StringComparer.Ordinal);
            if (!string.Equals(previousCommit, result.HeadCommit, StringComparison.OrdinalIgnoreCase))
            {
                var diff = await _git.RunAsync(result.RepositoryRoot, new[] { "diff", "--name-status", "-z", previousCommit, result.HeadCommit, "--", folderAbsolute }, ct);
                if (!diff.Ok)
                {
                    // Il commit ricordato non esiste più (rebase, clone nuovo): si riparte da tutto, e si dice.
                    result.Reason = L($"il commit analizzato {previousCommit[..Math.Min(8, previousCommit.Length)]} non è più raggiungibile: analisi completa", $"the analysed commit {previousCommit[..Math.Min(8, previousCommit.Length)]} can no longer be reached: full analysis");
                    result.Candidates.AddRange(result.AllFiles);
                    return result;
                }
                foreach (var path in ParseNameStatus(diff.Stdout)) candidates.Add(Path.GetFullPath(Path.Combine(result.RepositoryRoot, path)));
            }
            var status = await _git.RunAsync(result.RepositoryRoot, new[] { "status", "--porcelain", "-z", "--untracked-files=all", "--", folderAbsolute }, ct);
            if (status.Ok)
                foreach (var path in ParsePorcelain(status.Stdout)) candidates.Add(Path.GetFullPath(Path.Combine(result.RepositoryRoot, path)));

            result.Reason = L($"git: {candidates.Count} percorsi cambiati da {previousCommit[..Math.Min(8, previousCommit.Length)]}", $"git: {candidates.Count} paths changed since {previousCommit[..Math.Min(8, previousCommit.Length)]}");
            result.Candidates.AddRange(result.AllFiles.Where(f => candidates.Contains(f.AbsolutePath)));
            return result;
        }

        private static void WalkWithoutGit(Func<string, string> toLogical, string folder, CodeScanResult result, CodeLanguageCatalog languages)
        {
            var stack = new Stack<string>();
            stack.Push(folder);
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                IEnumerable<string> entries;
                try { entries = Directory.EnumerateFileSystemEntries(dir); }
                catch (UnauthorizedAccessException) { continue; }
                foreach (var e in entries)
                {
                    if (Directory.Exists(e))
                    {
                        if (!IgnoredDirectories.Contains(Path.GetFileName(e))) stack.Push(e);
                    }
                    else if (languages.ForFile(e) != null)
                    {
                        result.AllFiles.Add(new CodeSourceFile(Path.GetFullPath(e), toLogical(Path.GetFullPath(e))));
                    }
                }
            }
        }

        private static IEnumerable<string> SplitZ(string stdout)
            => (stdout ?? string.Empty).Split('\0', StringSplitOptions.RemoveEmptyEntries);

        /// <summary><c>diff --name-status -z</c>: «M\0path\0», «A\0path\0», «D\0path\0», «R100\0old\0new\0».</summary>
        private static IEnumerable<string> ParseNameStatus(string stdout)
        {
            var parts = SplitZ(stdout).ToList();
            for (var i = 0; i < parts.Count; i++)
            {
                var status = parts[i];
                if (status.Length == 0) continue;
                var code = status[0];
                if (code == 'R' || code == 'C')
                {
                    if (i + 2 < parts.Count) { yield return parts[i + 1]; yield return parts[i + 2]; }
                    i += 2;
                }
                else
                {
                    if (i + 1 < parts.Count) yield return parts[i + 1];
                    i += 1;
                }
            }
        }

        /// <summary><c>status --porcelain -z</c>: «XY path\0» e, per le rinomine, «R  new\0old\0».</summary>
        private static IEnumerable<string> ParsePorcelain(string stdout)
        {
            var parts = SplitZ(stdout).ToList();
            for (var i = 0; i < parts.Count; i++)
            {
                var entry = parts[i];
                if (entry.Length < 4) continue;
                var xy = entry.Substring(0, 2);
                yield return entry.Substring(3);
                if (xy.Contains('R') || xy.Contains('C')) { if (i + 1 < parts.Count) yield return parts[i + 1]; i++; }
            }
        }
    }
}
