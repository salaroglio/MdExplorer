using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using System;
using System.Reflection; // Required for Assembly scanning

namespace Ad.Tools.Dal.Evo
{
    /// <summary>
    /// Configures NHibernate and builds the ISessionFactory.
    /// </summary>
    public static class NHibernateConfigurator
    {
        /// <summary>
        /// Builds the NHibernate ISessionFactory based on the provided configuration.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="mappingAssembly">The assembly containing FluentNHibernate mappings.</param>
        /// <param name="updateSchema">Flag indicating whether to update the database schema.</param>
        /// <returns>A configured ISessionFactory.</returns>
        /// <exception cref="ArgumentException">Thrown if connectionString is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if mappingAssembly is null.</exception>
        public static ISessionFactory BuildSessionFactory(string connectionString, Assembly mappingAssembly, bool updateSchema = false)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }
            if (mappingAssembly == null)
            {
                throw new ArgumentNullException(nameof(mappingAssembly), "Mapping assembly cannot be null.");
            }

            // Configure NHibernate using FluentNHibernate
            return Fluently.Configure()
                // Database Configuration (Example: SQL Server)
                // Choose the appropriate database configuration (MsSqlConfiguration, PostgreSQLConfiguration, SQLiteConfiguration, etc.)
                .Database(MsSqlConfiguration.MsSql2012
                    .ConnectionString(connectionString)
                    .ShowSql() // Optional: Log SQL to console
                    .FormatSql() // Optional: Format logged SQL
                 )

                // Mappings Configuration
                .Mappings(m => m.FluentMappings
                    .AddFromAssembly(mappingAssembly) // Scan the specified assembly for Fluent mappings
                    // .ExportTo("./mappings") // Optional: Export mapping files
                 )

                // Schema Update (Optional - Use with caution in production)
                .ExposeConfiguration(cfg =>
                {
                    if (updateSchema)
                    {
                        // This will update the schema based on mappings.
                        // Good for development, potentially dangerous for production.
                        new SchemaUpdate(cfg).Execute(true, true);
                    }
                    // You can add other NHibernate configuration options here, e.g.:
                    // cfg.SetProperty(NHibernate.Cfg.Environment.CommandTimeout, "60");
                    // cfg.SetProperty(NHibernate.Cfg.Environment.BatchSize, "100");
                })

                // Build the Session Factory
                .BuildSessionFactory();
        }

        // --- Overload for SQLite (Example) ---
        /// <summary>
        /// Builds the NHibernate ISessionFactory for SQLite.
        /// </summary>
        /// <param name="dbFilePath">The path to the SQLite database file.</param>
        /// <param name="mappingAssembly">The assembly containing FluentNHibernate mappings.</param>
        /// <param name="updateSchema">Flag indicating whether to create/update the database schema.</param>
        /// <returns>A configured ISessionFactory.</returns>
        public static ISessionFactory BuildSqliteSessionFactory(string dbFilePath, Assembly mappingAssembly, bool updateSchema = true) // Default updateSchema to true for SQLite often
        {
             if (string.IsNullOrEmpty(dbFilePath))
            {
                throw new ArgumentException("Database file path cannot be null or empty.", nameof(dbFilePath));
            }
            if (mappingAssembly == null)
            {
                throw new ArgumentNullException(nameof(mappingAssembly), "Mapping assembly cannot be null.");
            }

            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard
                    .UsingFile(dbFilePath)
                    .ShowSql()
                    .FormatSql())
                .Mappings(m => m.FluentMappings.AddFromAssembly(mappingAssembly))
                .ExposeConfiguration(cfg =>
                {
                    if (updateSchema)
                    {
                        // For SQLite, SchemaUpdate often creates the schema if it doesn't exist.
                        new SchemaUpdate(cfg).Execute(true, true);
                    }
                })
                .BuildSessionFactory();
        }
    }
}
