using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il canale gateway (A2A JSON-RPC, non autenticato) end-to-end sul Service reale:
    /// gateway → mailbox → dispatcher → consegna algoritmica. Il cittadino è <c>a2a-ping</c>
    /// (algoritmico, accetta <c>*</c>), così non serve né Copilot né un file agente.
    /// </summary>
    [TestClass]
    public class GatewayChannel_Should
    {
        private const string Ping = "a2a-ping";

        [TestMethod]
        public async Task Deliver_an_anonymous_message_to_a_trusted_algorithmic_citizen()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("proj-deliver");
            ctx.Trust(path, Ping);

            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, Ping, "ping!");
            Assert.IsFalse(rpc.IsError, $"atteso submit, ricevuto errore {rpc.ErrorCode}: {rpc.ErrorMessage}");

            var msgs = await ctx.WaitForMessages(m => m.Any(x =>
                x.ToAgent == Ping && x.State == AgentMessage.StateEnum.Processed));
            var delivered = msgs.FirstOrDefault(x => x.ToAgent == Ping);
            Assert.IsNotNull(delivered, "il messaggio dovrebbe esistere in mailbox");
            Assert.AreEqual(AgentMessage.StateEnum.Processed, delivered.State, "il ping dovrebbe essere consegnato");
            Assert.AreEqual("external", delivered.FromAgent, "mittente anonimo → external");
        }

        [TestMethod]
        public async Task Refuse_a_message_to_an_untrusted_citizen()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, _) = ctx.SeedProject("proj-untrusted");
            // NIENTE trust.

            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, Ping, "ping!");

            Assert.IsTrue(rpc.IsError);
            Assert.AreEqual(-32002, rpc.ErrorCode, "destinatario non trusted");
            Assert.AreEqual(0, ctx.Messages().Count, "niente deve essere accodato");
        }

        [TestMethod]
        public async Task Reject_a_sender_spoofing_the_human_user()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("proj-userspoof");
            ctx.Trust(path, Ping);

            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, Ping, "x", fromAgent: "user");

            Assert.IsTrue(rpc.IsError);
            Assert.AreEqual(-32602, rpc.ErrorCode, "'user' non è spendibile come mittente dichiarato");
            Assert.AreEqual(0, ctx.Messages().Count);
        }

        [TestMethod]
        public async Task Reject_a_sender_spoofing_a_citizen_name()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("proj-citizenspoof");
            ctx.Trust(path, Ping);

            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, Ping, "x", fromAgent: Ping);

            Assert.IsTrue(rpc.IsError);
            Assert.AreEqual(-32005, rpc.ErrorCode, "un nome di cittadino non passa dal gateway");
        }

        [TestMethod]
        public async Task Reject_an_oversized_body()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("proj-bigbody");
            ctx.Trust(path, Ping);

            var huge = new string('x', 40 * 1024); // oltre il cap di 32 KB
            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, Ping, huge);

            Assert.IsTrue(rpc.IsError, "un corpo oltre il cap dev'essere rifiutato");
            Assert.AreEqual(0, ctx.Messages().Count, "niente deve essere accodato");
        }

        [TestMethod]
        public async Task Persist_declared_topics_through_to_the_message()
        {
            using var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject("proj-topics");
            ctx.Trust(path, Ping);

            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, Ping, "con argomenti",
                topics: new[] { "deploy", "urgent" });
            Assert.IsFalse(rpc.IsError);

            var msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == Ping));
            var msg = msgs.First(x => x.ToAgent == Ping);
            Assert.AreEqual("deploy\nurgent", msg.Topics, "i topics dichiarati devono essere persistiti");
        }
    }
}
