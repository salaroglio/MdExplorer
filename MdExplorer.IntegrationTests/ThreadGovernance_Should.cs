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
    /// Fase 4b — osservabilità e governo dei thread. L'umano vede le conversazioni con il
    /// budget hop consumato, termina un thread (kill switch, §9) e riapre uno esaurito
    /// (exhausted → active, hop azzerati). Verificato end-to-end contro il Service reale.
    /// </summary>
    [TestClass]
    public class ThreadGovernance_Should
    {
        /// <summary>Prepara un 'worker' che scala all'umano e ritorna il thread reale che ne nasce.</summary>
        private static async Task<(AgentCityContext ctx, string path, Guid convId)> EscalatedThread(string projectName)
        {
            var ctx = new AgentCityContext();
            var (projectKey, path) = ctx.SeedProject(projectName);

            ctx.WriteLlmCitizen(path, "worker", "Scala all'umano", new[] { "*" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "worker");

            ctx.Runner.Behavior = async (request, _) =>
            {
                var token = request.Environment[LlmAgentWaker.EnvRunToken];
                await ctx.SendAuthenticated(token, "user", "Serve una decisione.");
                return "scalato";
            };

            var rpc = await GatewayRpc.SendMessage(ctx.Client, projectKey, "worker", "parti");
            Assert.IsFalse(rpc.IsError, $"errore inatteso: {rpc.ErrorCode} {rpc.ErrorMessage}");

            var msgs = await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "user"));
            var convId = msgs.First(x => x.ToAgent == "user").ConversationId;
            return (ctx, path, convId);
        }

        [TestMethod]
        public async Task List_conversations_and_read_a_thread()
        {
            var (ctx, path, convId) = await EscalatedThread("threads");
            using var _ctx = ctx;

            var (status, json) = await ctx.GetConversations(path);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status);

            var conv = json.RootElement.GetProperty("conversations").EnumerateArray()
                .Single(c => c.GetProperty("id").GetGuid() == convId);
            Assert.AreEqual("active", conv.GetProperty("status").GetString());
            Assert.AreEqual(1, conv.GetProperty("hopCount").GetInt32(), "external→worker = 1 hop; worker→user esente");
            var participants = conv.GetProperty("participants").EnumerateArray().Select(p => p.GetString()).ToList();
            CollectionAssert.Contains(participants, "worker");
            CollectionAssert.Contains(participants, "user");

            var (mstatus, mjson) = await ctx.GetConversationMessages(convId);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, mstatus);
            Assert.IsTrue(mjson.RootElement.GetProperty("messages").GetArrayLength() >= 1);
        }

        [TestMethod]
        public async Task Kill_a_thread_and_block_further_replies()
        {
            var (ctx, _, convId) = await EscalatedThread("kill");
            using var _ctx = ctx;

            Assert.AreEqual(System.Net.HttpStatusCode.OK, await ctx.Kill(convId));

            // Il thread killed rifiuta ogni accodamento successivo (anche la risposta umana).
            var (rstatus, rbody) = await ctx.Reply(convId.ToString(), "ci sei ancora?");
            Assert.AreEqual(System.Net.HttpStatusCode.Conflict, rstatus, rbody);

            Assert.AreEqual("killed", ctx.Conversations().Single(c => c.Id == convId).Status);
        }

        [TestMethod]
        public async Task Reopen_an_exhausted_thread_zeroes_hops_and_accepts_again()
        {
            var (ctx, _, convId) = await EscalatedThread("reopen");
            using var _ctx = ctx;

            // Porta il thread a 'exhausted' (come se avesse bruciato il budget hop).
            ctx.SetConversationStatus(convId, AgentConversation.StatusEnum.Exhausted, hopCount: 8);

            var (status, body) = await ctx.Reopen(convId);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            var conv = ctx.Conversations().Single(c => c.Id == convId);
            Assert.AreEqual("active", conv.Status);
            Assert.AreEqual(0, conv.HopCount, "la riapertura azzera gli hop");

            // Riaperto → la risposta umana viene di nuovo accettata.
            var (rstatus, rbody) = await ctx.Reply(convId.ToString(), "riprendiamo.");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, rstatus, rbody);
        }

        [TestMethod]
        public async Task Fail_loud_when_reopening_a_non_exhausted_thread()
        {
            var (ctx, _, convId) = await EscalatedThread("reopen-active");
            using var _ctx = ctx;

            // È 'active', non 'exhausted': la riapertura è un no-op fail-loud.
            var (status, _) = await ctx.Reopen(convId);
            Assert.AreEqual(System.Net.HttpStatusCode.UnprocessableEntity, status);
        }
    }
}
