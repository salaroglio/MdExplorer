using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.AI.CopilotChat
{
    /// <summary>
    /// How the chat talks to the Copilot CLI.
    /// </summary>
    public enum CopilotChatTransport
    {
        /// <summary>The official .NET SDK (<c>GitHub.Copilot.SDK</c>). The default.</summary>
        Sdk,

        /// <summary>
        /// The hand-written ACP adapter (<c>copilot --acp</c>). Kept reachable on purpose: the SDK
        /// is in public preview, and when it breaks the way back must not wait for a release.
        /// </summary>
        Acp
    }

    /// <summary>
    /// One chunk of a streamed answer, whatever transport produced it.
    /// </summary>
    public readonly struct CopilotChatChunk
    {
        public string Kind { get; }
        public string Text { get; }
        public CopilotChatChunk(string kind, string text) { Kind = kind; Text = text; }

        /// <summary>Text the user is meant to read.</summary>
        public const string KindMessage = "message";

        /// <summary>Reasoning: shown apart, and never mixed into the answer.</summary>
        public const string KindThinking = "thinking";
    }

    /// <summary>
    /// A live Copilot conversation, as the chat hub sees it. The hub holds this and nothing more
    /// specific, which is what lets the transport change underneath it without the hub learning
    /// anything new.
    /// </summary>
    public interface ICopilotChatSession : IAsyncDisposable
    {
        CopilotChatTransport Transport { get; }
        string SessionId { get; }
        string WorkingDirectory { get; }
        string ModelId { get; }
        bool IsAlive { get; }
        DateTime LastUsedUtc { get; }

        Task StartAsync(CancellationToken ct = default);

        /// <summary>Sends the prompt and yields the answer as it arrives.</summary>
        IAsyncEnumerable<CopilotChatChunk> PromptAsync(string text, CancellationToken ct = default);

        /// <summary>Aborts the turn in flight; a no-op when nothing is running.</summary>
        Task CancelAsync(CancellationToken ct = default);
    }
}
