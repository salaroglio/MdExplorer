using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La finestra di creazione compare una volta sola, quindi cambiare harness a un progetto già
    /// esistente deve poterlo fare la pagina delle impostazioni. Qui si verifica che gli endpoint
    /// facciano davvero le due cose che servono — scrivere la scelta <b>e</b> installare i file
    /// dove il nuovo harness li vuole — senza fare quella che non va fatta: cancellare i file
    /// dell'harness precedente.
    /// <para>Sprint: docs-internal/Sprints/2026-08-31-Opencode-Harness-Support.md, fase F5.</para>
    /// </summary>
    [TestClass]
    public class HarnessSettingsEndpoint_Should
    {
        private static StringContent Json(object body)
            => new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        private static async Task<JsonElement> ReadJson(HttpResponseMessage res)
            => JsonDocument.Parse(await res.Content.ReadAsStringAsync()).RootElement;

        [TestMethod]
        public async Task Install_the_opencode_files_when_the_harness_is_switched()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("cambio-harness");

            // Il progetto nasce Copilot, come un qualunque progetto esistente.
            MdExplorer.Service.ProjectsManager.ConfigTemplates(path, null, HarnessTarget.Copilot);
            Assert.IsTrue(File.Exists(Path.Combine(path, ".github", "skills", "mde-doc", "SKILL.md")));

            var res = await ctx.Client.PostAsync("/api/ProjectSettings/SetHarness",
                Json(new { projectPath = path, target = "opencode" }));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode, await res.Content.ReadAsStringAsync());

            Assert.IsTrue(File.Exists(Path.Combine(path, ".opencode", "skills", "mde-doc", "SKILL.md")),
                "cambiare harness deve installare subito i file, non aspettare la riapertura");
            Assert.IsTrue(File.Exists(Path.Combine(path, "AGENTS.md")));
            Assert.AreEqual(HarnessTarget.OpenCode, HarnessSettings.Read(path));
        }

        [TestMethod]
        public async Task Leave_the_previous_harness_files_where_they_are()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("niente-cancellazioni");
            MdExplorer.Service.ProjectsManager.ConfigTemplates(path, null, HarnessTarget.Copilot);

            await ctx.Client.PostAsync("/api/ProjectSettings/SetHarness",
                Json(new { projectPath = path, target = "opencode" }));

            // Sono file che stanno in un repository di qualcuno, magari personalizzati o gia'
            // committati: cancellarli per un cambio di impostazione sarebbe un danno non richiesto.
            Assert.IsTrue(File.Exists(Path.Combine(path, ".github", "skills", "mde-doc", "SKILL.md")),
                "i file del vecchio harness non si cancellano di iniziativa");
        }

        [TestMethod]
        public async Task Report_a_harness_that_the_project_has_not_declared_yet()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("non-dichiarato");
            // Progetto vecchio stile: ha la cartella ma non la voce nel yml.
            Directory.CreateDirectory(Path.Combine(path, ".github", "skills"));

            var res = await ctx.Client.GetAsync("/api/ProjectSettings/GetHarness?projectPath=" + Uri.EscapeDataString(path));
            Assert.AreEqual(HttpStatusCode.OK, res.StatusCode);

            var body = await ReadJson(res);
            Assert.AreEqual("copilot", body.GetProperty("target").GetString());
            Assert.IsFalse(body.GetProperty("declared").GetBoolean(),
                "la UI deve poter dire che la scelta e' dedotta e non ancora scritta");

            // Leggere le impostazioni non deve cambiare il progetto.
            Assert.IsNull(HarnessSettings.Read(path));
        }

        [TestMethod]
        public async Task Refuse_a_harness_it_does_not_know()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("harness-inventato");

            var res = await ctx.Client.PostAsync("/api/ProjectSettings/SetHarness",
                Json(new { projectPath = path, target = "cursor" }));

            Assert.AreEqual(HttpStatusCode.BadRequest, res.StatusCode);
            StringAssert.Contains(await res.Content.ReadAsStringAsync(), "opencode",
                "l'errore deve elencare i valori ammessi");
        }
    }
}
