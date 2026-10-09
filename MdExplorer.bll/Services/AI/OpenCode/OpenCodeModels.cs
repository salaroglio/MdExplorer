namespace MdExplorer.Features.Services.AI.OpenCode
{
    /// <summary>
    /// Un frammento del turno, nella stessa forma di <c>ClaudeCodeChunk</c>: l'hub della chat
    /// li traduce tutti negli stessi eventi SignalR, così il frontend non deve sapere quale
    /// motore sta parlando.
    /// </summary>
    public readonly struct OpenCodeChunk
    {
        public string Kind { get; }
        public string Text { get; }
        public OpenCodeChunk(string kind, string text) { Kind = kind; Text = text; }

        /// <summary>Testo visibile (evento <c>message.part.delta</c> con <c>field: "text"</c>).</summary>
        public const string KindMessage = "message";

        /// <summary>Ragionamento (delta con <c>field: "reasoning"</c>).</summary>
        public const string KindThinking = "thinking";

        /// <summary>
        /// Attività sui tool: «sta leggendo X». Riga di stato, non risposta, e non entra nel
        /// testo finale.
        /// </summary>
        public const string KindTool = "tool";
    }

    /// <summary>
    /// Consuntivo del turno, letto dalla risposta <b>sincrona</b> di
    /// <c>POST /session/{id}/message</c>: opencode la restituisce a turno finito con il modello
    /// che ha risposto, i token e il costo.
    /// <para>
    /// A differenza di Claude Code qui non esistono finestre di consumo (<c>rate_limit</c>), e
    /// i numeri sono <b>del turno</b>, non cumulati: misurato il 21/09/2026 su opencode 1.18.30.
    /// </para>
    /// </summary>
    public sealed class OpenCodeTurnUsage
    {
        /// <summary>Provider che ha risposto (<c>opencode</c>, …).</summary>
        public string ProviderId { get; init; }

        /// <summary>Modello che ha risposto: quello vero, che può non essere quello chiesto.</summary>
        public string ModelId { get; init; }

        public long? InputTokens { get; init; }
        public long? OutputTokens { get; init; }
        public long? ReasoningTokens { get; init; }
        public long? CacheReadTokens { get; init; }
        public long? CacheWriteTokens { get; init; }
        public long? TotalTokens { get; init; }

        /// <summary>
        /// Costo del turno. Con i modelli gratuiti di OpenCode Zen è <c>0</c>, e resta <c>0</c>:
        /// non è un «non lo so» travestito da zero.
        /// </summary>
        public double? CostUsd { get; init; }

        public long? DurationMs { get; init; }
    }
}
