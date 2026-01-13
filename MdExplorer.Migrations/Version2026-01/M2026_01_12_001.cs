using FluentMigrator;

namespace MdExplorer.Migrations.Version202601
{
    [Migration(20260112001, "Normalize GitCredential AccountName to standard pattern")]
    public class M2026_01_12_001 : Migration
    {
        public override void Up()
        {
            // Normalize all AccountName values to the standard pattern: "{AccountType} - {AuthUsername}"
            // This fixes:
            // 1. "Auto-detected (username)" -> "GitHub - username" (or appropriate type)
            // 2. "Generic - anagrafica_reale" -> "Generic - csalaroglio" (name was repo name, not username)
            // 3. Any other inconsistent patterns
            Execute.Sql(@"
                UPDATE GitCredential
                SET AccountName = AccountType || ' - ' || AuthUsername
                WHERE AuthUsername IS NOT NULL
                  AND AuthUsername != ''
                  AND AccountName != AccountType || ' - ' || AuthUsername;
            ");
        }

        public override void Down()
        {
            // Cannot reliably rollback - we don't know the original AccountName values
            // The data correction is intentional and the old values were inconsistent
        }
    }
}
