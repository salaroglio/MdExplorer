using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
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
        private static IServiceCollection _services;
        private static string _connectionString;
        private static System.Reflection.Assembly[] _assemblies;


        public static IServiceCollection AddFluentMigratorFeatures
            (this IServiceCollection services,
            string connectionString,
            Action<IMigrationRunnerBuilder> currentConfiguration,
            params System.Reflection.Assembly[] assemblies)
        {
            _assemblies = assemblies;
            _connectionString = connectionString;
            _services = services;

            if (services == null)
            {
                throw new Exception("services param has to be initialized");
            }           
            services.AddFluentMigratorCore()
                           .ConfigureRunner(currentConfiguration)
                           .AddLogging(lb => lb.AddFluentMigratorConsole());
            services.AddSingleton<IEngineMigrator,EngineMigrator>();

            return services;
        }

        public class DatabaseConfigurations
        {
            public static void ConfigureSQLServer(IMigrationRunnerBuilder rb)
            {
                rb.AddSqlServer().WithGlobalConnectionString(_connectionString).ScanIn(_assemblies).For.Migrations();
            }
            public static void ConfigureSQLite(IMigrationRunnerBuilder rb)
            {
                rb.AddSQLite().WithGlobalConnectionString(_connectionString).ScanIn(_assemblies).For.Migrations();
            }
            public static void ConfigureMariaDB(IMigrationRunnerBuilder rb)
            {
                rb.AddMySql5().WithGlobalConnectionString(_connectionString).ScanIn(_assemblies).For.Migrations();
            }

        }



    }


    
}
