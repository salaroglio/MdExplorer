using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// I flag opt-in della città (worktree, auto-merge) sono booleani: non distinguono "non
    /// inviato" da "false". La UI manda solo <c>enabled</c> e <c>ownershipDoc</c>, quindi senza
    /// preservazione il primo salvataggio dalle impostazioni li <b>spegnerebbe in silenzio</b> —
    /// e l'isolamento worktree sparirebbe senza che nessuno se ne accorga, fino al momento in cui
    /// un agente scrive nella working copy dell'umano.
    /// <para>Stessa forma del difetto già chiuso su <c>RelayUrl</c> e <c>RoomSecret</c>.</para>
    /// </summary>
    [TestClass]
    public class AgentCityFlagsPreserved_Should
    {
        [TestMethod]
        public async Task Survive_a_settings_save_that_does_not_carry_them()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("flag-opt-in");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();

            meta.SetAgentCity(path, new AgentCityConfig
            {
                Enabled = true,
                OwnershipDoc = "ownership.md",
                UseAgentWorktrees = true,
                AutoMergeAgentDeliverables = true,
            });

            // Salvataggio "come lo fa la UI": solo enabled + ownershipDoc.
            var query = "?path=" + System.Uri.EscapeDataString(path);
            var res = await ctx.Client.PostAsync("/api/MdProjects/SetAgentCity" + query,
                new StringContent("{\"enabled\":true,\"ownershipDoc\":\"ownership.md\"}",
                    Encoding.UTF8, "application/json"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, res.StatusCode, await res.Content.ReadAsStringAsync());

            var after = meta.GetAgentCity(path);
            Assert.IsTrue(after.UseAgentWorktrees,
                "l'isolamento worktree non deve spegnersi perché la UI non lo invia");
            Assert.IsTrue(after.AutoMergeAgentDeliverables,
                "nemmeno l'auto-merge");
        }

        [TestMethod]
        public async Task Still_be_switchable_when_explicitly_sent()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("flag-esplicito");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();

            meta.SetAgentCity(path, new AgentCityConfig { Enabled = true, UseAgentWorktrees = true });

            // Preservare non deve voler dire "impossibile spegnere": inviato esplicitamente, vince.
            var query = "?path=" + System.Uri.EscapeDataString(path);
            var res = await ctx.Client.PostAsync("/api/MdProjects/SetAgentCity" + query,
                new StringContent("{\"enabled\":true,\"useAgentWorktrees\":false}",
                    Encoding.UTF8, "application/json"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, res.StatusCode);

            Assert.IsFalse(meta.GetAgentCity(path).UseAgentWorktrees);
        }
    }
}
