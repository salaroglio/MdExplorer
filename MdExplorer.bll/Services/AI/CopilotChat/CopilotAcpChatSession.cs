using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.CopilotAcp;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.AI.CopilotChat
{
    /// <summary>
    /// The ACP transport seen through <see cref="ICopilotChatSession"/>.
    ///
    /// An adapter and not an edit of <see cref="CopilotAcpSession"/>: ACP stays reachable only as
    /// the way back if the SDK breaks, so the code that is known to work is the code that must not
    /// move. All this class does is translate one chunk type into the other.
    /// </summary>
    public sealed class CopilotAcpChatSession : ICopilotChatSession
    {
        private readonly CopilotAcpSession _inner;

        public CopilotAcpChatSession(ILogger logger, string workingDirectory, string modelId)
        {
            _inner = new CopilotAcpSession(logger, workingDirectory, modelId);
        }

        public CopilotChatTransport Transport => CopilotChatTransport.Acp;
        public string SessionId => _inner.SessionId;
        public string WorkingDirectory => _inner.WorkingDirectory;
        public string ModelId => _inner.ModelId;
        public bool IsAlive => _inner.IsAlive;
        public DateTime LastUsedUtc => _inner.LastUsedUtc;

        /// <summary>ACP does not report which model answered.</summary>
        public string AnsweredModel => null;

        /// <summary>
        /// ACP applies the model only when the session starts — and even then by sending
        /// <c>/model &lt;id&gt;</c> as the text of a prompt. Changing model means a new session.
        /// </summary>
        public bool CanSwitchModelLive => false;

        public Task StartAsync(CancellationToken ct = default) => _inner.StartAsync(ct);

        public Task SetModelAsync(string modelId, CancellationToken ct = default)
            => throw new NotSupportedException(
                "Il trasporto ACP non sa cambiare modello a conversazione aperta: serve una sessione nuova.");

        public async IAsyncEnumerable<CopilotChatChunk> PromptAsync(
            string text,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var chunk in _inner.PromptAsync(text, ct).ConfigureAwait(false))
            {
                yield return new CopilotChatChunk(ToChatKind(chunk.Kind), chunk.Text);
            }
        }

        public Task CancelAsync(CancellationToken ct = default) => _inner.CancelAsync(ct);

        public ValueTask DisposeAsync() => _inner.DisposeAsync();

        /// <summary>
        /// The two types carry the same two kinds today. Mapped explicitly anyway: were ACP to grow
        /// a third, it must not slip into the answer as if it were text for the user.
        /// </summary>
        private static string ToChatKind(string acpKind) => acpKind switch
        {
            CopilotAcpChunk.KindMessage => CopilotChatChunk.KindMessage,
            CopilotAcpChunk.KindThinking => CopilotChatChunk.KindThinking,
            _ => CopilotChatChunk.KindThinking,
        };
    }
}
