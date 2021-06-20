using FluentMigrator;
using System;

namespace MdExplorer.Migrations
{
    [Migration(0, "Creazione database")]
    public class Migration0 : Migration
    {
        public override void Up()
        {
            Create.Table("Setting")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Name").AsString().NotNullable().Unique()
                .WithColumn("ValueString").AsString().Nullable()
                .WithColumn("ValueInt").AsInt32().Nullable()
                .WithColumn("ValueDateTime").AsDateTime().Nullable()
                .WithColumn("ValueDecimal").AsDecimal().Nullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }        
    }
}
