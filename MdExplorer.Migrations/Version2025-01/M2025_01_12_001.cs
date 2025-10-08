using FluentMigrator;
using System;

namespace MdExplorer.Migrations.Version202501
{
    [Migration(20250112001, "Fix Graphviz path from GraphWiz to Graphviz")]
    public class M2025_01_12_001 : Migration
    {
        public override void Up()
        {
            // Corregge il percorso errato GraphWiz -> Graphviz
            Update.Table("Setting")
                .Set(new { ValueString = @"Binaries\Graphviz\windows\dot.exe" })
                .Where(new { Name = "LocalGraphvizDotPath" });
        }

        public override void Down()
        {
            // Ripristina il percorso precedente se necessario
            Update.Table("Setting")
                .Set(new { ValueString = @"Binaries\GraphWiz\binaries\dot.exe" })
                .Where(new { Name = "LocalGraphvizDotPath" });
        }
    }
}