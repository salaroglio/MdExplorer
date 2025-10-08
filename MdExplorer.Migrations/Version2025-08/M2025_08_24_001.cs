using FluentMigrator;
using System;

namespace MdExplorer.Migrations.Version202508
{
    [Migration(20250824001, "Create TocDescriptionCache table for AI-generated TOC descriptions")]
    public class M2025_08_24_001 : Migration
    {
        public override void Up()
        {
            Create.Table("TocDescriptionCache")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("FilePath").AsString(500).NotNullable()
                .WithColumn("FileHash").AsString(32).NotNullable()
                .WithColumn("Description").AsString(1000).NotNullable()
                .WithColumn("GeneratedAt").AsDateTime().NotNullable()
                .WithColumn("ModelUsed").AsString(100).Nullable()
                .WithColumn("ProjectId").AsInt32().Nullable();
            
            Create.Index("IX_TocCache_Path_Hash")
                .OnTable("TocDescriptionCache")
                .OnColumn("FilePath").Ascending()
                .OnColumn("FileHash").Ascending();
            
            // Insert default TOC generation settings
            Insert.IntoTable("Setting").Row(new 
            { 
                Id = Guid.NewGuid(),
                Name = "TOC_Generation_Prompt", 
                ValueString = "Analizza questo documento markdown e genera una descrizione dettagliata. Focus su: scopo principale, contenuti chiave, target audience, informazioni rilevanti.",
                ValueInt = 0
            });
            
            Insert.IntoTable("Setting").Row(new 
            { 
                Id = Guid.NewGuid(),
                Name = "TOC_Generation_EnableCache", 
                ValueString = "",
                ValueInt = 1  // 1 = true for cache enabled
            });
            
            Insert.IntoTable("Setting").Row(new 
            { 
                Id = Guid.NewGuid(),
                Name = "TOC_Generation_BatchSize", 
                ValueString = "",
                ValueInt = 10
            });
        }

        public override void Down()
        {
            Delete.FromTable("Setting").Row(new { Name = "TOC_Generation_Prompt" });
            Delete.FromTable("Setting").Row(new { Name = "TOC_Generation_EnableCache" });
            Delete.FromTable("Setting").Row(new { Name = "TOC_Generation_BatchSize" });
            
            Delete.Index("IX_TocCache_Path_Hash").OnTable("TocDescriptionCache");
            Delete.Table("TocDescriptionCache");
        }
    }
}