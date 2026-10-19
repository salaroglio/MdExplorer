using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.SourceMapping;
using MdExplorer.Features.Slides;
using MdExplorer.Services.Templates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.Templates
{
    /// <summary>
    /// La vetrina di «Crea nuovo progetto» (sprint 2026-10-19-Vetrina-Dei-Template.md): che cosa è un template e le sue slide,
    /// prima di sceglierlo. Le pagine delle slide stanno sotto il percorso della copia in cache, così le immagini relative
    /// della presentazione si risolvono da sole contro lo stesso indirizzo.
    /// </summary>
    [ApiController]
    [Route("api/templateshowcase")]
    public class TemplateShowcaseController : ControllerBase
    {
        private static readonly FileExtensionContentTypeProvider ContentTypes = new();
        private readonly ITemplateShowcase _showcase;
        private readonly ILogger<TemplateShowcaseController> _logger;

        public TemplateShowcaseController(ITemplateShowcase showcase, ILogger<TemplateShowcaseController> logger)
        {
            _showcase = showcase;
            _logger = logger;
        }

        /// <summary>Il template all'indirizzo <paramref name="url"/>, nella lingua scelta. Irraggiungibile: 502 con il motivo (VD3).</summary>
        [HttpGet("describe")]
        public async Task<IActionResult> Describe([FromQuery] string url, [FromQuery] string language, CancellationToken ct)
        {
            try
            {
                return Ok(await _showcase.DescribeAsync(url, language, ct));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("[Vetrina] {Url} ({Language}): {Message}", url, language, ex.Message);
                return StatusCode((int)HttpStatusCode.BadGateway, new { error = ex.Message });
            }
        }

        /// <summary>Una presentazione del template (resa come slide) o un suo file (immagini), dalla copia in cache.</summary>
        [HttpGet("deck/{hash}/{tip}/{**path}")]
        public IActionResult Deck(string hash, string tip, string path, [FromQuery] string? theme)
        {
            var file = _showcase.FileOf($"{hash}/{tip}", path);
            if (file == null) return NotFound();
            if (!file.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                return PhysicalFile(file, ContentTypes.TryGetContentType(file, out var type) ? type : "application/octet-stream");
            return Page(System.IO.File.ReadAllText(file), Path.GetFileName(file), theme);
        }

        /// <summary>La slide di circostanza di un template senza presentazione (VD8).</summary>
        [HttpGet("standin/{hash}/{tip}")]
        public IActionResult StandIn(string hash, string tip, [FromQuery] string language, [FromQuery] string? theme)
        {
            try
            {
                return Page(_showcase.StandInDeck($"{hash}/{tip}", language), "template", theme);
            }
            catch (InvalidOperationException ex)
            {
                return Content(ErrorPage("template", ex.Message), "text/html");
            }
        }

        /// <summary>La presentazione del progetto vuoto (VD4), che viaggia dentro MDE.</summary>
        [HttpGet("empty")]
        public IActionResult Empty([FromQuery] string language, [FromQuery] string? theme)
            => Page(_showcase.EmptyProjectDeck(language), "empty", theme);

        private ContentResult Page(string markdown, string name, string? theme)
        {
            try
            {
                // Una pagina che mostra e basta: senza gli script che correggono le slide o interrogano il servizio.
                return Content(SlideDeckRenderer.Render(markdown, new SlideDeckRenderOptions
                {
                    Pipeline = DocumentViewPipeline.Build(null),
                    DarkTheme = theme == "dark" || theme == "milan",
                    StaticExport = true,
                }), "text/html");
            }
            catch (SlideDeckException ex)
            {
                _logger.LogWarning("[Vetrina] {Name}: {Message}", name, ex.Message);
                return Content(ErrorPage(name, ex.Message), "text/html");
            }
        }

        private static string ErrorPage(string name, string message) => $@"<!DOCTYPE html>
<html><head><meta charset=""utf-8""><title>{WebUtility.HtmlEncode(name)}</title></head>
<body style=""font-family: sans-serif; padding: 2em; line-height: 1.5"">
<p>{WebUtility.HtmlEncode(message)}</p>
</body></html>";
    }
}
