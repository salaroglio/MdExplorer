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
    /// headless su GitHub Copilot CLI. Applica working directory ed <b>ambiente</b> (dove
    /// viaggia il RunToken) sul provider, poi ripulisce l'override — il provider è un
    /// singleton condiviso e un token non deve trapelare al chiamante successivo.
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

        public async Task<string> RunTurnAsync(AgentTurnRequest request, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var copilot = _providers?
                .FirstOrDefault(p => p.GetProviderType() == ProviderType.CopilotCli) as CopilotCliProvider;
            if (copilot == null || !copilot.IsAvailable())
            {
                throw new InvalidOperationException(
                    "Copilot CLI is not installed or not authenticated. Install it and run 'copilot' once to log in.");
            }

            copilot.WorkingDirectory = request.WorkingDirectory;
            copilot.EnvironmentOverrides = request.Environment;
            try
            {
                return await copilot.ChatAsync(request.ComposedPrompt, ct: ct);
            }
            finally
            {
                // Non lasciare il RunToken (o una working dir stantìa) sul singleton condiviso.
                copilot.EnvironmentOverrides = null;
            }
        }
    }
}
