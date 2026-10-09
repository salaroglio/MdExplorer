using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Com'è il commit in checkout di un submodule rispetto a quello che il progetto registra.
    /// <para>
    /// git dà una sigla sola (<c>SC..</c>, «commit cambiato») per tutti i casi, e non dice in che
    /// verso: è la stessa quando sei tu ad aver portato avanti il submodule e quando un pull del
    /// progetto ha portato una versione più nuova che il submodule non ha ancora raggiunto.
    /// Committare il progetto nel secondo caso registra la versione <b>vecchia</b> (provato in
    /// sandbox l'01/10/2026). Il verso va letto confrontando i due commit.
    /// </para>
    /// </summary>
    public static class SubmoduleRelation
    {
        public const string Same = "same";
        /// <summary>Il submodule è più avanti: c'è una versione nuova da registrare nel progetto.</summary>
        public const string Ahead = "ahead";
        /// <summary>Il submodule è più indietro: va allineato, non committato.</summary>
        public const string Behind = "behind";
        /// <summary>Né avanti né indietro: due lavori da unire, dentro il submodule.</summary>
        public const string Diverged = "diverged";
        /// <summary>Il commit registrato qui non c'è ancora: non è stato scaricato.</summary>
        public const string Unknown = "unknown";

        public static async Task<string> ReadAsync(
            INativeGitRunner git, string submoduleDir, string recorded, string checkedOut, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(recorded) || string.IsNullOrEmpty(checkedOut)) return null;
            if (string.Equals(recorded, checkedOut, StringComparison.OrdinalIgnoreCase)) return Same;

            var have = await git.RunAsync(submoduleDir, new[] { "cat-file", "-e", recorded + "^{commit}" }, ct);
            if (!have.Ok) return Unknown;

            if (await IsAncestorAsync(git, submoduleDir, recorded, checkedOut, ct)) return Ahead;
            if (await IsAncestorAsync(git, submoduleDir, checkedOut, recorded, ct)) return Behind;
            return Diverged;
        }

        public static async Task<bool> IsAncestorAsync(
            INativeGitRunner git, string dir, string ancestor, string descendant, CancellationToken ct)
            => (await git.RunAsync(dir, new[] { "merge-base", "--is-ancestor", ancestor, descendant }, ct)).Ok;
    }

    /// <summary>Una riga di <c>git submodule status</c>.</summary>
    public readonly struct SubmoduleStatusLine
    {
        /// <summary><c>' '</c> allineato, <c>'-'</c> non scaricato, <c>'+'</c> commit diverso, <c>'U'</c> conflitto.</summary>
        public char Prefix { get; init; }
        public string Sha { get; init; }
        public string Path { get; init; }

        public static IEnumerable<SubmoduleStatusLine> Parse(string stdout)
        {
            foreach (var raw in (stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var line = raw.TrimEnd('\r');
                if (line.Length < 3) continue;
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

                yield return new SubmoduleStatusLine
                {
                    Prefix = line[0], Sha = rest.Substring(0, space), Path = path.Replace('\\', '/'),
                };
            }
        }
    }

    public enum SubmoduleAlignMode
    {
        /// <summary>Dopo uno scaricamento: i submodule si spostano <b>solo in avanti</b>.</summary>
        Forward,
        /// <summary>Dopo un cambio di ramo: i submodule vanno al commit che il ramo nuovo registra, anche se è precedente.</summary>
        Exact,
    }

    public sealed class SubmoduleAlignStep
    {
        public const string Populated = "populated";
        public const string Already = "already";
        public const string Aligned = "aligned";
        public const string LeftAhead = "ahead";
        public const string LeftDiverged = "diverged";
        public const string Failed = "failed";

        /// <summary>Percorso relativo al repository da cui si è partiti.</summary>
        public string Path { get; init; }
        public string Outcome { get; init; }
        /// <summary>Cosa dire all'utente quando il submodule non è finito sul commit registrato. <c>null</c> = niente da dire.</summary>
        public string Note { get; init; }
        public string Before { get; init; }
        public string After { get; init; }
    }

    public sealed class SubmoduleAlignResult
    {
        public IReadOnlyList<SubmoduleAlignStep> Steps { get; init; } = Array.Empty<SubmoduleAlignStep>();
        public bool Failed => Steps.Any(s => s.Outcome == SubmoduleAlignStep.Failed);
        public IReadOnlyList<string> Notes => Steps.Where(s => s.Note != null).Select(s => s.Note).ToList();
    }

    public interface ISubmoduleAligner
    {
        /// <summary>
        /// Porta i submodule al commit che il repository registra. <paramref name="onlyPath"/>
        /// limita a un submodule (percorso relativo a <paramref name="repositoryPath"/>).
        /// </summary>
        Task<SubmoduleAlignResult> AlignAsync(string repositoryPath, SubmoduleAlignMode mode,
            string onlyPath = null, CancellationToken ct = default);

        /// <summary>
        /// Perché il ramo di questo repository non si può cambiare adesso. Vuoto = si può.
        /// Il cambio di ramo è l'unico caso in cui un submodule può andare a una versione
        /// precedente, quindi parte solo se nessun submodule ha lavoro che andrebbe perso di vista.
        /// </summary>
        Task<IReadOnlyList<string>> SwitchBlockersAsync(string repositoryPath, CancellationToken ct = default);
    }

    /// <summary>
    /// L'allineamento dei submodule <b>che non torna mai indietro da solo</b>.
    /// <para>
    /// Prima, dopo ogni pull, girava <c>git submodule update --init --recursive</c>: mette ogni
    /// submodule sul commit registrato <b>senza guardare dove stava</b>. Se l'avevi portato avanti
    /// — un pull dentro il submodule, o un commit tuo non ancora registrato nel progetto — te lo
    /// riportava indietro in silenzio e con HEAD staccato (provato in sandbox l'01/10/2026).
    /// </para>
    /// <para>Dopo uno scaricamento (<see cref="SubmoduleAlignMode.Forward"/>) un submodule:</para>
    /// <list type="bullet">
    /// <item>più <b>indietro</b> del registrato → avanza, restando sul suo ramo quando si può;</item>
    /// <item>più <b>avanti</b> → resta dov'è: è una versione nuova da registrare nel progetto;</item>
    /// <item>che <b>diverge</b> → resta dov'è e lo si dice: unire due lavori è una scelta, non un automatismo.</item>
    /// </list>
    /// <para>
    /// Dopo un cambio di ramo (<see cref="SubmoduleAlignMode.Exact"/>) va al commit del ramo nuovo
    /// qualunque esso sia: l'ha chiesto chi ha cambiato ramo, e <see cref="SwitchBlockersAsync"/>
    /// ha già garantito che non ci fosse lavoro in sospeso.
    /// </para>
    /// </summary>
    public sealed class SubmoduleAligner : ISubmoduleAligner
    {
        private readonly INativeGitRunner _git;
        private readonly INativeGitTransport _transport;
        private readonly ISubmoduleBranchAttacher _attacher;
        private readonly ILogger<SubmoduleAligner> _logger;

        public SubmoduleAligner(
            INativeGitRunner git,
            INativeGitTransport transport,
            ISubmoduleBranchAttacher attacher,
            ILogger<SubmoduleAligner> logger)
        {
            _git = git;
            _transport = transport;
            _attacher = attacher;
            _logger = logger;
        }

        public async Task<SubmoduleAlignResult> AlignAsync(string repositoryPath, SubmoduleAlignMode mode,
            string onlyPath = null, CancellationToken ct = default)
        {
            var steps = new List<SubmoduleAlignStep>();
            var only = string.IsNullOrWhiteSpace(onlyPath) ? null : onlyPath.Replace('\\', '/').Trim('/');
            await AlignLevelAsync(repositoryPath, string.Empty, mode, only, steps, ct);

            // Chi è arrivato sul commit registrato con HEAD staccato torna sul suo ramo, quando un
            // ramo sta proprio lì: senza, la riga direbbe «HEAD staccato» dopo ogni scaricamento.
            if (steps.Any(s => s.Outcome is SubmoduleAlignStep.Aligned or SubmoduleAlignStep.Populated or SubmoduleAlignStep.Already))
            {
                try { await _attacher.AttachAsync(repositoryPath, ct); }
                catch (Exception ex) { _logger.LogWarning(ex, "[Allinea] riaggancio dei rami non riuscito in '{Path}'.", repositoryPath); }
            }
            return new SubmoduleAlignResult { Steps = steps };
        }

        private async Task AlignLevelAsync(string repoDir, string prefix, SubmoduleAlignMode mode, string only,
            List<SubmoduleAlignStep> steps, CancellationToken ct)
        {
            if (!File.Exists(Path.Combine(repoDir, ".gitmodules"))) return;

            var status = await _git.RunAsync(repoDir, new[] { "submodule", "status" }, ct);
            if (!status.Ok)
            {
                _logger.LogWarning("[Allinea] 'submodule status' non riuscito in '{Dir}': {Why}", repoDir, status.Describe());
                return;
            }

            foreach (var line in SubmoduleStatusLine.Parse(status.Stdout))
            {
                var full = prefix.Length == 0 ? line.Path : prefix + "/" + line.Path;
                var dir = Path.Combine(repoDir, line.Path.Replace('/', Path.DirectorySeparatorChar));

                var wanted = only == null || string.Equals(only, full, StringComparison.OrdinalIgnoreCase);
                var onTheWay = only != null && only.StartsWith(full + "/", StringComparison.OrdinalIgnoreCase);
                if (!wanted && !onTheWay) continue;

                if (wanted)
                {
                    var step = await AlignOneAsync(repoDir, line, full, dir, mode, ct);
                    steps.Add(step);
                    if (step.Outcome == SubmoduleAlignStep.Failed) continue;   // dentro non si scende: non si sa cosa c'è
                }

                if (Directory.Exists(dir))
                    await AlignLevelAsync(dir, full, mode, wanted ? null : only, steps, ct);
            }
        }

        private async Task<SubmoduleAlignStep> AlignOneAsync(string repoDir, SubmoduleStatusLine line, string full,
            string dir, SubmoduleAlignMode mode, CancellationToken ct)
        {
            // Mai scaricato: non c'è un «prima» da proteggere, lo si popola e basta.
            if (line.Prefix == '-' || !Directory.Exists(dir))
            {
                var init = await _git.RunAsync(repoDir, new[] { "submodule", "update", "--init", "--", line.Path }, ct);
                return init.Ok
                    ? new SubmoduleAlignStep { Path = full, Outcome = SubmoduleAlignStep.Populated, After = await HeadAsync(dir, ct) }
                    : Fail(full, $"'{full}' non è stato scaricato: {init.Describe()}");
            }

            if (line.Prefix == 'U')
                return Fail(full, $"'{full}' ha un conflitto sulla versione registrata: va risolto prima di poterlo allineare.");

            var recorded = await RecordedAsync(repoDir, line.Path, ct);
            if (recorded == null)
                return Fail(full, $"Non si riesce a leggere quale versione di '{full}' registra il progetto.");

            var before = await HeadAsync(dir, ct);
            if (before == null)
                return Fail(full, $"Non si riesce a leggere la versione in checkout di '{full}'.");

            if (string.Equals(before, recorded, StringComparison.OrdinalIgnoreCase))
                return new SubmoduleAlignStep { Path = full, Outcome = SubmoduleAlignStep.Already, Before = before, After = before };

            // Il commit registrato può non essere ancora qui: lo porta un fetch nel submodule.
            var relation = await SubmoduleRelation.ReadAsync(_git, dir, recorded, before, ct);
            if (relation == SubmoduleRelation.Unknown)
            {
                var fetch = await _transport.FetchAsync(dir, "origin", ct);
                if (!fetch.Ok)
                    return Fail(full, $"'{full}': il remoto non ha risposto, quindi la versione registrata non è stata scaricata ({fetch.Error}).");
                relation = await SubmoduleRelation.ReadAsync(_git, dir, recorded, before, ct);
                if (relation == SubmoduleRelation.Unknown)
                    return Fail(full, $"Il progetto registra per '{full}' il commit {Short(recorded)}, che sul remoto di '{full}' non esiste: " +
                                      "chi l'ha registrato non ha ancora pubblicato quel submodule.");
            }

            if (mode == SubmoduleAlignMode.Exact)
                return await CheckoutRecordedAsync(repoDir, line.Path, full, dir, before, ct);

            switch (relation)
            {
                case SubmoduleRelation.Ahead:
                    return new SubmoduleAlignStep { Path = full, Outcome = SubmoduleAlignStep.LeftAhead, Before = before, After = before };

                case SubmoduleRelation.Diverged:
                    return new SubmoduleAlignStep
                    {
                        Path = full, Outcome = SubmoduleAlignStep.LeftDiverged, Before = before, After = before,
                        Note = $"'{full}' diverge dalla versione che il progetto registra: non è stato spostato. " +
                               $"Usa «Aggiorna all'ultima» dentro '{full}' per unire i due lavori.",
                    };
            }

            // Più indietro: si avanza. Restando sul ramo quando il commit registrato sta sul ramo
            // remoto che il submodule segue — così HEAD non si stacca e non serve riagganciarlo.
            if (await CanFastForwardOnBranchAsync(dir, recorded, ct))
            {
                var ff = await _git.RunAsync(dir, new[] { "merge", "--ff-only", recorded }, ct);
                if (!ff.Ok)
                    return Fail(full, $"'{full}' non è stato allineato: {ff.Describe()}");
                return new SubmoduleAlignStep { Path = full, Outcome = SubmoduleAlignStep.Aligned, Before = before, After = recorded };
            }

            return await CheckoutRecordedAsync(repoDir, line.Path, full, dir, before, ct);
        }

        /// <summary>Il checkout di git sul commit registrato: lascia HEAD staccato, ci pensa poi il riaggancio.</summary>
        private async Task<SubmoduleAlignStep> CheckoutRecordedAsync(string repoDir, string relativePath, string full,
            string dir, string before, CancellationToken ct)
        {
            var update = await _git.RunAsync(repoDir, new[] { "submodule", "update", "--init", "--", relativePath }, ct);
            if (!update.Ok)
                return Fail(full, $"'{full}' non è stato allineato: {update.Describe()}");
            return new SubmoduleAlignStep { Path = full, Outcome = SubmoduleAlignStep.Aligned, Before = before, After = await HeadAsync(dir, ct) };
        }

        private async Task<bool> CanFastForwardOnBranchAsync(string dir, string recorded, CancellationToken ct)
        {
            var branch = await _git.RunAsync(dir, new[] { "symbolic-ref", "--short", "-q", "HEAD" }, ct);
            if (!branch.Ok || string.IsNullOrWhiteSpace(branch.Stdout)) return false;   // HEAD staccato

            var upstream = await _git.RunAsync(dir, new[] { "rev-parse", "--verify", "--quiet", "@{upstream}" }, ct);
            if (!upstream.Ok || string.IsNullOrWhiteSpace(upstream.Stdout)) return false;

            // Se il commit registrato NON sta sul ramo remoto seguito appartiene a un altro ramo:
            // avanzare il ramo locale fin lì ci metterebbe dentro commit che poi si pubblicherebbero
            // sul ramo sbagliato.
            return await SubmoduleRelation.IsAncestorAsync(_git, dir, recorded, upstream.Stdout.Trim(), ct);
        }

        public async Task<IReadOnlyList<string>> SwitchBlockersAsync(string repositoryPath, CancellationToken ct = default)
        {
            var blockers = new List<string>();
            await SwitchBlockersLevelAsync(repositoryPath, string.Empty, blockers, ct);
            return blockers;
        }

        private async Task SwitchBlockersLevelAsync(string repoDir, string prefix, List<string> blockers, CancellationToken ct)
        {
            if (!File.Exists(Path.Combine(repoDir, ".gitmodules"))) return;
            var status = await _git.RunAsync(repoDir, new[] { "submodule", "status" }, ct);
            if (!status.Ok) return;

            foreach (var line in SubmoduleStatusLine.Parse(status.Stdout))
            {
                if (line.Prefix == '-') continue;   // non scaricato: non ha niente da perdere
                var full = prefix.Length == 0 ? line.Path : prefix + "/" + line.Path;
                var dir = Path.Combine(repoDir, line.Path.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(dir)) continue;

                if (line.Prefix != ' ')
                {
                    blockers.Add($"'{full}' non è alla versione che il progetto registra: prima committa il progetto o allinea '{full}'.");
                    continue;
                }

                var dirty = await _git.RunAsync(dir,
                    new[] { "status", "--porcelain", "--untracked-files=no", "--ignore-submodules=all" }, ct);
                if (dirty.Ok && !string.IsNullOrWhiteSpace(dirty.Stdout))
                {
                    blockers.Add($"'{full}' ha modifiche non salvate: committale (o scartale) prima di cambiare ramo.");
                    continue;
                }

                await SwitchBlockersLevelAsync(dir, full, blockers, ct);
            }
        }

        // ---- letture ----

        /// <summary>Il commit che il repository registra per il submodule: la voce dell'indice, quella che userebbe git.</summary>
        private async Task<string> RecordedAsync(string repoDir, string relativePath, CancellationToken ct)
        {
            var res = await _git.RunAsync(repoDir, new[] { "ls-files", "-s", "--", relativePath }, ct);
            if (!res.Ok) return null;
            foreach (var raw in (res.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                // "160000 <sha> <stage>\t<path>"
                var head = raw.Split('\t')[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (head.Length >= 3 && head[0] == "160000" && head[2] == "0") return head[1];
            }
            return null;
        }

        private async Task<string> HeadAsync(string dir, CancellationToken ct)
        {
            var res = await _git.RunAsync(dir, new[] { "rev-parse", "HEAD" }, ct);
            var sha = res.Stdout?.Trim();
            return res.Ok && !string.IsNullOrEmpty(sha) ? sha : null;
        }

        private static SubmoduleAlignStep Fail(string full, string why)
            => new SubmoduleAlignStep { Path = full, Outcome = SubmoduleAlignStep.Failed, Note = why };

        private static string Short(string sha) => sha.Substring(0, Math.Min(8, sha.Length));
    }
}
