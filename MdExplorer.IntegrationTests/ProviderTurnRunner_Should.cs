using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase C — il turno su un provider generico. Il ciclo di tool calling non è nostro
    /// (<c>FunctionInvokingChatClient</c>): quello che verifichiamo è ciò che <b>è</b> nostro —
    /// quali tool arrivano al modello, e come si traduce la fine del turno in un esito.
    /// <para>
    /// Il modello è finto di proposito: qui non si misura la bravura di un LLM, si misura che
    /// un turno fermato dal tetto non venga scambiato per un turno concluso.
    /// </para>
    /// </summary>
    [TestClass]
    public class ProviderTurnRunner_Should
    {
        /// <summary>Modello finto: decide lui cosa rispondere, per mettere il ciclo dove serve.</summary>
        private sealed class FakeChatClient : IChatClient
        {
            private readonly Func<IEnumerable<ChatMessage>, ChatOptions, ChatResponse> _reply;
            public int Calls;
            public ChatOptions LastOptions;

            public FakeChatClient(Func<IEnumerable<ChatMessage>, ChatOptions, ChatResponse> reply) => _reply = reply;

            public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages,
                ChatOptions options = null, CancellationToken cancellationToken = default)
            {
                Interlocked.Increment(ref Calls);
                LastOptions = options;
                return Task.FromResult(_reply(messages, options));
            }

            public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
                ChatOptions options = null, CancellationToken cancellationToken = default)
                => throw new NotSupportedException("il runner degli agenti non usa lo streaming");

            public object GetService(Type serviceType, object serviceKey = null) => null;
            public void Dispose() { }
        }

        private sealed class FixedFactory : IAgentChatClientFactory
        {
            private readonly IChatClient _client;
            public FixedFactory(IChatClient client) => _client = client;
            public IChatClient Create(string projectPath) => _client;
        }

        private static AgentTurnRequest Request() => new()
        {
            ComposedPrompt = "fai il tuo lavoro",
            WorkingDirectory = "/tmp",
            Environment = new Dictionary<string, string>
            {
                ["MDE_PROJECT_PATH"] = "/tmp/progetto",
                ["MDE_AGENT_NAME"] = "test-agent",
                [ProviderTurnRunner.EnvDeclaredTools] = "read,write",
                [ProviderTurnRunner.EnvTrusted] = "1",
            },
        };

        private static ProviderTurnRunner Runner(IChatClient chat)
            => new(new FixedFactory(chat),
                   new AgentMcpToolProvider(NullLogger<AgentMcpToolProvider>.Instance),
                   NullLogger<ProviderTurnRunner>.Instance);

        [TestMethod]
        public async Task Complete_a_turn_that_answers_without_tools()
        {
            var chat = new FakeChatClient((_, __) =>
                new ChatResponse(new ChatMessage(ChatRole.Assistant, "fatto")));

            AgentTurnResult result;
            try { result = await Runner(chat).RunTurnAsync(Request()); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Server MCP"))
            { Assert.Inconclusive("Eseguibile MCP non presente."); return; }

            Assert.AreEqual(AgentTurnOutcome.Completed, result.Outcome);
            Assert.AreEqual("fatto", result.Text);
        }

        [TestMethod]
        public async Task Hand_the_model_only_the_authorized_tools()
        {
            var chat = new FakeChatClient((_, __) =>
                new ChatResponse(new ChatMessage(ChatRole.Assistant, "ok")));

            var request = Request();
            // Non fidato: la lettura resta, scrittura e azioni verso l'esterno no.
            request.Environment = new Dictionary<string, string>(request.Environment)
            {
                [ProviderTurnRunner.EnvTrusted] = "0",
            };

            try { await Runner(chat).RunTurnAsync(request); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Server MCP"))
            { Assert.Inconclusive("Eseguibile MCP non presente."); return; }

            var names = chat.LastOptions.Tools.Select(t => t.Name).ToList();
            CollectionAssert.Contains(names, "list_agents");
            CollectionAssert.DoesNotContain(names, "send_agent_message");
            CollectionAssert.DoesNotContain(names, "assert_learned_fact");
        }

        [TestMethod]
        public async Task Call_a_turn_stopped_by_the_iteration_cap_exhausted_not_completed()
        {
            // Il modello chiede all'infinito lo stesso tool: il ciclo lo fermerà al tetto.
            var chat = new FakeChatClient((_, __) =>
            {
                var call = new FunctionCallContent(
                    callId: Guid.NewGuid().ToString(), name: "list_agents",
                    arguments: new Dictionary<string, object>());
                return new ChatResponse(new ChatMessage(ChatRole.Assistant, new List<AIContent> { call }));
            });

            AgentTurnResult result;
            try { result = await Runner(chat).RunTurnAsync(Request()); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Server MCP"))
            { Assert.Inconclusive("Eseguibile MCP non presente."); return; }

            Assert.AreEqual(AgentTurnOutcome.Exhausted, result.Outcome,
                "un turno fermato dal tetto NON è un successo: a valle farebbe pubblicare un " +
                "deliverable a metà e insegnerebbe alla memoria che il routing ha funzionato");
            Assert.IsTrue(chat.Calls > 1, "il ciclo ha davvero iterato");
            StringAssert.Contains(result.Diagnostic, "tetto");
        }
    }
}
