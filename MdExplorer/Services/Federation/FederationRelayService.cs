using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Federation;
using MdExplorer.Services.AgentRegistry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocketIOClient;

namespace MdExplorer.Services.Federation
{
    /// <summary>La città locale attiva su una stanza, dal punto di vista di questa macchina.</summary>
    public sealed class LocalCity
    {
        public string ProjectPath { get; init; }
        public string ProjectName { get; init; }
        public string RoomId { get; init; }
        public string RelayUrl { get; init; }
    }

    /// <summary>Vista dello stato federato locale, per la UI/rubrica (§12.5).</summary>
    public interface IFederationState
    {
        IReadOnlyList<LocalCity> GetLocalCities();
    }

    /// <summary>
    /// Spedisce una richiesta di intervento federata a un'altra città sulla stessa stanza-repo
    /// (§12.6): cifra il payload col room secret e lo emette al relay (<c>send</c> mirato per
    /// ownerId). Ritorna false se il progetto non ha una connessione federata attiva.
    /// </summary>
    public interface IFederationSender
    {
        Task<bool> SendFederatedRequestAsync(string projectPath, string targetOwnerId, FederatedRequestPayload payload);

        /// <summary>
        /// Spedisce l'esito di ritorno di un intervento delegato (Fase 7a): stesso tunnel cifrato
        /// della richiesta, payload distinto (<see cref="FederatedResultPayload"/>). false se il
        /// progetto non ha una connessione federata attiva.
        /// </summary>
        Task<bool> SendFederatedResultAsync(string projectPath, string targetOwnerId, FederatedResultPayload payload);
    }

    /// <summary>
    /// Il presidio della federazione lato Service (§12.7 regola 2: la città vive col Service,
    /// non con la UI). Hosted <see cref="BackgroundService"/> — a differenza della vecchia
    /// <c>VpsChatStreamingService</c> (singleton client-triggered) non dipende da un client
    /// connesso. Scansiona periodicamente l'elenco dei progetti (tabella <c>Project</c>,
    /// persistente) e, per quelli con la città attiva, <b>assembla l'annuncio cifrato</b> in
    /// uno snapshot locale.
    /// <para>
    /// <b>Dormiente per default</b>: nessun progetto con <c>agentCity.enabled</c> → nessuna
    /// connessione. Quando una città è attiva, apre verso il relay (namespace <c>/mdfed</c>)
    /// <b>una connessione Socket.IO per stanza</b> (il server tiene lo stato <c>joined</c>
    /// per-socket), presenta il join token derivato dal room secret e spedisce l'annuncio
    /// cifrato. Reconnect-forever. La chiave/segreto non lasciano mai la macchina (R15).
    /// </para>
    /// </summary>
    public class FederationRelayService : BackgroundService, IFederationState, IFederationSender
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(60);
        // Path dell'engine Socket.IO sul relay (dietro nginx), condiviso col canale chat.
        private const string EnginePath = "/mdchat/socket.io";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFederationPresenceService _presence;
        private readonly IHeadlessProjectActivator _activator;
        private readonly ILogger<FederationRelayService> _logger;
        private readonly string _baseUrl;   // es. https://errantia.net (namespace /mdfed appeso)
        private readonly string _apiKey;    // gate di base del relay (stesso del canale chat)

        private volatile IReadOnlyList<LocalCity> _snapshot = Array.Empty<LocalCity>();
        // Una connessione per stanza (roomId). ConcurrentDictionary: scritto dal loop di
        // scansione, letto anche dal thread dell'endpoint RequestIntervention (send).
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, RoomConnection> _rooms = new();
        // Progetti già attivati headless in questa esecuzione: evita il walk FS + Engine DB a ogni scan.
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> _activatedProjects = new();
        private bool _warnedNoApiKey;

