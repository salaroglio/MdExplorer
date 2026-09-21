using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.OpenCode
{
    /// <summary>
    /// Una conversazione di MarkAgent con opencode: una sessione sul server
    /// (<c>POST /session?directory=</c>) più lo stream degli eventi che la racconta mentre
    /// accade.
    /// <para>
    /// <b>Perché due canali.</b> <c>POST /session/{id}/message</c> è <b>sincrono</b>: torna a
    /// turno finito, con il testo completo, i token e il costo — ma dopo secondi di silenzio.
    /// I frammenti da mostrare subito arrivano invece da <c>GET /event</c> (SSE). Quindi lo
    /// stream si apre <b>prima</b> di inviare il messaggio, altrimenti i primi frammenti si
    /// perdono, e il testo autorevole resta quello della risposta sincrona.
    /// </para>
    /// <para>
    /// Eventi usati, tutti misurati il 21/09/2026 su opencode 1.18.30:
    /// <list type="bullet">
    /// <item><description><c>message.part.delta</c> <c>{sessionID, messageID, partID, field, delta}</c>.
    /// ⚠️ <c>field</c> vale <b>sempre</b> «text» — è il nome della proprietà della parte, non la sua
    /// natura. A dire se è risposta o ragionamento è il <b>tipo della parte</b>, che arriva in
    /// <c>message.part.updated</c>. Fidarsi di <c>field</c> metteva il ragionamento del modello
    /// dentro la risposta, in chat (visto nell'app il 21/09/2026);</description></item>
    /// <item><description><c>message.part.updated</c> con <c>part.type: "tool"</c> — <c>tool</c>,
    /// <c>callID</c>, <c>state.status</c>;</description></item>
    /// <item><description><c>session.idle</c> — il turno è finito.</description></item>
    /// </list>
    /// </para>
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Motore-OpenCode.md, fase F4.</para>
    /// </summary>
    public sealed class OpenCodeSession : IAsyncDisposable
    {
        /// <summary>
        /// Quanto si aspettano gli ultimi frammenti dopo che la risposta sincrona è già
        /// tornata. Serve solo a chiudere in ordine: il testo autorevole ce l'abbiamo già.
        /// </summary>
        private static readonly TimeSpan DrainAfterAnswer = TimeSpan.FromSeconds(2);

        private readonly ILogger _logger;
        private readonly OpenCodeServer _server;
        private readonly SemaphoreSlim _turnGate = new(1, 1);

        private HttpClient _client;
        private CancellationTokenSource _streamCts;
        private Task _streamTask;
        private volatile bool _disposed;

        /// <summary>Il turno in corso, se c'è: gli eventi del server finiscono qui dentro.</summary>
        private volatile Turn _turn;

        private sealed class Turn
        {
            public Channel<OpenCodeChunk> Channel { get; } =
                System.Threading.Channels.Channel.CreateUnbounded<OpenCodeChunk>(
                    new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
            public TaskCompletionSource Idle { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public HashSet<string> ToolsAnnounced { get; } = new(StringComparer.Ordinal);

            /// <summary>
            /// Di che tipo è ogni parte (<c>text</c>, <c>reasoning</c>, <c>tool</c>…). ⚠️ È
            /// l'UNICA cosa che distingue la risposta dal ragionamento: nei delta il campo
            /// <c>field</c> vale <b>sempre</b> «text», perché è il nome della proprietà della
            /// parte, non la sua natura (misurato il 21/09/2026).
            /// </summary>
            public Dictionary<string, string> PartTypes { get; } = new(StringComparer.Ordinal);

            /// <summary>
            /// Delta arrivati prima di sapere a che parte appartengono. Non si indovina e non
            /// si buttano: si tengono qui e si consegnano appena il tipo arriva.
            /// </summary>
            public Dictionary<string, List<string>> Pending { get; } = new(StringComparer.Ordinal);
        }

        public OpenCodeSession(ILogger logger, OpenCodeServer server, string workingDirectory, string model)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _server = server ?? throw new ArgumentNullException(nameof(server));

            if (string.IsNullOrWhiteSpace(workingDirectory) || !Directory.Exists(workingDirectory))
            {
                throw new ArgumentException(
                    $"La cartella del progetto non esiste: '{workingDirectory}'. opencode lavora dentro il " +
                    "progetto, non altrove.", nameof(workingDirectory));
            }

            WorkingDirectory = Path.GetFullPath(workingDirectory);
            Model = model;
        }

        /// <summary>La cartella del progetto: viaggia in <c>?directory=</c> a ogni chiamata.</summary>
        public string WorkingDirectory { get; }

        /// <summary>
        /// Il modello chiesto, come <c>provider/modello</c> (es. <c>opencode/big-pickle</c>), o
        /// <c>null</c> = quello di default del server. Si passa a <b>ogni</b> messaggio, quindi
        /// cambiarlo non tocca la conversazione (misurato).
        /// </summary>
        public string Model { get; private set; }

        /// <summary>L'identificativo della sessione sul server, o <c>null</c> se non è ancora nata.</summary>
        public string SessionId { get; private set; }

        /// <summary>Ultima volta che qualcuno ha parlato con questa sessione: la usa lo sweep del pool.</summary>
        public DateTime LastUsedUtc { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// La sessione è ancora utilizzabile? Qui «viva» vuol dire «non chiusa da noi»: non c'è
        /// un processo da sorvegliare, il server è condiviso e lo sorveglia
        /// <see cref="OpenCodeServer"/>.
        /// </summary>
        public bool IsAlive => !_disposed;

        /// <summary>Consuntivo dell'ultimo turno finito.</summary>
        public OpenCodeTurnUsage LastTurnUsage { get; private set; }

        /// <summary>
        /// Il testo dell'ultimo turno <b>come lo dichiara il server</b>. È questo che va in
        /// cronologia: i frammenti servono a far vedere la risposta mentre nasce, ma se lo
        /// stream ne perdesse uno la cronologia resterebbe monca senza che nessuno lo sappia.
        /// </summary>
        public string LastTurnText { get; private set; }

        /// <summary>
        /// Cambia il modello dei prossimi messaggi. Non chiude niente e non chiede niente al
        /// server: il modello è un campo del messaggio.
        /// </summary>
        public void SetModel(string model) => Model = model;

        /// <summary>
        /// Manda un messaggio e restituisce i frammenti mentre arrivano.
        /// </summary>
        public async IAsyncEnumerable<OpenCodeChunk> PromptAsync(
            string prompt,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await _turnGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            LastUsedUtc = DateTime.UtcNow;
            var turn = new Turn();
            Task<HttpResponseMessage> post = null;
            try
            {
                await EnsureSessionAsync(cancellationToken).ConfigureAwait(false);
                _turn = turn;

                post = SendMessageAsync(prompt, cancellationToken);

                // Si legge finché il canale non viene chiuso: lo chiude chi conclude il turno.
                _ = ConcludeWhenAnsweredAsync(turn, post, cancellationToken);

                while (await turn.Channel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (turn.Channel.Reader.TryRead(out var chunk))
                    {
                        yield return chunk;
                    }
                }
            }
            finally
            {
                _turn = null;
                _turnGate.Release();

                // Stop dell'utente: fermare la NOSTRA lettura non basta, il server starebbe
                // ancora generando (e pagando). Si ferma davvero, e la risposta sincrona che
                // resta in volo va comunque osservata per non lasciarne l'errore orfano.
                if (cancellationToken.IsCancellationRequested)
                {
                    _ = AbortAsync(CancellationToken.None);
                    if (post != null)
                    {
                        _ = post.ContinueWith(t =>
                        {
                            if (t.IsFaulted) _logger.LogDebug(t.Exception, "[opencode] turno annullato, POST in errore");
                            else if (t.IsCompletedSuccessfully) t.Result.Dispose();
                        }, TaskScheduler.Default);
                        post = null;
                    }
                }
            }

            // Fuori dal `try` con lo `yield`: qui si propaga l'errore vero del POST, se c'è
            // stato. Un turno fallito non deve sembrare un turno vuoto.
            if (post != null)
            {
                using var response = await post.ConfigureAwait(false);
                await ReadAnswerAsync(response, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Chiude il canale dei frammenti quando il turno è davvero finito: la risposta
        /// sincrona è tornata <b>e</b> lo stream ha detto <c>session.idle</c> (o è scaduta la
        /// breve attesa di coda).
        /// </summary>
        private async Task ConcludeWhenAnsweredAsync(Turn turn, Task<HttpResponseMessage> post, CancellationToken ct)
        {
            try
            {
                await Task.WhenAny(post, Task.Delay(Timeout.Infinite, ct)).ConfigureAwait(false);
                await Task.WhenAny(turn.Idle.Task, Task.Delay(DrainAfterAnswer, CancellationToken.None))
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[opencode] chiusura del turno interrotta");
            }
            finally
            {
                turn.Channel.Writer.TryComplete();
            }
        }

        private async Task<HttpResponseMessage> SendMessageAsync(string prompt, CancellationToken ct)
        {
            var body = new Dictionary<string, object>
            {
                ["parts"] = new object[] { new { type = "text", text = prompt } }
            };
            if (!string.IsNullOrWhiteSpace(Model))
            {
                var (providerId, modelId) = SplitModel(Model);
                body["providerID"] = providerId;
                body["modelID"] = modelId;
            }

            var url = $"/session/{SessionId}/message?directory={Uri.EscapeDataString(WorkingDirectory)}";
            return await _client.PostAsJsonAsync(url, body, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// <c>opencode/big-pickle</c> → provider + modello. Lo slash divide <b>al primo</b>
        /// taglio: il nome del modello può contenerne altri.
        /// </summary>
        public static (string providerId, string modelId) SplitModel(string model)
        {
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Modello vuoto.", nameof(model));

            var i = model.IndexOf('/');
            if (i <= 0 || i == model.Length - 1)
            {
                throw new ArgumentException(
                    $"Il modello di opencode si scrive 'provider/modello' (es. 'opencode/big-pickle'), ricevuto '{model}'.",
                    nameof(model));
            }
            return (model.Substring(0, i), model.Substring(i + 1));
        }

        private async Task ReadAnswerAsync(HttpResponseMessage response, CancellationToken ct)
        {
            var payload = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"opencode ha rifiutato il messaggio ({(int)response.StatusCode}): {Truncate(payload, 400)}");
            }

            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            var text = new StringBuilder();
            if (root.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("type", out var t) && t.GetString() == "text"
                        && part.TryGetProperty("text", out var txt))
                    {
                        text.Append(txt.GetString());
                    }
                }
            }
            LastTurnText = text.ToString();

            if (root.TryGetProperty("info", out var info))
            {
                LastTurnUsage = ReadUsage(info);
            }
        }

        private static OpenCodeTurnUsage ReadUsage(JsonElement info)
        {
            long? Num(JsonElement parent, string name)
                => parent.TryGetProperty(name, out var v) && v.TryGetInt64(out var n) ? n : (long?)null;

            JsonElement tokens = default;
            var hasTokens = info.TryGetProperty("tokens", out tokens);
            JsonElement cache = default;
            var hasCache = hasTokens && tokens.TryGetProperty("cache", out cache);

            long? duration = null;
            if (info.TryGetProperty("time", out var time)
                && time.TryGetProperty("created", out var created) && created.TryGetInt64(out var startedAt)
                && time.TryGetProperty("completed", out var completed) && completed.TryGetInt64(out var endedAt))
            {
                duration = endedAt - startedAt;
            }

            return new OpenCodeTurnUsage
            {
                ProviderId = info.TryGetProperty("providerID", out var p) ? p.GetString() : null,
                ModelId = info.TryGetProperty("modelID", out var m) ? m.GetString() : null,
                InputTokens = hasTokens ? Num(tokens, "input") : null,
                OutputTokens = hasTokens ? Num(tokens, "output") : null,
                ReasoningTokens = hasTokens ? Num(tokens, "reasoning") : null,
                TotalTokens = hasTokens ? Num(tokens, "total") : null,
                CacheReadTokens = hasCache ? Num(cache, "read") : null,
                CacheWriteTokens = hasCache ? Num(cache, "write") : null,
                CostUsd = info.TryGetProperty("cost", out var c) && c.TryGetDouble(out var cost) ? cost : null,
                DurationMs = duration,
            };
        }

        /// <summary>
        /// Stop dell'utente: il turno in volo si ferma sul server, non solo nella nostra
        /// lettura. Restituisce <c>false</c> se non c'era niente da fermare.
        /// </summary>
        public async Task<bool> AbortAsync(CancellationToken cancellationToken = default)
        {
            if (SessionId == null || _client == null) return false;
            try
            {
                var url = $"/session/{SessionId}/abort?directory={Uri.EscapeDataString(WorkingDirectory)}";
                using var res = await _client.PostAsync(url, content: null, cancellationToken).ConfigureAwait(false);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[opencode] abort rifiutato dal server ({Code})", (int)res.StatusCode);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[opencode] abort fallito");
                return false;
            }
        }

        private async Task EnsureSessionAsync(CancellationToken ct)
        {
            if (SessionId != null && _client != null) return;

            _client = await _server.CreateClientAsync(ct).ConfigureAwait(false);

            var url = $"/session?directory={Uri.EscapeDataString(WorkingDirectory)}";
            using var res = await _client.PostAsJsonAsync(url, new { }, ct).ConfigureAwait(false);
            var payload = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"opencode non ha creato la sessione ({(int)res.StatusCode}): {Truncate(payload, 400)}");
            }

            using var doc = JsonDocument.Parse(payload);
            SessionId = doc.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
            if (string.IsNullOrEmpty(SessionId))
            {
                throw new InvalidOperationException(
                    $"opencode ha risposto alla creazione della sessione senza un id: {Truncate(payload, 400)}");
            }

            _logger.LogInformation("[opencode] sessione {Session} su {Dir}", SessionId, WorkingDirectory);
            StartEventStream();
        }

        /// <summary>
        /// Apre <c>GET /event</c> e lo tiene aperto per tutta la vita della sessione: aprirlo a
        /// ogni turno perderebbe i primi frammenti, che arrivano prima che la connessione sia
        /// stabilita.
        /// </summary>
        private void StartEventStream()
        {
            _streamCts = new CancellationTokenSource();
            _streamTask = Task.Run(() => PumpEventsAsync(_streamCts.Token));
        }

        private async Task PumpEventsAsync(CancellationToken ct)
        {
            try
            {
                var url = $"/event?directory={Uri.EscapeDataString(WorkingDirectory)}";
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Accept.ParseAdd("text/event-stream");

                using var response = await _client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
                using var reader = new StreamReader(stream, Encoding.UTF8);

                string line;
                while ((line = await reader.ReadLineAsync(ct).ConfigureAwait(false)) != null)
                {
                    if (!line.StartsWith("data:", StringComparison.Ordinal)) continue;
                    var json = line.Substring(5).Trim();
                    if (json.Length == 0) continue;

                    try { HandleEvent(json); }
                    catch (Exception ex) { _logger.LogDebug(ex, "[opencode] evento non interpretato: {Json}", Truncate(json, 200)); }
                }
            }
            catch (OperationCanceledException) { /* sessione chiusa: normale */ }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[opencode] lo stream degli eventi si è interrotto");
            }
        }

        private void HandleEvent(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("type", out var typeEl)) return;
            if (!root.TryGetProperty("properties", out var props)) return;

            // Il server parla di TUTTE le sessioni che ospita, comprese quelle di altri
            // progetti: quello che non è nostro non ci riguarda.
            if (props.TryGetProperty("sessionID", out var sid) && sid.GetString() != SessionId) return;

            var turn = _turn;
            switch (typeEl.GetString())
            {
                case "message.part.delta":
                    if (turn == null) return;
                    var partId = props.TryGetProperty("partID", out var pid2) ? pid2.GetString() : null;
                    var delta = props.TryGetProperty("delta", out var d) ? d.GetString() : null;
                    if (string.IsNullOrEmpty(delta) || string.IsNullOrEmpty(partId)) return;

                    string type;
                    lock (turn.PartTypes)
                    {
                        if (!turn.PartTypes.TryGetValue(partId, out type))
                        {
                            // Tipo non ancora annunciato: si mette da parte. Mostrarlo come
                            // risposta sarebbe il ragionamento del modello in mezzo al testo.
                            if (!turn.Pending.TryGetValue(partId, out var buffer))
                            {
                                buffer = new List<string>();
                                turn.Pending[partId] = buffer;
                            }
                            buffer.Add(delta);
                            return;
                        }
                    }
                    EmitDelta(turn, type, delta);
                    return;

                case "message.part.updated":
                    if (turn == null) return;
                    if (!props.TryGetProperty("part", out var part)) return;
                    if (!part.TryGetProperty("type", out var pt)) return;

                    // Si impara il tipo della parte, e si consegna quello che era in attesa.
                    if (part.TryGetProperty("id", out var partIdEl))
                    {
                        var id = partIdEl.GetString();
                        var partType = pt.GetString();
                        List<string> waiting = null;
                        lock (turn.PartTypes)
                        {
                            if (!string.IsNullOrEmpty(id))
                            {
                                turn.PartTypes[id] = partType;
                                if (turn.Pending.TryGetValue(id, out waiting)) turn.Pending.Remove(id);
                            }
                        }
                        if (waiting != null)
                        {
                            foreach (var piece in waiting) EmitDelta(turn, partType, piece);
                        }
                    }

                    if (pt.GetString() != "tool") return;
                    var tool = part.TryGetProperty("tool", out var tn) ? tn.GetString() : "?";
                    var status = part.TryGetProperty("state", out var st) && st.TryGetProperty("status", out var sv)
                        ? sv.GetString() : null;
                    var callId = part.TryGetProperty("callID", out var cid) ? cid.GetString() : tool;
                    // Lo stesso tool arriva più volte mentre cambia stato: si annuncia una volta
                    // per chiamata e poi solo la fine, altrimenti la riga di stato lampeggia.
                    var key = callId + "|" + status;
                    lock (turn.ToolsAnnounced)
                    {
                        if (!turn.ToolsAnnounced.Add(key)) return;
                    }
                    var label = status == null ? tool : $"{tool} · {status}";
                    turn.Channel.Writer.TryWrite(new OpenCodeChunk(OpenCodeChunk.KindTool, label));
                    return;

                case "session.idle":
                    turn?.Idle.TrySetResult();
                    return;
            }
        }

        /// <summary>
        /// Traduce un delta nel frammento giusto <b>secondo il tipo della sua parte</b>. Un
        /// tipo che non conosciamo non diventa testo della risposta: nel dubbio non si mostra
        /// niente, invece di sporcare la chat con dati interni.
        /// </summary>
        private void EmitDelta(Turn turn, string partType, string delta)
        {
            switch (partType)
            {
                case "text":
                    turn.Channel.Writer.TryWrite(new OpenCodeChunk(OpenCodeChunk.KindMessage, delta));
                    break;
                case "reasoning":
                    turn.Channel.Writer.TryWrite(new OpenCodeChunk(OpenCodeChunk.KindThinking, delta));
                    break;
                default:
                    _logger.LogDebug("[opencode] delta di una parte '{Type}' non mostrato", partType);
                    break;
            }
        }

        private static string Truncate(string s, int max)
            => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";

        public async ValueTask DisposeAsync()
        {
            _disposed = true;
            try { _streamCts?.Cancel(); } catch { }
            if (_streamTask != null)
            {
                try { await _streamTask.WaitAsync(TimeSpan.FromSeconds(3)).ConfigureAwait(false); }
                catch { /* lo stream stava già chiudendo */ }
            }
            _streamCts?.Dispose();
            _client?.Dispose();
            _turnGate.Dispose();
        }
    }
}
