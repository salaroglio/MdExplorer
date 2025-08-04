using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version000001
{
    [Migration(20210709002, "Insert new setting: JavaPath and LocalGraphVizDotPath")]
    public class M2021_07_09_002 : Migration
    {
        public override void Up()
        {
            Insert.IntoTable("Setting").Row(new
            {
                Id = Guid.NewGuid().ToByteArray(),
                Name = @"JavaPath",
                ValueString = (string)null  // Will trigger auto-discovery
            });
            Insert.IntoTable("Setting").Row(new
            {
                Id = Guid.NewGuid().ToByteArray(),
                Name = @"LocalGraphvizDotPath",
                ValueString = (string)null  // Should be configured or discovered as needed
            });
        }


        public override void Down()
        {
            throw new NotImplementedException();
        }

        
    }
}
