using System;
using System.Collections.Concurrent;
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

        public async Task<NativeGitOutcome> PushAsync(string repositoryPath, string remoteName, string branchName, CancellationToken ct = default)
            // -u: il primo push di un branch nuovo lo mette a tracciare il remoto; sui successivi è innocuo.
            => await Gated(await RemoteUrlAsync(repositoryPath, remoteName ?? "origin", ct),
                () => Run(repositoryPath, new[] { "push", "-u", remoteName ?? "origin", string.IsNullOrEmpty(branchName) ? "HEAD" : branchName }, ct), ct);

        public async Task<NativeGitOutcome> PullAsync(string repositoryPath, CancellationToken ct = default)
            // --no-rebase: lo stesso merge che faceva Commands.Pull, senza dipendere da pull.rebase dell'utente.
            => await Gated(await RemoteUrlAsync(repositoryPath, "origin", ct),
                () => Run(repositoryPath, new[] { "pull", "--no-rebase" }, ct), ct);

        public async Task<NativeGitOutcome> FetchAsync(string repositoryPath, string remoteName, CancellationToken ct = default)
            => await Gated(await RemoteUrlAsync(repositoryPath, remoteName ?? "origin", ct),
                () => Run(repositoryPath, new[] { "fetch", remoteName ?? "origin" }, ct), ct);

        public async Task<NativeGitOutcome> LsRemoteAsync(string workingDirectory, string remoteNameOrUrl, CancellationToken ct = default)
        {
            var url = LooksLikeUrl(remoteNameOrUrl) ? remoteNameOrUrl : await RemoteUrlAsync(workingDirectory, remoteNameOrUrl, ct);
            return await Gated(url, () => Run(workingDirectory, new[] { "ls-remote", "--heads", remoteNameOrUrl }, ct, LsRemoteTimeoutMs), ct);
        }

        public Task<NativeGitOutcome> CloneAsync(string url, string localPath, string branchName, CancellationToken ct = default)
        {
            var args = new List<string> { "clone" };
            if (!string.IsNullOrEmpty(branchName)) { args.Add("--branch"); args.Add(branchName); }
            args.Add(url);
            args.Add(localPath);
            // La cartella di lavoro è il padre: la destinazione ancora non esiste.
            var parent = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(localPath)) ?? System.IO.Path.GetTempPath();
            return Gated(url, () => Run(parent, args.ToArray(), ct), ct);
        }

        // ------------------------------------------------------------------ un login alla volta

        /// <summary>
        /// Un cancello per host. Su Windows ogni processo git che non trova la credenziale apre il
        /// SUO login di Git Credential Manager nel browser: all'apertura di un progetto toolbar e
        /// polling chiedono insieme remote-status e get-data-to-pull, e l'utente si è trovato
        /// davanti tante finestre di login tutte insieme (23/09/2026). Qui le operazioni di rete
        /// verso lo stesso host passano una alla volta: la prima fa il login, le altre trovano la
        /// credenziale già salvata.
        /// </summary>
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> HostGates = new();
        /// <summary>L'ultimo fallimento di autenticazione per host, con l'ora in cui è finito.</summary>
        private static readonly ConcurrentDictionary<string, (DateTime At, NativeGitOutcome Outcome)> LastAuthFailure = new();

        private async Task<NativeGitOutcome> Gated(string url, Func<Task<NativeGitOutcome>> op, CancellationToken ct)
        {
            var key = HostKey(url);
            if (key == null) return await op();

            var queuedAt = DateTime.UtcNow;
            var gate = HostGates.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                // Mentre aspettavo, chi era davanti ha provato e l'autenticazione è fallita (login
                // chiuso, credenziale mancante): riprovare subito aprirebbe un'altra finestra
                // identica. Prendo il suo esito. Una chiamata NUOVA, partita dopo, riprova davvero.
                if (LastAuthFailure.TryGetValue(key, out var last) && last.At > queuedAt)
                {
                    _logger?.LogInformation("[git] {Host}: login appena fallito per un'altra operazione, non ne apro un altro", key);
                    return last.Outcome;
                }

                var outcome = await op();
                if (outcome.Kind is NativeGitFailureKind.CredentialsMissing or NativeGitFailureKind.AuthenticationFailed)
                    LastAuthFailure[key] = (DateTime.UtcNow, outcome);
                else
                    LastAuthFailure.TryRemove(key, out _);
                return outcome;
            }
            finally
            {
                gate.Release();
            }
        }

        /// <summary><c>scheme://host[:porta]</c> dell'URL; per <c>git@host:path</c> <c>ssh://host</c>. Null se non si capisce.</summary>
        private static string HostKey(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.Host))
                return uri.IsDefaultPort ? $"{uri.Scheme}://{uri.Host}" : $"{uri.Scheme}://{uri.Host}:{uri.Port}";
            var at = url.IndexOf('@'); var colon = url.IndexOf(':', Math.Max(at, 0));
            if (at >= 0 && colon > at) return "ssh://" + url.Substring(at + 1, colon - at - 1);
            return null;
        }

        private static bool LooksLikeUrl(string s) => !string.IsNullOrEmpty(s) && (s.Contains("://") || s.Contains('@'));

        private async Task<string> RemoteUrlAsync(string repositoryPath, string remoteName, CancellationToken ct)
        {
            var r = await _git.RunAsync(repositoryPath, new[] { "remote", "get-url", remoteName }, ct);
            return r.Ok ? r.Stdout?.Trim() : null;
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
