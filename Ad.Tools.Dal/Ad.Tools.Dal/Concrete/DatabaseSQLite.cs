using Ad.Tools.Dal.Abstractions;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Concrete
{
    public class DatabaseSQLite : IDatabase
    {
        public IPersistenceConfigurer Config(string connectionString)
        {
            return SQLiteConfiguration.Standard.ConnectionString(connectionString);
        }

        /// <summary>
        /// Applies SQLite PRAGMA settings for concurrent access:
        /// - WAL mode: allows concurrent reads during writes
        /// - busy_timeout: waits up to 5s instead of failing immediately on lock
        /// - synchronous=NORMAL: safe performance trade-off with WAL
        /// </summary>
        public static void ApplyPragmas(ISession session)
        {
            if (session?.Connection == null)
                return;

            var connection = session.Connection;

            // Ensure connection is open before executing PRAGMAs
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode = WAL;";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA busy_timeout = 5000;";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA synchronous = NORMAL;";
                cmd.ExecuteNonQuery();
            }
        }
    }
}
