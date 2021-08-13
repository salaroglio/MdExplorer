using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.EngineDb.Version202107
{
    [Migration(20210812001, "Source field Added to LinkInsideMarkdown")]
    public class ME2021_08_12_001 : Migration
    {
        public override void Up()
        {
            Alter.Table("LinkInsideMarkdown").AddColumn("Source").AsString(255).NotNullable().WithDefaultValue("Markdown");
            Alter.Table("LinkInsideMarkdown").AddColumn("SectionIndex").AsInt16().Nullable();
            Alter.Table("LinkInsideMarkdown").AddColumn("LinkedCommand").AsString(512).Nullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
