using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;

namespace Ad.Tools.Dal.UnitTest.Migrations
{
    [Migration(0, @"Creazione/Inizializzazione database")]
    public class Migration0 : Migration
    {
        public override void Up()
        {
            CreateUser();
            CreateInvoice();
            CreateBulk();
        }

        private void CreateBulk()
        {
            Create.Table("Bulk")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("Call").AsInt32().Nullable()
                .WithColumn("StartDate").AsDateTime().Nullable()
                .WithColumn("EndDate").AsDateTime().Nullable()
                .WithColumn("CurrentCall").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("CurrentCallInError").AsInt32().Nullable()
                .WithColumn("UserInfoId").AsGuid().NotNullable();
            Create.ForeignKey("FK_Bulk_UserInfo").FromTable("Bulk").ForeignColumn("UserInfoId").ToTable("UserInfo").PrimaryColumn("Id");
        }

        private void CreateInvoice()
        {
            Create.Table("Invoice")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("IP").AsString(39).NotNullable()
                .WithColumn("CustomerName").AsString(255).Nullable()
                .WithColumn("UserInfoId").AsGuid().NotNullable()
                .WithColumn("StartDate").AsDateTime().Nullable()
                .WithColumn("EndDate").AsDateTime().Nullable()
                .WithColumn("FileName").AsString(1000).NotNullable()
                .WithColumn("Message").AsString(1000).Nullable();

            Create.ForeignKey("FK_Invoice_UserInfo").FromTable("Invoice").ForeignColumn("UserInfoId").ToTable("UserInfo").PrimaryColumn("Id");
        }

        private void CreateUser()
        {
            Create.Table("UserInfo")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("UserName").AsString(255).NotNullable()
                .WithColumn("Password").AsString(255).NotNullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
        
    }
}
