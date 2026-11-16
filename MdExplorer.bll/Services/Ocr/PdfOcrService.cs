using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.Ocr
{
    public class PdfOcrResult
    {
        public string Markdown { get; set; }
        public int Pages { get; set; }

        /// <summary>
        /// Le pagine (da 1) che il modello non ha finito di scrivere: nel markdown ci sono, ma
        /// incomplete. Chi mostra l'esito le deve nominare.
        /// </summary>
        public List<int> TruncatedPages { get; set; } = new List<int>();
    }

    public interface IPdfOcrService
    {
        /// <param name="pageDone">Chiamata dopo ogni pagina con (pagina letta, pagine totali).</param>
        Task<PdfOcrResult> ConvertAsync(string pdfPath, string modelId, Action<int, int> pageDone, CancellationToken ct);
    }

    /// <summary>Un pdf in markdown: ogni pagina diventa un'immagine, il modello OCR la legge.</summary>
    public class PdfOcrService : IPdfOcrService
    {
        /// <summary>
        /// Quanto può scrivere il modello per una pagina. Una pagina A4 fitta sta sotto i 2000 token;
        /// il tetto serve a fermare un modello che si incanta a ripetere la stessa riga.
        /// </summary>
        public const int MaxTokensPerPage = 4096;

        /// <summary>
        /// Il prompt «Document» della scheda di Xiaomi-OCR-0, parola per parola: il modello è
        /// addestrato su questa frase, riscriverla peggiora il risultato.
        /// </summary>
        private const string DocumentPrompt =
            "Extract all information from the main body of the document image and represent it in markdown format, " +
            "ignoring headers and footers. Tables should be expressed in OTSL format, formulas in the document should be " +
            "represented using LATEX format, and the parsing should be organized according to the reading order.";

        private readonly IModelDownloadService _models;
        private readonly IOcrEngine _engine;
        private readonly ILogger<PdfOcrService> _logger;

        public PdfOcrService(IModelDownloadService models, IOcrEngine engine, ILogger<PdfOcrService> logger)
        {
            _models = models;
            _engine = engine;
            _logger = logger;
        }

        public async Task<PdfOcrResult> ConvertAsync(string pdfPath, string modelId, Action<int, int> pageDone, CancellationToken ct)
        {
            var model = _models.FindModel(modelId);
            if (model == null || model.Category != "ocr")
            {
                throw new ArgumentException(UiText.Say(
                    $"«{modelId}» non è un modello OCR del catalogo.", $"\"{modelId}\" is not an OCR model of the catalog."));
            }
            if (!model.IsInstalled)
            {
                throw new OcrNotReadyException(UiText.Say(
                    $"Il modello OCR «{model.Name}» non è installato: scaricalo dal passo «Conversione in markdown».",
                    $"The OCR model \"{model.Name}\" is not installed: download it from the \"Convert to markdown\" step."));
            }

            var total = PdfPageRenderer.CountPages(pdfPath);
            var result = new PdfOcrResult { Pages = total };
            var pages = new List<string>();

            await using (var session = await _engine.StartAsync(
                model.LocalPath, _models.GetModelPath(model.Companions.Single().FileName), model.ContextLength, ct))
            {
                var number = 0;
                foreach (var png in PdfPageRenderer.RenderPages(pdfPath))
                {
                    ct.ThrowIfCancellationRequested();
                    number++;
                    var answer = await session.ReadPageAsync(png, DocumentPrompt, ct);
                    if (answer.Truncated)
                    {
                        result.TruncatedPages.Add(number);
                        _logger.LogWarning("[OCR] {Pdf}: pagina {Page} interrotta al tetto di {Max} token", pdfPath, number, MaxTokensPerPage);
                    }
                    pages.Add(OcrMarkdown.FromPage(answer.Text));
                    pageDone?.Invoke(number, total);
                }
            }

            result.Markdown = OcrMarkdown.JoinPages(pages);
            return result;
        }
    }
}
