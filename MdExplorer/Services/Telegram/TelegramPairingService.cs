using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using Microsoft.Extensions.Configuration;

namespace MdExplorer.Services.Telegram
{
    /// <summary>Un abbinamento in corso, come lo vede l'interfaccia.</summary>
    public sealed record TelegramPairingView(
        string State, string Link, DateTimeOffset ExpiresAt, string Error,
        long? TelegramUserId, string TelegramUserName, string BotUsername, bool Recovered);

    /// <summary>
    /// Abbinamento col bot gestore (F1 sul relay, <c>/mdchat/tg</c>): MDE crea una coppia di chiavi RSA, chiede un codice, l'utente
    /// apre il link e preme Avvia, il gestore crea o recupera il bot e consegna il token cifrato con la chiave pubblica. La chiave
    /// privata resta in memoria e muore con l'abbinamento.
    /// <para>
    /// Chi apre il link per primo si abbina: se qualcuno vede il QR di un altro e preme Avvia prima di lui, questo MDE riceverebbe
    /// il bot dell'intruso. Per questo il token NON si salva da solo: lo stato <c>ready</c> mostra chi si è abbinato e serve
    /// <see cref="ConfirmAsync"/>.
    /// </para>
    /// </summary>
    public sealed class TelegramPairingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly TelegramRemoteService _remote;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private Pending _pending;

        private sealed class Pending
        {
            public RSA Key;
            public string Code;
            public string PollSecret;
            public string Link;
            public string Action;
            public DateTimeOffset ExpiresAt;
            public TelegramPairingView Last;
            public string Token;        // decifrato, in memoria finché l'utente non conferma
            public long BotId;
        }

