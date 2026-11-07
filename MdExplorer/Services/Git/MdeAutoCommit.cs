using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>Che cosa ha fatto il commit automatico per un repository: lo leggeranno le notifiche (S3).</summary>
    public sealed class MdeAutoCommitResult
    {
        public string RepositoryRoot { get; init; }
        public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> Reasons { get; init; } = Array.Empty<string>();
        public bool Committed { get; init; }
        public string CommitMessage { get; init; }
        public bool Pushed { get; init; }
        /// <summary>Perché il commit non c'è o il push non è partito; null se tutto è andato.</summary>
        public string Pending { get; init; }
        public DateTime At { get; init; } = DateTime.Now;
    }

    /// <summary>
    /// S2 dello sprint «File di sistema di MDE» (docs-internal/Sprints/2026-11-07-File-Di-Sistema-MDE.md): MDE non lascia mai
    /// modificati senza commit i suoi file del repository. Prima li lasciava, e al primo cambio di un collega il pull si bloccava
    /// (progetto A, progetto B).
    /// <para>
    /// Ascolta <see cref="MdeRepoWriter.Written"/>, raccoglie le scritture per repository per <c>MdeAutoCommit:DelayMs</c> (un'operazione
    /// scrive più volte: un commit solo) e poi:
    /// </para>
    /// <list type="number">
    /// <item>solo i file di <see cref="MdeOwnedFiles"/>, non ignorati da git e davvero diversi dall'ultimo commit;</item>
    /// <item>niente durante un'unione, un rebase o un cherry-pick a metà, niente senza identità git: si riprova alla scrittura dopo;</item>
    /// <item><c>git commit --only -- &lt;file&gt;</c> con «MDE: &lt;motivo&gt;»: con <c>--only</c> lo stage dell'utente resta com'è;</item>
    /// <item>push SOLO nel caso sicuro: dopo un fetch il remoto non ha novità e i commit da pubblicare sono tutti «MDE: …». Git pubblica
    /// commit, non file: con un commit dell'utente in mezzo il push porterebbe anche quello, e allora si aspetta il suo push.</item>
    /// </list>
    /// Si spegne con <c>MdeAutoCommit:Enabled=false</c> (lo fanno i test d'integrazione, tranne quelli di questa funzione).
    /// </summary>
    public sealed class MdeAutoCommit : IHostedService, IDisposable
    {
        public const string MessagePrefix = "MDE: ";

        private readonly INativeGitRunner _git;
        private readonly IServiceScopeFactory _scopes;
        private readonly ILogger<MdeAutoCommit> _logger;
        private readonly bool _enabled;
        private readonly int _delayMs;

        private readonly object _lock = new();
        /// <summary>Per repository: i file scritti, il motivo di ciascuno, e il timer che fa partire il commit.</summary>
        private readonly Dictionary<string, (Dictionary<string, string> Files, Timer Timer)> _pending = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, Task> _running = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentQueue<MdeAutoCommitResult> _results = new();

        public MdeAutoCommit(INativeGitRunner git, IServiceScopeFactory scopes, IConfiguration configuration, ILogger<MdeAutoCommit> logger)
        {
            _git = git;
            _scopes = scopes;
            _logger = logger;
            _enabled = configuration?["MdeAutoCommit:Enabled"] is not { } v || !string.Equals(v, "false", StringComparison.OrdinalIgnoreCase);
            _delayMs = int.TryParse(configuration?["MdeAutoCommit:DelayMs"], out var d) ? d : 1500;
        }

        /// <summary>Gli esiti più recenti (al massimo 50), dal più vecchio.</summary>
        public IReadOnlyList<MdeAutoCommitResult> RecentResults => _results.ToArray();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_enabled) MdeRepoWriter.Written += OnWritten;
            else _logger.LogInformation("[MdeAutoCommit] spento (MdeAutoCommit:Enabled=false)");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            MdeRepoWriter.Written -= OnWritten;
            await FlushAsync(cancellationToken);
        }

        public void Dispose() => MdeRepoWriter.Written -= OnWritten;

        private void OnWritten(string fullPath, string why)
        {
            var root = FindRepositoryRoot(fullPath);
            if (root == null) return; // non è in un repository git
            if (!IsOwned(root, fullPath)) return;

            lock (_lock)
            {
                if (!_pending.TryGetValue(root, out var entry))
                {
                    entry = (new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), new Timer(_ => Fire(root)));
                    _pending[root] = entry;
                }
                entry.Files[fullPath] = why;
                entry.Timer.Change(_delayMs, Timeout.Infinite);
            }
        }

        /// <summary>Fa partire subito quello che è in attesa e aspetta che finisca (test, chiusura del servizio).</summary>
        public async Task FlushAsync(CancellationToken ct = default)
        {
            string[] roots;
            lock (_lock) roots = _pending.Keys.ToArray();
            foreach (var root in roots) Fire(root);
            foreach (var task in _running.Values.ToArray())
            {
                try { await task.WaitAsync(ct); } catch { /* l'esito è già nei risultati */ }
            }
        }

        private void Fire(string root)
        {
            Dictionary<string, string> files;
            lock (_lock)
            {
                if (!_pending.Remove(root, out var entry)) return;
                entry.Timer.Dispose();
                files = entry.Files;
            }
            // un commit alla volta per repository: il successivo aspetta il precedente
            _running.AddOrUpdate(root,
                _ => Task.Run(() => CommitAsync(root, files)),
                (_, previous) => previous.ContinueWith(_ => CommitAsync(root, files)).Unwrap());
        }

        private async Task CommitAsync(string root, Dictionary<string, string> written)
        {
            MdeAutoCommitResult result;
            try { result = await CommitCoreAsync(root, written); }
            catch (Exception ex)
            {
                result = new MdeAutoCommitResult { RepositoryRoot = root, Files = written.Keys.ToList(), Reasons = written.Values.Distinct().ToList(), Pending = ex.Message };
            }
            _results.Enqueue(result);
            while (_results.Count > 50) _results.TryDequeue(out _);
            if (result.Pending == null)
                _logger.LogInformation("[MdeAutoCommit] {Root}: «{Message}» ({Files}), {Push}", root, result.CommitMessage, string.Join(", ", result.Files), result.Pushed ? "pubblicato" : "non pubblicato");
            else
                _logger.LogInformation("[MdeAutoCommit] {Root}: {Pending}", root, result.Pending);
        }

        private async Task<MdeAutoCommitResult> CommitCoreAsync(string root, Dictionary<string, string> written)
        {
            MdeAutoCommitResult Pending(string why, IReadOnlyList<string> files = null, string message = null, bool committed = false)
                => new() { RepositoryRoot = root, Files = files ?? written.Keys.Select(p => Rel(root, p)).ToList(), Reasons = written.Values.Distinct().ToList(), Committed = committed, CommitMessage = message, Pending = why };

            var gitDir = await _git.RunAsync(root, new[] { "rev-parse", "--absolute-git-dir" });
            if (!gitDir.Ok) return Pending("non è un repository git");
            var g = gitDir.Stdout.Trim();
            if (File.Exists(Path.Combine(g, "MERGE_HEAD")) || Directory.Exists(Path.Combine(g, "rebase-merge"))
                || Directory.Exists(Path.Combine(g, "rebase-apply")) || File.Exists(Path.Combine(g, "CHERRY_PICK_HEAD")))
                return Pending("c'è un'unione o un rebase a metà: il commit dei file di MDE aspetta che finisca");
            var email = await _git.RunAsync(root, new[] { "config", "user.email" });
            if (!email.Ok || string.IsNullOrWhiteSpace(email.Stdout))
                return Pending("git non sa chi sei (user.email): il commit dei file di MDE aspetta che l'identità sia impostata");

            // Solo i file di MDE, non ignorati, davvero diversi dall'ultimo commit.
            var candidates = written.Keys.Select(p => Rel(root, p)).Where(r => !r.StartsWith("..")).Distinct().ToList();
            var paths = new List<string>();
            var untracked = new List<string>();
            foreach (var rel in candidates)
            {
                if ((await _git.RunAsync(root, new[] { "check-ignore", "-q", "--", rel })).ExitCode == 0) continue;
                var status = await _git.RunAsync(root, new[] { "status", "--porcelain", "--", rel });
                var line = (status.Stdout ?? string.Empty).Trim();
                if (line.Length == 0) continue;
                paths.Add(rel);
                if (line.StartsWith("??")) untracked.Add(rel);
            }
            if (paths.Count == 0) return Pending("niente da salvare: i file di MDE sono già come nell'ultimo commit", candidates);

            if (untracked.Count > 0)
            {
                var add = await _git.RunAsync(root, new[] { "add", "--" }.Concat(untracked).ToArray());
                if (!add.Ok) return Pending("git add non riuscito: " + add.Describe(), paths);
            }
            var reasons = written.Where(kv => paths.Contains(Rel(root, kv.Key))).Select(kv => kv.Value).Distinct().ToList();
            var message = MessagePrefix + string.Join("; ", reasons);
            var body = "File di MDE salvati da MDE:\n" + string.Join("\n", paths.Select(p => "- " + p));
            var commit = await _git.RunAsync(root, new[] { "commit", "--only", "-m", message, "-m", body, "--" }.Concat(paths).ToArray());
            if (!commit.Ok) return Pending("commit non riuscito: " + commit.Describe(), paths);

            var push = await PushIfSafeAsync(root);
            return new MdeAutoCommitResult
            {
                RepositoryRoot = root, Files = paths, Reasons = reasons, Committed = true, CommitMessage = message,
                Pushed = push == null, Pending = push,
            };
        }

        /// <summary>Null = pubblicato; altrimenti perché no.</summary>
        private async Task<string> PushIfSafeAsync(string root)
        {
            var upstream = await _git.RunAsync(root, new[] { "rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}" });
            if (!upstream.Ok || string.IsNullOrWhiteSpace(upstream.Stdout)) return "il ramo non ha un remoto da seguire: resta locale";
            var up = upstream.Stdout.Trim();
            var slash = up.IndexOf('/');
            if (slash <= 0) return "remoto non riconosciuto (" + up + "): resta locale";
            var remote = up.Substring(0, slash);
            var branch = (await _git.RunAsync(root, new[] { "symbolic-ref", "--short", "HEAD" })).Stdout?.Trim();
            if (string.IsNullOrEmpty(branch)) return "nessun ramo attivo: resta locale";

            using var scope = _scopes.CreateScope();
            var transport = scope.ServiceProvider.GetRequiredService<INativeGitTransport>();
            var fetch = await transport.FetchAsync(root, remote);
            if (!fetch.Ok) return "il remoto non risponde, il commit aspetta il tuo prossimo push (" + fetch.Stderr?.Trim() + ")";

            var behind = await _git.RunAsync(root, new[] { "rev-list", "--count", "HEAD..@{u}" });
            if (!behind.Ok || behind.Stdout.Trim() != "0") return "il remoto ha novità: il commit aspetta il prossimo pull e push";
            var ahead = await _git.RunAsync(root, new[] { "log", "--format=%s", "@{u}..HEAD" });
            var subjects = (ahead.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (subjects.Any(s => !s.StartsWith(MessagePrefix, StringComparison.Ordinal)))
                return "hai commit tuoi non ancora pubblicati: il commit di MDE parte con il tuo prossimo push";

            var push = await transport.PushAsync(root, remote, branch);
            return push.Ok ? null : "push non riuscito, il commit aspetta il tuo prossimo push (" + push.Stderr?.Trim() + ")";
        }

        /// <summary>La radice del repository git che contiene il file (dove c'è <c>.git</c>, cartella o file di un submodule).</summary>
        private static string FindRepositoryRoot(string fullPath)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(fullPath));
            while (!string.IsNullOrEmpty(dir))
            {
                var dotGit = Path.Combine(dir, ".git");
                if (Directory.Exists(dotGit) || File.Exists(dotGit)) return dir;
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }

        /// <summary>
        /// Un file di MDE rispetto alla radice del PROGETTO, che può essere una sottocartella del repository: si prova ogni cartella
        /// fra il file e la radice del repository (<c>sotto/.development.yml</c> è di MDE se il progetto è <c>sotto/</c>).
        /// </summary>
        private static bool IsOwned(string repositoryRoot, string fullPath)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(fullPath));
            var stop = Path.GetFullPath(repositoryRoot).TrimEnd(Path.DirectorySeparatorChar);
            while (!string.IsNullOrEmpty(dir))
            {
                if (MdeOwnedFiles.Find(dir, fullPath) != null) return true;
                if (string.Equals(dir.TrimEnd(Path.DirectorySeparatorChar), stop, StringComparison.OrdinalIgnoreCase)) return false;
                dir = Path.GetDirectoryName(dir);
            }
            return false;
        }

        private static string Rel(string root, string fullPath) => Path.GetRelativePath(root, fullPath).Replace('\\', '/');
    }
}
