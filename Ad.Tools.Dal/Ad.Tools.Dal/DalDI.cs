using Ad.Tools.Dal.Abstractions;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Decorators;
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
using Microsoft.Extensions.DependencyInjection.Extensions;

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

       public static void ReplaceDalFeatures(this IServiceProvider serviceProvider, Assembly assembly,
            IDatabase databaseType, Type currentInterface,
            string connectionString)
        {
            var config = CreateConfiguration(connectionString, databaseType);
            Type ISessionFactoryDB = typeof(ISessionFactoryDB<>);
            Type genericISessionFactoryDB = ISessionFactoryDB.MakeGenericType(currentInterface);

            var currentInstance = (dynamic) serviceProvider.GetService(genericISessionFactoryDB);
            currentInstance.ReplaceDB(config, assembly);

        }


        public static IServiceCollection AddDalFeatures(this IServiceCollection services, Assembly assembly,
            IDatabase databaseType,Type currentInterface,
            string connectionString)
        {            
            var config = CreateConfiguration(connectionString, databaseType);

            //var sessionFactory = Fluently.Configure()
            //    .Database(config)
            //    .Mappings(_ => _.FluentMappings.AddFromAssembly(assembly)).BuildSessionFactory();

            Type ISessionFactoryDB = typeof(ISessionFactoryDB<>);
            Type genericISessionFactoryDB = ISessionFactoryDB.MakeGenericType(currentInterface);

            Type sessionFactoryDB = typeof(SessionFactoryDB<>);
            Type genericSessionFactoryDB = sessionFactoryDB.MakeGenericType(currentInterface);

            services.AddSingleton(genericISessionFactoryDB,
               Activator.CreateInstance(genericSessionFactoryDB,config,assembly));

            //services.AddSingleton();
            services.AddLogging();
            Type IdalFactoryType = typeof(IDALFactory<>);
            Type genericIDalFactory = IdalFactoryType.MakeGenericType(currentInterface);

            Type sqlDalFactoryType = typeof(SQDalFactory<>);
            Type genericSqlDalFactoryType = sqlDalFactoryType.MakeGenericType(currentInterface);
            
            
            services.AddSingleton(genericIDalFactory, genericSqlDalFactoryType);
            

            services.AddScoped(currentInterface, _ => {
                return ((dynamic) _.GetService(genericIDalFactory)).OpenSession();
                });

            return services;
        }

        private static IPersistenceConfigurer CreateConfiguration(string connectionString, IDatabase db)
        {
            IPersistenceConfigurer toReturn=null;       
            toReturn = db.Config(connectionString);

            return toReturn;
        }

       
        
    }
}
