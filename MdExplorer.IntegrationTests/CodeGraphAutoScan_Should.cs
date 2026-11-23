using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.CodeGraph;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La scansione automatica del codice per il grafo è una scelta del progetto su questa macchina (23/11/2026):
    /// spenta finché non la si accende, perché su un computer lento rallenta l'apertura e la chat.
    /// </summary>
    [TestClass]
    public class CodeGraphAutoScan_Should
    {
        private static async Task<bool> Enabled(AgentCityContext ctx, string path)
        {
            var resp = await ctx.Client.GetAsync("/api/ProjectSettings/GetCodeGraphAutoScanSetting?projectPath=" + Uri.EscapeDataString(path));
            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode, await resp.Content.ReadAsStringAsync());
            return JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync()).GetProperty("enabled").GetBoolean();
        }

        [TestMethod]
        public async Task Stay_off_until_turned_on_and_remember_the_choice_per_project()
        {
            using var ctx = new AgentCityContext();
            var (_, first) = ctx.SeedProject("grafo-" + Guid.NewGuid().ToString("N")[..6]);
            var (_, second) = ctx.SeedProject("grafo-" + Guid.NewGuid().ToString("N")[..6]);

            Assert.IsFalse(await Enabled(ctx, first), "di partenza è spenta");

            var on = await ctx.Client.PostAsJsonAsync("/api/ProjectSettings/SetCodeGraphAutoScanSetting", new { projectPath = first, enabled = true });
            Assert.AreEqual(HttpStatusCode.OK, on.StatusCode, await on.Content.ReadAsStringAsync());
            Assert.IsTrue(await Enabled(ctx, first));
            Assert.IsTrue(await Enabled(ctx, first + System.IO.Path.DirectorySeparatorChar), "lo stesso progetto, scritto con la barra in fondo");
            Assert.IsFalse(await Enabled(ctx, second), "un altro progetto non ne risente");

            await ctx.Client.PostAsJsonAsync("/api/ProjectSettings/SetCodeGraphAutoScanSetting", new { projectPath = first, enabled = false });
            Assert.IsFalse(await Enabled(ctx, first));
        }

        [TestMethod]
        public async Task Keep_loading_into_fuseki_and_scanning_the_code_as_two_separate_choices()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("grafo-" + Guid.NewGuid().ToString("N")[..6]);

            async Task<(bool Scan, bool Load)> Both()
            {
                var body = JsonSerializer.Deserialize<JsonElement>(await (await ctx.Client.GetAsync(
                    "/api/ProjectSettings/GetCodeGraphAutoScanSetting?projectPath=" + Uri.EscapeDataString(path))).Content.ReadAsStringAsync());
                return (body.GetProperty("enabled").GetBoolean(), body.GetProperty("loadEnabled").GetBoolean());
            }

            Assert.AreEqual((false, false), await Both(), "di partenza spente tutte e due");

            await ctx.Client.PostAsJsonAsync("/api/ProjectSettings/SetCodeGraphAutoScanSetting", new { projectPath = path, loadEnabled = true });
            Assert.AreEqual((false, true), await Both(), "accendere il caricamento non accende la scansione");

            await ctx.Client.PostAsJsonAsync("/api/ProjectSettings/SetCodeGraphAutoScanSetting", new { projectPath = path, enabled = true });
            Assert.AreEqual((true, true), await Both());

            await ctx.Client.PostAsJsonAsync("/api/ProjectSettings/SetCodeGraphAutoScanSetting", new { projectPath = path, loadEnabled = false });
            Assert.AreEqual((true, false), await Both(), "spegnere il caricamento lascia la scansione com'è");
        }

        [TestMethod]
        public void Name_the_setting_by_the_fingerprint_of_the_path()
        {
            var name = CodeGraphAutoScanPreference.SettingName(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "progetti", "acme"));
            StringAssert.StartsWith(name, CodeGraphAutoScanPreference.SettingPrefix);
            Assert.AreEqual(CodeGraphAutoScanPreference.SettingPrefix.Length + 32, name.Length, "corto: un percorso lungo non starebbe nel nome");
        }
    }
}
