using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il canale autenticato agente→agente (<c>/api/A2A/messages/send</c>, header
    /// <c>X-MDE-Run-Token</c>) sul Service reale. Prova la garanzia R2: il mittente è
    /// certificato dal token e non è spoofabile dal body.
    /// </summary>
    [TestClass]
    public class AuthenticatedChannel_Should
    {
        private const string Ping = "a2a-ping";

        [TestMethod]
        public async Task Reject_a_send_without_a_run_token()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("auth-notoken");
            ctx.Trust(path, Ping);

            var (status, _) = await ctx.SendAuthenticated(runToken: null, toAgent: Ping, message: "hi");

            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
            Assert.AreEqual(0, ctx.Messages().Count);
        }

        [TestMethod]
        public async Task Reject_a_send_with_a_bogus_run_token()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("auth-bogus");
            ctx.Trust(path, Ping);

            var (status, _) = await ctx.SendAuthenticated("non-esiste", Ping, "hi");

            Assert.AreEqual(HttpStatusCode.Unauthorized, status);
        }

        [TestMethod]
        public async Task Certify_the_sender_from_the_token_not_the_body()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("auth-certify");
            ctx.Trust(path, Ping);

            // Token coniato per 'agent-a': il body NON dichiara alcun mittente.
            var token = ctx.MintRunToken("agent-a", path, conversationId: null);
            var (status, _) = await ctx.SendAuthenticated(token, Ping, "ciao ping");

            Assert.AreEqual(HttpStatusCode.OK, status);
            var msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == Ping));
            var msg = msgs.First(x => x.ToAgent == Ping);
            Assert.AreEqual("agent-a", msg.FromAgent, "il mittente viene dal token, non dal body");
        }

        [TestMethod]
        public async Task Enforce_accepts_messages_from_of_the_recipient()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("auth-accepts");
            // Cittadino LLM che accetta SOLO 'qualcun-altro', non 'agent-a'.
            ctx.WriteLlmCitizen(path, "agent-b", "Guardiano", new[] { "qualcun-altro" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "agent-b");

            var token = ctx.MintRunToken("agent-a", path, conversationId: null);
            var (status, _) = await ctx.SendAuthenticated(token, "agent-b", "fammi entrare");

            Assert.AreEqual(HttpStatusCode.Forbidden, status, "agent-a non è tra i mittenti accettati da agent-b");
            Assert.AreEqual(0, ctx.Messages().Count(m => m.ToAgent == "agent-b"));
        }
    }
}
