using System;
using System.Data.Common;
using FluentMigrator.Runner.Processors.SQLite;

namespace Ad.Tools.FluentMigrator
{
    /// <summary>
    /// Le migrazioni aprono i database SQLite con System.Data.SQLite, la stessa libreria di NHibernate.
    /// FluentMigrator 8 sceglie da solo Microsoft.Data.Sqlite quando la trova caricabile (la cerca per prima; la 3.2.17
    /// cercava System.Data.SQLite): nel servizio c'è (FTS), e due librerie native sullo stesso file in WAL si cancellano a
    /// vicenda il -wal/-shm (misurato il 22/09/2026, «disk I/O error»). Sonda del 21/11/2026, sprint .NET 10, L1d.
    /// </summary>
    public sealed class SystemDataSQLiteDbFactory : SQLiteDbFactory
    {
        public SystemDataSQLiteDbFactory(IServiceProvider serviceProvider) : base(serviceProvider) { }

        protected override DbProviderFactory CreateFactory() => System.Data.SQLite.SQLiteFactory.Instance;
    }
}
