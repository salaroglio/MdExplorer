using System.IO;
using System.Linq;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.DatabaseManager;
using Ad.Tools.Dal.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Il posto di lavoro di un agente contiene una copia intera del progetto, schede comprese. Se l'indice la
    /// contiene, il registro vede lo stesso nome due volte e — per la regola dei nomi duplicati — li esclude tutti:
    /// la rubrica si svuota e nessun agente può più scrivere a un collega.
    /// <para>
    /// È successo sul clone del demo in inglese, al secondo risveglio di un agente: la risposta di
    /// <c>list_agents</c> era <c>[]</c> e <c>send_agent_message</c> rispondeva «Destinatario non trovato».
    /// </para>
    /// </summary>
    [TestClass]
    public class AgentRegistryWorktrees_Should
    {
        [TestMethod]
        public void Ignore_the_copy_of_a_card_inside_an_agent_workplace()
        {
            using var ctx = new AgentCityContext();
            var (_, path) = ctx.SeedProject("registro-con-posti");

            var original = Path.Combine(path, "agente.agent.md");
            ctx.WriteLlmCitizen(path, "worker", "Lavoratore", new[] { "*" });
            var workplaceDir = Path.Combine(path, ".worktrees", "slot-1");
            Directory.CreateDirectory(workplaceDir);
            ctx.WriteLlmCitizen(workplaceDir, "worker", "Lavoratore", new[] { "*" });

            // Entrambi nell'indice, come quando i file del posto di lavoro vengono indicizzati.
            var dbm = ctx.Factory.Services.GetRequiredService<IDatabaseManager>();
            using (var engine = dbm.CreateIsolatedEngineDBForProjectPath(path))
            {
                engine.BeginTransaction();
                var dal = engine.GetDal<MarkdownFile>();
                foreach (var file in Directory.EnumerateFiles(path, "*.agent.md", SearchOption.AllDirectories))
                    dal.Save(new MarkdownFile { FileName = Path.GetFileName(file), Path = file, FileType = ".md" });
                engine.Commit();
            }

            var catalog = ctx.Factory.Services.GetRequiredService<IAgentRegistryService>().RefreshCatalog(path);

            var workers = catalog.Where(e => e.Name == "worker").ToList();
            Assert.AreEqual(1, workers.Count, "una sola scheda: la copia nel posto di lavoro non è un agente");
            Assert.IsNull(workers[0].RegistrationError, "nessun «nome duplicato»: l'agente deve restare registrato");
            Assert.IsTrue(workers[0].IsCitizen);
            Assert.IsFalse(workers[0].AgentFilePath.Contains(".worktrees"), "quella registrata è l'originale");
        }
    }
}
