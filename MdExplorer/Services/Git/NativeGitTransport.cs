using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Come è andata un'operazione di rete del git nativo, già letta per chi deve raccontarla
    /// all'utente: non solo «exit 128», ma «manca la credenziale per questo host».
    /// </summary>
    public enum NativeGitFailureKind
    {
        None = 0,
        /// <summary>git non ha trovato una credenziale nel suo credential helper e, senza terminale, non può chiederla.</summary>
        CredentialsMissing,
        /// <summary>Il server ha rifiutato la credenziale (401/403, chiave SSH non accettata).</summary>
        AuthenticationFailed,
        /// <summary>Host non risolto, connessione rifiutata, timeout di rete, TLS.</summary>
        Network,
        /// <summary>Il repository non esiste (404) o l'URL non è un repository git.</summary>
        NotFound,
        /// <summary>git non è nel PATH.</summary>
        GitMissing,
        /// <summary>Il processo non è finito entro il tempo massimo.</summary>
        Timeout,
        Other,
    }

    public sealed class NativeGitOutcome
    {
        public bool Ok { get; init; }
        public int ExitCode { get; init; }
        public string Stdout { get; init; } = string.Empty;
        public string Stderr { get; init; } = string.Empty;
        public NativeGitFailureKind Kind { get; init; }

        /// <summary>Lo stderr di git così com'è, ripulito: è il messaggio che l'utente deve leggere.</summary>
        public string Error => Kind switch
        {
            NativeGitFailureKind.None => string.Empty,
            NativeGitFailureKind.GitMissing => "git non trovato nel PATH: installa Git e riavvia MdExplorer",
            NativeGitFailureKind.Timeout => "git non ha risposto entro il tempo massimo",
            _ => string.IsNullOrWhiteSpace(Stderr) ? $"git exit {ExitCode}" : Stderr.Trim(),
        };
    }

    /// <summary>
    /// <b>L'unico meccanismo</b> con cui MdExplorer parla con un remoto git: il git di sistema.
    /// Push, pull, fetch, clone e ls-remote passano di qui, e l'autenticazione la fa il
    /// credential helper di git (Git Credential Manager su Windows, store/keychain/libsecret
    /// altrove), lo stesso che l'utente usa dal terminale. MdExplorer non risolve, non conserva
    /// e non inietta credenziali: al massimo, quando l'utente le digita nella maschera del clone,
    /// le consegna a git con <c>git credential approve</c> e gliele fa dimenticare con
    /// <c>reject</c> se il server le rifiuta.
    /// <para>
    /// Con <c>GIT_TERMINAL_PROMPT=0</c> (impostato da <see cref="NativeGitRunner"/>) git non resta
    /// mai appeso ad aspettare una password che nessuno digiterà: fallisce subito e
    /// <see cref="Classify"/> traduce lo stderr in un <see cref="NativeGitFailureKind"/>.
    /// </para>
    /// </summary>
    public interface INativeGitTransport
    {
        Task<NativeGitOutcome> PushAsync(string repositoryPath, string remoteName, string branchName, CancellationToken ct = default);
        Task<NativeGitOutcome> PullAsync(string repositoryPath, CancellationToken ct = default);
        Task<NativeGitOutcome> FetchAsync(string repositoryPath, string remoteName, CancellationToken ct = default);
        /// <summary><c>git ls-remote --heads</c>: la prova più leggera che il remoto risponde e la credenziale è buona.</summary>
        Task<NativeGitOutcome> LsRemoteAsync(string workingDirectory, string remoteNameOrUrl, CancellationToken ct = default);
        Task<NativeGitOutcome> CloneAsync(string url, string localPath, string branchName, CancellationToken ct = default);
        /// <summary>Consegna al credential helper di git una credenziale digitata dall'utente. Non verifica: quello lo fa l'operazione dopo.</summary>
        Task<NativeGitOutcome> ApproveCredentialAsync(string url, string username, string password, CancellationToken ct = default);
        /// <summary>Fa dimenticare a git una credenziale che il server ha rifiutato, così non la ripropone.</summary>
        Task<NativeGitOutcome> RejectCredentialAsync(string url, string username, CancellationToken ct = default);
    }

    public sealed class NativeGitTransport : INativeGitTransport
    {
        private const int LsRemoteTimeoutMs = 60_000;
        private readonly INativeGitRunner _git;
        private readonly ILogger<NativeGitTransport> _logger;

        public NativeGitTransport(INativeGitRunner git, ILogger<NativeGitTransport> logger)
        {
            _git = git ?? throw new ArgumentNullException(nameof(git));
            _logger = logger;
        }

        public Task<NativeGitOutcome> PushAsync(string repositoryPath, string remoteName, string branchName, CancellationToken ct = default)
            // -u: il primo push di un branch nuovo lo mette a tracciare il remoto; sui successivi è innocuo.
            => Run(repositoryPath, new[] { "push", "-u", remoteName ?? "origin", string.IsNullOrEmpty(branchName) ? "HEAD" : branchName }, ct);

        public Task<NativeGitOutcome> PullAsync(string repositoryPath, CancellationToken ct = default)
            // --no-rebase: lo stesso merge che faceva Commands.Pull, senza dipendere da pull.rebase dell'utente.
            => Run(repositoryPath, new[] { "pull", "--no-rebase" }, ct);

        public Task<NativeGitOutcome> FetchAsync(string repositoryPath, string remoteName, CancellationToken ct = default)
            => Run(repositoryPath, new[] { "fetch", remoteName ?? "origin" }, ct);

        public Task<NativeGitOutcome> LsRemoteAsync(string workingDirectory, string remoteNameOrUrl, CancellationToken ct = default)
            => Run(workingDirectory, new[] { "ls-remote", "--heads", remoteNameOrUrl }, ct, LsRemoteTimeoutMs);

        public Task<NativeGitOutcome> CloneAsync(string url, string localPath, string branchName, CancellationToken ct = default)
        {
            var args = new List<string> { "clone" };
            if (!string.IsNullOrEmpty(branchName)) { args.Add("--branch"); args.Add(branchName); }
            args.Add(url);
            args.Add(localPath);
            // La cartella di lavoro è il padre: la destinazione ancora non esiste.
            var parent = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(localPath)) ?? System.IO.Path.GetTempPath();
            return Run(parent, args.ToArray(), ct);
        }

        public Task<NativeGitOutcome> ApproveCredentialAsync(string url, string username, string password, CancellationToken ct = default)
            => Credential("approve", url, username, password, ct);

        public Task<NativeGitOutcome> RejectCredentialAsync(string url, string username, CancellationToken ct = default)
            => Credential("reject", url, username, null, ct);

        private async Task<NativeGitOutcome> Credential(string verb, string url, string username, string password, CancellationToken ct)
        {
            // `url=` lascia a git il compito di scomporre schema, host CON la porta e path:
            // farlo a mano è esattamente il punto dove il codice precedente perdeva la porta.
            var sb = new StringBuilder();
            sb.Append("url=").Append(StripUserInfo(url)).Append('\n');
            if (!string.IsNullOrEmpty(username)) sb.Append("username=").Append(username).Append('\n');
            if (password != null) sb.Append("password=").Append(password).Append('\n');
            sb.Append('\n');
            var r = await _git.RunAsync(System.IO.Path.GetTempPath(), new[] { "credential", verb }, ct, stdin: sb.ToString());
            if (!r.Ok)
                _logger.LogWarning("[git] credential {Verb} per {Url}: {Why}", verb, StripUserInfo(url), r.Describe());
            return ToOutcome(r);
        }

        private async Task<NativeGitOutcome> Run(string cwd, string[] args, CancellationToken ct, int timeoutMs = NativeGitRunner.DefaultTimeoutMs)
        {
            var r = await _git.RunAsync(cwd, args, ct, timeoutMs: timeoutMs);
            var outcome = ToOutcome(r);
            if (!outcome.Ok)
                _logger.LogWarning("[git] {Args} in {Dir}: {Kind} — {Err}", string.Join(' ', args), cwd, outcome.Kind, outcome.Error);
            return outcome;
        }

        private static NativeGitOutcome ToOutcome(GitResult r) => new()
        {
            Ok = r.Ok,
            ExitCode = r.ExitCode,
            Stdout = r.Stdout ?? string.Empty,
            Stderr = r.Stderr ?? string.Empty,
            Kind = r.Ok ? NativeGitFailureKind.None : Classify(r),
        };

        /// <summary>
        /// Legge lo stderr di git e dice che cosa è successo. L'ordine conta: «unable to access …
        /// returned error: 403» contiene anche parole da rete, ma è un rifiuto della credenziale.
        /// </summary>
        public static NativeGitFailureKind Classify(GitResult r)
        {
            if (r.ExitCode == NativeGitRunner.GitNotFoundExit) return NativeGitFailureKind.GitMissing;
            if (r.ExitCode == NativeGitRunner.GitTimeoutExit) return NativeGitFailureKind.Timeout;
            var e = (r.Stderr ?? string.Empty).ToLowerInvariant();
            if (Any(e, "could not read username", "could not read password", "terminal prompts disabled"))
                return NativeGitFailureKind.CredentialsMissing;
            if (Any(e, "authentication failed", "returned error: 401", "returned error: 403", "http 401", "http 403",
                       "invalid credentials", "logon failed", "permission denied (publickey)", "access denied", "invalid username or password"))
                return NativeGitFailureKind.AuthenticationFailed;
            if (Any(e, "repository not found", "returned error: 404", "does not appear to be a git repository", "not found"))
                return NativeGitFailureKind.NotFound;
            if (Any(e, "could not resolve host", "connection refused", "failed to connect", "timed out", "network is unreachable",
                       "no route to host", "connection reset", "ssl", "certificate", "unable to access"))
                return NativeGitFailureKind.Network;
            return NativeGitFailureKind.Other;
        }

        private static bool Any(string haystack, params string[] needles) => needles.Any(haystack.Contains);

        private static string StripUserInfo(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.UserInfo))
                return new UriBuilder(uri) { UserName = string.Empty, Password = string.Empty }.Uri.ToString();
            return url;
        }
    }
}
