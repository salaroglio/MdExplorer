using System;
using System.IO;
using System.Linq;
using MdExplorer.IntegrationTests.Infrastructure;
using MdExplorer.Services.CodeGraph;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// «Non ricordarmelo più» delle radici di codice esterne nello UserDB vero (sprint 2026-10-12, F5). Setting.Name è
    /// UNIQUE: la riga di un SECONDO progetto non deve scontrarsi con quella del primo (trovato il 21/10/2026 leggendo la
    /// migrazione, durante lo sprint Cartella-Remota-Via-SSH).
    /// </summary>
    [TestClass]
    public class CodeRootReminderStore_Should
    {
        [TestMethod]
        public void Remember_dismissed_roots_for_each_project_separately()
        {
            using var ctx = new AgentCityContext();
            var store = ctx.Factory.Services.GetRequiredService<ICodeRootBindingStore>();
            var first = Path.Combine(ctx.Factory.DataDir, "doc-uno");
            var second = Path.Combine(ctx.Factory.DataDir, "doc-due");

            store.DismissReminders(first, new[] { "backend" });
            store.DismissReminders(second, new[] { "frontend" });
            store.DismissReminders(first, new[] { "tools" });

            CollectionAssert.AreEquivalent(new[] { "backend", "tools" }, store.DismissedReminders(first).ToArray());
            CollectionAssert.AreEquivalent(new[] { "frontend" }, store.DismissedReminders(second).ToArray());
            Assert.AreEqual(0, store.DismissedReminders(Path.Combine(ctx.Factory.DataDir, "altro")).Count);
        }
    }
}
