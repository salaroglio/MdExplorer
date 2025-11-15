using FluentMigrator;
using System;

namespace MdExplorer.Migrations.Version202511
{
    [Migration(20251115001, "Add SelectedIde column to Project table for storing preferred IDE per project")]
    public class M2025_11_15_001 : Migration
    {
        public override void Up()
        {
            // Add SelectedIde column to Project table
            // Nullable to allow existing projects to default to null (will use "vscode" as default in code)
            Alter.Table("Project")
                .AddColumn("SelectedIde").AsString(50).Nullable();
        }

        public override void Down()
        {
            // Remove SelectedIde column if rolling back
            Delete.Column("SelectedIde").FromTable("Project");
        }
    }
}
