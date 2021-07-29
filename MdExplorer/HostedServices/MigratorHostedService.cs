using Ad.Tools.FluentMigrator;
using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
using MdExplorer.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Ad.Tools.FluentMigrator.FluentMigratorDI;

namespace MdExplorer.Service.HostedServices
{
    public class MigratorHostedService : IHostedService
    {
        private readonly IEngineMigrator _migrator;
        private readonly IServiceCollection _services;

        public MigratorHostedService(
            ILogger<MigratorHostedService> logger)
        {

            _services = new ServiceCollection();
            var appdata = Environment.GetEnvironmentVariable("LocalAppData");
            logger.LogInformation($@"Upgrade database in: {appdata}");
            var databasePath = @$"Data Source={appdata}\MdExplorer.db";            
            _services.AddFluentMigratorFeatures(
                                            (rb) => {
                                                rb.AddSQLite()
                                                .WithGlobalConnectionString(databasePath)
                                                .ScanIn(typeof(M2021_06_23_001).Assembly)
                                                .For.Migrations();
                                            }, "SQLite");

           
            var builder = _services.BuildServiceProvider();

            _migrator = builder.GetService<IEngineMigrator>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _migrator.UpgradeDatabase();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
