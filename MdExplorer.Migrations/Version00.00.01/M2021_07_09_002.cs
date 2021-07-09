using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version000001
{
    [Migration(20210709002, "Insert new settings")]
    public class M2021_07_09_002 : Migration
    {
        public override void Up()
        {
            Insert.IntoTable("Setting").Row(new
            {
                Id = Guid.NewGuid().ToByteArray(),
                Name = @"JavaPath",
                ValueString = @"C:\Program Files\Java\jre1.8.0_291\bin\javaw.exe"
            });
            Insert.IntoTable("Setting").Row(new
            {
                Id = Guid.NewGuid().ToByteArray(),
                Name = @"LocalGraphvizDotPath",
                ValueString = @"E:\Sviluppo\MdExplorer\InstallBinaries\Graphviz\bin\dot.exe"
            });
        }


        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
