using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Services.AI;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il modello dichiarato nella card arriva a <c>--model</c> del CLI, e se non è concesso il
    /// turno <b>fallisce dicendolo</b>.
    /// <para>
    /// Questo test parla con Copilot CLI <b>vero</b>: è l'unico modo di sapere davvero come si
    /// comporta un modello rifiutato. Costa nulla perché il CLI si ferma prima di interrogare
    /// qualsiasi modello. Diventa Inconclusive se Copilot non è installato o autenticato.
    /// </para>
    /// </summary>
    [TestClass]
    public class CopilotModelDeclaration_Should
    {
        private static CopilotTurnRunner Runner()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            var provider = new CopilotCliProvider(NullLogger<CopilotCliProvider>.Instance, services);
            return new CopilotTurnRunner(new IAiProvider[] { provider });
        }

        private static AgentTurnRequest Request(string provider, string model) => new()
        {
            ComposedPrompt = "rispondi solo: ok",
            WorkingDirectory = System.IO.Path.GetTempPath(),
            AgentName = "wiki-curator",
            ProjectPath = System.IO.Path.GetTempPath(),
            RequestedProvider = provider,
            RequestedModel = model,
            Environment = new Dictionary<string, string>(),
        };

        /// <summary>Un nome che non può diventare un modello vero (vedi sotto).</summary>
        private const string Missing = "modello-che-non-esiste-mde-test";

        [TestMethod]
        public async Task Fail_loud_when_the_declared_model_is_not_available()
        {
            AgentTurnResult result;
            try
            {
                // Un nome che non può diventare vero: «gpt-5.6-luna», usato fino al 07/11, nel frattempo è diventato un modello
                // di Copilot (CLI 1.0.107: lo usa e lo dichiara negli eventi), e il test falliva senza che MDE sbagliasse.
                result = await Runner().RunTurnAsync(Request("copilot", Missing));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Copilot CLI"))
            {
                Assert.Inconclusive("Copilot CLI non installato/autenticato in questo ambiente.");
                return;
            }

            Assert.AreEqual(AgentTurnOutcome.ProviderError, result.Outcome,
                "un modello non concesso non può risultare un turno riuscito");

            // Il messaggio deve dire QUALE agente, QUALE modello e COSA fare — e dichiarare che
            // non si ripiega su un altro modello.
            StringAssert.Contains(result.Diagnostic, Missing);
            StringAssert.Contains(result.Diagnostic, "wiki-curator");
            // Il messaggio segue la lingua dell'interfaccia (UiText): nei test di solito è l'inglese.
            Assert.IsTrue(result.Diagnostic.Contains("Nessun ripiego") || result.Diagnostic.Contains("No fallback"), result.Diagnostic);
        }

        /// <summary>
        /// R4 della valutazione del 07/11: il turno di un agente si legge dagli eventi del CLI (<c>--output-format json</c>),
        /// che arrivano mentre lavora, così il silenzio si misura. Il testo del turno è quello dei messaggi dell'assistente.
        /// Costa una richiesta.
        /// </summary>
        [TestMethod]
        public async Task Answer_a_turn_read_from_the_events_of_the_cli()
        {
            AgentTurnResult result;
            try { result = await Runner().RunTurnAsync(Request("copilot", null)); }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Copilot CLI"))
            {
                Assert.Inconclusive("Copilot CLI non installato/autenticato in questo ambiente.");
                return;
            }

            Assert.AreEqual(AgentTurnOutcome.Completed, result.Outcome, result.Diagnostic);
            StringAssert.Contains(result.Text.ToLowerInvariant(), "ok");
            Assert.IsFalse(result.Text.Contains("\"type\""), "il testo, non gli eventi: " + result.Text);
        }

        [TestMethod]
        public async Task Refuse_to_run_an_agent_that_asked_for_another_provider()
        {
            // Nessun processo lanciato: il rifiuto è a monte. Un agente che chiede 'openai' non
            // deve girare su Copilot facendo finta di niente.
            AgentTurnResult result;
            try
            {
                result = await Runner().RunTurnAsync(Request("openai", "gpt-qualcosa"));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Copilot CLI"))
            {
                Assert.Inconclusive("Copilot CLI non installato/autenticato in questo ambiente.");
                return;
            }

            Assert.AreEqual(AgentTurnOutcome.ProviderError, result.Outcome);
            StringAssert.Contains(result.Diagnostic, "openai");
        }
    }
}
