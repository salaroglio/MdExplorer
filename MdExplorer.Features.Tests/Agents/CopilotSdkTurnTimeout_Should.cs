using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.CopilotChat;
using MdExplorer.Features.Services.AI.CopilotSdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// The answer must end when the timeouts fire. Seen on Windows 22/10/2026: Copilot ran a
    /// PowerShell command that never came back, and the chat stayed on "Esegue: …" for good —
    /// the timeouts only reached SendAsync, which had returned long before.
    /// </summary>
    [TestClass]
    public class CopilotSdkTurnTimeout_Should
    {
        /// <summary>Read as the session does, so the reason arrives as the reader sees it.</summary>
        private static async Task ReadToTheEnd(ChannelReader<CopilotChatChunk> reader)
        {
            using var guard = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await foreach (var _ in reader.ReadAllAsync(guard.Token)) { }
        }

        [TestMethod]
        public async Task EndTheAnswerWhenTheAgentFallsSilent()
        {
            var channel = Channel.CreateUnbounded<CopilotChatChunk>();
            using var idle = new CancellationTokenSource();
            using var hard = new CancellationTokenSource();
            using var watch = CopilotSdkSession.CloseOnTimeout(channel.Writer, idle, hard);

            idle.Cancel();

            var ex = await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                () => ReadToTheEnd(channel.Reader));
            StringAssert.Contains(ex.Message, "300");
        }

        [TestMethod]
        public async Task EndTheAnswerAtTheHardCap()
        {
            var channel = Channel.CreateUnbounded<CopilotChatChunk>();
            using var idle = new CancellationTokenSource();
            using var hard = new CancellationTokenSource();
            using var watch = CopilotSdkSession.CloseOnTimeout(channel.Writer, idle, hard);

            hard.Cancel();

            var ex = await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                () => ReadToTheEnd(channel.Reader));
            StringAssert.Contains(ex.Message, "30");
        }

        [TestMethod]
        public void LeaveAnAnswerThatEndedOnItsOwnAlone()
        {
            var channel = Channel.CreateUnbounded<CopilotChatChunk>();
            using var idle = new CancellationTokenSource();
            using var hard = new CancellationTokenSource();
            var watch = CopilotSdkSession.CloseOnTimeout(channel.Writer, idle, hard);

            watch.Dispose();
            idle.Cancel();

            Assert.IsFalse(channel.Reader.Completion.IsCompleted);
        }
    }
}
