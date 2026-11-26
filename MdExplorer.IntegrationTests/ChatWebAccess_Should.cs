using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// «Consenti alla chat l'accesso al web» (rotella di MarkAgent): per utente e per progetto nello userdb, col Service reale
    /// (entità, mapping, migrazione, endpoint e la lettura che l'hub fa a ogni messaggio), e le opzioni di Claude che ne seguono.
    /// </summary>
    [TestClass]
    public class ChatWebAccess_Should
    {
        private static bool ReadAsTheHubDoes(AgentCityContext ctx, string path)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            return MdExplorer.Service.ProjectsManager.ChatWebAccessEnabled(db, path);
        }

        private static Task<(HttpStatusCode Status, string Body)> Save(AgentCityContext ctx, string path, bool enabled)
            => ctx.PostJson("/api/ProjectSettings/SetChatWebAccessSetting", JsonSerializer.Serialize(new { projectPath = path, enabled }));

        [TestMethod]
        public async Task Start_off_and_keep_the_choice_of_each_project()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("web-on");
            var (_, other) = ctx.SeedProject("web-off");

            Assert.IsFalse(ReadAsTheHubDoes(ctx, path), "spento di default: un progetto esistente resta com'era");
            var (status, body) = await Save(ctx, path, true);
            Assert.AreEqual(HttpStatusCode.OK, status, body);

            Assert.IsTrue(ReadAsTheHubDoes(ctx, path));
            Assert.IsFalse(ReadAsTheHubDoes(ctx, other), "la scelta è del progetto, non di tutti");
            var (getStatus, json) = await ctx.GetJson("/api/ProjectSettings/GetChatWebAccessSetting?projectPath=" + WebUtility.UrlEncode(path));
            Assert.AreEqual(HttpStatusCode.OK, getStatus);
            Assert.IsTrue(json.RootElement.GetProperty("enabled").GetBoolean());

            (status, body) = await Save(ctx, path, false);
            Assert.AreEqual(HttpStatusCode.OK, status, body);
            Assert.IsFalse(ReadAsTheHubDoes(ctx, path));
        }

        [TestMethod]
        public async Task Answer_not_found_for_a_project_that_does_not_exist()
        {
            using var ctx = new AgentCityContext();
            var (status, _) = await Save(ctx, "/nessun/progetto", true);
            Assert.AreEqual(HttpStatusCode.NotFound, status);
            Assert.IsFalse(ReadAsTheHubDoes(ctx, "/nessun/progetto"));
        }

        [TestMethod]
        public void Authorize_WebFetch_and_WebSearch_for_Claude_only_when_on()
        {
            var on = MdExplorer.Utilities.ClaudeCodeMcp.ChatOptions(null, null, webAccess: true).AllowedTools;
            var off = MdExplorer.Utilities.ClaudeCodeMcp.ChatOptions(null, null, webAccess: false).AllowedTools;
            CollectionAssert.IsSubsetOf(new[] { "WebFetch", "WebSearch", "Edit(./**)" }, on.ToList());
            Assert.IsFalse(off.Any(t => t.StartsWith("Web")), "spento: in dontAsk restano negati come prima");
            Assert.AreNotEqual(MdExplorer.Features.Services.AI.ClaudeCode.ClaudeCodeSession.DirectoriesKey(on),
                MdExplorer.Features.Services.AI.ClaudeCode.ClaudeCodeSession.DirectoriesKey(off),
                "chiave diversa: cambiare l'interruttore riavvia la sessione con --resume");
        }
    }
}
