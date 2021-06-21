using Ad.Tools.FluentMigrator.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Service.HostedServices
{
    public class MigratorHostedService : IHostedService
    {
        private readonly IEngineMigrator _migrator;

        public MigratorHostedService(IEngineMigrator migrator, 
            ILogger<MigratorHostedService> logger)
        {
            _migrator = migrator;
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
