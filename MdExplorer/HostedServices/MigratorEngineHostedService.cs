using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
using MdExplorer.Migrations.EngineDb.Version202107;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Ad.Tools.FluentMigrator.FluentMigratorDI;

namespace MdExplorer.Service.HostedServices
{
    public class MigratorEngineHostedService : IHostedService
    {
        private readonly IEngineMigrator _migrator;
        private readonly IServiceCollection _services;

        public MigratorEngineHostedService(ILogger<MigratorEngineHostedService> logger)
        {
            _services = new ServiceCollection();            
            logger.LogInformation($@"Upgrade database in: {Directory.GetCurrentDirectory()}"); 
            var databaseEngine = @$"Data Source=.\MdEngine.db";            
            _services.AddFluentMigratorFeatures(
                                            (rb) => {
                                                rb.AddSQLite()
                                                .WithGlobalConnectionString(databaseEngine)
                                                .ScanIn(typeof(ME2021_07_23_001).Assembly)
                                                .For.Migrations();
                                            }, "SQLite");
            var builder = _services.BuildServiceProvider();

            _migrator = builder.GetService<IEngineMigrator>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _migrator.UpgradeDatabase();
                return Task.CompletedTask;
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {            
            return Task.CompletedTask;
        }
    }
}
