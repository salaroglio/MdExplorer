using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Features.AgentMemory;
using MdExplorer.Features.Services.KnowledgeGraph;
using MdExplorer.Services.AgentMemory;
using MdExplorer.Services.AgentRegistry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// La <b>vista umana</b> della memoria degli agenti (§11 Fase 5d): a differenza di
    /// <see cref="MemoryController"/> (l'agente, autenticato via RunToken, che vede solo il
    /// proprio grafo), questo è il canale UI — l'umano ispeziona la memoria di TUTTI i cittadini
    /// del progetto + il grafo condiviso, e <b>cura</b> i fatti (cambia confidence, rimuove).
    /// Loopback + guardia R12. La curatela è vincolata ai soli grafi appartenenti al progetto
    /// (grafi degli agenti cittadini + shared): nessuna scrittura su grafi arbitrari.
    /// </summary>
    [ApiController]
    [Route("api/mem")]
    public class MemoryAdminController : ControllerBase
    {
        private readonly IAgentRegistryService _registry;
        private readonly IAgentMemoryService _memory;
        private readonly IFusekiConnectionResolver _fusekiResolver;
        private readonly ILogger<MemoryAdminController> _logger;

        public MemoryAdminController(
            IAgentRegistryService registry,
            IAgentMemoryService memory,
            IFusekiConnectionResolver fusekiResolver,
            ILogger<MemoryAdminController> logger)
        {
            _registry = registry;
            _memory = memory;
            _fusekiResolver = fusekiResolver;
            _logger = logger;
        }

        /// <summary>Elenca i fatti in memoria del progetto (tutti gli agenti + shared), o di un solo agente.</summary>
        [HttpGet("facts")]
        public async Task<IActionResult> Facts([FromQuery] string projectPath, [FromQuery] string agent = null, [FromQuery] int limit = 200)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = "projectPath è obbligatorio." });

            FusekiConnection conn;
            try { conn = await _fusekiResolver.ResolveAsync(projectPath); }
            catch (FusekiAddonMissingException ax) { return StatusCode(409, new { error = ax.Message }); }
            if (conn == null)
                return StatusCode(409, new { error = "La memoria (Fuseki) non è abilitata per questo progetto." });

            var graphToName = ResolveGraphNames(projectPath);   // grafo → nome agente
            var graphs = new List<string> { AgentMemoryGraphs.Shared };
            if (!string.IsNullOrWhiteSpace(agent))
            {
                var only = graphToName.FirstOrDefault(kv => string.Equals(kv.Value, agent.Trim(), StringComparison.OrdinalIgnoreCase));
                if (only.Key != null) graphs.Add(only.Key);
            }
            else
            {
                graphs.AddRange(graphToName.Keys);
            }

            try
            {
                var facts = await _memory.ListAsync(conn, graphs, limit);
                return Ok(new
                {
                    facts = facts.Select(f => new
                    {
                        factUri = f.FactUri,
                        graph = f.Graph,
                        agent = f.Shared ? "(condiviso)" : (graphToName.TryGetValue(f.Graph, out var n) ? n : "(sconosciuto)"),
                        statement = f.Statement,
                        confidence = f.Confidence,
                        tags = f.Tags,
                        createdAt = f.CreatedAt,
                        shared = f.Shared,
                    }).ToList()
                });
            }
            catch (FusekiRequestException fx)
            {
                return StatusCode(502, new { error = $"Fuseki ha rifiutato la query: {fx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MemoryAdmin] elenco fatti fallito per '{Project}'", projectPath);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>Curatela: cambia la confidence di un fatto (solo grafi del progetto).</summary>
        [HttpPost("facts/confidence")]
        public async Task<IActionResult> SetConfidence([FromBody] CurateConfidenceRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ProjectPath) ||
                string.IsNullOrWhiteSpace(request.FactUri) || string.IsNullOrWhiteSpace(request.Graph))
                return BadRequest(new { error = "projectPath, factUri, graph sono obbligatori." });
            if (request.Confidence < 0 || request.Confidence > 1)
                return BadRequest(new { error = "confidence deve essere tra 0 e 1." });

            var (conn, error) = await GuardProjectGraphAsync(request.ProjectPath, request.Graph);
            if (conn == null) return error;

            try
            {
                await _memory.SetConfidenceAsync(conn, request.Graph, request.FactUri, request.Confidence);
                return Ok(new { updated = true });
            }
            catch (FusekiRequestException fx)
            {
                return StatusCode(502, new { error = $"Fuseki ha rifiutato l'update: {fx.Message}" });
            }
        }

        /// <summary>Curatela: rimuove un fatto (solo grafi del progetto).</summary>
        [HttpDelete("facts")]
        public async Task<IActionResult> DeleteFact([FromQuery] string projectPath, [FromQuery] string graph, [FromQuery] string factUri)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || string.IsNullOrWhiteSpace(graph) || string.IsNullOrWhiteSpace(factUri))
                return BadRequest(new { error = "projectPath, graph, factUri sono obbligatori." });

            var (conn, error) = await GuardProjectGraphAsync(projectPath, graph);
            if (conn == null) return error;

            try
            {
                await _memory.DeleteFactAsync(conn, graph, factUri);
                return Ok(new { deleted = true });
            }
            catch (FusekiRequestException fx)
            {
                return StatusCode(502, new { error = $"Fuseki ha rifiutato la delete: {fx.Message}" });
            }
        }

        /// <summary>Diario markdown della memoria (§11.1, terzo livello): vista leggibile, generata a richiesta.</summary>
        [HttpGet("diary")]
        public async Task<IActionResult> Diary([FromQuery] string projectPath, [FromQuery] string agent = null)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { error = "projectPath è obbligatorio." });
            FusekiConnection conn;
            try { conn = await _fusekiResolver.ResolveAsync(projectPath); }
            catch (FusekiAddonMissingException ax) { return StatusCode(409, new { error = ax.Message }); }
            if (conn == null)
                return StatusCode(409, new { error = "La memoria (Fuseki) non è abilitata per questo progetto." });

            var graphToName = ResolveGraphNames(projectPath);
            var graphs = new List<string> { AgentMemoryGraphs.Shared };
            graphs.AddRange(string.IsNullOrWhiteSpace(agent)
                ? graphToName.Keys
                : graphToName.Where(kv => string.Equals(kv.Value, agent.Trim(), StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Key));

            var facts = await _memory.ListAsync(conn, graphs, 1000);
            var byAgent = facts
                .GroupBy(f => f.Shared ? "Città (condiviso)" : (graphToName.TryGetValue(f.Graph, out var n) ? n : "(sconosciuto)"))
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

            var sb = new StringBuilder();
            sb.Append("# Diario della memoria degli agenti\n\n");
            foreach (var grp in byAgent)
            {
                sb.Append("## ").Append(grp.Key).Append("\n\n");
                foreach (var f in grp.OrderByDescending(x => x.Confidence))
                {
                    sb.Append("- ").Append(f.Statement.Trim())
                      .Append(" _(affidabilità ").Append(f.Confidence.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture));
                    if (f.Tags != null && f.Tags.Count > 0)
                        sb.Append("; tag: ").Append(string.Join(", ", f.Tags));
                    sb.Append(")_\n");
                }
                sb.Append('\n');
            }
            return Content(sb.ToString(), "text/markdown");
        }

        // Mappa grafo → nome agente per i cittadini del progetto (l'identità è la chiave del grafo).
        private Dictionary<string, string> ResolveGraphNames(string projectPath)
        {
            var map = new Dictionary<string, string>();
            foreach (var e in _registry.RefreshCatalog(projectPath).Where(e => e.IsCitizen && e.IdentityId != null))
                map[AgentMemoryGraphs.ForAgent(e.IdentityId.Value)] = e.Name;
            return map;
        }

        // Vincola una curatela a un grafo che appartiene DAVVERO al progetto (agente cittadino o
        // shared): impedisce all'umano — o a un input manipolato — di toccare grafi arbitrari.
        private async Task<(FusekiConnection Conn, IActionResult Error)> GuardProjectGraphAsync(string projectPath, string graph)
        {
            FusekiConnection conn;
            try { conn = await _fusekiResolver.ResolveAsync(projectPath); }
            catch (FusekiAddonMissingException ax) { return (null, StatusCode(409, new { error = ax.Message })); }
            if (conn == null)
                return (null, StatusCode(409, new { error = "La memoria (Fuseki) non è abilitata per questo progetto." }));

            var allowed = new HashSet<string>(ResolveGraphNames(projectPath).Keys) { AgentMemoryGraphs.Shared };
            if (!allowed.Contains(graph))
                return (null, BadRequest(new { error = "Il grafo indicato non appartiene a questo progetto." }));
            return (conn, null);
        }
    }

    public class CurateConfidenceRequest
    {
        public string ProjectPath { get; set; }
        public string Graph { get; set; }
        public string FactUri { get; set; }
        public double Confidence { get; set; }
    }
}
