using System.IO;
using System.Linq;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.Federation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Fase 6b/6c — attivazione headless (§12.7 regola 2): un progetto "spento" (mai aperto da
    /// un client, quindi con l'indice Engine DB vuoto) deve diventare scopribile per la
    /// federazione. L'attivatore indicizza gli .agent.md e riconcilia il registry, così il gate
    /// federato può risolvere e svegliare gli agenti senza UI.
    /// </summary>
    [TestClass]
    public class HeadlessActivation_Should
    {
        [TestMethod]
        public void Index_agents_of_an_unopened_project_so_they_become_discoverable()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("headless");

            // Agenti su disco ma NON indicizzati (niente IndexAgentFiles): progetto "spento".
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            var sub = Path.Combine(path, "sub");
            Directory.CreateDirectory(sub);
            ctx.WriteLlmCitizen(sub, "nested", "Annidato", new[] { "*" });
            // Un .agent.md dentro .git NON deve essere scoperto (cartella di rumore).
            var gitDir = Path.Combine(path, ".git");
            Directory.CreateDirectory(gitDir);
            ctx.WriteLlmCitizen(gitDir, "ghost", "Fantasma", new[] { "*" });

            var registry = ctx.Factory.Services.GetRequiredService<IAgentRegistryService>();
            Assert.IsFalse(registry.RefreshCatalog(path).Any(e => e.IsCitizen && e.Name == "worker"),
                "non indicizzato → non scoperto");

            // Attivazione headless.
            ctx.Factory.Services.GetRequiredService<IHeadlessProjectActivator>().ActivateForFederation(path);

            var catalog = registry.RefreshCatalog(path);
            Assert.IsTrue(catalog.Any(e => e.IsCitizen && e.Name == "worker"), "worker scoperto dopo l'attivazione");
            Assert.IsTrue(catalog.Any(e => e.IsCitizen && e.Name == "nested"), "anche i nested (.agent.md in sottocartelle)");
            Assert.IsFalse(catalog.Any(e => e.IsCitizen && e.Name == "ghost"), "gli .agent.md dentro .git sono saltati");
        }

        [TestMethod]
        public void Be_idempotent()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("headless-idem");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });

            var activator = ctx.Factory.Services.GetRequiredService<IHeadlessProjectActivator>();
            activator.ActivateForFederation(path);
            activator.ActivateForFederation(path);   // seconda volta: nessun duplicato

            var registry = ctx.Factory.Services.GetRequiredService<IAgentRegistryService>();
            Assert.AreEqual(1, registry.RefreshCatalog(path).Count(e => e.IsCitizen && e.Name == "worker"));
        }
    }
}
