using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Services;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Git
{
    /// <summary>Cosa è successo ai submodule quando si è aperto il progetto.</summary>
    public sealed class SubmoduleEnsureResult
    {
        /// <summary>Nessun <c>.gitmodules</c>, oppure erano già tutti a posto: non è stato fatto nulla.</summary>
        public bool NothingToDo { get; init; }
        /// <summary>Percorsi dei submodule che erano vuoti prima dell'intervento.</summary>
        public IReadOnlyList<string> Missing { get; init; } = Array.Empty<string>();
        public bool Success { get; init; }
        /// <summary>Motivo del fallimento, scritto per essere letto da una persona.</summary>
        public string Error { get; init; }
    }

    public interface IProjectSubmoduleInitializer
    {
        /// <summary>
        /// Popola i submodule del progetto se ce n'è bisogno. Idempotente: se sono già a posto
        /// non lancia nessun processo.
        /// </summary>
        Task<SubmoduleEnsureResult> EnsureAsync(string projectPath, CancellationToken ct = default);
    }

    /// <summary>
    /// Popola i submodule <b>all'apertura del progetto</b>.
    /// <para>
    /// Prima lo faceva solo chi clonava o faceva pull da dentro MdExplorer. Ma un progetto lo si
    /// apre anche dopo averlo clonato da fuori — o clonato senza <c>--recurse-submodules</c> — e
    /// in quel caso nessuno li popolava mai: la cartella del codice restava vuota e l'unico modo
    /// di accorgersene era aprirla. Da qui il «i submodule li trovo sempre vuoti».
    /// </para>
    /// <para>
    /// Non blocca l'apertura: gira in sottofondo e racconta cosa sta facendo, come
    /// l'indicizzazione. Su un repository grande l'inizializzazione non è istantanea, e far
    /// aspettare l'apertura di un progetto per una cartella che magari non guarderai è il modo
    /// più sicuro di far sembrare lenta l'applicazione.
    /// </para>
    /// <para>
    /// <b>Il fallimento si vede.</b> Prima veniva appeso al messaggio di successo del clone
    /// (<c>" (warning: submodule update failed: …)"</c>): il clone risultava riuscito, la cartella
    /// restava vuota e nessuno lo leggeva. Qui è una notifica sua, con dentro il motivo.
    /// </para>
    /// </summary>
    public sealed class ProjectSubmoduleInitializer : IProjectSubmoduleInitializer, IProjectOpenedEventHandler
    {
        private readonly INativeGitRunner _git;
        private readonly IHubContext<MonitorMDHub> _hub;
        private readonly ILogger<ProjectSubmoduleInitializer> _logger;

        // Un progetto alla volta: aprire due volte di fila lo stesso progetto — o due finestre
        // sullo stesso — non deve far partire due 'submodule update' che si contendono l'indice.
        private readonly ConcurrentDictionary<string, byte> _running =
            new(StringComparer.OrdinalIgnoreCase);

        public ProjectSubmoduleInitializer(
            INativeGitRunner git,
            IHubContext<MonitorMDHub> hub,
            ILogger<ProjectSubmoduleInitializer> logger,
            ISubmoduleBranchAttacher attacher = null)
        {
            _git = git;
            _hub = hub;
            _logger = logger;
            _attacher = attacher;
        }

        /// <summary>
        /// Chi rimette i submodule sul loro ramo. Lo stesso che usano pull e clone: il riaggancio
        /// deve comportarsi allo stesso modo da qualunque strada si arrivi.
        /// </summary>
        private readonly ISubmoduleBranchAttacher _attacher;

        /// <summary>
        /// L'aggancio all'apertura del progetto. Il contratto dell'hook dice «veloce o
        /// fire-and-forget»: qui è la seconda, perché il lavoro vero può durare minuti.
        /// </summary>
        public void OnProjectOpened(string projectPath)
        {
            _ = Task.Run(async () =>
            {
                try { await EnsureAsync(projectPath); }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Submodule] inizializzazione per '{Path}' fallita.", projectPath);
                }
            });
        }

        public async Task<SubmoduleEnsureResult> EnsureAsync(string projectPath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return new SubmoduleEnsureResult { NothingToDo = true, Success = true };

            // Niente .gitmodules → non c'è nemmeno un processo da lanciare. È il caso della
            // stragrande maggioranza dei progetti di documentazione, e deve costare zero.
            if (!File.Exists(Path.Combine(projectPath, ".gitmodules")))
                return new SubmoduleEnsureResult { NothingToDo = true, Success = true };

            // Il riaggancio va fatto ANCHE quando non c'era niente da popolare: un submodule
            // scaricato in un giro precedente e' li' con HEAD staccato, e senza un ramo non ci si
            // puo' committare. E' la strada da cui passa l'apertura del progetto, dove capita di
            // piu': chi clona da fuori MdExplorer se li ritrova cosi'.
            if (_attacher != null)
            {
                try { await _attacher.AttachAsync(projectPath, ct); }
                catch (Exception ex) { _logger.LogWarning(ex, "[Submodule] riaggancio non riuscito per '{Path}'.", projectPath); }
            }

            if (!_running.TryAdd(projectPath, 0))
            {
                _logger.LogDebug("[Submodule] '{Path}': inizializzazione già in corso, non ne parto un'altra.", projectPath);
                return new SubmoduleEnsureResult { NothingToDo = true, Success = true };
            }

            try
            {
                var missing = await MissingSubmodulesAsync(projectPath, ct);
                if (missing.Count == 0)
                {
                    _logger.LogDebug("[Submodule] '{Path}': già tutti popolati.", projectPath);
                    return new SubmoduleEnsureResult { NothingToDo = true, Success = true };
                }

                _logger.LogInformation("[Submodule] '{Path}': {Count} da popolare ({List}).",
                    projectPath, missing.Count, string.Join(", ", missing));
                await NotifyAsync("submoduleInitStarted", new { projectPath, submodules = missing }, ct);

                var res = await _git.RunAsync(projectPath,
                    new[] { "submodule", "update", "--init", "--recursive" }, ct);

                if (!res.Ok)
                {
                    var reason = ExplainFailure(res, missing);
                    _logger.LogError("[Submodule] '{Path}': {Reason}", projectPath, reason);
                    await NotifyAsync("submoduleInitFailed", new { projectPath, submodules = missing, error = reason }, ct);
                    return new SubmoduleEnsureResult { Missing = missing, Success = false, Error = reason };
                }

                // Riuscito il comando non basta: quello che conta è che le cartelle non siano
                // più vuote. Un 'update' che esce zero lasciando tutto com'era sarebbe il
                // fallimento silenzioso di prima, sotto un altro nome.
                var stillMissing = await MissingSubmodulesAsync(projectPath, ct);
                if (stillMissing.Count > 0)
                {
                    var reason = $"git non ha segnalato errori ma {string.Join(", ", stillMissing)} " +
                                 "risulta ancora vuoto. Prova ad aprire un terminale nel progetto e a lanciare " +
                                 "'git submodule update --init --recursive' per vedere cosa dice.";
                    _logger.LogError("[Submodule] '{Path}': {Reason}", projectPath, reason);
                    await NotifyAsync("submoduleInitFailed", new { projectPath, submodules = stillMissing, error = reason }, ct);
                    return new SubmoduleEnsureResult { Missing = missing, Success = false, Error = reason };
                }

                _logger.LogInformation("[Submodule] '{Path}': popolati {Count}.", projectPath, missing.Count);
                await NotifyAsync("submoduleInitCompleted", new { projectPath, submodules = missing }, ct);
                return new SubmoduleEnsureResult { Missing = missing, Success = true };
            }
            finally
            {
                _running.TryRemove(projectPath, out _);
            }
        }

        /// <summary>
        /// I submodule non ancora inizializzati. <c>git submodule status</c> li marca con un
        /// <c>-</c> iniziale: è il modo di chiederlo a git invece di indovinare guardando se la
        /// cartella è vuota (potrebbe contenere file ignorati e sembrare popolata).
        /// </summary>
        private async Task<IReadOnlyList<string>> MissingSubmodulesAsync(string projectPath, CancellationToken ct)
        {
            var res = await _git.RunAsync(projectPath, new[] { "submodule", "status", "--recursive" }, ct);
            if (!res.Ok)
            {
                _logger.LogWarning("[Submodule] stato non leggibile per '{Path}': {Why}", projectPath, res.Describe());
                return Array.Empty<string>();
            }

            var missing = new List<string>();
            foreach (var raw in (res.Stdout ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var line = raw.TrimEnd('\r');
                if (line.Length == 0 || line[0] != '-') continue;

                // Formato: "-<sha> <path> (<descrizione>)"
                var parts = line.Substring(1).Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) missing.Add(parts[1]);
            }
            return missing;
        }

        /// <summary>
        /// Traduce il fallimento in qualcosa su cui si può agire. Il caso che capita davvero è
        /// il submodule privato: il clone del padre è andato bene perché quelle credenziali
        /// c'erano, ma il figlio sta su un altro indirizzo e nessuno le ha per lui.
        /// </summary>
        private static string ExplainFailure(GitResult res, IReadOnlyList<string> missing)
        {
            var stderr = res.Stderr ?? string.Empty;
            var quali = string.Join(", ", missing);

            if (stderr.Contains("Authentication failed", StringComparison.OrdinalIgnoreCase)
                || stderr.Contains("could not read Username", StringComparison.OrdinalIgnoreCase)
                || stderr.Contains("Permission denied", StringComparison.OrdinalIgnoreCase)
                || stderr.Contains("terminal prompts disabled", StringComparison.OrdinalIgnoreCase))
            {
                return $"Il codice in {quali} è su un repository che richiede credenziali che qui non ci sono. " +
                       "Le credenziali del progetto principale non valgono automaticamente per i sottomoduli: " +
                       "configurale per quel repository (Git → gestione account) e riapri il progetto.";
            }

            if (stderr.Contains("could not resolve host", StringComparison.OrdinalIgnoreCase)
                || stderr.Contains("Could not resolve", StringComparison.OrdinalIgnoreCase))
            {
                return $"{quali} non è raggiungibile: sembra che la rete non ci sia. " +
                       "Il progetto si apre lo stesso, ma il codice resta vuoto finché non riapri con la rete.";
            }

            return $"Non sono riuscito a popolare {quali}: {res.Describe()}";
        }

        private async Task NotifyAsync(string method, object payload, CancellationToken ct)
        {
            // Best-effort: nessuna finestra aperta non è un motivo per far fallire il lavoro.
            try { await _hub.Clients.All.SendAsync(method, payload, ct); }
            catch (Exception ex) { _logger.LogDebug(ex, "[Submodule] notifica '{Method}' non recapitata.", method); }
        }
    }
}
