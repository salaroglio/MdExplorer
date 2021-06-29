using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Concrete;
using Ad.Tools.Dal.Extensions;
using Ad.Tools.Dal.UnitTest.Migrations;
using Ad.Tools.FluentMigrator;
using Ad.Tools.FluentMigrator.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlX.XDevAPI;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping;
using System.Collections.Generic;
using System.Reflection;
using static Ad.Tools.FluentMigrator.FluentMigratorDI;

namespace Ad.Tools.Dal.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        private const string _CONN_STRING = @"Server=.\SQLEXPRESS;Database=BellaCopiaWebAPI;User Id=sa;Password=advice;";
        private const string _CONN_STRING_SQLITE = @"Data Source = UnitTest.db";
        IServiceCollection services;
        [TestInitialize]
        public void Init()
        {
            services = new ServiceCollection();
            var connectionString = _CONN_STRING_SQLITE;
            services.AddFluentMigratorFeatures(connectionString,
                    DatabaseConfigurations.ConfigureSQLite, typeof(Migration0).Assembly);
            var serviceProvider = services.BuildServiceProvider();
            var engineMigrator = serviceProvider.GetService<IEngineMigrator>();
            engineMigrator.UpgradeDatabase();
        }

        [TestMethod]
        public void Should_save_simple_userInfo()
        {
            
            services.AddDalFeatures(Assembly.GetExecutingAssembly(), 
                                    new DatabaseSQLite(),
                                    _CONN_STRING_SQLITE);

            var serviceProvider = services.BuildServiceProvider();
            var user = new UserInfo { 
                UserName = "carlos",
                Password = "supercalifragilisticospiralidoso"
            };

            var factory = serviceProvider.GetService<IDALFactory>();
            var session = factory.OpenSession();
            session.BeginTransaction();
            var userDal = session.GetDal<UserInfo>();
            userDal.Save(user);
            session.GetCurrentTransaction()?.Commit();
            factory.CloseSession();
        }

        [TestMethod]
        public void Should_Create_multiple_databases()
        {
            IDalMetafactory sessoionCreator = CreateBellaCopiaDatabase("BellaCopiaWeb");
            var dalFactory = sessoionCreator.GetDALFactory("BellaCopiaWeb");
        }

        private IDalMetafactory CreateBellaCopiaDatabase(string databaseName)
        {
            List<DalDI.Configuration> configurations = new List<DalDI.Configuration>();
            configurations.Add(new DalDI.Configuration
            {
                DatabaseName = databaseName,
                Assembly = Assembly.GetExecutingAssembly(),
                ConnectionString = _CONN_STRING_SQLITE,
                DatabaseType = new DatabaseSQLite()
            });
            services.AddDalFeatures(configurations);
            var serviceProvider = services.BuildServiceProvider();
            var sessoionCreator = serviceProvider.GetService<IDalMetafactory>();
            return sessoionCreator;
        }

        [TestMethod]
        public void Should_save_on_multiples_database()
        {
            IDalMetafactory sessoionCreator = CreateBellaCopiaDatabase("BellaCopiaWeb");

            var user = new UserInfo
            {
                UserName = "carlos",
                Password = "supercalifragilisticospiralidoso"
            };

            var factory = sessoionCreator.GetDALFactory("BellaCopiaWeb");
            
            factory.OpenSession();
            //factory.BeginTransaction();
            var userDal = factory.GetDAL<UserInfo>();
            userDal.Save(user);
            
            //factory.CommitTransaction();
            factory.CloseSession();
        }

    }
}
