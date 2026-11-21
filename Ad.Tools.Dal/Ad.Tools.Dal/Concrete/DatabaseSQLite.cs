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
        /// <summary>Quanto aspetta una connessione su un database occupato prima di rispondere «database is locked».</summary>
        public const int BusyTimeoutMs = 5000;

        public IPersistenceConfigurer Config(string connectionString)
        {
            return SQLiteConfiguration.Standard.ConnectionString(WithBusyTimeout(connectionString));
        }

        /// <summary>
        /// La stringa di connessione con <c>BusyTimeout</c> (se non c'è già): l'attesa vale dal momento in cui la connessione si
        /// apre, prima di qualunque istruzione. Anche in WAL SQLite prende per un attimo un lock esclusivo (checkpoint quando si
        /// chiude l'ultima connessione, ricostruzione del -shm alla prima dopo): chi apre in quell'istante senza attesa riceve
        /// subito «database is locked». Misurato il 21/11/2026 (sprint .NET 10): IndexingParseLinksQuery_Should falliva così
        /// una volta su due, sul primo PRAGMA della sessione.
        /// </summary>
        public static string WithBusyTimeout(string connectionString)
        {
            // il builder solo per sapere se c'è già (chiavi senza maiuscole); il testo resta quello di chi chiama
            var builder = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = connectionString };
            if (builder.ContainsKey("BusyTimeout")) return connectionString;
            return connectionString.TrimEnd().TrimEnd(';') + ";BusyTimeout=" + BusyTimeoutMs;
        }

        /// <summary>
        /// Applies SQLite PRAGMA settings for concurrent access:
        /// - busy_timeout (first): waits up to 5s instead of failing immediately on lock
        /// - WAL mode: allows concurrent reads during writes
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

            // l'attesa PRIMA di tutto: anche il cambio di journal_mode può trovare il database occupato
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA busy_timeout = {BusyTimeoutMs};";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode = WAL;";
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
