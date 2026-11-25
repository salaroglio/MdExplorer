using System.Text.Json;

namespace MdExplorer.Features.Services.AI.ChatHistory
{
    /// <summary>
    /// Una chiamata di strumento, o il suo risultato, com'è tornata dal motore e senza tagli (sprint
    /// 2026-11-24-Chat-Memorizzate, F3). Viaggia accanto alla riga corta che il client mostra: quella resta corta, questa
    /// va nel file della chat.
    /// </summary>
    public sealed class ChatToolRecord
    {
        /// <summary><see cref="ChatRecordEvent.KindToolCall"/> o <see cref="ChatRecordEvent.KindToolResult"/>.</summary>
        public string Kind { get; init; }
        public string Tool { get; init; }
        public string CallId { get; init; }
        /// <summary>Il testo del risultato, intero; null per una chiamata.</summary>
        public string Content { get; init; }
        /// <summary>Gli argomenti della chiamata, o il risultato strutturato quando il motore lo dà.</summary>
        public JsonElement? Json { get; init; }
        public bool IsError { get; init; }

        public static ChatToolRecord Call(string tool, string callId, JsonElement? args) => new()
        {
            Kind = ChatRecordEvent.KindToolCall, Tool = tool, CallId = callId, Json = args,
        };

        public static ChatToolRecord Result(string tool, string callId, string content, JsonElement? structured, bool isError) => new()
        {
            Kind = ChatRecordEvent.KindToolResult, Tool = tool, CallId = callId, Content = content, Json = structured, IsError = isError,
        };
    }
}
