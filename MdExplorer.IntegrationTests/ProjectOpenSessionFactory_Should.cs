using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Sprint .NET 10, L1b (18/11/2026): aprire un progetto sostituiva TUTTE e tre le session factory, anche quella del
    /// database dell'utente, il cui file non cambia mai, e chiudeva le vecchie. Le sessioni già prese nella stessa
    /// richiesta (MdProjectsController.SetFolderProject, JavaHealthService) restavano su una factory chiusa: NHibernate
    /// 5.3.9 le lasciava lavorare, la 5.4 e successive lanciano ObjectDisposedException alla prima query LINQ.
    /// Il test guarda la factory, non la query, così vale con qualunque versione di NHibernate.
    /// </summary>
    [TestClass]
    public class ProjectOpenSessionFactory_Should
    {
        [TestMethod]
        public async Task Keep_the_factory_of_a_database_that_does_not_change_and_close_the_one_that_does()
        {
            var cwd = Directory.GetCurrentDirectory();
            try { await Run(); } finally { Directory.SetCurrentDirectory(cwd); }
        }

        private static async Task Run()
        {
            using var ctx = new AgentCityContext();
            var (_, first) = ctx.SeedProject("factory-uno");
            var (_, second) = ctx.SeedProject("factory-due");
            await Open(ctx, first);

            using var scope = ctx.Factory.Services.CreateScope();
            var user = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            var engine = scope.ServiceProvider.GetRequiredService<IEngineDB>();
            var project = scope.ServiceProvider.GetRequiredService<IProjectDB>();

            // Riaprire lo stesso progetto: nessuno dei tre database cambia, nessuna factory si chiude.
            await Open(ctx, first);
            Assert.IsFalse(user.SessionFactory.IsClosed, "database dell'utente: la factory della sessione già presa è stata chiusa");
            Assert.IsFalse(engine.SessionFactory.IsClosed, "stesso progetto: la factory del motore è stata chiusa");
            Assert.IsFalse(project.SessionFactory.IsClosed, "stesso progetto: la factory del progetto è stata chiusa");
            Assert.IsTrue(user.GetDal<Project>().GetList().Where(p => p.Path == first).Any(), "la sessione già presa non legge il progetto appena salvato");

            // Aprire un altro progetto: cambiano motore e progetto, non l'utente.
            await Open(ctx, second);
            Assert.IsFalse(user.SessionFactory.IsClosed, "altro progetto: la factory del database dell'utente è stata chiusa");
            Assert.IsTrue(engine.SessionFactory.IsClosed, "altro progetto: la factory del motore di prima doveva chiudersi (database diverso)");
            Assert.IsTrue(project.SessionFactory.IsClosed, "altro progetto: la factory del progetto di prima doveva chiudersi (database diverso)");
            Assert.IsTrue(user.GetDal<Project>().GetList().Where(p => p.Path == second).Any(), "la sessione dell'utente non vede il secondo progetto");
        }

        private static async Task Open(AgentCityContext ctx, string path)
        {
            var (status, body) = await ctx.PostJson("/api/MdProjects/SetFolderProject?ConnectionId=factory-" + Guid.NewGuid().ToString("N"),
                System.Text.Json.JsonSerializer.Serialize(new { path }));
            Assert.AreEqual(HttpStatusCode.OK, status, body);
        }
    }
}
