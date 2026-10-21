using System;
using MdExplorer.Features.Services;
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
using MdExplorer.Features.Services.RemoteFolders;
using MdExplorer.RemoteProtocol;
using MdExplorer.Services.RemoteFolders;
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

        /// <summary>Le codifiche per estensione della radice (nel <c>.development.yml</c>, su Git). Eccezione con il motivo se non sono valide.</summary>
        Task<CodeRootState> SetEncodingsAsync(string projectPath, string alias, IReadOnlyList<CodeRootEncodingRule> rules, CancellationToken ct = default);

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
        private readonly IRemoteSessions _remote;
        private readonly ILogger<CodeRootService> _logger;
        private readonly SemaphoreSlim _configGate = new(1, 1);

        public CodeRootService(ICodeRootBindingStore bindings, INativeGitRunner git, IRemoteSessions remote, ILogger<CodeRootService> logger)
        {
            _bindings = bindings; _git = git; _remote = remote; _logger = logger;
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
            CodeRootState State(CodeRootStatus status, string reason, string rootPath = null, IReadOnlyList<string> remotes = null, RemoteLocation remote = null) => new()
            {
                Alias = root.Alias, Repository = root.Repository, Subpath = root.Subpath ?? string.Empty,
                Tags = root.Tags ?? new List<string>(), Languages = root.CodeGraph?.Languages ?? new List<string>(),
                LocalPath = local, RootPath = rootPath, Status = status, Reason = reason, ActualRemotes = remotes ?? Array.Empty<string>(),
                Remote = remote, ExtensionEncodings = EncodingMap(root),
            };

            if (local != null && RemoteLocation.IsRemote(local))
                return await RemoteStateOfAsync(root, local, State, ct);
            if (local == null)
                return State(CodeRootStatus.NotLinked, UiText.Say($"su questa macchina non è collegata: indica dove sta la cartella{(root.Repository != null ? " del repository " + root.Repository : string.Empty)}.", $"it is not linked on this machine: say where the folder{(root.Repository != null ? " of repository " + root.Repository : string.Empty)} is."));
            if (!Directory.Exists(local))
                return State(CodeRootStatus.NotFound, UiText.Say($"la cartella {local} non esiste più (spostata o cancellata): ricollegala.", $"the folder {local} no longer exists (moved or deleted): link it again."));

            if (root.Repository != null)
            {
                var top = await TopLevelAsync(local, ct);
                if (top == null)
                    return State(CodeRootStatus.Mismatch, UiText.Say($"{local} non è un repository git, ma la radice è il repository {root.Repository}.", $"{local} is not a git repository, but the root is the repository {root.Repository}."));
                var remotes = await RemotesAsync(top, ct);
                if (!remotes.Any(r => CodeRootPaths.SameRepository(r, root.Repository)))
                    return State(CodeRootStatus.Mismatch, UiText.Say($"{local} è {Describe(remotes)}, non il repository dichiarato {root.Repository}.", $"{local} is {Describe(remotes)}, not the declared repository {root.Repository}."), remotes: remotes);
                var rootPath = Combine(top, root.Subpath);
                if (!Directory.Exists(rootPath))
                    return State(CodeRootStatus.NotFound, UiText.Say($"nel clone {top} manca la sottocartella «{root.Subpath}» (un altro ramo?).", $"the clone {top} is missing the subfolder «{root.Subpath}» (another branch?)."), remotes: remotes);
                return State(CodeRootStatus.Linked, null, rootPath, remotes);
            }
            return State(CodeRootStatus.Linked, null, Combine(local, root.Subpath));
        }

        /// <summary>
        /// Una radice su un server (sprint 2026-10-21-Cartella-Remota-Via-SSH, F2): collegata se MDE Remote parte sulla
        /// cartella analizzata e, con un repository dichiarato, se i remote letti da <c>.git</c> sul server sono quelli (R7).
        /// Server irraggiungibile o MDE Remote fermo → <see cref="CodeRootStatus.Unreachable"/> con la porta chiusa.
        /// </summary>
        private async Task<CodeRootState> RemoteStateOfAsync(CodeRootConfig root, string local,
            Func<CodeRootStatus, string, string, IReadOnlyList<string>, RemoteLocation, CodeRootState> state, CancellationToken ct)
        {
            RemoteLocation target;
            try { target = RemoteLocation.Parse(local).Combine(root.Subpath ?? string.Empty); }
            catch (Exception ex) when (ex is FormatException or InvalidOperationException)
            {
                return state(CodeRootStatus.NotFound, UiText.Say($"la posizione salvata {local} non è valida ({ex.Message}): ricollegala.", $"the saved location {local} is not valid ({ex.Message}): link it again."), null, null, null);
            }
            RemoteChannel channel;
            try { channel = await _remote.OpenAsync(target, ct); }
            catch (RemoteUnavailableException ex)
            {
                return state(ex.Gate == RemoteGate.Folder ? CodeRootStatus.NotFound : CodeRootStatus.Unreachable, ex.Message, null, null, null);
            }
            catch (RemoteUploadConsentRequiredException ex)
            {
                // finché l'utente non dice sì (finestra delle radici), la radice resta ferma e lo dice — con il suo stato:
                // «non raggiungibile» era improprio, il server risponde (21/10)
                return state(CodeRootStatus.ConsentRequired, ex.Message, null, null, null);
            }
            if (root.Repository == null) return state(CodeRootStatus.Linked, null, null, null, target);
            var git = (await channel.SendAsync(new RemoteRequest { Op = RemoteOps.GitInfo }, ct)).Git;
            if (git == null)
                return state(CodeRootStatus.Mismatch, UiText.Say($"{target} non è un repository git, ma la radice è il repository {root.Repository}.", $"{target} is not a git repository, but the root is the repository {root.Repository}."), null, null, null);
            var remotes = git.Remotes.Select(r => r.Url).Distinct(StringComparer.Ordinal).ToList();
            if (!remotes.Any(r => CodeRootPaths.SameRepository(r, root.Repository)))
                return state(CodeRootStatus.Mismatch, UiText.Say($"{target} è {Describe(remotes)}, non il repository dichiarato {root.Repository}.", $"{target} is {Describe(remotes)}, not the declared repository {root.Repository}."), null, remotes, null);
            return state(CodeRootStatus.Linked, null, null, remotes, target);
        }

        // ------------------------------------------------------------------ collegare

        /// <summary>
        /// Che cosa c'è nella cartella scelta, locale o remota: la radice del repository (null senza git), i remote, come
        /// si scrive la posizione da salvare e se una sottocartella esiste. Le regole d'identità di <see cref="LinkAsync"/>
        /// sono le stesse per i due casi.
        /// </summary>
        private sealed record Place(string Chosen, string Top, IReadOnlyList<string> Remotes, Func<string, string> RelativeToTop, Func<string, CancellationToken, Task<bool>> SubpathExistsAsync);

        private async Task<Place> InspectLocalAsync(string projectPath, string localPath, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(localPath) || !Directory.Exists(localPath))
                throw new InvalidOperationException(UiText.Say($"La cartella '{localPath}' non esiste.", $"The folder '{localPath}' does not exist."));
            var chosen = Path.GetFullPath(localPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (PathContainment.IsInside(projectPath, chosen))
                throw new InvalidOperationException(UiText.Say($"{chosen} sta dentro il progetto: non serve una radice esterna, marcala come cartella «program» dall'albero.", $"{chosen} is inside the project: no external root is needed, mark it as a «program» folder from the tree."));
            if (PathContainment.IsInside(chosen, projectPath))
                throw new InvalidOperationException(UiText.Say($"{chosen} contiene il progetto stesso: scegli la cartella del codice, non una che la contiene.", $"{chosen} contains the project itself: choose the code folder, not one that contains it."));
            var top = await TopLevelAsync(chosen, ct);
            var remotes = top == null ? (IReadOnlyList<string>)Array.Empty<string>() : await RemotesAsync(top, ct);
            return new Place(chosen, top, remotes, t => Relative(t, chosen), (sub, _) => Task.FromResult(Directory.Exists(Combine(top, sub))));
        }

        /// <summary>Una cartella su un server: MDE Remote deve partire lì (porte P10), e i remote si leggono da <c>.git</c> sul server.</summary>
        private async Task<Place> InspectRemoteAsync(string localPath, CancellationToken ct)
        {
            var chosen = RemoteLocation.Parse(localPath);
            var channel = await _remote.OpenAsync(chosen, ct);
            var git = (await channel.SendAsync(new RemoteRequest { Op = RemoteOps.GitInfo }, ct)).Git;
            var top = git == null ? null : RemoteLocation.Of(chosen.Host, git.Top);
            var remotes = git?.Remotes.Select(r => r.Url).Distinct(StringComparer.Ordinal).ToList() ?? new List<string>();
            return new Place(chosen.ToString(), top?.ToString(), remotes,
                _ => chosen.RelativeTo(top) ?? string.Empty,
                async (sub, token) =>
                {
                    try { await _remote.OpenAsync(top.Combine(sub ?? string.Empty), token); return true; }
                    catch (RemoteUnavailableException ex) when (ex.Gate == RemoteGate.Folder) { return false; }
                });
        }

        public async Task<CodeRootState> LinkAsync(string projectPath, string alias, string localPath, CancellationToken ct = default)
        {
            CodeRootPaths.RequireAlias(alias);
            var place = RemoteLocation.IsRemote(localPath) ? await InspectRemoteAsync(localPath, ct) : await InspectLocalAsync(projectPath, localPath, ct);
            var (chosen, top, remotes) = (place.Chosen, place.Top, place.Remotes);

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
                            throw new InvalidOperationException(UiText.Say($"{chosen} è {Describe(remotes)}, ma la radice «@{alias}» è il repository {root.Repository}: scegli il clone giusto.", $"{chosen} is {Describe(remotes)}, but the root «@{alias}» is the repository {root.Repository}: choose the right clone."));
                        if (!await place.SubpathExistsAsync(root.Subpath, ct))
                            throw new InvalidOperationException(UiText.Say($"Nel clone {top} manca la sottocartella «{root.Subpath}» della radice «@{alias}» (un altro ramo?).", $"The clone {top} is missing the subfolder «{root.Subpath}» of the root «@{alias}» (another branch?)."));
                    }
                    else
                    {
                        var origin = PreferredRemote(remotes);
                        if (origin == null)
                            throw new InvalidOperationException(UiText.Say($"{top} è un repository git senza remote: senza un remote i colleghi non possono riconoscere la stessa cartella. Aggiungi un remote (git remote add origin …) e ricollega.", $"{top} is a git repository without a remote: without a remote colleagues cannot recognise the same folder. Add a remote (git remote add origin …) and link it again."));
                        var subpath = place.RelativeToTop(top);
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
                        throw new InvalidOperationException(UiText.Say($"{chosen} non è un repository git, ma la radice «@{alias}» è il repository {root.Repository}: scegli il clone.", $"{chosen} is not a git repository, but the root «@{alias}» is the repository {root.Repository}: choose the clone."));
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

        public async Task<CodeRootState> SetEncodingsAsync(string projectPath, string alias, IReadOnlyList<CodeRootEncodingRule> rules, CancellationToken ct = default)
        {
            CodeRootPaths.RequireAlias(alias);
            var normalized = NormalizeEncodings(rules);
            await _configGate.WaitAsync(ct);
            CodeRootConfig root;
            try
            {
                var config = ReadConfig(projectPath);
                root = config.CodeRoots?.FirstOrDefault(r => r.Alias == alias)
                    ?? throw new InvalidOperationException(UiText.Say($"La radice «@{alias}» non è dichiarata nel .development.yml.", $"The root «@{alias}» is not declared in .development.yml."));
                root.Encodings = normalized.Count == 0 ? null : normalized;
                WriteConfig(projectPath, config);
            }
            finally { _configGate.Release(); }
            _logger.LogInformation("[CodeRoots] '{Project}': @{Alias} codifiche {Rules}", projectPath, alias,
                string.Join("; ", normalized.Select(r => string.Join(",", r.Extensions) + " → " + r.Encoding)));
            return await StateOfAsync(projectPath, root, ct);
        }

        /// <summary>
        /// Estensioni con il punto e in minuscolo, codifiche fra quelle assegnabili, un'estensione in una regola sola, regole
        /// vuote tolte. Niente correzioni silenziose: un errore dice che cosa non va.
        /// </summary>
        public static List<CodeRootEncodingRule> NormalizeEncodings(IReadOnlyList<CodeRootEncodingRule> rules)
        {
            var result = new List<CodeRootEncodingRule>();
            var seen = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var rule in rules ?? Array.Empty<CodeRootEncodingRule>())
            {
                var exts = (rule.Extensions ?? new List<string>())
                    .SelectMany(e => (e ?? "").Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(e => (e.StartsWith(".", StringComparison.Ordinal) ? e : "." + e).ToLowerInvariant())
                    .Distinct(StringComparer.Ordinal).ToList();
                if (exts.Count == 0) continue;
                if (!MdExplorer.RemoteProtocol.TextDecoding.Assignable.Contains(rule.Encoding))
                    throw new InvalidOperationException(UiText.Say($"«{rule.Encoding}» non è una codifica che MDE sa usare: scegli fra UTF-8, CP-850, CP-1252, UTF-16.", $"«{rule.Encoding}» is not an encoding MDE can use: choose among UTF-8, CP-850, CP-1252, UTF-16."));
                foreach (var e in exts)
                {
                    if (e.Length < 2 || e.IndexOfAny(new[] { '/', '\\', '*', '?' }) >= 0)
                        throw new InvalidOperationException(UiText.Say($"«{e}» non è un'estensione (es. .cbl).", $"«{e}» is not an extension (e.g. .cbl)."));
                    if (seen.TryGetValue(e, out var other))
                        throw new InvalidOperationException(UiText.Say($"L'estensione {e} è assegnata due volte ({other} e {rule.Encoding}).", $"The extension {e} is assigned twice ({other} and {rule.Encoding})."));
                    seen[e] = rule.Encoding;
                }
                result.Add(new CodeRootEncodingRule { Extensions = exts, Encoding = rule.Encoding });
            }
            return result;
        }

        private static IReadOnlyDictionary<string, string> EncodingMap(CodeRootConfig root)
            => (root.Encodings ?? new List<CodeRootEncodingRule>())
                .SelectMany(r => (r.Extensions ?? new List<string>()).Select(e => (Ext: e.ToLowerInvariant(), r.Encoding)))
                .GroupBy(x => x.Ext).ToDictionary(g => g.Key, g => g.First().Encoding, StringComparer.Ordinal);

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
                ?? throw new InvalidOperationException(UiText.Say($"La radice «@{alias}» non è dichiarata nel .development.yml.", $"The root «@{alias}» is not declared in .development.yml."));
            if (root.Status != CodeRootStatus.Linked) throw new InvalidOperationException($"@{alias}: {root.Reason}");
            if (root.IsRemote)
                throw new InvalidOperationException(UiText.Say($"@{alias} è una radice remota ({root.Remote}): i file modificati sulle radici remote non ci sono ancora.", $"@{alias} is a remote root ({root.Remote}): changed files on remote roots are not available yet."));
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
                    if (branch.StartsWith(noCommits, StringComparison.Ordinal)) branch = branch.Substring(noCommits.Length) + UiText.Say(" (nessun commit)", " (no commits)");
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
            => remotes.Count == 0 ? UiText.Say("un repository senza remote", "a repository without a remote") : UiText.Say("il repository ", "the repository ") + string.Join(" / ", remotes);

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
        /// «Non ricordarmelo più»: una riga <see cref="Setting"/> per progetto (Name = <c>CodeRootsReminderDismissed:</c> +
        /// progetto, Description = progetto, ValueString = alias uno per riga). Una preferenza dell'utente su questa
        /// macchina, come la posizione. ⚠️ <c>Setting.Name</c> è UNIQUE: fino al 21/10/2026 il Name era fisso e il secondo
        /// progetto non entrava (migrazione M2026_10_21_001 per la riga che esisteva già).
        /// </summary>
        private const string ReminderSetting = "CodeRootsReminderDismissed";

        public IReadOnlyCollection<string> DismissedReminders(string projectPath) => WithDb(db =>
            (IReadOnlyCollection<string>)(FindReminder(db, projectPath)?.ValueString ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal));

        public void DismissReminders(string projectPath, IEnumerable<string> aliases) => WithDb(db =>
        {
            var row = FindReminder(db, projectPath) ?? new Setting { Name = ReminderName(projectPath), Description = Key(projectPath) };
            var all = (row.ValueString ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Concat(aliases).Distinct(StringComparer.Ordinal).OrderBy(a => a, StringComparer.Ordinal);
            row.ValueString = string.Join("\n", all);
            db.GetDal<Setting>().Save(row);
            return true;
        });

        private static string ReminderName(string projectPath) => ReminderSetting + ":" + Key(projectPath);

        // ⚠️ .ToList() prima del confronto: NHibernate non traduce i confronti tolleranti (vedi FindProjectByPath)
        private static Setting FindReminder(IUserSettingsDB db, string projectPath)
        {
            var key = Key(projectPath);
            return db.GetDal<Setting>().GetList().Where(s => s.Name.StartsWith(ReminderSetting + ":")).ToList()
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
