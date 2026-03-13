using FluentMigrator;

namespace MdExplorer.Migrations.Version202603
{
    [Migration(20260312001, "Fix AppStoreRepository GUIDs: convert TEXT to BLOB format for NHibernate compatibility")]
    public class M2026_03_12_001 : Migration
    {
        public override void Up()
        {
            // AppStoreRepository IDs were seeded as TEXT strings (hex(randomblob(...))).
            // NHibernate expects GUIDs as 16-byte BLOBs (randomblob(16)).
            // Recreate rows with BLOB IDs preserving all other data.
            Execute.Sql(@"
                CREATE TABLE AppStoreRepository_tmp (
                    Id BLOB PRIMARY KEY,
                    Label TEXT NOT NULL,
                    Url TEXT NOT NULL,
                    Username TEXT,
                    Password TEXT,
                    SortOrder INTEGER NOT NULL DEFAULT 0
                );

                INSERT INTO AppStoreRepository_tmp (Id, Label, Url, Username, Password, SortOrder)
                SELECT randomblob(16), Label, Url, Username, Password, SortOrder
                FROM AppStoreRepository;

                DROP TABLE AppStoreRepository;

                ALTER TABLE AppStoreRepository_tmp RENAME TO AppStoreRepository;
            ");
        }

        public override void Down()
        {
            // No rollback needed — NHibernate works with BLOB GUIDs natively
        }
    }
}
