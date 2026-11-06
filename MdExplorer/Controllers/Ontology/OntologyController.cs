using System;
using System.Collections.Generic;
using MdExplorer.Features.Ontology;
using MdExplorer.Features.Services;
using Microsoft.AspNetCore.Mvc;

namespace MdExplorer.Controllers.Ontology
{
    /// <summary>
    /// La verifica di un'ontologia sui file del progetto (P6 dei template di progetto): la usa lo strumento MCP
    /// <c>rdf_check</c>, per chi scrive TBox, ABox, regole SHACL e domande di competenza. In memoria, senza Fuseki.
    /// </summary>
    [ApiController]
    [Route("api/ontology")]
    public class OntologyController : ControllerBase
    {
        [HttpPost("check")]
        public IActionResult Check([FromBody] RdfCheckRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ProjectPath))
                return BadRequest(new { error = UiText.Say("projectPath è obbligatorio.", "projectPath is required.") });
            try
            {
                return Ok(RdfCheck.Check(request.ProjectPath, request.Data, request.Shapes, request.Queries));
            }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        }
    }

    public class RdfCheckRequest
    {
        public string? ProjectPath { get; set; }
        /// <summary>File <c>.ttl</c> o cartelle, dalla radice del progetto: schema e istanze.</summary>
        public List<string>? Data { get; set; }
        /// <summary>File <c>.ttl</c> o cartelle con le regole SHACL.</summary>
        public List<string>? Shapes { get; set; }
        /// <summary>File <c>.rq</c> o cartelle: le domande di competenza.</summary>
        public List<string>? Queries { get; set; }
    }
}
