using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Hubs;
using MdExplorer.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>Perché il commit non c'è o il push non è partito.</summary>
    public enum MdeCommitPending
    {
        None,
        NotARepository,
        MergeInProgress,
        NoIdentity,
        NothingToSave,
        CommitFailed,
        NoUpstream,
        RemoteNotReachable,
        RemoteHasNews,
        UserCommitsToPublish,
        PushFailed,
        /// <summary>Commit fatto prima di un pull: si pubblica dopo, con il push dell'utente.</summary>
        BeforePull,
        Error,
    }

    /// <summary>Che cosa ha fatto il commit automatico per un repository, e che cosa si è detto all'utente.</summary>
    public sealed class MdeAutoCommitResult
    {
        public string RepositoryRoot { get; init; }
        /// <summary>I file di MDE (registro <see cref="MdeOwnedFiles"/>), relativi alla radice del repository.</summary>
        public IReadOnlyList<string> Files { get; init; } = Array.Empty<string>();
        public IReadOnlyList<MdeWhy> Reasons { get; init; } = Array.Empty<MdeWhy>();
        public bool Committed { get; init; }
        public string CommitMessage { get; init; }
        public bool Pushed { get; init; }
        public MdeCommitPending PendingKind { get; init; }
        /// <summary>Il dettaglio del motivo (stderr di git e simili), già nella lingua dell'interfaccia.</summary>
        public string PendingDetail { get; init; }
        /// <summary>File dell'utente toccati da MDE (per esempio <c>.gitignore</c>): niente commit, solo la notifica.</summary>
        public IReadOnlyList<(string File, MdeWhy Why)> UserFiles { get; init; } = Array.Empty<(string, MdeWhy)>();
        /// <summary>Le notifiche mandate all'interfaccia per questo giro, nella lingua dell'interfaccia.</summary>
        public IReadOnlyList<string> Notices { get; init; } = Array.Empty<string>();
        public DateTime At { get; init; } = DateTime.Now;
    }

    /// <summary>
    /// S2-S3 dello sprint «File di sistema di MDE» (docs-internal/Sprints/2026-11-07-File-Di-Sistema-MDE.md): MDE non lascia mai
    /// modificati senza commit i suoi file del repository, e ogni modifica che fa a un file del repository la dice, con il motivo.
    /// Prima li lasciava, e al primo cambio di un collega il pull si bloccava (progetto A, progetto B).
    /// <para>
    /// Ascolta <see cref="MdeRepoWriter.Written"/>, raccoglie le scritture per repository per <c>MdeAutoCommit:DelayMs</c> (un'operazione
    /// scrive più volte: un commit e una notifica soli) e poi:
    /// </para>
    /// <list type="number">
    /// <item>file di MDE (<see cref="MdeOwnedFiles"/>), non ignorati e diversi dall'ultimo commit: <c>git commit --only</c> «MDE: …»
    /// (motivo in italiano: è contenuto del repository); lo stage dell'utente resta com'è;</item>
    /// <item>niente commit durante un'unione, un rebase o un cherry-pick a metà, o senza identità git: lo si dice, si riprova dopo;</item>
    /// <item>push SOLO nel caso sicuro: dopo un fetch il remoto non ha novità e i commit da pubblicare sono tutti «MDE: …»;</item>
    /// <item>file NON di MDE toccati da MDE (<c>.gitignore</c>, file tolti dal versionamento): nessun commit, sono dell'utente; solo la notifica;</item>
    /// <item>la notifica (evento SignalR <c>mdeRepoChange</c> a tutte le finestre) nella lingua dell'interfaccia, letta qui, in
    /// background e fuori da ogni transazione.</item>
    /// </list>
    /// Si spegne con <c>MdeAutoCommit:Enabled=false</c> (lo fanno i test d'integrazione, tranne quelli di questa funzione).
    /// </summary>
    public sealed class MdeAutoCommit : IHostedService, IDisposable
    {
        public const string MessagePrefix = "MDE: ";
        public const string NoticeEvent = "mdeRepoChange";

        private readonly INativeGitRunner _git;
        private readonly IServiceScopeFactory _scopes;
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly ILogger<MdeAutoCommit> _logger;
        private readonly bool _enabled;
        private readonly int _delayMs;

        private readonly object _lock = new();
        private sealed class Batch
        {
            public readonly Dictionary<string, MdeWhy> Owned = new(StringComparer.OrdinalIgnoreCase);
            public readonly Dictionary<string, MdeWhy> User = new(StringComparer.OrdinalIgnoreCase);
            public Timer Timer;
        }
        private readonly Dictionary<string, Batch> _pending = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, Task> _running = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentQueue<MdeAutoCommitResult> _results = new();

        public MdeAutoCommit(INativeGitRunner git, IServiceScopeFactory scopes, IHubContext<MonitorMDHub> hub, IConfiguration configuration, ILogger<MdeAutoCommit> logger)
        {
            _git = git;
            _scopes = scopes;
            _hub = hub;
            _logger = logger;
            _enabled = configuration?["MdeAutoCommit:Enabled"] is not { } v || !string.Equals(v, "false", StringComparison.OrdinalIgnoreCase);
            _delayMs = int.TryParse(configuration?["MdeAutoCommit:DelayMs"], out var d) ? d : 1500;
        }

        /// <summary>Acceso (<c>MdeAutoCommit:Enabled</c>): spento, MDE non fa commit da sé, nemmeno prima del pull.</summary>
        public bool Enabled => _enabled;

        /// <summary>
        /// S5: prima di un pull fatto da MDE, il commit dei file di MDE rimasti modificati (versioni vecchie, scritture di un momento fa).
        /// Senza push: il remoto ha novità per definizione, si pubblica con il prossimo push dell'utente. Null se spento o niente da fare.
        /// </summary>
        public async Task<MdeAutoCommitResult> CommitPendingAsync(string repositoryRoot, CancellationToken ct = default)
        {
            if (!_enabled) return null;
            await FlushAsync(ct);
            var root = Path.GetFullPath(repositoryRoot).TrimEnd(Path.DirectorySeparatorChar);
            var status = await _git.RunAsync(root, new[] { "status", "--porcelain", "--untracked-files=all" }, ct);
            if (!status.Ok) return null;
            var batch = new Batch();
            foreach (var line in (status.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Length < 4) continue;
                var rel = line.Substring(3).Trim().Trim('"');
                var arrow = rel.IndexOf(" -> ", StringComparison.Ordinal);
                if (arrow >= 0) rel = rel.Substring(arrow + 4);
                var full = Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar));
                if (IsOwned(root, full))
                    batch.Owned[full] = new MdeWhy("modifiche di MDE rimaste senza commit, salvate prima di scaricare", "MDE changes left without a commit, saved before pulling");
            }
            if (batch.Owned.Count == 0) return null;
            return await HandleAsync(root, batch, push: false);
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

        private void OnWritten(string fullPath, MdeWhy why)
        {
            var root = FindRepositoryRoot(fullPath);
            if (root == null) return; // non è in un repository git: niente da salvare né da dire
            var owned = IsOwned(root, fullPath);
            lock (_lock)
            {
                if (!_pending.TryGetValue(root, out var batch))
                {
                    batch = new Batch();
                    batch.Timer = new Timer(_ => Fire(root));
                    _pending[root] = batch;
                }
                (owned ? batch.Owned : batch.User)[fullPath] = why ?? MdeWhy.Generic;
                batch.Timer.Change(_delayMs, Timeout.Infinite);
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
            Batch batch;
            lock (_lock)
            {
                if (!_pending.Remove(root, out batch)) return;
                batch.Timer.Dispose();
            }
            // un giro alla volta per repository: il successivo aspetta il precedente
            _running.AddOrUpdate(root,
                _ => Task.Run(() => HandleAsync(root, batch)),
                (_, previous) => previous.ContinueWith(_ => HandleAsync(root, batch)).Unwrap());
        }

        private async Task<MdeAutoCommitResult> HandleAsync(string root, Batch batch, bool push = true)
        {
            MdeAutoCommitResult result;
            try { result = batch.Owned.Count > 0 ? await CommitCoreAsync(root, batch.Owned, push) : new MdeAutoCommitResult { RepositoryRoot = root, PendingKind = MdeCommitPending.NothingToSave }; }
            catch (Exception ex)
            {
                result = new MdeAutoCommitResult { RepositoryRoot = root, Files = batch.Owned.Keys.Select(p => Rel(root, p)).ToList(), Reasons = batch.Owned.Values.Distinct().ToList(), PendingKind = MdeCommitPending.Error, PendingDetail = ex.Message };
            }

            var userFiles = batch.User.Select(kv => (Rel(root, kv.Key), kv.Value)).ToList();
            var notices = Compose(result, userFiles);
            result = new MdeAutoCommitResult
            {
                RepositoryRoot = result.RepositoryRoot, Files = result.Files, Reasons = result.Reasons, Committed = result.Committed,
                CommitMessage = result.CommitMessage, Pushed = result.Pushed, PendingKind = result.PendingKind, PendingDetail = result.PendingDetail,
                UserFiles = userFiles, Notices = notices,
            };
            _results.Enqueue(result);
            while (_results.Count > 50) _results.TryDequeue(out _);

            foreach (var notice in notices) _logger.LogInformation("[MdeAutoCommit] {Root}: {Notice}", root, notice);
            if (notices.Count > 0)
            {
                try
                {
                    await _hub.Clients.All.SendAsync(NoticeEvent, new
                    {
                        repositoryRoot = root,
                        files = result.Files.Concat(userFiles.Select(u => u.Item1)).ToList(),
                        committed = result.Committed,
                        pushed = result.Pushed,
                        pending = result.PendingKind.ToString(),
                        notices,
                    });
                }
                catch (Exception ex) { _logger.LogWarning(ex, "[MdeAutoCommit] notifica non mandata"); }
            }
            return result;
        }

        /// <summary>Le notifiche del giro, nella lingua dell'interfaccia. «Niente da salvare» non si dice.</summary>
        private static List<string> Compose(MdeAutoCommitResult r, IReadOnlyList<(string File, MdeWhy Why)> userFiles)
        {
            static string Why(IEnumerable<MdeWhy> whys) => string.Join("; ", whys.Select(w => UiText.Say(w.It, w.En)).Distinct());
            var notices = new List<string>();
            if (r.Files.Count > 0 && r.PendingKind != MdeCommitPending.NothingToSave)
            {
                var files = string.Join(", ", r.Files);
                var head = UiText.Say($"MDE ha aggiornato {files}: {Why(r.Reasons)}.", $"MDE updated {files}: {Why(r.Reasons)}.");
                var tail = r.Pushed
                    ? UiText.Say(" Commit fatto e pubblicato.", " Committed and pushed.")
                    : r.Committed
                        ? UiText.Say(" Commit fatto, non pubblicato: ", " Committed, not pushed: ") + PendingText(r.PendingKind, r.PendingDetail)
                        : UiText.Say(" Commit rimandato: ", " Commit postponed: ") + PendingText(r.PendingKind, r.PendingDetail);
                notices.Add(head + tail);
            }
            foreach (var group in userFiles.GroupBy(u => u.Why))
            {
                var files = string.Join(", ", group.Select(u => u.File));
                notices.Add(UiText.Say(
                    $"MDE ha modificato {files}: {group.Key.It}. Il file è tuo: fai il commit quando vuoi.",
                    $"MDE changed {files}: {group.Key.En}. The file is yours: commit it when you want."));
            }
            return notices;
        }

        private static string PendingText(MdeCommitPending kind, string detail)
        {
            var text = kind switch
            {
                MdeCommitPending.MergeInProgress => UiText.Say("c'è un'unione o un rebase a metà, il commit aspetta che finisca", "a merge or a rebase is half done, the commit waits for it to end"),
                MdeCommitPending.NoIdentity => UiText.Say("git non sa chi sei (user.email)", "git does not know who you are (user.email)"),
                MdeCommitPending.NoUpstream => UiText.Say("il ramo non ha un remoto da seguire, resta locale", "the branch has no remote to follow, it stays local"),
                MdeCommitPending.RemoteNotReachable => UiText.Say("il remoto non risponde, parte con il tuo prossimo push", "the remote does not answer, it goes with your next push"),
                MdeCommitPending.RemoteHasNews => UiText.Say("il remoto ha novità, parte con il prossimo pull e push", "the remote has news, it goes with the next pull and push"),
                MdeCommitPending.UserCommitsToPublish => UiText.Say("hai commit tuoi non ancora pubblicati, parte con il tuo prossimo push", "you have commits of your own not yet pushed, it goes with your next push"),
                MdeCommitPending.PushFailed => UiText.Say("push non riuscito, parte con il tuo prossimo push", "push failed, it goes with your next push"),
                MdeCommitPending.BeforePull => UiText.Say("fatto prima di scaricare, parte con il tuo prossimo push", "made before pulling, it goes with your next push"),
                MdeCommitPending.CommitFailed => UiText.Say("commit non riuscito", "commit failed"),
                MdeCommitPending.NotARepository => UiText.Say("non è un repository git", "not a git repository"),
                _ => UiText.Say("errore", "error"),
            };
            return string.IsNullOrWhiteSpace(detail) ? text + "." : $"{text} ({detail.Trim()}).";
        }

        private async Task<MdeAutoCommitResult> CommitCoreAsync(string root, Dictionary<string, MdeWhy> written, bool push = true)
        {
            MdeAutoCommitResult Stop(MdeCommitPending kind, string detail = null, IReadOnlyList<string> files = null, string message = null, bool committed = false)
                => new()
                {
                    RepositoryRoot = root, Files = files ?? written.Keys.Select(p => Rel(root, p)).ToList(), Reasons = written.Values.Distinct().ToList(),
                    Committed = committed, CommitMessage = message, PendingKind = kind, PendingDetail = detail,
                };

            var gitDir = await _git.RunAsync(root, new[] { "rev-parse", "--absolute-git-dir" });
            if (!gitDir.Ok) return Stop(MdeCommitPending.NotARepository);
            var g = gitDir.Stdout.Trim();
            if (File.Exists(Path.Combine(g, "MERGE_HEAD")) || Directory.Exists(Path.Combine(g, "rebase-merge"))
                || Directory.Exists(Path.Combine(g, "rebase-apply")) || File.Exists(Path.Combine(g, "CHERRY_PICK_HEAD")))
                return Stop(MdeCommitPending.MergeInProgress);
            var email = await _git.RunAsync(root, new[] { "config", "user.email" });
            if (!email.Ok || string.IsNullOrWhiteSpace(email.Stdout)) return Stop(MdeCommitPending.NoIdentity);

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
            if (paths.Count == 0) return Stop(MdeCommitPending.NothingToSave, files: candidates);

            if (untracked.Count > 0)
            {
                var add = await _git.RunAsync(root, new[] { "add", "--" }.Concat(untracked).ToArray());
                if (!add.Ok) return Stop(MdeCommitPending.CommitFailed, add.Describe(), paths);
            }
            var reasons = written.Where(kv => paths.Contains(Rel(root, kv.Key))).Select(kv => kv.Value).Distinct().ToList();
            var message = MessagePrefix + string.Join("; ", reasons.Select(r => r.It).Distinct());
            var body = "File di MDE salvati da MDE:\n" + string.Join("\n", paths.Select(p => "- " + p));
            var commit = await _git.RunAsync(root, new[] { "commit", "--only", "-m", message, "-m", body, "--" }.Concat(paths).ToArray());
            if (!commit.Ok) return Stop(MdeCommitPending.CommitFailed, commit.Describe(), paths);

            var (pushKind, pushDetail) = push ? await PushIfSafeAsync(root) : (MdeCommitPending.BeforePull, null);
            return new MdeAutoCommitResult
            {
                RepositoryRoot = root, Files = paths, Reasons = reasons, Committed = true, CommitMessage = message,
                Pushed = pushKind == MdeCommitPending.None, PendingKind = pushKind, PendingDetail = pushDetail,
            };
        }

        /// <summary>None = pubblicato; altrimenti perché no.</summary>
        private async Task<(MdeCommitPending Kind, string Detail)> PushIfSafeAsync(string root)
        {
            var upstream = await _git.RunAsync(root, new[] { "rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}" });
            if (!upstream.Ok || string.IsNullOrWhiteSpace(upstream.Stdout)) return (MdeCommitPending.NoUpstream, null);
            var up = upstream.Stdout.Trim();
            var slash = up.IndexOf('/');
            if (slash <= 0) return (MdeCommitPending.NoUpstream, up);
            var remote = up.Substring(0, slash);
            var branch = (await _git.RunAsync(root, new[] { "symbolic-ref", "--short", "HEAD" })).Stdout?.Trim();
            if (string.IsNullOrEmpty(branch)) return (MdeCommitPending.NoUpstream, null);

            using var scope = _scopes.CreateScope();
            var transport = scope.ServiceProvider.GetRequiredService<INativeGitTransport>();
            var fetch = await transport.FetchAsync(root, remote);
            if (!fetch.Ok) return (MdeCommitPending.RemoteNotReachable, fetch.Stderr);

            var behind = await _git.RunAsync(root, new[] { "rev-list", "--count", "HEAD..@{u}" });
            if (!behind.Ok || behind.Stdout.Trim() != "0") return (MdeCommitPending.RemoteHasNews, null);
            var ahead = await _git.RunAsync(root, new[] { "log", "--format=%s", "@{u}..HEAD" });
            var subjects = (ahead.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (subjects.Any(s => !s.StartsWith(MessagePrefix, StringComparison.Ordinal))) return (MdeCommitPending.UserCommitsToPublish, null);

            var push = await transport.PushAsync(root, remote, branch);
            return push.Ok ? (MdeCommitPending.None, null) : (MdeCommitPending.PushFailed, push.Stderr);
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
