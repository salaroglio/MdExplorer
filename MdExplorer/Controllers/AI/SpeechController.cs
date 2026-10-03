using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.Speech;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// La dettatura: la pagina registra, qui si trascrive.
    /// <para>
    /// Il confine è voluto. Il browser fa la sola cosa che solo lui può fare — aprire il microfono — e
    /// manda un WAV; il riconoscimento sta nel Service, dove funziona uguale su Windows e su Linux e da
    /// dove l'audio non esce. Vedi docs-internal/Sprints/2026-09-22-Dettatura-Vocale-MarkAgent.md.
    /// </para>
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SpeechController : ControllerBase
    {
        /// <summary>
        /// Tetto alla dimensione dell'audio accettato. A 16 kHz mono in PCM a 16 bit un minuto pesa
        /// ~1,9 MB: 25 MB sono circa 13 minuti, molto oltre una dettatura, ma abbastanza da non
        /// rifiutare la registrazione lunga di chi detta un paragrafo intero.
        /// </summary>
        private const long MaxAudioBytes = 25L * 1024 * 1024;

        private readonly ISpeechToTextService _speech;
        private readonly ILogger<SpeechController> _logger;

        public SpeechController(ISpeechToTextService speech, ILogger<SpeechController> logger)
        {
            _speech = speech;
            _logger = logger;
        }

        /// <summary>
        /// Se la dettatura è utilizzabile adesso, e con quale modello. La UI la chiama per decidere se
        /// il microfono si accende o se va detto «scarica il modello».
        /// </summary>
        [HttpGet("status")]
        public IActionResult Status()
        {
            var modello = _speech.InstalledModelName;
            return Ok(new
            {
                available = modello != null,
                model = modello,
                // Il formato non è una preferenza: Whisper accetta solo questo.
                expects = new { format = "wav", sampleRate = 16000, channels = 1 }
            });
        }

        /// <summary>
        /// Trascrive l'audio registrato dalla pagina. <c>audio</c> deve essere un WAV mono a 16 kHz.
        /// </summary>
        [HttpPost("transcribe")]
        [RequestSizeLimit(MaxAudioBytes)]
        public async Task<IActionResult> Transcribe(IFormFile audio, [FromQuery] string language = "it",
            CancellationToken ct = default)
        {
            if (audio == null || audio.Length == 0)
            {
                return BadRequest(new { error = "Nessun audio ricevuto: il campo 'audio' è vuoto." });
            }

            try
            {
                await using var stream = audio.OpenReadStream();
                // Whisper.net legge lo stream più volte (header, poi campioni): un MemoryStream evita
                // di dipendere dalla seekability di quello che ASP.NET consegna.
                using var memoria = new MemoryStream();
                await stream.CopyToAsync(memoria, ct);
                memoria.Position = 0;

                var esito = await _speech.TranscribeAsync(memoria, language, ct);
                return Ok(new { text = esito.Text, milliseconds = esito.Milliseconds, model = esito.Model });
            }
            catch (SpeechModelMissingException ex)
            {
                // 409: la richiesta è giusta, manca un presupposto che l'utente può soddisfare.
                _logger.LogInformation("[Dettatura] rifiutata: {Messaggio}", ex.Message);
                return Conflict(new { error = ex.Message, needsModel = true });
            }
            catch (SpeechAudioFormatException ex)
            {
                _logger.LogWarning("[Dettatura] formato audio rifiutato: {Messaggio}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (OperationCanceledException)
            {
                // Chi ha chiuso la chat mentre trascriveva non ha bisogno di una risposta.
                return new StatusCodeResult(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Dettatura] trascrizione fallita");
                return StatusCode(500, new { error = "La trascrizione non è riuscita: " + ex.Message });
            }
        }
    }
}
