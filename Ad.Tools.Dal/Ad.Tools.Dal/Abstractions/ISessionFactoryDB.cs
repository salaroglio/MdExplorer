using FluentNHibernate.Cfg.Db;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Abstractions
{
    public interface ISessionFactoryDB<T>:ISessionFactory
    {
        void ReplaceDB(IPersistenceConfigurer config, Assembly assembly);
    }
}
