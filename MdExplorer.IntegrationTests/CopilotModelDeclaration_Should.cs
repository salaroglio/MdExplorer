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

        [TestMethod]
        public async Task Fail_loud_when_the_declared_model_is_not_available()
        {
            AgentTurnResult result;
            try
            {
                result = await Runner().RunTurnAsync(Request("copilot", "gpt-5.6-luna"));
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
            StringAssert.Contains(result.Diagnostic, "gpt-5.6-luna");
            StringAssert.Contains(result.Diagnostic, "wiki-curator");
            StringAssert.Contains(result.Diagnostic, "Nessun ripiego");
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
