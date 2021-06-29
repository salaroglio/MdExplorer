using Ad.Tools.Dal.Abstractions;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Antlr.Runtime.Misc;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.DependencyInjection;
using MySqlX.XDevAPI;
using NHibernate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Ad.Tools.Dal
{
    public static class DalDI
    {
        public enum DatabaseTypeEnum
        {
            SQLite,
            SQLServer,
            MySQLOrMariaDB
        }

        public class Configuration
        {
            public string DatabaseName;
            public Assembly Assembly { get; set; }
            public IDatabase DatabaseType { get; set; }
            public string ConnectionString { get; set; }
        }

        public static IServiceCollection AddDalFeatures(this IServiceCollection services, IList<Configuration> configurations)
        {
            services.AddSingleton(configurations);            
            services.AddSingleton<IDalMetafactory,DalMetafactory>();            
            return services;
        }


        public static IServiceCollection AddDalFeatures(this IServiceCollection services, Assembly assembly,
            IDatabase databaseType,
            string connectionString)
        {            
            var config = CreateConfiguration(connectionString, databaseType);

            var sessionFactory = Fluently.Configure()
                .Database(config)
                .Mappings(_ => _.FluentMappings.AddFromAssembly(assembly)).BuildSessionFactory();
            services.AddSingleton(sessionFactory);
            services.AddLogging();
            services.AddSingleton<IDALFactory, SQDalFactory>();
            services.AddScoped(_ => _.GetService<IDALFactory>().OpenSession());

            return services;
        }

        private static IPersistenceConfigurer CreateConfiguration(string connectionString, IDatabase db)
        {
            IPersistenceConfigurer toReturn=null;
            //switch (databaseType)
            //{
            //    case DatabaseTypeEnum.SQLite:
            //        toReturn = SQLiteConfiguration.Standard.ConnectionString(connectionString);
            //        break;
            //    case DatabaseTypeEnum.SQLServer:
            //        toReturn = MsSqlConfiguration.MsSql2012.ConnectionString(connectionString);
            //        break;
            //    case DatabaseTypeEnum.MySQLOrMariaDB:
            //        toReturn = MySQLConfiguration.Standard.ConnectionString(connectionString);
            //        break;

            //    default:
            //        break;
            //}
            toReturn = db.Config(connectionString);

            return toReturn;
        }

       
        
    }
}
