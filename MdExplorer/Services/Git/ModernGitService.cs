using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MdExplorer.Services.Git.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.DB;

namespace MdExplorer.Services.Git
{
    public class ModernGitService : IModernGitService
    {
        private readonly IEnumerable<ICredentialResolver> _credentialResolvers;
        private readonly ILogger<ModernGitService> _logger;
        private readonly GitAuthenticationOptions _authOptions;
        private readonly GitOperationOptions _operationOptions;
        private readonly IUserSettingsDB _userSettingsDB;

        /// <summary>L'unico meccanismo di rete: il git di sistema col suo credential manager (vedi <see cref="NativeGitTransport"/>).</summary>
        private readonly INativeGitTransport _transport;

        public ModernGitService(
            IEnumerable<ICredentialResolver> credentialResolvers,
            ILogger<ModernGitService> logger,
            IUserSettingsDB userSettingsDB,
            INativeGitTransport transport,
            IOptions<GitAuthenticationOptions> authOptions = null,
            IOptions<GitOperationOptions> operationOptions = null,
            IProjectSubmoduleInitializer submodules = null,
            ISubmoduleBranchAttacher attacher = null)
        {
            _credentialResolvers = credentialResolvers?.OrderBy(r => r.GetPriority()) ?? throw new ArgumentNullException(nameof(credentialResolvers));
            _transport = transport ?? throw new ArgumentNullException(nameof(transport), "INativeGitTransport non registrato: push, pull, fetch e clone passano di lì");
            _logger = logger;
            _userSettingsDB = userSettingsDB;
            _authOptions = authOptions?.Value ?? new GitAuthenticationOptions();
            _operationOptions = operationOptions?.Value ?? new GitOperationOptions();
            _submodules = submodules;
            _attacher = attacher;
        }

        /// <summary>
        /// Chi popola i submodule. Clone e pull passano di qui invece di farlo per conto loro,
        /// così esiste un solo posto che sa come si popolano e un solo posto che racconta com'è
        /// andata: prima il fallimento finiva appeso al messaggio di successo del clone, il clone
        /// risultava riuscito, la cartella restava vuota e nessuno leggeva quel pezzo di frase.
        /// Opzionale perché il servizio git viene costruito anche fuori dal grafo completo.
        /// </summary>
        private readonly IProjectSubmoduleInitializer _submodules;

        /// <summary>
        /// Chi rimette i submodule sul loro ramo dopo un aggiornamento. Condiviso con l'apertura
        /// del progetto: senza, il riaggancio sarebbe solo su clone e pull, e chi apre una cartella
        /// clonata da fuori se li ritroverebbe staccati per sempre.
        /// </summary>
        private readonly ISubmoduleBranchAttacher _attacher;

        /// <summary>Un riaggancio non riuscito non invalida l'operazione: i file sono gia' quelli giusti.</summary>
        private async Task AttachSafely(string repositoryPath)
        {
            try { await _attacher.AttachAsync(repositoryPath); }
            catch (Exception ex) { _logger.LogWarning(ex, "[Submodule] riaggancio non riuscito in '{Path}'.", repositoryPath); }
        }

        /// <summary>
        /// Popola i submodule tramite l'inizializzatore condiviso; senza di lui (costruzione
        /// isolata) ripiega sul comando diretto, che fa la stessa cosa ma senza notifica.
        /// </summary>
        private async Task<GitOperationResult> EnsureSubmodulesAsync(string repositoryPath)
        {
            // L'inizializzatore popola i submodule VUOTI, con le sue notifiche. Non basta dopo un
            // pull: se il submodule c'e' gia' ma sta a un commit vecchio, lui dice «gia' tutti
            // popolati» e non tocca niente — e il codice resta indietro rispetto alla
            // documentazione appena tirata giu'. Verificato con un test: pull fast-forward
            // riuscito, contenuto del submodule ancora quello di prima.
            if (_submodules != null)
            {
                var ensured = await _submodules.EnsureAsync(repositoryPath);
                if (!ensured.Success)
                    return new GitOperationResult { Success = false, ErrorMessage = ensured.Error };
                if (ensured.NothingToDo && !File.Exists(Path.Combine(repositoryPath, ".gitmodules")))
                    return new GitOperationResult { Success = true, Message = "No submodules" };
            }

            // E poi si allinea davvero ai commit registrati: e' un no-op quando sono gia' li'.
            var updated = await UpdateSubmodulesAsync(repositoryPath);
            if (updated.Success && _attacher != null) await AttachSafely(repositoryPath);
            return updated;
        }

        /// <summary>
        /// Sets the Git execution context with repository path and known username.
        /// This allows credential resolvers to use the correct account without prompting.
        /// </summary>
        private void SetGitExecutionContext(string repositoryPath)
        {
            GitExecutionContext.CurrentRepositoryPath = repositoryPath;
            GitExecutionContext.CurrentUsername = null; // Reset first

            try
            {
                // Look up saved account for this repository
                var normalizedPath = Path.GetFullPath(repositoryPath);
                using var tx = _userSettingsDB.BeginTransaction();
                var accountDal = _userSettingsDB.GetDal<GitRepositoryAccount>();
                var credentialDal = _userSettingsDB.GetDal<GitCredential>();

                // Fetch all active accounts first, then filter in memory
                // (Path.GetFullPath cannot be translated to SQL by NHibernate)
                var allAccounts = accountDal.GetList().Where(a => a.IsActive).ToList();
                var account = allAccounts.FirstOrDefault(a =>
                    !string.IsNullOrEmpty(a.RepositoryPath) &&
                    Path.GetFullPath(a.RepositoryPath).Equals(normalizedPath, StringComparison.OrdinalIgnoreCase));

                if (account != null)
                {
                    // Load the associated GitCredential explicitly (NHibernate lazy loading doesn't work with convenience properties)
                    if (account.CredentialId.HasValue)
                    {
                        account.Credential = credentialDal.GetList()
                            .FirstOrDefault(c => c.Id == account.CredentialId.Value);

                        _logger.LogDebug("[GitContext] Loaded credential {CredentialId} for {RepoPath}",
                            account.CredentialId, repositoryPath);
                    }

                    // Now AuthUsername will work correctly (reads from Credential.AuthUsername)
                    if (!string.IsNullOrEmpty(account.AuthUsername))
                    {
                        GitExecutionContext.CurrentUsername = account.AuthUsername;
                        _logger.LogInformation("[GitContext] Set username for {RepoPath}: {Username}",
                            repositoryPath, account.AuthUsername);
                    }
                    else
                    {
                        _logger.LogDebug("[GitContext] Account found but no AuthUsername for {RepoPath}", repositoryPath);
                    }
                }
                else
                {
                    _logger.LogDebug("[GitContext] No saved account for {RepoPath}", repositoryPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[GitContext] Failed to lookup account for {RepoPath}", repositoryPath);
            }
        }

        /// <summary>
        /// Le opzioni di ogni lettura dello stato: i submodule restano <b>fuori</b>.
        /// <para>
        /// Non è più configurabile per progetto (colonna rimossa il 18/08/2026). La manopola
        /// esisteva perché un submodule sporco teneva acceso per sempre il pulsante Commit della
        /// toolbar; quel pulsante ora legge la vista per repository, che dice <i>cosa</i> c'è e
        /// <i>dove</i>, quindi non c'era più niente da tarare.
        /// </para>
        /// <para>
        /// L'esclusione resta perché serve a chi legge ancora questo stato — l'avviso prima del
        /// cambio di ramo — e lì è la risposta giusta: cambiare ramo nel progetto non tocca il
        /// contenuto dei submodule, quindi un submodule sporco non è un motivo per fermarti.
        /// </para>
        /// </summary>
        private static StatusOptions BuildStatusOptions(string repositoryPath)
            => new StatusOptions { ExcludeSubmodules = true };

        /// <summary>
        /// Full-path normalization tolerant of a trailing directory separator, so that a
        /// project path and a <see cref="RepositoryInformation.WorkingDirectory"/> (which carries
        /// a trailing slash) compare equal.
        /// </summary>
        private static string NormalizeRepositoryPath(string path)
            => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

public async Task<GitOperationResult> PullAsync(string repositoryPath)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting pull operation for repository: {RepositoryPath}", repositoryPath);
                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                    Duration = stopwatch.Elapsed
                };
                }

                string headCommitBefore;
                using (var before = new Repository(repositoryPath))
                    headCommitBefore = before.Head.Tip?.Sha;

                // Il git nativo fa il pull e la sua autenticazione: MdExplorer non tocca credenziali.
                var pull = await _transport.PullAsync(repositoryPath);
                if (!pull.Ok)
                {
                    stopwatch.Stop();
                    var conflicts = pull.Stdout.Contains("CONFLICT") || pull.Stderr.Contains("CONFLICT");
                    return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = conflicts ? $"Pull completed but conflicts need to be resolved: {pull.Error}" : $"Pull failed: {pull.Error}",
                    Duration = stopwatch.Elapsed
                };
                }

                string headCommitAfter;
                IEnumerable<string> changes;
                using (var after = new Repository(repositoryPath))
                {
                    headCommitAfter = after.Head.Tip?.Sha;
                    changes = headCommitBefore != headCommitAfter
                        ? GetCommitDiffPaths(after, headCommitBefore, headCommitAfter)
                        : new string[0];
                }
                var hasChanges = headCommitBefore != headCommitAfter;
                stopwatch.Stop();
                var message = hasChanges ? "Pull completed" : "Repository is up to date";
                _logger.LogInformation("Pull operation completed, HasChanges: {HasChanges}, Duration: {Duration}ms",
                    hasChanges, stopwatch.ElapsedMilliseconds);

                // Populate/refresh submodules after pull (native git; no-op when .gitmodules absent)
                var submoduleResult = await EnsureSubmodulesAsync(repositoryPath);
                if (!submoduleResult.Success)
                {
                    message += $" (warning: submodule update failed: {submoduleResult.ErrorMessage})";
                }

