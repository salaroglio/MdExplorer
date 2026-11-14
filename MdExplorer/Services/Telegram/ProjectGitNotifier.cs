using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Services.Git;

namespace MdExplorer.Services.Telegram
{
    /// <summary>
    /// Gli avvisi del bot di progetto (D5): dopo un push riuscito, chi ha pubblicato quali commit; dopo un pull fallito con
    /// conflitti, chi e quali file. Il progetto è la cartella più vicina, risalendo dal repository, che ha il bot in
    /// <see cref="ProjectKeysFile"/> (un submodule avvisa nel canale del progetto che lo contiene). Niente bot = niente lavoro
    /// oltre a qualche controllo di file. L'invio a Telegram va in sfondo: git non aspetta mai la rete per questo.
    /// <para>Limite noto: i push fatti fuori da MDE non arrivano (D5).</para>
    /// </summary>
    public sealed class ProjectGitNotifier : IGitPublishEvents
    {
        private const int MaxCommits = 10;
        private const int MaxFiles = 15;
        private const int MaxLevels = 8;

        private readonly INativeGitRunner _git;
        private readonly ProjectBotService _bot;

        private sealed record Pending(string ProjectRoot, string Repository, string Branch, string Author, int Count, string[] Commits);

        public ProjectGitNotifier(INativeGitRunner git, ProjectBotService bot)
        {
            _git = git;
            _bot = bot;
        }

        public async Task<object> BeforePushAsync(string repositoryPath, string remoteName, string branchName, CancellationToken ct)
        {
            var root = FindProjectRoot(repositoryPath);
            if (root == null) return null;
            // I commit che il remoto non ha ancora: per un ramo nuovo come per uno che esiste già.
            var range = new[] { "HEAD", "--not", $"--remotes={remoteName}" };
            var count = await _git.RunAsync(repositoryPath, new[] { "rev-list", "--count" }.Concat(range).ToArray(), ct);
            if (!count.Ok || !int.TryParse(count.Stdout.Trim(), out var n) || n == 0) return null;
            var log = await _git.RunAsync(repositoryPath, new[] { "log", $"--max-count={MaxCommits}", "--format=%h %s" }.Concat(range).ToArray(), ct);
            var branch = string.IsNullOrEmpty(branchName) ? await BranchAsync(repositoryPath, ct) : branchName;
            return new Pending(root, Label(root, repositoryPath), branch, await AuthorAsync(repositoryPath, ct), n,
                log.Ok ? Lines(log.Stdout) : Array.Empty<string>());
        }

        public void AfterPush(object before, bool ok)
        {
            if (!ok || before is not Pending p) return;
            var text = new StringBuilder(UiText.Say(
                $"📤 {p.Author} ha pubblicato {p.Count} commit su {p.Branch} ({p.Repository})",
                $"📤 {p.Author} pushed {p.Count} commit(s) to {p.Branch} ({p.Repository})"));
            foreach (var c in p.Commits) text.Append("\n• ").Append(c);
            if (p.Count > p.Commits.Length) text.Append("\n…");
            Send(p.ProjectRoot, text.ToString());
        }

        public async Task PullFailedAsync(string repositoryPath, CancellationToken ct)
        {
            var root = FindProjectRoot(repositoryPath);
            if (root == null) return;
            var conflicts = await _git.RunAsync(repositoryPath, new[] { "diff", "--name-only", "--diff-filter=U" }, ct);
            var files = conflicts.Ok ? Lines(conflicts.Stdout) : Array.Empty<string>();
            if (files.Length == 0) return;   // fallito per rete o credenziali: non è un evento del progetto
            var upstream = await _git.RunAsync(repositoryPath, new[] { "rev-parse", "--abbrev-ref", "@{u}" }, ct);
            var text = new StringBuilder(UiText.Say(
                $"⚠️ Scaricamento con conflitti per {await AuthorAsync(repositoryPath, ct)}: {await BranchAsync(repositoryPath, ct)} ← {(upstream.Ok ? upstream.Stdout.Trim() : "origin")} ({Label(root, repositoryPath)})\nFile in conflitto:",
                $"⚠️ Pull with conflicts for {await AuthorAsync(repositoryPath, ct)}: {await BranchAsync(repositoryPath, ct)} ← {(upstream.Ok ? upstream.Stdout.Trim() : "origin")} ({Label(root, repositoryPath)})\nConflicting files:"));
            foreach (var f in files.Take(MaxFiles)) text.Append("\n• ").Append(f);
            if (files.Length > MaxFiles) text.Append("\n…");
            Send(root, text.ToString());
        }

        private void Send(string root, string text)
            => _ = Task.Run(async () =>
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await _bot.NotifyAsync(root, text, cts.Token);
            });

        /// <summary>
        /// Risale dal repository fino alla cartella con <c>.mde-keys.yml</c>; si ferma dopo il primo repository principale
        /// (cartella con <c>.git</c> come directory, non come file di un submodule).
        /// </summary>
        public static string FindProjectRoot(string repositoryPath)
        {
            if (string.IsNullOrEmpty(repositoryPath)) return null;
            var dir = new DirectoryInfo(Path.GetFullPath(repositoryPath));
            for (var i = 0; dir != null && i < MaxLevels; i++, dir = dir.Parent)
            {
                if (File.Exists(Path.Combine(dir.FullName, ProjectKeysFile.FileName))) return dir.FullName;
                if (Directory.Exists(Path.Combine(dir.FullName, ".git"))) return null;
            }
            return null;
        }

        private static string Label(string root, string repositoryPath)
        {
            var rel = Path.GetRelativePath(root, Path.GetFullPath(repositoryPath)).Replace('\\', '/');
            var project = Path.GetFileName(root.TrimEnd('/', '\\'));
            return rel == "." ? project : $"{project}/{rel}";
        }

        private async Task<string> AuthorAsync(string dir, CancellationToken ct)
        {
            var r = await _git.RunAsync(dir, new[] { "config", "user.name" }, ct);
            return r.Ok && !string.IsNullOrWhiteSpace(r.Stdout) ? r.Stdout.Trim() : Environment.UserName;
        }

        private async Task<string> BranchAsync(string dir, CancellationToken ct)
        {
            var r = await _git.RunAsync(dir, new[] { "rev-parse", "--abbrev-ref", "HEAD" }, ct);
            return r.Ok ? r.Stdout.Trim() : "?";
        }

        private static string[] Lines(string s)
            => (s ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
