using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Abstractions.DB;
using MdExplorer.Features.Commands;
using MdExplorer.Features.Diagrams;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.Ocr;
using MdExplorer.Features.Utilities;
using MdExplorer.Hubs;
using MdExplorer.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.MarkActions
{
    public interface IDocumentTidyJobService
    {
        /// <summary>
        /// Sistema le immagini di un documento appena convertito e lo racconta a Mark, un'immagine alla volta, sul
        /// canale <c>documentTidyProgress</c>. Un lavoro per connessione; il controllo «già in corso» è sincrono.
        /// </summary>
        Task RunAsync(string connectionId, string documentFullPath, string projectPath, CancellationToken ct = default);

        void Cancel(string connectionId);
    }

    /// <summary>
    /// Il secondo passo dopo la conversione di un Word (sprint 2026-11-17-Mark-Sistema-Il-Documento-Convertito): il
    /// motore del progetto guarda ogni immagine e dice che cos'è; MdExplorer verifica e scrive. Uno schema ridisegnato
    /// in PlantUML che passa la verifica prende il posto dell'immagine, le altre ricevono una descrizione ripiegata.
    /// <para>
    /// Ogni immagine è una domanda a sé in una sessione di sola lettura (<see cref="LlmSessions.OneShotReadOnlySession"/>):
    /// il modello non scrive mai nel documento. Il documento cambia subito, immagine per immagine, e Mark lo commenta
    /// (utente, 17/11/2026: «commenti strada facendo, senza aspettare l'autorizzazione»).
    /// </para>
    /// </summary>
    public class DocumentTidyJobService : IDocumentTidyJobService
    {
        private const string ProgressEvent = "documentTidyProgress";
        private const string PromptName = "mde-doc-tidy";
        private const string RedrawPromptName = "mde-doc-tidy-redraw";

        private readonly ILogger<DocumentTidyJobService> _logger;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly LlmSessions.OneShotReadOnlySession _llm;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly PlantumlServer _plantuml;

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();

        public DocumentTidyJobService(
            ILogger<DocumentTidyJobService> logger,
            IHubContext<MonitorMDHub> hubContext,
            LlmSessions.OneShotReadOnlySession llm,
            IServiceScopeFactory scopeFactory,
            PlantumlServer plantuml)
        {
            _logger = logger;
            _hubContext = hubContext;
            _llm = llm;
            _scopeFactory = scopeFactory;
            _plantuml = plantuml;
        }

        // Non async di proposito: il controllo «già in corso» deve arrivare al controller prima della risposta.
        public Task RunAsync(string connectionId, string documentFullPath, string projectPath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
                throw new ArgumentException("connectionId is required", nameof(connectionId));

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (!_running.TryAdd(connectionId, cts))
            {
                cts.Dispose();
                throw new InvalidOperationException(UiText.Say(
                    "Mark sta già sistemando un documento per questa sessione.",
                    "Mark is already tidying a document for this session."));
            }
            return RunInternalAsync(connectionId, documentFullPath, projectPath, cts);
        }

        public void Cancel(string connectionId)
        {
            if (!string.IsNullOrWhiteSpace(connectionId) && _running.TryGetValue(connectionId, out var cts))
            {
                try { cts.Cancel(); }
                catch (ObjectDisposedException) { /* già finito */ }
            }
        }

        private async Task RunInternalAsync(string connectionId, string documentFullPath, string projectPath, CancellationTokenSource cts)
        {
            var ct = cts.Token;
            var documentName = Path.GetFileName(documentFullPath);
            int redrawn = 0, toCheck = 0, described = 0, transcribed = 0, captioned = 0, skipped = 0, failed = 0;
            try
            {
                if (PathContainment.RelativeOrNull(projectPath, documentFullPath) == null || !File.Exists(documentFullPath))
                    throw new FileNotFoundException(UiText.Say($"Il documento non c'è: {documentFullPath}", $"The document does not exist: {documentFullPath}"));

                var todo = DocumentImageTidier.PendingImages(await File.ReadAllTextAsync(documentFullPath, ct));
                if (todo.Count == 0)
                {
                    await SendAsync(connectionId, new { phase = "nothing", document = documentName });
                    return;
                }

                MarkAgentEngine engine;
                string model;
                using (var scope = _scopeFactory.CreateScope())
                    (engine, model) = MdExplorer.Service.ProjectsManager.ProjectEngine(scope.ServiceProvider.GetService<IUserSettingsDB>(), projectPath);
                var whyNot = LlmSessions.ReadOnlyEngineSessions.WhyNot(engine);
                if (whyNot != null)
                {
                    // Senza motore Mark non compare: il documento convertito resta com'è.
                    await SendAsync(connectionId, new { phase = "no-engine", document = documentName, detail = whyNot });
                    _logger.LogInformation("[DocumentTidy] {Document}: {Why}", documentName, whyNot);
                    return;
                }
                // Il fumetto del pensiero di Mark: le tappe di MdExplorer e il ragionamento del modello, una riga alla volta
                // e nell'ordine in cui nascono (gli invii si accodano: il motore chiama da un suo thread).
                var thinkingLock = new object();
                var thinkingSends = Task.CompletedTask;
                void Think(string line)
                {
                    lock (thinkingLock)
                        thinkingSends = thinkingSends.ContinueWith(_ => SendAsync(connectionId, new { phase = "thinking", text = line }),
                            CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default).Unwrap();
                }
                Task ThinkingSent() { lock (thinkingLock) return thinkingSends; }

                async Task<string> Ask(string prompt)
                {
                    var reasoning = new ThinkingLines(Think);
                    try
                    {
                        return await _llm.AskAsync(projectPath, engine, model, prompt, "document-tidy",
                            "le immagini del documento si guardano soltanto: nel documento scrive MdExplorer", ct,
                            onActivity: (kind, text) =>
                            {
                                if (kind == LlmSessions.OneShotReadOnlySession.ActivityThinking) reasoning.Add(text);
                                else { reasoning.Flush(); Think(text); }
                            });
                    }
                    finally { reasoning.Flush(); }
                }

                var tidier = new DocumentImageTidier(documentFullPath, projectPath, LoadPromptBody(projectPath, PromptName), LoadPromptBody(projectPath, RedrawPromptName), Ask,
                    new PlantumlBlockVerifier(_plantuml.CheckAsync)) { OnStage = Think };

                await SendAsync(connectionId, new
                {
                    phase = "started",
                    document = documentName,
                    total = todo.Count,
                    provider = LlmSessions.ReadOnlyEngineSessions.Label(engine, model),
                    text = todo.Count == 1
                        ? UiText.Say($"Ho convertito {documentName}. C'è un'immagine: la guardo e ti dico.", $"I converted {documentName}. There is one image: I'll look at it and tell you.")
                        : UiText.Say($"Ho convertito {documentName}. Ci sono {todo.Count} immagini: le guardo una a una e ti dico.", $"I converted {documentName}. There are {todo.Count} images: I'll look at them one by one and tell you."),
                });

                for (var i = 0; i < todo.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var path = todo[i];
                    var label = UiText.Say($"Immagine {i + 1} di {todo.Count}", $"Image {i + 1} of {todo.Count}");
                    await SendAsync(connectionId, new { phase = "looking", index = i + 1, total = todo.Count, image = path });

                    string action, comment;
                    try
                    {
                        (action, comment) = await tidier.TidyAsync(path, ct);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[DocumentTidy] {Document}: immagine {Image} non sistemata", documentName, path);
                        action = DocumentImageTidier.Failed;
                        comment = UiText.Say($"non sono riuscito a sistemarla: {ex.Message}", $"I could not tidy it: {ex.Message}");
                    }

                    switch (action)
                    {
                        case DocumentImageTidier.Redrawn: redrawn++; break;
                        case DocumentImageTidier.RedrawnToCheck: toCheck++; break;
                        case DocumentImageTidier.Described: described++; break;
                        case DocumentImageTidier.Transcribed: transcribed++; break;
                        case DocumentImageTidier.Captioned: captioned++; break;
                        case DocumentImageTidier.Skipped: skipped++; break;
                        default: failed++; break;
                    }
                    // Il commento arriva dopo l'ultima riga di pensiero, mai prima.
                    try { await ThinkingSent(); } catch (Exception ex) { _logger.LogDebug(ex, "[DocumentTidy] una riga di pensiero non è partita"); }
                    // Una riga per immagine: è da qui che si capisce perché uno schema non è stato ridisegnato.
                    _logger.LogInformation("[DocumentTidy] {Document}: {Image} -> {Action}: {Comment}", documentName, path, action, comment);
                    await SendAsync(connectionId, new { phase = "comment", index = i + 1, total = todo.Count, image = path, action, text = $"{label}: {comment}" });
                }

                await SendAsync(connectionId, new
                {
                    phase = "done",
                    document = documentName,
                    redrawn,
                    toCheck,
                    described,
                    transcribed,
                    captioned,
                    skipped,
                    failed,
                    text = Summary(redrawn, toCheck, described, transcribed, captioned, skipped, failed),
                });
                _logger.LogInformation("[DocumentTidy] {Document}: ridisegnate {R}, ridisegnate da controllare {K}, descritte {D}, trascritte {T}, con testo alternativo {C}, lasciate {S}, non riuscite {F}",
                    documentName, redrawn, toCheck, described, transcribed, captioned, skipped, failed);
            }
            catch (OperationCanceledException)
            {
                await SendAsync(connectionId, new { phase = "cancelled", document = documentName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DocumentTidy] {Document} non sistemato", documentName);
                await SendAsync(connectionId, new { phase = "error", document = documentName, message = ex.Message });
            }
            finally
            {
                _running.TryRemove(connectionId, out _);
                cts.Dispose();
            }
        }

        private static string Summary(int redrawn, int toCheck, int described, int transcribed, int captioned, int skipped, int failed)
        {
            var parts = new List<string>();
            if (redrawn > 0) parts.Add(UiText.Say($"{redrawn} ridisegnate in PlantUML", $"{redrawn} redrawn in PlantUML"));
            if (toCheck > 0) parts.Add(UiText.Say($"{toCheck} ridisegnate dalla descrizione, da controllare", $"{toCheck} redrawn from the description, to be checked"));
            if (described > 0) parts.Add(UiText.Say($"{described} descritte", $"{described} described"));
            if (transcribed > 0) parts.Add(UiText.Say($"{transcribed} trascritte", $"{transcribed} transcribed"));
            if (captioned > 0) parts.Add(UiText.Say($"{captioned} con il testo alternativo", $"{captioned} given an alternative text"));
            if (skipped > 0) parts.Add(UiText.Say($"{skipped} lasciate com'erano", $"{skipped} left as they were"));
            if (failed > 0) parts.Add(UiText.Say($"{failed} non riuscite", $"{failed} not done"));
            return UiText.Say("Ho finito con le immagini: ", "I am done with the images: ") + string.Join(", ", parts) + ".";
        }

        // Come MarkFolderJobService: il prompt distribuito da MdeSkillUpdater, senza intestazione né commento.
        private static string LoadPromptBody(string projectPath, string name)
        {
            var text = File.ReadAllText(MdeAssetResolver.PromptFullPath(projectPath, name));
            var frontMatter = Regex.Match(text, @"^---[ \t]*\r?\n.*?\r?\n---[ \t]*\r?\n", RegexOptions.Singleline);
            if (frontMatter.Success && frontMatter.Index == 0) text = text.Substring(frontMatter.Length);
            return Regex.Replace(text, @"<!--.*?-->", string.Empty, RegexOptions.Singleline).Trim();
        }

        private Task SendAsync(string connectionId, object payload)
            => _hubContext.Clients.Client(connectionId).SendAsync(ProgressEvent, payload);
    }
}
