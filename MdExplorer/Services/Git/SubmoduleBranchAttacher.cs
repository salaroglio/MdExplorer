using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    public interface ISubmoduleBranchAttacher
    {
        /// <summary>Rimette i submodule sul loro ramo, dove si puo' farlo senza spostare un file.</summary>
        Task AttachAsync(string repositoryPath, CancellationToken ct = default);
    }

    public sealed class SubmoduleBranchAttacher : ISubmoduleBranchAttacher
    {
        private readonly INativeGitRunner _git;
        private readonly ILogger<SubmoduleBranchAttacher> _logger;

        public SubmoduleBranchAttacher(INativeGitRunner git, ILogger<SubmoduleBranchAttacher> logger)
        {
            _git = git;
            _logger = logger;
        }

        private async Task<(int ExitCode, string Stdout, string Stderr)> Run(
            string dir, string arguments, CancellationToken ct)
        {
            var res = await _git.RunAsync(dir, SplitArgs(arguments), ct);
            return (res.ExitCode, res.Stdout, res.Stderr);
        }

        /// <summary>
        /// Gli argomenti passano come vettore, non come riga: cosi' un percorso o un nome di ramo
        /// con spazi non va a pezzi, e non serve mettere virgolette che poi git si porta dietro.
        /// </summary>
        private static string[] SplitArgs(string arguments)
        {
            var parts = new List<string>();
            var current = new System.Text.StringBuilder();
            var quoted = false;
            foreach (var c in arguments)
            {
                if (c == '"') { quoted = !quoted; continue; }
                if (c == ' ' && !quoted)
                {
                    if (current.Length > 0) { parts.Add(current.ToString()); current.Clear(); }
                    continue;
                }
                current.Append(c);
            }
            if (current.Length > 0) parts.Add(current.ToString());
            return parts.ToArray();
        }

        /// <summary>
        /// Rimette ogni submodule sul suo ramo, quando si può farlo senza spostare un file.
        /// <para>
        /// <c>git submodule update</c> mette il submodule <b>sul commit registrato</b>, che è un
        /// commit e non un ramo: il risultato è <b>HEAD staccato</b> (verificato — un submodule che
        /// stava su <c>main</c> ci finisce dopo ogni pull). Da lì non si può più committare senza
        /// produrre un commit orfano, e prima della vista per repository la cosa era semplicemente
        /// invisibile.
        /// </para>
        /// <para>
        /// Il riaggancio avviene <b>solo se un ramo punta esattamente a quel commit</b>: allora il
        /// checkout non cambia un byte del contenuto, cambia solo dove HEAD è appoggiato. Se
        /// nessun ramo ci punta, o se ce ne punta più d'uno, <b>si lascia staccato</b>: agganciare
        /// un ramo che sta altrove sposterebbe i file, e indovinare fra due rami sarebbe una
        /// scelta presa al posto di chi lavora.
        /// </para>
        /// <para>
        /// Non tocca i posti di lavoro degli agenti, che passano da un'altra strada
        /// (<c>AgentWorktreeManager.SyncSubmodulesAsync</c>) e devono restare staccati: sul codice
        /// il git è in mano all'umano.
        /// </para>
        /// </summary>
        public async Task AttachAsync(string repositoryPath, CancellationToken ct = default)
        {
            try
            {
                var list = await Run(repositoryPath, "submodule status --recursive", ct);
                if (list.ExitCode != 0) return;

                foreach (var line in (list.Stdout ?? string.Empty)
                             .Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    var row = line.TrimEnd('\r');
                    if (row.Length < 3) continue;
                    if (row[0] == '-') continue;                      // non popolato: non c'è niente da agganciare

                    var rest = row.Substring(1);
                    var space = rest.IndexOf(' ');
                    if (space <= 0) continue;
                    var path = rest.Substring(space + 1).Trim();
                    if (path.EndsWith(")", StringComparison.Ordinal))
                    {
                        var open = path.LastIndexOf(" (", StringComparison.Ordinal);
                        if (open > 0) path = path.Substring(0, open).Trim();
                    }
                    if (path.Length == 0) continue;

                    var dir = Path.Combine(repositoryPath, path.Replace('/', Path.DirectorySeparatorChar));
                    if (!Directory.Exists(dir)) continue;

                    var head = await Run(dir, "rev-parse --abbrev-ref HEAD", ct);
                    if (head.ExitCode != 0) continue;
                    if ((head.Stdout ?? string.Empty).Trim() != "HEAD") continue;   // già su un ramo

                    var sha = await Run(dir, "rev-parse HEAD", ct);
                    if (sha.ExitCode != 0) continue;
                    var commit = (sha.Stdout ?? string.Empty).Trim();
                    if (commit.Length == 0) continue;

                    var branches = await BranchesSafeToAttachAsync(dir, commit, ct);
                    if (branches.Count == 0)
                    {
                        // Nessun ramo locale: e' un submodule appena popolato, che 'submodule
                        // update' non dota mai di rami. Se un solo ramo REMOTO sta esattamente
                        // qui, lo si crea in locale — vedi il metodo per le condizioni.
                        if (await TryCreateTrackingBranchAsync(dir, commit, path, ct)) continue;
                    }

                    if (branches.Count != 1)
                    {
                        _logger.LogInformation(
                            "[Submodule] '{Path}' resta staccato: {Quanti} rami candidati per {Commit}.",
                            path, branches.Count, commit.Substring(0, Math.Min(8, commit.Length)));
                        continue;
                    }

                    // '-B' porta il ramo al commit e ci si mette sopra. È un avanzamento, non uno
                    // spostamento: il commit registrato è già in checkout, quindi non cambia un file.
                    var checkout = await Run(dir, $"checkout -B \"{branches[0]}\" {commit}", ct);
                    if (checkout.ExitCode == 0)
                        _logger.LogInformation("[Submodule] '{Path}' rimesso sul ramo '{Ramo}'.", path, branches[0]);
                    else
                        _logger.LogWarning("[Submodule] '{Path}' non rimesso su '{Ramo}': {Err}",
                            path, branches[0], checkout.Stderr);
                }
            }
            catch (Exception ex)
            {
                // Un riaggancio non riuscito non invalida il pull: i file sono già quelli giusti,
                // manca solo la comodità di poter committare. Si dice e si va avanti.
                _logger.LogWarning(ex, "[Submodule] riaggancio dei rami non riuscito in '{Path}'.", repositoryPath);
            }
        }

        /// <summary>
        /// Crea il ramo locale mancante e ci mette sopra HEAD, quando è <b>l'unica lettura
        /// possibile</b> della situazione.
        /// <para>
        /// È il caso di ogni submodule appena scaricato: <c>git submodule update</c> lo mette sul
        /// commit registrato e non gli dà nessun ramo, quindi resta inservibile finché qualcuno non
        /// interviene a mano.
        /// </para>
        /// <para>Si agisce solo se valgono <b>tutte e quattro</b>:</para>
        /// <list type="number">
        /// <item>HEAD è staccato e non esiste nessun ramo locale candidato (garantito dal chiamante);</item>
        /// <item>esiste <b>un solo</b> riferimento remoto che punta esattamente al commit registrato —
        /// i riferimenti simbolici come <c>origin/HEAD</c> sono esclusi, altrimenti conterebbero
        /// doppio insieme al ramo che rappresentano, e due candidati fanno rinunciare;</item>
        /// <item>quel riferimento appartiene a un remoto, e il nome del ramo si ricava togliendo il
        /// nome del remoto — non si inventa;</item>
        /// <item><b>non esiste già</b> un ramo locale con quel nome. Se esiste e sta altrove, è
        /// lavoro di qualcuno: si usa <c>-b</c>, che fallisce se il ramo c'è, mai <c>-B</c>, che lo
        /// sposterebbe.</item>
        /// </list>
        /// <para>
        /// Con queste condizioni il checkout è sul commit dove HEAD è già: <b>non sposta un file</b>
        /// e non tocca eventuali modifiche non salvate.
        /// </para>
        /// </summary>
        private async Task<bool> TryCreateTrackingBranchAsync(string dir, string commit, string path, CancellationToken ct)
        {
            var refs = await Run(dir,
                $"for-each-ref --points-at {commit} --format=%(refname:short)%09%(symref) refs/remotes/", ct);
            if (refs.ExitCode != 0) return false;

            var candidates = new List<string>();
            foreach (var row in (refs.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = row.TrimEnd('\r').Split('\t');
                var name = parts[0].Trim();
                var symref = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                if (name.Length == 0) continue;
                if (symref.Length > 0) continue;          // 'origin/HEAD' rappresenta un altro ramo: conterebbe doppio
                candidates.Add(name);
            }

            if (candidates.Count != 1)
            {
                _logger.LogInformation(
                    "[Submodule] '{Path}' resta staccato: {Quanti} rami remoti puntano a {Commit}.",
                    path, candidates.Count, commit.Substring(0, Math.Min(8, commit.Length)));
                return false;
            }

            var remoteRef = candidates[0];
            var slash = remoteRef.IndexOf('/');
            if (slash <= 0 || slash == remoteRef.Length - 1) return false;
            var branch = remoteRef.Substring(slash + 1);   // 'origin/feature/x' → 'feature/x'

            var exists = await Run(dir, $"show-ref --verify --quiet refs/heads/{branch}", ct);
            if (exists.ExitCode == 0)
            {
                // C'e' gia' un ramo con quel nome che sta da un'altra parte: e' lavoro di qualcuno.
                _logger.LogInformation(
                    "[Submodule] '{Path}' resta staccato: il ramo '{Ramo}' esiste gia' e sta altrove.", path, branch);
                return false;
            }

            var created = await Run(dir, $"checkout -b \"{branch}\" --track \"{remoteRef}\"", ct);
            if (created.ExitCode != 0)
            {
                _logger.LogWarning("[Submodule] '{Path}': ramo '{Ramo}' non creato: {Err}", path, branch, created.Stderr);
                return false;
            }

            _logger.LogInformation("[Submodule] '{Path}': creato il ramo '{Ramo}' che segue '{Remoto}'.",
                path, branch, remoteRef);
            return true;
        }

        /// <summary>
        /// I rami locali su cui si può rimettere HEAD <b>senza spostare un file</b>.
        /// <para>Due casi, e nessun altro:</para>
        /// <list type="number">
        /// <item>il ramo <b>punta esattamente</b> al commit registrato: il checkout non cambia nulla;</item>
        /// <item>il ramo è un <b>antenato</b> del commit registrato e il suo ramo remoto sta
        /// proprio lì: è il caso normale dopo un pull — il commit è già arrivato, il ramo locale
        /// deve solo raggiungerlo, e portarcelo è un avanzamento, non una scelta.</item>
        /// </list>
        /// <para>
        /// Fuori da questi due si lascia staccato: agganciare un ramo che sta altrove
        /// sposterebbe davvero i file.
        /// </para>
        /// </summary>
        private async Task<List<string>> BranchesSafeToAttachAsync(string dir, string commit, CancellationToken ct)
        {
            var found = new List<string>();

            var exact = await Run(dir,
                $"for-each-ref --format=%(refname:short) --points-at {commit} refs/heads/", ct);
            if (exact.ExitCode == 0)
                found.AddRange((exact.Stdout ?? string.Empty)
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).Where(x => x.Length > 0));

            if (found.Count > 0) return found;

            var all = await Run(dir,
                "for-each-ref --format=%(refname:short)%09%(upstream:short) refs/heads/", ct);
            if (all.ExitCode != 0) return found;

            foreach (var row in (all.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = row.TrimEnd('\r').Split('\t');
                if (parts.Length < 2) continue;
                var branch = parts[0].Trim();
                var upstream = parts[1].Trim();
                if (branch.Length == 0 || upstream.Length == 0) continue;

                var tip = await Run(dir, $"rev-parse \"{upstream}\"", ct);
                if (tip.ExitCode != 0 || (tip.Stdout ?? string.Empty).Trim() != commit) continue;

                var ancestor = await Run(dir, $"merge-base --is-ancestor \"{branch}\" {commit}", ct);
                if (ancestor.ExitCode == 0) found.Add(branch);
            }

            return found;
        }
    }
}
