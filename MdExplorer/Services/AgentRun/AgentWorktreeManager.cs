using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Utilities;
using MdExplorer.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>Esito del prepare-before-run di un worktree (Fase 7c/7d).</summary>
    public sealed class WorktreePrepareResult
    {
        public bool Success { get; private set; }
        public string WorktreePath { get; private set; }
        /// <summary>Il merge del branch di handoff è in conflitto → il chiamante lo mappa su not-ready/merge-conflict-with-main.</summary>
        public bool MergeConflict { get; private set; }
        /// <summary>La sync al ref/commit di handoff è fallita (ref assente/fetch ko, Fase 7d.5) → not-ready/git-sync-failed.</summary>
        public bool SyncFailed { get; private set; }
        public string Error { get; private set; }

        public static WorktreePrepareResult Ok(string path) => new() { Success = true, WorktreePath = path };
        public static WorktreePrepareResult Conflict(string path) => new() { Success = false, MergeConflict = true, WorktreePath = path, Error = "merge-conflict-with-main" };
        public static WorktreePrepareResult SyncFail(string path) => new() { Success = false, SyncFailed = true, WorktreePath = path, Error = "git-sync-failed" };
        public static WorktreePrepareResult Fail(string error) => new() { Success = false, Error = error };
    }

    /// <summary>Esito dell'auto-merge di un deliverable nel default (Fase 7g).</summary>
    public enum DeliverableMergeOutcome
    {
        /// <summary>Fuso nel default e pushato su origin.</summary>
        Merged,
        /// <summary>Conflitto di merge → il chiamante lo mappa su not-ready (l'agente rilavora).</summary>
        Conflict,
        /// <summary>Fallito (fetch/checkout/push ko, es. non-fast-forward): ritentabile.</summary>
        Failed,
    }

    /// <summary>Esito del commit+push del branch d'attività di un agente (Fase 7d.2/7d.5).</summary>
    public sealed class HandoffPushResult
    {
        /// <summary>Nome del branch pushato (ref COMPLETO da usare come handoffRef, es. <c>agent/&lt;A&gt;/&lt;id&gt;</c>).</summary>
        /// <summary>
        /// Nome <b>pubblicato</b> su origin: parlante, ricavato dai file toccati. È quello da
        /// spedire al collega come ref di handoff — il locale su origin non esiste.
        /// </summary>
        public string Branch { get; init; }

        /// <summary>
        /// Nome <b>locale</b> del branch d'attività. Serve alle operazioni git in casa (merge,
        /// cancellazione): il nome pubblicato non ha un ref locale, e usarlo per un <c>merge</c>
        /// lo farebbe fallire — che è esattamente ciò che è successo la prima volta.
        /// </summary>
        public string LocalBranch { get; init; }
        /// <summary>Sha del commit di testa pushato (baseCommit a cui il ricevente deve sincronizzarsi).</summary>
        public string HeadSha { get; init; }
    }

    /// <summary>
    /// Isolamento d'esecuzione per-agente (Fase 7c, §7bis): ogni agente ha un <b>worktree git
    /// persistente</b> FUORI dal progetto sorvegliato, in <c>{AppData}/MdExplorer/worktrees/
    /// {project-hash}/{agente}</c>. A ogni risveglio il worktree è "preparato" (fetch + reset +
    /// branch fresco per-attività da <c>origin/&lt;base&gt;</c> + merge dell'eventuale handoff),
    /// così il lavoro dell'agente non tocca la working tree dell'umano.
    /// <para>
    /// <b>LibGit2Sharp non sa fare <c>git worktree</c></b> → tutte le operazioni shellano il
    /// binario <c>git</c> (stesso pattern di <c>ModernGitService.RunNativeGitAsync</c>, con
    /// <c>GIT_TERMINAL_PROMPT=0</c>). Opt-in: gira solo se <c>agentCity.useAgentWorktrees</c>.
    /// </para>
    /// </summary>
    public interface IAgentWorktreeManager
    {
        /// <summary>Crea (se assente) il worktree dell'agente e ne ritorna il path. Idempotente.</summary>
        Task<string> EnsureWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default);

        /// <summary>
        /// Prepara il worktree per un run: fetch, reset --hard + clean -fd (MAI -x), branch
        /// <c>agent/&lt;agente&gt;/&lt;activityId&gt;</c> da <c>origin/&lt;baseBranch&gt;</c>, e —
        /// se <paramref name="handoffRef"/> è dato (ref COMPLETO) — merge di <c>origin/&lt;handoffRef&gt;</c>.
        /// Conflitto di merge → <see cref="WorktreePrepareResult.MergeConflict"/> (non auto-risolve).
        /// </summary>
        Task<WorktreePrepareResult> PrepareForRunAsync(
            string projectPath, string agentName, string activityId,
            string baseBranch = null, string handoffRef = null, CancellationToken ct = default);

        /// <summary>Rimuove il worktree dell'agente (git worktree remove + prune; best-effort se il progetto è sparito).</summary>
        Task RemoveWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default);

        /// <summary>
        /// Fase 7d.2/7d.5 — nel worktree dell'agente: <c>commit -a</c> (identità dell'agente, salta
        /// se pulito) poi push del branch d'attività su origin (refspec, non upstream-only).
        /// Ritorna branch (ref completo) + sha di testa, o <c>null</c> se non c'è worktree o il push
        /// fallisce. È il "commit → push" che precede la richiesta federata (§6).
        /// </summary>
        Task<HandoffPushResult> CommitAndPushBranchAsync(string projectPath, string agentName, string commitMessage, CancellationToken ct = default);

        /// <summary>Fase 7d.1 — merge esplicito <paramref name="sourceRef"/> in <paramref name="intoBranch"/> (native). Metodo base.</summary>
        /// <summary>File toccati dal lavoro dell'agente, per la revisione umana.</summary>
        Task<IReadOnlyList<ChangedFile>> ChangedFilesAsync(string projectPath, string agentName, CancellationToken ct = default);

        Task<bool> MergeBranchAsync(string projectPath, string sourceRef, string intoBranch, CancellationToken ct = default);

        /// <summary>
        /// Fase 7g — fonde il branch d'attività nel default e lo pusha su origin, SENZA disturbare
        /// la working tree dell'umano: opera nel worktree dell'agente in <b>detached HEAD</b> su
        /// <c>origin/&lt;default&gt;</c> (git vieta lo stesso branch in due worktree), poi
        /// <c>push origin HEAD:&lt;default&gt;</c>. Conflitto → <see cref="DeliverableMergeOutcome.Conflict"/>.
        /// </summary>
        Task<DeliverableMergeOutcome> MergeDeliverableIntoDefaultAsync(string projectPath, string agentName, string activityBranch, CancellationToken ct = default);

        /// <summary>Fase 7d.1 — cancella un branch (locale e, se <paramref name="remoteToo"/>, sul remote).</summary>
        Task DeleteBranchAsync(string projectPath, string branch, bool remoteToo, CancellationToken ct = default);

        /// <summary>Fase 7d.1/7d.4 — branch <c>agent/*</c> già fusi in <paramref name="intoBranch"/> (native <c>git branch --merged</c>).</summary>
        Task<IReadOnlyList<string>> ListMergedAgentBranchesAsync(string projectPath, string intoBranch, CancellationToken ct = default);

        /// <summary>
        /// Fase 7e.1 — path relativi dei submodule "toccati" (sporchi) nel worktree, per il gate del
        /// codice (§6bis). Legge i submodule dal <c>.gitmodules</c> e li incrocia con
        /// <c>git status --porcelain</c>. Vuoto se il worktree non ha submodule o sono puliti.
        /// </summary>
        Task<IReadOnlyList<string>> GetDirtySubmodulesAsync(string worktreePath, CancellationToken ct = default);

        /// <summary>
        /// Dove stanno i posti di lavoro degli agenti: <c>{progetto}/.worktrees</c>.
        /// <para>
        /// Dentro il progetto, non in AppData, perché un worktree è utile solo finché il progetto
        /// esiste: spostare o cancellare la documentazione portava via i worktree lasciando in
        /// AppData cartelle che nessuno collegava più a niente. La cartella è esclusa da indice,
        /// albero e git (<see cref="MdExplorer.Service.Services.FoldersIgnoreService.AgentWorktreesFolder"/>).
        /// </para>
        /// </summary>
        string WorktreeRootForProject(string projectPath);

        /// <summary>
        /// Il posto attualmente occupato da <paramref name="agentName"/>, oppure <c>null</c> se
        /// l'agente non ne ha uno.
        /// <para>
        /// Non è più una funzione del nome: con il pool un agente non ha una cartella sua, occupa
        /// uno dei posti disponibili, e quale sia lo dice <b>git</b> — il branch in checkout nel
        /// posto. Chiedere a git invece che tenere un registro a parte significa che spegnere
        /// l'applicazione, rimuovere un worktree a mano o cambiare macchina non lasciano mai
        /// un'assegnazione che dice una cosa mentre il disco ne dice un'altra.
        /// </para>
        /// </summary>
        Task<string> FindAgentWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default);

        /// <summary>I posti esistenti e chi li occupa, per la vista di revisione e per il reaper.</summary>
        Task<IReadOnlyList<WorktreeSlot>> ListSlotsAsync(string projectPath, CancellationToken ct = default);

        /// <summary>
        /// Rimette il lavoro di un agente su un posto perché una persona possa metterci mano.
        /// <para>
        /// Serve perché con il pool il posto dov'è nato quel lavoro può essere già stato preso da
        /// un altro agente. Il lavoro però non è andato perso — è un branch — quindi lo si
        /// rimaterializza: si prende un posto e ci si fa il check-out. Da qui in poi la sessione
        /// d'intervento lo protegge da chi vorrebbe subentrare.
        /// </para>
        /// </summary>
        Task<string> MaterializeForReviewAsync(string projectPath, string agentName, string localBranch, CancellationToken ct = default);

        /// <summary>
        /// Il turno è finito: il posto torna disponibile. Va chiamato comunque sia andata —
        /// altrimenti un run fallito prima del commit terrebbe la scrivania occupata.
        /// </summary>
        void ReleaseSlot(string worktreePath);

        /// <summary>Rimuove un posto del pool (usato dal reaper per i posti oltre il limite).</summary>
        Task RemoveSlotAsync(string projectPath, int slotIndex, CancellationToken ct = default);

        /// <summary>
        /// <c>git worktree prune</c>: toglie da <c>.git/worktrees</c> i riferimenti a cartelle
        /// sparite. Senza, git rifiuta di ricreare un posto con lo stesso nome.
        /// </summary>
        Task PruneWorktreesAsync(string projectPath, CancellationToken ct = default);
    }

    /// <summary>Un posto di lavoro del pool e il suo occupante attuale.</summary>
    public sealed class WorktreeSlot
    {
        /// <summary>Numero del posto (1..N), come compare nel path: <c>.worktrees/slot-1</c>.</summary>
        public int Index { get; init; }
        public string Path { get; init; }
        /// <summary>Agente che ci sta lavorando, dedotto dal branch in checkout. <c>null</c> = libero.</summary>
        public string Agent { get; init; }
        /// <summary>Branch in checkout, o <c>null</c> se detached/indeterminato.</summary>
        public string Branch { get; init; }
        /// <summary>Data dell'ultimo commit visibile dal posto: dice quale posto è fermo da più tempo.</summary>
        public DateTimeOffset LastActivityUtc { get; init; }
        /// <summary>C'è una sessione d'intervento umana aperta: il posto non si tocca.</summary>
        public bool Held { get; init; }
    }

    public sealed class AgentWorktreeManager : IAgentWorktreeManager, IDisposable
    {
        // git non trovato / timeout: codici sentinella (fuori dal range 0..255 dei veri exit).
        private const int GitNotFoundExit = -9999;
        private const int GitTimeoutExit = -9998;
        private const int DefaultTimeoutMs = 300000;

        private readonly ILogger<AgentWorktreeManager> _logger;
        // Un lock per repo (project-hash): git worktree add/remove toccano lo stesso .git/worktrees;
        // e un agente esegue una cosa alla volta (actor model). Serializza per repo, non globale.
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _repoGates = new();

        // Posti prenotati da un run in corso: path del posto → quando è cominciato.
        // Senza questo, l'eviction sceglierebbe per data dell'ultimo commit e un agente appena
        // partito — che di commit non ne ha ancora fatti — sarebbe il candidato ideale a farsi
        // togliere la scrivania mentre ci sta scrivendo. La prenotazione scade da sola: se un
        // processo muore a metà run, il posto non resta bloccato per sempre.
        private readonly ConcurrentDictionary<string, (string Agent, DateTimeOffset Since)> _leases =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly TimeSpan LeaseMaxAge = TimeSpan.FromHours(6);

        /// <summary>
        /// Il posto è prenotato da <b>qualcun altro</b>. La prenotazione porta il nome di chi l'ha
        /// presa perché altrimenti impedirebbe anche a lui di tornarci: un agente che prepara due
        /// volte di fila si troverebbe la propria prenotazione a sbarrargli la strada.
        /// </summary>
        private bool IsLeasedByOthers(string slotPath, string agentName)
            => _leases.TryGetValue(slotPath, out var lease)
               && DateTimeOffset.UtcNow - lease.Since < LeaseMaxAge
               && !string.Equals(lease.Agent, agentName, StringComparison.OrdinalIgnoreCase);

        private bool IsLeased(string slotPath)
            => _leases.TryGetValue(slotPath, out var lease)
               && DateTimeOffset.UtcNow - lease.Since < LeaseMaxAge;
        private bool _disposed;

        private readonly IAgentWorktreeHoldService _hold;
        private readonly MdExplorer.Services.Federation.IEffectiveOwnerIdentity _ownerIdentity;
        private readonly IAgentWorktreePreference _preference;

        public AgentWorktreeManager(
            ILogger<AgentWorktreeManager> logger,
            IAgentWorktreeHoldService hold,
            MdExplorer.Services.Federation.IEffectiveOwnerIdentity ownerIdentity,
            IAgentWorktreePreference preference)
        {
            _logger = logger;
            _hold = hold;
            _ownerIdentity = ownerIdentity;
            _preference = preference;
        }

        public string WorktreeRootForProject(string projectPath)
            => Path.Combine(projectPath ?? string.Empty,
                MdExplorer.Service.Services.FoldersIgnoreService.AgentWorktreesFolder);

        // Allowlist stretta per il nome agente usato come componente di path e di branch: solo
        // identificatori (i nomi a2a sono kebab-case). Blocca traversal ('..', separatori) e
        // qualsiasi carattere che possa uscire dalla dir dell'agente o confondere git.
        private static readonly System.Text.RegularExpressions.Regex SafeAgentName =
            new("^[A-Za-z0-9._-]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

        /// <summary>
        /// Il nome dell'agente finisce in un path e in un nome di branch: qui si ferma tutto ciò
        /// che potrebbe uscire dalla cartella del pool o confondere git.
        /// </summary>
        public static string ValidateAgentName(string agentName)
        {
            var name = (agentName ?? string.Empty).Trim();
            if (name.Length == 0 || name == "." || name == ".." || !SafeAgentName.IsMatch(name))
                throw new ArgumentException($"Nome agente non valido per un path di worktree: '{agentName}'.", nameof(agentName));
            return name;
        }

        public async Task<string> EnsureWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default)
        {
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                return await AcquireSlotUnlockedAsync(projectPath, agentName, ct);
            }
            finally { gate.Release(); }
        }

        /// <summary>
        /// Assegna un posto del pool a <paramref name="agentName"/> — riusandolo, creandone uno
        /// nuovo se ci sta nel limite, o subentrando in quello fermo da più tempo.
        /// <para>
        /// Il pool esiste perché un worktree costa una copia intera del progetto: con un posto per
        /// agente, una città di dieci agenti moltiplicava per dieci la documentazione sul disco.
        /// Con due posti (il default), due agenti lavorano davvero in parallelo e il terzo aspetta
        /// il suo turno subentrando — che è esattamente quello che farebbe una persona con due
        /// scrivanie.
        /// </para>
        /// <para>
        /// Subentrare non perde niente: il lavoro dell'agente precedente è già un commit sul suo
        /// branch e, se aveva consegnato, anche su origin. Quello che <b>non</b> si tocca mai è un
        /// posto con una sessione d'intervento aperta: lì dentro c'è del lavoro umano non salvato.
        /// </para>
        /// </summary>
        private async Task<string> AcquireSlotUnlockedAsync(string projectPath, string agentName, CancellationToken ct)
        {
            var name = ValidateAgentName(agentName);
            var root = WorktreeRootForProject(projectPath);
            var slots = await ReadSlotsUnlockedAsync(projectPath, ct);

            // 1) L'agente ha già un posto: ci torna. Il prepare lo riporterà in ordine.
            var own = slots.FirstOrDefault(x => string.Equals(x.Agent, name, StringComparison.OrdinalIgnoreCase));
            if (own != null)
            {
                _leases[own.Path] = (name, DateTimeOffset.UtcNow);
                return own.Path;
            }

            var limit = _preference.SlotsFor(projectPath);

            // 2) Un posto libero (nessuno ci lavora) entro il limite: il caso normale all'avvio.
            var free = slots.FirstOrDefault(
                x => x.Agent == null && !x.Held && !IsLeasedByOthers(x.Path, name) && x.Index <= limit);
            if (free != null)
            {
                _leases[free.Path] = (name, DateTimeOffset.UtcNow);
                return free.Path;
            }

            // 3) C'è spazio nel limite per un posto nuovo.
            if (slots.Count(x => x.Index <= limit) < limit)
            {
                var used = new HashSet<int>(slots.Select(x => x.Index));
                var index = Enumerable.Range(1, limit).First(i => !used.Contains(i));
                var path = Path.Combine(root, "slot-" + index);

                Directory.CreateDirectory(root);
                var (code, _, err) = await GitAsync(projectPath, new[] { "worktree", "add", "--detach", path }, ct);
                if (code != 0)
                    throw new InvalidOperationException(
                        $"Creazione del posto di lavoro {index} fallita in '{projectPath}' (è un repo git?): {Describe(code, err)}");

                _leases[path] = (name, DateTimeOffset.UtcNow);
                _logger.LogInformation("[Worktree] posto {Index} creato per '{Agent}': {Path}", index, name, path);
                return path;
            }

            // 4) Tutti occupati: subentro in quello fermo da più tempo, saltando le sessioni umane.
            var victim = slots.Where(x => !x.Held && !IsLeasedByOthers(x.Path, name))
                              .OrderByDescending(x => x.Index > limit)   // i posti oltre il limite vanno riassorbiti per primi
                              .ThenBy(x => x.LastActivityUtc)
                              .FirstOrDefault();
            if (victim == null)
                throw new InvalidOperationException(
                    $"Tutti i {slots.Count} posti di lavoro di '{projectPath}' sono occupati da un run in corso o da " +
                    $"una sessione d'intervento aperta: '{name}' non ha dove lavorare. Aspetta che un agente finisca, " +
                    "concludi una revisione dalla vista di revisione, oppure aumenta i posti nelle impostazioni del progetto.");

            _logger.LogInformation(
                "[Worktree] '{Agent}' subentra nel posto {Index}, lasciato da '{Previous}' (fermo dal {When:u}).",
                name, victim.Index, victim.Agent ?? "nessuno", victim.LastActivityUtc);
            _leases[victim.Path] = (name, DateTimeOffset.UtcNow);
            return victim.Path;
        }

        /// <summary>Legge i posti esistenti da disco e da git. Chiamare con il gate del repo preso.</summary>
        private async Task<List<WorktreeSlot>> ReadSlotsUnlockedAsync(string projectPath, CancellationToken ct)
        {
            var root = WorktreeRootForProject(projectPath);
            var result = new List<WorktreeSlot>();
            if (!Directory.Exists(root)) return result;

            IEnumerable<string> dirs;
            try { dirs = Directory.EnumerateDirectories(root, "slot-*"); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Worktree] lettura dei posti in '{Root}' fallita.", root);
                return result;
            }

            foreach (var dir in dirs)
            {
                if (!int.TryParse(Path.GetFileName(dir).Substring("slot-".Length), out var index))
                    continue;

                // Cartella che non è (più) un worktree valido: la tolgo di mezzo e la ricreo dopo.
                var (probe, _, _) = await GitAsync(dir, new[] { "rev-parse", "--is-inside-work-tree" }, ct);
                if (probe != 0)
                {
                    _logger.LogWarning("[Worktree] '{Path}' non è un worktree valido: lo rimuovo.", dir);
                    await RemoveWorktreeUnlockedAsync(projectPath, "slot-" + index, dir, ct);
                    continue;
                }

                var (_, branchOut, _) = await GitAsync(dir, new[] { "rev-parse", "--abbrev-ref", "HEAD" }, ct);
                var branch = (branchOut ?? string.Empty).Trim();
                if (branch.Length == 0 || branch == "HEAD") branch = null;

                var agent = AgentOfBranch(branch);

                var when = DateTimeOffset.MinValue;
                var (lc, lout, _) = await GitAsync(dir, new[] { "log", "-1", "--format=%ct" }, ct);
                if (lc == 0 && long.TryParse((lout ?? string.Empty).Trim(), out var epoch))
                    when = DateTimeOffset.FromUnixTimeSeconds(epoch);

                result.Add(new WorktreeSlot
                {
                    Index = index,
                    Path = dir,
                    Agent = agent,
                    Branch = branch,
                    LastActivityUtc = when,
                    Held = agent != null && _hold.IsHeld(projectPath, agent),
                });
            }

            return result.OrderBy(x => x.Index).ToList();
        }

        /// <summary>
        /// Allinea i submodule al commit pinnato dal branch in checkout. Restituisce <c>null</c>
        /// se è andata, altrimenti il motivo — che il chiamante deve trattare come un fallimento,
        /// non ignorare: un agente con il codice assente o al commit sbagliato documenterebbe una
        /// realtà che non esiste.
        /// </summary>
        private async Task<string> SyncSubmodulesAsync(string worktreePath, CancellationToken ct)
        {
            if (!File.Exists(Path.Combine(worktreePath, ".gitmodules")))
                return null;

            // sync: l'URL nel .gitmodules può essere cambiato SUL BRANCH appena messo in checkout.
            var (syc, _, sye) = await GitAsync(worktreePath, new[] { "submodule", "sync", "--recursive" }, ct);
            if (syc != 0)
                return $"git submodule sync fallito: {Describe(syc, sye)}";

            // --force è il pezzo che mancava: senza, un submodule con modifiche locali resta com'è
            // e se le porta nel run successivo. --recursive per i nidificati.
            var (sc, _, se) = await GitAsync(worktreePath,
                new[] { "submodule", "update", "--init", "--recursive", "--force" }, ct);
            if (sc != 0)
                return $"git submodule update fallito: {Describe(sc, se)}. " +
                       "L'agente lavorerebbe con il codice assente o disallineato rispetto alla documentazione.";

            // Il 'clean -fd' del padre non scende nei submodule: i file non tracciati lasciati lì
            // dentro sopravvivrebbero. MAI -x, come per il padre: gli ignorati (build, dipendenze)
            // restano e non si ricostruiscono ogni volta.
            await GitAsync(worktreePath, new[] { "submodule", "foreach", "--recursive", "git clean -fd" }, ct);
            return null;
        }

        /// <summary>Da <c>agent/&lt;nome&gt;/&lt;attività&gt;</c> al nome dell'agente. Il branch È l'assegnazione.</summary>
        private static string AgentOfBranch(string branch)
        {
            if (string.IsNullOrEmpty(branch) || !branch.StartsWith("agent/", StringComparison.Ordinal))
                return null;
            var rest = branch.Substring("agent/".Length);
            var slash = rest.IndexOf('/');
            return slash > 0 ? rest.Substring(0, slash) : null;
        }

        public async Task<string> FindAgentWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default)
        {
            var name = ValidateAgentName(agentName);
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var slots = await ReadSlotsUnlockedAsync(projectPath, ct);
                return slots.FirstOrDefault(x => string.Equals(x.Agent, name, StringComparison.OrdinalIgnoreCase))?.Path;
            }
            finally { gate.Release(); }
        }

        public async Task<IReadOnlyList<WorktreeSlot>> ListSlotsAsync(string projectPath, CancellationToken ct = default)
        {
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try { return await ReadSlotsUnlockedAsync(projectPath, ct); }
            finally { gate.Release(); }
        }

        public void ReleaseSlot(string worktreePath)
        {
            if (!string.IsNullOrEmpty(worktreePath)) _leases.TryRemove(worktreePath, out _);
        }

        public async Task RemoveSlotAsync(string projectPath, int slotIndex, CancellationToken ct = default)
        {
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var path = Path.Combine(WorktreeRootForProject(projectPath), "slot-" + slotIndex);
                if (IsLeased(path))
                {
                    _logger.LogInformation("[Worktree] posto {Index} in uso da un run: non lo rimuovo ora.", slotIndex);
                    return;
                }
                await RemoveWorktreeUnlockedAsync(projectPath, "slot-" + slotIndex, path, ct);
                _leases.TryRemove(path, out _);
            }
            finally { gate.Release(); }
        }

        public async Task PruneWorktreesAsync(string projectPath, CancellationToken ct = default)
        {
            if (!Directory.Exists(projectPath)) return;
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try { await GitAsync(projectPath, new[] { "worktree", "prune" }, ct); }
            finally { gate.Release(); }
        }

        public async Task<string> MaterializeForReviewAsync(
            string projectPath, string agentName, string localBranch, CancellationToken ct = default)
        {
            var name = ValidateAgentName(agentName);
            if (string.IsNullOrWhiteSpace(localBranch))
                throw new ArgumentException("Branch mancante: non so cosa rimettere sul posto.", nameof(localBranch));

            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var slots = await ReadSlotsUnlockedAsync(projectPath, ct);

                // Già lì e già su quel branch: non c'è niente da rifare.
                var here = slots.FirstOrDefault(x => string.Equals(x.Branch, localBranch, StringComparison.Ordinal));
                if (here != null)
                {
                    _leases[here.Path] = (name, DateTimeOffset.UtcNow);
                    return here.Path;
                }

                var path = await AcquireSlotUnlockedAsync(projectPath, name, ct);

                // Il posto arriva da un altro lavoro: va ripulito, altrimenti il check-out
                // si porterebbe dietro i file dell'agente precedente.
                await GitAsync(path, new[] { "reset", "--hard" }, ct);
                await GitAsync(path, new[] { "clean", "-fd" }, ct);

                var (cc, _, ce) = await GitAsync(path, new[] { "checkout", localBranch }, ct);
                if (cc != 0)
                    throw new InvalidOperationException(
                        $"Non riesco a rimettere il branch '{localBranch}' sul posto {path}: {Describe(cc, ce)}. " +
                        "Se il branch risulta già in uso, un altro posto lo sta tenendo aperto.");

                var subProblem = await SyncSubmodulesAsync(path, ct);
                if (subProblem != null)
                    throw new InvalidOperationException(subProblem);
                return path;
            }
            finally { gate.Release(); }
        }

        public async Task<WorktreePrepareResult> PrepareForRunAsync(
            string projectPath, string agentName, string activityId,
            string baseBranch = null, string handoffRef = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(activityId))
                return WorktreePrepareResult.Fail("activityId mancante: impossibile nominare il branch d'attività.");

            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                // RETE SOTTO LA RETE: con una sessione d'intervento aperta l'agente e' gia' in
                // coda e non dovrebbe arrivare fin qui. Se ci arriva lo stesso — coda aggirata,
                // run forzato — qui sotto ci sono 'reset --hard' e 'clean -fd', che
                // cancellerebbero il lavoro non committato di una persona senza chiedere e senza
                // recupero. Meglio un run rifiutato che lavoro umano distrutto.
                if (_hold.IsHeld(projectPath, agentName))
                {
                    var why = _hold.ReasonFor(projectPath, agentName);
                    return WorktreePrepareResult.Fail(
                        $"Sessione d'intervento aperta sul worktree di '{agentName}' ({why}): mi rifiuto di ripulirlo. " +
                        "Chiudila dalla vista di revisione — concludendo o annullando — e l'agente riprende.");
                }

                var worktreePath = await AcquireSlotUnlockedAsync(projectPath, agentName, ct);

                // 1) fetch origin
                var (fc, _, fe) = await GitAsync(worktreePath, new[] { "fetch", "origin" }, ct);
                if (fc != 0) return WorktreePrepareResult.Fail($"git fetch origin fallito: {Describe(fc, fe)}");

                // baseBranch: se non dato, risolvi il default del remote (origin/HEAD), fallback main.
                var resolvedBase = string.IsNullOrWhiteSpace(baseBranch) ? await ResolveDefaultBranchAsync(worktreePath, ct) : baseBranch.Trim();

                // 2) reset --hard + clean -fd (MAI -x: gli untracked ignorati restano)
                var (rc, _, re) = await GitAsync(worktreePath, new[] { "reset", "--hard" }, ct);
                if (rc != 0) return WorktreePrepareResult.Fail($"git reset --hard fallito: {Describe(rc, re)}");
                await GitAsync(worktreePath, new[] { "clean", "-fd" }, ct);   // best-effort: pulizia scratch

                // 3) branch fresco per-attività da origin/<base>
                var branch = $"agent/{agentName}/{activityId}";
                var (cc, _, ce) = await GitAsync(worktreePath, new[] { "checkout", "-B", branch, "origin/" + resolvedBase }, ct);
                if (cc != 0) return WorktreePrepareResult.Fail($"git checkout -B '{branch}' da 'origin/{resolvedBase}' fallito: {Describe(cc, ce)}");

                // 4) merge dell'handoff (ref COMPLETO): sync fallita (ref assente) → git-sync-failed;
                //    conflitto di merge → merge-conflict-with-main. NON auto-risolvere (7d.5).
                if (!string.IsNullOrWhiteSpace(handoffRef))
                {
                    var remoteRef = "origin/" + handoffRef.Trim();
                    // Il ref di handoff deve esistere sul remote (dopo il fetch): se manca, la sync
                    // al lavoro di A è fallita a monte (branch non pushato/ref sbagliato).
                    var (vc, _, _) = await GitAsync(worktreePath, new[] { "rev-parse", "--verify", "--quiet", remoteRef + "^{commit}" }, ct);
                    if (vc != 0)
                    {
                        _logger.LogWarning("[Worktree] ref di handoff '{Ref}' assente sul remote per '{Agent}': git-sync-failed.", remoteRef, agentName);
                        return WorktreePrepareResult.SyncFail(worktreePath);
                    }
                    var (mc, _, me) = await GitAsync(worktreePath, new[] { "merge", remoteRef }, ct);
                    if (mc != 0)
                    {
                        await GitAsync(worktreePath, new[] { "merge", "--abort" }, ct);   // ripristina lo stato pulito
                        _logger.LogWarning("[Worktree] merge di '{Ref}' in conflitto per '{Agent}': not-ready. {Err}", remoteRef, agentName, me);
                        return WorktreePrepareResult.Conflict(worktreePath);
                    }
                }

                // 5) SUBMODULE: devono seguire il padre, non arrangiarsi.
                //
                // Un worktree fresco non li popola, e il 'reset --hard' del padre NON entra
                // dentro di loro: senza i passi qui sotto un agente si troverebbe il codice al
                // commit di un run precedente — o modificato da un altro agente — mentre la
                // documentazione e' a quello nuovo. Lavorerebbe su una realta' che non esiste.
                {
                    var problem = await SyncSubmodulesAsync(worktreePath, ct);
                    if (problem != null) return WorktreePrepareResult.Fail(problem);
                }

                _logger.LogInformation("[Worktree] preparato per '{Agent}': branch '{Branch}' da origin/{Base}", agentName, branch, resolvedBase);
                return WorktreePrepareResult.Ok(worktreePath);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return WorktreePrepareResult.Fail(ex.Message);
            }
            finally { gate.Release(); }
        }

        public async Task RemoveWorktreeAsync(string projectPath, string agentName, CancellationToken ct = default)
        {
            var name = ValidateAgentName(agentName);
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var slots = await ReadSlotsUnlockedAsync(projectPath, ct);
                var own = slots.FirstOrDefault(x => string.Equals(x.Agent, name, StringComparison.OrdinalIgnoreCase));
                // Nessun posto occupato da questo agente: non c'è niente da rimuovere. Con il pool
                // è la condizione normale — un agente che non lavora semplicemente non occupa nulla.
                if (own == null) return;
                await RemoveWorktreeUnlockedAsync(projectPath, name, own.Path, ct);
            }
            finally { gate.Release(); }
        }

        private async Task RemoveWorktreeUnlockedAsync(string projectPath, string agentName, string worktreePath, CancellationToken ct)
        {
            if (!Directory.Exists(worktreePath))
                return;

            // Progetto ancora presente → rimozione pulita via git (aggiorna .git/worktrees).
            if (Directory.Exists(projectPath))
            {
                var (code, _, err) = await GitAsync(projectPath, new[] { "worktree", "remove", "--force", worktreePath }, ct);
                await GitAsync(projectPath, new[] { "worktree", "prune" }, ct);
                if (code == 0)
                {
                    _logger.LogInformation("[Worktree] rimosso per '{Agent}': {Path}", agentName, worktreePath);
                    return;
                }
                _logger.LogWarning("[Worktree] 'git worktree remove' fallito per '{Agent}' ({Err}): elimino la cartella direttamente.", agentName, err);
            }

            // Progetto sparito o remove fallito: elimina la cartella best-effort.
            try { Directory.Delete(worktreePath, recursive: true); }
            catch (Exception ex) { _logger.LogWarning(ex, "[Worktree] eliminazione cartella '{Path}' fallita.", worktreePath); }
        }

        public async Task<HandoffPushResult> CommitAndPushBranchAsync(string projectPath, string agentName, string commitMessage, CancellationToken ct = default)
        {
            var worktreePath = await FindAgentWorktreeAsync(projectPath, agentName, ct);
            if (worktreePath == null || !Directory.Exists(worktreePath))
                return null;

            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var identity = AgentGitIdentity.EnvFor(agentName);
                await GitAsync(worktreePath, new[] { "add", "-A" }, ct);

                // Distinguo "nulla da committare" (ok, si prosegue) da un commit FALLITO (index.lock,
                // hook, stato non risolto): non basta ignorare l'exit code, altrimenti pubblicheremmo
                // un branch SENZA il lavoro dell'agente (fallback silenzioso vietato, REGOLA #2).
                var (stc, statusOut, _) = await GitAsync(worktreePath, new[] { "status", "--porcelain" }, ct);
                if (stc == 0 && !string.IsNullOrWhiteSpace(statusOut))
                {
                    var msg = string.IsNullOrWhiteSpace(commitMessage) ? "agent deliverable" : commitMessage;
                    var (cc, _, ce) = await GitAsync(worktreePath, new[] { "commit", "-m", msg }, ct, identity);
                    if (cc != 0)
                    {
                        _logger.LogError("[Worktree] commit del deliverable di '{Agent}' FALLITO ({Err}): handoff NON pubblicato per non perdere il lavoro dell'agente.", agentName, Describe(cc, ce));
                        return null;
                    }
                }

                var (bc, branchOut, _) = await GitAsync(worktreePath, new[] { "rev-parse", "--abbrev-ref", "HEAD" }, ct);
                var (hc, shaOut, _) = await GitAsync(worktreePath, new[] { "rev-parse", "HEAD" }, ct);
                if (bc != 0 || hc != 0)
                    return null;
                var branch = branchOut.Trim();
                var headSha = shaOut.Trim();
                if (string.IsNullOrWhiteSpace(branch) || branch == "HEAD")
                    return null;   // detached: nessun branch d'attività da pushare

                // NOME PUBBLICATO: si decide QUI, non alla creazione del branch. È l'unico
                // istante in cui l'esito è noto — e non è troppo tardi, perché il push usa una
                // refspec e i due lati sono indipendenti. L'etichetta viene dai file
                // effettivamente toccati: un fatto, non un'interpretazione.
                var published = await ComposePublishedBranchAsync(projectPath, worktreePath, agentName, branch, ct);

                // Push per REFSPEC (PushAsync di LibGit2Sharp è upstream-only): pubblica il branch
                // locale sotto il nome parlante.
                var (pc, _, pe) = await GitAsync(worktreePath, new[] { "push", "origin", "--force-with-lease", branch + ":refs/heads/" + published }, ct);
                if (pc != 0)
                {
                    _logger.LogWarning("[Worktree] push del branch '{Branch}' per '{Agent}' fallito: {Err}", published, agentName, Describe(pc, pe));
                    return null;
                }

                _logger.LogInformation("[Worktree] deliverable di '{Agent}' pubblicato: {Branch}@{Sha} (locale: {Local})",
                    agentName, published, headSha, branch);

                // Il ref di handoff spedito al collega DEVE essere quello pubblicato: il locale
                // non esiste su origin, e il peer farebbe 'merge origin/<locale>' senza trovarlo.
                return new HandoffPushResult { Branch = published, LocalBranch = branch, HeadSha = headSha };
            }
            finally { gate.Release(); }
        }

        /// <summary>
        /// Compone il nome pubblicato dai file toccati rispetto al default del remote.
        /// Fail-soft: se il diff non è calcolabile si ripiega sull'etichetta generica — un nome
        /// meno parlante è meglio di un deliverable non pubblicato.
        /// </summary>
        private async Task<string> ComposePublishedBranchAsync(
            string projectPath, string worktreePath, string agentName, string localBranch, CancellationToken ct)
        {
            var changed = new List<string>();
            try
            {
                var baseBranch = await ResolveDefaultBranchAsync(worktreePath, ct);
                // Tre punti: differenza rispetto al punto in cui il ramo si è separato, non
                // rispetto allo stato attuale del default (che nel frattempo può essere avanzato).
                var (dc, dout, _) = await GitAsync(worktreePath,
                    new[] { "diff", "--name-only", $"origin/{baseBranch}...HEAD" }, ct);
                if (dc == 0 && !string.IsNullOrWhiteSpace(dout))
                    changed.AddRange(dout.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Worktree] diff per il nome del branch non calcolabile: uso l'etichetta generica.");
            }

            // L'activityId è l'ultimo segmento del nome locale (agent/<agente>/<activityId>).
            var activityId = localBranch?.Split('/').LastOrDefault();

            var ownerEmail = _ownerIdentity?.ResolveEmail(projectPath);

            return AgentBranchNaming.ComposePublishedBranch(
                ownerEmail, agentName, changed, activityId, DateTime.UtcNow);
        }

        /// <summary>
        /// File toccati dal lavoro dell'agente rispetto al punto in cui il ramo si e' separato
        /// dal default. E' lo stesso dato da cui si ricava l'etichetta del branch: qui serve
        /// all'umano per capire cosa sta autorizzando.
        /// </summary>
        public async Task<IReadOnlyList<ChangedFile>> ChangedFilesAsync(
            string projectPath, string agentName, CancellationToken ct = default)
        {
            var worktreePath = await FindAgentWorktreeAsync(projectPath, agentName, ct);
            if (worktreePath == null || !Directory.Exists(worktreePath)) return System.Array.Empty<ChangedFile>();

            try
            {
                var baseBranch = await ResolveDefaultBranchAsync(worktreePath, ct);
                var (dc, dout, _) = await GitAsync(worktreePath,
                    new[] { "diff", "--name-status", $"origin/{baseBranch}...HEAD" }, ct);
                if (dc != 0) return System.Array.Empty<ChangedFile>();
                return AgentMergeRequestService.ParseNameStatus(dout);
            }
            catch (System.Exception ex)
            {
                _logger.LogWarning(ex, "[Worktree] elenco file toccati non calcolabile per '{Agent}'.", agentName);
                return System.Array.Empty<ChangedFile>();
            }
        }

        public async Task<bool> MergeBranchAsync(string projectPath, string sourceRef, string intoBranch, CancellationToken ct = default)
        {
            // ⚠️ Muta la working tree del repo (checkout + merge): metodo DORMIENTE in 7d, lo
            // innescherà 7g (gate del merge) con le sue guardie. Non è chiamato automaticamente ora.
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var (cc, _, ce) = await GitAsync(projectPath, new[] { "checkout", intoBranch }, ct);
                if (cc != 0) { _logger.LogWarning("[Worktree] merge: checkout '{Into}' fallito: {Err}", intoBranch, ce); return false; }
                var (mc, _, me) = await GitAsync(projectPath, new[] { "merge", sourceRef }, ct);
                if (mc != 0)
                {
                    await GitAsync(projectPath, new[] { "merge", "--abort" }, ct);
                    _logger.LogWarning("[Worktree] merge di '{Src}' in '{Into}' fallito: {Err}", sourceRef, intoBranch, me);
                    return false;
                }
                return true;
            }
            finally { gate.Release(); }
        }

        public async Task<DeliverableMergeOutcome> MergeDeliverableIntoDefaultAsync(string projectPath, string agentName, string activityBranch, CancellationToken ct = default)
        {
            var worktreePath = await FindAgentWorktreeAsync(projectPath, agentName, ct);
            if (worktreePath == null || !Directory.Exists(worktreePath) || string.IsNullOrWhiteSpace(activityBranch))
                return DeliverableMergeOutcome.Failed;

            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var (fc, _, _) = await GitAsync(worktreePath, new[] { "fetch", "origin" }, ct);
                if (fc != 0) return DeliverableMergeOutcome.Failed;

                var def = await ResolveDefaultBranchAsync(worktreePath, ct);

                // Detached su origin/<default>: evita "branch già checked-out" (il main è nel progetto umano).
                var (cc, _, ce) = await GitAsync(worktreePath, new[] { "checkout", "--detach", "origin/" + def }, ct);
                if (cc != 0) { _logger.LogWarning("[Worktree] auto-merge: checkout detached su origin/{Def} fallito: {Err}", def, ce); return DeliverableMergeOutcome.Failed; }

                var (mc, _, me) = await GitAsync(worktreePath, new[] { "merge", "--no-edit", activityBranch }, ct);
                if (mc != 0)
                {
                    await GitAsync(worktreePath, new[] { "merge", "--abort" }, ct);
                    _logger.LogWarning("[Worktree] auto-merge di '{Branch}' in '{Def}' in conflitto: not-ready. {Err}", activityBranch, def, me);
                    return DeliverableMergeOutcome.Conflict;
                }

                // Push del merge sul default di origin (non-fast-forward → qualcun altro ha spinto: ritentabile).
                var (pc, _, pe) = await GitAsync(worktreePath, new[] { "push", "origin", "HEAD:refs/heads/" + def }, ct);
                if (pc != 0) { _logger.LogWarning("[Worktree] auto-merge: push di '{Def}' fallito (non-fast-forward?): {Err}", def, pe); return DeliverableMergeOutcome.Failed; }

                _logger.LogInformation("[Worktree] deliverable di '{Agent}' auto-merge in '{Def}' e pushato.", agentName, def);
                return DeliverableMergeOutcome.Merged;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "[Worktree] auto-merge fallito per '{Agent}'", agentName);
                return DeliverableMergeOutcome.Failed;
            }
            finally { gate.Release(); }
        }

        public async Task DeleteBranchAsync(string projectPath, string branch, bool remoteToo, CancellationToken ct = default)
        {
            var gate = _repoGates.GetOrAdd(RepoKey(projectPath), _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                var (lc, _, le) = await GitAsync(projectPath, new[] { "branch", "-D", branch }, ct);
                if (lc != 0) _logger.LogWarning("[Worktree] cancellazione branch locale '{Branch}' fallita: {Err}", branch, le);
                if (remoteToo)
                {
                    var (rc, _, re) = await GitAsync(projectPath, new[] { "push", "origin", "--delete", branch }, ct);
                    if (rc != 0) _logger.LogWarning("[Worktree] cancellazione branch remoto '{Branch}' fallita: {Err}", branch, re);
                }
            }
            finally { gate.Release(); }
        }

        public async Task<IReadOnlyList<string>> ListMergedAgentBranchesAsync(string projectPath, string intoBranch, CancellationToken ct = default)
        {
            var into = string.IsNullOrWhiteSpace(intoBranch) ? await ResolveDefaultBranchAsync(projectPath, ct) : intoBranch.Trim();
            var (code, outp, _) = await GitAsync(projectPath, new[] { "branch", "--merged", into }, ct);
            if (code != 0) return System.Array.Empty<string>();
            var result = new List<string>();
            foreach (var raw in outp.Split('\n'))
            {
                var line = raw.TrimStart('*', ' ', '+').Trim();   // '*' = branch corrente, '+' = in un worktree
                if (line.StartsWith("agent/", System.StringComparison.Ordinal))
                    result.Add(line);
            }
            return result;
        }

        public async Task<IReadOnlyList<string>> GetDirtySubmodulesAsync(string worktreePath, CancellationToken ct = default)
        {
            var submodulePaths = ReadSubmodulePaths(worktreePath);
            if (submodulePaths.Count == 0)
                return System.Array.Empty<string>();

            var (code, outp, _) = await GitAsync(worktreePath, new[] { "status", "--porcelain", "--ignore-submodules=none" }, ct);
            if (code != 0)
                return System.Array.Empty<string>();

            var dirty = new List<string>();
            foreach (var raw in outp.Split('\n'))
            {
                if (raw.Length < 4) continue;
                var status = raw.Substring(0, 2);
                if (status == "  ") continue;                 // pulito
                var path = raw.Substring(3).Trim().Trim('"'); // porcelain: 2 char stato + spazio + path
                var slash = path.IndexOf(" -> ", System.StringComparison.Ordinal);
                if (slash >= 0) path = path.Substring(slash + 4);   // rename: prendi la destinazione
                path = path.TrimEnd('/');
                if (submodulePaths.Contains(path) && !dirty.Contains(path))
                    dirty.Add(path);
            }
            return dirty;
        }

        /// <summary>Legge i path dei submodule dal <c>.gitmodules</c> del worktree (normalizzati a '/').</summary>
        private static HashSet<string> ReadSubmodulePaths(string worktreePath)
        {
            var set = new HashSet<string>(System.StringComparer.Ordinal);
            var gm = Path.Combine(worktreePath, ".gitmodules");
            if (!File.Exists(gm)) return set;
            try
            {
                foreach (var line in File.ReadAllLines(gm))
                {
                    var t = line.Trim();
                    if (!t.StartsWith("path", System.StringComparison.OrdinalIgnoreCase)) continue;
                    var eq = t.IndexOf('=');
                    if (eq < 0) continue;
                    var p = t.Substring(eq + 1).Trim().Replace('\\', '/').TrimEnd('/');
                    if (p.Length > 0) set.Add(p);
                }
            }
            catch { /* best-effort */ }
            return set;
        }

        private async Task<string> ResolveDefaultBranchAsync(string worktreePath, CancellationToken ct)
        {
            // origin/HEAD → refs/remotes/origin/<default>. Fallback 'main' se non risolvibile.
            var (code, outp, _) = await GitAsync(worktreePath, new[] { "symbolic-ref", "--short", "refs/remotes/origin/HEAD" }, ct);
            if (code == 0 && !string.IsNullOrWhiteSpace(outp))
            {
                var name = outp.Trim();
                var slash = name.IndexOf('/');
                return slash >= 0 ? name.Substring(slash + 1) : name;   // "origin/main" → "main"
            }
            _logger.LogWarning("[Worktree] default branch del remote non risolvibile in '{Path}': uso 'main'.", worktreePath);
            return "main";
        }

        private static string RepoKey(string projectPath) => Helper.HGetHashString(projectPath ?? string.Empty);

        private static string Describe(int code, string stderr)
            => code == GitNotFoundExit ? "git non trovato nel PATH"
             : code == GitTimeoutExit ? "timeout"
             : $"exit {code}: {stderr?.Trim()}";

        /// <summary>
        /// Esegue git nativo (LibGit2Sharp non fa worktree). Gli argomenti passano come
        /// <see cref="ProcessStartInfo.ArgumentList"/>: ogni token è un argv <b>letterale</b>, quindi
        /// nessun quoting e <b>nessuna injection</b> possibile (anche da ref/branch controllati da
        /// una città peer). <c>GIT_TERMINAL_PROMPT=0</c> = fail-fast senza hang.
        /// </summary>
        private async Task<(int ExitCode, string Stdout, string Stderr)> GitAsync(
            string workingDirectory, string[] args, CancellationToken ct,
            IReadOnlyDictionary<string, string> extraEnv = null)
        {
            var pretty = string.Join(" ", args);
            _logger.LogDebug("[Worktree] git -C {Dir}: git {Args}", workingDirectory, pretty);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };
            foreach (var a in args)
                process.StartInfo.ArgumentList.Add(a);
            process.StartInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            if (extraEnv != null)
                foreach (var kv in extraEnv)
                    process.StartInfo.EnvironmentVariables[kv.Key] = kv.Value;

            try { process.Start(); }
            catch (System.ComponentModel.Win32Exception ex)
            {
                _logger.LogError(ex, "[Worktree] git non trovato nel PATH");
                return (GitNotFoundExit, string.Empty, ex.Message);
            }

            // Leggere PRIMA di attendere: leggere dopo WaitForExit può deadlockare col buffer pieno.
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();
            var completed = await Task.Run(() => process.WaitForExit(DefaultTimeoutMs), ct);
            if (!completed)
            {
                try { process.Kill(true); } catch { }
                _logger.LogError("[Worktree] git {Args} in timeout ({Ms}ms)", pretty, DefaultTimeoutMs);
                return (GitTimeoutExit, string.Empty, "timeout");
            }

            return (process.ExitCode, await stdoutTask, await stderrTask);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var g in _repoGates.Values)
                g.Dispose();
            _repoGates.Clear();
        }
    }
}
