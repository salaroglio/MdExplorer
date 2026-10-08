using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using MdExplorer.Services.Git.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// «Collega a un repository remoto»: scrive il remote nel repository, dice a git quale account
    /// usare per quell'host, mette il branch a tracciare il remoto e fa il primo push col git di
    /// sistema. Il repository remoto deve esistere già: l'ha creato l'utente sul provider.
    /// <para>
    /// Prima di questo sprint qui dentro si creava il repository via API con un token, si salvavano
    /// le credenziali in tre posti e si rispondeva <c>success: true</c> anche col push fallito.
    /// Ora la risposta è vera: se il push non passa, <see cref="SetupRemoteGenericResult.Success"/>
    /// è falso e <see cref="SetupRemoteGenericResult.Error"/> è lo stderr di git.
    /// </para>
    /// </summary>
    public class GenericRemoteService : IGenericRemoteService
    {
        private readonly ILogger<GenericRemoteService> _logger;
        private readonly IModernGitService _modernGitService;
        private readonly INativeGitRunner _git;
        private readonly INativeGitTransport _transport;

        public GenericRemoteService(ILogger<GenericRemoteService> logger, IModernGitService modernGitService, INativeGitRunner git,
            INativeGitTransport transport)
        {
            _logger = logger;
            _modernGitService = modernGitService ?? throw new ArgumentNullException(nameof(modernGitService));
            _git = git ?? throw new ArgumentNullException(nameof(git));
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        }

        public async Task<SetupRemoteGenericResult> SetupRemoteGenericAsync(SetupRemoteGenericRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var remoteName = string.IsNullOrWhiteSpace(request.RemoteName) ? "origin" : request.RemoteName;
            SetupRemoteGenericResult Fail(string error) => new()
            {
                Success = false, Error = error, RemoteUrl = request.RemoteUrl, DurationMs = stopwatch.ElapsedMilliseconds
            };

            if (string.IsNullOrWhiteSpace(request.RepositoryPath) || !Directory.Exists(request.RepositoryPath))
                return Fail($"La cartella del progetto non esiste: {request.RepositoryPath}");
            if (!Repository.IsValid(request.RepositoryPath))
                return Fail("La cartella del progetto non è un repository git: inizializzalo prima");
            if (string.IsNullOrWhiteSpace(request.RemoteUrl))
                return Fail("L'URL del repository remoto è obbligatorio");

            try
            {
                // 1. Il remote: nuovo, o riallineato all'URL che l'utente ha dato.
                var existing = await _git.RunAsync(request.RepositoryPath, new[] { "remote", "get-url", remoteName });
                if (existing.Ok)
                {
                    if (existing.Stdout.Trim() != request.RemoteUrl.Trim())
                    {
                        var setUrl = await _git.RunAsync(request.RepositoryPath, new[] { "remote", "set-url", remoteName, request.RemoteUrl });
                        if (!setUrl.Ok) return Fail($"git remote set-url: {setUrl.Describe()}");
                        _logger.LogInformation("Remote '{Remote}' riallineato a {Url}", remoteName, request.RemoteUrl);
                    }
                }
                else
                {
                    var add = await _git.RunAsync(request.RepositoryPath, new[] { "remote", "add", remoteName, request.RemoteUrl });
                    if (!add.Ok) return Fail($"git remote add: {add.Describe()}");
                    _logger.LogInformation("Remote '{Remote}' aggiunto: {Url}", remoteName, request.RemoteUrl);
                }

                // 2. Quale account per questo host: è così che git, con più account, sceglie quello giusto.
                var isHttp = Uri.TryCreate(request.RemoteUrl, UriKind.Absolute, out var uri)
                    && (uri.Scheme == "http" || uri.Scheme == "https");
                var requestedAccount = string.IsNullOrWhiteSpace(request.AccountUsername) ? null : request.AccountUsername.Trim();
                if (isHttp && requestedAccount != null)
                {
                    var key = GitCredentialMoveService.HostKeyOf(request.RemoteUrl);
                    var cfg = await _git.RunAsync(request.RepositoryPath, new[] { "config", key, requestedAccount });
                    if (!cfg.Ok) return Fail($"git config {key}: {cfg.Describe()}");
                }

                // 3. Il primo commit, se il progetto ha file ma nessun commit: senza, non c'è niente da pubblicare.
                string branchName;
                using (var repo = new Repository(request.RepositoryPath))
                {
                    if (repo.Head?.Tip == null && request.PushAfterAdd)
                    {
                        var hasFiles = repo.RetrieveStatus().Any(s => s.State != FileStatus.Ignored && s.State != FileStatus.Nonexistent);
                        if (hasFiles)
                        {
                            Commands.Stage(repo, "*");
                            var signature = repo.Config.BuildSignature(DateTimeOffset.Now)
                                ?? new Signature("MdExplorer", "mdexplorer@local", DateTimeOffset.Now);
                            repo.Commit("Initial commit", signature, signature);
                            _logger.LogInformation("Primo commit creato in {Repo}", request.RepositoryPath);
                        }
                    }
                    branchName = repo.Head?.FriendlyName;
                }

                // 4. Il branch traccia il remoto anche senza push (col push, -u lo fa da solo).
                if (!string.IsNullOrEmpty(branchName) && branchName != "(no branch)")
                {
                    await _git.RunAsync(request.RepositoryPath, new[] { "config", $"branch.{branchName}.remote", remoteName });
                    await _git.RunAsync(request.RepositoryPath, new[] { "config", $"branch.{branchName}.merge", $"refs/heads/{branchName}" });
                }

                var result = new SetupRemoteGenericResult { RemoteUrl = request.RemoteUrl, AccountUsername = requestedAccount };
                if (!request.PushAfterAdd)
                {
                    result.Success = true;
                    result.Message = "Remote configured successfully";
                    result.DurationMs = stopwatch.ElapsedMilliseconds;
                    return result;
                }

                // 5. Il primo push: lo fa git, e se serve un login lo chiede il suo credential manager.
                bool hasCommits;
                using (var repo = new Repository(request.RepositoryPath)) hasCommits = repo.Head?.Tip != null;
                if (!hasCommits)
                {
                    result.Success = true;
                    result.Message = "Remote configured; nothing to push yet (no commits)";
                    result.DurationMs = stopwatch.ElapsedMilliseconds;
                    return result;
                }
                result.PushAttempted = true;
                // Gli account che il credential helper conosce PRIMA del push: se il push fa un login
                // nuovo, l'account che compare dopo è quello sotto cui l'ha salvato.
                var accountsBefore = isHttp ? await _transport.KnownAccountsAsync(request.RemoteUrl) : null;
                var push = await _modernGitService.PushAsync(request.RepositoryPath, remoteName, branchName);
                result.PushSucceeded = push.Success;
                if (!push.Success)
                {
                    result.Success = false;
                    result.Message = "Remote configured, but the push failed";
                    result.Error = push.ErrorMessage;
                    result.DurationMs = stopwatch.ElapsedMilliseconds;
                    return result;
                }

                // 6. Il push è passato, ma passerà anche la prossima operazione? Git Credential Manager
                // salva il login sotto il login GitHub REALE e, se gli si chiede un altro utente, lo
                // cerca con quello e non lo trova: login a ogni operazione (visto l'08/10/2026 con
                // l'organizzazione «dedabit» scritta al posto dell'account). Qui si verifica, senza
                // mai aprire una finestra, e se serve si riscrive l'account con quello vero.
                var aligned = isHttp
                    ? await AlignAccountAsync(request.RepositoryPath, request.RemoteUrl, requestedAccount, accountsBefore)
                    : (Ok: true, Account: requestedAccount, Note: (string)null, Error: (string)null);
                result.AccountUsername = aligned.Account;
                result.DurationMs = stopwatch.ElapsedMilliseconds;
                if (aligned.Ok)
                {
                    result.Success = true;
                    result.Message = $"Remote configured and {branchName} pushed to {remoteName}"
                        + (aligned.Note == null ? string.Empty : $". {aligned.Note}");
                    return result;
                }
                result.Success = false;
                result.Message = $"Remote configured and {branchName} pushed to {remoteName}, but git would ask for the login again at the next operation";
                result.Error = aligned.Error;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setup remote failed for {Repo}", request.RepositoryPath);
                return Fail($"Failed to setup remote: {ex.Message}");
            }
        }

        /// <summary>
        /// Dopo un push riuscito: l'account scritto nel repository deve essere quello sotto cui il
        /// credential helper ha salvato la credenziale, altrimenti git non la ritrova. Se non lo è,
        /// lo chiede a git (fill senza utente) o lo deduce dall'account comparso in Git Credential
        /// Manager col push, lo riscrive e lo dice. Se non riesce a capirlo, lo dice: non indovina.
        /// </summary>
        private async Task<(bool Ok, string Account, string Note, string Error)> AlignAccountAsync(
            string repositoryPath, string remoteUrl, string requestedAccount, KnownAccounts accountsBefore)
        {
            var host = new Uri(remoteUrl).Host;
            if (requestedAccount != null)
            {
                var asRequested = await _transport.LookupCredentialAsync(remoteUrl, requestedAccount, repositoryPath);
                if (asRequested.Found) return (true, requestedAccount, null, null);
                _logger.LogWarning("[collega] git non ritrova la credenziale di {Host} con l'account '{User}': {Why}", host, requestedAccount, asRequested.Error);
            }

            // Sotto quale account l'ha salvata? Prima glielo si chiede: con un solo account il helper
            // risponde. Ma «senza utente» dev'essere senza davvero: con la chiave nel repository git
            // inietterebbe da solo l'account sbagliato. La si toglie, e la si riscrive subito dopo.
            var key = GitCredentialMoveService.HostKeyOf(remoteUrl);
            if (requestedAccount != null)
                await _git.RunAsync(repositoryPath, new[] { "config", "--unset", key });
            var any = await _transport.LookupCredentialAsync(remoteUrl, null, repositoryPath);
            var account = any.Found && !string.IsNullOrEmpty(any.Username) ? any.Username : null;
            if (account == null)
            {
                // Più account: quello comparso col push è quello del login appena fatto.
                var after = await _transport.KnownAccountsAsync(remoteUrl);
                var fresh = after.Accounts.Except(accountsBefore?.Accounts ?? Array.Empty<string>()).ToList();
                if (fresh.Count == 1) account = fresh[0];
                else if (after.Accounts.Count == 1) account = after.Accounts[0];
                else
                {
                    if (requestedAccount != null)
                        await _git.RunAsync(repositoryPath, new[] { "config", key, requestedAccount }); // com'era: l'errore spiega
                    var known = after.Accounts.Count > 0 ? $" Accounts known to git for {host}: {string.Join(", ", after.Accounts)}." : string.Empty;
                    var requested = requestedAccount == null ? "no account was given" : $"the account '{requestedAccount}' is not the one the login was stored under";
                    return (false, requestedAccount, null,
                        $"git does not find the credential for {host}: {requested}.{known} Connect again choosing the account you log in with (your login, not the organization).");
                }
            }

            var cfg = await _git.RunAsync(repositoryPath, new[] { "config", key, account });
            if (!cfg.Ok) return (false, requestedAccount, null, $"git config {key}: {cfg.Describe()}");

            var check = await _transport.LookupCredentialAsync(remoteUrl, account, repositoryPath);
            if (!check.Found)
                return (false, account, null, $"git does not find the credential for {host} even with the account '{account}': {check.Error}");

            var note = requestedAccount == null
                ? $"Git account for {host}: {account}"
                : $"The git account for {host} is '{account}', not '{requestedAccount}': fixed in the repository";
            _logger.LogInformation("[collega] {Note}", note);
            return (true, account, note, null);
        }
    }
}
