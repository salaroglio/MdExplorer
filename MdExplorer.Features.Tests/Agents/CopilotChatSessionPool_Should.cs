using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.CopilotChat;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// When does the chat keep its conversation, and when does it start over? Picking another
    /// model in the chat header used to throw the conversation away, always: the pool treated any
    /// difference as "new session". These pin down which differences deserve that.
    /// </summary>
    [TestClass]
    public class CopilotChatSessionPool_Should
    {
        private const string Conn = "conn-1";
        private const string Project = "/progetti/alfa";

        private sealed class Switch : ICopilotChatTransportSource
        {
            public CopilotChatTransport Value = CopilotChatTransport.Sdk;
            public CopilotChatTransport Current() => Value;
        }

        private sealed class FakeSession : ICopilotChatSession
        {
            public FakeSession(CopilotChatTransport transport, string workingDirectory, string modelId, bool canSwitchLive)
            {
                Transport = transport; WorkingDirectory = workingDirectory; ModelId = modelId; CanSwitchModelLive = canSwitchLive;
            }

            public CopilotChatTransport Transport { get; }
            public string SessionId => "fake";
            public string WorkingDirectory { get; }
            public string ModelId { get; private set; }
            public bool IsAlive { get; private set; } = true;
            public DateTime LastUsedUtc => DateTime.UtcNow;
            public string AnsweredModel => null;
            public Task<CopilotUsageSnapshot> GetUsageAsync(CancellationToken ct = default) => Task.FromResult<CopilotUsageSnapshot>(null);
            public bool CanSwitchModelLive { get; }
            public List<string> ModelSwitches { get; } = new List<string>();

            public Task StartAsync(CancellationToken ct = default) => Task.CompletedTask;

            public Task SetModelAsync(string modelId, CancellationToken ct = default)
            {
                ModelSwitches.Add(modelId);
                ModelId = modelId;
                return Task.CompletedTask;
            }

            public async IAsyncEnumerable<CopilotChatChunk> PromptAsync(string text, [EnumeratorCancellation] CancellationToken ct = default)
            {
                await Task.CompletedTask;
                yield break;
            }

            public Task CancelAsync(CancellationToken ct = default) => Task.CompletedTask;

            public ValueTask DisposeAsync() { IsAlive = false; return ValueTask.CompletedTask; }
        }

        private Switch _switch;
        private List<FakeSession> _created;
        private CopilotChatSessionPool _pool;

        private void BuildPool(bool canSwitchLive)
        {
            _switch = new Switch();
            _created = new List<FakeSession>();
            _pool = new CopilotChatSessionPool(
                NullLoggerFactory.Instance,
                NullLogger<CopilotChatSessionPool>.Instance,
                _switch,
                (transport, wd, model) =>
                {
                    var s = new FakeSession(transport, wd, model, canSwitchLive);
                    _created.Add(s);
                    return s;
                });
        }

        [TestCleanup]
        public async Task DisposePool()
        {
            if (_pool != null) await _pool.DisposeAsync();
        }

        [TestMethod]
        public async Task ReuseTheSessionWhenNothingChanged()
        {
            BuildPool(canSwitchLive: true);

            var first = await _pool.GetOrCreateAsync(Conn, Project, "modello-a");
            var second = await _pool.GetOrCreateAsync(Conn, Project, "modello-a");

            Assert.AreSame(first, second);
            Assert.AreEqual(1, _created.Count);
        }

        [TestMethod]
        public async Task KeepTheConversationWhenOnlyTheModelChanges()
        {
            BuildPool(canSwitchLive: true);

            var first = (FakeSession)await _pool.GetOrCreateAsync(Conn, Project, "modello-a");
            var second = await _pool.GetOrCreateAsync(Conn, Project, "modello-b");

            Assert.AreSame(first, second, "same session = same conversation");
            Assert.AreEqual(1, _created.Count, "no new session may be started");
            CollectionAssert.AreEqual(new[] { "modello-b" }, first.ModelSwitches, "the model must be changed in place");
            Assert.IsTrue(first.IsAlive);
        }

        [TestMethod]
        public async Task StartOverWhenTheTransportCannotChangeModelLive()
        {
            // ACP: the model only applies when a session starts.
            BuildPool(canSwitchLive: false);

            var first = (FakeSession)await _pool.GetOrCreateAsync(Conn, Project, "modello-a");
            var second = await _pool.GetOrCreateAsync(Conn, Project, "modello-b");

            Assert.AreNotSame(first, second);
            Assert.AreEqual("modello-b", second.ModelId);
            Assert.IsFalse(first.IsAlive, "the old session must be closed, not left running");
            Assert.AreEqual(0, first.ModelSwitches.Count, "SetModelAsync must never be called on a transport that cannot do it");
        }

        [TestMethod]
        public async Task StartOverWhenTheTransportIsSwitched()
        {
            BuildPool(canSwitchLive: true);

            var first = (FakeSession)await _pool.GetOrCreateAsync(Conn, Project, "modello-a");
            _switch.Value = CopilotChatTransport.Acp;
            var second = await _pool.GetOrCreateAsync(Conn, Project, "modello-a");

            Assert.AreNotSame(first, second);
            Assert.AreEqual(CopilotChatTransport.Acp, second.Transport);
            Assert.IsFalse(first.IsAlive);
        }

        [TestMethod]
        public async Task StartOverWhenAskedToLetTheCliChoose()
        {
            // "Let the CLI choose" has no model id to hand to SetModelAsync.
            BuildPool(canSwitchLive: true);

            var first = (FakeSession)await _pool.GetOrCreateAsync(Conn, Project, "modello-a");
            var second = await _pool.GetOrCreateAsync(Conn, Project, null);

            Assert.AreNotSame(first, second);
            Assert.IsNull(second.ModelId);
            Assert.AreEqual(0, first.ModelSwitches.Count);
        }

        [TestMethod]
        public async Task StartOverWhenTheProjectChanges()
        {
            BuildPool(canSwitchLive: true);

            var first = (FakeSession)await _pool.GetOrCreateAsync(Conn, Project, "modello-a");
            var second = await _pool.GetOrCreateAsync(Conn, "/progetti/beta", "modello-b");

            Assert.AreNotSame(first, second, "a conversation belongs to its project");
            Assert.AreEqual(0, first.ModelSwitches.Count);
        }
    }
}
