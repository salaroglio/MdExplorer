using Ad.Tools.Dal.Abstractions;
using Ad.Tools.Dal.Abstractions.Interfaces;
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
    public class SessionFactoryDB<T> :  IConvertible where T : ISessionDB
    {
        private ISessionFactory _sessionFactory;        

        public SessionFactoryDB(IPersistenceConfigurer config, Assembly assembly)
        {
            _sessionFactory =  Fluently.Configure()
                        .Database(config)
                        .Mappings(_ => _.FluentMappings.AddFromAssembly(assembly)).BuildSessionFactory();
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

        public ISessionDB GetCurrentSession()
        {
            return (ISessionDB) _sessionFactory.GetCurrentSession();
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

        public T OpenSession(DbConnection connection)
        {
            return (T)_sessionFactory.OpenSession(connection);
        }

        public T OpenSession(IInterceptor sessionLocalInterceptor)
        {
            return (T)_sessionFactory.OpenSession(sessionLocalInterceptor);
        }

        public T OpenSession(DbConnection conn, IInterceptor sessionLocalInterceptor)
        {
            return (T)_sessionFactory.OpenSession(conn, sessionLocalInterceptor);
        }

        public T OpenSession()
        {
            return (T)_sessionFactory.OpenSession();
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
    }
}
