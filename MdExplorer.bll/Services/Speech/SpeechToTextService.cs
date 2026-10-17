using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Whisper.net;

namespace MdExplorer.Features.Services.Speech
{
    /// <summary>Il testo di una dettatura, e quanto è costato produrlo.</summary>
    public sealed record Transcription(string Text, long Milliseconds, string Model);

    /// <summary>
    /// Da voce a testo, in locale.
    /// <para>
    /// Sta nel Service e non nel browser per tre ragioni decise con l'utente il 22/09/2026: funziona
    /// uguale su Windows e su Linux, l'audio <b>non esce dalla macchina</b>, e la stessa capacità serve
    /// oltre il pulsante della chat (dettare nell'editor, buttare una registrazione dentro un `.md`).
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-22-Dettatura-Vocale-MarkAgent.md.</para>
    /// </summary>
    public interface ISpeechToTextService
    {
        /// <summary>Il modello installato, o <c>null</c>: serve alla UI per dire «scaricalo» prima di provarci.</summary>
        string InstalledModelName { get; }

        /// <summary>
        /// Trascrive un WAV <b>a 16 kHz</b>. Solleva <see cref="SpeechModelMissingException"/> se non c'è
        /// un modello, e <see cref="SpeechAudioFormatException"/> se l'audio non è nel formato giusto.
        /// </summary>
        Task<Transcription> TranscribeAsync(Stream wav16k, string language, CancellationToken ct = default);
    }

    /// <summary>Non c'è nessun modello di dettatura installato. Non è un errore tecnico: è una cosa da fare.</summary>
    public sealed class SpeechModelMissingException : Exception
    {
        public SpeechModelMissingException(string message) : base(message) { }
    }

    /// <summary>L'audio arrivato non è un WAV a 16 kHz, l'unico che Whisper accetta.</summary>
    public sealed class SpeechAudioFormatException : Exception
    {
        public SpeechAudioFormatException(string message, Exception inner = null) : base(message, inner) { }
    }

    /// <summary>
    /// Implementazione con Whisper.net (whisper.cpp), MIT, binari nativi già dentro il pacchetto per
    /// Windows, Linux e macOS: nessun download a runtime, a differenza di llama.cpp.
    /// </summary>
    public sealed class SpeechToTextService : ISpeechToTextService, IDisposable
    {
        /// <summary>
        /// Quanto il modello resta in memoria dopo l'ultimo uso.
        /// <para>
        /// Caricarlo costa poco (217 ms misurati per <c>small</c>), tenerlo costa ~500 MB residenti: per
        /// un'app desktop è tanto da lasciare lì per una dettatura di dieci secondi. Chi detta più frasi
        /// di fila però non deve pagare il caricamento ogni volta — da cui la finestra di grazia.
        /// </para>
        /// </summary>
        private static readonly TimeSpan IdleUnloadAfter = TimeSpan.FromMinutes(3);

        /// <summary>
        /// Quanti thread dare a whisper.cpp. Uno in meno dei core, per lasciare respiro alla UI.
        /// <para>
        /// Non è un dettaglio: il default della libreria (4) faceva 4,4 s per una frase di 6 secondi,
        /// con 10 thread ne fa 2,9 (misurato il 22/09/2026, Core Ultra 7 155H, modello <c>small</c>).
        /// E il tempo <b>non</b> dipende da quanto si è parlato: whisper lavora a finestre di 30
        /// secondi, quindi una frase corta costa quasi come una lunga — l'attesa che si taglia qui è
        /// quella di <b>ogni</b> dettatura.
        /// </para>
        /// </summary>
        private static int Threads => Math.Max(1, Environment.ProcessorCount - 1);

        private readonly IModelDownloadService _models;
        private readonly ILogger<SpeechToTextService> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private WhisperFactory _factory;
        private string _factoryModelPath;
        private Timer _unloadTimer;
        private bool _disposed;

        public SpeechToTextService(IModelDownloadService models, ILogger<SpeechToTextService> logger)
        {
            _models = models;
            _logger = logger;
        }

