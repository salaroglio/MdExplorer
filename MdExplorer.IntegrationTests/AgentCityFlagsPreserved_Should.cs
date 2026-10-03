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
                AutoMergeAgentDeliverables = true,
            });

            // Salvataggio "come lo fa la UI": solo enabled + ownershipDoc.
            var query = "?path=" + System.Uri.EscapeDataString(path);
            var res = await ctx.Client.PostAsync("/api/MdProjects/SetAgentCity" + query,
                new StringContent("{\"enabled\":true,\"ownershipDoc\":\"ownership.md\"}",
                    Encoding.UTF8, "application/json"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, res.StatusCode, await res.Content.ReadAsStringAsync());

            var after = meta.GetAgentCity(path);
            Assert.AreEqual(true, after.AutoMergeAgentDeliverables,
                "l'auto-merge non deve spegnersi perché la UI non lo invia");
        }

        [TestMethod]
        public async Task Still_be_switchable_when_explicitly_sent()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("flag-esplicito");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();

            meta.SetAgentCity(path, new AgentCityConfig { Enabled = true, AutoMergeAgentDeliverables = true });

            // Preservare non deve voler dire "impossibile spegnere": inviato esplicitamente, vince.
            var query = "?path=" + System.Uri.EscapeDataString(path);
            var res = await ctx.Client.PostAsync("/api/MdProjects/SetAgentCity" + query,
                new StringContent("{\"enabled\":true,\"autoMergeAgentDeliverables\":false}",
                    Encoding.UTF8, "application/json"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, res.StatusCode);

            Assert.AreEqual(false, meta.GetAgentCity(path).AutoMergeAgentDeliverables);
        }

        [TestMethod]
        public async Task Leave_the_flags_unset_on_the_very_first_save()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("flag-primo-salvataggio");

            // Nessuno ha mai scritto la sezione: la UI salva solo `enabled`.
            var query = "?path=" + System.Uri.EscapeDataString(path);
            var res = await ctx.Client.PostAsync("/api/MdProjects/SetAgentCity" + query,
                new StringContent("{\"enabled\":true}", Encoding.UTF8, "application/json"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, res.StatusCode, await res.Content.ReadAsStringAsync());

            // Si guarda il FILE, non GetAgentCity: in lettura quest'ultimo applica i default
            // (worktree attivi se c'è git), quindi non può dire «assente».
            var yml = System.IO.File.ReadAllText(System.IO.Path.Combine(path, ".development.yml"));
            StringAssert.Contains(yml, "agentCity:");
            Assert.IsFalse(yml.Contains("useAgentWorktrees"),
                "i worktree non vanno spenti da un salvataggio che non ne parla: `null` vuol dire «decide l'app»");
            Assert.IsFalse(yml.Contains("autoMergeAgentDeliverables"),
                "una scelta che nessuno ha fatto non va scritta nel file del progetto");
        }
    }
}
