using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>Esito di <see cref="IGitAccountAligner.AlignAsync"/>.</summary>
    public sealed class AccountAlignment
    {
        /// <summary>Vero se git ritrova la credenziale con l'account scritto nel repository.</summary>
        public bool Ok { get; init; }
        /// <summary>L'account scritto nel repository (quello chiesto, o quello vero se era diverso).</summary>
        public string Account { get; init; }
        /// <summary>Cosa è stato corretto, da dire all'utente; null se non c'era niente da correggere.</summary>
        public string Note { get; init; }
        /// <summary>Perché non è andata, da dire all'utente.</summary>
        public string Error { get; init; }
    }

    /// <summary>
    /// Dopo un'operazione di rete riuscita che può aver fatto un login (il primo push di «Collega a un
    /// repository remoto», il clone): l'account scritto nel repository in
    /// <c>credential.&lt;scheme://host&gt;.username</c> deve essere quello sotto cui il credential
    /// helper ha salvato la credenziale, altrimenti git non la ritrova e chiede il login a ogni
    /// operazione. Git Credential Manager salva sotto il login GitHub REALE e cerca l'utente
    /// richiesto con confronto esatto (visto l'08/10/2026 con l'organizzazione «dedabit» scritta
    /// al posto dell'account).
    /// </summary>
    public interface IGitAccountAligner
    {
        /// <param name="requestedAccount">L'account che l'utente ha chiesto (già scritto nel repository), o null.</param>
        /// <param name="accountsBefore">Gli account noti al helper PRIMA dell'operazione (<see cref="INativeGitTransport.KnownAccountsAsync"/>), o null.</param>
        Task<AccountAlignment> AlignAsync(string repositoryPath, string remoteUrl, string requestedAccount, KnownAccounts accountsBefore, CancellationToken ct = default);
    }

    public sealed class GitAccountAligner : IGitAccountAligner
    {
        private readonly INativeGitTransport _transport;
        private readonly INativeGitRunner _git;
        private readonly ILogger<GitAccountAligner> _logger;

        public GitAccountAligner(INativeGitTransport transport, INativeGitRunner git, ILogger<GitAccountAligner> logger)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _git = git ?? throw new ArgumentNullException(nameof(git));
            _logger = logger;
        }

        /// <summary>
        /// Verifica senza mai aprire una finestra; se l'account chiesto non ritrova la credenziale, lo
        /// chiede a git (fill senza utente) o lo deduce dall'account comparso in Git Credential
        /// Manager con l'operazione, lo riscrive e lo dice. Se non riesce a capirlo, lo dice: non indovina.
        /// Per un remoto non http(s) non c'è niente da allineare.
        /// </summary>
        public async Task<AccountAlignment> AlignAsync(string repositoryPath, string remoteUrl, string requestedAccount, KnownAccounts accountsBefore, CancellationToken ct = default)
        {
            if (!Uri.TryCreate(remoteUrl, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                return new AccountAlignment { Ok = true, Account = requestedAccount };
            var host = uri.Host;
            requestedAccount = string.IsNullOrWhiteSpace(requestedAccount) ? null : requestedAccount.Trim();

            var key = GitCredentialMoveService.HostKeyOf(remoteUrl);
            if (requestedAccount != null)
            {
                var asRequested = await _transport.LookupCredentialAsync(remoteUrl, requestedAccount, repositoryPath, ct);
                if (asRequested.Found)
                {
                    // Chi arriva dal clone non ha ancora la chiave nel repository: la si scrive qui (idempotente).
                    var write = await _git.RunAsync(repositoryPath, new[] { "config", key, requestedAccount }, ct);
                    if (!write.Ok) return new AccountAlignment { Ok = false, Account = requestedAccount, Error = $"git config {key}: {write.Describe()}" };
                    return new AccountAlignment { Ok = true, Account = requestedAccount };
                }
                _logger.LogWarning("[account] git non ritrova la credenziale di {Host} con l'account '{User}': {Why}", host, requestedAccount, asRequested.Error);
            }

            // Sotto quale account l'ha salvata? Prima glielo si chiede: con un solo account il helper
            // risponde. Ma «senza utente» dev'essere senza davvero: con la chiave nel repository git
            // inietterebbe da solo l'account sbagliato. La si toglie, e la si riscrive subito dopo.
            if (requestedAccount != null)
                await _git.RunAsync(repositoryPath, new[] { "config", "--unset", key }, ct);
            var any = await _transport.LookupCredentialAsync(remoteUrl, null, repositoryPath, ct);
            var account = any.Found && !string.IsNullOrEmpty(any.Username) ? any.Username : null;
            if (account == null)
            {
                // Più account: quello comparso con l'operazione è quello del login appena fatto.
                var after = await _transport.KnownAccountsAsync(remoteUrl, ct);
                var fresh = after.Accounts.Except(accountsBefore?.Accounts ?? Array.Empty<string>()).ToList();
                if (fresh.Count == 1) account = fresh[0];
                else if (after.Accounts.Count == 1) account = after.Accounts[0];
                else
                {
                    if (requestedAccount != null)
                        await _git.RunAsync(repositoryPath, new[] { "config", key, requestedAccount }, ct); // com'era: l'errore spiega
                    var known = after.Accounts.Count > 0 ? $" Accounts known to git for {host}: {string.Join(", ", after.Accounts)}." : string.Empty;
                    var requested = requestedAccount == null ? "no account was given" : $"the account '{requestedAccount}' is not the one the login was stored under";
                    return new AccountAlignment
                    {
                        Ok = false, Account = requestedAccount,
                        Error = $"git does not find the credential for {host}: {requested}.{known} Connect again choosing the account you log in with (your login, not the organization)."
                    };
                }
            }

            var cfg = await _git.RunAsync(repositoryPath, new[] { "config", key, account }, ct);
            if (!cfg.Ok) return new AccountAlignment { Ok = false, Account = requestedAccount, Error = $"git config {key}: {cfg.Describe()}" };

            var check = await _transport.LookupCredentialAsync(remoteUrl, account, repositoryPath, ct);
            if (!check.Found)
                return new AccountAlignment { Ok = false, Account = account, Error = $"git does not find the credential for {host} even with the account '{account}': {check.Error}" };

            var note = requestedAccount == null
                ? $"Git account for {host}: {account}"
                : $"The git account for {host} is '{account}', not '{requestedAccount}': fixed in the repository";
            _logger.LogInformation("[account] {Note}", note);
            return new AccountAlignment { Ok = true, Account = account, Note = note };
        }
    }
}
