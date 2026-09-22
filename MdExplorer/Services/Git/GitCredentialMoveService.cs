using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>Che fine ha fatto una credenziale che MdExplorer conservava nel suo database.</summary>
    public sealed class GitCredentialMoveEntry
    {
        /// <summary>Da dove viene: <c>GitCredential</c>, <c>GitlabSetting</c> o <c>Setting:GitHubPersonalAccessToken</c>.</summary>
        public string Source { get; init; } = string.Empty;
        public string RepositoryPath { get; init; }
        public string Url { get; init; }
        public string Username { get; init; }
        public bool Moved { get; init; }
        /// <summary>Perché non si è potuta spostare: è il testo che l'utente deve leggere.</summary>
        public string Reason { get; init; }
    }

    public sealed class GitCredentialMoveReport
    {
        public DateTime RanAt { get; init; } = DateTime.UtcNow;
        public IReadOnlyList<GitCredentialMoveEntry> Entries { get; init; } = Array.Empty<GitCredentialMoveEntry>();
        public int Moved => Entries.Count(e => e.Moved);
        public int Failed => Entries.Count(e => !e.Moved);
        /// <summary>Vero quando non c'era niente da spostare: la faccenda è chiusa.</summary>
        public bool NothingLeft => Entries.Count == 0;
    }

    /// <summary>L'ultimo rapporto del trasloco, per la UI: un singleton, perché il servizio è scoped.</summary>
    public sealed class GitCredentialMoveReportHolder
    {
        public GitCredentialMoveReport Last { get; set; }
    }

    public interface IGitCredentialMoveService
    {
        /// <summary>Sposta nel credential helper di git ogni segreto che MdExplorer conserva ancora nel suo DB. Idempotente: ciò che è spostato non c'è più.</summary>
        Task<GitCredentialMoveReport> RunAsync(CancellationToken ct = default);
    }

    /// <summary>
    /// <b>Il trasloco.</b> Fino allo sprint «un solo meccanismo di autenticazione» MdExplorer salvava
    /// token e password nel suo database (<c>GitCredential</c>, <c>GitlabSetting</c>, la riga
    /// <c>GitHubPersonalAccessToken</c> di <c>Setting</c>) e li iniettava in LibGit2Sharp. Tre strade
    /// su sei non li salvavano ANCHE nel credential helper di git: chi le ha usate, dopo
    /// l'aggiornamento, avrebbe il push rotto. Questo servizio, una volta all'avvio, consegna ogni
    /// segreto a git con <c>credential approve</c>, <b>verifica</b> con <c>credential fill</c> che git
    /// lo ritrovi davvero (senza helper configurato <c>approve</c> esce 0 e non salva niente:
    /// misurato), scrive nel repository l'utente per quell'host, e solo allora azzera il segreto nel DB.
    /// <para>
    /// Legge le tabelle con SQL diretto perché le entità non sono più mappate: le tabelle restano
    /// nel file (FluentMigrator su SQLite non sa cancellarle), vuote di segreti.
    /// Ciò che non si è potuto spostare resta, con il perché nel rapporto, e si riprova all'avvio dopo.
    /// </para>
    /// </summary>
    public sealed class GitCredentialMoveService : IGitCredentialMoveService
    {
        private readonly INativeGitTransport _transport;
        private readonly INativeGitRunner _git;
        private readonly GitCredentialMoveReportHolder _holder;
        private readonly ILogger<GitCredentialMoveService> _logger;
        private readonly string _dbPath;

        public GitCredentialMoveService(INativeGitTransport transport, INativeGitRunner git,
            GitCredentialMoveReportHolder holder, ILogger<GitCredentialMoveService> logger)
            : this(transport, git, holder, logger, Path.Combine(CrossPlatformPath.GetAppDataPath(), "MdExplorer.db")) { }

        public GitCredentialMoveService(INativeGitTransport transport, INativeGitRunner git,
            GitCredentialMoveReportHolder holder, ILogger<GitCredentialMoveService> logger, string dbPath)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _git = git ?? throw new ArgumentNullException(nameof(git));
            _holder = holder ?? throw new ArgumentNullException(nameof(holder));
            _logger = logger;
            _dbPath = dbPath;
        }

        public async Task<GitCredentialMoveReport> RunAsync(CancellationToken ct = default)
        {
            var entries = new List<GitCredentialMoveEntry>();
            if (!File.Exists(_dbPath))
            {
                _logger.LogInformation("[trasloco] nessun DB utente in {Path}: niente da spostare", _dbPath);
                return Publish(entries);
            }

            using var db = new SqliteConnection($"Data Source={_dbPath}");
            await db.OpenAsync(ct);

            if (TableExists(db, "GitCredential"))
                await MoveGitCredentialsAsync(db, entries, ct);
            if (TableExists(db, "GitlabSetting"))
                await MoveGitlabSettingsAsync(db, entries, ct);
            if (TableExists(db, "Setting"))
                await MoveGitHubTokenAsync(db, entries, ct);

            foreach (var e in entries)
                _logger.Log(e.Moved ? LogLevel.Information : LogLevel.Warning,
                    "[trasloco] {Source} {User}@{Url} ({Repo}): {Esito} {Reason}",
                    e.Source, e.Username, e.Url, e.RepositoryPath ?? "-", e.Moved ? "spostata" : "NON spostata", e.Reason ?? string.Empty);
            return Publish(entries);
        }

        private GitCredentialMoveReport Publish(List<GitCredentialMoveEntry> entries)
        {
            var report = new GitCredentialMoveReport { Entries = entries };
            _holder.Last = report;
            return report;
        }

        // ------------------------------------------------------------------ GitCredential

        private async Task MoveGitCredentialsAsync(SqliteConnection db, List<GitCredentialMoveEntry> entries, CancellationToken ct)
        {
            var rows = new List<(long RowId, string User, string Type, string Secret, string SshKey)>();
            using (var cmd = db.CreateCommand())
            {
                cmd.CommandText = @"SELECT rowid, AuthUsername, AccountType,
                        COALESCE(GitHubPAT, GitLabToken, BitbucketAppPassword, HttpsPassword) AS Secret, SSHKeyPath
                    FROM GitCredential
                    WHERE (GitHubPAT IS NOT NULL OR GitLabToken IS NOT NULL OR BitbucketAppPassword IS NOT NULL OR HttpsPassword IS NOT NULL)";
                using var r = await cmd.ExecuteReaderAsync(ct);
                while (await r.ReadAsync(ct))
                    rows.Add((r.GetInt64(0), r.IsDBNull(1) ? null : r.GetString(1), r.IsDBNull(2) ? null : r.GetString(2),
                              r.IsDBNull(3) ? null : r.GetString(3), r.IsDBNull(4) ? null : r.GetString(4)));
            }

            foreach (var row in rows)
            {
                var repos = new List<string>();
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandText = @"SELECT RepositoryPath FROM GitRepositoryAccount
                        WHERE CredentialId = (SELECT Id FROM GitCredential WHERE rowid = $rowid)";
                    cmd.Parameters.AddWithValue("$rowid", row.RowId);
                    using var r = await cmd.ExecuteReaderAsync(ct);
                    while (await r.ReadAsync(ct)) if (!r.IsDBNull(0)) repos.Add(r.GetString(0));
                }

                // Gli URL da cui git imparerà la credenziale: quelli dei repository collegati, oppure,
                // se nessun repository lo è, l'host del provider dichiarato.
                var targets = new List<(string Repo, string Url)>();
                foreach (var repo in repos)
                {
                    var url = await OriginUrlAsync(repo, ct);
                    if (url != null) targets.Add((repo, url));
                    else entries.Add(new GitCredentialMoveEntry { Source = "GitCredential", RepositoryPath = repo, Username = row.User, Moved = false,
                        Reason = "il repository non esiste più o non ha un remote origin" });
                }
                if (targets.Count == 0)
                {
                    var host = ProviderHost(row.Type);
                    if (host == null)
                    {
                        entries.Add(new GitCredentialMoveEntry { Source = "GitCredential", Username = row.User, Moved = false,
                            Reason = $"account di tipo {row.Type} senza repository collegati: non so per quale host valga" });
                        continue;
                    }
                    targets.Add((null, host));
                }

                var allMoved = true;
                foreach (var (repo, url) in targets)
                {
                    var (ok, why) = await HandOverAsync(url, row.User, row.Secret, ct);
                    if (ok && repo != null)
                    {
                        await WriteUsernameForHostAsync(repo, url, row.User, ct);
                        if (!string.IsNullOrEmpty(row.SshKey) && File.Exists(row.SshKey))
                            await _git.RunAsync(repo, new[] { "config", "core.sshCommand", $"ssh -i \"{row.SshKey}\"" }, ct);
                    }
                    allMoved &= ok;
                    entries.Add(new GitCredentialMoveEntry { Source = "GitCredential", RepositoryPath = repo, Url = url, Username = row.User, Moved = ok, Reason = why });
                }

                if (allMoved)
                {
                    using var cmd = db.CreateCommand();
                    cmd.CommandText = @"UPDATE GitCredential SET GitHubPAT = NULL, GitLabToken = NULL, BitbucketAppPassword = NULL, HttpsPassword = NULL
                        WHERE rowid = $rowid";
                    cmd.Parameters.AddWithValue("$rowid", row.RowId);
                    await cmd.ExecuteNonQueryAsync(ct);
                }
            }
        }

        // ------------------------------------------------------------------ GitlabSetting (legacy)

        private async Task MoveGitlabSettingsAsync(SqliteConnection db, List<GitCredentialMoveEntry> entries, CancellationToken ct)
        {
            var rows = new List<(long RowId, string User, string Password, string Link, string LocalPath)>();
            using (var cmd = db.CreateCommand())
            {
                cmd.CommandText = "SELECT rowid, UserName, Password, GitlabLink, LocalPath FROM GitlabSetting WHERE Password IS NOT NULL AND Password <> ''";
                using var r = await cmd.ExecuteReaderAsync(ct);
                while (await r.ReadAsync(ct))
                    rows.Add((r.GetInt64(0), r.IsDBNull(1) ? null : r.GetString(1), r.IsDBNull(2) ? null : r.GetString(2),
                              r.IsDBNull(3) ? null : r.GetString(3), r.IsDBNull(4) ? null : r.GetString(4)));
            }
            foreach (var row in rows)
            {
                var url = (row.LocalPath != null ? await OriginUrlAsync(row.LocalPath, ct) : null) ?? row.Link;
                if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
                {
                    entries.Add(new GitCredentialMoveEntry { Source = "GitlabSetting", RepositoryPath = row.LocalPath, Username = row.User, Moved = false,
                        Reason = "né il repository né il link dicono per quale host valga" });
                    continue;
                }
                var (ok, why) = await HandOverAsync(url, row.User, row.Password, ct);
                if (ok && row.LocalPath != null && Directory.Exists(row.LocalPath))
                    await WriteUsernameForHostAsync(row.LocalPath, url, row.User, ct);
                entries.Add(new GitCredentialMoveEntry { Source = "GitlabSetting", RepositoryPath = row.LocalPath, Url = url, Username = row.User, Moved = ok, Reason = why });
                if (ok)
                {
                    using var cmd = db.CreateCommand();
                    cmd.CommandText = "UPDATE GitlabSetting SET Password = NULL WHERE rowid = $rowid";
                    cmd.Parameters.AddWithValue("$rowid", row.RowId);
                    await cmd.ExecuteNonQueryAsync(ct);
                }
            }
        }

        // ------------------------------------------------------------------ Setting: token GitHub

        private async Task MoveGitHubTokenAsync(SqliteConnection db, List<GitCredentialMoveEntry> entries, CancellationToken ct)
        {
            string token = null, user = null;
            using (var cmd = db.CreateCommand())
            {
                cmd.CommandText = "SELECT Name, ValueString FROM Setting WHERE Name IN ('GitHubPersonalAccessToken', 'GitHubTokenUsername')";
                using var r = await cmd.ExecuteReaderAsync(ct);
                while (await r.ReadAsync(ct))
                {
                    var v = r.IsDBNull(1) ? null : r.GetString(1);
                    if (r.GetString(0) == "GitHubPersonalAccessToken") token = v; else user = v;
                }
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                await DeleteTokenRowsAsync(db, ct); // righe vuote o solo l'utente: via
                return;
            }
            const string url = "https://github.com/";
            if (string.IsNullOrWhiteSpace(user))
            {
                // Senza utente git non saprebbe a chi appartiene il token; e con GitHub l'utente
                // del token è opaco. Non inventiamo niente: resta lì e lo dice.
                entries.Add(new GitCredentialMoveEntry { Source = "Setting:GitHubPersonalAccessToken", Url = url, Moved = false,
                    Reason = "token senza nome utente: rifai il login una volta (su Windows lo fa Git Credential Manager al primo push)" });
                return;
            }
            var (ok, why) = await HandOverAsync(url, user, token, ct);
            entries.Add(new GitCredentialMoveEntry { Source = "Setting:GitHubPersonalAccessToken", Url = url, Username = user, Moved = ok, Reason = why });
            if (ok) await DeleteTokenRowsAsync(db, ct);
        }

        private static async Task DeleteTokenRowsAsync(SqliteConnection db, CancellationToken ct)
        {
            using var cmd = db.CreateCommand();
            cmd.CommandText = "DELETE FROM Setting WHERE Name IN ('GitHubPersonalAccessToken', 'GitHubTokenUsername', 'GitHubPersonalAccessToken_Global')";
            await cmd.ExecuteNonQueryAsync(ct);
        }

        // ------------------------------------------------------------------ meccanica

        /// <summary>approve, poi fill per verificare: senza la verifica il trasloco sarebbe un fallback silenzioso.</summary>
        private async Task<(bool Ok, string Why)> HandOverAsync(string url, string username, string secret, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(username)) return (false, "manca il nome utente");
            if (string.IsNullOrEmpty(secret)) return (false, "manca il segreto");
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != "http" && uri.Scheme != "https"))
                return (false, $"url non http(s): {url} (per SSH git usa le chiavi in ~/.ssh, non serve spostare niente)");

            var approve = await _transport.ApproveCredentialAsync(url, username, secret, ct);
            if (!approve.Ok) return (false, $"git credential approve: {approve.Error}");

            var fill = await _git.RunAsync(Path.GetTempPath(), new[] { "credential", "fill" }, ct,
                stdin: $"url={url}\nusername={username}\n\n");
            var found = fill.Ok && (fill.Stdout ?? string.Empty).Split('\n').Any(l => l.StartsWith("password="));
            if (!found)
                return (false, "git non la ritrova: nessun credential helper configurato (git config --global credential.helper). Su Windows installa Git for Windows con Git Credential Manager; su Linux/Mac configura un helper e rifai il login una volta dal terminale");
            return (true, null);
        }

        private async Task<string> OriginUrlAsync(string repositoryPath, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(repositoryPath) || !Directory.Exists(repositoryPath)) return null;
            var r = await _git.RunAsync(repositoryPath, new[] { "config", "--get", "remote.origin.url" }, ct);
            var url = r.Ok ? r.Stdout?.Trim() : null;
            return string.IsNullOrEmpty(url) ? null : url;
        }

        /// <summary>
        /// <c>credential.&lt;scheme://host[:porta]&gt;.username</c> nel <c>.git/config</c> del repository: è così che
        /// git, con più account sullo stesso host, sa quale chiedere al credential helper. Sta nel
        /// repository, quindi basta l'host: non serve l'owner nel path.
        /// </summary>
        public static string HostKeyOf(string url)
        {
            var uri = new Uri(url);
            var hostPort = uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";
            return $"credential.{uri.Scheme}://{hostPort}.username";
        }

        private Task WriteUsernameForHostAsync(string repositoryPath, string url, string username, CancellationToken ct)
            => _git.RunAsync(repositoryPath, new[] { "config", HostKeyOf(url), username }, ct);

        private static string ProviderHost(string accountType) => accountType?.ToLowerInvariant() switch
        {
            "github" => "https://github.com/",
            "gitlab" => "https://gitlab.com/",
            "bitbucket" => "https://bitbucket.org/",
            _ => null,
        };

        private static bool TableExists(SqliteConnection db, string table)
        {
            using var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name";
            cmd.Parameters.AddWithValue("$name", table);
            return cmd.ExecuteScalar() != null;
        }
    }
}
