using FluentMigrator;
using System;

namespace MdExplorer.Migrations.Version202511
{
    [Migration(20251114001, "Fix IntelliJPath setting - remove corrupted records and allow fresh discovery")]
    public class M2025_11_14_001 : Migration
    {
        public override void Up()
        {
            // Remove any existing IntelliJPath setting (corrupted or not)
            // This allows the ApplicationInitializationService to run fresh discovery
            // and create a clean record on next startup
            Execute.Sql(@"
                DELETE FROM Setting WHERE Name = 'IntelliJPath';
            ");

            // Note: We don't create a new record here because:
            // 1. The discovery process in ApplicationInitializationService will handle it
            // 2. This ensures the path is always current and valid
            // 3. Avoids hardcoding paths or making assumptions about installation locations
        }

        public override void Down()
        {
            // Nothing to do - if we rollback, the discovery will run again anyway
            // We don't restore old potentially corrupted data
        }
    }
}
