using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Services.Git;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Di chi è il lavoro che si sta guardando.</summary>
    public enum ChangesContextKind
    {
        /// <summary>Il tuo: la working tree del progetto.</summary>
        User,
        /// <summary>Di un agente: il posto di lavoro dov'è in checkout il suo branch.</summary>
        Agent,
    }

    /// <summary>Un file diverso rispetto al ramo di partenza, e in che modo.</summary>
    public sealed class WorkingChange
    {
        /// <summary><c>added</c> | <c>modified</c> | <c>deleted</c> | <c>renamed</c> | <c>untracked</c>.</summary>
        public string Change { get; init; }
        /// <summary>Percorso relativo alla radice del contesto (progetto o posto di lavoro).</summary>
        public string Path { get; init; }
        /// <summary>Da dove veniva, sui rinominati.</summary>
        public string OldPath { get; init; }
    }

    /// <summary>
    /// Un <b>repository</b> dentro il contesto: il progetto stesso, oppure uno dei suoi submodule.
    /// <para>
    /// È l'unità di cui si parla, perché è l'unità in cui si <b>committa</b>. Prima la vista aveva
    /// un solo <c>Branch</c> e una sola lista di file: una forma che può rappresentare un
    /// repository solo, e infatti i submodule finivano nella lista dei file — l'unico posto
    /// rimasto — comparendo come cartelle travestite da documenti.
    /// </para>
    /// </summary>
    public sealed class RepoChanges
    {
        /// <summary>Vuoto per la radice; per i submodule il percorso relativo ad essa.</summary>
        public string Path { get; init; }
        /// <summary>Come chiamarlo in interfaccia: il nome della cartella del progetto, o il path del submodule.</summary>
        public string Label { get; init; }
        /// <summary>0 = radice, 1 = submodule, 2 = submodule dentro un submodule. Solo per il rientro visivo.</summary>
        public int Depth { get; init; }

        /// <summary>Ramo su cui si lavora qui. <c>null</c> se <see cref="Detached"/>.</summary>
        public string Branch { get; init; }
        /// <summary>HEAD staccato: un commit fatto qui resterebbe orfano.</summary>
        public bool Detached { get; init; }
        /// <summary>Ramo remoto di riferimento. <c>null</c> = nessun upstream configurato.</summary>
        public string Upstream { get; init; }
        /// <summary>Ramo di partenza rispetto al quale è calcolata la differenza.</summary>
        public string BaseBranch { get; init; }
        /// <summary>Commit avanti rispetto all'upstream: quanto c'è da pushare.</summary>
        public int Ahead { get; init; }
        /// <summary>Commit indietro rispetto all'upstream: quanto c'è da tirare giù.</summary>
        public int Behind { get; init; }

        /// <summary>
        /// Il padre registra per questo submodule un commit diverso da quello in checkout, e non è
        /// ancora committato nel padre. È una modifica <b>del padre</b>, non di questo repository:
        /// per questo sta qui come segnale e non fra i suoi file.
        /// </summary>
        public bool PointerMoved { get; init; }

        /// <summary>
        /// Submodule dichiarato ma mai popolato (prefisso <c>-</c> di <c>git submodule status</c>).
        /// Invisibile a <c>git status</c>: senza questo campo sembrerebbe pulito.
        /// </summary>
        public bool NotInitialized { get; init; }

        /// <summary>
        /// Il commit che <b>il progetto registra</b> per questo submodule <b>non risulta su nessun
        /// remoto</b>. È il segnale del disastro: pubblicando il progetto adesso, chi clona
        /// troverebbe un riferimento a un commit che non esiste per lui.
        /// <para>
        /// Non coincide con <see cref="Ahead"/>: con HEAD staccato <c>Ahead</c> vale 0 — non c'è un
        /// ramo di cui essere avanti — eppure il commit può benissimo non essere pubblicato.
        /// Verificato con un test, che senza questo campo passava a torto.
        /// </para>
        /// </summary>
        public bool RecordedCommitUnpublished { get; init; }

        /// <summary>Perché non si è potuto stabilire <see cref="RecordedCommitUnpublished"/>. <c>null</c> = si è stabilito.</summary>
        public string RecordedCommitUnknown { get; init; }

        public IReadOnlyList<WorkingChange> Files { get; init; }

        /// <summary>
        /// Perché qui non si può committare. <c>null</c> = si può. Il pulsante si disabilita
        /// sempre <b>con questo motivo scritto</b>: mai spento in silenzio.
        /// </summary>
        public string CommitBlocker { get; init; }

        /// <summary>
        /// Perché pushare <b>questo</b> repository romperebbe qualcosa per gli altri. Vuoto = niente
        /// da segnalare.
        /// </summary>
        public IReadOnlyList<string> PushWarnings { get; init; }
    }

    public sealed class WorkingChangesView
    {
        public string ContextKind { get; init; }
        /// <summary>Come si chiama il contesto in interfaccia: "Il tuo lavoro" o il nome dell'agente.</summary>
        public string ContextLabel { get; init; }
        /// <summary>Radice sul filesystem: il progetto, o il posto di lavoro dell'agente.</summary>
        public string RootPath { get; init; }
        /// <summary>Ramo su cui si sta lavorando qui.</summary>
        public string Branch { get; init; }
        /// <summary>Ramo di partenza rispetto al quale è calcolata la differenza.</summary>
        public string BaseBranch { get; init; }

        /// <summary>
        /// I repository del contesto: <c>Repos[0]</c> è <b>sempre</b> la radice, poi i submodule
        /// in ordine di percorso.
        /// </summary>
        public IReadOnlyList<RepoChanges> Repos { get; init; }


        /// <summary>Vero se non c'è git: l'interfaccia deve dirlo, non mostrare "nessuna modifica".</summary>
        public bool NotAGitRepository { get; init; }
        /// <summary>Motivo per cui la lettura non è riuscita. <c>null</c> = è andata.</summary>
        public string Problem { get; init; }
    }

    public interface IWorkingChangesService
    {
        /// <summary>
        /// Cosa è cambiato nel contesto indicato rispetto al ramo di partenza.
        /// <paramref name="agentName"/> <c>null</c> = il lavoro dell'utente nel progetto.
        /// </summary>
        Task<WorkingChangesView> GetAsync(string projectPath, string agentName, CancellationToken ct = default);

        /// <summary>
        /// Differenza testuale di un singolo file, in formato unified diff.
        /// <paramref name="repoPath"/> <c>null</c> o vuoto = la radice; altrimenti il percorso del
        /// submodule, e <paramref name="relativePath"/> è relativo a <b>quello</b>.
        /// </summary>
        Task<string> DiffAsync(string projectPath, string agentName, string relativePath,
            string repoPath = null, CancellationToken ct = default);

        /// <summary>
        /// Butta via le modifiche a un singolo file, riportandolo com'era sul ramo di partenza
        /// (o eliminandolo, se non esisteva). Irreversibile: nessun commit lo trattiene.
        /// </summary>
        Task<string> DiscardAsync(string projectPath, string agentName, string relativePath,
            string repoPath = null, CancellationToken ct = default);
    }

    /// <summary>
    /// La vista delle differenze, letta da <b>git</b>.
    /// <para>
    /// Git è la fonte di verità e interrogarlo costa poco. Il FileSystemWatcher sarebbe una
    /// seconda fonte, più rumorosa, destinata prima o poi a dire qualcosa di diverso — e a quel
    /// punto non si saprebbe a chi credere. Il prezzo, dichiarato: modificando un file con un
    /// editor esterno la vista non se ne accorge da sola, si aggiorna all'apertura, col pulsante
    /// rinfresca e dopo ogni azione.
    /// </para>
    /// <para>
    /// La domanda è la stessa nei due contesti — «cosa è cambiato qui rispetto al ramo
    /// principale» — e per questo la risposta arriva da un solo confronto, che comprende sia ciò
    /// che è già stato committato sia ciò che è ancora da salvare. Un agente che ha consegnato e
    /// una persona che sta scrivendo si guardano nello stesso modo.
    /// </para>
    /// </summary>
    public sealed class WorkingChangesService : IWorkingChangesService
    {
        private readonly INativeGitRunner _git;
        private readonly IAgentWorktreeManager _worktree;
        private readonly ILogger<WorkingChangesService> _logger;

        public WorkingChangesService(
            INativeGitRunner git,
            IAgentWorktreeManager worktree,
            ILogger<WorkingChangesService> logger)
        {
            _git = git;
            _worktree = worktree;
            _logger = logger;
        }

        public async Task<WorkingChangesView> GetAsync(string projectPath, string agentName, CancellationToken ct = default)
        {
            var (root, label, kind, problem) = await ResolveContextAsync(projectPath, agentName, ct);
            if (problem != null)
                return new WorkingChangesView
                {
                    ContextKind = kind.ToString().ToLowerInvariant(), ContextLabel = label, Problem = problem,
                    Repos = Array.Empty<RepoChanges>(),
                };

            var isRepo = await _git.RunAsync(root, new[] { "rev-parse", "--is-inside-work-tree" }, ct);
            if (!isRepo.Ok)
            {
                return new WorkingChangesView
                {
                    ContextKind = kind.ToString().ToLowerInvariant(),
                    ContextLabel = label,
                    RootPath = root,
                    NotAGitRepository = true,
                    Repos = Array.Empty<RepoChanges>(),
                };
            }

            // Chi esiste lo dice 'submodule status', MAI 'status': un submodule dichiarato ma non
            // popolato non produce nemmeno una riga di 'status' (verificato in F0), quindi
            // partendo da li' sparirebbe dalla vista invece di comparire come "non popolato".
            var subs = await ReadSubmodulesAsync(root, ct);
            var subPaths = new HashSet<string>(subs.Select(s => s.Path), StringComparer.OrdinalIgnoreCase);

            // Di quali il padre registra un commit diverso da quello in checkout: e' una modifica
            // DEL PADRE, che si committa nel padre — non del submodule.
            var moved = await ReadMovedPointersAsync(root, ct);

            // I submodule PRIMA della radice: gli avvisi sul push del padre si calcolano da come
            // stanno loro, e la radice va costruita gia' completa (i campi sono init-only).
            var subRepos = new List<RepoChanges>();
            foreach (var s in subs.OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase))
            {
                var depth = 1 + subs.Count(o => !ReferenceEquals(o, s) &&
                                                s.Path.StartsWith(o.Path + "/", StringComparison.OrdinalIgnoreCase));

                if (s.NotInitialized)
                {
                    // Dichiarato e mai popolato: non e' pulito e non e' un errore. C'e' gia' chi lo
                    // popola (ProjectSubmoduleInitializer), quindi e' una condizione risolvibile.
                    subRepos.Add(new RepoChanges
                    {
                        Path = s.Path, Label = s.Path, Depth = depth,
                        NotInitialized = true,
                        PointerMoved = moved.Contains(s.Path),
                        Files = Array.Empty<WorkingChange>(),
                        PushWarnings = Array.Empty<string>(),
                        CommitBlocker = "Submodule non popolato: non c'e' niente da committare finche' non viene scaricato.",
                    });
                    continue;
                }

                var dir = Path.Combine(root, s.Path.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(dir))
                {
                    subRepos.Add(new RepoChanges
                    {
                        Path = s.Path, Label = s.Path, Depth = depth,
                        NotInitialized = true,
                        Files = Array.Empty<WorkingChange>(),
                        PushWarnings = Array.Empty<string>(),
                        CommitBlocker = $"La cartella '{s.Path}' non esiste sul disco.",
                    });
                    continue;
                }

                var (unpublished, unknown) = await IsRecordedCommitUnpublishedAsync(root, dir, s.Path, ct);
                subRepos.Add(await ReadRepoAsync(dir, s.Path, s.Path, depth, null, moved.Contains(s.Path),
                                                 Array.Empty<string>(), unpublished, unknown, ct));
            }

            var repos = new List<RepoChanges>
            {
                await ReadRepoAsync(root, string.Empty,
                                    Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                                    0, subPaths, false, PushWarningsForRoot(subRepos), false, null, ct),
            };
            repos.AddRange(subRepos);

            var rootRepo = repos[0];
            return new WorkingChangesView
            {
                ContextKind = kind.ToString().ToLowerInvariant(),
                ContextLabel = label,
                RootPath = root,
                Branch = rootRepo.Branch,
                BaseBranch = rootRepo.BaseBranch,
                Repos = repos,
            };
        }

        /// <summary>
        /// Legge un repository: dove sta il suo ramo, e cosa e' diverso qui dentro.
        /// <paramref name="excludePaths"/> toglie dai file i percorsi che sono submodule — senza,
        /// una cartella comparirebbe travestita da documento, che e' il difetto da cui si parte.
        /// </summary>
        private async Task<RepoChanges> ReadRepoAsync(
            string dir, string relativePath, string label, int depth,
            ISet<string> excludePaths, bool pointerMoved, CancellationToken ct)
            => await ReadRepoAsync(dir, relativePath, label, depth, excludePaths, pointerMoved,
                                   Array.Empty<string>(), false, null, ct);

        private async Task<RepoChanges> ReadRepoAsync(
            string dir, string relativePath, string label, int depth,
            ISet<string> excludePaths, bool pointerMoved, IReadOnlyList<string> pushWarnings,
            bool recordedCommitUnpublished, string recordedCommitUnknown, CancellationToken ct)
        {
            var (branch, detached, upstream, ahead, behind) = await ReadBranchAsync(dir, ct);
            var baseRef = await ResolveBaseRefAsync(dir, ct);

            var files = new List<WorkingChange>();

            // Un solo confronto per entrambe le cose: 'diff <base>' senza i tre punti mette a
            // paragone il ramo di partenza con la working tree ATTUALE, quindi comprende sia i
            // commit gia' fatti sia le modifiche ancora da salvare. Con i tre punti vedremmo solo
            // i commit, e il lavoro in corso di una persona sparirebbe dalla vista.
            if (baseRef != null)
            {
                // '--ignore-submodules=all': un submodule sporco farebbe comparire la sua cartella
                // qui come "modificata". La sua riga esiste gia', con dentro i suoi file veri.
                var diff = await _git.RunAsync(dir, new[] { "diff", "--name-status", "-M", "--ignore-submodules=all", baseRef }, ct);
                if (diff.Ok) files.AddRange(ParseNameStatus(diff.Stdout));
                else _logger.LogDebug("[Changes] diff contro '{Base}' non riuscito in '{Dir}': {Why}", baseRef, dir, diff.Describe());
            }

            // I file mai aggiunti a git non compaiono in nessun diff: senza questo, un documento
            // appena creato — il caso piu' comune — risulterebbe inesistente.
            var status = await _git.RunAsync(dir, new[] { "status", "--porcelain", "--untracked-files=all", "--ignore-submodules=all" }, ct);
            if (status.Ok)
            {
                foreach (var line in SplitLines(status.Stdout))
                {
                    if (!line.StartsWith("?? ", StringComparison.Ordinal)) continue;
                    var path = Unquote(line.Substring(3).Trim());
                    if (files.Any(f => string.Equals(f.Path, path, StringComparison.Ordinal))) continue;
                    files.Add(new WorkingChange { Change = "untracked", Path = path });
                }
            }

            if (excludePaths != null && excludePaths.Count > 0)
                files.RemoveAll(f => excludePaths.Contains(f.Path.TrimEnd('/')));

            return new RepoChanges
            {
                Path = relativePath,
                Label = label,
                Depth = depth,
                Branch = branch,
                Detached = detached,
                Upstream = upstream,
                BaseBranch = baseRef,
                Ahead = ahead,
                Behind = behind,
                PointerMoved = pointerMoved,
                RecordedCommitUnpublished = recordedCommitUnpublished,
                RecordedCommitUnknown = recordedCommitUnknown,
                Files = files.OrderBy(f => f.Path, StringComparer.OrdinalIgnoreCase).ToList(),
                PushWarnings = pushWarnings ?? Array.Empty<string>(),
                CommitBlocker = detached
                    ? "HEAD staccato: un commit fatto qui non finirebbe su nessun ramo e resterebbe orfano."
                    : null,
            };
        }

        public async Task<string> DiffAsync(string projectPath, string agentName, string relativePath,
            string repoPath = null, CancellationToken ct = default)
        {
            var (root, _, _, problem) = await ResolveContextAsync(projectPath, agentName, ct);
            if (problem != null) throw new InvalidOperationException(problem);

            // Un file dentro un submodule appartiene a UN ALTRO repository: chiedere il suo diff
            // alla radice non darebbe niente, perche' la radice di quel file non sa nulla.
            var dir = await ResolveRepoDirAsync(root, repoPath, ct);
            var safe = SafeRelative(dir, relativePath);
            // Stesso riferimento della lista: se divergessero, un file elencato come cambiato
            // potrebbe mostrare un diff vuoto.
            var baseRef = await ResolveBaseRefAsync(dir, ct);

            // Un file mai aggiunto a git non ha un "prima": il diff contro il ramo di partenza
            // sarebbe vuoto e sembrerebbe che non sia cambiato niente. '--no-index' contro il
            // nulla mostra il contenuto come se fosse tutto aggiunto, che è la verità.
            var tracked = await _git.RunAsync(dir, new[] { "ls-files", "--error-unmatch", safe }, ct);
            if (!tracked.Ok)
            {
                var devnull = OperatingSystem.IsWindows() ? "NUL" : "/dev/null";
                var added = await _git.RunAsync(dir, new[] { "diff", "--no-index", "--", devnull, safe }, ct);
                // '--no-index' esce 1 quando i file differiscono: è il caso normale, non un errore.
                return added.Stdout ?? string.Empty;
            }

            var args = baseRef != null
                ? new[] { "diff", "-M", baseRef, "--", safe }
                : new[] { "diff", "-M", "--", safe };
            var res = await _git.RunAsync(dir, args, ct);
            if (!res.Ok && string.IsNullOrEmpty(res.Stdout))
                throw new InvalidOperationException($"Differenza non calcolabile per '{relativePath}': {res.Describe()}");
            return res.Stdout ?? string.Empty;
        }

        public async Task<string> DiscardAsync(string projectPath, string agentName, string relativePath,
            string repoPath = null, CancellationToken ct = default)
        {
            var (root, _, _, problem) = await ResolveContextAsync(projectPath, agentName, ct);
            if (problem != null) throw new InvalidOperationException(problem);

            var dir = await ResolveRepoDirAsync(root, repoPath, ct);
            var safe = SafeRelative(dir, relativePath);
            var full = Path.Combine(dir, safe);

            var tracked = await _git.RunAsync(dir, new[] { "ls-files", "--error-unmatch", safe }, ct);
            if (!tracked.Ok)
            {
                // Mai stato in git: l'unico modo di "riportarlo com'era" è che non esista.
                if (File.Exists(full)) File.Delete(full);
                return "eliminato";
            }

            var source = await ResolveBaseRefAsync(dir, ct) ?? "HEAD";
            var res = await _git.RunAsync(dir, new[] { "checkout", source, "--", safe }, ct);
            if (!res.Ok)
                throw new InvalidOperationException($"Ripristino di '{relativePath}' fallito: {res.Describe()}");
            return "ripristinato da " + source;
        }

        // ---- contesto ----

        private async Task<(string Root, string Label, ChangesContextKind Kind, string Problem)> ResolveContextAsync(
            string projectPath, string agentName, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return (null, null, ChangesContextKind.User, "Nessun progetto aperto.");

            if (string.IsNullOrWhiteSpace(agentName))
                return (projectPath, "Il tuo lavoro", ChangesContextKind.User, null);

            string worktree;
            try { worktree = await _worktree.FindAgentWorktreeAsync(projectPath, agentName, ct); }
            catch (ArgumentException ex) { return (null, agentName, ChangesContextKind.Agent, ex.Message); }

            if (worktree == null || !Directory.Exists(worktree))
                return (null, agentName, ChangesContextKind.Agent,
                    $"'{agentName}' non occupa nessun posto di lavoro: il suo lavoro va prima rimesso su un posto dalla revisione.");

            return (worktree, agentName, ChangesContextKind.Agent, null);
        }

        /// <summary>
        /// Il riferimento con cui confrontarsi qui, completo (es. <c>origin/develop-2026</c>).
        /// <para>
        /// È l'<b>upstream del ramo in checkout</b>: il ramo che l'utente vede. Il ramo predefinito
        /// del repository (<c>origin/HEAD</c>) risponderebbe a un'altra domanda — «di quanto questo
        /// ramo si discosta dal principale» — che su un ramo di lavoro lungo vuol dire mesi di
        /// lavoro già pubblicato. Misurato il 18/08 su MdExplorer: <b>469 file contro 20</b>.
        /// </para>
        /// <para>
        /// Senza upstream si ricade sul ramo predefinito, ed è il caso del ramo di un agente
        /// (<c>agent/&lt;nome&gt;/&lt;attività&gt;</c>), che un upstream non ce l'ha mai: lì la
        /// domanda giusta è davvero «cosa ha prodotto rispetto al ramo da cui è nato».
        /// </para>
        /// </summary>
        private async Task<string> ResolveBaseRefAsync(string dir, CancellationToken ct)
        {
            var upstream = await _git.RunAsync(dir,
                new[] { "rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{upstream}" }, ct);
            if (upstream.Ok)
            {
                var value = upstream.Stdout?.Trim();
                if (!string.IsNullOrEmpty(value)) return value;
            }

            var fallback = await ResolveBaseBranchAsync(dir, ct);
            return fallback != null ? "origin/" + fallback : null;
        }

        /// <summary>Il ramo di partenza del remote (<c>origin/HEAD</c>), <c>null</c> se non risolvibile.</summary>
        private async Task<string> ResolveBaseBranchAsync(string root, CancellationToken ct)
        {
            var res = await _git.RunAsync(root, new[] { "symbolic-ref", "--short", "refs/remotes/origin/HEAD" }, ct);
            if (res.Ok)
            {
                var value = res.Stdout?.Trim();
                if (!string.IsNullOrEmpty(value) && value.StartsWith("origin/", StringComparison.Ordinal))
                    return value.Substring("origin/".Length);
            }

            // Nessun origin/HEAD (repo mai clonato, remote senza default): se 'main' esiste sul
            // remote lo si usa, altrimenti si rinuncia — meglio nessun confronto che un confronto
            // contro un ramo inventato.
            foreach (var candidate in new[] { "main", "master" })
            {
                var probe = await _git.RunAsync(root, new[] { "rev-parse", "--verify", "--quiet", "origin/" + candidate }, ct);
                if (probe.Ok) return candidate;
            }
            return null;
        }

        /// <summary>
        /// La cartella del repository su cui agire: la radice, o uno dei suoi submodule.
        /// <para>
        /// Il percorso arriva dal client, quindi passa da <see cref="SafeRelative"/> — che
        /// impedisce di uscire dal contesto — e poi si verifica che li' ci sia davvero un
        /// repository git: un errore chiaro adesso, invece di un comando git eseguito nel posto
        /// sbagliato che risponde "nessuna differenza".
        /// </para>
        /// </summary>
        private async Task<string> ResolveRepoDirAsync(string root, string repoPath, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(repoPath)) return root;

            var safe = SafeRelative(root, repoPath);
            var dir = Path.Combine(root, safe.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(dir))
                throw new ArgumentException($"Il repository '{repoPath}' non esiste sul disco.", nameof(repoPath));

            var probe = await _git.RunAsync(dir, new[] { "rev-parse", "--is-inside-work-tree" }, ct);
            if (!probe.Ok)
                throw new ArgumentException($"'{repoPath}' non e' un repository git.", nameof(repoPath));

            return dir;
        }

        // ---- i repository ----

        private sealed class SubmoduleEntry
        {
            public string Path { get; init; }
            /// <summary>Prefisso <c>-</c>: dichiarato nel <c>.gitmodules</c> ma mai scaricato.</summary>
            public bool NotInitialized { get; init; }
        }

        /// <summary>
        /// I submodule dichiarati, <b>ricorsivamente</b>, col percorso relativo alla radice.
        /// <para>
        /// Fonte obbligata: <c>git status</c> non nomina un submodule non popolato, quindi
        /// enumerare da li' lo farebbe sparire invece di mostrarlo come "non popolato" (F0).
        /// Senza <c>.gitmodules</c> il comando non stampa niente ed esce 0: nessun submodule.
        /// </para>
        /// </summary>
        private async Task<List<SubmoduleEntry>> ReadSubmodulesAsync(string root, CancellationToken ct)
        {
            var list = new List<SubmoduleEntry>();
            var res = await _git.RunAsync(root, new[] { "submodule", "status", "--recursive" }, ct);
            if (!res.Ok)
            {
                _logger.LogDebug("[Changes] 'submodule status' non riuscito in '{Root}': {Why}", root, res.Describe());
                return list;
            }

            foreach (var line in SplitLines(res.Stdout))
            {
                // "<prefisso><sha> <path>[ (<ref>)]" — il prefisso e' un carattere solo:
                // ' ' allineato, '-' non inizializzato, '+' commit diverso, 'U' conflitti.
                if (line.Length < 3) continue;
                var prefix = line[0];
                var rest = line.Substring(1);
                var space = rest.IndexOf(' ');
                if (space <= 0) continue;

                var path = rest.Substring(space + 1).Trim();
                // Il riferimento fra parentesi in coda non fa parte del percorso.
                if (path.EndsWith(")", StringComparison.Ordinal))
                {
                    var open = path.LastIndexOf(" (", StringComparison.Ordinal);
                    if (open > 0) path = path.Substring(0, open).Trim();
                }
                if (path.Length == 0) continue;

                list.Add(new SubmoduleEntry { Path = path.Replace('\\', '/'), NotInitialized = prefix == '-' });
            }
            return list;
        }

        /// <summary>
        /// I submodule per cui <b>il padre</b> registra un commit diverso da quello in checkout.
        /// <para>
        /// Sta nella colonna <c>&lt;sub&gt;</c> di <c>--porcelain=v2</c>, che vale
        /// <c>S&lt;c&gt;&lt;m&gt;&lt;u&gt;</c>: <c>C</c> in prima posizione = commit cambiato
        /// (verificato: <c>SC..</c> puntatore spostato, <c>S.MU</c> solo contenuto sporco).
        /// È la distinzione fra "si committa nel padre" e "si committa nel submodule", e git la
        /// da' gia' fatta: non va ricostruita confrontando sha.
        /// </para>
        /// </summary>
        private async Task<HashSet<string>> ReadMovedPointersAsync(string root, CancellationToken ct)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var res = await _git.RunAsync(root, new[] { "status", "--porcelain=v2", "--untracked-files=no" }, ct);
            if (!res.Ok) return set;

            foreach (var line in SplitLines(res.Stdout))
            {
                if (!line.StartsWith("1 ", StringComparison.Ordinal)) continue;
                var f = line.Split(' ');
                if (f.Length < 9) continue;
                var sub = f[2];
                if (sub.Length < 4 || sub[0] != 'S' || sub[1] != 'C') continue;
                // Il percorso e' l'ultimo campo e puo' contenere spazi.
                set.Add(Unquote(string.Join(" ", f.Skip(8))));
            }
            return set;
        }

        /// <summary>
        /// Il commit che il progetto registra per questo submodule è già su un remoto?
        /// <para>
        /// È la stessa domanda che si fa <c>git push --recurse-submodules=check</c>, e si risponde
        /// allo stesso modo: si prende lo sha registrato dal padre e si guarda se è raggiungibile
        /// da un riferimento remoto del submodule. <c>rev-list &lt;sha&gt; --not --remotes</c> non
        /// stampa niente quando lo è.
        /// </para>
        /// <para>
        /// Se non si riesce a stabilirlo — lo sha non c'è nemmeno nel submodule, il comando
        /// fallisce — <b>si dichiara di non saperlo</b> invece di dare per buono. Chi decide se
        /// pubblicare deve poter distinguere «è a posto» da «non lo so».
        /// </para>
        /// </summary>
        private async Task<(bool Unpublished, string Unknown)> IsRecordedCommitUnpublishedAsync(
            string root, string dir, string submodulePath, CancellationToken ct)
        {
            var recorded = await _git.RunAsync(root, new[] { "rev-parse", $"HEAD:{submodulePath}" }, ct);
            var sha = recorded.Stdout?.Trim();
            if (!recorded.Ok || string.IsNullOrEmpty(sha))
                return (false, null);   // il padre non registra ancora niente: non c'è un rischio da segnalare

            var probe = await _git.RunAsync(dir, new[] { "rev-list", "--max-count=1", sha, "--not", "--remotes" }, ct);
            if (!probe.Ok)
                return (true, $"Il commit {sha.Substring(0, Math.Min(8, sha.Length))} registrato per " +
                              $"'{submodulePath}' non è verificabile qui: {probe.Describe()}");

            return (!string.IsNullOrWhiteSpace(probe.Stdout), null);
        }

        /// <summary>
        /// Dove sta il ramo di questo repository: nome, upstream e di quanto e' avanti o indietro.
        /// Una sola chiamata: le righe <c>#</c> di <c>--porcelain=v2 --branch</c> le dicono tutte.
        /// </summary>
        private async Task<(string Branch, bool Detached, string Upstream, int Ahead, int Behind)> ReadBranchAsync(
            string dir, CancellationToken ct)
        {
            string branch = null, upstream = null;
            var detached = false;
            int ahead = 0, behind = 0;

            var res = await _git.RunAsync(dir, new[] { "status", "--porcelain=v2", "--branch", "--untracked-files=no" }, ct);
            if (!res.Ok) return (null, false, null, 0, 0);

            foreach (var line in SplitLines(res.Stdout))
            {
                if (!line.StartsWith("# ", StringComparison.Ordinal)) break;   // le righe '#' vengono per prime

                if (line.StartsWith("# branch.head ", StringComparison.Ordinal))
                {
                    var value = line.Substring("# branch.head ".Length).Trim();
                    if (value == "(detached)") detached = true; else branch = value;
                }
                else if (line.StartsWith("# branch.upstream ", StringComparison.Ordinal))
                {
                    upstream = line.Substring("# branch.upstream ".Length).Trim();
                }
                else if (line.StartsWith("# branch.ab ", StringComparison.Ordinal))
                {
                    foreach (var token in line.Substring("# branch.ab ".Length).Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (token.Length < 2 || !int.TryParse(token.Substring(1), out var n)) continue;
                        if (token[0] == '+') ahead = n;
                        else if (token[0] == '-') behind = n;
                    }
                }
            }
            return (branch, detached, upstream, ahead, behind);
        }

        /// <summary>
        /// Perche' pushare il padre adesso romperebbe il repository per gli altri.
        /// <para>
        /// È il disastro classico dei submodule: il padre pubblica un riferimento a un commit che
        /// sul remoto del figlio non esiste, e l'errore lo vede <b>chi clona</b>, giorni dopo. Qui
        /// si dice <b>prima</b>; a impedirlo materialmente ci pensa
        /// <c>--recurse-submodules=on-demand</c>, che pusha il padre per ultimo (F5).
        /// </para>
        /// </summary>
        private static IReadOnlyList<string> PushWarningsForRoot(IEnumerable<RepoChanges> submodules)
        {
            var warnings = new List<string>();
            foreach (var s in submodules)
            {
                if (s.RecordedCommitUnknown != null)
                {
                    warnings.Add(s.RecordedCommitUnknown);
                }
                else if (s.RecordedCommitUnpublished)
                {
                    // Il caso che rompe il repository per gli altri, e l'unico che va detto sempre:
                    // vale anche con HEAD staccato, dove 'Ahead' e' 0 e non segnalerebbe niente.
                    warnings.Add(s.Detached
                        ? $"'{s.Path}' ha HEAD staccato e il commit registrato non e' su nessun remoto: non e' pubblicabile finche' non lo metti su un ramo."
                        : $"'{s.Path}' punta a un commit non pubblicato: va pubblicato per primo, altrimenti chi clona trova un riferimento inesistente.");
                }
                else if (s.Ahead > 0)
                {
                    warnings.Add($"'{s.Path}' ha {s.Ahead} commit non pushati.");
                }
                else if (!s.NotInitialized && s.Upstream == null && s.Branch != null)
                {
                    warnings.Add($"'{s.Path}' non ha un ramo remoto configurato: il suo lavoro non e' pubblicabile cosi' com'e'.");
                }
            }
            return warnings;
        }

        // ---- lettura dell'output di git ----

        internal static IReadOnlyList<WorkingChange> ParseNameStatus(string output)
        {
            var list = new List<WorkingChange>();
            foreach (var line in SplitLines(output))
            {
                var parts = line.Split('\t');
                if (parts.Length < 2) continue;
                var code = parts[0].Trim();

                if (code.StartsWith("R", StringComparison.Ordinal) && parts.Length >= 3)
                {
                    list.Add(new WorkingChange { Change = "renamed", OldPath = Unquote(parts[1]), Path = Unquote(parts[2]) });
                    continue;
                }

                var change = code[0] switch
                {
                    'A' => "added",
                    'D' => "deleted",
                    'M' => "modified",
                    'C' => "added",
                    'T' => "modified",
                    _ => null,
                };
                if (change == null) continue;
                list.Add(new WorkingChange { Change = change, Path = Unquote(parts[1]) });
            }
            return list;
        }

        private static IEnumerable<string> SplitLines(string s)
            => (s ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.TrimEnd('\r'));

        /// <summary>Git cita i percorsi con caratteri speciali: senza toglierle, il path non aprirebbe niente.</summary>
        private static string Unquote(string path)
        {
            var p = (path ?? string.Empty).Trim();
            if (p.Length >= 2 && p[0] == '"' && p[^1] == '"')
                p = p.Substring(1, p.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
            return p;
        }

        /// <summary>
        /// Il percorso arriva dal client: deve restare dentro la radice del contesto. Senza questo
        /// un <c>../../</c> farebbe leggere — o peggio ripristinare — file fuori dal progetto.
        /// </summary>
        private static string SafeRelative(string root, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Percorso mancante.", nameof(relativePath));

            var normalized = relativePath.Replace('\\', '/').TrimStart('/');
            var full = Path.GetFullPath(Path.Combine(root, normalized));
            var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Percorso fuori dal contesto: '{relativePath}'.", nameof(relativePath));

            return normalized;
        }
    }
}
