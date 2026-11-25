using System;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// La riga della cronologia delle chat nel DB utente (sprint 2026-11-24-Chat-Memorizzate, F1): mapping e migrazione
    /// del servizio vero, con un dettaglio markdown lungo e lettere accentate.
    /// </summary>
    [TestClass]
    public class ChatThreadStore_Should
    {
        [TestMethod]
        public void Save_and_read_back_a_chat_row()
        {
            using var ctx = new AgentCityContext();
            var detail = "- Inventario del codice per macOS\n- Prezzi di Apple, GitHub e Mac a noleggio\n- Scenari: gratis, «pulito», con un Mac\n"
                         + new string('x', 10_000);
            Guid id;
            var started = new DateTime(2026, 11, 25, 8, 0, 0);

            using (var scope = ctx.Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                db.BeginTransaction();
                var row = new ChatThread
                {
                    ProjectPath = "/home/acme/Università",
                    Title = "Studio del costo per macOS",
                    Detail = detail,
                    FilePath = "abc/placeholder.jsonl",
                    Engine = "claude",
                    ModelId = "sonnet",
                    EngineSessionId = "69c827e9-f3ee-4fb9-a384-8c8b593cb79e",
                    StartedAt = started,
                    LastActivityAt = started.AddMinutes(5),
                };
                db.GetDal<ChatThread>().Save(row);   // Id GuidComb assegnato al Save
                db.Commit();
                id = row.Id;
            }

            Assert.AreNotEqual(Guid.Empty, id);

            using (var scope = ctx.Factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
                var read = db.GetDal<ChatThread>().GetList().Single(t => t.Id == id);
                Assert.AreEqual("/home/acme/Università", read.ProjectPath);
                Assert.AreEqual("Studio del costo per macOS", read.Title);
                Assert.AreEqual(detail, read.Detail, "il dettaglio markdown torna intero");
                Assert.AreEqual("69c827e9-f3ee-4fb9-a384-8c8b593cb79e", read.EngineSessionId);
                Assert.AreEqual(started.AddMinutes(5), read.LastActivityAt);
            }
        }
    }
}
