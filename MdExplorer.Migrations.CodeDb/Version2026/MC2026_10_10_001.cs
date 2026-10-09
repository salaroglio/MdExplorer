using FluentMigrator;

namespace MdExplorer.Migrations.CodeDb.Version2026
{
    /// <summary>
    /// Il grafo del codice, schema 1: file, simboli, parametri, archi, le descrizioni scritte dalla skill,
    /// lo stato degli script caricati e dello scanner, e la FTS5 trigram sui simboli.
    /// <para>
    /// Le descrizioni (<c>code_symbol_doc</c>, <c>code_parameter_doc</c>) stanno in tabelle loro, agganciate al
    /// simbolo per id stabile: ricaricare lo script AST di un file non le tocca (D11).
    /// </para>
    /// <para>
    /// La virtual table FTS5 non si può esprimere col fluent API: è SQL diretto, e richiede che il runner giri su
    /// Microsoft.Data.Sqlite (System.Data.SQLite 1.0.113.1 non ha FTS5 — provato il 10/10/2026).
    /// </para>
    /// </summary>
    [Migration(20261010001, "Code graph schema 1: files, symbols, parameters, edges, docs, script/scan state, FTS5")]
    public class MC2026_10_10_001 : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("code_file").Exists())
            {
                Create.Table("code_file")
                    .WithColumn("Path").AsString(int.MaxValue).NotNullable().PrimaryKey()
                    .WithColumn("Language").AsString(64).NotNullable()
                    .WithColumn("ContentHash").AsString(64).NotNullable()
                    .WithColumn("Extractor").AsString(64).NotNullable()
                    .WithColumn("ExtractorVersion").AsString(32).NotNullable()
                    .WithColumn("ParsedAt").AsString(40).NotNullable()
                    .WithColumn("SymbolCount").AsInt32().NotNullable().WithDefaultValue(0)
                    .WithColumn("ScriptPath").AsString(int.MaxValue).NotNullable();
            }

            if (!Schema.Table("code_symbol").Exists())
            {
                Create.Table("code_symbol")
                    .WithColumn("Id").AsString(64).NotNullable().PrimaryKey()
                    .WithColumn("Kind").AsString(32).NotNullable()
                    .WithColumn("Name").AsString(int.MaxValue).NotNullable()
                    .WithColumn("QualifiedName").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Path").AsString(int.MaxValue).Nullable()          // null = external (fuori perimetro)
                    .WithColumn("LineStart").AsInt32().Nullable()
                    .WithColumn("LineEnd").AsInt32().Nullable()
                    .WithColumn("Signature").AsString(int.MaxValue).Nullable()
                    .WithColumn("ReturnType").AsString(int.MaxValue).Nullable()
                    .WithColumn("Visibility").AsString(32).Nullable()
                    .WithColumn("IsStatic").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("IsAsync").AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn("ParentId").AsString(64).Nullable()
                    .WithColumn("Language").AsString(64).Nullable();

                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_symbol_Path ON code_symbol(Path);");
                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_symbol_Name ON code_symbol(Name);");
                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_symbol_QualifiedName ON code_symbol(QualifiedName);");
                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_symbol_ParentId ON code_symbol(ParentId);");
                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_symbol_Kind ON code_symbol(Kind);");
            }

            if (!Schema.Table("code_parameter").Exists())
            {
                Create.Table("code_parameter")
                    .WithColumn("SymbolId").AsString(64).NotNullable()
                    .WithColumn("Position").AsInt32().NotNullable()
                    .WithColumn("Name").AsString(int.MaxValue).NotNullable()
                    .WithColumn("ParameterType").AsString(int.MaxValue).Nullable()
                    .WithColumn("Direction").AsString(16).NotNullable().WithDefaultValue("in")
                    .WithColumn("DefaultValue").AsString(int.MaxValue).Nullable()
                    .WithColumn("IsNullable").AsBoolean().NotNullable().WithDefaultValue(false);

                Execute.Sql("CREATE UNIQUE INDEX IF NOT EXISTS UX_code_parameter_Symbol_Position ON code_parameter(SymbolId, Position);");
            }

            if (!Schema.Table("code_symbol_doc").Exists())
            {
                Create.Table("code_symbol_doc")
                    .WithColumn("SymbolId").AsString(64).NotNullable().PrimaryKey()
                    .WithColumn("Summary").AsString(int.MaxValue).Nullable()
                    .WithColumn("ReturnsDescription").AsString(int.MaxValue).Nullable()
                    .WithColumn("Remarks").AsString(int.MaxValue).Nullable()
                    .WithColumn("WrittenBy").AsString(int.MaxValue).NotNullable()
                    .WithColumn("WrittenAt").AsString(40).NotNullable()
                    .WithColumn("SignatureHash").AsString(64).Nullable()
                    .WithColumn("ScriptPath").AsString(int.MaxValue).NotNullable();

                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_symbol_doc_ScriptPath ON code_symbol_doc(ScriptPath);");
            }

            if (!Schema.Table("code_parameter_doc").Exists())
            {
                Create.Table("code_parameter_doc")
                    .WithColumn("SymbolId").AsString(64).NotNullable()
                    .WithColumn("ParameterName").AsString(int.MaxValue).NotNullable()
                    .WithColumn("Description").AsString(int.MaxValue).NotNullable()
                    .WithColumn("ScriptPath").AsString(int.MaxValue).NotNullable();

                Execute.Sql("CREATE UNIQUE INDEX IF NOT EXISTS UX_code_parameter_doc_Symbol_Name ON code_parameter_doc(SymbolId, ParameterName);");
                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_parameter_doc_ScriptPath ON code_parameter_doc(ScriptPath);");
            }

            if (!Schema.Table("code_edge").Exists())
            {
                Create.Table("code_edge")
                    .WithColumn("Id").AsInt64().NotNullable().PrimaryKey().Identity()
                    .WithColumn("SourceId").AsString(64).NotNullable()
                    .WithColumn("TargetId").AsString(64).NotNullable()
                    .WithColumn("Relation").AsString(32).NotNullable()
                    .WithColumn("Confidence").AsString(16).NotNullable()
                    .WithColumn("Path").AsString(int.MaxValue).NotNullable()      // il file sorgente da cui nasce l'arco
                    .WithColumn("Line").AsInt32().Nullable();

                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_edge_SourceId ON code_edge(SourceId);");
                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_edge_TargetId ON code_edge(TargetId);");
                Execute.Sql("CREATE INDEX IF NOT EXISTS IX_code_edge_Path ON code_edge(Path);");
            }

            if (!Schema.Table("code_script_state").Exists())
            {
                Create.Table("code_script_state")
                    .WithColumn("ScriptPath").AsString(int.MaxValue).NotNullable().PrimaryKey()
                    .WithColumn("Kind").AsString(16).NotNullable()
                    .WithColumn("ContentHash").AsString(64).NotNullable()
                    .WithColumn("SchemaVersion").AsInt32().NotNullable()
                    .WithColumn("LoadedAt").AsString(40).NotNullable()
                    .WithColumn("RecordCount").AsInt32().NotNullable().WithDefaultValue(0);
            }

            if (!Schema.Table("code_scan_state").Exists())
            {
                Create.Table("code_scan_state")
                    .WithColumn("ProgramFolder").AsString(int.MaxValue).NotNullable().PrimaryKey()
                    .WithColumn("RepositoryRoot").AsString(int.MaxValue).Nullable()
                    .WithColumn("AnalyzedCommit").AsString(64).Nullable()
                    .WithColumn("Mode").AsString(16).NotNullable()
                    .WithColumn("ScannedAt").AsString(40).NotNullable()
                    .WithColumn("Candidates").AsInt32().NotNullable().WithDefaultValue(0)
                    .WithColumn("Reparsed").AsInt32().NotNullable().WithDefaultValue(0);
            }

            // FTS5 trigram: nomi E descrizioni, così «scrive su Neo4j» trova i metodi la cui descrizione lo dice.
            Execute.Sql(
                "CREATE VIRTUAL TABLE IF NOT EXISTS code_symbol_fts USING fts5(" +
                "name, qualified_name, summary, parameter_descriptions, symbol_id UNINDEXED, tokenize='trigram');");
        }

        public override void Down()
        {
            Execute.Sql("DROP TABLE IF EXISTS code_symbol_fts;");
            foreach (var table in new[] { "code_scan_state", "code_script_state", "code_edge", "code_parameter_doc",
                                          "code_symbol_doc", "code_parameter", "code_symbol", "code_file" })
            {
                if (Schema.Table(table).Exists()) Delete.Table(table);
            }
        }
    }
}
