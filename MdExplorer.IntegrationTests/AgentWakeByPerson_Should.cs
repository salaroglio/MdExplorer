using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Ogni risveglio di un agente passa da una persona, anche senza workflow (P6, «La posta in ordine»): un agente della città
    /// che scrive a un altro non lo sveglia da solo; il messaggio arriva «da avviare» al responsabile del destinatario.
    /// </summary>
    [TestClass]
    public class AgentWakeByPerson_Should
    {
        [TestMethod]
        public async Task Hold_a_message_from_a_city_agent_until_the_owner_of_the_recipient_starts_it()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("sveglia-persona");
            ctx.SetGitEmail(path, "io@test.local");
            ctx.Factory.Services.GetRequiredService<IProjectMetadataService>()
                .SetAgentCity(path, new MdExplorer.Service.Models.AgentCityConfig { Enabled = true, UseAgentWorktrees = false });
            ctx.WriteLlmCitizen(path, "capo", "Chi incarica", new[] { "user" });
            ctx.WriteLlmCitizen(path, "tecnico", "Chi scrive la scheda", new[] { "capo", "user" });
            ctx.IndexAgentFiles(path);
            ctx.Trust(path, "capo");
            ctx.Trust(path, "tecnico");
            ctx.OwnAgents(path, "capo", "tecnico");
            ctx.Runner.Behavior = (_, __) => Task.FromResult("ok");

            var token = ctx.MintRunToken("capo", path, null);
            var (status, body) = await ctx.SendAuthenticated(token, "tecnico", "[INCARICO] scrivi la scheda tecnica");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);

            var held = (await ctx.WaitForMessages(m => m.Any(x => x.ToAgent == "tecnico" && x.DeferredReason == AgentMessage.DeferredReasonEnum.AwaitingOwner)))
                .Single(x => x.ToAgent == "tecnico");
            await Task.Delay(2500);
            Assert.AreEqual(0, ctx.Runner.Calls, "senza workflow il capo non sveglia il tecnico da solo");

            var (_, inbox) = await ctx.GetJson("/api/A2A/mailbox/inbox?projectPath=" + System.Uri.EscapeDataString(path));
            var toStart = inbox.RootElement.GetProperty("toStart")[0];
            Assert.AreEqual("capo", toStart.GetProperty("fromAgent").GetString(), "nella posta: lo chiede il capo");

            var (started, startBody) = await ctx.PostJson($"/api/A2A/mailbox/to-start/{held.Id}/start", "{}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, started, startBody);
            await ctx.WaitForMessages(m => m.Any(x => x.Id == held.Id && x.State == AgentMessage.StateEnum.Processed));
            Assert.AreEqual(1, ctx.Runner.Calls, "avviato dal responsabile, il tecnico lavora");
        }
    }
}