        public FederationRelayService(
            IServiceScopeFactory scopeFactory,
            IFederationPresenceService presence,
            IHeadlessProjectActivator activator,
            IConfiguration configuration,
            ILogger<FederationRelayService> logger)
        {
            _scopeFactory = scopeFactory;
            _presence = presence;
            _activator = activator;
            _logger = logger;

            // Il relay è lo stesso server del canale chat: base host + engine path condivisi,
            // la federazione vive nel namespace /mdfed. Override futuro via agentCity.relayUrl.
            var wsUrl = configuration["MdChat:WebSocketUrl"] ?? "wss://errantia.net/mdchat";
            _baseUrl = wsUrl
                .Replace("wss://", "https://")
                .Replace("ws://", "http://")
                .Replace("/mdchat", "")
                .TrimEnd('/');
            // Placeholder versionato (YOUR_API_KEY_HERE) o assente ⇒ null ⇒ dormiente. La chiave
            // vera arriva da env MdChat__ApiKey / appsettings.Development.json (gitignored) / user-secrets.
            _apiKey = MdExplorer.Services.TeamChat.MdChatConfig.ResolveApiKey(configuration);
            _apiKeyIsPlaceholder = MdExplorer.Services.TeamChat.MdChatConfig.IsPlaceholderApiKey(configuration);
        }

        private readonly bool _apiKeyIsPlaceholder;

