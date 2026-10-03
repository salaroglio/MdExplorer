using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Service.Models;
using MdExplorer.Services;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Impostazioni del relay <b>per progetto</b> (UserDB, chiave cifrata). Esercita entità,
    /// mapping e migrazione col Service reale, e soprattutto la <b>catena di risoluzione</b>:
    /// progetto → .development.yml → globale. La chiave sta qui e non in git perché apre il
    /// relay intero, mentre il room secret apre una sola stanza.
    /// </summary>
    [TestClass]
    public class RelaySettings_Should
    {
        private static IProjectRelaySettingsService Svc(AgentCityContext ctx)
            => ctx.Factory.Services.GetRequiredService<IProjectRelaySettingsService>();

        [TestMethod]
        public void Store_the_api_key_encrypted_and_never_in_clear()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("relay-store");
            var svc = Svc(ctx);

            svc.Save(path, relayUrl: null, apiKeyPlain: "chiave-super-segreta", clearApiKey: false);

            // Risolta in chiaro solo attraverso il servizio...
            Assert.AreEqual("chiave-super-segreta", svc.ResolveApiKey(path));

            // ...ma la vista per la UI non la espone mai.
            var view = svc.Get(path, null);
            Assert.IsTrue(view.HasApiKey);
            Assert.AreEqual(RelaySettingSource.Project, view.ApiKeySource);

            // ...e a riposo non è in chiaro nel database.
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.BeginTransaction();
            var stored = db.GetDal<MdExplorer.Abstractions.Entities.UserDB.ProjectRelaySettings>()
                .GetList().ToList();
            db.Commit();

            Assert.AreEqual(1, stored.Count);
            Assert.IsFalse(string.IsNullOrWhiteSpace(stored[0].ApiKeyEncrypted));
            Assert.AreNotEqual("chiave-super-segreta", stored[0].ApiKeyEncrypted,
                "la chiave non deve essere salvata in chiaro");
        }

        [TestMethod]
        public void Keep_the_saved_key_when_the_ui_does_not_resend_it()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("relay-keep");
            var svc = Svc(ctx);

            svc.Save(path, null, "chiave-1", clearApiKey: false);
            // La UI salva solo l'indirizzo: la chiave non viaggia, e non deve sparire.
            svc.Save(path, "wss://relay.interno/mdchat", apiKeyPlain: null, clearApiKey: false);

            Assert.AreEqual("chiave-1", svc.ResolveApiKey(path));
            Assert.AreEqual("wss://relay.interno/mdchat", svc.Get(path, null).RelayUrl);
        }

        [TestMethod]
        public void Clear_the_key_only_when_asked_explicitly()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("relay-clear");
            var svc = Svc(ctx);

            svc.Save(path, null, "chiave-1", clearApiKey: false);
            svc.Save(path, null, apiKeyPlain: null, clearApiKey: true);

            var view = svc.Get(path, null);
            Assert.IsFalse(view.HasApiKey, "rimossa la chiave di progetto");
            Assert.AreEqual(RelaySettingSource.None, view.ApiKeySource,
                "senza chiave globale configurata la federazione resta dormiente");
            Assert.IsNull(svc.ResolveApiKey(path));
        }

        [TestMethod]
        public void Prefer_the_project_url_over_the_development_yml_one()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("relay-url");
            var svc = Svc(ctx);

            // Solo .development.yml: vince quello, ed è dichiarato come tale.
            var fromYml = svc.Get(path, "wss://relay-di-squadra/mdchat");
            Assert.AreEqual("wss://relay-di-squadra/mdchat", fromYml.RelayUrl);
            Assert.AreEqual(RelaySettingSource.DevelopmentYml, fromYml.RelayUrlSource);

            // Impostazione locale di progetto: ha la precedenza (override della macchina).
            svc.Save(path, "wss://relay-di-questa-macchina/mdchat", null, clearApiKey: false);
            var fromProject = svc.Get(path, "wss://relay-di-squadra/mdchat");
            Assert.AreEqual("wss://relay-di-questa-macchina/mdchat", fromProject.RelayUrl);
            Assert.AreEqual(RelaySettingSource.Project, fromProject.RelayUrlSource);

            // Nessuno dei due: si ricade sul default globale, senza inventare nulla.
            var (_, other) = ctx.SeedProject("relay-url-default");
            var fallback = svc.Get(other, null);
            Assert.IsFalse(string.IsNullOrWhiteSpace(fallback.RelayUrl));
            Assert.AreEqual(RelaySettingSource.Global, fallback.RelayUrlSource);
        }

        [TestMethod]
        public void Fail_loud_when_the_project_is_not_registered()
        {
            using var ctx = new AgentCityContext();
            var svc = Svc(ctx);

            var ex = Assert.ThrowsException<System.InvalidOperationException>(
                () => svc.Save("/percorso/mai/aperto", null, "k", false));

            StringAssert.Contains(ex.Message, "Nessun progetto registrato",
                "il messaggio deve dire cosa fare, non solo che è fallito");
        }

        /// <summary>
        /// Gli endpoint veri, non solo il servizio: la UI manda un corpo <b>parziale</b> (solo la
        /// chiave, senza relayUrl) e con DTO a reference type non-nullable la validazione
        /// automatica di <c>[ApiController]</c> risponderebbe 400 prima ancora di entrare
        /// nell'action — è esattamente il difetto che rese inutilizzabile l'attivazione città
        /// dalla UI (memoria <c>dto_nullable_implicit_required</c>).
        /// </summary>
        [TestMethod]
        public async System.Threading.Tasks.Task Accept_a_partial_body_from_the_ui()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("relay-http");
            var query = "?path=" + System.Uri.EscapeDataString(path);

            // Solo la chiave: nessun relayUrl nel corpo.
            var post = await ctx.Client.PostAsync(
                "/api/MdProjects/SetRelaySettings" + query,
                new System.Net.Http.StringContent("{\"apiKey\":\"chiave-dalla-ui\"}",
                    System.Text.Encoding.UTF8, "application/json"));

            Assert.AreEqual(System.Net.HttpStatusCode.OK, post.StatusCode,
                await post.Content.ReadAsStringAsync());

            var get = await ctx.Client.GetAsync("/api/MdProjects/RelaySettings" + query);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, get.StatusCode);
            var body = await get.Content.ReadAsStringAsync();

            StringAssert.Contains(body, "\"hasApiKey\":true");
            Assert.IsFalse(body.Contains("chiave-dalla-ui"),
                "la chiave non deve MAI tornare al client");

            // Corpo vuoto: la chiave salvata resta (la UI non la rimanda mai).
            var second = await ctx.Client.PostAsync(
                "/api/MdProjects/SetRelaySettings" + query,
                new System.Net.Http.StringContent("{\"relayUrl\":\"wss://altro/mdchat\"}",
                    System.Text.Encoding.UTF8, "application/json"));
            Assert.AreEqual(System.Net.HttpStatusCode.OK, second.StatusCode);
            Assert.AreEqual("chiave-dalla-ui", Svc(ctx).ResolveApiKey(path));
        }

        [TestMethod]
        public async System.Threading.Tasks.Task Report_a_missing_key_instead_of_pretending_to_test()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("relay-test-nokey");
            var svc = Svc(ctx);

            var result = await svc.TestAsync(path, null);

            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.Message, "Nessuna API key");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task Report_an_unreachable_relay_as_a_failure()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("relay-test-unreachable");
            var svc = Svc(ctx);

            // Porta chiusa su loopback: nessuna rete esterna, esito deterministico.
            svc.Save(path, "ws://127.0.0.1:1/mdchat", "chiave", clearApiKey: false);
            var result = await svc.TestAsync(path, null);

            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.Message, "irraggiungibile");

            // L'esito è registrato: la UI può mostrarlo anche dopo un riavvio.
            var view = svc.Get(path, null);
            Assert.IsNotNull(view.LastTestedAt);
            Assert.AreEqual(false, view.LastTestSuccess);
        }
    }
}