        public string InstalledModelName
        {
            get
            {
                var path = _models.GetInstalledSpeechModelPath();
                return path == null ? null : Path.GetFileName(path);
            }
        }

        public async Task<Transcription> TranscribeAsync(Stream wav16k, string language, CancellationToken ct = default)
        {
            var modelPath = _models.GetInstalledSpeechModelPath()
                ?? throw new SpeechModelMissingException(
                    "Nessun modello di dettatura installato. Scarica «Whisper Small (dettatura)» dal gestore " +
                    "dei modelli e riprova.");

            // Una trascrizione alla volta: whisper.cpp usa già tutti i core, e due richieste in
            // parallelo si toglierebbero CPU a vicenda raddoppiando l'attesa di entrambe.
            await _gate.WaitAsync(ct);
            try
            {
                var factory = EnsureFactory(modelPath);
                using var processor = factory.CreateBuilder()
                    .WithLanguage(string.IsNullOrWhiteSpace(language) ? "auto" : language)
                    .WithThreads(Threads)
                    .Build();

                var cronometro = System.Diagnostics.Stopwatch.StartNew();
                var testo = new StringBuilder();
                try
                {
                    await foreach (var segmento in processor.ProcessAsync(wav16k, ct))
                    {
                        testo.Append(segmento.Text);
                    }
                }
                catch (Exception ex) when (ex.GetType().Name.Contains("Wave"))
                {
                    // Whisper.net alza NotSupportedWaveException su tutto ciò che non è WAV a 16 kHz.
                    // Il tipo è interno alla sua gerarchia: si riconosce dal nome invece di legarsi a
                    // una classe che potrebbe cambiare, e si traduce in una frase che dice cosa fare.
                    throw new SpeechAudioFormatException(
                        "L'audio non è un WAV mono a 16 kHz, l'unico formato che il riconoscimento accetta. " +
                        "Chi registra deve campionare a 16 kHz (nel browser: `new AudioContext({ sampleRate: 16000 })`).",
                        ex);
                }
                cronometro.Stop();

                RestartUnloadTimer();

                var risultato = testo.ToString().Trim();
                _logger.LogInformation("[Dettatura] {Ms} ms con {Modello}, {Caratteri} caratteri",
                    cronometro.ElapsedMilliseconds, Path.GetFileName(modelPath), risultato.Length);

                return new Transcription(risultato, cronometro.ElapsedMilliseconds, Path.GetFileName(modelPath));
            }
            finally
            {
                _gate.Release();
            }
        }

        /// <summary>Il modello caricato, ricaricandolo se nel frattempo è cambiato o è stato scaricato dalla memoria.</summary>
        private WhisperFactory EnsureFactory(string modelPath)
        {
            if (_factory != null && _factoryModelPath == modelPath)
            {
                return _factory;
            }

            _factory?.Dispose();
            var cronometro = System.Diagnostics.Stopwatch.StartNew();
            _factory = WhisperFactory.FromPath(modelPath);
            _factoryModelPath = modelPath;
            _logger.LogInformation("[Dettatura] modello {Modello} caricato in {Ms} ms",
                Path.GetFileName(modelPath), cronometro.ElapsedMilliseconds);
            return _factory;
        }

        private void RestartUnloadTimer()
        {
            _unloadTimer?.Dispose();
            _unloadTimer = new Timer(_ => Unload(), null, IdleUnloadAfter, Timeout.InfiniteTimeSpan);
        }

        private void Unload()
        {
            // Se una trascrizione è in corso il turno non si prende e si riprova al prossimo giro:
            // scaricare il modello sotto i piedi di chi lo sta usando sarebbe un crash nativo.
            if (!_gate.Wait(TimeSpan.Zero)) { RestartUnloadTimer(); return; }
            try
            {
                if (_factory != null)
                {
                    _factory.Dispose();
                    _factory = null;
                    _factoryModelPath = null;
                    _logger.LogInformation("[Dettatura] modello scaricato dalla memoria dopo {Minuti} minuti di inattività",
                        IdleUnloadAfter.TotalMinutes);
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _unloadTimer?.Dispose();
            _factory?.Dispose();
            _gate.Dispose();
        }
    }
}
