using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Services.AgentRun;
using MdExplorer.Services.Git.Interfaces;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>Un passo del pubblica-tutto: quale repository, com'è andata.</summary>
    public sealed class PushStep
    {
        /// <summary>Vuoto per la radice; per i submodule il percorso relativo.</summary>
        public string Repo { get; init; }
        public string Label { get; init; }
        public bool Ok { get; init; }
        public string Outcome { get; init; }
    }

    public sealed class SafePushResult
    {
        public bool Success { get; init; }

        /// <summary>
        /// Perché non si è nemmeno partiti. <c>null</c> = si è partiti. Rifiutare prima di toccare
        /// qualunque remoto è il punto: dopo, un errore lascerebbe metà lavoro pubblicato.
        /// </summary>
        public string Refused { get; init; }

        public IReadOnlyList<PushStep> Steps { get; init; } = Array.Empty<PushStep>();

        /// <summary>
        /// Cosa <b>non</b> è stato pubblicato perché non committato. Non è un errore — pubblicare
        /// ciò che non è stato salvato non ha senso — ma senza dirlo si crede di aver pubblicato
        /// tutto.
        /// </summary>
        public IReadOnlyList<string> LeftBehind { get; init; } = Array.Empty<string>();
    }

    public interface ISafePushService
    {
        /// <summary>Pubblica il progetto e i suoi submodule senza poter rompere il repository.</summary>
        Task<SafePushResult> PushEverythingAsync(string projectPath, string agentName, CancellationToken ct = default);
    }

    /// <summary>
    /// Il <b>pubblica-tutto</b>, costruito perché non possa produrre un repository rotto.
    /// <para>
    /// Il disastro da evitare è uno solo: il progetto pubblica un riferimento a un commit che sul
    /// remoto del submodule non esiste. Chi clona ottiene un repository rotto, e l'errore lo vede
    /// <b>lui, giorni dopo</b>. La difesa non è un controllo in più: è l'<b>ordine</b>.
    /// </para>
    /// <para>
    /// <b>I figli prima, il padre per ultimo.</b> Qualunque cosa fallisca a monte — credenziali,
    /// rete, un ramo rifiutato — il padre non viene toccato, e il remoto resta <b>vecchio ma
    /// coerente</b>. Mai rotto. È l'invariante su cui poggia la promessa, non un caso fortunato.
    /// </para>
    /// <para>
    /// git offre <c>push --recurse-submodules=on-demand</c>, che fa esattamente questo (verificato
    /// in sandbox il 18/08). <b>Non lo usiamo</b>: l'ordine lo imponiamo noi, un repository alla
    /// volta, così ogni passo ha il suo esito e ci si ferma al primo che non riesce. (Quando è
    /// nato, il motivo era un altro: le credenziali erano cablate in LibGit2Sharp. Dal 22/09/2026
    /// il push passa da git nativo.)
    /// </para>
    /// </summary>
    public sealed class SafePushService : ISafePushService
    {
        private readonly IWorkingChangesService _changes;
        private readonly IModernGitService _git;
        private readonly ILogger<SafePushService> _logger;

        public SafePushService(
            IWorkingChangesService changes,
            IModernGitService git,
            ILogger<SafePushService> logger)
        {
            _changes = changes;
            _git = git;
            _logger = logger;
        }

        public async Task<SafePushResult> PushEverythingAsync(
            string projectPath, string agentName, CancellationToken ct = default)
        {
            var view = await _changes.GetAsync(projectPath, agentName, ct);
            if (view.Problem != null) return Refuse(view.Problem);
            if (view.NotAGitRepository) return Refuse("Questa cartella non è un repository git.");
            if (view.Repos == null || view.Repos.Count == 0) return Refuse("Nessun repository da pubblicare.");

            var root = view.Repos[0];
            var submodules = view.Repos.Skip(1).ToList();

            // ---- 1) si controlla PRIMA, e si rifiuta prima di toccare qualsiasi remoto ----
            foreach (var s in submodules)
            {
                // Non sapere se un riferimento e' pubblicato non e' «va bene»: si rifiuta.
                if (s.RecordedCommitUnknown != null) return Refuse(s.RecordedCommitUnknown);
            }

            foreach (var s in submodules.Where(NeedsPublishing))
            {
                if (s.NotInitialized)
                    return Refuse($"'{s.Path}' non è popolato, ma il progetto ne registra un commit diverso: " +
                                  "va scaricato prima di poter pubblicare.");

                if (s.Detached)
                    return Refuse($"'{s.Path}' ha HEAD staccato: il suo lavoro non sta su nessun ramo e non è " +
                                  "pubblicabile. Mettilo su un ramo, poi ripeti.");

                if (string.IsNullOrEmpty(s.Upstream))
                    return Refuse($"'{s.Path}' non ha un ramo remoto configurato: il suo lavoro non è pubblicabile " +
                                  "così com'è.");
            }

            if (root.Ahead > 0)
            {
                if (root.Detached)
                    return Refuse("Il progetto ha HEAD staccato: non c'è un ramo da pubblicare.");
                if (string.IsNullOrEmpty(root.Upstream))
                    return Refuse("Il progetto non ha un ramo remoto configurato: non c'è dove pubblicare.");
            }

            // Prima si scarica, poi si pubblica: un repository che è anche indietro verrebbe rifiutato
            // dal suo remoto a metà del giro. Meglio dirlo prima di toccarne uno.
            foreach (var r in view.Repos)
            {
                if (r.MergeInProgress)
                    return Refuse($"'{r.Label}' ha un'unione rimasta a metà: va conclusa con un commit, oppure annullata, prima di pubblicare.");
                if (r.Ahead > 0 && r.Behind > 0)
                    return Refuse($"'{r.Label}' ha {r.Behind} commit da scaricare: prima scarica, poi pubblica.");
            }

            var steps = new List<PushStep>();
            // Solo cio' che il push NON porta via: i file non committati e i puntatori non registrati nel
            // progetto. Prima si contava Files, che include i commit che questo stesso push sta per
            // pubblicare: dopo un commit il messaggio diceva «resta indietro» di file appena pubblicati.
            var leftBehind = view.Repos
                .Where(r => r.Uncommitted.Count > 0)
                .Select(r => $"{r.Uncommitted.Count} file non committati in '{(string.IsNullOrEmpty(r.Path) ? r.Label : r.Path)}'")
                // Solo se il submodule e' piu' AVANTI: uno rimasto indietro va allineato, non registrato.
                .Concat(view.Repos.Where(r => r.PointerMoved && (r.Relation == null || r.Relation == SubmoduleRelation.Ahead))
                    .Select(r => $"il nuovo commit di '{r.Path}' non è ancora registrato nel progetto (va committato lì)"))
                .ToList();

            // ---- 2) i figli, dal più profondo: un submodule dentro un submodule va prima del suo ----
            foreach (var s in submodules.Where(NeedsPublishing)
                                        .OrderByDescending(x => x.Depth)
                                        .ThenBy(x => x.Path, StringComparer.OrdinalIgnoreCase))
            {
                var dir = Path.Combine(view.RootPath, s.Path.Replace('/', Path.DirectorySeparatorChar));
                var res = await _git.PushAsync(dir);
                steps.Add(new PushStep
                {
                    Repo = s.Path, Label = s.Path, Ok = res.Success,
                    Outcome = res.Success ? (res.Message ?? "pubblicato") : (res.ErrorMessage ?? "non riuscito"),
                });

                if (!res.Success)
                {
                    // Qui ci si ferma, e il padre NON si tocca: il remoto resta vecchio ma coerente.
                    _logger.LogWarning("[PubblicaTutto] '{Repo}' non pubblicato: {Why}. Il progetto non viene toccato.",
                        s.Path, res.ErrorMessage);
                    return new SafePushResult { Success = false, Steps = steps, LeftBehind = leftBehind };
                }
            }

            // ---- 3) il padre per ULTIMO ----
            if (root.Ahead > 0)
            {
                var res = await _git.PushAsync(view.RootPath);
                steps.Add(new PushStep
                {
                    Repo = string.Empty, Label = root.Label, Ok = res.Success,
                    Outcome = res.Success ? (res.Message ?? "pubblicato") : (res.ErrorMessage ?? "non riuscito"),
                });
                if (!res.Success)
                    return new SafePushResult { Success = false, Steps = steps, LeftBehind = leftBehind };
            }

            return new SafePushResult { Success = true, Steps = steps, LeftBehind = leftBehind };
        }

        /// <summary>
        /// Va pubblicato se il commit che il progetto registra non è su nessun remoto — il caso che
        /// rompe il repository per gli altri — oppure se ha comunque lavoro non pubblicato.
        /// <para>
        /// Il primo controllo non è ridondante: con HEAD staccato <c>Ahead</c> vale 0 e il puntatore
        /// risulta già committato, quindi gli altri due segnali tacciono proprio nel caso peggiore.
        /// Scoperto da un test che passava a torto.
        /// </para>
        /// </summary>
        private static bool NeedsPublishing(RepoChanges repo)
            => repo.RecordedCommitUnpublished || repo.Ahead > 0 || repo.PointerMoved;

        private static SafePushResult Refuse(string why)
            => new SafePushResult { Success = false, Refused = why };
    }
}
