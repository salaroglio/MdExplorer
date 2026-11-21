using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Generators.SQLite;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.FluentMigrator
{
    public static class FluentMigratorDI
    {
        //private static IServiceCollection _services;
        //private static string _connectionString;
        //private static System.Reflection.Assembly[] _assemblies;


        public static IServiceCollection AddFluentMigratorFeatures
            (this IServiceCollection services,            
            Action<IMigrationRunnerBuilder> currentConfiguration,
            string processorId            
            )
        {
            //_assemblies = assemblies;
            //_connectionString = connectionString;
            //_services = services;

            if (services == null)
            {
                throw new Exception("services param has to be initialized");
            }           
            services.AddFluentMigratorCore()
                           .ConfigureRunner(currentConfiguration)
                           .Configure<SelectingProcessorAccessorOptions>((_) => {                                
                               _.ProcessorId = processorId; })
                           .AddLogging(lb => lb.AddFluentMigratorConsole());
            services.AddScoped<IEngineMigrator,EngineMigrator>();
            if (string.Equals(processorId, "SQLite", StringComparison.OrdinalIgnoreCase))
            {
                // dopo AddSQLite (dentro ConfigureRunner): l'ultima registrazione vince, il processore riceve questa
                services.AddScoped<SQLiteDbFactory, SystemDataSQLiteDbFactory>();
                // il generatore come quello della 3.2.17 (LegacySQLiteGenerator); stesso quoter e stessa mappa dei tipi di AddSQLite
                var registered = services.LastOrDefault(d => d.ServiceType == typeof(SQLiteGenerator))?.ImplementationFactory
                    ?? throw new InvalidOperationException("AddFluentMigratorFeatures(\"SQLite\"): la configurazione deve chiamare rb.AddSQLite()");
                // la stringa di connessione con l'attesa sul database occupato (BusyTimeoutConnectionStringAccessor)
                services.AddScoped<IConnectionStringAccessor>(sp => new BusyTimeoutConnectionStringAccessor(
                    ActivatorUtilities.CreateInstance<ConnectionStringAccessor>(sp)));
                services.AddScoped<SQLiteGenerator>(sp => new LegacySQLiteGenerator(
                    (SQLiteQuoter)((SQLiteGenerator)registered(sp)).Quoter,
                    sp.GetRequiredService<ISQLiteTypeMap>(),
                    sp.GetRequiredService<IOptions<GeneratorOptions>>()));
            }

            return services;
        }

        //public class DatabaseConfigurations
        //{
        //    public static void ConfigureSQLServer(IMigrationRunnerBuilder rb)
        //    {
        //        rb.AddSqlServer().WithGlobalConnectionString(_connectionString).ScanIn(_assemblies).For.Migrations();
        //    }
        //    public static void ConfigureSQLite(IMigrationRunnerBuilder rb)
        //    {                
        //        rb.AddSQLite().WithGlobalConnectionString(_connectionString).ScanIn(_assemblies).For.Migrations();
        //    }
        //    public static void ConfigureMariaDB(IMigrationRunnerBuilder rb)
        //    {
        //        rb.AddMySql5().WithGlobalConnectionString(_connectionString).ScanIn(_assemblies).For.Migrations();
        //    }

        //}



    }


    
}
