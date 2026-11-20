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
        /// <summary>
        /// Points the factory at <paramref name="connectionString"/>. If it is the database the factory already uses
        /// (and the factory is open), nothing happens: closing it would leave every session already handed out
        /// attached to a closed factory, which NHibernate 5.4+ refuses on the first LINQ query.
        /// </summary>
        void ReplaceDB(IPersistenceConfigurer config, Assembly assembly, string connectionString);
    }
}
