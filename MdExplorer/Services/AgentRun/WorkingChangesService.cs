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
        public IReadOnlyList<WorkingChange> Files { get; init; }
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

        /// <summary>Differenza testuale di un singolo file, in formato unified diff.</summary>
        Task<string> DiffAsync(string projectPath, string agentName, string relativePath, CancellationToken ct = default);

        /// <summary>
        /// Butta via le modifiche a un singolo file, riportandolo com'era sul ramo di partenza
        /// (o eliminandolo, se non esisteva). Irreversibile: nessun commit lo trattiene.
        /// </summary>
        Task<string> DiscardAsync(string projectPath, string agentName, string relativePath, CancellationToken ct = default);
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
                return new WorkingChangesView { ContextKind = kind.ToString().ToLowerInvariant(), ContextLabel = label, Problem = problem, Files = Array.Empty<WorkingChange>() };

            var isRepo = await _git.RunAsync(root, new[] { "rev-parse", "--is-inside-work-tree" }, ct);
            if (!isRepo.Ok)
            {
                return new WorkingChangesView
                {
                    ContextKind = kind.ToString().ToLowerInvariant(),
                    ContextLabel = label,
                    RootPath = root,
                    NotAGitRepository = true,
                    Files = Array.Empty<WorkingChange>(),
                };
            }

            var branch = (await _git.RunAsync(root, new[] { "rev-parse", "--abbrev-ref", "HEAD" }, ct)).Stdout?.Trim();
            var baseBranch = await ResolveBaseBranchAsync(root, ct);

            var files = new List<WorkingChange>();

            // Un solo confronto per entrambe le cose: 'diff <base>' senza i tre punti mette a
            // paragone il ramo di partenza con la working tree ATTUALE, quindi comprende sia i
            // commit già fatti sia le modifiche ancora da salvare. Con i tre punti vedremmo solo
            // i commit, e il lavoro in corso di una persona sparirebbe dalla vista.
            var baseRef = baseBranch != null ? "origin/" + baseBranch : null;
            if (baseRef != null)
            {
                var diff = await _git.RunAsync(root, new[] { "diff", "--name-status", "-M", baseRef }, ct);
                if (diff.Ok) files.AddRange(ParseNameStatus(diff.Stdout));
                else _logger.LogDebug("[Changes] diff contro '{Base}' non riuscito: {Why}", baseRef, diff.Describe());
            }

            // I file mai aggiunti a git non compaiono in nessun diff: senza questo, un documento
            // appena creato dall'agente — il caso più comune — risulterebbe inesistente.
            var status = await _git.RunAsync(root, new[] { "status", "--porcelain", "--untracked-files=all" }, ct);
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

            return new WorkingChangesView
            {
                ContextKind = kind.ToString().ToLowerInvariant(),
                ContextLabel = label,
                RootPath = root,
                Branch = string.IsNullOrEmpty(branch) || branch == "HEAD" ? null : branch,
                BaseBranch = baseBranch,
                Files = files.OrderBy(f => f.Path, StringComparer.OrdinalIgnoreCase).ToList(),
            };
        }

        public async Task<string> DiffAsync(string projectPath, string agentName, string relativePath, CancellationToken ct = default)
        {
            var (root, _, _, problem) = await ResolveContextAsync(projectPath, agentName, ct);
            if (problem != null) throw new InvalidOperationException(problem);

            var safe = SafeRelative(root, relativePath);
            var baseBranch = await ResolveBaseBranchAsync(root, ct);

            // Un file mai aggiunto a git non ha un "prima": il diff contro il ramo di partenza
            // sarebbe vuoto e sembrerebbe che non sia cambiato niente. '--no-index' contro il
            // nulla mostra il contenuto come se fosse tutto aggiunto, che è la verità.
            var tracked = await _git.RunAsync(root, new[] { "ls-files", "--error-unmatch", safe }, ct);
            if (!tracked.Ok)
            {
                var devnull = OperatingSystem.IsWindows() ? "NUL" : "/dev/null";
                var added = await _git.RunAsync(root, new[] { "diff", "--no-index", "--", devnull, safe }, ct);
                // '--no-index' esce 1 quando i file differiscono: è il caso normale, non un errore.
                return added.Stdout ?? string.Empty;
            }

            var args = baseBranch != null
                ? new[] { "diff", "-M", "origin/" + baseBranch, "--", safe }
                : new[] { "diff", "-M", "--", safe };
            var res = await _git.RunAsync(root, args, ct);
            if (!res.Ok && string.IsNullOrEmpty(res.Stdout))
                throw new InvalidOperationException($"Differenza non calcolabile per '{relativePath}': {res.Describe()}");
            return res.Stdout ?? string.Empty;
        }

        public async Task<string> DiscardAsync(string projectPath, string agentName, string relativePath, CancellationToken ct = default)
        {
            var (root, _, _, problem) = await ResolveContextAsync(projectPath, agentName, ct);
            if (problem != null) throw new InvalidOperationException(problem);

            var safe = SafeRelative(root, relativePath);
            var full = Path.Combine(root, safe);

            var tracked = await _git.RunAsync(root, new[] { "ls-files", "--error-unmatch", safe }, ct);
            if (!tracked.Ok)
            {
                // Mai stato in git: l'unico modo di "riportarlo com'era" è che non esista.
                if (File.Exists(full)) File.Delete(full);
                return "eliminato";
            }

            var baseBranch = await ResolveBaseBranchAsync(root, ct);
            var source = baseBranch != null ? "origin/" + baseBranch : "HEAD";
            var res = await _git.RunAsync(root, new[] { "checkout", source, "--", safe }, ct);
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
