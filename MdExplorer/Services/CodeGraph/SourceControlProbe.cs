using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Services.Git;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.CodeGraph
{
    public interface ISourceControlProbe
    {
        /// <summary>Il gestore di sorgenti della cartella e il suo stato adesso (<see cref="SourceControlInfo"/>); mai un'eccezione per un gestore che manca: lo dice <c>Note</c>.</summary>
        Task<SourceControlInfo> ProbeAsync(string folderAbsolute, CancellationToken ct = default);
    }

    /// <summary>
    /// Riconosce da solo git o SVN (30/10/2026): salendo dalla cartella, il primo fra <c>.git</c> e <c>.svn</c> decide, così
    /// una copia SVN dentro un repository git (o il contrario) risponde per quello che è davvero. Git dal git di sistema;
    /// SVN dai comandi <c>svnversion</c> (la revisione, anche mista, come la scrive SVN) e <c>svn</c> (URL e modifiche).
    /// Un <c>.svn</c> senza i comandi installati non diventa «nessun gestore»: si dice che cosa installare.
    /// </summary>
    public sealed class SourceControlProbe : ISourceControlProbe
    {
        private const int SvnTimeoutMs = 120_000;
        private readonly INativeGitRunner _git;
        private readonly ILogger<SourceControlProbe> _logger;

        public SourceControlProbe(INativeGitRunner git, ILogger<SourceControlProbe> logger)
        {
            _git = git;
            _logger = logger;
        }

        public async Task<SourceControlInfo> ProbeAsync(string folderAbsolute, CancellationToken ct = default)
        {
            folderAbsolute = Path.GetFullPath(folderAbsolute);
            return Nearest(folderAbsolute) switch
            {
                SourceControlInfo.Git => await GitAsync(folderAbsolute, ct),
                SourceControlInfo.Svn => await SvnAsync(folderAbsolute, ct),
                _ => new SourceControlInfo { Kind = SourceControlInfo.None, Note = UiText.Say("nessun gestore di sorgenti (né .git né .svn salendo dalla cartella)", "no source control (neither .git nor .svn above the folder)") },
            };
        }

        /// <summary>Il gestore della cartella di versionamento più vicina salendo; null se non ce n'è. <c>.git</c> può essere un file (worktree, submodule).</summary>
        public static string Nearest(string folderAbsolute)
        {
            for (var dir = new DirectoryInfo(folderAbsolute); dir != null; dir = dir.Parent)
            {
                var git = Path.Combine(dir.FullName, ".git");
                if (Directory.Exists(git) || File.Exists(git)) return SourceControlInfo.Git;
                if (Directory.Exists(Path.Combine(dir.FullName, ".svn"))) return SourceControlInfo.Svn;
            }
            return null;
        }

        private async Task<SourceControlInfo> GitAsync(string folder, CancellationToken ct)
        {
            var info = new SourceControlInfo { Kind = SourceControlInfo.Git };
            var head = await _git.RunAsync(folder, new[] { "rev-parse", "HEAD" }, ct);
            if (!head.Ok) { info.Note = UiText.Say("git: revisione non letta — ", "git: revision not read — ") + head.Describe(); return info; }
            info.Revision = head.Stdout.Trim();
            var branch = await _git.RunAsync(folder, new[] { "rev-parse", "--abbrev-ref", "HEAD" }, ct);
            if (branch.Ok) info.Branch = branch.Stdout.Trim();
            var status = await _git.RunAsync(folder, new[] { "status", "--porcelain", "-z", "--untracked-files=no", "--", "." }, ct);
            if (status.Ok) info.LocalChanges = CountPorcelain(status.Stdout);
            else info.Note = UiText.Say("git: modifiche locali non lette — ", "git: local changes not read — ") + status.Describe();
            return info;
        }

        /// <summary><c>status --porcelain -z</c>: una voce per file; una rinomina porta anche il nome vecchio, che non si conta.</summary>
        public static int CountPorcelain(string stdout)
        {
            var parts = (stdout ?? string.Empty).Split('\0', StringSplitOptions.RemoveEmptyEntries);
            var n = 0;
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length < 4) continue;
                n++;
                var xy = parts[i].Substring(0, 2);
                if (xy.Contains('R') || xy.Contains('C')) i++;
            }
            return n;
        }

        private async Task<SourceControlInfo> SvnAsync(string folder, CancellationToken ct)
        {
            var info = new SourceControlInfo { Kind = SourceControlInfo.Svn };
            var version = await RunAsync("svnversion", folder, new[] { "-n", "." }, ct);
            if (version.Missing)
            {
                info.Note = UiText.Say("cartella SVN, ma il comando svnversion non è installato: servono i comandi da riga di comando di SVN (con TortoiseSVN: «command line client tools» nell'installazione)",
                    "SVN folder, but the svnversion command is not installed: SVN's command line tools are needed (with TortoiseSVN: «command line client tools» in the installer)");
                return info;
            }
            if (version.ExitCode != 0) { info.Note = "svnversion: " + version.Why; return info; }
            (info.Revision, var modified) = ParseSvnVersion(version.Stdout);
            if (info.Revision == null) { info.Note = "svnversion: " + version.Stdout.Trim(); return info; }
            var url = await RunAsync("svn", folder, new[] { "info", "--show-item", "relative-url", "." }, ct);
            if (url.ExitCode == 0) info.Branch = url.Stdout.Trim();
            var status = await RunAsync("svn", folder, new[] { "status", "-q", "." }, ct);
            if (status.ExitCode == 0) info.LocalChanges = CountSvnStatus(status.Stdout);
            else info.Note = UiText.Say("svn: modifiche locali non lette — ", "svn: local changes not read — ") + status.Why + (modified ? " (svnversion: M)" : string.Empty);
            return info;
        }

        /// <summary>
        /// <c>svnversion</c>: <c>1240</c>, <c>1230:1240</c> (copia mista), con in coda <c>M</c> (modificata), <c>S</c>
        /// (switched), <c>P</c> (parziale). Fuori da una copia: «Unversioned directory», «exported»: revisione null.
        /// </summary>
        public static (string Revision, bool Modified) ParseSvnVersion(string stdout)
        {
            var text = (stdout ?? string.Empty).Trim();
            var revision = text.TrimEnd('M', 'S', 'P');
            if (revision.Length == 0 || !revision.All(c => char.IsDigit(c) || c == ':')) return (null, false);
            return (revision, text.Substring(revision.Length).Contains('M'));
        }

        /// <summary><c>svn status -q</c>: una riga per file versionato cambiato; non le righe sugli externals.</summary>
        public static int CountSvnStatus(string stdout)
            => (stdout ?? string.Empty).Split('\n').Count(l => l.Length > 8 && l[0] != ' ' && l[0] != 'X' && !l.StartsWith("Performing status", StringComparison.Ordinal));

        private sealed record Run(int ExitCode, string Stdout, string Why, bool Missing);

        private async Task<Run> RunAsync(string command, string folder, string[] args, CancellationToken ct)
        {
            var psi = new ProcessStartInfo
            {
                FileName = command, WorkingDirectory = folder, UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8,
            };
            foreach (var a in args) psi.ArgumentList.Add(a);
            // i messaggi di svn in inglese e i percorsi in UTF-8, qualunque sia la lingua della macchina
            psi.Environment["LC_ALL"] = "C.UTF-8";
            using var process = new Process { StartInfo = psi };
            try { process.Start(); }
            catch (Win32Exception) { return new Run(-1, string.Empty, command + " non trovato nel PATH", Missing: true); }
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            var done = await Task.Run(() => process.WaitForExit(SvnTimeoutMs), ct);
            if (!done)
            {
                try { process.Kill(true); } catch { }
                _logger.LogWarning("[SourceControl] {Command} {Args} in timeout in {Folder}", command, string.Join(" ", args), folder);
                return new Run(-1, string.Empty, "timeout", Missing: false);
            }
            return new Run(process.ExitCode, await stdout, (await stderr).Trim(), Missing: false);
        }
    }
}
