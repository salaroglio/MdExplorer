using Ad.Tools.Dal.Abstractions;
using FluentNHibernate.Cfg.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Concrete
{
    public class DatabaseSQLServer : IDatabase
    {
        public IPersistenceConfigurer Config(string connectionString)
        {
            return MsSqlConfiguration.MsSql2012.ConnectionString(connectionString); 
        }
    }
}
