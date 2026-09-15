using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace MdExplorer.Services.AgentRun
{
    /// <summary>
    /// I tool concessi a un agente per un turno, più il registro di <b>cosa è stato negato e
    /// perché</b>. Le decisioni non sono un dettaglio di log: sono la prova che il modello di
    /// autorizzazione è stato applicato, e vanno guardate quando un agente "non fa" qualcosa.
    /// </summary>
    public sealed class AgentToolSet : IAsyncDisposable
    {
        private readonly McpClient _client;

        internal AgentToolSet(McpClient client, IReadOnlyList<AIFunction> functions, IReadOnlyList<ToolDecision> decisions)
        {
            _client = client;
            Functions = functions;
            Decisions = decisions;
        }

        /// <summary>Solo i tool effettivamente invocabili nel turno.</summary>
        public IReadOnlyList<AIFunction> Functions { get; }

        /// <summary>Esito per ogni tool offerto dal server, concessi e non.</summary>
        public IReadOnlyList<ToolDecision> Decisions { get; }

        public async ValueTask DisposeAsync()
        {
            if (_client != null)
            {
                try { await _client.DisposeAsync(); } catch { /* chiusura best-effort */ }
            }
        }
    }

    public interface IAgentMcpToolProvider
    {
        /// <summary>
        /// Apre un server MCP dedicato a questo turno e ne restituisce i tool <b>filtrati</b> dal
        /// catalogo. Il chiamante deve fare <c>DisposeAsync</c> a fine turno.
        /// </summary>
        Task<AgentToolSet> OpenAsync(
            IReadOnlyDictionary<string, string> environment,
            IEnumerable<string> declaredManifest,
            bool trusted,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Ponte verso il <b>nostro stesso</b> server MCP (<c>MdExplorer.Mcp</c>).
    /// <para>
    /// Perché non ridefinire i tool della città: <c>McpClientTool</c> è già un
    /// <see cref="AIFunction"/>, quindi lo stesso server che serve Copilot serve anche un runner
    /// su <c>IChatClient</c>. Una sola definizione — non due cataloghi che divergono al primo
    /// cambiamento.
    /// </para>
    /// <para>
    /// L'identità viaggia esattamente come con Copilot: il server MCP figlio eredita
    /// <c>MDE_RUN_TOKEN</c> e compagni dall'ambiente che gli passiamo, e i suoi tool richiamano
    /// il Service autenticandosi con quello. Nessun meccanismo di identità nuovo da rivedere.
    /// </para>
    /// </summary>
    public class AgentMcpToolProvider : IAgentMcpToolProvider
    {
        private readonly ILogger<AgentMcpToolProvider> _logger;

        public AgentMcpToolProvider(ILogger<AgentMcpToolProvider> logger)
        {
            _logger = logger;
        }

        public async Task<AgentToolSet> OpenAsync(
            IReadOnlyDictionary<string, string> environment,
            IEnumerable<string> declaredManifest,
            bool trusted,
            CancellationToken ct = default)
        {
            var exe = MdExplorer.Service.ProjectsManager.ResolveMcpExecutable(AppContext.BaseDirectory);
            if (string.IsNullOrWhiteSpace(exe))
            {
                // Precondizione non soddisfatta: fail-loud con messaggio azionabile, mai un
                // agente che gira silenziosamente senza i tool della città.
                throw new InvalidOperationException(
                    "Server MCP di MdExplorer non trovato: senza di esso l'agente non avrebbe i tool della città " +
                    "(list_agents, send_agent_message, request_intervention, memoria). " +
                    "Verificare la pubblicazione in 'mcp/' accanto al Service.");
            }

            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "MdExplorer",
                Command = exe,
                EnvironmentVariables = environment?.ToDictionary(kv => kv.Key, kv => kv.Value),
            });

            var client = await McpClient.CreateAsync(transport, cancellationToken: ct);

            try
            {
                var offered = await client.ListToolsAsync(cancellationToken: ct);
                var decisions = AgentToolCatalog.Decide(
                    offered.Select(t => t.Name), declaredManifest, trusted);

                var allowed = new HashSet<string>(
                    AgentToolCatalog.AllowedNames(decisions), StringComparer.OrdinalIgnoreCase);

                var functions = offered
                    .Where(t => allowed.Contains(t.Name))
                    .Cast<AIFunction>()
                    .ToList();

                var refused = decisions.Where(d => d.Access != ToolAccess.Allowed).ToList();
                if (refused.Count > 0)
                {
                    _logger.LogInformation(
                        "[AgentTools] {Allowed}/{Total} tool concessi; non concessi: {Refused}",
                        functions.Count, decisions.Count,
                        string.Join(", ", refused.Select(r => $"{r.Tool} ({r.Access}: {r.Reason})")));
                }

                foreach (var missing in AgentToolCatalog.UnsupportedCapabilities(declaredManifest, runnerHasShell: false))
                {
                    _logger.LogWarning(
                        "[AgentTools] l'agente dichiara '{Tool}' ma questo runner non la fornisce: {Reason}",
                        missing.Tool, missing.Reason);
                }

                return new AgentToolSet(client, functions, decisions);
            }
            catch
            {
                await client.DisposeAsync();
                throw;
            }
        }
    }
}
