using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Generators.SQLite;
using Microsoft.Extensions.Options;

namespace Ad.Tools.FluentMigrator
{
    /// <summary>
    /// Il generatore SQLite come lo faceva FluentMigrator 3.2.17, con cui sono nati tutti i database degli utenti di oggi.
    /// La 8.0.1 cambia due cose che darebbero ai database creati da zero uno schema DIVERSO da quello degli altri (le
    /// migrazioni già eseguite non ripartono): parte in STRICT invece che in LOOSE (un'istruzione che SQLite non sa fare
    /// lancia invece di essere saltata) e traduce <c>Delete.Column</c> in <c>ALTER TABLE … DROP COLUMN</c>, mentre la
    /// 3.2.17 non scriveva niente (le colonne «cancellate» sono rimaste in tutti i database: vedi le migrazioni).
    /// Sprint .NET 10, L1d (21/11/2026): l'SQL delle 125 migrazioni è stato confrontato riga per riga con la 3.2.17.
    /// ⚠️ Per togliere davvero una colonna serve una migrazione che ricostruisce la tabella con <c>Execute.Sql</c>.
    /// </summary>
    public sealed class LegacySQLiteGenerator : SQLiteGenerator
    {
        public LegacySQLiteGenerator(SQLiteQuoter quoter, ISQLiteTypeMap typeMap, IOptions<GeneratorOptions> options)
            : base(quoter, typeMap, options)
        {
            CompatibilityMode = CompatibilityMode.LOOSE;
        }

        /// <summary>La 3.2.17 non scriveva niente: le colonne «cancellate» sono rimaste in tutti i database.</summary>
        public override string Generate(DeleteColumnExpression expression) => string.Empty;

        /// <summary>La 3.2.17 non scriveva i vincoli UNIQUE con nome (<c>Create.UniqueConstraint</c>): nei database non ci sono.</summary>
        public override string Generate(CreateConstraintExpression expression) =>
            expression.Constraint.IsUniqueConstraint ? string.Empty : base.Generate(expression);

        /// <summary>
        /// Con <c>.Unique()</c> la 3.2.17 creava solo l'indice unico a parte (lo genera la migrazione stessa); la 8.0.1 scrive
        /// in più UNIQUE sulla colonna, cioè un secondo indice automatico che nei database di oggi non c'è.
        /// </summary>
        public override string Generate(CreateTableExpression expression) =>
            WithoutInline(expression.Columns, false, () => base.Generate(expression));

        /// <summary>Una colonna aggiunta a una tabella esistente: la 3.2.17 non scriveva né UNIQUE né PRIMARY KEY.</summary>
        public override string Generate(CreateColumnExpression expression) =>
            WithoutInline(new[] { expression.Column }, true, () => base.Generate(expression));

        private static string WithoutInline(System.Collections.Generic.IEnumerable<global::FluentMigrator.Model.ColumnDefinition> columns, bool primaryKeyToo, System.Func<string> generate)
        {
            var saved = new System.Collections.Generic.List<(global::FluentMigrator.Model.ColumnDefinition Column, bool Unique, bool PrimaryKey)>();
            foreach (var c in columns) saved.Add((c, c.IsUnique, c.IsPrimaryKey));
            try
            {
                foreach (var (c, _, _) in saved) { c.IsUnique = false; if (primaryKeyToo) c.IsPrimaryKey = false; }
                return generate();
            }
            finally
            {
                foreach (var (c, unique, primaryKey) in saved) { c.IsUnique = unique; c.IsPrimaryKey = primaryKey; }
            }
        }
    }
}
