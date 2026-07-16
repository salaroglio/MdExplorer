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
    public class FederationRelayService : BackgroundService, IFederationState
    {
        private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(60);
        // Path dell'engine Socket.IO sul relay (dietro nginx), condiviso col canale chat.
        private const string EnginePath = "/mdchat/socket.io";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFederationPresenceService _presence;
        private readonly ILogger<FederationRelayService> _logger;
        private readonly string _baseUrl;   // es. https://errantia.net (namespace /mdfed appeso)
        private readonly string _apiKey;    // gate di base del relay (stesso del canale chat)

        private volatile IReadOnlyList<LocalCity> _snapshot = Array.Empty<LocalCity>();
        // Una connessione per stanza (roomId). Toccato solo dal loop di scansione (single-thread).
        private readonly Dictionary<string, RoomConnection> _rooms = new();
        private bool _warnedNoApiKey;

        public FederationRelayService(
            IServiceScopeFactory scopeFactory,
            IFederationPresenceService presence,
            IConfiguration configuration,
            ILogger<FederationRelayService> logger)
        {
            _scopeFactory = scopeFactory;
            _presence = presence;
            _logger = logger;

            // Il relay è lo stesso server del canale chat: base host + engine path condivisi,
            // la federazione vive nel namespace /mdfed. Override futuro via agentCity.relayUrl.
            var wsUrl = configuration["MdChat:WebSocketUrl"] ?? "wss://errantia.net/mdchat";
            _baseUrl = wsUrl
                .Replace("wss://", "https://")
                .Replace("ws://", "http://")
                .Replace("/mdchat", "")
                .TrimEnd('/');
            _apiKey = configuration["MdChat:ApiKey"];   // può mancare: allora resta dormiente
        }

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
            var desired = announces
                .Where(a => a != null && !string.IsNullOrWhiteSpace(a.RoomId))
                .GroupBy(a => a.RoomId)
                .ToDictionary(g => g.Key, g => g.First());

            // Chiudi le connessioni delle stanze non più attive.
            foreach (var roomId in _rooms.Keys.Where(k => !desired.ContainsKey(k)).ToList())
            {
                try { _rooms[roomId].Dispose(); } catch { /* best effort */ }
                _rooms.Remove(roomId);
                _logger.LogInformation("[Federation] stanza {Room} chiusa (città non più attiva)", roomId);
            }

            if (desired.Count == 0)
                return Task.CompletedTask;

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                if (!_warnedNoApiKey)
                {
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
                    var conn = new RoomConnection(_baseUrl, EnginePath, _apiKey, announce, _logger);
                    var captured = announce;             // per la closure del deliver
                    conn.OnDeliver = env => HandleDeliver(captured, env);
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
        private void HandleDeliver(FederationAnnounce announce, string envelope)
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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var conn in _rooms.Values)
            {
                try { conn.Dispose(); } catch { /* best effort */ }
            }
            _rooms.Clear();
            await base.StopAsync(cancellationToken);
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
                    socket.OnConnected += (s, e) => { _ = JoinAndAnnounceAsync(); };
                    socket.OnDisconnected += (s, reason) =>
                        _logger.LogWarning("[Federation] stanza {Room}: disconnesso ({Reason})", _announce?.RoomId, reason);
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
