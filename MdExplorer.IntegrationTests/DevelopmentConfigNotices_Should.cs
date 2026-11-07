using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// F4 dello sprint «Schema di .development.yml»: lo stato del file per il client (l'avviso nella lingua dell'interfaccia e i
    /// gruppi spenti), che il client chiede a ogni apertura del progetto e nelle impostazioni.
    /// </summary>
    [TestClass]
    public class DevelopmentConfigNotices_Should
    {
        [TestMethod]
        public async Task Give_the_notice_and_the_switched_off_groups_of_a_project()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("avviso-schema");
            var file = Path.Combine(path, ".development.yml");
            File.WriteAllText(file, "# mde-schema: 9\nfolders: []\nharness:\n  target: codex\nredmine:\n  baseUrl: https://r\n  projects: []\n  effort: 1\n");

            var state = await ctx.Client.GetFromJsonAsync<DevelopmentConfigState>("/api/MdProjects/DevelopmentConfigState?path=" + System.Uri.EscapeDataString(path));

            CollectionAssert.AreEquivalent(new[] { "harness", "redmine" }, state.Off.ToList());
            Assert.IsTrue(state.WrittenByNewerMde);
            Assert.AreEqual(9, state.FileSchema);
            StringAssert.Contains(state.Notice, "Redmine (redmine)");
            StringAssert.Contains(state.Notice, "9");

            File.WriteAllText(file, "folders: []\n");
            var good = await ctx.Client.GetFromJsonAsync<DevelopmentConfigState>("/api/MdProjects/DevelopmentConfigState?path=" + System.Uri.EscapeDataString(path));
            Assert.AreEqual(string.Empty, good.Notice, "tutto a posto: nessun avviso");
            Assert.AreEqual(0, good.Off.Length);
        }
    }
}
