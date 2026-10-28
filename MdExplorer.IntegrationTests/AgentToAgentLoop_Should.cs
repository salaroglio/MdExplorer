using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il giro completo della città, end-to-end, senza Copilot: un messaggio sveglia un agente
    /// LLM (fake runner al seam); l'agente trova il <c>MDE_RUN_TOKEN</c> nel proprio ambiente e
    /// lo usa per scrivere a un altro cittadino. Prova insieme: R2 (token coniato al risveglio,
    /// portato nell'ambiente, che certifica il mittente server-side) e l'accumulo degli hop
    /// nella stessa conversazione (anti-loop non aggirabile).
    /// </summary>
    [TestClass]
    public class AgentToAgentLoop_Should
    {
        [TestMethod]
        public async Task Let_a_woken_agent_message_a_colleague_with_its_run_token()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("loop");

            // waker: svegliabile da chiunque (external). recipient: accetta solo waker.
            ctx.WriteLlmCitizen(path, "waker", "Colui che inoltra", new[] { "*" });
            ctx.WriteLlmCitizen(path, "recipient", "Il destinatario", new[] { "waker" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "waker");
            ctx.Trust(path, "recipient");

            // Comportamento dell'agente svegliato: SOLO 'waker' inoltra, usando il token del
            // proprio ambiente per scrivere a 'recipient' (è ciò che farebbe Copilot via MCP).
            ctx.Runner.Behavior = async (request, _) =>
            {
                var self = request.Environment[LlmAgentWaker.EnvAgentName];
                if (self != "waker") return "(nessuna azione)";

                var token = request.Environment[LlmAgentWaker.EnvRunToken];
                await ctx.SendAuthenticated(token, "recipient", "inoltro il compito");
                return "inoltrato";
            };

            // Sveglia 'waker' dal gateway (mittente anonimo → external).
            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, "waker", "parti");
            Assert.IsFalse(rpc.IsError, $"errore inatteso: {rpc.ErrorCode} {rpc.ErrorMessage}");

            // Attendo che il messaggio waker→recipient compaia in mailbox.
            var msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "recipient"));
            var toRecipient = msgs.FirstOrDefault(x => x.ToAgent == "recipient");

            Assert.IsNotNull(toRecipient, "il messaggio waker→recipient deve esistere");
            Assert.AreEqual("waker", toRecipient.FromAgent, "mittente certificato dal token, non dichiarato");

            // La conversazione è UNA sola e gli hop si sono accumulati: external→waker (1),
            // waker→recipient (2). Prova che il fan-out passa dallo stesso thread anti-loop.
            var convs = ctx.Conversations();
            Assert.AreEqual(1, convs.Count, "un solo thread: il send eredita il contextId del risveglio");
            Assert.AreEqual(2, convs[0].HopCount, "gli hop si accumulano nel thread");
        }
    }
}
