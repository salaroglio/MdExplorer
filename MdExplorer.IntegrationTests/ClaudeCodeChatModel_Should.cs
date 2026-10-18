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
    /// Il modello di Claude Code scelto per un progetto, salvato in UserDB col Service reale: entità,
    /// mapping, migrazione ed endpoint insieme. Nessun CLI coinvolto.
    /// <para>Sprint: docs-internal/Sprints/2026-09-13-MarkAgent-Modello-Claude-Code.md, fase F2.</para>
    /// </summary>
    [TestClass]
    public class ClaudeCodeChatModel_Should
    {
        private static string Stored(AgentCityContext ctx, string path)
        {
            using var scope = ctx.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
            db.BeginTransaction();
            var project = db.GetDal<MdExplorer.Abstractions.Entities.UserDB.Project>()
                .GetList().ToList().Single(p => p.Path == path);
            db.Commit();
            return project.ClaudeCodeChatModel;
        }

        private static Task<(HttpStatusCode Status, string Body)> Save(AgentCityContext ctx, string path, string modelId)
            => ctx.PostJson("/api/ProjectSettings/SetClaudeCodeChatModelSetting",
                JsonSerializer.Serialize(new { projectPath = path, modelId }));

        [TestMethod]
        public async Task Start_unchosen_and_keep_the_model_picked_for_the_project()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("claude-model");

            Assert.IsNull(Stored(ctx, path), "a project that never chose keeps the old behaviour (sonnet), not a stored name");

            var (status, body) = await Save(ctx, path, " opus[1m] ");
            Assert.AreEqual(HttpStatusCode.OK, status, body);
            Assert.AreEqual("opus[1m]", Stored(ctx, path), "the CLI's own id, trimmed, brackets included");
        }

        [TestMethod]
        public async Task Go_back_to_unchosen_when_the_model_is_null()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("claude-model-reset");
            await Save(ctx, path, "haiku");

            // null is a legal value: the DTO's string? must not turn it into an opaque 400.
            var (status, body) = await Save(ctx, path, null);

            Assert.AreEqual(HttpStatusCode.OK, status, body);
            Assert.IsNull(Stored(ctx, path));
        }

        [TestMethod]
        public async Task Answer_not_found_for_an_unknown_project()
        {
            using var ctx = new AgentCityContext();

            var (status, _) = await Save(ctx, "/nessun/progetto/qui", "sonnet");

            Assert.AreEqual(HttpStatusCode.NotFound, status);
        }
    }
}
