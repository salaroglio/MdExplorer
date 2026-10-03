using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.AgenticEnvironments
{
    /// <summary>
    /// Perché un ambiente agentico non è utilizzabile. Sono codici, non frasi: la frase la scrive
    /// chi la mostra (Mark, nella lingua dell'app), il dettaglio tecnico sta in
    /// <see cref="AgenticEnvironmentStatus.Detail"/>.
    /// </summary>
    public static class AgenticEnvironmentReasons
    {
        /// <summary>Il CLI non è nel PATH del servizio.</summary>
        public const string NotInstalled = "not-installed";
        /// <summary>Il CLI c'è ma dichiara che l'accesso non è stato fatto.</summary>
        public const string NotLoggedIn = "not-logged-in";
        /// <summary>opencode c'è ma non elenca nessun modello.</summary>
        public const string NoModels = "no-models";
        /// <summary>Il CLI non ha risposto entro il tempo massimo.</summary>
        public const string Timeout = "timeout";
        /// <summary>Il CLI ha risposto qualcosa che non si sa leggere, o il controllo è fallito.</summary>
        public const string Error = "error";
    }

    /// <summary>Esito del controllo «si può usare?» di un ambiente che risulta installato.</summary>
    public sealed class AgenticEnvironmentUsability
    {
        public bool Usable { get; private set; }
        /// <summary>Uno dei <see cref="AgenticEnvironmentReasons"/>, oppure null se utilizzabile.</summary>
        public string Reason { get; private set; }
        public string Detail { get; private set; }

        public static AgenticEnvironmentUsability Ok(string detail) =>
            new AgenticEnvironmentUsability { Usable = true, Detail = detail };

        public static AgenticEnvironmentUsability No(string reason, string detail) =>
            new AgenticEnvironmentUsability { Usable = false, Reason = reason, Detail = detail };
    }

    /// <summary>Cosa si sa di un ambiente agentico su questo computer, dopo averlo provato.</summary>
    public sealed class AgenticEnvironmentStatus
    {
        /// <summary>Lo stesso id dell'harness: <c>copilot</c>, <c>claude</c>, <c>opencode</c>.</summary>
        public string Id { get; set; }
        public bool Installed { get; set; }
        /// <summary>Dove è stato trovato il CLI, oppure null.</summary>
        public string Path { get; set; }
        /// <summary>Installato <b>e</b> pronto a rispondere: è questo che decide la scelta.</summary>
        public bool Usable { get; set; }
        /// <summary>Uno dei <see cref="AgenticEnvironmentReasons"/>, oppure null se utilizzabile.</summary>
        public string Reason { get; set; }
        /// <summary>Dettaglio tecnico, per il registro e per chi vuole capire.</summary>
        public string Detail { get; set; }
        public long ElapsedMs { get; set; }
    }

    /// <summary>
    /// Il controllo di un singolo ambiente. Due domande separate, perché sono due fatti diversi e
    /// confonderli è già costato diagnosi sbagliate: «è installato?» è un'ispezione del PATH,
    /// «si può usare?» è un comando vero lanciato sul CLI.
    /// </summary>
    public interface IAgenticEnvironmentCheck
    {
        string Id { get; }
        /// <summary>Percorso del CLI nel PATH del servizio, oppure null. Nessun processo lanciato.</summary>
        string ResolvePath();
        /// <summary>Prova funzionale: chiamata solo se <see cref="ResolvePath"/> ha trovato il CLI.</summary>
        Task<AgenticEnvironmentUsability> CheckUsableAsync(CancellationToken ct);
    }

    public interface IAgenticEnvironmentProbe
    {
        /// <summary>Prova tutti gli ambienti, in parallelo, e restituisce un esito per ciascuno.</summary>
        Task<IReadOnlyList<AgenticEnvironmentStatus>> ProbeAsync(CancellationToken ct = default);
    }

    /// <summary>
    /// Dice quali ambienti agentici (Copilot CLI, Claude Code, opencode) sono utilizzabili su questo
    /// computer. Serve a chi deve <b>scegliere</b> un ambiente senza chiederlo all'utente — oggi il
    /// tour di Mark che apre il progetto demo.
    /// <para>
    /// Non decide niente e non ripiega su niente: ogni ambiente ha il suo esito, anche quando è
    /// «non ha risposto in tempo» o «il controllo è fallito». Un controllo che va male è una
    /// risposta, non un'eccezione che fa sparire gli altri due.
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-10-02-Demo-Ambiente-Agentico-Rilevato.md.</para>
    /// </summary>
    public sealed class AgenticEnvironmentProbe : IAgenticEnvironmentProbe
    {
        /// <summary>
        /// Tempo massimo per ambiente. Sulla VM i tre rispondono in 0,15–1,4 s (misurato il
        /// 02/10/2026); il margine è per l'avvio a freddo di un CLI node su Windows, che in passato ha
        /// superato i cinque secondi.
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

        private readonly IReadOnlyList<IAgenticEnvironmentCheck> _checks;
        private readonly ILogger<AgenticEnvironmentProbe> _logger;
        private readonly TimeSpan _timeout;

        public AgenticEnvironmentProbe(IEnumerable<IAgenticEnvironmentCheck> checks, ILogger<AgenticEnvironmentProbe> logger)
            : this(checks, logger, DefaultTimeout)
        {
        }

        public AgenticEnvironmentProbe(IEnumerable<IAgenticEnvironmentCheck> checks, ILogger<AgenticEnvironmentProbe> logger, TimeSpan timeout)
        {
            _checks = (checks ?? throw new ArgumentNullException(nameof(checks))).ToList();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeout = timeout;
        }

        public async Task<IReadOnlyList<AgenticEnvironmentStatus>> ProbeAsync(CancellationToken ct = default)
        {
            // In parallelo: i tre controlli sono indipendenti e il più lento decide l'attesa.
            var results = await Task.WhenAll(_checks.Select(check => ProbeOneAsync(check, ct))).ConfigureAwait(false);
            foreach (var r in results)
            {
                _logger.LogInformation(
                    "[AgenticEnvironmentProbe] {Id}: installed={Installed} usable={Usable} reason={Reason} ({Ms} ms) {Detail}",
                    r.Id, r.Installed, r.Usable, r.Reason ?? "-", r.ElapsedMs, r.Detail);
            }
            return results;
        }

        private async Task<AgenticEnvironmentStatus> ProbeOneAsync(IAgenticEnvironmentCheck check, CancellationToken ct)
        {
            var watch = Stopwatch.StartNew();
            var status = new AgenticEnvironmentStatus { Id = check.Id };
            try
            {
                status.Path = check.ResolvePath();
                status.Installed = status.Path != null;
                if (!status.Installed)
                {
                    status.Reason = AgenticEnvironmentReasons.NotInstalled;
                    status.Detail = $"'{check.Id}' non è nel PATH del servizio.";
                    return status;
                }

                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(_timeout);
                // WaitAsync oltre al token: un controllo che ignora la cancellazione non deve
                // tenere fermi gli altri due né chi aspetta la risposta.
                var usability = await check.CheckUsableAsync(timeout.Token).WaitAsync(_timeout, ct).ConfigureAwait(false)
                    ?? throw new InvalidOperationException($"Il controllo di '{check.Id}' non ha restituito un esito.");

                status.Usable = usability.Usable;
                status.Reason = usability.Usable ? null : usability.Reason ?? AgenticEnvironmentReasons.Error;
                status.Detail = usability.Detail;
            }
            catch (Exception ex) when (IsTimeout(ex, ct))
            {
                status.Usable = false;
                status.Reason = AgenticEnvironmentReasons.Timeout;
                status.Detail = $"'{check.Id}' non ha risposto entro {_timeout.TotalSeconds:0.#} secondi.";
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw; // chi ha chiesto il probe se n'è andato: non è un esito
            }
            catch (Exception ex)
            {
                status.Usable = false;
                status.Reason = AgenticEnvironmentReasons.Error;
                status.Detail = ex.Message;
            }
            finally
            {
                status.ElapsedMs = watch.ElapsedMilliseconds;
            }
            return status;
        }

        private static bool IsTimeout(Exception ex, CancellationToken callerToken) =>
            ex is TimeoutException || (ex is OperationCanceledException && !callerToken.IsCancellationRequested);
    }
}