                _lastUsedAuthMethod = AuthenticationMethod.GitCredentialHelper;
                return new GitOperationResult
                {
                    Success = true,
                    Message = message,
                    HasChanges = hasChanges,
                    Changes = changes,
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during pull operation for repository: {RepositoryPath}", repositoryPath);
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Pull failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

public async Task<GitOperationResult> PushAsync(string repositoryPath, string remoteName = "origin", string branchName = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting push operation for repository: {RepositoryPath}, Remote: {Remote}, Branch: {Branch}",
                    repositoryPath, remoteName, branchName ?? "current");
                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                    Duration = stopwatch.Elapsed
                };
                }

                string branchToPush;
                using (var repo = new Repository(repositoryPath))
                {
                    if (repo.Network.Remotes[remoteName] == null)
                    {
                        return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Remote '{remoteName}' not found",
                    Duration = stopwatch.Elapsed
                };
                    }
                    var branch = string.IsNullOrEmpty(branchName) ? repo.Head : repo.Branches[branchName];
                    if (branch == null)
                    {
                        return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Branch '{branchName}' not found",
                    Duration = stopwatch.Elapsed
                };
                    }
                    branchToPush = branch.FriendlyName;
                }

                _logger.LogInformation("Executing push to remote: {Remote}, Branch: {Branch}", remoteName, branchToPush);
                var push = await _transport.PushAsync(repositoryPath, remoteName, branchToPush);
                stopwatch.Stop();
                if (!push.Ok)
                {
                    _logger.LogError("Push failed for {RepositoryPath}: {Kind} — {Error}", repositoryPath, push.Kind, push.Error);
                    return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Push failed: {push.Error}",
                    Duration = stopwatch.Elapsed
                };
                }

                _logger.LogInformation("Push operation completed successfully, Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
                _lastUsedAuthMethod = AuthenticationMethod.GitCredentialHelper;
                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully pushed {branchToPush} to {remoteName}",
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during push operation for repository: {RepositoryPath}", repositoryPath);
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Push failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitOperationResult> CommitAsync(string repositoryPath, string message, GitAuthor author)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting commit operation for repository: {RepositoryPath}", repositoryPath);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = "Commit message cannot be empty",
                        Duration = stopwatch.Elapsed
                    };
                }

                using var repo = new Repository(repositoryPath);
                
                // Stage all changes
                Commands.Stage(repo, "*");

                // Check if there are any changes to commit
                var status = repo.RetrieveStatus(BuildStatusOptions(repositoryPath));
                if (!status.IsDirty)
                {
                    stopwatch.Stop();
                    return new GitOperationResult
                    {
                        Success = true,
                        Message = "No changes to commit",
                        Duration = stopwatch.Elapsed
                    };
                }

                // Create signature
                var signature = new Signature(author.Name, author.Email, DateTimeOffset.Now);

                // Commit
                var commit = repo.Commit(message, signature, signature);

                stopwatch.Stop();

                _logger.LogInformation("Commit operation completed successfully: {CommitHash}, Duration: {Duration}ms",
                    commit.Sha, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully committed changes",
                    CommitHash = commit.Sha,
                    Changes = GetStagedFiles(repo),
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during commit operation for repository: {RepositoryPath}", repositoryPath);
                
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Commit failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitOperationResult> CommitAndPushAsync(string repositoryPath, string message, GitAuthor author, string remoteName = "origin")
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Starting commit and push operation for repository: {RepositoryPath}", repositoryPath);

                // First commit
                var commitResult = await CommitAsync(repositoryPath, message, author);
                if (!commitResult.Success)
                {
                    return commitResult;
                }

                // If no changes were committed, don't try to push
                if (commitResult.Message == "No changes to commit")
                {
                    return commitResult;
                }

                // Then push
                var pushResult = await PushAsync(repositoryPath, remoteName);
                if (!pushResult.Success)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Commit succeeded but push failed: {pushResult.ErrorMessage}",
                        CommitHash = commitResult.CommitHash,
                        Duration = stopwatch.Elapsed
                    };
                }

                stopwatch.Stop();

                return new GitOperationResult
                {
                    Success = true,
                    Message = "Successfully committed and pushed changes",
                    CommitHash = commitResult.CommitHash,
                    Changes = commitResult.Changes,
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = pushResult.AuthenticationMethodUsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during commit and push operation for repository: {RepositoryPath}", repositoryPath);
                
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Commit and push failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

public async Task<GitOperationResult> CloneAsync(string url, string localPath, string branchName = null,
            bool useSavedToken = true, string username = null, string password = null)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Clone {Url} → {LocalPath} (branch: {Branch}, typed credentials: {Typed})",
                    url, localPath, branchName ?? "(default)", !string.IsNullOrEmpty(username));

                // Auto-create parent directories if they don't exist
                // This supports the Share Project feature where basePath may include nested folders
                var parentDirectory = System.IO.Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(parentDirectory) && !Directory.Exists(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }
                if (Directory.Exists(localPath) && Directory.GetFileSystemEntries(localPath).Length > 0)
                {
                    return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Target directory is not empty: {localPath}",
                    Duration = stopwatch.Elapsed
                };
                }

                // Se l'utente ha digitato utente e password nella maschera, le consegniamo a git:
                // da qui in poi le conserva il suo credential helper, non MdExplorer.
                var typed = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
                if (typed)
                {
                    var approve = await _transport.ApproveCredentialAsync(url, username, password);
                    if (!approve.Ok)
                    {
                        stopwatch.Stop();
                        return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Clone failed: impossibile consegnare la credenziale a git: {approve.Error}",
                    Duration = stopwatch.Elapsed
                };
                    }
                }

                var clone = await _transport.CloneAsync(url, localPath, branchName);
                if (!clone.Ok)
                {
                    if (typed && clone.Kind is NativeGitFailureKind.AuthenticationFailed or NativeGitFailureKind.CredentialsMissing)
                    {
                        // Il server l'ha rifiutata: git non deve riproporla al prossimo tentativo.
                        await _transport.RejectCredentialAsync(url, username);
                    }
                    stopwatch.Stop();
                    return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Clone failed: {clone.Error}",
                    Duration = stopwatch.Elapsed
                };
                }

                var fileCount = EnsureBranchCheckedOutAfterClone(localPath);
                var cloneMessage = $"Successfully cloned repository ({fileCount} items)";

                // Populate submodules after clone (native git; no-op when .gitmodules absent)
                var submoduleUpdateResult = await EnsureSubmodulesAsync(localPath);
                if (!submoduleUpdateResult.Success)
                {
                    cloneMessage += $" (warning: submodule update failed: {submoduleUpdateResult.ErrorMessage})";
                }
                stopwatch.Stop();
                _lastUsedAuthMethod = AuthenticationMethod.GitCredentialHelper;
                return new GitOperationResult
                {
                    Success = true,
                    Message = cloneMessage,
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during clone operation: {Url} to {LocalPath}", url, localPath);
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Clone failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Dopo il clone, se HEAD è staccato o senza commit (remoto con HEAD che punta a un ramo
        /// inesistente), mette in checkout il primo ramo che esiste tra master, develop e main,
        /// tracciandolo. Ritorna quanti elementi ci sono nella cartella di lavoro.
        /// </summary>
        private int EnsureBranchCheckedOutAfterClone(string localPath)
        {
            int Count() => Directory.GetFileSystemEntries(localPath).Count(e => !e.EndsWith(".git"));
            try
            {
                using var repo = new Repository(localPath);
                var currentBranch = repo.Head.FriendlyName;
                var hasCommits = repo.Head.Tip != null;
                var isDetached = currentBranch == "(no branch)" || !repo.Head.CanonicalName.StartsWith("refs/heads/");
                if (hasCommits && !isDetached)
                {
                    return Count();
                }
                foreach (var preferredBranch in new[] { "master", "develop", "main" })
                {
                    var remoteBranch = repo.Branches[$"origin/{preferredBranch}"];
                    if (remoteBranch?.Tip == null) continue;
                    var localBranch = repo.Branches[preferredBranch];
                    if (localBranch == null)
                    {
                        localBranch = repo.CreateBranch(preferredBranch, remoteBranch.Tip);
                        repo.Branches.Update(localBranch, b => b.TrackedBranch = remoteBranch.CanonicalName);
                    }
                    Commands.Checkout(repo, localBranch);
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Post-clone branch check failed for {LocalPath} (non-fatal)", localPath);
            }
            return Count();
        }



        /// <summary>
        /// Checks if the error message indicates an authentication failure
        /// </summary>
        private bool IsAuthenticationError(string stderr)
        {
            if (string.IsNullOrEmpty(stderr)) return false;
            var lowerStderr = stderr.ToLowerInvariant();

            return lowerStderr.Contains("authentication failed") ||
                   lowerStderr.Contains("401") ||
                   lowerStderr.Contains("403") ||
                   lowerStderr.Contains("could not read username") ||
                   lowerStderr.Contains("invalid credentials") ||
                   lowerStderr.Contains("logon failed");
        }

        #region Submodules (native git CLI only — auth via git's own credential chain)

        private const int NativeGitNotFoundExitCode = -9999;
        private const int NativeGitTimeoutExitCode = -9998;

        /// <summary>
        /// Runs a native git command. Authentication is handled entirely by git's own
        /// credential chain (Git Credential Manager may show its own UI); MdExplorer never
        /// supplies credentials. GIT_TERMINAL_PROMPT=0 makes git fail fast with a clear
        /// error instead of hanging when no credential helper is configured.
        /// </summary>
        private async Task<(int ExitCode, string Stdout, string Stderr)> RunNativeGitAsync(
            string workingDirectory, string arguments, int timeoutMs = 300000)
        {
            _logger.LogInformation("Executing native git in {WorkingDirectory}: git {Arguments}", workingDirectory, arguments);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    // CreateNoWindow = false (same as ValidateWithGitCommandAsync): lets Git Credential Manager open its UI
                    CreateNoWindow = false
                }
            };
            process.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";

            try
            {
                process.Start();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _logger.LogError(ex, "git executable not found on PATH");
                return (NativeGitNotFoundExitCode, string.Empty, ex.Message);
            }

            // Start reading BEFORE waiting: reading after WaitForExit can deadlock when output fills the pipe buffer
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            var completed = await Task.Run(() => process.WaitForExit(timeoutMs));
            if (!completed)
            {
                try { process.Kill(); } catch { }
                _logger.LogError("git {Arguments} timed out after {TimeoutMs}ms", arguments, timeoutMs);
                return (NativeGitTimeoutExitCode, string.Empty, $"timeout after {timeoutMs / 1000}s");
            }

            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            _logger.LogInformation("git exited with code {ExitCode}", process.ExitCode);
            if (!string.IsNullOrEmpty(stdout))
                _logger.LogInformation("git stdout: {Stdout}", stdout);
            if (!string.IsNullOrEmpty(stderr))
                _logger.LogInformation("git stderr: {Stderr}", stderr);

            return (process.ExitCode, stdout, stderr);
        }

        public async Task<GitOperationResult> AddSubmoduleAsync(string repositoryPath, string url,
            string destinationRelativePath, string branchName = null)
        {
            var stopwatch = Stopwatch.StartNew();

            // Preconditions: explicit, actionable errors — no fallbacks
            if (string.IsNullOrWhiteSpace(repositoryPath) ||
                (!Directory.Exists(Path.Combine(repositoryPath, ".git")) &&
                 !File.Exists(Path.Combine(repositoryPath, ".git"))))
            {
                return SubmoduleFailure("The project folder is not a Git repository.", stopwatch);
            }

            url = url?.Trim();
            var hasValidScheme = !string.IsNullOrEmpty(url) &&
                                 (url.StartsWith("https://") || url.StartsWith("http://") ||
                                  url.StartsWith("ssh://") || url.StartsWith("git@"));
            if (!hasValidScheme || url.Contains("\""))
            {
                return SubmoduleFailure("Invalid repository URL. Use an https://, ssh:// or git@ URL.", stopwatch);
            }

            if (string.IsNullOrWhiteSpace(destinationRelativePath) ||
                Path.IsPathRooted(destinationRelativePath) ||
                destinationRelativePath.Contains("\""))
            {
                return SubmoduleFailure("Destination must be a relative path inside the project.", stopwatch);
            }

            var normalizedPath = destinationRelativePath.Replace('\\', '/').Trim().Trim('/');
            var projectRoot = Path.GetFullPath(repositoryPath);
            var destinationFull = Path.GetFullPath(Path.Combine(projectRoot, normalizedPath));
            var rootPrefix = projectRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!destinationFull.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return SubmoduleFailure("Destination path escapes the project root.", stopwatch);
            }

            if (Directory.Exists(destinationFull) && Directory.GetFileSystemEntries(destinationFull).Length > 0)
            {
                return SubmoduleFailure($"Destination folder '{normalizedPath}' already exists and is not empty.", stopwatch);
            }

            branchName = string.IsNullOrWhiteSpace(branchName) ? null : branchName.Trim();
            if (branchName != null &&
                (branchName.StartsWith("-") || branchName.Contains("\"") || branchName.Contains(" ")))
            {
                return SubmoduleFailure($"Invalid branch name: {branchName}", stopwatch);
            }

            var submoduleName = normalizedPath.Contains('/')
                ? normalizedPath.Substring(normalizedPath.LastIndexOf('/') + 1)
                : normalizedPath;

            try
            {
                var branchArg = branchName == null ? string.Empty : $"-b \"{branchName}\" ";
                var addArgs = $"submodule add {branchArg}-- \"{url}\" \"{normalizedPath}\"";
                var addResult = await RunNativeGitAsync(repositoryPath, addArgs);
                if (addResult.ExitCode != 0)
                {
                    return SubmoduleFailure(
                        MapSubmoduleError(addResult.ExitCode, addResult.Stderr, url, normalizedPath, branchName), stopwatch);
                }

                // Commit with pathspec: only .gitmodules + the gitlink, never unrelated staged changes
                var commitArgs = $"commit -m \"Add submodule {submoduleName}\" -- .gitmodules \"{normalizedPath}\"";
                var commitResult = await RunNativeGitAsync(repositoryPath, commitArgs);
                if (commitResult.ExitCode != 0)
                {
                    var commitOutput = (commitResult.Stderr + commitResult.Stdout).ToLowerInvariant();
                    if (commitOutput.Contains("please tell me who you are") ||
                        commitOutput.Contains("unable to auto-detect email"))
                    {
                        return SubmoduleFailure(
                            "Submodule was added and staged, but the commit failed: git user identity is not configured. " +
                            "Run: git config --global user.name \"Your Name\" and git config --global user.email \"you@example.com\", " +
                            "then commit manually.", stopwatch);
                    }
                    return SubmoduleFailure(
                        "Submodule was added and staged, but the commit failed: " +
                        MapSubmoduleError(commitResult.ExitCode, commitResult.Stderr, url, normalizedPath, branchName), stopwatch);
                }

                // Populate nested submodules; a failure here is a visible warning — the add itself succeeded
                var message = $"Submodule '{submoduleName}' added and committed.";
                var nestedResult = await EnsureSubmodulesAsync(repositoryPath);
                if (!nestedResult.Success)
                {
                    message += $" Warning: nested submodule init failed: {nestedResult.ErrorMessage}";
                }

                stopwatch.Stop();
                _logger.LogInformation("Submodule '{Name}' added at '{Path}' in {Duration}ms",
                    submoduleName, normalizedPath, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = message,
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during submodule add: {Url} -> {Path}", url, normalizedPath);
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Submodule add failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitOperationResult> UpdateSubmodulesAsync(string repositoryPath)
        {
            var stopwatch = Stopwatch.StartNew();

            // No .gitmodules → nothing to do, don't spawn a process on every pull
            if (string.IsNullOrWhiteSpace(repositoryPath) ||
                !File.Exists(Path.Combine(repositoryPath, ".gitmodules")))
            {
                stopwatch.Stop();
                return new GitOperationResult
                {
                    Success = true,
                    Message = "No submodules",
                    Duration = stopwatch.Elapsed
                };
            }

            try
            {
                var result = await RunNativeGitAsync(repositoryPath, "submodule update --init --recursive");
                stopwatch.Stop();

                if (result.ExitCode != 0)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = MapSubmoduleError(result.ExitCode, result.Stderr, null, null, null),
                        Duration = stopwatch.Elapsed
                    };
                }

                return new GitOperationResult
                {
                    Success = true,
                    Message = "Submodules updated",
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during submodule update for {RepositoryPath}", repositoryPath);
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Submodule update failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Maps native git stderr to an actionable user-facing message. Unknown errors are
        /// surfaced verbatim — never swallowed.
        /// </summary>
        private string MapSubmoduleError(int exitCode, string stderr, string url, string destinationPath, string branchName)
        {
            if (exitCode == NativeGitNotFoundExitCode)
                return "Git executable not found. Install Git for Windows and ensure 'git' is on PATH.";
            if (exitCode == NativeGitTimeoutExitCode)
                return "Git operation timed out after 5 minutes.";

            var lower = (stderr ?? string.Empty).ToLowerInvariant();

            if (lower.Contains("could not read username") || lower.Contains("terminal prompts disabled"))
                return $"Authentication required for {url}, but no Git credential helper answered. " +
                       "Configure Git Credential Manager (git config --global credential.helper manager) " +
                       "or authenticate once from a terminal, then retry.";

            if (IsAuthenticationError(stderr))
                return $"Authentication failed for {url}. Update your credentials in Git Credential Manager and retry.";

            if (lower.Contains("already exists in the index"))
                return $"Destination path '{destinationPath}' already contains tracked content or an existing submodule.";

            if (lower.Contains("already exists and is not an empty directory"))
                return $"Destination folder '{destinationPath}' already exists and is not empty.";

            if ((lower.Contains("repository") && lower.Contains("not found")) || lower.Contains("404"))
                return "Remote repository not found (or you do not have access to it).";

            if (lower.Contains("remote branch") && lower.Contains("not found"))
                return $"Branch '{branchName}' does not exist in the remote repository.";

            if (lower.Contains("not a git repository"))
                return "The project folder is not a Git repository.";

            return string.IsNullOrWhiteSpace(stderr) ? $"git exited with code {exitCode}" : stderr.Trim();
        }

        private GitOperationResult SubmoduleFailure(string error, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            _logger.LogWarning("Submodule operation failed: {Error}", error);
            return new GitOperationResult
            {
                Success = false,
                ErrorMessage = error,
                Duration = stopwatch.Elapsed
            };
        }

        #endregion



        public async Task<GitBranchInfo> GetCurrentBranchAsync(string repositoryPath)
        {
            try
            {
                using var repo = new Repository(repositoryPath);
                var currentBranch = repo.Head;

                return new GitBranchInfo
                {
                    Name = currentBranch.FriendlyName,
                    IsCurrent = true,
                    IsRemote = currentBranch.IsRemote,
                    CommitHash = currentBranch.Tip?.Sha,
                    LastCommitDate = currentBranch.Tip?.Committer.When.DateTime ?? DateTime.MinValue,
                    LastCommitMessage = currentBranch.Tip?.MessageShort,
                    CommitsAhead = currentBranch.TrackingDetails?.AheadBy ?? 0,
                    CommitsBehind = currentBranch.TrackingDetails?.BehindBy ?? 0,
                    RemoteTrackingBranch = currentBranch.TrackedBranch?.FriendlyName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current branch for repository: {RepositoryPath}", repositoryPath);
                return null;
            }
        }

        public async Task<IEnumerable<GitBranchInfo>> GetBranchesAsync(string repositoryPath, bool includeRemote = true)
        {
            try
            {
                using var repo = new Repository(repositoryPath);
                var branches = includeRemote ? repo.Branches : repo.Branches.Where(b => !b.IsRemote);

                return branches.Select(branch => new GitBranchInfo
                {
                    Name = branch.FriendlyName,
                    IsCurrent = branch.IsCurrentRepositoryHead,
                    IsRemote = branch.IsRemote,
                    CommitHash = branch.Tip?.Sha,
                    LastCommitDate = branch.Tip?.Committer.When.DateTime ?? DateTime.MinValue,
                    LastCommitMessage = branch.Tip?.MessageShort,
                    CommitsAhead = branch.TrackingDetails?.AheadBy ?? 0,
                    CommitsBehind = branch.TrackingDetails?.BehindBy ?? 0,
                    RemoteTrackingBranch = branch.TrackedBranch?.FriendlyName
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting branches for repository: {RepositoryPath}", repositoryPath);
                return new List<GitBranchInfo>();
            }
        }

        public async Task<GitOperationResult> CheckoutBranchAsync(string repositoryPath, string branchName)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Starting checkout operation for repository: {RepositoryPath}, Branch: {Branch}",
                    repositoryPath, branchName);

                // Set repository path and username in execution context for credential resolvers
                SetGitExecutionContext(repositoryPath);

                using var repo = new Repository(repositoryPath);

                var headCommitBefore = repo.Head.Tip?.Sha;

                // STEP 1: Try to find local branch first
                var branch = repo.Branches[branchName];

                // STEP 2: If not found locally, check if it's a remote branch
                if (branch == null)
                {
                    _logger.LogInformation("Local branch '{BranchName}' not found, searching remote branches", branchName);

                    // Try to find remote branch (check all remotes, typically "origin/branchName")
                    var remoteBranch = repo.Branches.FirstOrDefault(b =>
                        b.IsRemote && b.FriendlyName.EndsWith($"/{branchName}"));

                    if (remoteBranch != null)
                    {
                        _logger.LogInformation("Found remote branch: {RemoteBranch}, creating local tracking branch '{LocalBranch}'",
                            remoteBranch.FriendlyName, branchName);

                        // Create local branch from remote tip
                        branch = repo.CreateBranch(branchName, remoteBranch.Tip);

                        // Set up tracking relationship
                        repo.Branches.Update(branch, b => b.TrackedBranch = remoteBranch.CanonicalName);

                        _logger.LogInformation("✅ Created local tracking branch '{BranchName}' → '{RemoteBranch}'",
                            branchName, remoteBranch.FriendlyName);
                    }
                    else
                    {
                        // Still not found in local or remote - return error
                        return new GitOperationResult
                        {
                            Success = false,
                            ErrorMessage = $"Branch '{branchName}' not found in local or remote branches",
                            Duration = stopwatch.Elapsed
                        };
                    }
                }
                else
                {
                    _logger.LogInformation("Found local branch: {BranchName}", branchName);
                }

                // STEP 3: Checkout the branch
                _logger.LogInformation("Checking out branch: {BranchName}", branchName);
                Commands.Checkout(repo, branch);

                // STEP 4: If it has a remote tracking branch, pull latest changes (native git; non-fatal)
                if (branch.TrackedBranch != null)
                {
                    var pull = await _transport.PullAsync(repositoryPath);
                    if (!pull.Ok)
                    {
                        _logger.LogWarning("Pull after checkout of {Branch} failed (non-fatal): {Error}", branchName, pull.Error);
                    }
                }

                stopwatch.Stop();

                // Verify the current branch with a fresh repository instance to avoid caching issues
                string currentBranchName;
                string headCommitAfter;
                IEnumerable<string> changedPaths;
                using (var freshRepo = new Repository(repositoryPath))
                {
                    currentBranchName = freshRepo.Head.FriendlyName;
                    headCommitAfter = freshRepo.Head.Tip?.Sha;
                    changedPaths = GetCommitDiffPaths(freshRepo, headCommitBefore, headCommitAfter);
                    _logger.LogInformation("✅ Verified current branch from fresh repository: {CurrentBranch}", currentBranchName);
                }

                var headMoved = headCommitBefore != headCommitAfter;

                _logger.LogInformation("✅ Checkout operation completed successfully: {BranchName}, HeadMoved: {HeadMoved}, Duration: {Duration}ms",
                    branchName, headMoved, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully checked out branch '{branchName}'",
                    BranchName = currentBranchName,  // Return verified branch name
                    HasChanges = headMoved,
                    Changes = headMoved ? changedPaths : new string[0],
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during checkout operation for repository: {RepositoryPath}, Branch: {Branch}",
                    repositoryPath, branchName);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Checkout failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        public async Task<GitRepositoryStatus> GetStatusAsync(string repositoryPath)
        {
            try
            {
                using var repo = new Repository(repositoryPath);
                var status = repo.RetrieveStatus(BuildStatusOptions(repositoryPath));
                var currentBranch = repo.Head;

                return new GitRepositoryStatus
                {
                    Added = status.Added.Select(s => s.FilePath).ToList(),
                    Modified = status.Modified.Select(s => s.FilePath).ToList(),
                    Removed = status.Removed.Select(s => s.FilePath).ToList(),
                    Untracked = status.Untracked.Select(s => s.FilePath).ToList(),
                    CommitsAhead = currentBranch.TrackingDetails?.AheadBy ?? 0,
                    CommitsBehind = currentBranch.TrackingDetails?.BehindBy ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for repository: {RepositoryPath}", repositoryPath);
                return new GitRepositoryStatus
                {
                    Added = new List<string>(),
                    Modified = new List<string>(),
                    Removed = new List<string>(),
                    Untracked = new List<string>()
                };
            }
        }

public async Task<GitOperationResult> FetchAsync(string repositoryPath, string remoteName = "origin")
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting fetch operation for repository: {RepositoryPath}, Remote: {Remote}",
                    repositoryPath, remoteName);
                using (var repo = new Repository(repositoryPath))
                {
                    if (repo.Network.Remotes[remoteName] == null)
                    {
                        return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Remote '{remoteName}' not found",
                    Duration = stopwatch.Elapsed
                };
                    }
                }

                var fetch = await _transport.FetchAsync(repositoryPath, remoteName);
                stopwatch.Stop();
                if (!fetch.Ok)
                {
                    return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Fetch failed: {fetch.Error}",
                    Duration = stopwatch.Elapsed
                };
                }
                _lastUsedAuthMethod = AuthenticationMethod.GitCredentialHelper;
                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully fetched from {remoteName}",
                    Duration = stopwatch.Elapsed,
                    AuthenticationMethodUsed = _lastUsedAuthMethod
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error during fetch operation for repository: {RepositoryPath}", repositoryPath);
                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Fetch failed: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        #region Private Helper Methods

        private AuthenticationMethod _lastUsedAuthMethod = AuthenticationMethod.UserPrompt;

        // STATIC cache shared across all instances - per-project and permanent until application close
        // Key format: "repositoryPath|url|username|types"
        private static readonly Dictionary<string, CachedCredential> _credentialCache = new Dictionary<string, CachedCredential>();
        private static readonly Dictionary<string, int> _credentialCallHistory = new Dictionary<string, int>();
        private static readonly Dictionary<string, SemaphoreSlim> _credentialResolutionLocks = new Dictionary<string, SemaphoreSlim>();
        private static readonly object _cacheLock = new object(); // Thread safety for cache access
        private static readonly object _lockDictionaryLock = new object(); // Thread safety for lock dictionary

        private const int MaxAuthenticationAttempts = 3;

        private class CachedCredential
        {
            public Credentials Credentials { get; set; }
            public DateTime CachedAt { get; set; }
            public AuthenticationMethod AuthMethod { get; set; }
            public string RepositoryPath { get; set; }
        }

        private async Task<Credentials> ResolveCredentials(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var resolverCallId = Guid.NewGuid().ToString("N")[..8];

            // Get repository path from execution context for per-project caching
            var repositoryPath = GitExecutionContext.CurrentRepositoryPath ?? "global";

            // Create per-project cache key
            var cacheKey = $"{repositoryPath}|{url}|{usernameFromUrl}|{types}";

            // IMPORTANT: Check cache FIRST before doing anything else - with thread safety
            // Cache is PERMANENT (no timeout) - credentials persist until application close
            lock (_cacheLock)
            {
                if (_credentialCache.ContainsKey(cacheKey))
                {
                    var cached = _credentialCache[cacheKey];
                    var age = DateTime.UtcNow - cached.CachedAt;

                    _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Using CACHED credentials for {Url} in project {Project} (age: {Age:F1} seconds)",
                        resolverCallId, url, repositoryPath, age.TotalSeconds);
                    _lastUsedAuthMethod = cached.AuthMethod;
                    return cached.Credentials;
                }
            }

            // Get or create a semaphore for this specific cache key to prevent concurrent resolution
            SemaphoreSlim resolutionLock;
            lock (_lockDictionaryLock)
            {
                if (!_credentialResolutionLocks.ContainsKey(cacheKey))
                {
                    _credentialResolutionLocks[cacheKey] = new SemaphoreSlim(1, 1);
                }
                resolutionLock = _credentialResolutionLocks[cacheKey];
            }

            // Wait for any ongoing credential resolution for this cache key
            _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Waiting for resolution lock for {Url}", resolverCallId, url);
            await resolutionLock.WaitAsync();

            try
            {
                // Double-check cache after acquiring lock (another thread might have resolved it)
                lock (_cacheLock)
                {
                    if (_credentialCache.ContainsKey(cacheKey))
                    {
                        var cached = _credentialCache[cacheKey];
                        var age = DateTime.UtcNow - cached.CachedAt;

                        _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Using CACHED credentials (found after lock wait) for {Url} in project {Project} (age: {Age:F1} seconds)",
                            resolverCallId, url, repositoryPath, age.TotalSeconds);
                        _lastUsedAuthMethod = cached.AuthMethod;
                        return cached.Credentials;
                    }

                    // Track call history for this URL
                    if (_credentialCallHistory.ContainsKey(cacheKey))
                    {
                        _credentialCallHistory[cacheKey]++;
                    }
                    else
                    {
                        _credentialCallHistory[cacheKey] = 1;
                    }
                }

                var callCount = _credentialCallHistory[cacheKey];
            
            _logger.LogInformation("CREDENTIAL RESOLUTION CALL [{CallId}] - URL: {Url}, User: {User}, Types: {Types}, CallCount: {CallCount}", 
                resolverCallId, url, usernameFromUrl, types, callCount);
                
            // Log warning if this is a repeated call
            if (callCount > 1)
            {
                _logger.LogWarning("CREDENTIAL RESOLUTION [{CallId}] - REPEATED CALL #{Count} for same URL/user/types combination", 
                    resolverCallId, callCount);
                    
                // If we've been called too many times, fail fast to prevent infinite loops
                if (callCount > MaxAuthenticationAttempts)
                {
                    _logger.LogError("CREDENTIAL RESOLUTION [{CallId}] - EXCEEDED MAX ATTEMPTS ({Count}/{Max}) - Failing to prevent infinite loop", 
                        resolverCallId, callCount, MaxAuthenticationAttempts);
                    return null;
                }
            }

            var resolverIndex = 0;
            foreach (var resolver in _credentialResolvers)
            {
                resolverIndex++;
                try
                {
                    _logger.LogDebug("CREDENTIAL RESOLUTION [{CallId}] - Checking resolver #{Index}: {ResolverType}, Priority: {Priority}", 
                        resolverCallId, resolverIndex, resolver.GetType().Name, resolver.GetPriority());

                    if (resolver.CanResolveCredentials(url, types))
                    {
                        _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Trying resolver #{Index}: {ResolverType}", 
                            resolverCallId, resolverIndex, resolver.GetType().Name);
                        
                        var credentials = await resolver.ResolveCredentialsAsync(url, usernameFromUrl, types);
                        if (credentials != null)
                        {
                            _lastUsedAuthMethod = resolver.GetAuthenticationMethod();
                            
                            // Log detailed credential type information
                            var credType = credentials.GetType().Name;
                            var isSSH = url.StartsWith("git@") || url.StartsWith("ssh://");
                            var isHTTPS = url.StartsWith("https://");
                            
                            _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - SUCCESS using {ResolverType}: {AuthMethod}, CredType: {CredType}, SSH: {IsSSH}, HTTPS: {IsHTTPS}",
                                resolverCallId, resolver.GetType().Name, _lastUsedAuthMethod, credType, isSSH, isHTTPS);

                            // Cache the successful credential for future use - with thread safety
                            // Credentials are cached per-project and persist until application close
                            lock (_cacheLock)
                            {
                                _credentialCache[cacheKey] = new CachedCredential
                                {
                                    Credentials = credentials,
                                    CachedAt = DateTime.UtcNow,
                                    AuthMethod = _lastUsedAuthMethod,
                                    RepositoryPath = repositoryPath
                                };

                                _logger.LogInformation("CREDENTIAL RESOLUTION [{CallId}] - Credentials CACHED PERMANENTLY for {Url} in project {Project} (valid until application close)",
                                    resolverCallId, url, repositoryPath);

                                // Reset call history on success
                                _credentialCallHistory[cacheKey] = 0;
                            }
                            
                            return credentials;
                        }
                        else
                        {
                            _logger.LogWarning("CREDENTIAL RESOLUTION [{CallId}] - FAILED {ResolverType} returned null", 
                                resolverCallId, resolver.GetType().Name);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("CREDENTIAL RESOLUTION [{CallId}] - SKIPPED {ResolverType}: cannot handle URL/types", 
                            resolverCallId, resolver.GetType().Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CREDENTIAL RESOLUTION [{CallId}] - ERROR in {ResolverType}: {Error}", 
                        resolverCallId, resolver.GetType().Name, ex.Message);
                }
            }

                _logger.LogError("CREDENTIAL RESOLUTION [{CallId}] - FAILED: No resolver could provide credentials for URL: {Url}",
                    resolverCallId, url);
                return null;
            }
            finally
            {
                // Always release the semaphore
                resolutionLock.Release();
                _logger.LogDebug("CREDENTIAL RESOLUTION [{CallId}] - Released resolution lock for {Url}", resolverCallId, url);
            }
        }

        private Signature GetGitSignature(Repository repo)
        {
            try
            {
                // Try to get from local repository config first
                var config = repo.Config;
                var name = config.Get<string>("user.name")?.Value;
                var email = config.Get<string>("user.email")?.Value;

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                {
                    _logger.LogDebug("Using Git signature from local config: {Name} <{Email}>", name, email);
                    return new Signature(name, email, DateTimeOffset.Now);
                }

                // Try global Git config
                var globalName = config.Get<string>("user.name", ConfigurationLevel.Global)?.Value;
                var globalEmail = config.Get<string>("user.email", ConfigurationLevel.Global)?.Value;

                if (!string.IsNullOrEmpty(globalName) && !string.IsNullOrEmpty(globalEmail))
                {
                    _logger.LogDebug("Using Git signature from global config: {Name} <{Email}>", globalName, globalEmail);
                    return new Signature(globalName, globalEmail, DateTimeOffset.Now);
                }

                // Log warning but still return a fallback for pull operations
                _logger.LogWarning("Git user.name and user.email not configured in repository at {Path}. Using fallback signature.", repo.Info.Path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not get Git signature from config");
            }

            // Fallback for pull operations (which need a signature for merge commits)
            // This should rarely be used as commits should use the GitAuthor passed from the controller
            return new Signature("Unknown User", "user@example.com", DateTimeOffset.Now);
        }

        private IEnumerable<string> GetChangedFiles(Repository repo)
        {
            try
            {
                var status = repo.RetrieveStatus(BuildStatusOptions(repo.Info.WorkingDirectory));
                return status.Modified.Concat(status.Added).Concat(status.Removed)
                    .Select(s => s.FilePath).ToList();
            }
            catch
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Files changed between two commits (e.g. HEAD before/after a pull or checkout).
        /// This is the ONLY correct source for "what did the operation bring in":
        /// the working-directory status (RetrieveStatus) is unrelated to it and on a
        /// clean tree is empty even after a pull that changed hundreds of files.
        /// Includes old paths of renamed/deleted entries so consumers can react to
        /// files that disappeared.
        /// </summary>
        private IEnumerable<string> GetCommitDiffPaths(Repository repo, string shaBefore, string shaAfter)
        {
            try
            {
                if (shaBefore == shaAfter)
                {
                    return new string[0];
                }

                var treeBefore = shaBefore != null ? repo.Lookup<Commit>(shaBefore)?.Tree : null;
                var treeAfter = shaAfter != null ? repo.Lookup<Commit>(shaAfter)?.Tree : null;

                using var diff = repo.Diff.Compare<TreeChanges>(treeBefore, treeAfter);

                var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var change in diff)
                {
                    if (!string.IsNullOrEmpty(change.Path))
                    {
                        paths.Add(change.Path);
                    }
                    if (!string.IsNullOrEmpty(change.OldPath) && change.OldPath != change.Path)
                    {
                        paths.Add(change.OldPath);
                    }
                }
                return paths.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCommitDiffPaths failed comparing {Before} → {After}", shaBefore, shaAfter);
                return new string[0];
            }
        }

        private IEnumerable<string> GetStagedFiles(Repository repo)
        {
            try
            {
                var status = repo.RetrieveStatus(BuildStatusOptions(repo.Info.WorkingDirectory));
                return status.Staged.Select(s => s.FilePath).ToList();
            }
            catch
            {
                return new string[0];
            }
        }

        private void ClearCredentialCallHistory()
        {
            // Clear the call history to prevent false positives on next operation - with thread safety
            lock (_cacheLock)
            {
                _credentialCallHistory.Clear();
                _logger.LogDebug("Credential call history cleared after successful operation");
            }
        }

        /// <summary>
        /// Clears all cached credentials for a specific repository
        /// Useful when changing credentials for a project
        /// </summary>
        public void ClearProjectCache(string repositoryPath)
        {
            lock (_cacheLock)
            {
                var keysToRemove = _credentialCache
                    .Where(kvp => kvp.Value.RepositoryPath == repositoryPath)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _credentialCache.Remove(key);
                }

                if (keysToRemove.Any())
                {
                    _logger.LogInformation("Cleared {Count} cached credentials for project: {RepositoryPath}", keysToRemove.Count, repositoryPath);
                }
            }
        }

        #endregion

        public async Task<GitPullPushData> GetPullPushDataAsync(string repositoryPath)
        {
            var callId = Guid.NewGuid().ToString("N")[..8];
            try
            {
                var result = new GitPullPushData
                {
                    HasDataToPull = false,
                    CommitsBehind = 0,
                    CommitsAhead = 0,
                    FilesToPull = new List<GitFileChange>(),
                    IsRemoteAvailable = false,
                    RemoteConnectionError = null
                };

                bool hasOrigin, isTracked;
                using (var probe = new Repository(repositoryPath))
                {
                    hasOrigin = probe.Network.Remotes["origin"] != null;
                    isTracked = probe.Head.TrackedBranch != null;
                }
                if (!isTracked)
                {
                    return result;
                }

                // Fetch col git nativo PRIMA di aprire il repository con LibGit2Sharp, così i ref
                // che leggiamo dopo sono quelli appena scaricati e non una copia in cache.
                if (hasOrigin)
                {
                    var fetch = await _transport.FetchAsync(repositoryPath, "origin");
                    result.IsRemoteAvailable = fetch.Ok;
                    if (!fetch.Ok)
                    {
                        // Don't fail the whole operation - use cached tracking information
                        result.RemoteConnectionError = $"Fetch failed: {fetch.Error}";
                    }
                }

                using var repo = new Repository(repositoryPath);
                var currentBranch = repo.Head;
                try
                {
                    // Get tracking details after fetch
                    var trackingDetails = currentBranch.TrackingDetails;

                    // Special case: check if we're dealing with an empty remote repository
                    // In this case, TrackingDetails might be null or incorrect
                    // The IsRemoteEmpty flag is set by the workaround when it detects an empty repository
                    if (result.IsRemoteEmpty && currentBranch.Tip != null)
                    {
                        // For empty remote, count all commits in the current branch as "ahead"
                        var allCommits = currentBranch.Commits.Count();
                        result.CommitsAhead = allCommits;
                        result.CommitsBehind = 0;
                        result.HasDataToPull = false;
                        _logger.LogInformation("[EMPTY REMOTE] Found {Count} commits to push to empty repository", allCommits);
                    }
                    else if (trackingDetails != null)
                    {
                        result.CommitsBehind = trackingDetails.BehindBy ?? 0;
                        result.CommitsAhead = trackingDetails.AheadBy ?? 0;
                        result.HasDataToPull = result.CommitsBehind > 0;

                        // Get incoming changes if there are commits behind
                        if (result.CommitsBehind > 0)
                        {
                            var trackedBranch = currentBranch.TrackedBranch;
                            var currentCommit = currentBranch.Tip;
                            var remoteCommit = trackedBranch.Tip;

                            // Get all commits between current and remote
                            var filter = new CommitFilter
                            {
                                ExcludeReachableFrom = currentCommit,
                                IncludeReachableFrom = remoteCommit,
                                SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time
                            };

                            var incomingCommits = repo.Commits.QueryBy(filter).ToList();
                            _logger.LogDebug("Found {Count} incoming commits", incomingCommits.Count);

                            // Analyze changes in incoming commits
                            var changedFiles = new Dictionary<string, GitFileChange>();

                            foreach (var commit in incomingCommits)
                            {
                                var parent = commit.Parents.FirstOrDefault();
                                if (parent != null)
                                {
                                    var changes = repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);

                                    foreach (var change in changes)
                                    {
                                        var filePath = change.Path;
                                        if (!changedFiles.ContainsKey(filePath))
                                        {
                                            changedFiles[filePath] = new GitFileChange
                                            {
                                                FilePath = filePath,
                                                Author = commit.Author.Name,
                                                ChangeType = ConvertChangeKind(change.Status),
                                                CommitMessage = commit.MessageShort,
                                                ChangeDate = commit.Author.When.DateTime
                                            };
                                        }
                                    }
                                }
                            }

                            result.FilesToPull = changedFiles.Values.ToList();
                        }
                    }
                    else if (currentBranch.Tip != null)
                    {
                        // No tracking details but we have commits - likely all need to be pushed
                        _logger.LogInformation("[NO TRACKING] No tracking details found, counting all commits as 'ahead'");
                        var allCommits = currentBranch.Commits.Count();
                        result.CommitsAhead = allCommits;
                        result.CommitsBehind = 0;
                        result.HasDataToPull = false;
                    }

                    _logger.LogInformation("Pull/push data retrieved: Behind={Behind}, Ahead={Ahead}, Files={Files}",
                        result.CommitsBehind, result.CommitsAhead, result.FilesToPull?.Count() ?? 0);

                    return result;
                }
                catch (LibGit2SharpException ex)
                {
                    _logger.LogWarning(ex, "Error fetching from remote, returning local data only");
                    result.RemoteConnectionError = ex.Message;
                    
                    // Return local data without remote info
                    var trackingDetails = currentBranch.TrackingDetails;
                    if (trackingDetails != null)
                    {
                        result.CommitsBehind = trackingDetails.BehindBy ?? 0;
                        result.CommitsAhead = trackingDetails.AheadBy ?? 0;
                        result.HasDataToPull = result.CommitsBehind > 0;
                    }
                    
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pull/push data for repository: {RepositoryPath}", repositoryPath);
                
                return new GitPullPushData
                {
                    HasDataToPull = false,
                    CommitsBehind = 0,
                    CommitsAhead = 0,
                    FilesToPull = new List<GitFileChange>(),
                    IsRemoteAvailable = false,
                    RemoteConnectionError = ex.Message
                };
            }
        }

        private string ConvertChangeKind(ChangeKind changeKind)
        {
            return changeKind switch
            {
                ChangeKind.Added => "Added",
                ChangeKind.Deleted => "Deleted",
                ChangeKind.Modified => "Modified",
                ChangeKind.Renamed => "Renamed",
                ChangeKind.Copied => "Copied",
                ChangeKind.TypeChanged => "TypeChanged",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets the commit history for a repository
        /// </summary>
        /// <param name="repositoryPath">Path to the local repository</param>
        /// <param name="maxCommits">Maximum number of commits to retrieve</param>
        /// <returns>List of commits with author, message, and other details</returns>
        public async Task<IList<GitCommitInfo>> GetCommitHistoryAsync(string repositoryPath, int maxCommits = 50)
        {
            return await Task.Run(() =>
            {
                var commits = new List<GitCommitInfo>();

                try
                {
                    _logger.LogInformation("Getting commit history for repository: {RepositoryPath}, MaxCommits: {MaxCommits}",
                        repositoryPath, maxCommits);

                    if (!Directory.Exists(repositoryPath))
                    {
                        _logger.LogWarning("Repository directory does not exist: {RepositoryPath}", repositoryPath);
                        return commits;
                    }

                    using (var repo = new Repository(repositoryPath))
                    {
                        // Get the current branch name
                        var currentBranch = repo.Head?.FriendlyName ?? "unknown";

                        // Get commits from the current branch
                        var commitLog = repo.Commits.Take(maxCommits);

                        foreach (var commit in commitLog)
                        {
                            var commitInfo = new GitCommitInfo
                            {
                                Hash = commit.Sha,
                                Author = commit.Author.Name,
                                Email = commit.Author.Email,
                                Message = commit.Message?.Trim(),
                                Date = commit.Author.When.DateTime,
                                Branch = currentBranch,
                                Parents = commit.Parents?.Select(p => p.Sha).ToList() ?? new List<string>()
                            };

                            commits.Add(commitInfo);
                        }

                        _logger.LogInformation("Retrieved {CommitCount} commits from repository", commits.Count);
                    }
                }
                catch (RepositoryNotFoundException ex)
                {
                    _logger.LogWarning(ex, "Repository not found at path: {RepositoryPath}", repositoryPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting commit history for repository: {RepositoryPath}", repositoryPath);
                }

                return commits;
            });
        }

        /// <summary>
        /// Checks if a remote repository is configured
        /// </summary>
        public async Task<RemoteStatus> CheckRemoteStatusAsync(string repositoryPath)
        {
            var callId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogWarning("🔵 [CHECK REMOTE STATUS - START] CallId: {CallId}, Repository: {RepositoryPath}", callId, repositoryPath);

            return await Task.Run(() =>
            {
                var status = new RemoteStatus
                {
                    CanAuthenticate = false,
                    AuthenticationMethod = null
                };

                try
                {
                    _logger.LogInformation("🔵 [CHECK REMOTE STATUS {CallId}] Checking remote status for repository: {RepositoryPath}", callId, repositoryPath);

                    if (!Directory.Exists(repositoryPath))
                    {
                        status.IsGitRepository = false;
                        status.ErrorMessage = "Directory does not exist";
                        return status;
                    }

                    var gitPath = Path.Combine(repositoryPath, ".git");
                    if (!Directory.Exists(gitPath))
                    {
                        status.IsGitRepository = false;
                        status.ErrorMessage = "Not a Git repository";
                        return status;
                    }

                    status.IsGitRepository = true;

                    using (var repo = new Repository(repositoryPath))
                    {
                        // Check for origin remote
                        var origin = repo.Network.Remotes["origin"];
                        if (origin != null)
                        {
                            status.HasRemote = true;
                            status.RemoteName = origin.Name;
                            status.RemoteUrl = origin.Url;
                            _logger.LogInformation("Remote found: {RemoteName} -> {RemoteUrl}", origin.Name, origin.Url);

                            // Test authentication by attempting a lightweight fetch
                            var authResult = TestRemoteAuthentication(repo, origin, repositoryPath);
                            status.CanAuthenticate = authResult.Success;
                            status.AuthenticationMissing = authResult.CredentialsMissing;
                            status.AuthenticationFailed = authResult.AuthFailed;
                            status.AuthenticationFailureReason = authResult.FailureReason;

                            if (status.CanAuthenticate)
                            {
                                status.AuthenticationMethod = _lastUsedAuthMethod.ToString();
                                _logger.LogInformation("Authentication test successful using method: {Method}", status.AuthenticationMethod);
                            }
                            else if (status.AuthenticationMissing)
                            {
                                _logger.LogWarning("⚠️ No credentials configured for remote: {RemoteUrl}", origin.Url);
                            }
                            else
                            {
                                _logger.LogWarning("❌ Authentication failed for remote: {RemoteUrl} - Reason: {Reason}",
                                    origin.Url, status.AuthenticationFailureReason);
                            }
                        }
                        else
                        {
                            status.HasRemote = false;
                            _logger.LogInformation("No remote configured for repository");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking remote status for repository: {RepositoryPath}", repositoryPath);
                    status.ErrorMessage = ex.Message;
                }

                return status;
            });
        }

        /// <summary>
        /// Result of authentication test with detailed failure information
        /// </summary>
        private class AuthTestResult
        {
            public bool Success { get; set; }
            public bool CredentialsMissing { get; set; }
            public bool AuthFailed { get; set; }
            public string FailureReason { get; set; }
        }

        /// <summary>
        /// Tests if authentication to the remote works by attempting a lightweight fetch
        /// </summary>
private AuthTestResult TestRemoteAuthentication(Repository repo, Remote remote, string repositoryPath)
        {
            // `git ls-remote --heads origin`: la prova più leggera che il remoto risponde e la
            // credenziale del credential helper è buona. Senza terminale git non resta appeso.
            var probe = _transport.LsRemoteAsync(repositoryPath, remote.Name).GetAwaiter().GetResult();
            if (probe.Ok)
            {
                _lastUsedAuthMethod = AuthenticationMethod.GitCredentialHelper;
                return new AuthTestResult { Success = true };
            }
            return probe.Kind switch
            {
                NativeGitFailureKind.CredentialsMissing => new AuthTestResult { CredentialsMissing = true, FailureReason = probe.Error },
                NativeGitFailureKind.Network => new AuthTestResult { AuthFailed = true, FailureReason = "Cannot connect to remote server (check VPN or network): " + probe.Error },
                _ => new AuthTestResult { AuthFailed = true, FailureReason = probe.Error },
            };
        }

        /// <summary>
        /// Removes a remote from the repository
        /// </summary>
        public async Task<GitOperationResult> RemoveRemoteAsync(string repositoryPath, string remoteName = "origin")
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Removing remote from repository: {RepositoryPath}, Remote: {RemoteName}",
                    repositoryPath, remoteName);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                using (var repo = new Repository(repositoryPath))
                {
                    // Check if remote exists
                    var existingRemote = repo.Network.Remotes[remoteName];
                    if (existingRemote == null)
                    {
                        return new GitOperationResult
                        {
                            Success = false,
                            ErrorMessage = $"Remote '{remoteName}' does not exist",
                            Duration = stopwatch.Elapsed
                        };
                    }

                    // Remove the remote
                    repo.Network.Remotes.Remove(remoteName);
                    _logger.LogInformation("Remote removed successfully: {RemoteName}", remoteName);

                    return new GitOperationResult
                    {
                        Success = true,
                        Message = $"Remote '{remoteName}' removed successfully",
                        Duration = stopwatch.Elapsed
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing remote from repository: {RepositoryPath}", repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to remove remote: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Adds a GitHub remote repository to the local repository
        /// </summary>
        public async Task<GitOperationResult> AddRemoteAsync(string repositoryPath, string organization, string repositoryName, bool pushAfterAdd = true)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Adding remote to repository: {RepositoryPath}, Org: {Org}, Repo: {Repo}",
                    repositoryPath, organization, repositoryName);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                using (var repo = new Repository(repositoryPath))
                {
                    // Check if origin already exists
                    var existingRemote = repo.Network.Remotes["origin"];
                    if (existingRemote != null)
                    {
                        return new GitOperationResult
                        {
                            Success = false,
                            ErrorMessage = $"Remote 'origin' already exists: {existingRemote.Url}",
                            Duration = stopwatch.Elapsed
                        };
                    }

                    // Construct GitHub URL
                    var remoteUrl = $"https://github.com/{organization}/{repositoryName}.git";

                    // Add the remote
                    repo.Network.Remotes.Add("origin", remoteUrl);
                    _logger.LogInformation("Remote added successfully: origin -> {RemoteUrl}", remoteUrl);

                    // Configure the branch to track the remote branch
                    var currentBranch = repo.Head;
                    if (currentBranch != null && !currentBranch.IsRemote)
                    {
                        var remoteBranchName = $"refs/remotes/origin/{currentBranch.FriendlyName}";
                        _logger.LogInformation("Setting up tracking branch: {LocalBranch} -> {RemoteBranch}",
                            currentBranch.FriendlyName, remoteBranchName);

                        try
                        {
                            // Set the upstream branch
                            repo.Branches.Update(currentBranch,
                                b => b.TrackedBranch = remoteBranchName);

                            // Also set the config directly to ensure it's properly configured
                            repo.Config.Set($"branch.{currentBranch.FriendlyName}.remote", "origin");
                            repo.Config.Set($"branch.{currentBranch.FriendlyName}.merge", $"refs/heads/{currentBranch.FriendlyName}");

                            _logger.LogInformation("Tracking branch configured successfully");
                        }
                        catch (Exception trackEx)
                        {
                            _logger.LogWarning(trackEx, "Failed to set up tracking branch, continuing anyway");
                        }
                    }

                    // If requested and there are commits, push to the remote
                    if (pushAfterAdd && repo.Head?.Tip != null)
                    {
                        _logger.LogInformation("Attempting initial push to remote");

                        // Create a simple push with the current branch
                        var currentBranchName = repo.Head.FriendlyName;
                        var pushResult = await PushAsync(repositoryPath, "origin", currentBranchName);

                        if (!pushResult.Success)
                        {
                            _logger.LogWarning("Initial push failed: {Error}. Remote was added but not pushed.", pushResult.ErrorMessage);
                            return new GitOperationResult
                            {
                                Success = true,
                                Message = $"Remote added successfully but initial push failed: {pushResult.ErrorMessage}",
                                Duration = stopwatch.Elapsed
                            };
                        }

                        return new GitOperationResult
                        {
                            Success = true,
                            Message = $"Remote added and initial push completed successfully",
                            Duration = stopwatch.Elapsed
                        };
                    }

                    return new GitOperationResult
                    {
                        Success = true,
                        Message = "Remote added successfully",
                        Duration = stopwatch.Elapsed
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding remote to repository: {RepositoryPath}", repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to add remote: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Initializes a new Git repository
        /// </summary>
        public async Task<InitRepositoryResponse> InitRepositoryAsync(InitRepositoryRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var callId = Guid.NewGuid().ToString("N").Substring(0, 8);

            _logger.LogWarning($"🔵 [INIT REPOSITORY - START] CallId: {callId}, Path: {request.RepositoryPath}");

            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.RepositoryPath))
                {
                    return new InitRepositoryResponse
                    {
                        Success = false,
                        Message = "Repository path is required",
                        IsGitRepository = false
                    };
                }

                if (!Directory.Exists(request.RepositoryPath))
                {
                    return new InitRepositoryResponse
                    {
                        Success = false,
                        Message = $"Directory does not exist: {request.RepositoryPath}",
                        IsGitRepository = false,
                        RepositoryPath = request.RepositoryPath
                    };
                }

                var gitPath = Path.Combine(request.RepositoryPath, ".git");

                // Check if Git repository already exists
                if (Directory.Exists(gitPath))
                {
                    _logger.LogInformation($"[INIT REPOSITORY {callId}] Git repository already exists");
                    return new InitRepositoryResponse
                    {
                        Success = false,
                        Message = "Git repository already initialized",
                        IsGitRepository = true,
                        RepositoryPath = request.RepositoryPath
                    };
                }

                // Initialize Git repository
                _logger.LogInformation($"[INIT REPOSITORY {callId}] Initializing Git repository");
                Repository.Init(request.RepositoryPath);

                // Create .gitignore file
                await CreateGitignoreFileAsync(request.RepositoryPath, request.GitignoreTemplate);

                // Set initial branch name if specified
                if (!string.IsNullOrWhiteSpace(request.InitialBranch) && request.InitialBranch != "master")
                {
                    using (var repo = new Repository(request.RepositoryPath))
                    {
                        // Create initial commit to establish branch
                        var signature = new Signature("MdExplorer", "noreply@mdexplorer.net", DateTimeOffset.Now);
                        repo.Commit($"Initial commit", signature, signature, new CommitOptions { AllowEmptyCommit = true });

                        // Rename branch
                        var currentBranch = repo.Head;
                        repo.Branches.Rename(currentBranch, request.InitialBranch);
                    }
                }

                stopwatch.Stop();
                _logger.LogInformation($"✅ [INIT REPOSITORY {callId}] Repository initialized successfully in {stopwatch.ElapsedMilliseconds}ms");

                return new InitRepositoryResponse
                {
                    Success = true,
                    Message = $"Git repository initialized successfully with branch '{request.InitialBranch}'",
                    IsGitRepository = true,
                    RepositoryPath = request.RepositoryPath,
                    InitialBranch = request.InitialBranch
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError($"❌ [INIT REPOSITORY {callId}] Error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");

                return new InitRepositoryResponse
                {
                    Success = false,
                    Message = $"Failed to initialize Git repository: {ex.Message}",
                    IsGitRepository = false,
                    RepositoryPath = request.RepositoryPath
                };
            }
        }

        /// <summary>
        /// Creates a .gitignore file based on the selected template
        /// </summary>
        private async Task CreateGitignoreFileAsync(string repositoryPath, string template)
        {
            var gitignorePath = Path.Combine(repositoryPath, ".gitignore");

            // Don't overwrite existing .gitignore
            if (File.Exists(gitignorePath))
            {
                _logger.LogInformation($"[CREATE GITIGNORE] .gitignore already exists, skipping");
                return;
            }

            var content = GetGitignoreTemplate(template);
            await File.WriteAllTextAsync(gitignorePath, content);
            _logger.LogInformation($"[CREATE GITIGNORE] Created .gitignore with template: {template}");
        }

        /// <summary>
        /// Gets the .gitignore template content
        /// </summary>
        private string GetGitignoreTemplate(string template)
        {
            var sb = new StringBuilder();

            switch (template?.ToLower())
            {
                case "mdexplorer":
                    sb.AppendLine("# MdExplorer specific files and folders");
                    sb.AppendLine(".md/");
                    sb.AppendLine("");
                    sb.AppendLine("# Database files");
                    sb.AppendLine("*.db");
                    sb.AppendLine("*.db-shm");
                    sb.AppendLine("*.db-wal");
                    sb.AppendLine("");
                    sb.AppendLine("# Temporary files");
                    sb.AppendLine("*.tmp");
                    sb.AppendLine("*.temp");
                    sb.AppendLine("~*");
                    sb.AppendLine("");
                    sb.AppendLine("# Log files");
                    sb.AppendLine("*.log");
                    sb.AppendLine("");
                    sb.AppendLine("# OS specific files");
                    sb.AppendLine(".DS_Store");
                    sb.AppendLine("Thumbs.db");
                    sb.AppendLine("desktop.ini");
                    break;

                case "node":
                    sb.AppendLine("# Node.js");
                    sb.AppendLine("node_modules/");
                    sb.AppendLine("npm-debug.log*");
                    sb.AppendLine("yarn-debug.log*");
                    sb.AppendLine("yarn-error.log*");
                    sb.AppendLine(".npm");
                    sb.AppendLine(".env");
                    sb.AppendLine(".env.local");
                    sb.AppendLine("");
                    sb.AppendLine("# Build outputs");
                    sb.AppendLine("dist/");
                    sb.AppendLine("build/");
                    sb.AppendLine("*.tgz");
                    break;

                case "python":
                    sb.AppendLine("# Python");
                    sb.AppendLine("__pycache__/");
                    sb.AppendLine("*.py[cod]");
                    sb.AppendLine("*$py.class");
                    sb.AppendLine("*.so");
                    sb.AppendLine(".Python");
                    sb.AppendLine("");
                    sb.AppendLine("# Virtual environments");
                    sb.AppendLine("venv/");
                    sb.AppendLine("ENV/");
                    sb.AppendLine(".venv");
                    sb.AppendLine("");
                    sb.AppendLine("# Distribution / packaging");
                    sb.AppendLine("dist/");
                    sb.AppendLine("build/");
                    sb.AppendLine("*.egg-info/");
                    break;

                case "csharp":
                    sb.AppendLine("# .NET / C#");
                    sb.AppendLine("bin/");
                    sb.AppendLine("obj/");
                    sb.AppendLine("*.dll");
                    sb.AppendLine("*.exe");
                    sb.AppendLine("*.pdb");
                    sb.AppendLine("");
                    sb.AppendLine("# User-specific files");
                    sb.AppendLine("*.user");
                    sb.AppendLine("*.suo");
                    sb.AppendLine("*.userosscache");
                    sb.AppendLine("");
                    sb.AppendLine("# Visual Studio");
                    sb.AppendLine(".vs/");
                    sb.AppendLine("*.sln.docstates");
                    break;

                case "none":
                    // Empty .gitignore
                    sb.AppendLine("# No template selected");
                    break;

                default:
                    // Default to mdexplorer template
                    return GetGitignoreTemplate("mdexplorer");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Discards changes to a specific file (equivalent to git restore/checkout -- file)
        /// </summary>
        public async Task<GitOperationResult> DiscardFileChangesAsync(string repositoryPath, string filePath)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Discarding changes for file: {FilePath} in repository: {RepositoryPath}", filePath, repositoryPath);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                using var repo = new Repository(repositoryPath);

                // Check if the file exists in the current commit (HEAD)
                var treeEntry = repo.Head.Tip?.Tree[filePath];
                if (treeEntry == null)
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"File '{filePath}' does not exist in the current commit. Use UnstageFileAsync for new files.",
                        Duration = stopwatch.Elapsed
                    };
                }

                // Restore the file from HEAD using CheckoutPaths
                var checkoutOptions = new CheckoutOptions
                {
                    CheckoutModifiers = CheckoutModifiers.Force
                };

                repo.CheckoutPaths(repo.Head.FriendlyName, new[] { filePath }, checkoutOptions);

                stopwatch.Stop();

                _logger.LogInformation("Successfully discarded changes for file: {FilePath}, Duration: {Duration}ms",
                    filePath, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully discarded changes for '{filePath}'",
                    Changes = new[] { filePath },
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error discarding changes for file: {FilePath} in repository: {RepositoryPath}",
                    filePath, repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to discard changes: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Removes a file from staging area (equivalent to git reset HEAD file)
        /// The file remains on disk as untracked
        /// </summary>
        public async Task<GitOperationResult> UnstageFileAsync(string repositoryPath, string filePath)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Unstaging file: {FilePath} in repository: {RepositoryPath}", filePath, repositoryPath);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                using var repo = new Repository(repositoryPath);

                // Unstage the file (removes it from the index)
                Commands.Unstage(repo, filePath);

                stopwatch.Stop();

                _logger.LogInformation("Successfully unstaged file: {FilePath}, Duration: {Duration}ms",
                    filePath, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully removed '{filePath}' from staging",
                    Changes = new[] { filePath },
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error unstaging file: {FilePath} in repository: {RepositoryPath}",
                    filePath, repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to unstage file: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Deletes an untracked file from disk
        /// </summary>
        public async Task<GitOperationResult> DeleteUntrackedFileAsync(string repositoryPath, string filePath)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Deleting untracked file: {FilePath} in repository: {RepositoryPath}", filePath, repositoryPath);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"Repository directory does not exist: {repositoryPath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                var fullPath = Path.Combine(repositoryPath, filePath);

                if (!File.Exists(fullPath))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"File does not exist: {filePath}",
                        Duration = stopwatch.Elapsed
                    };
                }

                // Verify the file is untracked or newly added (not part of HEAD)
                using var repo = new Repository(repositoryPath);
                var status = repo.RetrieveStatus(filePath);

                // Allow deletion only for untracked or newly added files
                if (status != FileStatus.NewInIndex && status != FileStatus.NewInWorkdir &&
                    !status.HasFlag(FileStatus.NewInIndex) && !status.HasFlag(FileStatus.NewInWorkdir))
                {
                    return new GitOperationResult
                    {
                        Success = false,
                        ErrorMessage = $"File '{filePath}' is not a new/untracked file and cannot be deleted this way.",
                        Duration = stopwatch.Elapsed
                    };
                }

                // First unstage if it's staged
                if (status.HasFlag(FileStatus.NewInIndex))
                {
                    Commands.Unstage(repo, filePath);
                }

                // Delete the file
                File.Delete(fullPath);

                stopwatch.Stop();

                _logger.LogInformation("Successfully deleted file: {FilePath}, Duration: {Duration}ms",
                    filePath, stopwatch.ElapsedMilliseconds);

                return new GitOperationResult
                {
                    Success = true,
                    Message = $"Successfully deleted '{filePath}'",
                    Changes = new[] { filePath },
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error deleting file: {FilePath} in repository: {RepositoryPath}",
                    filePath, repositoryPath);

                return new GitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to delete file: {ex.Message}",
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Gets detailed information about all changed files in the repository
        /// </summary>
        public async Task<GitDetailedStatus> GetDetailedStatusAsync(string repositoryPath)
        {
            try
            {
                _logger.LogInformation("Getting detailed status for repository: {RepositoryPath}", repositoryPath);

                if (!Directory.Exists(repositoryPath))
                {
                    return new GitDetailedStatus { Files = new List<GitChangedFileInfo>() };
                }

                using var repo = new Repository(repositoryPath);
                var status = repo.RetrieveStatus(BuildStatusOptions(repositoryPath));

                var files = new List<GitChangedFileInfo>();

                // Process Modified files
                foreach (var item in status.Modified)
                {
                    files.Add(new GitChangedFileInfo
                    {
                        FileName = Path.GetFileName(item.FilePath),
                        RelativePath = item.FilePath,
                        FullPath = Path.Combine(repositoryPath, item.FilePath),
                        Status = "Modified",
                        IsNew = false
                    });
                }

                // Process Added (staged new files)
                foreach (var item in status.Added)
                {
                    files.Add(new GitChangedFileInfo
                    {
                        FileName = Path.GetFileName(item.FilePath),
                        RelativePath = item.FilePath,
                        FullPath = Path.Combine(repositoryPath, item.FilePath),
                        Status = "Added",
                        IsNew = true
                    });
                }

                // Process Removed files (removed from the git index).
                // A file staged for removal but still present on disk (e.g. `git rm --cached`,
                // as done for the per-install artifacts) is NOT actually deleted: label it
                // "Unversioned" so the UI doesn't claim the file is gone. Only a file that is
                // also missing from disk is a genuine "Deleted".
                foreach (var item in status.Removed)
                {
                    var fullPath = Path.Combine(repositoryPath, item.FilePath);
                    files.Add(new GitChangedFileInfo
                    {
                        FileName = Path.GetFileName(item.FilePath),
                        RelativePath = item.FilePath,
                        FullPath = fullPath,
                        Status = File.Exists(fullPath) ? "Unversioned" : "Deleted",
                        IsNew = false
                    });
                }

                // Process Untracked files
                foreach (var item in status.Untracked)
                {
                    files.Add(new GitChangedFileInfo
                    {
                        FileName = Path.GetFileName(item.FilePath),
                        RelativePath = item.FilePath,
                        FullPath = Path.Combine(repositoryPath, item.FilePath),
                        Status = "Untracked",
                        IsNew = true
                    });
                }

                _logger.LogInformation("Found {Count} changed files in repository", files.Count);

                return new GitDetailedStatus { Files = files };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed status for repository: {RepositoryPath}", repositoryPath);
                return new GitDetailedStatus { Files = new List<GitChangedFileInfo>() };
            }
        }

        /// <summary>
        /// Validates if a remote Git URL is reachable by performing a lightweight ls-remote check.
        /// Uses different approaches based on provider type:
        /// - OAuth providers (GitHub, GitLab, Azure, Bitbucket): uses git command to allow GCM to open browser
        /// - Basic auth providers (SCM Manager, Gitea, etc.): uses LibGit2Sharp with existing credential resolution
        /// </summary>
public async Task<RemoteUrlValidationResult> ValidateRemoteUrlAsync(string url)
        {
            var probe = await _transport.LsRemoteAsync(Path.GetTempPath(), url);
            if (probe.Ok)
            {
                return new RemoteUrlValidationResult
                {
                    IsReachable = true,
                    ReferenceCount = probe.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length
                };
            }
            // GitHub risponde 404 a un repository privato senza credenziale: anche quello è un
            // problema di autenticazione, non di URL.
            return new RemoteUrlValidationResult
            {
                IsReachable = false,
                Error = probe.Error,
                IsAuthenticationError = probe.Kind is NativeGitFailureKind.CredentialsMissing
                    or NativeGitFailureKind.AuthenticationFailed
                    or NativeGitFailureKind.NotFound
            };
        }




    }
}