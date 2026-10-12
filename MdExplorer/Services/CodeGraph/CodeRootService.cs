using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Features.Utilities;
using MdExplorer.Service.Models;
using MdExplorer.Services.Git;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>Dove sta una radice su questa macchina: lo UserDB in produzione, un dizionario nei test.</summary>
    public interface ICodeRootBindingStore
    {
        string Get(string projectPath, string alias);
        void Set(string projectPath, string alias, string localPath);
        void Remove(string projectPath, string alias);

        /// <summary>Gli alias per cui l'utente ha detto «non ricordarmelo più» in questo progetto (avviso all'apertura).</summary>
        IReadOnlyCollection<string> DismissedReminders(string projectPath);
        void DismissReminders(string projectPath, IEnumerable<string> aliases);
    }

    public interface ICodeRootService
    {
        /// <summary>Le radici dichiarate nel progetto, con lo stato su questa macchina (git interrogato adesso).</summary>
        Task<IReadOnlyList<CodeRootState>> ListAsync(string projectPath, CancellationToken ct = default);

        /// <summary>Una fotografia per tradurre molti percorsi senza ripetere git (analisi, MCP).</summary>
        Task<CodeRootResolver> CreateResolverAsync(string projectPath, CancellationToken ct = default);

        /// <summary>
        /// Collega una cartella di questa macchina all'alias. Se l'alias non è dichiarato lo dichiara (identità presa dal
        /// remote della cartella, R7: il primo che collega la fissa per tutti); se è dichiarato verifica che sia lo stesso
        /// repository. Eccezione con il motivo se non lo è.
        /// </summary>
        Task<CodeRootState> LinkAsync(string projectPath, string alias, string localPath, CancellationToken ct = default);

        /// <summary>Dimentica dove sta la radice su questa macchina; la dichiarazione nel progetto resta (è del team).</summary>
        void Unlink(string projectPath, string alias);

        /// <summary>
        /// Ramo e file modificati del repository di una radice collegata (radici esterne F5, R6): per vedere che cosa ha
        /// toccato MarkAgent prima di committare con i propri strumenti. Solo lettura.
        /// </summary>
        Task<CodeRootChanges> ChangesAsync(string projectPath, string alias, CancellationToken ct = default);

        /// <summary>
        /// Le radici da ricordare all'apertura del progetto: non utilizzabili qui (da collegare, non trovate, repository
        /// diverso) e per cui l'utente non ha detto «non ricordarmelo più». Un alias nuovo si ricorda di nuovo.
        /// </summary>
        Task<IReadOnlyList<CodeRootState>> PendingReminderAsync(string projectPath, CancellationToken ct = default);

        void DismissReminder(string projectPath, IEnumerable<string> aliases);
    }

    public sealed class CodeRootChanges
    {
        public string Alias { get; init; }
        public bool IsGit { get; init; }
        public string Branch { get; init; }
        /// <summary>«M», «A», «D», «??»… come in <c>git status --porcelain</c>, e il percorso relativo alla radice analizzata.</summary>
        public List<(string Status, string Path)> Files { get; } = new();

        public CodeRootChanges With(IEnumerable<(string Status, string Path)> files) { Files.AddRange(files); return this; }
    }

    /// <summary>
    /// Le radici di codice esterne (sprint 2026-10-12-Radici-Di-Codice-Esterne, F1): identità in <c>.development.yml</c>
    /// (<c>codeRoots</c>), posizione nello UserDB (<see cref="ICodeRootBindingStore"/>), stato reale da git. Con git la
    /// posizione è la radice del repository, qualunque cartella del clone l'utente scelga, e l'analisi parte dalla
    /// sottocartella dichiarata: l'identità è «repository + sottocartella», non dove sta il clone.
    /// </summary>
    public sealed class CodeRootService : ICodeRootService
    {
        private readonly ICodeRootBindingStore _bindings;
        private readonly INativeGitRunner _git;
        private readonly ILogger<CodeRootService> _logger;
        private readonly SemaphoreSlim _configGate = new(1, 1);

        public CodeRootService(ICodeRootBindingStore bindings, INativeGitRunner git, ILogger<CodeRootService> logger)
        {
            _bindings = bindings; _git = git; _logger = logger;
        }

        // ------------------------------------------------------------------ lettura

        public async Task<IReadOnlyList<CodeRootState>> ListAsync(string projectPath, CancellationToken ct = default)
        {
            var list = new List<CodeRootState>();
            foreach (var root in ReadConfig(projectPath).CodeRoots ?? new List<CodeRootConfig>())
                list.Add(await StateOfAsync(projectPath, root, ct));
            return list;
        }

        public async Task<CodeRootResolver> CreateResolverAsync(string projectPath, CancellationToken ct = default)
            => new CodeRootResolver(projectPath, await ListAsync(projectPath, ct));

        private async Task<CodeRootState> StateOfAsync(string projectPath, CodeRootConfig root, CancellationToken ct)
        {
            var local = _bindings.Get(projectPath, root.Alias);
            CodeRootState State(CodeRootStatus status, string reason, string rootPath = null, IReadOnlyList<string> remotes = null) => new()
            {
                Alias = root.Alias, Repository = root.Repository, Subpath = root.Subpath ?? string.Empty,
                Tags = root.Tags ?? new List<string>(), Languages = root.CodeGraph?.Languages ?? new List<string>(),
                LocalPath = local, RootPath = rootPath, Status = status, Reason = reason, ActualRemotes = remotes ?? Array.Empty<string>(),
            };

            if (local == null)
                return State(CodeRootStatus.NotLinked, $"su questa macchina non è collegata: indica dove sta la cartella{(root.Repository != null ? " del repository " + root.Repository : string.Empty)}.");
            if (!Directory.Exists(local))
                return State(CodeRootStatus.NotFound, $"la cartella {local} non esiste più (spostata o cancellata): ricollegala.");

            if (root.Repository != null)
            {
                var top = await TopLevelAsync(local, ct);
                if (top == null)
                    return State(CodeRootStatus.Mismatch, $"{local} non è un repository git, ma la radice è il repository {root.Repository}.");
                var remotes = await RemotesAsync(top, ct);
                if (!remotes.Any(r => CodeRootPaths.SameRepository(r, root.Repository)))
                    return State(CodeRootStatus.Mismatch, $"{local} è {Describe(remotes)}, non il repository dichiarato {root.Repository}.", remotes: remotes);
                var rootPath = Combine(top, root.Subpath);
                if (!Directory.Exists(rootPath))
                    return State(CodeRootStatus.NotFound, $"nel clone {top} manca la sottocartella «{root.Subpath}» (un altro ramo?).", remotes: remotes);
                return State(CodeRootStatus.Linked, null, rootPath, remotes);
            }
            return State(CodeRootStatus.Linked, null, Combine(local, root.Subpath));
        }

        // ------------------------------------------------------------------ collegare

        public async Task<CodeRootState> LinkAsync(string projectPath, string alias, string localPath, CancellationToken ct = default)
        {
            CodeRootPaths.RequireAlias(alias);
            if (string.IsNullOrWhiteSpace(localPath) || !Directory.Exists(localPath))
                throw new InvalidOperationException($"La cartella '{localPath}' non esiste.");
            var chosen = Path.GetFullPath(localPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (PathContainment.IsInside(projectPath, chosen))
                throw new InvalidOperationException($"{chosen} sta dentro il progetto: non serve una radice esterna, marcala come cartella «program» dall'albero.");
            if (PathContainment.IsInside(chosen, projectPath))
                throw new InvalidOperationException($"{chosen} contiene il progetto stesso: scegli la cartella del codice, non una che la contiene.");

            var top = await TopLevelAsync(chosen, ct);
            var remotes = top == null ? (IReadOnlyList<string>)Array.Empty<string>() : await RemotesAsync(top, ct);

            await _configGate.WaitAsync(ct);
            try
            {
                var config = ReadConfig(projectPath);
                config.CodeRoots ??= new List<CodeRootConfig>();
                var root = config.CodeRoots.FirstOrDefault(r => r.Alias == alias);
                var changed = false;
                string bindTo;

                if (top != null)
                {
                    if (root?.Repository != null)
                    {
                        if (!remotes.Any(r => CodeRootPaths.SameRepository(r, root.Repository)))
                            throw new InvalidOperationException($"{chosen} è {Describe(remotes)}, ma la radice «@{alias}» è il repository {root.Repository}: scegli il clone giusto.");
                        if (!Directory.Exists(Combine(top, root.Subpath)))
                            throw new InvalidOperationException($"Nel clone {top} manca la sottocartella «{root.Subpath}» della radice «@{alias}» (un altro ramo?).");
                    }
                    else
                    {
                        var origin = PreferredRemote(remotes);
                        if (origin == null)
                            throw new InvalidOperationException($"{top} è un repository git senza remote: senza un remote i colleghi non possono riconoscere la stessa cartella. Aggiungi un remote (git remote add origin …) e ricollega.");
                        var subpath = Relative(top, chosen);
                        if (root == null)
                        {
                            root = new CodeRootConfig { Alias = alias, Repository = origin, Subpath = subpath, Tags = new List<string> { "program" } };
                            config.CodeRoots.Add(root);
                        }
                        else
                        {
                            root.Repository = origin;   // dichiarata fuori da git, ora è un repository: l'identità diventa il remote
                            root.Subpath = subpath;
                        }
                        changed = true;
                    }
                    bindTo = top;
                }
                else
                {
                    if (root?.Repository != null)
                        throw new InvalidOperationException($"{chosen} non è un repository git, ma la radice «@{alias}» è il repository {root.Repository}: scegli il clone.");
                    if (root == null)
                    {
                        root = new CodeRootConfig { Alias = alias, Subpath = string.Empty, Tags = new List<string> { "program" } };
                        config.CodeRoots.Add(root);
                        changed = true;
                    }
                    // senza git non c'è un'identità da verificare: si accetta la scelta, e la UI lo dice (R7)
                    bindTo = chosen;
                }

                if (changed) WriteConfig(projectPath, config);
                _bindings.Set(projectPath, alias, bindTo);
                _logger.LogInformation("[CodeRoots] '{Project}': @{Alias} → {Path}{Repo}", projectPath, alias, bindTo,
                    root.Repository != null ? " (" + root.Repository + ")" : " (senza git)");
                return await StateOfAsync(projectPath, root, ct);
            }
            finally { _configGate.Release(); }
        }

        public void Unlink(string projectPath, string alias)
        {
            CodeRootPaths.RequireAlias(alias);
            _bindings.Remove(projectPath, alias);
            _logger.LogInformation("[CodeRoots] '{Project}': @{Alias} scollegata su questa macchina", projectPath, alias);
        }

        public async Task<IReadOnlyList<CodeRootState>> PendingReminderAsync(string projectPath, CancellationToken ct = default)
        {
            var dismissed = _bindings.DismissedReminders(projectPath);
            return (await ListAsync(projectPath, ct))
                .Where(r => r.Status != CodeRootStatus.Linked && !dismissed.Contains(r.Alias))
                .ToList();
        }

        public void DismissReminder(string projectPath, IEnumerable<string> aliases)
        {
            var list = (aliases ?? Enumerable.Empty<string>()).ToList();
            foreach (var a in list) CodeRootPaths.RequireAlias(a);
            _bindings.DismissReminders(projectPath, list);
        }

        public async Task<CodeRootChanges> ChangesAsync(string projectPath, string alias, CancellationToken ct = default)
        {
            CodeRootPaths.RequireAlias(alias);
            var root = (await ListAsync(projectPath, ct)).FirstOrDefault(r => r.Alias == alias)
                ?? throw new InvalidOperationException($"La radice «@{alias}» non è dichiarata nel .development.yml.");
            if (root.Status != CodeRootStatus.Linked) throw new InvalidOperationException($"@{alias}: {root.Reason}");
            if (root.Repository == null) return new CodeRootChanges { Alias = alias, IsGit = false };
            var status = await _git.RunAsync(root.RootPath, new[] { "status", "--porcelain=v1", "--branch", "--untracked-files=all", "--", "." }, ct);
            if (!status.Ok) throw new InvalidOperationException($"git status in {root.RootPath}: {status.Stderr}".Trim());
            var changes = new CodeRootChanges { Alias = alias, IsGit = true };
            var top = await TopLevelAsync(root.RootPath, ct);
            string branch = null;
            foreach (var line in status.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.StartsWith("## ", StringComparison.Ordinal))
                {
                    branch = line.Substring(3).Split("...")[0].Trim();
                    // un repository senza commit: «## No commits yet on master» → «master (nessun commit)»
                    const string noCommits = "No commits yet on ";
                    if (branch.StartsWith(noCommits, StringComparison.Ordinal)) branch = branch.Substring(noCommits.Length) + " (nessun commit)";
                    continue;
                }
                if (line.Length < 4) continue;
                var path = line.Substring(3).Trim().Trim('"');
                var arrow = path.IndexOf(" -> ", StringComparison.Ordinal);
                if (arrow >= 0) path = path.Substring(arrow + 4);
                // git dà i percorsi relativi alla radice del repository: qui relativi alla radice analizzata (sottocartella)
                var abs = Path.GetFullPath(Path.Combine(top ?? root.RootPath, path.Replace('/', Path.DirectorySeparatorChar)));
                changes.Files.Add((line.Substring(0, 2).Trim(), PathContainment.RelativeOrNull(root.RootPath, abs) ?? path));
            }
            return new CodeRootChanges { Alias = alias, IsGit = true, Branch = branch }.With(changes.Files);
        }

        // ------------------------------------------------------------------ git

        private async Task<string> TopLevelAsync(string folder, CancellationToken ct)
        {
            var r = await _git.RunAsync(folder, new[] { "rev-parse", "--show-toplevel" }, ct);
            return r.Ok && !string.IsNullOrWhiteSpace(r.Stdout) ? Path.GetFullPath(r.Stdout.Trim()) : null;
        }

        /// <summary>Gli URL dei remote (fetch), con «origin» per primo.</summary>
        private async Task<IReadOnlyList<string>> RemotesAsync(string repo, CancellationToken ct)
        {
            var r = await _git.RunAsync(repo, new[] { "remote", "-v" }, ct);
            if (!r.Ok) return Array.Empty<string>();
            return r.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                .Where(p => p.Length >= 2)
                .OrderBy(p => p[0] == "origin" ? 0 : 1)
                .Select(p => p[1])
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static string PreferredRemote(IReadOnlyList<string> remotes) => remotes.FirstOrDefault();

        private static string Describe(IReadOnlyList<string> remotes)
            => remotes.Count == 0 ? "un repository senza remote" : "il repository " + string.Join(" / ", remotes);

        // ------------------------------------------------------------------ .development.yml

        private static DevelopmentConfig ReadConfig(string projectPath)
        {
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            return File.Exists(path) ? MdExplorer.Utilities.DevelopmentConfigCleanup.ReadStrict(path) : new DevelopmentConfig();
        }

        private static void WriteConfig(string projectPath, DevelopmentConfig config)
        {
            var path = Path.Combine(projectPath, MdExplorer.Utilities.DevelopmentConfigCleanup.FileName);
            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            File.WriteAllText(path, serializer.Serialize(config));
        }

        private static string Combine(string root, string subpath)
            => string.IsNullOrEmpty(subpath) ? root : Path.GetFullPath(Path.Combine(root, subpath.Replace('/', Path.DirectorySeparatorChar)));

        private static string Relative(string root, string path)
        {
            var rel = PathContainment.RelativeOrNull(root, path);
            return rel == null || rel == "." ? string.Empty : rel;
        }
    }

    /// <summary>Le posizioni nello UserDB (tabella CodeRootBinding), una transazione per operazione.</summary>
    public sealed class UserDbCodeRootBindingStore : ICodeRootBindingStore
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public UserDbCodeRootBindingStore(IServiceScopeFactory scopeFactory) { _scopeFactory = scopeFactory; }

        public string Get(string projectPath, string alias)
            => WithDb(db => Find(db, projectPath, alias)?.LocalPath);

        public void Set(string projectPath, string alias, string localPath) => WithDb(db =>
        {
            var row = Find(db, projectPath, alias);
            if (row == null) db.GetDal<CodeRootBinding>().Save(new CodeRootBinding { ProjectPath = Key(projectPath), Alias = alias, LocalPath = localPath, BoundAt = DateTime.UtcNow });
            else { row.LocalPath = localPath; row.BoundAt = DateTime.UtcNow; db.GetDal<CodeRootBinding>().Save(row); }
            return true;
        });

        public void Remove(string projectPath, string alias) => WithDb(db =>
        {
            var row = Find(db, projectPath, alias);
            if (row != null) db.GetDal<CodeRootBinding>().Delete(row);
            return true;
        });

        /// <summary>
        /// «Non ricordarmelo più»: una riga <see cref="Setting"/> per progetto (Name fisso, Description = progetto,
        /// ValueString = alias uno per riga). Una preferenza dell'utente su questa macchina, come la posizione.
        /// </summary>
        private const string ReminderSetting = "CodeRootsReminderDismissed";

        public IReadOnlyCollection<string> DismissedReminders(string projectPath) => WithDb(db =>
            (IReadOnlyCollection<string>)(FindReminder(db, projectPath)?.ValueString ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal));

        public void DismissReminders(string projectPath, IEnumerable<string> aliases) => WithDb(db =>
        {
            var row = FindReminder(db, projectPath) ?? new Setting { Name = ReminderSetting, Description = Key(projectPath) };
            var all = (row.ValueString ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Concat(aliases).Distinct(StringComparer.Ordinal).OrderBy(a => a, StringComparer.Ordinal);
            row.ValueString = string.Join("\n", all);
            db.GetDal<Setting>().Save(row);
            return true;
        });

        private static Setting FindReminder(IUserSettingsDB db, string projectPath)
        {
            var key = Key(projectPath);
            return db.GetDal<Setting>().GetList().Where(s => s.Name == ReminderSetting).ToList()
                .FirstOrDefault(s => string.Equals(s.Description, key, PathContainment.Comparison));
        }

        private T WithDb<T>(Func<IUserSettingsDB, T> work)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.Clear();
            db.BeginTransaction();
            try
            {
                var result = work(db);
                db.Commit();
                return result;
            }
            catch
            {
                db.Rollback();
                throw;
            }
        }

        /// <summary>Il progetto senza separatore finale: la chiave non deve dipendere da come arriva il percorso.</summary>
        private static string Key(string projectPath) => Path.GetFullPath(projectPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        // ⚠️ .ToList() prima del confronto: NHibernate non traduce i confronti tolleranti (vedi FindProjectByPath)
        private static CodeRootBinding Find(IUserSettingsDB db, string projectPath, string alias)
        {
            var key = Key(projectPath);
            return db.GetDal<CodeRootBinding>().GetList().Where(b => b.Alias == alias).ToList()
                .FirstOrDefault(b => string.Equals(b.ProjectPath, key, PathContainment.Comparison));
        }
    }
}
