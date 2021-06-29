using Ad.Tools.Dal.Abstractions;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Concrete;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal
{
    public class DalMetafactory:IDalMetafactory
    {
        private readonly IDictionary<string,IDALFactory> DalFactories = new Dictionary<string,IDALFactory>();
        private readonly ILogger<SQDalFactory> logger;

        public DalMetafactory(ILogger<SQDalFactory> logger, IList<DalDI.Configuration> configurations)
        {
            foreach (var item in configurations)
            {
                var configurer = CreateConfiguration(item.ConnectionString, item.DatabaseType);
                var session = Fluently.Configure()
                                        .Database(configurer)
                                        .Mappings(_ => _.FluentMappings.AddFromAssembly(item.Assembly))
                                        .BuildSessionFactory();                
                DalFactories.Add(item.DatabaseName, new SQDalFactory(logger, session));
            }
            this.logger = logger;
        }

        IDALFactory IDalMetafactory.GetDALFactory(string databaseName)
        {            
            return DalFactories[databaseName];
        }

        private IPersistenceConfigurer CreateConfiguration(string connectionString, IDatabase db)// DalDI.DatabaseTypeEnum databaseType)
        {
            IPersistenceConfigurer toReturn = null;
            //IDatabase db;
            //switch (databaseType)
            //{
            //    case DalDI.DatabaseTypeEnum.SQLite:
            //        db = new DatabaseSQLite();// SQLiteConfiguration.Standard.ConnectionString(connectionString);
            //        break;
            //    case DalDI.DatabaseTypeEnum.SQLServer:
            //        db = new DatabaseSQLServer();// toReturn = MsSqlConfiguration.MsSql2012.ConnectionString(connectionString);
            //        break;
            //    case DalDI.DatabaseTypeEnum.MySQLOrMariaDB:
            //        db = new DatabaseMySQLOrMariaDB();// toReturn = MySQLConfiguration.Standard.ConnectionString(connectionString);
            //        break;

            //    default:
            //        break;
            //}

            toReturn = db.Config(connectionString);

            return toReturn;
        }

        
    }
}
