using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// I posti di lavoro del pool: quanti agenti possono girare insieme su questa macchina.
    /// <para>
    /// Non è una regola del repo ma una capacità della macchina — dipende da disco e CPU — e per
    /// questo vive in UserDB accanto alla preferenza sull'isolamento, non nel
    /// <c>.development.yml</c>.
    /// </para>
    /// </summary>
    [TestClass]
    public class AgentWorktreeSlots_Should
    {
        private static IAgentWorktreePreference Pref(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IAgentWorktreePreference>();

        [TestMethod]
        public void Default_to_two_when_nobody_decided()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("slot-default");

            Assert.AreEqual(AgentWorktreePreference.DefaultSlots, Pref(ctx).SlotsFor(path));
            Assert.AreEqual(2, AgentWorktreePreference.DefaultSlots,
                "due: abbastanza per farne collaborare due, poco abbastanza per lo spazio nel progetto");
        }

        [TestMethod]
        public void Remember_an_explicit_choice()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("slot-scelta");

            Pref(ctx).SetSlots(path, 4);
            Assert.AreEqual(4, Pref(ctx).SlotsFor(path));

            // null = torna al default, non "zero posti".
            Pref(ctx).SetSlots(path, null);
            Assert.AreEqual(AgentWorktreePreference.DefaultSlots, Pref(ctx).SlotsFor(path));
        }

        [TestMethod]
        public void Refuse_a_number_that_makes_no_sense()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("slot-assurdi");

            // Zero posti significherebbe una città che non può lavorare; troppi significherebbero
            // altrettanti modelli in esecuzione insieme.
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Pref(ctx).SetSlots(path, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => Pref(ctx).SetSlots(path, AgentWorktreePreference.MaxSlots + 1));
        }

        [TestMethod]
        public async Task Be_reachable_from_the_settings_endpoints()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("slot-endpoint");

            var res = await ctx.Client.PostAsync("/api/ProjectSettings/SetAgentWorktreesSetting",
                new StringContent($"{{\"projectPath\":{System.Text.Json.JsonSerializer.Serialize(path)},\"enabled\":true,\"slots\":3}}",
                    Encoding.UTF8, "application/json"));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode, await res.Content.ReadAsStringAsync());

            var get = await ctx.Client.GetAsync(
                "/api/ProjectSettings/GetAgentWorktreesSetting?projectPath=" + Uri.EscapeDataString(path));
            var body = await get.Content.ReadAsStringAsync();

            StringAssert.Contains(body, "\"slots\":3");
            StringAssert.Contains(body, "\"defaultSlots\":2", "la UI deve poter dire qual è il default");
        }

        [TestMethod]
        public async Task Answer_with_a_reason_instead_of_a_server_error()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("slot-errore");

            var res = await ctx.Client.PostAsync("/api/ProjectSettings/SetAgentWorktreesSetting",
                new StringContent($"{{\"projectPath\":{System.Text.Json.JsonSerializer.Serialize(path)},\"enabled\":true,\"slots\":99}}",
                    Encoding.UTF8, "application/json"));

            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, res.StatusCode,
                "un numero fuori scala è una richiesta sbagliata, non un errore del server");
            StringAssert.Contains(await res.Content.ReadAsStringAsync(), "posti");
        }
    }
}
