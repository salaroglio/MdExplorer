using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Services.Telegram
{
    /// <summary>Un errore della Bot API con il codice di Telegram (409 = un'altra installazione ascolta lo stesso bot).</summary>
    public sealed class TelegramApiException : Exception
    {
        public int ErrorCode { get; }
        public TelegramApiException(string method, int errorCode, string description)
            : base($"Telegram {method}: {errorCode} {description}") { ErrorCode = errorCode; }
    }

    /// <summary>Chiamate alla Bot API col token del bot personale: MDE parla con Telegram direttamente, senza relay.</summary>
    public sealed class TelegramBotApi
    {
        public const string HttpClientName = "Telegram";
        private readonly IHttpClientFactory _httpClientFactory;

        public TelegramBotApi(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

        public async Task<JsonNode> CallAsync(string token, string method, object parameters, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var content = new StringContent(JsonSerializer.Serialize(parameters ?? new { }), Encoding.UTF8, "application/json");
            using var response = await client.PostAsync($"https://api.telegram.org/bot{token}/{method}", content, ct);
            var text = await response.Content.ReadAsStringAsync(ct);
            JsonNode body;
            try { body = JsonNode.Parse(text); }
            catch (JsonException) { throw new TelegramApiException(method, (int)response.StatusCode, "risposta non JSON"); }
            if (body?["ok"]?.GetValue<bool>() != true)
                throw new TelegramApiException(method, body?["error_code"]?.GetValue<int>() ?? (int)response.StatusCode,
                    body?["description"]?.GetValue<string>());
            return body["result"];
        }

        public Task SendMessageAsync(string token, long chatId, string text, CancellationToken ct)
            => CallAsync(token, "sendMessage", new { chat_id = chatId, text }, ct);
    }
}
