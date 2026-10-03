using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MdExplorer.IntegrationTests.Infrastructure
{
    /// <summary>Esito di una chiamata JSON-RPC al gateway A2A: o un result, o un codice d'errore.</summary>
    public sealed class RpcResult
    {
        public int? ErrorCode { get; init; }
        public string ErrorMessage { get; init; }
        public JsonElement? Result { get; init; }
        public bool IsError => ErrorCode.HasValue;
    }

    /// <summary>Helper per parlare col gateway JSON-RPC 2.0 (<c>POST /a2a/{projectKey}/{agent}</c>).</summary>
    public static class GatewayRpc
    {
        public static async Task<RpcResult> SendMessage(
            HttpClient client, System.Guid projectKey, string toAgent, string text,
            string fromAgent = null, IEnumerable<string> topics = null)
        {
            var metadata = new Dictionary<string, object>();
            if (fromAgent != null) metadata["fromAgent"] = fromAgent;
            if (topics != null) metadata["topics"] = topics;

            var message = new Dictionary<string, object>
            {
                ["parts"] = new[] { new Dictionary<string, object> { ["text"] = text } },
            };
            if (metadata.Count > 0) message["metadata"] = metadata;

            var envelope = new Dictionary<string, object>
            {
                ["jsonrpc"] = "2.0",
                ["id"] = 1,
                ["method"] = "message/send",
                ["params"] = new Dictionary<string, object> { ["message"] = message },
            };

            var json = JsonSerializer.Serialize(envelope);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync($"/a2a/{projectKey}/{toAgent}", content);
            var body = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            if (root.TryGetProperty("error", out var error))
            {
                return new RpcResult
                {
                    ErrorCode = error.GetProperty("code").GetInt32(),
                    ErrorMessage = error.TryGetProperty("message", out var m) ? m.GetString() : null,
                };
            }
            return new RpcResult { Result = root.GetProperty("result").Clone() };
        }
    }
}
