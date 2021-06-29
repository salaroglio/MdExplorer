using Ad.Tools.FluentMigrator.Interfaces;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.FluentMigrator
{

    public class EngineMigrator : IEngineMigrator
    {

        private readonly IMigrationRunner _runner;
        private readonly Assembly[] _assemblies;

        public EngineMigrator(IMigrationRunner runner)
        {            
            _runner = runner;            
        }
        public void UpgradeDatabase()
        {
            // Execute the migrations
            _runner.MigrateUp();
        }
       
        
    }
}
