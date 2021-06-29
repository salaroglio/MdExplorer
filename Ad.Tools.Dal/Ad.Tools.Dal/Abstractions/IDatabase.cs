using FluentNHibernate.Cfg.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Abstractions
{
    public interface IDatabase
    {
        IPersistenceConfigurer Config(string connectionString);
    }
}
