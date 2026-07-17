using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.AgentMemory;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services.KnowledgeGraph;
using MdExplorer.Services.AgentMemory;
using MdExplorer.Services.AgentRegistry;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.A2A
{
    /// <summary>
    /// La memoria semantica degli agenti (Fase 5b, §11): due sole operazioni — <b>assert</b> e
    /// <b>query</b> — riservate a un agente svegliato da un messaggio (identità dal <c>RunToken</c>,
    /// R2). Il <b>named graph è forzato server-side</b> dall'<c>AgentIdentity.Id</c> risolto dal
    /// token, esattamente come <c>$pid</c> per i tool Neo4j: nessun parametro del chiamante può
    /// indicare un grafo, quindi un agente non può né scrivere né leggere la memoria di un altro.
    /// Loopback + guardia R12. Nessuna SPARQL grezza attraversa questo confine.
    /// </summary>
    [ApiController]
    [Route("api/A2A/memory")]
    public class MemoryController : ControllerBase
    {
        public const string RunTokenHeader = "X-MDE-Run-Token";
        private const int DefaultLimit = 20;

        private readonly IRunTokenStore _tokens;
        private readonly IAgentRegistryService _registry;
        private readonly IAgentMemoryService _memory;
        private readonly IFusekiConnectionResolver _fusekiResolver;
        private readonly ILogger<MemoryController> _logger;

        public MemoryController(
            IRunTokenStore tokens,
            IAgentRegistryService registry,
            IAgentMemoryService memory,
            IFusekiConnectionResolver fusekiResolver,
            ILogger<MemoryController> logger)
        {
            _tokens = tokens;
            _registry = registry;
            _memory = memory;
            _fusekiResolver = fusekiResolver;
            _logger = logger;
        }

        /// <summary>
        /// L'agente assere un fatto appreso. Le triple PROV-O (fatto + provenance del run) le
        /// costruisce il Service (R8: input strutturato, mai SPARQL). Il grafo è quello
        /// dell'agente chiamante, ricavato dal token.
        /// </summary>
        [HttpPost("assert")]
        public async Task<IActionResult> Assert([FromBody] AssertFactRequest request)
        {
            var claims = ResolveClaims();
            if (claims == null)
                return Unauthorized(new { error = "RunToken assente o non valido: la memoria è riservata agli agenti svegliati da un messaggio." });
            if (request == null || string.IsNullOrWhiteSpace(request.Statement))
                return BadRequest(new { error = "statement è obbligatorio." });

            var (resolved, error) = await ResolveAgentAndFusekiAsync(claims);
            if (resolved == null)
                return error;

            var fact = new LearnedFactInput
            {
                Statement = request.Statement.Trim(),
                Confidence = request.Confidence ?? 0.7,
                AboutTags = SplitTags(request.About),
                RunId = claims.RunId,
                ConversationId = claims.ConversationId,
                CreatedAtUtc = DateTime.UtcNow,
            };

            try
            {
                var factUri = await _memory.AssertFactAsync(resolved.Conn, resolved.AgentIdentityId, fact);
                _logger.LogInformation("[Memory] {Agent} ha asserito un fatto ({Uri})", claims.AgentName, factUri);
                return Ok(new { asserted = true, factUri });
            }
            catch (FusekiRequestException fx)
            {
                return StatusCode(502, new { error = $"Fuseki ha rifiutato la scrittura: {fx.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Memory] assert fallito per '{Agent}'", claims.AgentName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// L'agente interroga la propria memoria (+ shared) per topic. Ritorna solo enunciati e
        /// confidence: mai grafi di altri agenti (barriera nella query stessa).
        /// </summary>
        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] QueryMemoryRequest request)
        {
            var claims = ResolveClaims();
            if (claims == null)
                return Unauthorized(new { error = "RunToken assente o non valido." });

            var (resolved, error) = await ResolveAgentAndFusekiAsync(claims);
            if (resolved == null)
                return error;

            var topics = request?.Topics ?? new List<string>();
            var limit = request?.Limit ?? DefaultLimit;

            try
            {
                var facts = await _memory.QueryAsync(resolved.Conn, resolved.AgentIdentityId, topics, limit);
                return Ok(new
                {
                    facts = facts.Select(f => new
                    {
                        statement = f.Statement,
                        confidence = f.Confidence,
                        tags = f.Tags,
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
                _logger.LogError(ex, "[Memory] query fallita per '{Agent}'", claims.AgentName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ---- risoluzione identità + connessione Fuseki (tutto server-side) ----

        private sealed class Resolved
        {
            public Guid AgentIdentityId { get; set; }
            public FusekiConnection Conn { get; set; }
        }

        /// <summary>
        /// Risolve l'<c>AgentIdentity.Id</c> del chiamante (dal token, mai dal body) e le
        /// coordinate Fuseki del progetto. Ritorna null + un IActionResult d'errore se l'agente
        /// non è un cittadino o Fuseki non è abilitato.
        /// </summary>
        private async Task<(Resolved Resolved, IActionResult Error)> ResolveAgentAndFusekiAsync(RunTokenClaims claims)
        {
            var entry = _registry.RefreshCatalog(claims.ProjectPath)
                .FirstOrDefault(e => e.IsCitizen && string.Equals(e.Name, claims.AgentName, StringComparison.OrdinalIgnoreCase));
            if (entry == null || entry.IdentityId == null)
                return (null, NotFound(new { error = $"Identità dell'agente '{claims.AgentName}' non risolvibile: memoria non disponibile." }));

            FusekiConnection conn;
            try
            {
                conn = await _fusekiResolver.ResolveAsync(claims.ProjectPath);
            }
            catch (FusekiAddonMissingException ax)
            {
                return (null, StatusCode(409, new { error = ax.Message }));
            }
            if (conn == null)
                return (null, StatusCode(409, new { error = "La memoria (Fuseki) non è abilitata per questo progetto." }));

            return (new Resolved { AgentIdentityId = entry.IdentityId.Value, Conn = conn }, null);
        }

        private static List<string> SplitTags(string csv)
            => string.IsNullOrWhiteSpace(csv)
                ? new List<string>()
                : csv.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList();

        private RunTokenClaims ResolveClaims()
        {
            var token = Request.Headers[RunTokenHeader].ToString();
            return string.IsNullOrWhiteSpace(token) ? null : _tokens.Validate(token);
        }
    }

    /// <summary>
    /// Corpo di <c>assert</c>. Campi nullable (memoria dto_nullable_implicit_required): la
    /// validazione automatica risponderebbe 400 prima del check del token.
    /// </summary>
    public class AssertFactRequest
    {
        /// <summary>L'enunciato del fatto (specifico, verificabile, operativo).</summary>
        public string? Statement { get; set; }
        /// <summary>Tag dell'argomento, CSV (allineati ai topics dei messaggi).</summary>
        public string? About { get; set; }
        /// <summary>0..1; default 0.7. I fatti confermati da un umano tendono a 1.</summary>
        public double? Confidence { get; set; }
    }

    /// <summary>Corpo di <c>query</c>.</summary>
    public class QueryMemoryRequest
    {
        public List<string>? Topics { get; set; }
        public int? Limit { get; set; }
    }
}
