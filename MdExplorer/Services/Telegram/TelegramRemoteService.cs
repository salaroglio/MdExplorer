using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.KnowledgeGraph;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Telegram
{
    /// <summary>Lo stato del telecomando di questa installazione (D3).</summary>
    public sealed record TelegramRemoteStatus(
        string State, string InstallationName, string BotUsername, string OwnerName, string LastError);

    /// <summary>
    /// Il telecomando Telegram di questa installazione (F2): ascolta il bot personale con <c>getUpdates</c>, direttamente su
    /// Telegram, e risponde solo al proprietario abbinato.
    /// <para>
    /// Stati (D3): <c>not-connected</c> (nessun bot) → <c>off</c> (interruttore spento) → <c>standby</c> (un'altra installazione
    /// ha preso il bot) → <c>listening</c>. Due <c>getUpdates</c> sullo stesso token si escludono e vince l'ULTIMO arrivato
    /// (prova F0): chi riceve il 409 passa in attesa e non riprova mai da solo; «Prendi il controllo» lo fa tornare in ascolto.
    /// Il ruolo si salva, così dopo un riavvio un'installazione in attesa resta in attesa e non ruba il bot.
    /// </para>
    /// </summary>
    public sealed class TelegramRemoteService : BackgroundService
    {
        private const int LongPollSeconds = 25;

        private readonly TelegramSettingsStore _store;
        private readonly IPasswordProtector _protector;
        private readonly TelegramBotApi _api;
        private readonly ILogger<TelegramRemoteService> _logger;
        private readonly object _lock = new();

        private string _token;
        private string _botUsername;
        private long _ownerId;
        private string _ownerName;
        private bool _enabled;
        private string _role;
        private string _installationName;
        private string _lastError;
        private CancellationTokenSource _listenCts;
        private CancellationToken _stopping;

        public TelegramRemoteService(TelegramSettingsStore store, IPasswordProtector protector, TelegramBotApi api,
            ILogger<TelegramRemoteService> logger)
        {
            _store = store;
            _protector = protector;
            _api = api;
            _logger = logger;
        }

        public string InstallationName => _installationName ?? Environment.MachineName;

        public TelegramRemoteStatus Status
        {
            get
            {
                lock (_lock) return new TelegramRemoteStatus(State(), InstallationName, _botUsername, _ownerName, _lastError);
            }
        }

        private string State()
            => _token == null ? "not-connected" : !_enabled ? "off" : _role == "standby" ? "standby" : "listening";

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stopping = stoppingToken;
            var s = _store.ReadAll();
            lock (_lock)
            {
                _installationName = s.GetValueOrDefault(TelegramSettingsStore.InstallationName);
                var protectedToken = s.GetValueOrDefault(TelegramSettingsStore.TokenProtected);
                if (protectedToken != null)
                {
                    try { _token = _protector.Unprotect(protectedToken); }
                    catch (Exception ex)
                    {
                        // Chiavi di cifratura cambiate (profilo copiato da un altro PC): lo si dice, il bot si recupera con «Collega».
                        _lastError = UiText.Say(
                            "Il token del bot salvato non si riesce a decifrare su questo computer: ricollega il bot da Impostazioni → Telegram.",
                            "The saved bot token cannot be decrypted on this computer: reconnect the bot from Settings → Telegram.");
                        _logger.LogError(ex, "[Telegram] token non decifrabile");
                    }
                }
                _botUsername = s.GetValueOrDefault(TelegramSettingsStore.BotUsername);
                _ownerId = long.TryParse(s.GetValueOrDefault(TelegramSettingsStore.OwnerId), out var id) ? id : 0;
                _ownerName = s.GetValueOrDefault(TelegramSettingsStore.OwnerName);
                _enabled = s.GetValueOrDefault(TelegramSettingsStore.Enabled) == "1";
                _role = s.GetValueOrDefault(TelegramSettingsStore.Role) ?? "listening";
                Reconcile();
            }
            return Task.CompletedTask;
        }

        /// <summary>Salva il bot appena abbinato (confermato dall'utente) e comincia ad ascoltarlo da qui.</summary>
        public async Task ConnectAsync(string token, long botId, string botUsername, long ownerId, string ownerName, CancellationToken ct)
        {
            _store.Write(new Dictionary<string, string>
            {
                [TelegramSettingsStore.TokenProtected] = _protector.Protect(token),
                [TelegramSettingsStore.BotId] = botId.ToString(),
                [TelegramSettingsStore.BotUsername] = botUsername,
                [TelegramSettingsStore.OwnerId] = ownerId.ToString(),
                [TelegramSettingsStore.OwnerName] = ownerName,
                [TelegramSettingsStore.Enabled] = "1",
                [TelegramSettingsStore.Role] = "listening",
            });
            lock (_lock)
            {
                (_token, _botUsername, _ownerId, _ownerName, _enabled, _role, _lastError) =
                    (token, botUsername, ownerId, ownerName, true, "listening", null);
                Reconcile();
            }
            await AnnounceAsync(UiText.Say($"Collegato: ora ti risponde «{InstallationName}».", $"Connected: «{InstallationName}» answers you now."), ct);
        }

        /// <summary>Interruttore «Telecomando acceso / spento».</summary>
        public async Task SetEnabledAsync(bool enabled, CancellationToken ct)
        {
            RequireBot();
            bool wasListening;
            lock (_lock) wasListening = State() == "listening";
            if (!enabled && wasListening)
                await AnnounceAsync(UiText.Say($"Telecomando spento su «{InstallationName}».", $"Remote control switched off on «{InstallationName}»."), ct);
            _store.Write(new Dictionary<string, string>
            {
                [TelegramSettingsStore.Enabled] = enabled ? "1" : "0",
                [TelegramSettingsStore.Role] = enabled ? "listening" : null,
            });
            lock (_lock)
            {
                _enabled = enabled;
                _role = "listening";
                _lastError = null;
                Reconcile();
            }
            if (enabled && !wasListening)
                await AnnounceAsync(UiText.Say($"Ora ti risponde «{InstallationName}».", $"«{InstallationName}» answers you now."), ct);
        }

        /// <summary>«Prendi il controllo»: questa installazione torna ad ascoltare; quella che ascoltava riceve il 409.</summary>
        public Task TakeControlAsync(CancellationToken ct) => SetEnabledAsync(true, ct);

        /// <summary>«Scollega»: il token sparisce da questa installazione (il bot resta dell'utente e si può recuperare).</summary>
        public async Task DisconnectAsync(CancellationToken ct)
        {
            await SetEnabledAsync(false, ct);
            _store.Write(new Dictionary<string, string>
            {
                [TelegramSettingsStore.TokenProtected] = null,
                [TelegramSettingsStore.BotId] = null,
                [TelegramSettingsStore.BotUsername] = null,
                [TelegramSettingsStore.OwnerId] = null,
                [TelegramSettingsStore.OwnerName] = null,
                [TelegramSettingsStore.Enabled] = null,
                [TelegramSettingsStore.Role] = null,
            });
            lock (_lock)
            {
                (_token, _botUsername, _ownerId, _ownerName, _enabled) = (null, null, 0, null, false);
                Reconcile();
            }
        }

        public void SetInstallationName(string name)
        {
            name = name?.Trim();
            if (string.IsNullOrEmpty(name) || name.Length > 64)
                throw new ArgumentException(UiText.Say("Il nome dell'installazione va da 1 a 64 caratteri.", "The installation name must be 1 to 64 characters."));
            _store.Write(TelegramSettingsStore.InstallationName, name);
            lock (_lock) _installationName = name;
        }

        private void RequireBot()
        {
            lock (_lock)
                if (_token == null)
                    throw new InvalidOperationException(UiText.Say(
                        "Nessun bot Telegram collegato a questa installazione: prima «Collega».",
                        "No Telegram bot connected to this installation: «Connect» first."));
        }

        /// <summary>Ferma l'ascolto in corso e, se lo stato lo vuole, ne fa partire uno nuovo. Da chiamare sotto <c>_lock</c>.</summary>
        private void Reconcile()
        {
            _listenCts?.Cancel();
            _listenCts = null;
            if (State() != "listening" || _stopping.IsCancellationRequested) return;
            _listenCts = CancellationTokenSource.CreateLinkedTokenSource(_stopping);
            var token = _token;
            var ct = _listenCts.Token;
            _ = Task.Run(() => ListenAsync(token, ct));
        }

        private async Task ListenAsync(string token, CancellationToken ct)
        {
            long offset = 0;
            _logger.LogInformation("[Telegram] in ascolto su @{Bot}", _botUsername);
            while (!ct.IsCancellationRequested)
            {
                JsonNode updates;
                try
                {
                    updates = await _api.CallAsync(token, "getUpdates",
                        new { offset, timeout = LongPollSeconds, allowed_updates = new[] { "message" } }, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
                catch (TelegramApiException ex) when (ex.ErrorCode == 409)
                {
                    // Un'altra installazione ha preso il bot: in attesa, senza riprovare (niente ping-pong).
                    _logger.LogInformation("[Telegram] un'altra installazione ascolta @{Bot}: passo in attesa", _botUsername);
                    _store.Write(TelegramSettingsStore.Role, "standby");
                    lock (_lock) { _role = "standby"; if (_listenCts?.Token == ct) _listenCts = null; }
                    return;
                }
                catch (TelegramApiException ex) when (ex.ErrorCode == 401)
                {
                    // Token rigenerato da un'altra installazione o bot cancellato: serve ricollegare.
                    _logger.LogWarning("[Telegram] token rifiutato da Telegram (401)");
                    lock (_lock)
                    {
                        _lastError = UiText.Say(
                            "Telegram rifiuta il token del bot (rigenerato altrove o bot cancellato): ricollega il bot da Impostazioni → Telegram.",
                            "Telegram rejects the bot token (regenerated elsewhere or bot deleted): reconnect the bot from Settings → Telegram.");
                        if (_listenCts?.Token == ct) _listenCts = null;
                    }
                    return;
                }
                catch (Exception ex)
                {
                    // Rete assente, sospensione del PC: si riprova, e lo stato lo dice finché non torna.
                    lock (_lock) _lastError = UiText.Say($"Telegram non risponde: {ex.Message}", $"Telegram is not answering: {ex.Message}");
                    try { await Task.Delay(TimeSpan.FromSeconds(10), ct); } catch (OperationCanceledException) { return; }
                    continue;
                }

                lock (_lock) _lastError = null;
                foreach (var update in updates.AsArray())
                {
                    offset = update["update_id"]!.GetValue<long>() + 1;
                    try { await HandleAsync(token, update["message"], ct); }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "[Telegram] update {Id}", offset - 1);
                    }
                }
            }
        }

        /// <summary>
        /// Un messaggio al bot. Solo il proprietario abbinato, in chat privata; gli altri si ignorano (l'accesso è già limitato
        /// anche da Telegram). Per ora risponde sullo stato: il dialogo con Mark arriva con <c>/remote-control</c> (D9).
        /// </summary>
        private async Task HandleAsync(string token, JsonNode message, CancellationToken ct)
        {
            if (message == null) return;
            var from = message["from"]?["id"]?.GetValue<long>();
            var chatType = message["chat"]?["type"]?.GetValue<string>();
            if (from != _ownerId || chatType != "private") return;
            var text = message["text"]?.GetValue<string>()?.Trim() ?? "";
            var reply = text switch
            {
                "/start" => UiText.Say($"Sono MdExplorer su «{InstallationName}». Il telecomando è acceso.",
                    $"I am MdExplorer on «{InstallationName}». Remote control is on."),
                "/stato" or "/status" => UiText.Say($"«{InstallationName}» è in ascolto.", $"«{InstallationName}» is listening."),
                _ => UiText.Say($"Ricevuto da «{InstallationName}». Il dialogo con Mark arriva nella prossima fase.",
                    $"Received by «{InstallationName}». The dialogue with Mark comes in the next phase."),
            };
            await _api.SendMessageAsync(token, _ownerId, reply, ct);
        }

        /// <summary>Un avviso al proprietario sul bot. Se non parte (rete), lo stato lo dice: l'azione resta valida.</summary>
        private async Task AnnounceAsync(string text, CancellationToken ct)
        {
            string token; long owner;
            lock (_lock) (token, owner) = (_token, _ownerId);
            if (token == null || owner == 0) return;
            try { await _api.SendMessageAsync(token, owner, text, ct); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "[Telegram] avviso non consegnato");
                lock (_lock) _lastError = UiText.Say($"Avviso su Telegram non consegnato: {ex.Message}", $"Telegram notice not delivered: {ex.Message}");
            }
        }
    }
}
