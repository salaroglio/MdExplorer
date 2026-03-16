using Ad.Tools.Dal.Abstractions;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Concrete;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Metadata;
using NHibernate.Stat;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Decorators
{
    public class SessionFactoryDB<T> : ISessionFactoryDB<T>, IConvertible where T : ISessionDB
    {
        private ISessionFactory _sessionFactory;        

        public SessionFactoryDB(IPersistenceConfigurer config, Assembly assembly)
        {
            _sessionFactory =  Fluently.Configure()
                        .Database(config)
                        .Mappings(_ => _.FluentMappings.AddFromAssembly(assembly)).BuildSessionFactory();
        }

        public void ReplaceDB(IPersistenceConfigurer config, Assembly assembly)
        {
            // CRITICAL: Close the old session factory to release database connections
            // This prevents "database is locked" errors when switching projects
            var oldFactory = _sessionFactory;
            System.Diagnostics.Debug.WriteLine($"*** [ReplaceDB] Called! OldFactory is null: {oldFactory == null}, OldFactory.IsClosed: {oldFactory?.IsClosed}");
            Console.WriteLine($"*** [ReplaceDB] Called! OldFactory is null: {oldFactory == null}, OldFactory.IsClosed: {oldFactory?.IsClosed}");

            if (oldFactory != null && !oldFactory.IsClosed)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("*** [ReplaceDB] Closing old factory...");
                    Console.WriteLine("*** [ReplaceDB] Closing old factory...");
                    oldFactory.Close();
                    oldFactory.Dispose();
                    System.Diagnostics.Debug.WriteLine("*** [ReplaceDB] Old factory closed and disposed");
                    Console.WriteLine("*** [ReplaceDB] Old factory closed and disposed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"*** [ReplaceDB] Error closing old factory: {ex.Message}");
                    Console.WriteLine($"*** [ReplaceDB] Error closing old factory: {ex.Message}");
                    // Ignore errors during cleanup - we're replacing anyway
                }
            }

            _sessionFactory = Fluently.Configure()
                        .Database(config)
                        .Mappings(_ => _.FluentMappings.AddFromAssembly(assembly)).BuildSessionFactory();
            System.Diagnostics.Debug.WriteLine("*** [ReplaceDB] New factory created");
            Console.WriteLine("*** [ReplaceDB] New factory created");
        }

        public IStatistics Statistics => _sessionFactory.Statistics;

        public bool IsClosed => _sessionFactory.IsClosed;

        public ICollection<string> DefinedFilterNames => _sessionFactory.DefinedFilterNames;

        public void Close()
        {
            _sessionFactory.Close();
        }

        public Task CloseAsync(CancellationToken cancellationToken = default)
        {
            return _sessionFactory.CloseAsync(cancellationToken);
        }

        public void Dispose()
        {
            _sessionFactory.Dispose();
        }

        public void Evict(Type persistentClass)
        {
            _sessionFactory.Evict(persistentClass);
        }

        public void Evict(Type persistentClass, object id)
        {
            _sessionFactory.Evict(persistentClass, id);
        }

        public Task EvictAsync(Type persistentClass, CancellationToken cancellationToken = default)
        {
            return _sessionFactory.EvictAsync(persistentClass, cancellationToken);
        }

        public Task EvictAsync(Type persistentClass, object id, CancellationToken cancellationToken = default)
        {
            return _sessionFactory.EvictAsync(persistentClass, id, cancellationToken);
        }

        public void EvictCollection(string roleName)
        {
            _sessionFactory.EvictCollection(roleName);
        }

        public void EvictCollection(string roleName, object id)
        {
            _sessionFactory.EvictCollection(roleName, id);
        }

        public Task EvictCollectionAsync(string roleName, CancellationToken cancellationToken = default)
        {
            return _sessionFactory.EvictCollectionAsync(roleName, cancellationToken);
        }

        public Task EvictCollectionAsync(string roleName, object id, CancellationToken cancellationToken = default)
        {
            return _sessionFactory.EvictCollectionAsync(roleName, id, cancellationToken);
        }

        public void EvictEntity(string entityName)
        {
            _sessionFactory.EvictEntity(entityName);
        }

        public void EvictEntity(string entityName, object id)
        {
            _sessionFactory.EvictEntity(entityName, id);
        }

        public Task EvictEntityAsync(string entityName, CancellationToken cancellationToken = default)
        {
            return _sessionFactory.EvictEntityAsync(entityName, cancellationToken);
        }

        public Task EvictEntityAsync(string entityName, object id, CancellationToken cancellationToken = default)
        {
            return _sessionFactory.EvictEntityAsync(entityName, id, cancellationToken);
        }

        public void EvictQueries()
        {
            _sessionFactory.EvictQueries();
        }

        public void EvictQueries(string cacheRegion)
        {
            _sessionFactory.EvictQueries(cacheRegion);
        }

        public Task EvictQueriesAsync(CancellationToken cancellationToken = default)
        {
            return EvictQueriesAsync(cancellationToken);
        }

        public Task EvictQueriesAsync(string cacheRegion, CancellationToken cancellationToken = default)
        {
            return _sessionFactory.EvictQueriesAsync(cacheRegion, cancellationToken);
        }

        public IDictionary<string, IClassMetadata> GetAllClassMetadata()
        {
            return _sessionFactory.GetAllClassMetadata();
        }

        public IDictionary<string, ICollectionMetadata> GetAllCollectionMetadata()
        {
            return _sessionFactory.GetAllCollectionMetadata();
        }

        public IClassMetadata GetClassMetadata(Type persistentClass)
        {
            return _sessionFactory.GetClassMetadata(persistentClass);
        }

        public IClassMetadata GetClassMetadata(string entityName)
        {
            return _sessionFactory.GetClassMetadata(entityName);
        }

        public ICollectionMetadata GetCollectionMetadata(string roleName)
        {
            return _sessionFactory.GetCollectionMetadata(roleName);
        }

        public ISession GetCurrentSession()
        {
            return  _sessionFactory.GetCurrentSession();
        }

        public FilterDefinition GetFilterDefinition(string filterName)
        {
            return _sessionFactory.GetFilterDefinition(filterName);
        }


        /// <summary>
        /// Inizio interfaccia convertible
        /// </summary>
        /// <returns></returns>
        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public ISession OpenSession(DbConnection connection)
        {
            return _sessionFactory.OpenSession(connection);
        }

        public ISession OpenSession(IInterceptor sessionLocalInterceptor)
        {
            return _sessionFactory.OpenSession(sessionLocalInterceptor);
        }

        public ISession OpenSession(DbConnection conn, IInterceptor sessionLocalInterceptor)
        {
            return _sessionFactory.OpenSession(conn, sessionLocalInterceptor);
        }

        public ISession OpenSession()
        {
            var session = _sessionFactory.OpenSession();
            DatabaseSQLite.ApplyPragmas(session);
            return session;
        }

        public IStatelessSession OpenStatelessSession()
        {
            return _sessionFactory.OpenStatelessSession();
        }

        public IStatelessSession OpenStatelessSession(DbConnection connection)
        {
            return _sessionFactory.OpenStatelessSession(connection);
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            return this;
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ISessionBuilder WithOptions()
        {
            throw new NotImplementedException();
        }

        public IStatelessSessionBuilder WithStatelessOptions()
        {
            throw new NotImplementedException();
        }
       

        //ISession ISessionFactory.OpenSession(DbConnection connection)
        //{
        //    throw new NotImplementedException();
        //}

        //ISession ISessionFactory.OpenSession(IInterceptor sessionLocalInterceptor)
        //{
        //    throw new NotImplementedException();
        //}

        //ISession ISessionFactory.OpenSession(DbConnection conn, IInterceptor sessionLocalInterceptor)
        //{
        //    throw new NotImplementedException();
        //}

        //ISession ISessionFactory.OpenSession()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
