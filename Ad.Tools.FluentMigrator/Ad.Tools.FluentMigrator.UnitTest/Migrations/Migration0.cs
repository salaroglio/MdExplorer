using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.FluentMigrator.UnitTest.Migrations
{
    [Migration(0, "Creazione database unit test")]
    public class Migration0 : Migration
    {
        public override void Up()
        {
            CreateTemplate();
            CreateGraphic();
            CreateMatch();
            CreateRendering();
        }

        private void CreateRendering()
        {
            Create.Table("Rendering")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("GraphicId").AsGuid().NotNullable()
                .WithColumn("TextMatching").AsString().NotNullable()
                .WithColumn("TextToReplace").AsString().Nullable()
                .WithColumn("ActionType").AsString().NotNullable()
                .WithColumn("ImageAction").AsString().Nullable()
                .WithColumn("ImagePath").AsString().Nullable();

            Create.ForeignKey("FK_Rendering_Graphic").FromTable("Rendering").ForeignColumn("GraphicId").ToTable("Graphic").PrimaryColumn("Id");

        }

        private void CreateMatch()
        {
            Create.Table("Match")
                .WithColumn("Id").AsGuid().PrimaryKey()
                .WithColumn("TemplateId").AsGuid()
                .WithColumn("SequenceCheck").AsInt32().NotNullable()
                .WithColumn("CodModello").AsInt32().NotNullable()
                .WithColumn("Text").AsString(int.MaxValue).NotNullable()
                .WithColumn("Col").AsInt32().NotNullable()
                .WithColumn("Row").AsInt32().NotNullable()
                .WithColumn("SelLength").AsInt32().NotNullable();

            Create.ForeignKey("FK_Match_Template").FromTable("Match").ForeignColumn("TemplateId").ToTable("Template").PrimaryColumn("Id");

        }

        private void CreateGraphic()
        {

            Create.Table("Graphic")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("TemplateId").AsGuid().NotNullable()
            .WithColumn("CodModello").AsInt32().NotNullable()
            .WithColumn("Tipo").AsString(10).Nullable()
            .WithColumn("Top").AsDecimal().Nullable()
            .WithColumn("Left").AsDecimal().Nullable()
            .WithColumn("Width").AsDecimal().Nullable()
            .WithColumn("Height").AsDecimal().Nullable()
            .WithColumn("X1").AsDecimal().Nullable()
            .WithColumn("Y1").AsDecimal().Nullable()
            .WithColumn("X2").AsDecimal().Nullable()
            .WithColumn("Y2").AsDecimal().Nullable()
            .WithColumn("BackColor").AsString(20).Nullable()
            .WithColumn("ForeColor").AsString(20).Nullable()
            .WithColumn("FillColor").AsString(20).Nullable()
            .WithColumn("BorderColor").AsString(20).Nullable()
            .WithColumn("Picture").AsString(int.MaxValue).Nullable()
            .WithColumn("Text").AsString(int.MaxValue).Nullable()
            .WithColumn("FontName").AsString(50).Nullable()
            .WithColumn("BorderWidth").AsInt32().Nullable()
            .WithColumn("FillStyle").AsInt32().Nullable()
            .WithColumn("BorderStyle").AsInt32().Nullable()
            .WithColumn("Shape").AsInt32().Nullable()
            .WithColumn("Barcode").AsBoolean().Nullable()
            .WithColumn("Shadow").AsBoolean().Nullable()
            .WithColumn("FontSize").AsByte().Nullable()
            .WithColumn("FontBold").AsBoolean().Nullable()
            // seconda parte
            .WithColumn("FontItalic").AsBoolean().Nullable()
            .WithColumn("FontUnderLine").AsBoolean().Nullable()
            .WithColumn("Alignment").AsInt32().Nullable()
            .WithColumn("Col").AsInt32().Nullable()
            .WithColumn("Row").AsInt32().Nullable()
            .WithColumn("Sel").AsInt32().Nullable()
            .WithColumn("StartColor").AsInt32().Nullable()
            .WithColumn("EndColor").AsInt32().Nullable()
            .WithColumn("Gradient").AsBoolean().Nullable()
            .WithColumn("TipoGradient").AsByte().Nullable()
            .WithColumn("RotazioneGradient").AsInt32().Nullable()
            .WithColumn("DPIX").AsInt32().Nullable()
            .WithColumn("DPIY").AsInt32().Nullable()
            .WithColumn("TipoBarCode").AsInt32().Nullable()
            .WithColumn("MainPosition").AsByte().Nullable()
            .WithColumn("AddPosition").AsByte().Nullable()
            .WithColumn("BarCodeRotation").AsInt32().Nullable()
            .WithColumn("TextAdd").AsString().Nullable()
            .WithColumn("ZOrder").AsBoolean().Nullable()
            .WithColumn("CodOggetto").AsInt32().Nullable()
            .WithColumn("CodAssocia").AsInt32().Nullable()
            .WithColumn("Associa").AsInt32().Nullable()
            .WithColumn("Sfondo").AsBoolean().Nullable()
            .WithColumn("ObjRotation").AsInt32().Nullable()
            .WithColumn("CityPost").AsBoolean().Nullable()
            .WithColumn("NomeGruppo").AsString().Nullable()
            .WithColumn("NColonna").AsInt32().Nullable()
            .WithColumn("NoCheckSum").AsBoolean().Nullable()
            .WithColumn("MantieniProporzioni").AsBoolean().Nullable()
            .WithColumn("StampaCorrTab").AsBoolean().Nullable()
            .WithColumn("FlAddTxtDavanti").AsBoolean().Nullable();

            Create.ForeignKey("FK_Graphic_Template").FromTable("Graphic").ForeignColumn("TemplateId").ToTable("Template").PrimaryColumn("Id");

        }


        private void CreateTemplate()
        {

            Create.Table("Template")
            .WithColumn("Id").AsGuid().PrimaryKey()
            .WithColumn("CodModello").AsInt32().NotNullable()
            .WithColumn("DescrModello").AsString(50).Nullable()
            .WithColumn("Orientamento").AsByte().Nullable()
            .WithColumn("DPIX").AsInt32().Nullable()
            .WithColumn("DPIY").AsInt32().Nullable()
            .WithColumn("CodMaster").AsInt32().Nullable()
            .WithColumn("TipoCover").AsBoolean().Nullable()
            .WithColumn("NCover").AsInt32().Nullable()
            .WithColumn("Fascicola").AsBoolean().Nullable()
            .WithColumn("Disattivo").AsBoolean().Nullable()
            .WithColumn("Note").AsString(255).Nullable()
            .WithColumn("CityPost").AsBoolean().Nullable()
            .WithColumn("ModelloCopia").AsBoolean().Nullable()
            .WithColumn("HFoglio").AsDecimal().Nullable()
            .WithColumn("VFoglio").AsDecimal().Nullable()
            .WithColumn("DataUltUso").AsDateTime().Nullable()
            .WithColumn("VerificaDuplicati").AsBoolean().Nullable()
            .WithColumn("InManutenzione").AsBoolean().Nullable();

        }

        public override void Down()
        {
            throw new NotImplementedException();
        }


    }
}
