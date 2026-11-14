using System.IO;
using System.Threading.Tasks;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services;
using MdExplorer.Services.AgentRun;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// R8 della valutazione del 07/11, sul percorso vero del lancio a mano: una scheda scritta male non parte (prima partiva
    /// senza i limiti di <c>tools:</c> e sul motore predefinito); con <c>agentCards.requireToolsAndRuntime</c> in
    /// <c>.development.yml</c> non parte nemmeno una scheda senza <c>tools:</c> o <c>runtime:</c>; i limiti di <c>tools:</c>
    /// arrivano al motore anche per una scheda che non è un cittadino.
    /// </summary>
    [TestClass]
    public class AgentCardRules_Should
    {
        private static async Task Launch(AgentCityContext ctx, string path, string card)
        {
            File.WriteAllText(Path.Combine(path, "prova.agent.md"), card);
            await ctx.Factory.Services.GetRequiredService<IAgentRunJobService>().RunAsync(new AgentRunRequestModel
            {
                ProjectPath = path,
                AgentFilePath = Path.Combine(path, "prova.agent.md"),
                PreparedPrompt = "lavora",
                TriggerSource = "manual",
                UseWorktree = false,
            });
        }

        [TestMethod]
        public async Task Not_launch_a_card_that_cannot_be_read()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("schede-rotte");

            await Launch(ctx, path, "---\ntools: [read\nruntime:\n  provder: claude\n---\nlavora");

            Assert.AreEqual(0, ctx.Runner.Calls, "scritta male: non parte");
        }

        [TestMethod]
        public async Task Pass_the_tools_of_a_card_that_is_not_a_citizen_to_the_engine()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("schede-limiti");

            await Launch(ctx, path, "---\ndescription: legge soltanto\ntools: [read]\n---\nlavora");

            Assert.AreEqual(1, ctx.Runner.Calls);
            CollectionAssert.AreEqual(new[] { "read" }, new System.Collections.Generic.List<string>(ctx.Runner.LastRequest.DeclaredTools), "prima: null, cioè nessun limite");
        }

        [TestMethod]
        public async Task Require_tools_and_runtime_only_when_the_project_says_so_and_save_the_rule_for_the_team()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("schede-regola");
            const string bare = "---\ndescription: niente dichiarazioni\n---\nlavora";

            await Launch(ctx, path, bare);
            Assert.AreEqual(1, ctx.Runner.Calls, "spenta (il predefinito): facoltativi");

            var (status, body) = await ctx.PostJson("/api/MdProjects/SetAgentCards?path=" + System.Uri.EscapeDataString(path), "{\"requireToolsAndRuntime\":true}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, status, body);
            var yml = File.ReadAllText(Path.Combine(path, ".development.yml"));
            StringAssert.Contains(yml, "agentCards:");
            StringAssert.Contains(yml, "requireToolsAndRuntime: true");
            StringAssert.StartsWith(yml, $"# mde-schema: {MdExplorer.Features.Configuration.DevelopmentSchemaSnapshots.Current}\n");
            Assert.IsTrue(ctx.Factory.Services.GetRequiredService<IProjectMetadataService>().GetAgentCards(path).RequireToolsAndRuntime);

            await Launch(ctx, path, bare);
            Assert.AreEqual(1, ctx.Runner.Calls, "accesa: senza tools: né runtime: non parte");

            await Launch(ctx, path, "---\ntools: [read, write]\nruntime:\n  provider: claude\n---\nlavora");
            Assert.AreEqual(2, ctx.Runner.Calls, "accesa e dichiarate: parte");
        }
    }
}
