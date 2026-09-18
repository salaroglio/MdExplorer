using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models.AI;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services.AI;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.MarkDiagram
{
    /// <summary>
    /// See <see cref="IMarkDiagramExplainService"/>.
    ///
    /// Streams the answer chunk by chunk: Mark's dialog is a typewriter, and the
    /// model's own pace makes a better typewriter than a fixed character delay.
    /// </summary>
    public class MarkDiagramExplainService : IMarkDiagramExplainService
    {
        private const string StreamEvent = "markDiagramExplain";
        private const string DefaultProviderKey = "AI_DefaultProvider";
        private const string DefaultModelKey = "AI_DefaultModel";

        private readonly ILogger<MarkDiagramExplainService> _logger;
        private readonly IHubContext<MonitorMDHub> _hubContext;
        private readonly IEnumerable<IAiProvider> _aiProviders;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>One explanation in flight per connection: a new box supersedes the old one.</summary>
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _running = new();

        /// <summary>
        /// Conversazione aperta per connessione: quale box, quale sessione del CLI.
        /// Un box nuovo apre una sessione nuova — la spiegazione riparte da zero, come
        /// deciso; le domande di seguito invece restano dentro quella sessione.
        /// Volatile come il resto: chiuso il documento, non resta niente.
        /// </summary>
        private readonly ConcurrentDictionary<string, DiagramConversation> _conversations = new();

        private sealed record DiagramConversation(
            MarkDiagramContextDto Context,
            string ProjectPath,
            string SessionId);

        /// <summary>
        /// Proposta di modifica in attesa di conferma, per connessione. Vive qui e non nel
        /// client perché è il backend ad applicarla: il client conferma, non trasporta il
        /// contenuto — così quello che viene scritto è esattamente quello che è stato
        /// mostrato, e non qualcosa che ha fatto un viaggio in più.
        /// </summary>
        private readonly ConcurrentDictionary<string, MarkDiagramEditProposal> _pendingEdits = new();

        public MarkDiagramExplainService(
            ILogger<MarkDiagramExplainService> logger,
            IHubContext<MonitorMDHub> hubContext,
            IEnumerable<IAiProvider> aiProviders,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _hubContext = hubContext;
            _aiProviders = aiProviders;
            _scopeFactory = scopeFactory;
        }

        public async Task ExplainBoxAsync(
            string connectionId,
            MarkDiagramContextDto context,
            string projectPath,
            CancellationToken ct = default)
        {
            // Supersede the previous request for this connection: the user clicked
            // another box, the old answer is already stale on screen.
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (_running.TryRemove(connectionId, out var previous))
            {
                try { previous.Cancel(); } catch { /* already gone */ }
                previous.Dispose();
            }
            _running[connectionId] = cts;

            var boxName = context?.Box?.Name;

            // Sessione nuova per ogni box: le spiegazioni non si contaminano fra loro.
            var sessionId = Guid.NewGuid().ToString();
            _conversations[connectionId] = new DiagramConversation(context, projectPath, sessionId);

            // Una proposta lasciata in sospeso non deve sopravvivere: l'utente ha cambiato
            // box, quindi non e' piu' quella che gli era stata mostrata. Una conferma che
            // arrivasse dopo applicherebbe qualcosa che nessuno sta piu' guardando.
            _pendingEdits.TryRemove(connectionId, out _);

            try
            {
                await SendAsync(connectionId, new { phase = "start", box = boxName });

                await SendStatusAsync(connectionId, boxName, "Cerco il motore AI configurato...");

                var provider = ResolveConfiguredProvider(projectPath, out var modelId, out var whyNot);
                if (provider == null)
                {
                    // No silent fallback to "some other provider that happens to work":
                    // the user configured a reference LLM, or did not. Say which.
                    await SendAsync(connectionId, new { phase = "error", box = boxName, message = whyNot });
                    return;
                }

                var engineLabel = string.IsNullOrWhiteSpace(modelId)
                    ? provider.GetName()
                    : $"{provider.GetName()} ({modelId})";

                var documentName = System.IO.Path.GetFileName(context?.DocumentPath ?? string.Empty);
                await SendStatusAsync(connectionId, boxName,
                    string.IsNullOrWhiteSpace(documentName)
                        ? "Leggo il documento..."
                        : $"Leggo {documentName}...");

                var documentText = ReadDocument(context, projectPath, out var truncated);

                // Le regole vanno DENTRO il prompt, non passate da SetSystemPromptAsync.
                //
                // Quel metodo sembra fare al caso nostro e invece fa due danni. Primo: il
                // valore che salva viene letto solo da ChatWithToolsAsync — StreamChatAsync,
                // che è la strada di MarkAgent, non lo guarda mai. Le regole finivano scritte
                // in un'impostazione e poi ignorate: nessun limite di dieci frasi, nessun
                // divieto di inventare, nessun elenco puntato mai arrivato al modello.
                // Secondo: quell'impostazione è GLOBALE (CopilotCli_SystemPrompt) ed è la
                // stessa che usa la chat AI dell'utente — ogni spiegazione di un box gliela
                // sovrascriveva in silenzio.
                //
                // Un prompt che non arriva è peggio di un prompt assente: si continua a
                // ritoccarlo credendo di cambiare qualcosa.
                var systemPrompt = MarkDiagramPromptBuilder.BuildSystemPrompt();
                var userPrompt = systemPrompt + "\n\n---\n\n"
                               + MarkDiagramPromptBuilder.BuildUserPrompt(context!, documentText, truncated);

                var relationCount = context?.Relations?.Count ?? 0;
                await SendStatusAsync(connectionId, boxName,
                    $"Chiedo a {engineLabel} di spiegare \"{boxName}\" " +
                    $"({relationCount} relazioni, {documentText.Length / 1000} KB di documento)...");

                var answer = new StringBuilder();
                await foreach (var chunk in StreamAsync(provider, userPrompt, modelId, sessionId, cts.Token))
                {
                    if (cts.Token.IsCancellationRequested) return;
                    if (string.IsNullOrEmpty(chunk)) continue;
                    answer.Append(chunk);
                    await SendAsync(connectionId, new { phase = "chunk", box = boxName, text = chunk });
                }

                var full = answer.ToString().Trim();

                // The ten-sentence rule is asked for in the prompt and checked here.
                // Deliberately NOT enforced by truncation: a reply cut mid-thought is
                // worse than a long one, and the fix belongs in the prompt.
                var sentences = MarkDiagramPromptBuilder.CountSentences(full);
                if (sentences > MarkDiagramPromptBuilder.MaxSentences)
                {
                    _logger.LogWarning(
                        "[MarkDiagram] Box '{Box}': the model answered with {Count} sentences, limit is {Max}",
                        boxName, sentences, MarkDiagramPromptBuilder.MaxSentences);
                }

                await SendAsync(connectionId, new { phase = "done", box = boxName, text = full, sentences });
            }
            catch (OperationCanceledException)
            {
                // Superseded by a newer box, or the user closed the document. Not an error.
                _logger.LogInformation("[MarkDiagram] Explanation for box '{Box}' cancelled", boxName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MarkDiagram] Explanation for box '{Box}' failed", boxName);
                await SendAsync(connectionId, new { phase = "error", box = boxName, message = ex.Message });
            }
            finally
            {
                if (_running.TryGetValue(connectionId, out var mine) && mine == cts)
                    _running.TryRemove(connectionId, out _);
                cts.Dispose();
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Domande di seguito
        // ─────────────────────────────────────────────────────────────────────

        public async Task<bool> AskFollowUpAsync(string connectionId, string question, CancellationToken ct = default)
        {
            if (!_conversations.TryGetValue(connectionId, out var conversation))
                return false;

            var boxName = conversation.Context?.Box?.Name;

            // Stessa ragione: una nuova domanda supera la proposta precedente.
            _pendingEdits.TryRemove(connectionId, out _);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (_running.TryRemove(connectionId, out var previous))
            {
                try { previous.Cancel(); } catch { /* già finita */ }
                previous.Dispose();
            }
            _running[connectionId] = cts;

            try
            {
                await SendAsync(connectionId, new { phase = "start", box = boxName });
                await SendStatusAsync(connectionId, boxName, "Riprendo il filo del discorso...");

                var provider = ResolveConfiguredProvider(conversation.ProjectPath, out var modelId, out var whyNot);
                if (provider == null)
                {
                    await SendAsync(connectionId, new { phase = "error", box = boxName, message = whyNot });
                    return true;
                }

                var engineLabel = string.IsNullOrWhiteSpace(modelId) ? provider.GetName() : $"{provider.GetName()} ({modelId})";
                await SendStatusAsync(connectionId, boxName, $"Chiedo a {engineLabel}...");

                // Nella sessione il modello ha ancora davanti diagramma, documento e
                // spiegazione appena data: si manda solo la domanda, non di nuovo tutto.
                // Il promemoria del limite invece va ripetuto — è la regola che il modello
                // dimentica per prima quando la conversazione si allunga.
                var followUpPrompt =
                    $"{question}\n\n" +
                    $"(Ricorda: stiamo parlando del box \"{boxName}\" del diagramma. " +
                    $"Rispondi in non più di {MarkDiagramPromptBuilder.MaxSentences} frasi, " +
                    "senza inventare ciò che il documento non dice.)\n\n" +
                    MarkDiagramPromptBuilder.BuildEditInstructions();

                var answer = new StringBuilder();
                await foreach (var chunk in StreamAsync(provider, followUpPrompt, modelId, conversation.SessionId, cts.Token))
                {
                    if (cts.Token.IsCancellationRequested) return true;
                    if (string.IsNullOrEmpty(chunk)) continue;
                    answer.Append(chunk);
                    await SendAsync(connectionId, new { phase = "chunk", box = boxName, text = chunk });
                }

                var full = answer.ToString().Trim();

                // Il modello ha proposto una modifica invece di rispondere?
                var proposal = TryParseProposal(full);
                if (proposal != null)
                {
                    proposal.OtherDocuments = FindOtherDocumentsMentioning(conversation, cts.Token);
                    _pendingEdits[connectionId] = proposal;

                    await SendAsync(connectionId, new
                    {
                        phase = "proposal",
                        box = boxName,
                        summary = proposal.Summary,
                        changesDiagram = !string.IsNullOrWhiteSpace(proposal.NewPlantuml),
                        textEdits = proposal.TextEdits?.Count ?? 0,
                        otherDocuments = proposal.OtherDocuments,
                    });
                    return true;
                }

                var sentences = MarkDiagramPromptBuilder.CountSentences(full);
                if (sentences > MarkDiagramPromptBuilder.MaxSentences)
                {
                    _logger.LogWarning(
                        "[MarkDiagram] Follow-up su '{Box}': {Count} frasi, limite {Max}",
                        boxName, sentences, MarkDiagramPromptBuilder.MaxSentences);
                }

                // NOTA: la risposta a una domanda di seguito non entra nella cache dei box.
                // La cache conserva la sintesi iniziale, il punto fermo a cui si torna
                // riselezionando il box; sovrascriverla con l'esito di una digressione
                // farebbe perdere proprio quel punto fermo.
                await SendAsync(connectionId, new { phase = "done", box = boxName, text = full, sentences, followUp = true });
                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("[MarkDiagram] Domanda di seguito su '{Box}' annullata", boxName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MarkDiagram] Domanda di seguito su '{Box}' fallita", boxName);
                await SendAsync(connectionId, new { phase = "error", box = boxName, message = ex.Message });
                return true;
            }
            finally
            {
                if (_running.TryGetValue(connectionId, out var mine) && mine == cts)
                    _running.TryRemove(connectionId, out _);
                cts.Dispose();
            }
        }

        /// <summary>
        /// Stream della risposta, dentro una sessione del CLI quando il provider ne ha una.
        ///
        /// <para>
        /// Non tutti i provider hanno il concetto di sessione: Copilot CLI e Claude Code sì
        /// (<c>--session-id</c>), le API di Gemini e OpenAI no. Dove manca, la domanda di
        /// seguito parte comunque, ma <b>senza il filo del discorso</b> — e questo viene
        /// scritto nel log, perché è una differenza che si sente nelle risposte e chi
        /// legge un comportamento strano deve poterne trovare la ragione.
        /// </para>
        /// </summary>
        private IAsyncEnumerable<string> StreamAsync(
            IAiProvider provider, string prompt, string modelId, string sessionId, CancellationToken ct)
        {
            if (provider is CopilotCliProvider copilot && !string.IsNullOrWhiteSpace(sessionId))
                return copilot.StreamChatInSessionAsync(prompt, modelId, sessionId, ct);

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                _logger.LogInformation(
                    "[MarkDiagram] Il provider '{Provider}' non ha sessioni: le domande di seguito " +
                    "partiranno senza il filo del discorso precedente.", provider.GetName());
            }
            return provider.StreamChatAsync(prompt, modelId, ct);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Modifica su conferma (F5) e impatti fuori documento (F6)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estrae la proposta dal blocco ```mde-edit. Se il blocco non c'è, il modello ha
        /// solo risposto: non è un errore. Se c'è ma è malformato lo è, e va detto — una
        /// proposta letta a metà è peggio di nessuna proposta.
        /// </summary>
        private MarkDiagramEditProposal? TryParseProposal(string answer)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                answer,
                "```" + MarkDiagramPromptBuilder.EditFence + "\\s*(.+?)```",
                System.Text.RegularExpressions.RegexOptions.Singleline);
            if (!match.Success) return null;

            try
            {
                var proposal = System.Text.Json.JsonSerializer.Deserialize<MarkDiagramEditProposal>(
                    match.Groups[1].Value,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Un blocco che non cambia niente non è una proposta.
                if (proposal == null) return null;
                if (string.IsNullOrWhiteSpace(proposal.NewPlantuml) &&
                    (proposal.TextEdits == null || proposal.TextEdits.Count == 0))
                {
                    _logger.LogWarning("[MarkDiagram] Blocco di modifica senza modifiche: ignorato");
                    return null;
                }
                return proposal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MarkDiagram] Blocco di modifica malformato");
                throw new InvalidOperationException(
                    "Ho preparato una modifica ma non sono riuscito a rileggerla. Riprova a chiedermela.", ex);
            }
        }

        /// <summary>
        /// F6 — altri documenti che nominano le entità toccate. <b>Non vengono modificati</b>:
        /// servono a dire all'utente fin dove arriva l'onda, e a fermarsi lì.
        ///
        /// <para>
        /// La ricerca la facciamo noi con il trigram invece di chiederla al modello: chi
        /// nomina cosa è un fatto verificabile, e su un fatto un indice è più affidabile di
        /// un'inferenza.
        /// </para>
        /// </summary>
        private List<string> FindOtherDocumentsMentioning(DiagramConversation conversation, CancellationToken ct)
        {
            var risultati = new List<string>();
            var boxName = conversation.Context?.Box?.Name;
            if (string.IsNullOrWhiteSpace(boxName)) return risultati;

            // Il nome qualificato può essere "A.B.C": si cerca l'ultimo segmento, che è
            // quello che compare nella prosa.
            var termine = boxName.Contains('.') ? boxName.Substring(boxName.LastIndexOf('.') + 1) : boxName;
            if (termine.Length < 3) return risultati; // sotto i 3 caratteri il trigram non può

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var search = scope.ServiceProvider.GetService<ISearchService>();
                if (search == null) return risultati;

                var trovati = search.SearchContentAsync(termine, conversation.ProjectPath, 20)
                    .GetAwaiter().GetResult();

                var documentoCorrente = System.IO.Path.GetFileName(conversation.Context?.DocumentPath ?? string.Empty);
                foreach (var r in trovati)
                {
                    if (string.Equals(r.FileName, documentoCorrente, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!risultati.Contains(r.FileName)) risultati.Add(r.FileName);
                }
            }
            catch (Exception ex)
            {
                // Non fatale: la modifica sul documento corrente resta valida. Ma va detto,
                // perché l'assenza di avvisi non deve essere scambiata per "nessun impatto".
                _logger.LogWarning(ex, "[MarkDiagram] Ricerca degli impatti fuori documento non riuscita");
            }
            return risultati;
        }

        public async Task<bool> ApplyEditAsync(string connectionId, CancellationToken ct = default)
        {
            if (!_pendingEdits.TryRemove(connectionId, out var proposal)) return false;
            if (!_conversations.TryGetValue(connectionId, out var conversation)) return false;

            var boxName = conversation.Context?.Box?.Name;

            try
            {
                await SendStatusAsync(connectionId, boxName, "Applico la modifica al documento...");

                var fullPath = ResolveDocumentPath(conversation.Context, conversation.ProjectPath);
                if (fullPath == null || !System.IO.File.Exists(fullPath))
                    throw new InvalidOperationException("Non trovo più il documento da modificare.");

                var original = System.IO.File.ReadAllText(fullPath);
                var updated = original;

                // 1 ─ Il blocco PlantUML. Si sostituisce il SORGENTE ESATTO che era stato
                //     letto dall'SVG: se nel frattempo il documento è cambiato non lo si
                //     trova, e ci si ferma invece di scrivere su un testo diverso da quello
                //     su cui il modello ha ragionato.
                if (!string.IsNullOrWhiteSpace(proposal.NewPlantuml))
                {
                    var sorgenteVecchio = conversation.Context?.PlantumlSource?.Trim();
                    if (string.IsNullOrWhiteSpace(sorgenteVecchio))
                        throw new InvalidOperationException("Non ho il sorgente originale del diagramma.");

                    var occorrenze = ContaOccorrenze(updated, sorgenteVecchio);
                    if (occorrenze != 1)
                        throw new InvalidOperationException(
                            occorrenze == 0
                                ? "Il diagramma nel documento non è più quello che avevo letto: forse è stato modificato nel frattempo. Non tocco nulla."
                                : $"Il sorgente del diagramma compare {occorrenze} volte nel documento: non so quale intendi. Non tocco nulla.");

                    updated = updated.Replace(sorgenteVecchio, proposal.NewPlantuml.Trim());
                }

                // 2 ─ Le conseguenze nel testo. Ogni frammento deve comparire esattamente
                //     una volta: zero significa che il modello l'ha inventato, più di una
                //     che non si sa dove. In entrambi i casi si rifiuta TUTTO il blocco,
                //     perché una modifica applicata a metà lascia il documento incoerente,
                //     che è peggio di non averla applicata.
                foreach (var edit in proposal.TextEdits ?? new List<MarkDiagramTextEdit>())
                {
                    if (string.IsNullOrEmpty(edit.Find)) continue;
                    var n = ContaOccorrenze(updated, edit.Find);
                    if (n != 1)
                        throw new InvalidOperationException(
                            n == 0
                                ? $"Non trovo nel documento il testo «{Abbrevia(edit.Find)}». Non applico nulla."
                                : $"Il testo «{Abbrevia(edit.Find)}» compare {n} volte: non so quale cambiare. Non applico nulla.");

                    updated = updated.Replace(edit.Find, edit.Replace ?? string.Empty);
                }

                if (updated == original)
                    throw new InvalidOperationException("La modifica non cambierebbe nulla nel documento.");

                ScriviInModoAtomico(fullPath, updated);

                var quante = (proposal.TextEdits?.Count ?? 0);
                var cosa = !string.IsNullOrWhiteSpace(proposal.NewPlantuml)
                    ? (quante > 0 ? $"diagramma e {quante} punti del testo" : "diagramma")
                    : $"{quante} punti del testo";

                var messaggio = $"Fatto: ho aggiornato {cosa}.";
                if (proposal.OtherDocuments is { Count: > 0 })
                {
                    messaggio += $" Ricorda che {string.Join(", ", proposal.OtherDocuments)} " +
                                 (proposal.OtherDocuments.Count == 1 ? "nomina" : "nominano") +
                                 " la stessa entità: quelli non li ho toccati.";
                }

                await SendAsync(connectionId, new { phase = "done", box = boxName, text = messaggio, followUp = true });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MarkDiagram] Applicazione della modifica fallita");
                await SendAsync(connectionId, new { phase = "error", box = boxName, message = ex.Message });
                return true;
            }
        }

        public Task<bool> DiscardEditAsync(string connectionId)
            => Task.FromResult(_pendingEdits.TryRemove(connectionId, out _));

        private static int ContaOccorrenze(string testo, string frammento)
        {
            if (string.IsNullOrEmpty(frammento)) return 0;
            var n = 0;
            var i = testo.IndexOf(frammento, StringComparison.Ordinal);
            while (i >= 0)
            {
                n++;
                if (n > 1) return n; // basta sapere che è ambiguo
                i = testo.IndexOf(frammento, i + frammento.Length, StringComparison.Ordinal);
            }
            return n;
        }

        private static string Abbrevia(string s)
            => s.Length <= 60 ? s.Replace("\n", " ") : s.Substring(0, 60).Replace("\n", " ") + "…";

        /// <summary>
        /// Scrittura atomica: si scrive un file temporaneo nella stessa cartella e poi lo si
        /// sposta sopra l'originale. Se qualcosa va storto a metà, il documento dell'utente
        /// resta quello di prima invece di diventare mezzo scritto.
        /// </summary>
        private static void ScriviInModoAtomico(string path, string content)
        {
            var dir = System.IO.Path.GetDirectoryName(path)!;
            var temp = System.IO.Path.Combine(dir, $".{System.IO.Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
            try
            {
                System.IO.File.WriteAllText(temp, content);
                System.IO.File.Move(temp, path, overwrite: true);
            }
            finally
            {
                if (System.IO.File.Exists(temp))
                {
                    try { System.IO.File.Delete(temp); } catch { /* residuo innocuo */ }
                }
            }
        }

        private string? ResolveDocumentPath(MarkDiagramContextDto? context, string projectPath)
        {
            var documentPath = context?.DocumentPath;
            if (string.IsNullOrWhiteSpace(documentPath)) return null;
            var fullPath = System.IO.Path.IsPathRooted(documentPath)
                ? System.IO.Path.GetFullPath(documentPath)
                : System.IO.Path.GetFullPath(System.IO.Path.Combine(projectPath ?? string.Empty, documentPath.TrimStart('/', '\\')));
            return IsInsideProject(fullPath, projectPath) ? fullPath : null;
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Document
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Reads the document that contains the diagram. The path comes from the page,
        /// so it is validated against the project root before being opened.
        /// </summary>
        private string ReadDocument(MarkDiagramContextDto? context, string projectPath, out bool truncated)
        {
            truncated = false;
            var documentPath = context?.DocumentPath;
            if (string.IsNullOrWhiteSpace(documentPath)) return string.Empty;

            try
            {
                // Stesso calcolo che usa l'applicazione delle modifiche: due modi diversi di
                // risolvere lo stesso percorso sono un invito a leggere un file e scriverne
                // un altro.
                var fullPath = ResolveDocumentPath(context, projectPath);

                if (fullPath == null)
                {
                    _logger.LogWarning("[MarkDiagram] Document outside the project, refusing to read: {Path}", documentPath);
                    return string.Empty;
                }

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("[MarkDiagram] Document not found: {Path}", fullPath);
                    return string.Empty;
                }

                var text = File.ReadAllText(fullPath);
                if (text.Length > MarkDiagramPromptBuilder.MaxDocChars)
                {
                    truncated = true;
                    text = text.Substring(0, MarkDiagramPromptBuilder.MaxDocChars);
                }
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkDiagram] Could not read document {Path}", documentPath);
                return string.Empty;
            }
        }

        private static bool IsInsideProject(string fullPath, string? projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return false;
            var root = Path.GetFullPath(projectPath);
            if (!root.EndsWith(Path.DirectorySeparatorChar))
                root += Path.DirectorySeparatorChar;
            return fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase);
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Provider
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the LLM the user actually chose, and only that one.
        ///
        /// MdExplorer expresses that choice in TWO independent places, and both count:
        ///
        ///   1. <c>Setting.AI_DefaultProvider</c> / <c>AI_DefaultModel</c> — global, set
        ///      explicitly from the AI preferences. Wins when present.
        ///   2. <c>Project.UseClaudeCodeAsDefault</c> / <c>UseCopilotCliAsDefault</c> —
        ///      per project, "use this CLI automatically when it is installed".
        ///
        /// Reading only the first one was a bug: <c>UseCopilotCliAsDefault</c> is born
        /// <c>true</c>, so a user who never opened the AI preferences still has a working
        /// engine everywhere else in the app — and MarkAgent alone claimed there was none.
        ///
        /// When both per-project flags are on, Claude Code wins. Its flag is born OFF, so
        /// finding it on is a deliberate choice, while Copilot's may simply be the default
        /// nobody touched: the explicit choice beats the default. Same rule as
        /// MdProjectsController, on purpose — two places must not arbitrate differently.
        ///
        /// What this method still refuses to do is walk a chain of substitutes: if the
        /// chosen engine is missing or unavailable, MarkAgent says so instead of answering
        /// through a model the user never picked.
        /// </summary>
        private IAiProvider? ResolveConfiguredProvider(string projectPath, out string? modelId, out string? whyNot)
        {
            modelId = null;
            whyNot = null;

            var byKey = _aiProviders?
                .Where(p => p != null)
                .GroupBy(p => ProviderKey(p.GetProviderType()))
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            if (byKey == null || byKey.Count == 0)
            {
                whyNot = "Nessun provider AI risulta registrato in questa installazione.";
                return null;
            }

            // 1 ─ Preferenza globale esplicita.
            var (preferredKey, preferredModel) = ReadDefaultPreferences();
            if (!string.IsNullOrWhiteSpace(preferredKey))
            {
                if (!byKey.TryGetValue(preferredKey, out var chosen))
                {
                    whyNot = $"Il provider configurato ('{preferredKey}') non risulta registrato in questa installazione.";
                    return null;
                }
                if (!IsUsable(chosen, projectPath, out var why))
                {
                    whyNot = why;
                    return null;
                }
                modelId = preferredModel;
                return chosen;
            }

            // 2 ─ Auto-select per progetto, con la precedenza di MdProjectsController.
            var (useClaudeCode, useCopilotCli) = ReadProjectAutoSelect(projectPath);

            if (useClaudeCode &&
                byKey.TryGetValue("claudecode", out var claude) &&
                IsUsable(claude, projectPath, out _))
            {
                // Alias, non nome pieno: punta sempre all'ultimo Sonnet e non invecchia.
                modelId = "sonnet";
                return claude;
            }

            if (useCopilotCli && byKey.TryGetValue("copilotcli", out var copilot))
            {
                if (IsUsable(copilot, projectPath, out var whyCopilot))
                {
                    // Nessun modello: il flag --model viene omesso e sceglie il CLI.
                    //
                    // Quali modelli esistano è una proprietà DELL'INSTALLAZIONE, non del
                    // programma: nessuna costante scritta qui può essere giusta ovunque.
                    // Verificato il 04/09/2026 — su questa macchina (Copilot CLI 1.0.82)
                    // claude-sonnet-5, gpt-5 e claude-haiku-4.5 sono tutti rifiutati con
                    // "Model ... is not available" e passa solo 'auto', mentre su altre
                    // installazioni esistono modelli che qui non ci sono. È lo stesso
                    // motivo per cui CopilotCliProvider non ha una costante di default.
                    //
                    // Chi vuole UN modello preciso lo dichiara nelle preferenze AI
                    // (AI_DefaultProvider + AI_DefaultModel): vivono nel DB utente, quindi
                    // hanno la stessa granularità del problema — per installazione. Quel
                    // ramo sta più in alto e vince su questo.
                    modelId = null;
                    return copilot;
                }
                // Il progetto ha scelto Copilot CLI ma non è utilizzabile: dire perché è
                // più utile del generico "nessun LLM configurato".
                whyNot = whyCopilot;
                return null;
            }

            whyNot = "Non ho un LLM di riferimento configurato. Impostalo nelle preferenze AI, "
                   + "oppure attiva un CLI nelle impostazioni del progetto.";
            return null;
        }

        /// <summary>
        /// Availability check. The CLI providers answer differently depending on the
        /// directory they run in, so the project path is handed to them first — the same
        /// thing MdProjectsController does when the project is opened.
        /// </summary>
        private bool IsUsable(IAiProvider provider, string projectPath, out string? whyNot)
        {
            whyNot = null;
            var key = ProviderKey(provider.GetProviderType());
            try
            {
                if (!string.IsNullOrWhiteSpace(projectPath))
                {
                    if (provider is CopilotCliProvider copilot) copilot.WorkingDirectory = projectPath;
                    else if (provider is ClaudeCodeProvider claude) claude.WorkingDirectory = projectPath;
                }

                if (provider.IsAvailable()) return true;

                whyNot = $"Il motore configurato ('{key}') non è al momento disponibile su questa macchina.";
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkDiagram] Availability check failed for '{Key}'", key);
                whyNot = $"Non riesco a contattare il motore configurato ('{key}'): {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Per-project auto-select flags. A project row that cannot be found is treated as
        /// "nothing chosen here" rather than as the entity defaults: the defaults describe a
        /// project that exists, and inventing one would resurrect the silent fallback.
        /// </summary>
        private (bool useClaudeCode, bool useCopilotCli) ReadProjectAutoSelect(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) return (false, false);
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                if (db == null) return (false, false);

                db.BeginTransaction();
                var projects = db.GetDal<Project>().GetList().ToList();
                db.Commit();

                var project = projects.FirstOrDefault(p =>
                    string.Equals(p.Path, projectPath, StringComparison.OrdinalIgnoreCase));

                if (project == null)
                {
                    _logger.LogWarning("[MarkDiagram] No Project row for path {Path}", projectPath);
                    return (false, false);
                }

                return (project.UseClaudeCodeAsDefault, project.UseCopilotCliAsDefault);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkDiagram] Could not read the per-project AI auto-select flags");
                return (false, false);
            }
        }

        private static string ProviderKey(ProviderType type) => type switch
        {
            ProviderType.CopilotCli => "copilotcli",
            ProviderType.Gemini => "gemini",
            ProviderType.OpenAI => "openai",
            ProviderType.Local => "local",
            _ => type.ToString().ToLowerInvariant()
        };

        /// <summary>
        /// IUserSettingsDB is a shared NHibernate session: even a read must sit inside
        /// an explicit transaction, or another controller's Commit() breaks. Hence the
        /// short-lived scope.
        /// </summary>
        private (string? provider, string? model) ReadDefaultPreferences()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<IUserSettingsDB>();
                if (db == null) return (null, null);

                db.BeginTransaction();
                var settings = db.GetDal<Setting>().GetList().ToList();
                db.Commit();

                return (
                    settings.FirstOrDefault(s => s.Name == DefaultProviderKey)?.ValueString,
                    settings.FirstOrDefault(s => s.Name == DefaultModelKey)?.ValueString
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MarkDiagram] Could not read the AI default preferences");
                return (null, null);
            }
        }

        /// <summary>
        /// Racconta all'utente cosa sta succedendo mentre aspetta.
        /// L'attesa senza spiegazione fa sembrare rotto ciò che sta solo lavorando: qui il
        /// primo token può tardare parecchi secondi, perché prima si legge il documento e
        /// poi si avvia un CLI. Ogni riga viene sostituita dalla successiva, e tutte quante
        /// dalla risposta vera.
        /// </summary>
        private Task SendStatusAsync(string connectionId, string box, string message)
            => SendAsync(connectionId, new { phase = "status", box, message });

        private Task SendAsync(string connectionId, object payload)
            => _hubContext.Clients.Client(connectionId).SendAsync(StreamEvent, payload);
    }
}
