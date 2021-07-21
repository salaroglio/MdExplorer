using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Migrations.Version000001
{
    [Migration(20210712001, "Insert new setting: EditorPath")]
    public class M2021_07_12_001 : Migration
    {
        public override void Up()
        {
            Insert.IntoTable("Setting").Row(new
            {
                Id = Guid.NewGuid().ToByteArray(),
                Name = @"EditorPath",
                ValueString = @"C:\Users\Carlo Salaroglio\AppData\Local\Programs\Microsoft VS Code\Code.exe"
            });
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }

       
    }
}
