using MdExplorer.Features.snippets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Controllers.Entities
{
    [ApiController]
    [Route("/api/entities/snippets")]
    public class SnippetsController : ControllerBase
    {
        private readonly ISnippet<DictionarySnippetParam>[] _snippets;

        public SnippetsController(ISnippet<DictionarySnippetParam>[] snippets)
        {
            _snippets = snippets;
        }
        [HttpGet]
        public IActionResult GetSnippetList()
        {
            var toReturn = _snippets.Select(_ => new { Id = _.Id, Name = _.Name });
            return Ok(toReturn);
        }

    }
}