        public IReadOnlyList<LocalCity> GetLocalCities() => _snapshot;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var (cities, announces) = RebuildSnapshot();
                    _snapshot = cities;
                    await TransmitPendingAsync(announces, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Federation] Scansione presenza fallita");
                }
                try { await Task.Delay(ScanInterval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        /// <summary>
        /// Costruisce lo snapshot delle città locali attive e i relativi annunci cifrati.
        /// Fail-soft per-progetto: un progetto il cui git non è leggibile viene semplicemente
        /// saltato (loggato), non fa fallire l'intera scansione.
        /// </summary>
        private (IReadOnlyList<LocalCity> Cities, IReadOnlyList<FederationAnnounce> Announces) RebuildSnapshot()
        {
            var cities = new List<LocalCity>();
            var announces = new List<FederationAnnounce>();

            using var scope = _scopeFactory.CreateScope();
            var metadata = scope.ServiceProvider.GetRequiredService<IProjectMetadataService>();
            var registry = scope.ServiceProvider.GetRequiredService<IAgentRegistryService>();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();

            List<Project> projects;
            db.BeginTransaction();
            projects = db.GetDal<Project>().GetList().ToList();
            db.Commit();

            foreach (var project in projects)
            {
                // Pre-filtro economico: git-resolviamo SOLO i progetti con la città attiva.
                var cfg = Safe(() => metadata.GetAgentCity(project.Path));
                if (cfg == null || !cfg.Enabled) continue;

                // Attivazione headless (§12.7): senza client, indicizza gli .agent.md e scalda
                // il registry. UNA volta per progetto (non a ogni tick da 60s): l'operazione fa
                // walk del filesystem + apertura Engine DB, sarebbe churn continuo. I file agente
                // aggiunti a caldo li ripesca la riapertura del progetto / il FSW.
                if (_activatedProjects.TryAdd(project.Path, 0))
                    _activator.ActivateForFederation(project.Path);

                var (origin, email) = ResolveGit(project.Path);
                var roster = BuildTrustedRoster(registry, project.Path);

                FederationAnnounce announce;
                try
                {
                    announce = _presence.BuildAnnounce(project.Path, origin, email, roster);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Federation] Annuncio non costruibile per '{Project}'", project.Path);
                    continue;
                }
                if (announce == null) continue;

                cities.Add(new LocalCity
                {
                    ProjectPath = project.Path,
                    ProjectName = project.Name,
                    RoomId = announce.RoomId,
                    RelayUrl = announce.RelayUrl,
                });
                announces.Add(announce);
            }

            return (cities, announces);
        }

        /// <summary>
        /// Trasmette gli annunci verso il relay (§12.5): apre/aggiorna una connessione Socket.IO
        /// per stanza sul namespace <c>/mdfed</c>, presenta il join token e spedisce l'annuncio
        /// cifrato. Le stanze non più attive vengono chiuse. Dormiente senza annunci o senza API
        /// key del relay (non blocca la UX: logga una volta e prosegue).
        /// </summary>
        private Task TransmitPendingAsync(IReadOnlyList<FederationAnnounce> announces, CancellationToken ct)
        {
            // Una connessione per stanza. Se PIÙ progetti risolvono alla stessa stanza (stesso
            // repo aperto due volte), teniamo il primo ma lo diciamo forte: gli altri NON si
            // federano (le deliver andrebbero legate al progetto sbagliato). Limitazione visibile,
            // non misrouting silenzioso.
            var grouped = announces
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.RoomId))
                .GroupBy(a => a.RoomId)
                .ToList();
            foreach (var g in grouped.Where(g => g.Count() > 1))
                _logger.LogWarning("[Federation] stanza {Room}: più progetti mappano sullo stesso repo — federo solo '{Kept}', ignoro: {Dropped}",
                    g.Key, g.First().ProjectPath, string.Join(", ", g.Skip(1).Select(a => a.ProjectPath)));
            var desired = grouped.ToDictionary(g => g.Key, g => g.First());

            // Chiudi le connessioni delle stanze non più attive.
            foreach (var roomId in _rooms.Keys.Where(k => !desired.ContainsKey(k)).ToList())
            {
                if (_rooms.TryRemove(roomId, out var closing))
                {
                    try { closing.Dispose(); } catch { /* best effort */ }
                    _logger.LogInformation("[Federation] stanza {Room} chiusa (città non più attiva)", roomId);
                }
            }

            if (desired.Count == 0)
                return Task.CompletedTask;

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                if (!_warnedNoApiKey)
                {
                    if (_apiKeyIsPlaceholder)
                        _logger.LogWarning("[Federation] città attive ma 'MdChat:ApiKey' è ancora il placeholder ('{Ph}'): imposta la chiave vera via env MdChat__ApiKey, appsettings.Development.json (gitignored) o user-secrets. Resto dormiente.",
                            MdExplorer.Services.TeamChat.MdChatConfig.PlaceholderApiKey);
                    else
                        _logger.LogWarning("[Federation] città attive ma 'MdChat:ApiKey' assente: impossibile connettersi al relay. Resto dormiente.");
                    _warnedNoApiKey = true;
                }
                return Task.CompletedTask;
            }

            // Apri/aggiorna le connessioni delle stanze attive.
            foreach (var (roomId, announce) in desired)
            {
                if (_rooms.TryGetValue(roomId, out var existing))
                {
                    existing.UpdateAnnounce(announce);   // presenza cambiata? ri-annuncia
                }
                else
                {
                    // Onora l'override per-progetto agentCity.relayUrl: la connessione va sul
                    // relay dichiarato (scheme+host), non sempre su quello globale. Se assente/
                    // non parsabile, ricade sul base globale.
                    var baseUrl = ToHttpBase(announce.RelayUrl) ?? _baseUrl;
                    var conn = new RoomConnection(baseUrl, EnginePath, _apiKey, announce, _logger);
                    var captured = announce;             // per la closure del deliver
                    conn.OnDeliver = env => { _ = HandleDeliverAsync(captured, env); };
                    _rooms[roomId] = conn;
                    conn.Start();                        // fire-and-forget: OnConnected fa join+announce
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Un messaggio federato è arrivato per una stanza: decifra la busta col room secret del
        /// progetto (mai lasciato la macchina), ricostruisce la richiesta e la passa al gate umano
        /// (§12.6) — che NON fa partire nulla finché l'umano non autorizza. Fail-loud sui log:
        /// busta non apribile (secret sbagliato/manomessa) o payload malformato.
        /// </summary>
        /// <summary>Solo il discriminante di busta, per instradare senza deserializzare tutto (Fase 7a).</summary>
        private sealed class KindPeek
        {
            public string Kind { get; set; }
        }

        private async Task HandleDeliverAsync(FederationAnnounce announce, string envelope)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var meta = scope.ServiceProvider.GetRequiredService<MdExplorer.Services.IProjectMetadataService>();
                var secret = meta.GetAgentCity(announce.ProjectPath)?.RoomSecret;
                if (string.IsNullOrWhiteSpace(secret))
                {
                    _logger.LogWarning("[Federation] deliver per stanza {Room} ma room secret assente: scartato.", announce.RoomId);
                    return;
                }

                var json = FederationCrypto.Decrypt(secret, announce.RoomId, envelope);

                // Peek del discriminante di busta (Fase 7a) PRIMA di deserializzare il payload
                // pieno: una busta senza Kind (origine vecchia) = request-intervention.
                var kind = System.Text.Json.JsonSerializer.Deserialize<KindPeek>(json)?.Kind;
                if (string.Equals(kind, FederationKind.InterventionResult, StringComparison.OrdinalIgnoreCase))
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<FederatedResultPayload>(json);
                    if (result == null)
                    {
                        _logger.LogWarning("[Federation] deliver 'intervention-result' per stanza {Room}: payload nullo dopo decrypt.", announce.RoomId);
                        return;
                    }
                    var resultReceiver = scope.ServiceProvider.GetRequiredService<IFederatedResultReceiver>();
                    await resultReceiver.Receive(announce.ProjectPath, result);
                    return;
                }

                var payload = System.Text.Json.JsonSerializer.Deserialize<FederatedRequestPayload>(json);
                if (payload == null)
                {
                    _logger.LogWarning("[Federation] deliver per stanza {Room}: payload nullo dopo decrypt.", announce.RoomId);
                    return;
                }

                var receiver = scope.ServiceProvider.GetRequiredService<IFederatedRequestReceiver>();
                receiver.Receive(announce.ProjectPath, payload);
            }
            catch (FederationCryptoException ex)
            {
                _logger.LogWarning(ex, "[Federation] busta 'deliver' non apribile per stanza {Room} (secret errato/manomessa).", announce.RoomId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Federation] gestione 'deliver' fallita per stanza {Room}.", announce.RoomId);
            }
        }

        /// <summary>
        /// Spedisce una richiesta federata (§12.6): cifra il payload col room secret del progetto
        /// e lo emette al relay (<c>send</c> mirato per <paramref name="targetOwnerId"/>) sulla
        /// connessione della stanza. false se il progetto non ha una connessione federata attiva.
        /// </summary>
        public Task<bool> SendFederatedRequestAsync(string projectPath, string targetOwnerId, FederatedRequestPayload payload)
            => SendEnvelopeAsync(projectPath, targetOwnerId, System.Text.Json.JsonSerializer.Serialize(payload));

        /// <inheritdoc/>
        public Task<bool> SendFederatedResultAsync(string projectPath, string targetOwnerId, FederatedResultPayload payload)
            => SendEnvelopeAsync(projectPath, targetOwnerId, System.Text.Json.JsonSerializer.Serialize(payload));

        // Core condiviso: cifra il JSON già serializzato del payload TIPATO col room secret del
        // progetto e lo emette al relay. Serializzare qui su `object` perderebbe le proprietà
        // (System.Text.Json guarda il tipo statico), perciò ogni overload serializza il proprio
        // tipo concreto e passa qui la stringa.
        private async Task<bool> SendEnvelopeAsync(string projectPath, string targetOwnerId, string payloadJson)
        {
            var city = _snapshot.FirstOrDefault(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));
            if (city == null || !_rooms.TryGetValue(city.RoomId, out var conn))
            {
                _logger.LogWarning("[Federation] send federato: nessuna connessione attiva per '{Project}'.", projectPath);
                return false;
            }

            string secret;
            using (var scope = _scopeFactory.CreateScope())
            {
                var meta = scope.ServiceProvider.GetRequiredService<MdExplorer.Services.IProjectMetadataService>();
                secret = meta.GetAgentCity(projectPath)?.RoomSecret;
            }
            if (string.IsNullOrWhiteSpace(secret))
            {
                _logger.LogWarning("[Federation] send federato: room secret assente per '{Project}'.", projectPath);
                return false;
            }

            var envelope = FederationCrypto.Encrypt(secret, city.RoomId, payloadJson);
            try
            {
                // true SOLO se il relay ackka la presa in carico (consegnata o accodata).
                var acked = await conn.SendAsync(targetOwnerId, envelope);
                if (!acked)
                    _logger.LogWarning("[Federation] send federato per '{Project}': il relay non ha confermato (perso/ok:false/timeout).", projectPath);
                return acked;
            }
            catch (Exception ex)
            {
                // Contratto: "false se nessuna connessione attiva" — vale anche quando la
                // RoomConnection esiste ma il socket è giù (relay caduto, mid-reconnect):
                // il chiamante deve vedere un 503 ritentabile, non un 500 generico.
                _logger.LogWarning(ex, "[Federation] send federato fallito per '{Project}' (socket non attivo?)", projectPath);
                return false;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var conn in _rooms.Values)
            {
                try { conn.Dispose(); } catch { /* best effort */ }
            }
            _rooms.Clear();
            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Ricava il base URL HTTP (scheme+host) da un URL ws/wss del relay, ignorando il path.
        /// <c>wss://relay.example.com/mdfed</c> → <c>https://relay.example.com</c>. null se non parsabile.
        /// </summary>
        private static string ToHttpBase(string wsUrl)
        {
            if (string.IsNullOrWhiteSpace(wsUrl)) return null;
            try
            {
                var s = wsUrl.Trim().Replace("wss://", "https://").Replace("ws://", "http://");
                if (!s.StartsWith("http", StringComparison.OrdinalIgnoreCase)) s = "https://" + s;
                var uri = new Uri(s);
                return $"{uri.Scheme}://{uri.Authority}";
            }
            catch { return null; }
        }

        private (string Origin, string Email) ResolveGit(string projectPath)
        {
            try
            {
                using var repo = new LibGit2Sharp.Repository(projectPath);
                var origin = repo.Network.Remotes["origin"]?.Url;
                var email = repo.Config.Get<string>("user.email")?.Value;
                return (origin, email);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "[Federation] git non leggibile per '{Project}' (non un repo?)", projectPath);
                return (null, null);
            }
        }

        private static IReadOnlyList<AgentRosterEntry> BuildTrustedRoster(IAgentRegistryService registry, string projectPath)
        {
            var catalog = registry.RefreshCatalog(projectPath);
            return catalog
                .Where(e => e.IsCitizen && e.Trusted)
                .Select(e => new AgentRosterEntry
                {
                    Name = e.Name,
                    Role = e.Role,
                    Skills = e.Skills?
                        .Select(s => s.Id)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToList() ?? new List<string>(),
                })
                .ToList();
        }

        private T Safe<T>(Func<T> f) where T : class
        {
            try { return f(); } catch { return null; }
        }

        /// <summary>
        /// Una connessione Socket.IO verso <c>/mdfed</c> dedicata a UNA stanza (il server tiene
        /// lo stato <c>joined</c> per-socket). All'apertura (e a ogni reconnect) presenta il join
        /// token e spedisce l'annuncio cifrato. Tutto best-effort: il relay può essere giù, la
        /// UX non deve risentirne.
        /// </summary>
        private sealed class RoomConnection : IDisposable
        {
            private readonly string _baseUrl;
            private readonly string _enginePath;
            private readonly string _apiKey;
            private readonly ILogger _logger;

            private volatile FederationAnnounce _announce;
            private SocketIOClient.SocketIO _socket;
            private bool _disposed;
            private int _connectFailures;

            /// <summary>Invocato con la busta cifrata di un messaggio in arrivo (evento relay <c>deliver</c>).</summary>
            public Action<string> OnDeliver;

            public RoomConnection(string baseUrl, string enginePath, string apiKey, FederationAnnounce announce, ILogger logger)
            {
                _baseUrl = baseUrl;
                _enginePath = enginePath;
                _apiKey = apiKey;
                _announce = announce;
                _logger = logger;
            }

            public void UpdateAnnounce(FederationAnnounce a)
            {
                _announce = a;
                var s = _socket;
                if (s != null && s.Connected) _ = AnnounceAsync(s, a);
            }

            public void Start()
            {
                try
                {
                    var socket = new SocketIOClient.SocketIO(_baseUrl + "/mdfed", new SocketIOOptions
                    {
                        Path = _enginePath,
                        Auth = new Dictionary<string, string> { ["apiKey"] = _apiKey },
                        ExtraHeaders = new Dictionary<string, string> { ["X-API-Key"] = _apiKey },
                        Reconnection = true,
                        ReconnectionAttempts = int.MaxValue,   // reconnect-forever (supera il cap a 10 della chat)
                        ReconnectionDelay = 2000,
                        ReconnectionDelayMax = 30000,
                    });
                    // A ogni (ri)connessione: join della stanza + annuncio.
                    socket.OnConnected += (s, e) =>
                    {
                        System.Threading.Interlocked.Exchange(ref _connectFailures, 0);
                        _ = JoinAndAnnounceAsync();
                    };
                    socket.OnDisconnected += (s, reason) =>
                        _logger.LogWarning("[Federation] stanza {Room}: disconnesso ({Reason})", _announce?.RoomId, reason);
                    // Un handshake rifiutato (es. API key errata) altrimenti è un retry infinito
                    // PERFETTAMENTE silenzioso: nessuna riga di log, città mai federata. Log
                    // throttlato: il primo fallimento e poi uno ogni 20.
                    socket.OnError += (s, err) =>
                        _logger.LogWarning("[Federation] stanza {Room}: errore dal relay: {Error}", _announce?.RoomId, err);
                    socket.OnReconnectError += (s, ex) =>
                    {
                        var n = System.Threading.Interlocked.Increment(ref _connectFailures);
                        if (n == 1 || n % 20 == 0)
                            _logger.LogWarning("[Federation] stanza {Room}: connessione al relay fallita (tentativo {N}): {Msg}",
                                _announce?.RoomId, n, ex.Message);
                    };
                    // Messaggio federato in arrivo: passa la busta cifrata al service (decifra+gate).
                    socket.On("deliver", resp =>
                    {
                        try
                        {
                            var msg = resp.GetValue<System.Text.Json.JsonElement>(0);
                            if (msg.TryGetProperty("envelope", out var env) && env.ValueKind == System.Text.Json.JsonValueKind.String)
                                OnDeliver?.Invoke(env.GetString());
                        }
                        catch (Exception ex) { _logger.LogWarning(ex, "[Federation] parse 'deliver' fallito"); }
                    });
                    _socket = socket;
                    _ = ConnectAsync(socket);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Federation] avvio connessione stanza fallito");
                }
            }

            private async Task ConnectAsync(SocketIOClient.SocketIO socket)
            {
                try { await socket.ConnectAsync(); }
                catch (Exception ex) { _logger.LogWarning(ex, "[Federation] connessione al relay fallita (riproverà)"); }
            }

            private async Task JoinAndAnnounceAsync()
            {
                var socket = _socket;
                var a = _announce;
                if (socket == null || a == null) return;
                try
                {
                    // L'ordine è garantito sul singolo socket: il server processa join prima di announce.
                    await socket.EmitAsync("join", new { roomId = a.RoomId, ownerId = a.OwnerId, joinToken = a.JoinToken });
                    await AnnounceAsync(socket, a);
                    _logger.LogInformation("[Federation] stanza {Room}: join+announce verso il relay ok", a.RoomId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Federation] join/announce stanza {Room} fallito", a.RoomId);
                }
            }

            private async Task AnnounceAsync(SocketIOClient.SocketIO socket, FederationAnnounce a)
            {
                try { await socket.EmitAsync("announce", new { presence = a.EncryptedPresence }); }
                catch (Exception ex) { _logger.LogWarning(ex, "[Federation] announce stanza {Room} fallito", a.RoomId); }
            }

            /// <summary>
            /// Emette un <c>send</c> mirato (richiesta federata cifrata) verso un'altra città e
            /// <b>attende l'ack del relay</b>: ritorna true SOLO se il relay conferma
            /// <c>ok:true</c> (consegnata alla città online, o accodata per una città spenta —
            /// entrambe consegne durevoli). Un frame perso, un <c>ok:false</c> o un ack mancante
            /// entro il timeout ⇒ false: il chiamante non deve credere spedito ciò che non lo è
            /// (altrimenti brucia un hop per nulla).
            /// </summary>
            public async Task<bool> SendAsync(string toOwnerId, string envelope)
            {
                var socket = _socket;
                if (socket == null || !socket.Connected)
                    throw new InvalidOperationException("connessione alla stanza non attiva: impossibile spedire.");

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                await socket.EmitAsync("send", response =>
                {
                    try
                    {
                        var el = response.GetValue<System.Text.Json.JsonElement>(0);
                        var ok = el.TryGetProperty("ok", out var okEl)
                                 && okEl.ValueKind == System.Text.Json.JsonValueKind.True;
                        tcs.TrySetResult(ok);
                    }
                    catch { tcs.TrySetResult(false); }
                }, new { toOwnerId, envelope });

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using (cts.Token.Register(() => tcs.TrySetResult(false)))
                    return await tcs.Task;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                var s = _socket;
                _socket = null;
                if (s != null)
                {
                    try { _ = s.DisconnectAsync(); } catch { /* best effort */ }
                    try { s.Dispose(); } catch { /* best effort */ }
                }
            }
        }
    }
}
