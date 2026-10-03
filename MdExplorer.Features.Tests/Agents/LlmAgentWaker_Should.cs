using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Agents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Agents
{
    /// <summary>
    /// Verifica deterministica del risveglio LLM (§7, R1+R2) SENZA spawn di processo: una
    /// fake <see cref="IAgentTurnRunner"/> sostituisce Copilot al seam. È la controparte del
    /// "fake IAiProvider" della spec — realizzata un gradino sopra il provider, così il test
    /// vive in Features.Tests senza referenziare l'EXE del Service.
    /// </summary>
    [TestClass]
    public class LlmAgentWaker_Should
    {
        private const string Body = "# stem-curator\n\nSei un curatore di stem.";

        /// <summary>Fake del seam: cattura la richiesta e, DURANTE il turno, registra se il token
        /// presente nell'ambiente è valido nello store condiviso (deve esserlo).</summary>
        private sealed class CapturingRunner : IAgentTurnRunner
        {
            private readonly IRunTokenStore _store;
            public CapturingRunner(IRunTokenStore store) { _store = store; }

            public AgentTurnRequest LastRequest { get; private set; }
            public bool TokenWasValidDuringRun { get; private set; }
            public string ReturnValue { get; set; } = "fatto";
            public Exception ToThrow { get; set; }

            /// <summary>Esito non riuscito da restituire SENZA sollevare.</summary>
            public AgentTurnOutcome? FailWith { get; set; }
            public int? Iterations { get; set; }

            public Task<AgentTurnResult> RunTurnAsync(AgentTurnRequest request, CancellationToken ct = default)
            {
                LastRequest = request;
                var token = request.Environment != null && request.Environment.TryGetValue(LlmAgentWaker.EnvRunToken, out var t) ? t : null;
                TokenWasValidDuringRun = token != null && _store.Validate(token) != null;
                if (ToThrow != null) throw ToThrow;
                return Task.FromResult(FailWith.HasValue
                    ? AgentTurnResult.Failed(FailWith.Value, "budget esaurito", ReturnValue, Iterations)
                    : AgentTurnResult.Completed(ReturnValue, Iterations));
            }
        }

        private static LlmWakeRequest Request() => new LlmWakeRequest
        {
            RunId = Guid.NewGuid(),
            AgentName = "stem-curator",
            AgentFileContent = Body,
            ProjectPath = "/prj",
            ConversationId = Guid.NewGuid().ToString(),
            FromAgent = "cobol-pipeline",
            MessageBody = "12 stem nuovi da classificare",
        };

        [TestMethod]
        public async Task Compose_the_wake_prompt_with_the_message_as_data()
        {
            var store = new RunTokenStore();
            var runner = new CapturingRunner(store);
            var waker = new LlmAgentWaker(store, runner);

            var outcome = await waker.WakeAsync(Request());

            Assert.IsTrue(outcome.Success);
            Assert.AreEqual("fatto", outcome.Output);
            var prompt = runner.LastRequest.ComposedPrompt;
            StringAssert.Contains(prompt, "Sei un curatore di stem.");
            StringAssert.Contains(prompt, "# Messaggio ricevuto");
            StringAssert.Contains(prompt, "cobol-pipeline");
            StringAssert.Contains(prompt, "12 stem nuovi da classificare");
            StringAssert.Contains(prompt, "DATO");
        }

        [TestMethod]
        public async Task Carry_the_run_token_in_the_environment_never_in_the_prompt()
        {
            var store = new RunTokenStore();
            var runner = new CapturingRunner(store);
            var waker = new LlmAgentWaker(store, runner);

            await waker.WakeAsync(Request());

            var env = runner.LastRequest.Environment;
            Assert.IsNotNull(env);
            Assert.IsTrue(env.ContainsKey(LlmAgentWaker.EnvRunToken));
            var token = env[LlmAgentWaker.EnvRunToken];
            Assert.IsFalse(string.IsNullOrWhiteSpace(token), "il token deve esserci");
            // R2: il token viaggia SOLO nell'ambiente, mai nel prompt.
            Assert.IsFalse(runner.LastRequest.ComposedPrompt.Contains(token), "il token non deve finire nel prompt");
        }

        [TestMethod]
        public async Task Bind_the_environment_to_the_woken_identity_and_context()
        {
            var store = new RunTokenStore();
            var runner = new CapturingRunner(store);
            var waker = new LlmAgentWaker(store, runner);
            var req = Request();

            await waker.WakeAsync(req);

            var env = runner.LastRequest.Environment;
            Assert.AreEqual("stem-curator", env[LlmAgentWaker.EnvAgentName]);
            Assert.AreEqual("/prj", env[LlmAgentWaker.EnvProjectPath]);
            Assert.AreEqual(req.ConversationId, env[LlmAgentWaker.EnvConversationId]);
            Assert.AreEqual("cobol-pipeline", env[LlmAgentWaker.EnvFromAgent]);
            Assert.AreEqual("/prj", runner.LastRequest.WorkingDirectory);
        }

        [TestMethod]
        public async Task Sign_the_run_environment_with_the_agent_git_identity()
        {
            var store = new RunTokenStore();
            var runner = new CapturingRunner(store);
            var waker = new LlmAgentWaker(store, runner);

            await waker.WakeAsync(Request());

            var env = runner.LastRequest.Environment;
            // Firma git per-agente: i commit dell'agente sono attribuiti a lui, non all'umano.
            Assert.AreEqual("stem-curator", env[AgentGitIdentity.EnvAuthorName]);
            Assert.AreEqual("stem-curator@agents.mde", env[AgentGitIdentity.EnvAuthorEmail]);
            Assert.AreEqual("stem-curator", env[AgentGitIdentity.EnvCommitterName]);
            Assert.AreEqual("stem-curator@agents.mde", env[AgentGitIdentity.EnvCommitterEmail]);
        }

        [TestMethod]
        public async Task Keep_the_token_valid_during_the_run_then_revoke_it_after()
        {
            var store = new RunTokenStore();
            var runner = new CapturingRunner(store);
            var waker = new LlmAgentWaker(store, runner);

            await waker.WakeAsync(Request());

            // Valido MENTRE il runner girava...
            Assert.IsTrue(runner.TokenWasValidDuringRun, "il token deve essere valido durante il run");
            // ...e revocato DOPO (non più risolvibile).
            var token = runner.LastRequest.Environment[LlmAgentWaker.EnvRunToken];
            Assert.IsNull(store.Validate(token), "il token deve essere revocato a fine run");
        }

        [TestMethod]
        public async Task Revoke_the_token_even_when_the_run_throws()
        {
            var store = new RunTokenStore();
            var runner = new CapturingRunner(store) { ToThrow = new InvalidOperationException("boom") };
            var waker = new LlmAgentWaker(store, runner);

            var outcome = await waker.WakeAsync(Request());

            Assert.IsFalse(outcome.Success);
            StringAssert.Contains(outcome.Error, "boom");
            var token = runner.LastRequest.Environment[LlmAgentWaker.EnvRunToken];
            Assert.IsNull(store.Validate(token), "anche in errore il token va revocato");
        }

        /// <summary>
        /// Il caso per cui esiste <see cref="AgentTurnResult"/>: il runner non solleva, torna un
        /// testo, ma il turno NON è concluso. Col contratto a stringa questo passava per successo
        /// e faceva partire deliverable, auto-merge e verdetto federato positivo.
        /// </summary>
        [TestMethod]
        public async Task Treat_an_unfinished_turn_as_a_failure_even_without_an_exception()
        {
            var store = new RunTokenStore();
            var runner = new CapturingRunner(store)
            {
                FailWith = AgentTurnOutcome.Exhausted,
                ReturnValue = "Tool execution loop exceeded maximum iterations.",
                Iterations = 10,
            };
            var waker = new LlmAgentWaker(store, runner);

            var outcome = await waker.WakeAsync(Request());

            Assert.IsFalse(outcome.Success, "un turno lasciato a metà non è un successo");
            Assert.AreEqual(AgentTurnOutcome.Exhausted, outcome.Outcome);
            Assert.AreEqual(10, outcome.Iterations, "le iterazioni consumate restano osservabili");
            StringAssert.Contains(outcome.Error, "budget esaurito");
        }

        [TestMethod]
        public async Task Report_a_completed_turn_with_its_iterations()
        {
            var store = new RunTokenStore();
            var runner = new CapturingRunner(store) { ReturnValue = "fatto", Iterations = 3 };
            var waker = new LlmAgentWaker(store, runner);

            var outcome = await waker.WakeAsync(Request());

            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(AgentTurnOutcome.Completed, outcome.Outcome);
            Assert.AreEqual("fatto", outcome.Output);
            Assert.AreEqual(3, outcome.Iterations);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Refuse_to_wake_a_bodyless_agent()
        {
            var waker = new LlmAgentWaker(new RunTokenStore(), new CapturingRunner(new RunTokenStore()));
            var req = Request();
            req.AgentFileContent = "   ";
            await waker.WakeAsync(req);
        }
    }
}
