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
    /// Il flag opt-in dei worktree è booleano: non distingue "non inviato" da "false". La UI manda solo
    /// <c>enabled</c> e <c>ownershipDoc</c>, quindi un salvataggio dalle impostazioni non deve scrivere nel file
    /// del progetto una scelta che nessuno ha fatto.
    /// <para>
    /// C'era anche <c>autoMergeAgentDeliverables</c>: ritirato il 2026-08-02 (un documento di un agente propone,
    /// decide una persona) e tolto del tutto il 2026-10-05. I file che lo contengono ancora si leggono lo stesso,
    /// e al primo salvataggio la riga sparisce.
    /// </para>
    /// </summary>
    [TestClass]
    public class AgentCityFlagsPreserved_Should
    {
        [TestMethod]
        public async Task Read_a_file_that_still_has_the_retired_auto_merge_key_and_drop_it_on_save()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("flag-ritirato");
            var meta = ctx.Factory.Services.GetRequiredService<IProjectMetadataService>();
            var file = System.IO.Path.Combine(path, ".development.yml");
            System.IO.File.WriteAllText(file,
                "agentCity:\n  enabled: true\n  ownershipDoc: ownership.md\n  autoMergeAgentDeliverables: true\n");

            var read = meta.GetAgentCity(path);
            Assert.IsNotNull(read, "una chiave che l'app non conosce più non deve impedire di leggere la città");
            Assert.IsTrue(read.Enabled);
            Assert.AreEqual("ownership.md", read.OwnershipDoc);

            var query = "?path=" + System.Uri.EscapeDataString(path);
            var res = await ctx.Client.PostAsync("/api/MdProjects/SetAgentCity" + query,
                new StringContent("{\"enabled\":true,\"ownershipDoc\":\"ownership.md\"}", Encoding.UTF8, "application/json"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, res.StatusCode, await res.Content.ReadAsStringAsync());
            Assert.IsFalse((await res.Content.ReadAsStringAsync()).Contains("autoMerge"), "il servizio non lo dichiara più");

            var yml = System.IO.File.ReadAllText(file);
            Assert.IsFalse(yml.Contains("autoMergeAgentDeliverables"), "al primo salvataggio la riga ritirata sparisce");
            StringAssert.Contains(yml, "ownershipDoc: ownership.md");
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
        }

        [TestMethod]
        public void Delete_the_retired_key_from_the_file_when_the_project_opens()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("riga-ritirata-apertura");
            var file = System.IO.Path.Combine(path, ".development.yml");
            System.IO.File.WriteAllText(file,
                "folders: []\nagentCity:\n  enabled: true\n  autoMergeAgentDeliverables: true\n");

            // È ciò che fa l'apertura di un progetto, prima che qualcuno legga il file.
            var rewritten = MdExplorer.Utilities.DevelopmentConfigCleanup.CleanFile(path, null);
            Assert.IsTrue(rewritten, "il file aveva una riga ritirata: va riscritto");
            Assert.IsFalse(MdExplorer.Utilities.DevelopmentConfigCleanup.CleanFile(path, null), "pulito una volta, non si riscrive più");

            var yml = System.IO.File.ReadAllText(file);
            Assert.IsFalse(yml.Contains("autoMergeAgentDeliverables"), "la riga ritirata si cancella dal file, non si ignora");
            StringAssert.Contains(yml, "enabled: true", "il resto resta");
        }

        [TestMethod]
        public async Task Read_strictly_a_file_with_the_retired_key_but_refuse_a_key_nobody_knows()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("lettura-rigida");
            var file = System.IO.Path.Combine(path, ".development.yml");
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(path, "docs"));
            var body = System.Text.Json.JsonSerializer.Serialize(new
            {
                folderPath = System.IO.Path.Combine(path, "docs"), projectRoot = path, tags = new[] { "wip" },
            });

            // La riga ritirata non fa fallire un punto che legge in modo rigido: viene tolta prima.
            System.IO.File.WriteAllText(file, "folders: []\nagentCity:\n  enabled: true\n  autoMergeAgentDeliverables: true\n");
            var ok = await ctx.Client.PostAsync("/api/mdfiles/SetDevelopmentTags", new StringContent(body, Encoding.UTF8, "application/json"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, ok.StatusCode, await ok.Content.ReadAsStringAsync());
            Assert.IsFalse(System.IO.File.ReadAllText(file).Contains("autoMergeAgentDeliverables"));

            // Un errore di battitura NON è una riga ritirata: la lettura rigida si ferma e dice quale riga.
            System.IO.File.WriteAllText(file, "folders: []\nagentCity:\n  enabeld: true\n");
            var refused = await ctx.Client.PostAsync("/api/mdfiles/SetDevelopmentTags", new StringContent(body, Encoding.UTF8, "application/json"));
            Assert.AreEqual(System.Net.HttpStatusCode.InternalServerError, refused.StatusCode);
            StringAssert.Contains(await refused.Content.ReadAsStringAsync(), "enabeld");
            StringAssert.Contains(System.IO.File.ReadAllText(file), "enabeld", "e il file non viene riscritto");
        }
    }
}
