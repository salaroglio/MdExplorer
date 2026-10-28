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

    /// <summary>Come sta la connessione di una stanza col relay.</summary>
    public enum RoomJoinState
    {
        /// <summary>Nessuna connessione aperta per il progetto (città spenta, niente origin o identità).</summary>
        NoRoom,
        /// <summary>Connessione aperta, esito del join non ancora arrivato.</summary>
        Connecting,
        /// <summary>Il relay ha ammesso la città nella stanza.</summary>
        Joined,
        /// <summary>Il relay ha risposto di no: chiave master sbagliata, stanza non registrata, token sbagliato.</summary>
        Rejected,
        /// <summary>Il relay non risponde.</summary>
        Unreachable,
    }

    /// <summary>Stato della stanza di un progetto, con il motivo già leggibile dall'utente.</summary>
    public sealed class RoomStatus
    {
        public RoomJoinState State { get; init; }
        /// <summary>Codice del relay quando rifiuta (<c>room-not-registered</c>, <c>bad-token</c>), altrimenti null.</summary>
        public string Code { get; init; }
        public string Message { get; init; }
    }

    /// <summary>Vista dello stato federato locale, per la UI/rubrica (§12.5).</summary>
    public interface IFederationState
    {
        IReadOnlyList<LocalCity> GetLocalCities();

        /// <summary>
        /// Riapre la connessione della stanza del progetto e aspetta, fino a <paramref name="timeout"/>,
        /// che il relay dica se la ammette. È la «Verifica» delle impostazioni: lo stato vero, non
        /// una deduzione dalla presenza di una chiave.
        /// </summary>
        Task<RoomStatus> CheckRoomAsync(string projectPath, TimeSpan timeout, CancellationToken ct = default);

        /// <summary>
        /// Le città REMOTE attualmente accese sulla stanza del progetto (§12.5), decifrate dal
        /// roster del relay. Lista vuota se il progetto non ha una connessione federata attiva o
        /// nessun'altra città è online. La presenza è effimera: rispecchia lo stato del relay ora.
        /// </summary>
        IReadOnlyList<CityPresence> GetRemotePresence(string projectPath);

        /// <summary>
        /// true se la connessione al relay per la stanza del progetto è attiva ora. Con
        /// <paramref name="projectPath"/> vuoto: true se ALMENO una stanza è connessa.
        /// </summary>
        bool IsRelayConnected(string projectPath);
    }

    /// <summary>
    /// Che cosa ha fatto il relay di una busta (16/10/2026). Prima era un sì/no e «accodata» valeva come «consegnata»:
    /// ma una busta accodata aspetta che l'altro computer si accenda (non scade: 17/10/2026).
    /// </summary>
    public enum RelayDelivery
    {
        /// <summary>Non partita: nessuna connessione attiva, il relay ha detto no o non ha risposto in tempo.</summary>
        NotSent,
        /// <summary>Consegnata adesso alla città di destinazione, che è accesa.</summary>
        Delivered,
        /// <summary>La città di destinazione è spenta: la busta aspetta sul relay, senza scadenza, e arriva quando si accende.</summary>
        Queued,
    }

    /// <summary>Come si legge la risposta del relay a <c>send</c>.</summary>
    public static class RelayAck
    {
        /// <summary>
        /// <c>{ok:true, status:"delivered"}</c> o <c>{ok:true, status:"queued"}</c>. Un ok senza uno stato che si conosce vale
        /// «accodata»: la cosa più prudente da dire, mai una consegna che non si sa.
        /// </summary>
        public static RelayDelivery ReadSend(System.Text.Json.JsonElement ack)
        {
            if (ack.ValueKind != System.Text.Json.JsonValueKind.Object
                || !(ack.TryGetProperty("ok", out var ok) && ok.ValueKind == System.Text.Json.JsonValueKind.True))
                return RelayDelivery.NotSent;
            return ack.TryGetProperty("status", out var status) && status.ValueKind == System.Text.Json.JsonValueKind.String
                   && status.GetString() == "delivered"
                ? RelayDelivery.Delivered
                : RelayDelivery.Queued;
        }
    }

    /// <summary>
    /// Spedisce una richiesta di intervento federata a un'altra città sulla stessa stanza-repo
    /// (§12.6): cifra il payload col room secret e lo emette al relay (<c>send</c> mirato per
    /// ownerId). Dice se la busta è consegnata, accodata o non partita (<see cref="RelayDelivery"/>).
    /// </summary>
    public interface IFederationSender
    {
        Task<RelayDelivery> SendFederatedRequestAsync(string projectPath, string targetOwnerId, FederatedRequestPayload payload);

        /// <summary>
        /// Spedisce l'esito di ritorno di un intervento delegato (Fase 7a): stesso tunnel cifrato
        /// della richiesta, payload distinto (<see cref="FederatedResultPayload"/>). false se il
        /// progetto non ha una connessione federata attiva.
        /// </summary>
        Task<RelayDelivery> SendFederatedResultAsync(string projectPath, string targetOwnerId, FederatedResultPayload payload);

        /// <summary>Il campanello del workflow verso il computer di un collega: «novità nel registro dei giri».</summary>
        Task<RelayDelivery> SendWorkflowNewsAsync(string projectPath, string targetOwnerId, WorkflowNewsPayload payload);

        /// <summary>
        /// Chiude e ricostruisce la connessione della stanza del progetto (test impersonazione):
        /// dopo un cambio di identità effettiva, la macchina si riconnette al relay col nuovo ownerId.
        /// </summary>
        Task ReconnectProjectAsync(string projectPath);
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
        private readonly IProjectRelaySettingsService _relaySettings;   // chiave/URL per progetto

        private volatile IReadOnlyList<LocalCity> _snapshot = Array.Empty<LocalCity>();
        // Una connessione per stanza (roomId). ConcurrentDictionary: scritto dal loop di
        // scansione, letto anche dal thread dell'endpoint RequestIntervention (send).
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, RoomConnection> _rooms = new();
        // Progetti già attivati headless in questa esecuzione: evita il walk FS + Engine DB a ogni scan.
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> _activatedProjects = new();

        // Fase 7: serializza il processing dei deliver PER STANZA. OnDeliver è fire-and-forget
        // (`_ = HandleDeliverAsync(...)`), quindi due buste consecutive girerebbero in concorrenza:
        // ripristina l'ordinamento sequenziale che il vecchio handler sincrono garantiva, così il
        // reinforce/erode della memoria (read-modify-write SPARQL, 7b) e i check-then-write dei
        // receiver non interleavano.
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, System.Threading.SemaphoreSlim> _deliverGates = new();

        public FederationRelayService(
            IServiceScopeFactory scopeFactory,
            IFederationPresenceService presence,
            IHeadlessProjectActivator activator,
            IProjectRelaySettingsService relaySettings,
            IConfiguration configuration,
            ILogger<FederationRelayService> logger)
        {
            _scopeFactory = scopeFactory;
            _presence = presence;
            _activator = activator;
            _relaySettings = relaySettings;
            _logger = logger;

            // Il relay è lo stesso server del canale chat: base host + engine path condivisi,
            // la federazione vive nel namespace /mdfed. Override futuro via agentCity.relayUrl.
            var wsUrl = configuration["MdChat:WebSocketUrl"] ?? "wss://errantia.net/mdchat";
            _baseUrl = wsUrl
                .Replace("wss://", "https://")
                .Replace("ws://", "http://")
                .Replace("/mdchat", "")
                .TrimEnd('/');
            // NB: la chiave master del relay NON si legge qui. È facoltativa (serve solo a registrare
            // una stanza nuova), per progetto con ricaduta sulla globale, e va risolta al momento di
            // aprire la stanza: un cambio dalle impostazioni ha effetto al prossimo scan.
        }

        public IReadOnlyList<LocalCity> GetLocalCities() => _snapshot;

        /// <inheritdoc/>
        public IReadOnlyList<CityPresence> GetRemotePresence(string projectPath)
        {
            var city = _snapshot.FirstOrDefault(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));
            if (city == null || !_rooms.TryGetValue(city.RoomId, out var conn))
                return Array.Empty<CityPresence>();

            var envelopes = conn.RemotePresenceEnvelopes;
            if (envelopes.Count == 0)
                return Array.Empty<CityPresence>();

            string secret;
            using (var scope = _scopeFactory.CreateScope())
            {
                var meta = scope.ServiceProvider.GetRequiredService<MdExplorer.Services.IProjectMetadataService>();
                secret = meta.GetAgentCity(projectPath)?.RoomSecret;
            }
            if (string.IsNullOrWhiteSpace(secret))
                return Array.Empty<CityPresence>();

            var result = new List<CityPresence>(envelopes.Count);
            foreach (var kv in envelopes)
            {
                try
                {
                    var json = FederationCrypto.Decrypt(secret, city.RoomId, kv.Value);
                    var presence = System.Text.Json.JsonSerializer.Deserialize<CityPresence>(json);
                    if (presence == null) continue;
                    if (string.IsNullOrWhiteSpace(presence.OwnerId)) presence.OwnerId = kv.Key;
                    result.Add(presence);
                }
                catch (FederationCryptoException ex)
                {
                    _logger.LogWarning(ex, "[Federation] presenza remota di {Owner} non apribile (secret errato/manomessa).", kv.Key);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Federation] presenza remota di {Owner} malformata dopo decrypt.", kv.Key);
                }
            }
            return result;
        }

        /// <inheritdoc/>
        public async Task<RoomStatus> CheckRoomAsync(string projectPath, TimeSpan timeout, CancellationToken ct = default)
        {
            await ReconnectProjectAsync(projectPath);
            var until = DateTime.UtcNow + timeout;
            while (true)
            {
                var status = CurrentRoomStatus(projectPath);
                // NoRoom subito dopo il reconnect è normale: la scansione non ha ancora riaperto.
                if (status.State is RoomJoinState.Joined or RoomJoinState.Rejected or RoomJoinState.Unreachable)
                    return status;
                if (DateTime.UtcNow >= until)
                    return status.State == RoomJoinState.NoRoom
                        ? new RoomStatus { State = RoomJoinState.NoRoom, Message = MdExplorer.Features.Services.UiText.Say("La città di questo progetto non ha una stanza aperta: serve la città attiva, un origin git e l'email dell'identità.", "The city of this project has no open room: it needs the city on, a git origin and the identity email.") }
                        : new RoomStatus { State = RoomJoinState.Unreachable, Message = MdExplorer.Features.Services.UiText.Say($"Il relay non ha risposto entro {timeout.TotalSeconds:0} secondi.", $"The relay did not answer within {timeout.TotalSeconds:0} seconds.") };
                await Task.Delay(250, ct);
            }
        }

        private RoomStatus CurrentRoomStatus(string projectPath)
        {
            var city = _snapshot.FirstOrDefault(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));
            if (city == null || !_rooms.TryGetValue(city.RoomId, out var conn))
                return new RoomStatus { State = RoomJoinState.NoRoom };
            return conn.Status;
        }

        /// <inheritdoc/>
        public bool IsRelayConnected(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return _rooms.Values.Any(r => r.Connected);
            var city = _snapshot.FirstOrDefault(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));
            return city != null && _rooms.TryGetValue(city.RoomId, out var conn) && conn.Connected;
        }

        // Segnale per una ri-scansione IMMEDIATA (es. cambio identità impersonata → riconnetti la
        // stanza col nuovo ownerId senza aspettare il tick da 60s).
        private readonly System.Threading.SemaphoreSlim _rescanSignal = new(0, int.MaxValue);

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
                // Attende il tick OPPURE un segnale di rescan immediato (cambio identità).
                try { await Task.WhenAny(Task.Delay(ScanInterval, stoppingToken), _rescanSignal.WaitAsync(stoppingToken)); }
                catch (OperationCanceledException) { break; }
            }
        }

        /// <inheritdoc/>
        public Task ReconnectProjectAsync(string projectPath)
        {
            // Chiude la connessione della stanza del progetto: la prossima scansione la ricostruisce
            // con l'identità effettiva corrente (il join fissa l'ownerId → non basta ri-annunciare).
            var city = _snapshot.FirstOrDefault(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));
            if (city != null && _rooms.TryRemove(city.RoomId, out var conn))
            {
                try { conn.Dispose(); } catch { /* best effort */ }
                _logger.LogInformation("[Federation] stanza {Room} chiusa per riaprirla (cambio identità o verifica).", city.RoomId);
            }
            try { _rescanSignal.Release(); } catch { /* già al massimo */ }
            return Task.CompletedTask;
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
            var effectiveIdentity = scope.ServiceProvider.GetRequiredService<IEffectiveOwnerIdentity>();

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

                // origin = da git; email = dal SEAM identità (impersonata in modalità test, altrimenti
                // la git email reale). È qui che la macchina si presenta al relay come l'ownerId di X.
                var (origin, _) = ResolveGit(project.Path);
                var email = effectiveIdentity.ResolveEmail(project.Path);
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
        /// cifrato. Le stanze non più attive vengono chiuse. Si entra col join token (il room secret
        /// sta in git); la chiave master del relay, se c'è, serve a registrare una stanza nuova.
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

            // Apri/aggiorna le connessioni delle stanze attive.
            foreach (var (roomId, announce) in desired)
            {
                // Chiave master del relay: FACOLTATIVA. Per progetto (UserDB) con ricaduta sulla
                // globale MdChat:ApiKey; senza, si entra col solo join token di una stanza già
                // registrata. Se manca e la stanza è nuova, il rifiuto del relay lo dice (Status).
                var apiKey = _relaySettings.ResolveApiKey(announce.ProjectPath);
                if (string.IsNullOrWhiteSpace(apiKey)) apiKey = null;

                // Onora l'override per-progetto del relay: la connessione va sul relay
                // dichiarato (scheme+host), non sempre su quello globale. Se assente/
                // non parsabile, ricade sul base globale.
                var baseUrl = ToHttpBase(announce.RelayUrl) ?? _baseUrl;

                if (_rooms.TryGetValue(roomId, out var existing))
                {
                    // Chiave/indirizzo sono catturati alla costruzione della connessione: se
                    // l'utente li cambia dalle impostazioni, una connessione già aperta
                    // continuerebbe a ritentare con quelli vecchi PER SEMPRE (osservato dal vivo:
                    // salvata la chiave giusta, la stanza restava su "Invalid API key"). Quindi
                    // qui si confronta e, se sono cambiati, si riapre.
                    if (existing.Matches(baseUrl, apiKey))
                    {
                        existing.UpdateAnnounce(announce);   // presenza cambiata? ri-annuncia
                        continue;
                    }

                    CloseRoom(roomId, "chiave o indirizzo del relay cambiati");
                }

                var conn = new RoomConnection(baseUrl, EnginePath, apiKey, announce, _logger);
                var captured = announce;             // per la closure del deliver
                conn.OnDeliver = env => { _ = HandleDeliverAsync(captured, env); };
                _rooms[roomId] = conn;
                conn.Start();                        // fire-and-forget: OnConnected fa join+announce
            }

            return Task.CompletedTask;
        }

        /// <summary>Chiude e dimentica una stanza aperta, spiegando nel log il perché.</summary>
        private void CloseRoom(string roomId, string reason)
        {
            if (!_rooms.TryRemove(roomId, out var conn)) return;
            try { conn.Dispose(); } catch { /* best effort */ }
            _logger.LogInformation("[Federation] stanza {Room} chiusa: {Reason}", roomId, reason);
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
            // Serializza per stanza: la busta N+1 attende il completamento di N (memoria/idempotenza).
            var gate = _deliverGates.GetOrAdd(announce.RoomId ?? string.Empty, _ => new System.Threading.SemaphoreSlim(1, 1));
            await gate.WaitAsync();
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
                if (string.Equals(kind, FederationKind.WorkflowNews, StringComparison.OrdinalIgnoreCase))
                {
                    var news = System.Text.Json.JsonSerializer.Deserialize<WorkflowNewsPayload>(json);
                    _logger.LogInformation("[Federation] campanello del workflow da {From} (giro {Round}): scarico il registro.", news?.FromOwner, news?.Round);
                    scope.ServiceProvider.GetRequiredService<MdExplorer.Services.AgentRun.IWorkflowPoller>().Ring(announce.ProjectPath);
                    return;
                }
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
            finally { gate.Release(); }
        }

        /// <summary>
        /// Spedisce una richiesta federata (§12.6): cifra il payload col room secret del progetto
        /// e lo emette al relay (<c>send</c> mirato per <paramref name="targetOwnerId"/>) sulla
        /// connessione della stanza. <see cref="RelayDelivery.NotSent"/> se il progetto non ha una connessione federata attiva.
        /// </summary>
        public Task<RelayDelivery> SendFederatedRequestAsync(string projectPath, string targetOwnerId, FederatedRequestPayload payload)
            => SendEnvelopeAsync(projectPath, targetOwnerId, System.Text.Json.JsonSerializer.Serialize(payload));

        /// <inheritdoc/>
        public Task<RelayDelivery> SendFederatedResultAsync(string projectPath, string targetOwnerId, FederatedResultPayload payload)
            => SendEnvelopeAsync(projectPath, targetOwnerId, System.Text.Json.JsonSerializer.Serialize(payload));

        /// <inheritdoc/>
        public Task<RelayDelivery> SendWorkflowNewsAsync(string projectPath, string targetOwnerId, WorkflowNewsPayload payload)
            => SendEnvelopeAsync(projectPath, targetOwnerId, System.Text.Json.JsonSerializer.Serialize(payload));

        // Core condiviso: cifra il JSON già serializzato del payload TIPATO col room secret del
        // progetto e lo emette al relay. Serializzare qui su `object` perderebbe le proprietà
        // (System.Text.Json guarda il tipo statico), perciò ogni overload serializza il proprio
        // tipo concreto e passa qui la stringa.
        private async Task<RelayDelivery> SendEnvelopeAsync(string projectPath, string targetOwnerId, string payloadJson)
        {
            var city = _snapshot.FirstOrDefault(c => AgentPathComparer.Equals(c.ProjectPath, projectPath));
            if (city == null || !_rooms.TryGetValue(city.RoomId, out var conn))
            {
                _logger.LogWarning("[Federation] send federato: nessuna connessione attiva per '{Project}'.", projectPath);
                return RelayDelivery.NotSent;
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
                return RelayDelivery.NotSent;
            }

            var envelope = FederationCrypto.Encrypt(secret, city.RoomId, payloadJson);
            try
            {
                // Il relay dice se l'ha consegnata (città accesa) o accodata (città spenta, senza scadenza).
                var outcome = await conn.SendAsync(targetOwnerId, envelope);
                if (outcome == RelayDelivery.NotSent)
                    _logger.LogWarning("[Federation] send federato per '{Project}': il relay non ha confermato (perso/ok:false/timeout).", projectPath);
                else if (outcome == RelayDelivery.Queued)
                    _logger.LogInformation("[Federation] send federato per '{Project}': la città {Owner} è spenta, la busta aspetta sul relay.", projectPath, targetOwnerId);
                return outcome;
            }
            catch (Exception ex)
            {
                // Contratto: "false se nessuna connessione attiva" — vale anche quando la
                // RoomConnection esiste ma il socket è giù (relay caduto, mid-reconnect):
                // il chiamante deve vedere un 503 ritentabile, non un 500 generico.
                _logger.LogWarning(ex, "[Federation] send federato fallito per '{Project}' (socket non attivo?)", projectPath);
                return RelayDelivery.NotSent;
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
            private volatile RoomStatus _status = new() { State = RoomJoinState.Connecting };

            /// <summary>L'ultimo esito noto della connessione: ammessa, rifiutata (col motivo), irraggiungibile.</summary>
            public RoomStatus Status => _status;

            /// <summary>Cambia lo stato e lo scrive nel log solo quando cambia: i rifiuti si ripetono a ogni reconnect.</summary>
            private void SetStatus(RoomJoinState state, string message, string code = null)
            {
                var old = _status;
                _status = new RoomStatus { State = state, Message = message, Code = code };
                if (old.State == state && old.Code == code) return;
                if (state == RoomJoinState.Joined)
                    _logger.LogInformation("[Federation] stanza {Room}: {Message}", _announce?.RoomId, message);
                else
                    _logger.LogWarning("[Federation] stanza {Room}: {Message}", _announce?.RoomId, message);
            }

            /// <summary>Invocato con la busta cifrata di un messaggio in arrivo (evento relay <c>deliver</c>).</summary>
            public Action<string> OnDeliver;

            // Presenza remota della stanza: ownerId → busta cifrata dell'annuncio (CityPresence).
            // Popolata dal roster ritornato nell'ack di 'join' e mantenuta dagli eventi 'presence'
            // (città accesa/riannunciata) e 'offline' (ultima connessione della città caduta). Le
            // buste restano cifrate qui: la decifratura vive nel service (ha lo scope col room secret).
            private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _remotePresence = new();

            /// <summary>Snapshot delle presenze remote note: (ownerId, busta cifrata). Mai la mia città.</summary>
            public IReadOnlyList<KeyValuePair<string, string>> RemotePresenceEnvelopes => _remotePresence.ToArray();

            /// <summary>true se il socket verso il relay è connesso E il relay ha ammesso la città nella stanza.</summary>
            public bool Connected => _socket?.Connected == true && _status.State == RoomJoinState.Joined;

            public RoomConnection(string baseUrl, string enginePath, string apiKey, FederationAnnounce announce, ILogger logger)
            {
                _baseUrl = baseUrl;
                _enginePath = enginePath;
                _apiKey = apiKey;
                _announce = announce;
                _logger = logger;
            }

            /// <summary>
            /// true se questa connessione sta già usando esattamente questo relay e questa chiave.
            /// Serve al presidio per accorgersi che l'utente li ha cambiati dalle impostazioni:
            /// sono catturati alla costruzione, quindi l'unico modo di applicarli è riaprire.
            /// </summary>
            public bool Matches(string baseUrl, string apiKey)
                => string.Equals(_baseUrl, baseUrl, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(_apiKey, apiKey, StringComparison.Ordinal);   // null == null: entrambe senza chiave

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
                    // La chiave master va al relay solo se c'è: senza, si entra col join token.
                    var socket = new SocketIOClient.SocketIO(_baseUrl + "/mdfed", new SocketIOOptions
                    {
                        Path = _enginePath,
                        Auth = _apiKey == null ? null : new Dictionary<string, string> { ["apiKey"] = _apiKey },
                        ExtraHeaders = _apiKey == null ? null : new Dictionary<string, string> { ["X-API-Key"] = _apiKey },
                        Reconnection = true,
                        ReconnectionAttempts = int.MaxValue,   // reconnect-forever (supera il cap a 10 della chat)
                        ReconnectionDelay = 2000,
                        ReconnectionDelayMax = 30000,
                    });
                    // A ogni (ri)connessione: join della stanza + annuncio.
                    socket.OnConnected += (s, e) =>
                    {
                        System.Threading.Interlocked.Exchange(ref _connectFailures, 0);
                        _status = new RoomStatus { State = RoomJoinState.Connecting };   // l'esito lo dà l'ack di 'join'
                        _ = JoinAndAnnounceAsync();
                    };
                    socket.OnDisconnected += (s, reason) =>
                    {
                        // La presenza remota è effimera lato relay: a ogni rejoin l'ack di 'join'
                        // rimanda il roster completo. Svuota per non mostrare città come accese dopo
                        // una caduta della NOSTRA connessione (non sapremmo dei loro 'offline').
                        _remotePresence.Clear();
                        _logger.LogWarning("[Federation] stanza {Room}: disconnesso ({Reason})", _announce?.RoomId, reason);
                    };
                    // Un handshake rifiutato (es. API key errata) altrimenti è un retry infinito
                    // PERFETTAMENTE silenzioso: nessuna riga di log, città mai federata. Log
                    // throttlato: il primo fallimento e poi uno ogni 20.
                    socket.OnError += (s, err) =>
                        SetStatus(RoomJoinState.Rejected, err == "Invalid API key"
                            ? MdExplorer.Features.Services.UiText.Say("il relay ha rifiutato la chiave master: non è quella giusta (toglila o correggila nelle impostazioni del progetto).", "the relay refused the master key: it is not the right one (remove or fix it in the project settings).")
                            : MdExplorer.Features.Services.UiText.Say($"il relay ha rifiutato la connessione: {err}", $"the relay refused the connection: {err}"));
                    socket.OnReconnectError += (s, ex) =>
                    {
                        var n = System.Threading.Interlocked.Increment(ref _connectFailures);
                        SetStatus(RoomJoinState.Unreachable, MdExplorer.Features.Services.UiText.Say($"relay irraggiungibile: {ex.Message}", $"relay unreachable: {ex.Message}"));
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
                    // Presenza: una città della stanza si è accesa o ha riannunciato il suo catalogo.
                    socket.On("presence", resp =>
                    {
                        try
                        {
                            var msg = resp.GetValue<System.Text.Json.JsonElement>(0);
                            if (msg.TryGetProperty("ownerId", out var oid) && oid.ValueKind == System.Text.Json.JsonValueKind.String
                                && msg.TryGetProperty("presence", out var env) && env.ValueKind == System.Text.Json.JsonValueKind.String)
                                _remotePresence[oid.GetString()] = env.GetString();
                        }
                        catch (Exception ex) { _logger.LogWarning(ex, "[Federation] parse 'presence' fallito"); }
                    });
                    // Offline: l'ultima connessione di una città della stanza è caduta.
                    socket.On("offline", resp =>
                    {
                        try
                        {
                            var msg = resp.GetValue<System.Text.Json.JsonElement>(0);
                            if (msg.TryGetProperty("ownerId", out var oid) && oid.ValueKind == System.Text.Json.JsonValueKind.String)
                                _remotePresence.TryRemove(oid.GetString(), out _);
                        }
                        catch (Exception ex) { _logger.LogWarning(ex, "[Federation] parse 'offline' fallito"); }
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
                catch (Exception ex)
                {
                    SetStatus(RoomJoinState.Unreachable, MdExplorer.Features.Services.UiText.Say($"relay irraggiungibile: {ex.Message}", $"relay unreachable: {ex.Message}"));
                    _logger.LogWarning(ex, "[Federation] connessione al relay fallita (riproverà)");
                }
            }

            private async Task JoinAndAnnounceAsync()
            {
                var socket = _socket;
                var a = _announce;
                if (socket == null || a == null) return;
                try
                {
                    // L'ack di 'join' dice se il relay ci ammette e porta il roster della stanza (chi è
                    // già acceso): lo assorbiamo come stato iniziale della presenza remota, poi
                    // 'presence'/'offline' lo mantengono. L'annuncio parte SOLO dopo un sì: prima si
                    // ignorava il rifiuto e il log diceva «ok» a una città mai entrata.
                    var admitted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    await socket.EmitAsync("join", response =>
                    {
                        try
                        {
                            var ack = response.GetValue<System.Text.Json.JsonElement>(0);
                            if (!(ack.TryGetProperty("ok", out var ok) && ok.ValueKind == System.Text.Json.JsonValueKind.True))
                            {
                                var code = ack.TryGetProperty("code", out var c) && c.ValueKind == System.Text.Json.JsonValueKind.String ? c.GetString() : null;
                                var error = ack.TryGetProperty("error", out var e) && e.ValueKind == System.Text.Json.JsonValueKind.String ? e.GetString() : "rifiuto senza motivo";
                                SetStatus(RoomJoinState.Rejected, DescribeRejection(code, error), code);
                                admitted.TrySetResult(false);
                                return;
                            }
                            var registered = ack.TryGetProperty("registered", out var r) && r.ValueKind == System.Text.Json.JsonValueKind.True;
                            SetStatus(RoomJoinState.Joined, registered
                                ? MdExplorer.Features.Services.UiText.Say("stanza registrata sul relay con la chiave master: da ora chi ha il repo entra col solo room secret.", "room registered on the relay with the master key: from now on whoever has the repo joins with the room secret alone.")
                                : MdExplorer.Features.Services.UiText.Say("il relay ha ammesso la città nella stanza.", "the relay admitted the city into the room."));
                            _remotePresence.Clear();
                            if (ack.TryGetProperty("roster", out var roster) && roster.ValueKind == System.Text.Json.JsonValueKind.Array)
                            {
                                foreach (var e in roster.EnumerateArray())
                                {
                                    if (e.TryGetProperty("ownerId", out var oid) && oid.ValueKind == System.Text.Json.JsonValueKind.String
                                        && e.TryGetProperty("presence", out var env) && env.ValueKind == System.Text.Json.JsonValueKind.String)
                                        _remotePresence[oid.GetString()] = env.GetString();
                                }
                            }
                        }
                        catch (Exception ex) { _logger.LogWarning(ex, "[Federation] parse roster di 'join' fallito"); }
                        admitted.TrySetResult(true);
                    }, new { roomId = a.RoomId, ownerId = a.OwnerId, joinToken = a.JoinToken });
                    if (!await admitted.Task) return;
                    await AnnounceAsync(socket, a);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[Federation] join/announce stanza {Room} fallito", a.RoomId);
                }
            }

            /// <summary>Il rifiuto del relay detto all'utente con cosa fare.</summary>
            private static string DescribeRejection(string code, string error) => code switch
            {
                "room-not-registered" => MdExplorer.Features.Services.UiText.Say("la stanza non è ancora registrata sul relay. La prima volta serve la chiave master del relay: inseriscila nelle impostazioni del progetto (Città degli agenti), poi si può togliere.", "the room is not registered on the relay yet. The first time the relay master key is needed: enter it in the project settings (Agent city), then it can be removed."),
                "bad-token" => MdExplorer.Features.Services.UiText.Say("il relay conosce questa stanza con un altro room secret: controlla che agentCity.roomSecret in .development.yml sia quello del repo (fai il pull).", "the relay knows this room with a different room secret: check that agentCity.roomSecret in .development.yml is the one in the repo (pull)."),
                _ => MdExplorer.Features.Services.UiText.Say($"il relay ha rifiutato l'ingresso nella stanza: {error}", $"the relay refused entry into the room: {error}"),
            };

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
            public async Task<RelayDelivery> SendAsync(string toOwnerId, string envelope)
            {
                var socket = _socket;
                if (socket == null || !socket.Connected)
                    throw new InvalidOperationException(MdExplorer.Features.Services.UiText.Say("connessione alla stanza non attiva: impossibile spedire.", "connection to the room not active: cannot send."));

                var tcs = new TaskCompletionSource<RelayDelivery>(TaskCreationOptions.RunContinuationsAsynchronously);
                await socket.EmitAsync("send", response =>
                {
                    try { tcs.TrySetResult(RelayAck.ReadSend(response.GetValue<System.Text.Json.JsonElement>(0))); }
                    catch { tcs.TrySetResult(RelayDelivery.NotSent); }
                }, new { toOwnerId, envelope });

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                using (cts.Token.Register(() => tcs.TrySetResult(RelayDelivery.NotSent)))
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
