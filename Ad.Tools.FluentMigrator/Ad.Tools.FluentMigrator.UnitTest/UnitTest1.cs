using System;
using Ad.Tools.FluentMigrator.Interfaces;
using Ad.Tools.FluentMigrator.UnitTest.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Ad.Tools.FluentMigrator.FluentMigratorDI;

namespace Ad.Tools.FluentMigrator.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Should_CreateDatabase_SQLite()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddFluentMigratorFeatures(@"Data Source=UnitTest.db", 
                    DatabaseConfigurations.ConfigureSQLite, typeof(Migration0).Assembly);            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var engineMigrator = serviceProvider.GetService<IEngineMigrator>();
            engineMigrator.UpgradeDatabase();
        }

        /// <summary>
        /// Tenete presente che per SQLServer il database deve già esistere, vuoto, ma deve esistere
        /// </summary>
        [TestMethod]
        public void Should_CreateDatabase_SQLServer()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddFluentMigratorFeatures(@"Server=.\SQLEXPRESS;Database=DatabaseDiTest;User Id=sa;Password=advice;",
                    DatabaseConfigurations.ConfigureSQLServer, typeof(Migration0).Assembly);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var engineMigrator = serviceProvider.GetService<IEngineMigrator>();
            engineMigrator.UpgradeDatabase();
        }
    }
}
