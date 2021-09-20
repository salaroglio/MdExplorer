using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.ProjectDb.Version202109
{
    [Migration(20210920001, "Start new database MDdEplorerProject")]
    public class MP2021_09_20_001 : Migration
    {
        public override void Up()
        {
            Create.Table("SemanticCluster")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable();

            Create.Table("SemanticClusterElement")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Description").AsString(int.MaxValue).Nullable()
                .WithColumn("SemanticClusterId").AsGuid().NotNullable();


            Create.ForeignKey("SemanticClusterElement_SemanticCluster_SemanticClusterId")
            .FromTable("SemanticClusterElement").ForeignColumn("SemanticClusterId").ToTable("SemanticCluster")
            .PrimaryColumn("Id");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