        public TelegramPairingService(IHttpClientFactory httpClientFactory, IConfiguration configuration, TelegramRemoteService remote)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _remote = remote;
        }

        /// <summary>L'indirizzo del gestore: <c>Telegram:ManagerUrl</c>, per esempio https://errantia.net/mdchat/tg.</summary>
        private string ManagerUrl()
        {
            var url = _configuration["Telegram:ManagerUrl"];
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException(UiText.Say(
                    "Manca Telegram:ManagerUrl nella configurazione di MDE: senza, non so a quale gestore chiedere il bot.",
                    "Telegram:ManagerUrl is missing from the MDE configuration: without it I don't know which manager to ask for the bot."));
            return url.TrimEnd('/');
        }

        /// <summary>Comincia un abbinamento (<c>connect</c> o <c>regenerate</c>); quello in corso, se c'è, si abbandona.</summary>
        public async Task<TelegramPairingView> StartAsync(string action, CancellationToken ct)
        {
            if (action != "connect" && action != "regenerate")
                throw new ArgumentException(UiText.Say("Azione sconosciuta: connect oppure regenerate.", "Unknown action: connect or regenerate."));
            await _gate.WaitAsync(ct);
            try
            {
                Drop();
                var key = RSA.Create(2048);
                var response = await Client().PostAsJsonAsync($"{ManagerUrl()}/pairings", new
                {
                    publicKey = Convert.ToBase64String(key.ExportSubjectPublicKeyInfo()),
                    installationName = _remote.InstallationName,
                    action,
                }, ct);
                var body = await ReadAsync(response, ct);
                _pending = new Pending
                {
                    Key = key,
                    Code = body["code"]!.GetValue<string>(),
                    PollSecret = body["pollSecret"]!.GetValue<string>(),
                    Link = body["link"]!.GetValue<string>(),
                    Action = action,
                    ExpiresAt = DateTimeOffset.FromUnixTimeMilliseconds(body["expiresAt"]!.GetValue<long>()),
                };
                _pending.Last = new TelegramPairingView("waiting-start", _pending.Link, _pending.ExpiresAt, null, null, null, null, false);
                return _pending.Last;
            }
            finally { _gate.Release(); }
        }

        /// <summary>Lo stato dell'abbinamento in corso, chiesto al gestore; <c>null</c> se non ce n'è uno.</summary>
        public async Task<TelegramPairingView> PollAsync(CancellationToken ct)
        {
            await _gate.WaitAsync(ct);
            try
            {
                var p = _pending;
                if (p == null) return null;
                if (p.Token != null || p.Last.State == "failed") return p.Last;    // già consegnato: il gestore l'ha cancellato
                if (p.ExpiresAt < DateTimeOffset.UtcNow)
                    return p.Last = p.Last with { State = "expired" };

                using var request = new HttpRequestMessage(HttpMethod.Get, $"{ManagerUrl()}/pairings/{p.Code}");
                request.Headers.Add("X-Pairing-Secret", p.PollSecret);
                var body = await ReadAsync(await Client().SendAsync(request, ct), ct);
                var user = body["telegramUser"];
                var bot = body["bot"];
                var state = body["state"]!.GetValue<string>();
                p.Last = new TelegramPairingView(state, p.Link, p.ExpiresAt, body["error"]?.GetValue<string>(),
                    user?["id"]?.GetValue<long>(), DisplayName(user), bot?["username"]?.GetValue<string>(),
                    body["recovered"]?.GetValue<bool>() ?? false);
                if (state == "ready")
                {
                    var sealedToken = Convert.FromBase64String(body["sealedToken"]!.GetValue<string>());
                    p.Token = Encoding.UTF8.GetString(p.Key.Decrypt(sealedToken, RSAEncryptionPadding.OaepSHA256));
                    p.BotId = bot!["id"]!.GetValue<long>();
                }
                return p.Last;
            }
            finally { _gate.Release(); }
        }

        /// <summary>L'utente ha riconosciuto se stesso nell'abbinamento: il token si salva e il telecomando si accende.</summary>
        public async Task ConfirmAsync(CancellationToken ct)
        {
            await _gate.WaitAsync(ct);
            try
            {
                var p = _pending;
                if (p?.Token == null)
                    throw new InvalidOperationException(UiText.Say(
                        "Non c'è un bot pronto da confermare: l'abbinamento non è finito o è scaduto.",
                        "There is no bot ready to confirm: the pairing has not finished or has expired."));
                await _remote.ConnectAsync(p.Token, p.BotId, p.Last.BotUsername, p.Last.TelegramUserId!.Value, p.Last.TelegramUserName, ct);
                Drop();
            }
            finally { _gate.Release(); }
        }

        public async Task CancelAsync(CancellationToken ct)
        {
            await _gate.WaitAsync(ct);
            try
            {
                var p = _pending;
                if (p != null && p.Token == null && p.Last.State != "failed")
                {
                    using var request = new HttpRequestMessage(HttpMethod.Delete, $"{ManagerUrl()}/pairings/{p.Code}");
                    request.Headers.Add("X-Pairing-Secret", p.PollSecret);
                    using var _ = await Client().SendAsync(request, ct);    // scaduto o già cancellato: va bene lo stesso
                }
                Drop();
            }
            finally { _gate.Release(); }
        }

        private void Drop()
        {
            _pending?.Key.Dispose();
            _pending = null;
        }

        private static string DisplayName(JsonNode user)
        {
            if (user == null) return null;
            var first = user["firstName"]?.GetValue<string>();
            var username = user["username"]?.GetValue<string>();
            return string.IsNullOrEmpty(username) ? first : $"{first} (@{username})";
        }

        private HttpClient Client() => _httpClientFactory.CreateClient(TelegramBotApi.HttpClientName);

        /// <summary>Il corpo JSON, o l'errore del gestore col suo testo (mai un successo finto).</summary>
        private static async Task<JsonNode> ReadAsync(HttpResponseMessage response, CancellationToken ct)
        {
            using (response)
            {
                var text = await response.Content.ReadAsStringAsync(ct);
                JsonNode body = null;
                try { body = JsonNode.Parse(text); } catch (System.Text.Json.JsonException) { }
                if (response.IsSuccessStatusCode && body != null) return body;
                var error = body?["error"]?.GetValue<string>() ?? $"HTTP {(int)response.StatusCode}";
                throw new InvalidOperationException(UiText.Say($"Il gestore Telegram risponde: {error}", $"The Telegram manager replies: {error}"));
            }
        }
    }
}
