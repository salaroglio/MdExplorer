using System;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 4a — l'umano nella città. Un agente svegliato scala all'umano (scrive a <c>user</c>
    /// col proprio RunToken); il messaggio si posa nella inbox non-letto; l'umano risponde e la
    /// risposta risveglia l'agente <b>nella stessa conversazione</b>, senza consumare hop (§9).
    /// Prova end-to-end il canale bidirezionale città↔umano senza Copilot (fake runner al seam).
    /// </summary>
    [TestClass]
    public class HumanInTheCity_Should
    {
        /// <summary>Prepara un progetto con un cittadino LLM 'worker' che scala a 'user', lo sveglia e attende il messaggio to:user.</summary>
        private static async Task<(AgentCityContext ctx, string path, Guid convId, Guid msgId)> EscalateToUser(string projectName)
        {
            var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject(projectName);

            ctx.WriteLlmCitizen(path, "worker", "Lavoratore che scala all'umano", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");

            // Svegliato, l'agente usa il token del proprio ambiente per scrivere a 'user'.
            ctx.Runner.Behavior = async (request, _) =>
            {
                var token = request.Environment[LlmAgentWaker.EnvRunToken];
                await ctx.SendAuthenticated(token, "user", "Serve una tua decisione sul workflow.");
                return "scalato all'umano";
            };

            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker", "parti");
            Assert.IsFalse(rpc.IsError, $"errore inatteso al risveglio: {rpc.ErrorCode} {rpc.ErrorMessage}");

            var msgs = await ctx.WaitForMessages(m =>
                m.Any(x => x.ToAgent == "user" && x.State == AgentMessage.StateEnum.Processed));
            var toUser = msgs.FirstOrDefault(x => x.ToAgent == "user");
            Assert.IsNotNull(toUser, "il messaggio worker→user deve esistere ed essere consegnato");
            Assert.AreEqual("worker", toUser.FromAgent, "mittente certificato dal token");

            return (ctx, path, toUser.ConversationId, toUser.Id);
        }

        [TestMethod]
        public async Task Deliver_an_agent_escalation_into_the_human_inbox_unread()
        {
            var (ctx, path, _, msgId) = await EscalateToUser("escalate");
            using var _ctx = ctx;

            var (status, json) = await ctx.GetInbox(path);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);

            var root = json.RootElement;
            Assert.IsTrue(root.GetProperty("unread").GetInt32() >= 1, "almeno un non-letto nel badge");

            var items = root.GetProperty("messages").EnumerateArray().ToList();
            var mine = items.FirstOrDefault(e => e.GetProperty("id").GetGuid() == msgId);
            Assert.AreNotEqual(default, mine.ValueKind, "il messaggio scalato deve comparire nella inbox");
            Assert.AreEqual("worker", mine.GetProperty("fromAgent").GetString());
            Assert.IsFalse(mine.GetProperty("read").GetBoolean(), "appena arrivato = non letto");
        }

        [TestMethod]
        public async Task Let_the_human_reply_and_wake_the_agent_in_the_same_conversation()
        {
            var (ctx, path, convId, _) = await EscalateToUser("reply");
            using var _ctx = ctx;

            // Prima della risposta: gateway external→worker = 1 hop; worker→user è esente.
            var before = ctx.Conversations().Single();
            Assert.AreEqual(1, before.HopCount, "worker→user non consuma hop (esente)");

            var (status, body) = await ctx.Reply(convId.ToString(), "Approvato, procedi.");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            // Nasce un messaggio user→worker nello stesso thread.
            var msgs = await ctx.WaitForMessages(m =>
                m.Any(x => x.FromAgent == "user" && x.ToAgent == "worker"));
            var reply = msgs.First(x => x.FromAgent == "user" && x.ToAgent == "worker");
            Assert.AreEqual(convId, reply.ConversationId, "la risposta resta nella stessa conversazione");

            // La risposta dell'umano è esente: l'hopCount NON è cambiato.
            var after = ctx.Conversations().Single();
            Assert.AreEqual(1, after.HopCount, "user→worker non consuma hop (esente)");

            // Rispondere ha marcato letti i messaggi to:user del thread (fuori dal badge).
            var (_, json) = await ctx.GetInbox(path);
            Assert.AreEqual(0, json.RootElement.GetProperty("unread").GetInt32(), "rispondendo si azzera il badge del thread");
        }

        [TestMethod]
        public async Task Mark_a_message_read_and_drop_it_from_the_unread_badge()
        {
            var (ctx, path, _, msgId) = await EscalateToUser("read");
            using var _ctx = ctx;

            var status = await ctx.MarkRead(msgId);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);

            var (_, json) = await ctx.GetInbox(path);           // default: solo non-letti
            Assert.AreEqual(0, json.RootElement.GetProperty("unread").GetInt32());
            Assert.AreEqual(0, json.RootElement.GetProperty("messages").GetArrayLength(),
                "il letto non compare più nella inbox non-letti");

            var (_, all) = await ctx.GetInbox(path, includeRead: true);
            Assert.IsTrue(all.RootElement.GetProperty("messages").EnumerateArray()
                .Any(e => e.GetProperty("id").GetGuid() == msgId), "ma resta visibile con includeRead");
        }

        [TestMethod]
        public async Task Fail_loud_when_replying_to_an_unknown_conversation()
        {
            using var ctx = new AgentCityContext();
            ctx.SeedProject("noconv");

            var (status, body) = await ctx.Reply(Guid.NewGuid().ToString(), "ciao?");
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound, status, body);
        }
    }
}
