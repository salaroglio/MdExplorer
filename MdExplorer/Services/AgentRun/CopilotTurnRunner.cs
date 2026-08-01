using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services.AI;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// Implementazione reale del seam <see cref="IAgentTurnRunner"/>: esegue un turno
    /// headless su GitHub Copilot CLI passando working directory ed <b>ambiente</b> (dove
    /// viaggia il RunToken) <b>per-chiamata</b> via <see cref="CopilotInvocation"/>. Nessuna
    /// scrittura su stato condiviso del provider: due run concorrenti non possono scambiarsi
    /// l'identità perché non esiste un campo su cui competere (superato il vecchio pattern
    /// "set property → run → clear" che sotto concorrenza faceva partire un run col token
    /// di un altro).
    /// <para>
    /// Questo è il punto in cui una fake <see cref="IAgentTurnRunner"/> sostituisce Copilot
    /// nei test, senza spawn di processo.
    /// </para>
    /// </summary>
    public class CopilotTurnRunner : IAgentTurnRunner
    {
        private readonly IEnumerable<IAiProvider> _providers;

        public CopilotTurnRunner(IEnumerable<IAiProvider> providers)
        {
            _providers = providers;
        }

        public async Task<AgentTurnResult> RunTurnAsync(AgentTurnRequest request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var copilot = _providers?
                .FirstOrDefault(p => p.GetProviderType() == ProviderType.CopilotCli) as CopilotCliProvider;
            if (copilot == null || !copilot.IsAvailable())
            {
                throw new InvalidOperationException(
                    "Copilot CLI is not installed or not authenticated. Install it and run 'copilot' once to log in.");
            }

            // Identità e working dir viaggiano nell'invocation, non sul singleton: isolamento
            // per costruzione tra run concorrenti.
            var invocation = new CopilotInvocation(request.WorkingDirectory, request.Environment);

            // Esito DETTAGLIATO, non solo il testo: `RunHeadlessAsync` solleva solo quando
            // l'uscita è non-zero E stderr non è vuoto, quindi un'uscita non-zero silenziosa
            // passava per successo e faceva partire tutta la macchina a valle (pubblicazione
            // del deliverable, auto-merge, verdetto federato di successo).
            var run = await copilot.RunHeadlessDetailedAsync(request.ComposedPrompt, invocation, ct: ct);

            if (run.ExitCode != 0)
            {
                var detail = string.IsNullOrWhiteSpace(run.Error)
                    ? $"Copilot CLI è uscito con codice {run.ExitCode} senza messaggio d'errore."
                    : $"Copilot CLI è uscito con codice {run.ExitCode}: {run.Error}";
                return AgentTurnResult.Failed(AgentTurnOutcome.ProviderError, detail, run.Text);
            }

            return AgentTurnResult.Completed(run.Text);
        }
    }
}
