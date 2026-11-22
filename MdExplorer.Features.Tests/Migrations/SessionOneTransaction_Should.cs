using System;
using System.IO;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.Dal.Decorators;
using Ad.Tools.Dal.Extensions;
using FluentNHibernate.Cfg;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Migrations
{
    /// <summary>
    /// Una transazione alla volta per sessione (21/11/2026). NHibernate, a un secondo BeginTransaction sulla stessa sessione,
    /// restituisce la transazione già aperta: il Commit della funzione interna chiudeva tutto e il Rollback di quella esterna
    /// non annullava più niente (provato: 3 righe scritte su 3, con chi stava fuori che voleva annullarle). Ora si ferma subito.
    /// </summary>
    [TestClass]
    public class SessionOneTransaction_Should
    {
        [TestMethod]
        public void Refuse_a_second_transaction_and_accept_the_next_one_after_commit_or_rollback()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"mde-one-tx-{Guid.NewGuid():N}.db");
            try
            {
                using var factory = Fluently.Configure().Database(new DatabaseSQLite().Config($"Data Source={dbPath}")).BuildSessionFactory();
                long Count() { using var other = factory.OpenSession(); return Convert.ToInt64(other.CreateSQLQuery("select count(*) from t").UniqueResult()); }
                using (var setup = factory.OpenSession()) setup.CreateSQLQuery("create table t(x text)").ExecuteUpdate();

                using var session = new SessionDB(factory.OpenSession());
                session.BeginTransaction();
                session.CreateSQLQuery("insert into t values('fuori')").ExecuteUpdate();
                var refused = Assert.ThrowsExactly<InvalidOperationException>(() => session.BeginTransaction());
                StringAssert.Contains(refused.Message, Path.GetFileName(dbPath), "dice su quale database");
                Assert.ThrowsExactly<InvalidOperationException>(() => session.BeginTransaction(System.Data.IsolationLevel.ReadCommitted));
                session.Rollback();
                Assert.AreEqual(0L, Count(), "il rollback di chi ha aperto annulla tutto");

                session.BeginTransaction();   // dopo un rollback si riparte
                session.CreateSQLQuery("insert into t values('uno')").ExecuteUpdate();
                session.Commit();
                session.BeginTransaction();   // e dopo un commit
                session.CreateSQLQuery("insert into t values('due')").ExecuteUpdate();
                session.Commit();
                Assert.AreEqual(2L, Count());
            }
            finally
            {
                System.Data.SQLite.SQLiteConnection.ClearAllPools();
                GC.Collect(); GC.WaitForPendingFinalizers();
                foreach (var f in new[] { dbPath, dbPath + "-wal", dbPath + "-shm" }) try { File.Delete(f); } catch { /* best effort */ }
            }
        }
    }
}
