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

        public GenericRemoteService(ILogger<GenericRemoteService> logger, IModernGitService modernGitService, INativeGitRunner git)
        {
            _logger = logger;
            _modernGitService = modernGitService ?? throw new ArgumentNullException(nameof(modernGitService));
            _git = git ?? throw new ArgumentNullException(nameof(git));
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
                if (!string.IsNullOrWhiteSpace(request.AccountUsername)
                    && Uri.TryCreate(request.RemoteUrl, UriKind.Absolute, out var uri)
                    && (uri.Scheme == "http" || uri.Scheme == "https"))
                {
                    var key = GitCredentialMoveService.HostKeyOf(request.RemoteUrl);
                    var cfg = await _git.RunAsync(request.RepositoryPath, new[] { "config", key, request.AccountUsername.Trim() });
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

                var result = new SetupRemoteGenericResult { RemoteUrl = request.RemoteUrl };
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
                var push = await _modernGitService.PushAsync(request.RepositoryPath, remoteName, branchName);
                result.PushSucceeded = push.Success;
                result.DurationMs = stopwatch.ElapsedMilliseconds;
                if (push.Success)
                {
                    result.Success = true;
                    result.Message = $"Remote configured and {branchName} pushed to {remoteName}";
                    return result;
                }
                result.Success = false;
                result.Message = "Remote configured, but the push failed";
                result.Error = push.ErrorMessage;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setup remote failed for {Repo}", request.RepositoryPath);
                return Fail($"Failed to setup remote: {ex.Message}");
            }
        }
    }
}
