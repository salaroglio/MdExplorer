using System;
using System.Text.Json;

namespace MdExplorer.Features.Services.AI.ChatHistory
{
    /// <summary>
    /// Una riga del file di una chat (sprint 2026-11-24-Chat-Memorizzate, §3.1): un evento come è tornato dal motore, con
    /// il contenuto intero. Il formato è lo stesso per Claude Code, Copilot e opencode.
    /// </summary>
    public sealed class ChatRecordEvent
    {
        public const string KindUserPrompt = "user_prompt";
        public const string KindContext = "context";
        public const string KindText = "text";
        public const string KindThinking = "thinking";
        public const string KindToolCall = "tool_call";
        public const string KindToolResult = "tool_result";
        public const string KindUsage = "usage";
        public const string KindError = "error";
        public const string KindMeta = "meta";

        /// <summary>Il canale del tab; gli altri sono quelli di <c>askOnChannel</c> (commit, diagrammi…).</summary>
        public const string DefaultChannel = "default";

        /// <summary>Progressivo nella chat: l'ordine non dipende dagli orologi.</summary>
        public long Seq { get; set; }
        public int Turn { get; set; }
        public DateTime At { get; set; }
        public string Channel { get; set; } = DefaultChannel;
        public string Kind { get; set; }
        public string Engine { get; set; }
        public string Tool { get; set; }
        /// <summary>Lega una chiamata di strumento al suo risultato.</summary>
        public string CallId { get; set; }
        public bool IsError { get; set; }
        /// <summary>Il testo intero, mai troncato.</summary>
        public string Content { get; set; }
        /// <summary>Il dato strutturato del motore, quando c'è (argomenti, consumi, il blocco titolo/dettaglio).</summary>
        public JsonElement? Json { get; set; }
    }
}
