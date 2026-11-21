using System.Data.Common;
using FluentMigrator.Runner.Initialization;

namespace Ad.Tools.FluentMigrator
{
    /// <summary>
    /// La stringa di connessione delle migrazioni SQLite con <c>BusyTimeout</c>, come quelle di NHibernate
    /// (Ad.Tools.Dal.Concrete.DatabaseSQLite.WithBusyTimeout): le migrazioni girano anche a servizio acceso (ogni attività in
    /// sottofondo che apre il database del motore le esegue), e senza attesa un lock esclusivo di un attimo (checkpoint del
    /// WAL) le farebbe fallire. Sprint .NET 10, L1d (21/11/2026).
    /// </summary>
    public sealed class BusyTimeoutConnectionStringAccessor : IConnectionStringAccessor
    {
        public const int BusyTimeoutMs = 5000;
        private readonly IConnectionStringAccessor _inner;

        public BusyTimeoutConnectionStringAccessor(IConnectionStringAccessor inner) => _inner = inner;

        public string ConnectionString
        {
            get
            {
                var value = _inner.ConnectionString;
                if (string.IsNullOrEmpty(value)) return value;
                var builder = new DbConnectionStringBuilder { ConnectionString = value };
                if (builder.ContainsKey("BusyTimeout")) return value;
                return value.TrimEnd().TrimEnd(';') + ";BusyTimeout=" + BusyTimeoutMs;
            }
        }
    }
}
