using Ad.Tools.Dal.Abstractions.Interfaces;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Stat;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Decorators
{
    public class SessionDB :  ISessionDB, IConvertible //where T1:ISessionDB
    {
        private readonly ISession _implementation;
        

        public SessionDB(ISession implementation)
        {
            _implementation = implementation;
        }

        public FlushMode FlushMode
        {
            get
            {
                return _implementation.FlushMode;
            }
            set { _implementation.FlushMode = value; }
        }
        public CacheMode CacheMode { get { return _implementation.CacheMode; } set { _implementation.CacheMode = value; } }

        public ISessionFactory SessionFactory => _implementation.SessionFactory;

        public DbConnection Connection => _implementation.Connection;

        public bool IsOpen => _implementation.IsOpen;

        public bool IsConnected => _implementation.IsConnected;

        public bool DefaultReadOnly { get { return _implementation.DefaultReadOnly; } set { _implementation.DefaultReadOnly = value; } }

        public ITransaction Transaction => _implementation.Transaction;

        public ISessionStatistics Statistics => _implementation.Statistics;

        public ITransaction BeginTransaction()
        {
            return _implementation.BeginTransaction();
        }

        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return _implementation.BeginTransaction(isolationLevel);
        }

        public void CancelQuery()
        {
            _implementation.CancelQuery();
        }

        public void Clear()
        {
            _implementation.Clear();
        }

        public DbConnection Close()
        {
            return _implementation.Close();
        }

        public bool Contains(object obj)
        {
            return _implementation.Contains(obj);
        }

        public ICriteria CreateCriteria<T>() where T : class
        {
            return _implementation.CreateCriteria<T>();
        }

        public ICriteria CreateCriteria<T>(string alias) where T : class
        {
            return _implementation.CreateCriteria<T>(alias);
        }

        public ICriteria CreateCriteria(Type persistentClass)
        {
            return _implementation.CreateCriteria(persistentClass);
        }

        public ICriteria CreateCriteria(Type persistentClass, string alias)
        {
            return _implementation.CreateCriteria(persistentClass,alias);
        }

        public ICriteria CreateCriteria(string entityName)
        {
            return _implementation.CreateCriteria(entityName);
        }

        public ICriteria CreateCriteria(string entityName, string alias)
        {
            return _implementation.CreateCriteria(entityName,alias);
        }

        public IQuery CreateFilter(object collection, string queryString)
        {
            return _implementation.CreateFilter(collection,queryString);
        }

        public Task<IQuery> CreateFilterAsync(object collection, string queryString, CancellationToken cancellationToken = default)
        {
            return _implementation.CreateFilterAsync(collection,queryString,cancellationToken);
        }

        public IMultiCriteria CreateMultiCriteria()
        {
            return _implementation.CreateMultiCriteria();
        }

        public IMultiQuery CreateMultiQuery()
        {
            return _implementation.CreateMultiQuery();
        }

        public IQuery CreateQuery(string queryString)
        {
            return _implementation.CreateQuery(queryString);
        }

        public ISQLQuery CreateSQLQuery(string queryString)
        {
            return _implementation.CreateSQLQuery(queryString);
        }

        public void Delete(object obj)
        {
            _implementation.Delete(obj);
        }

        public void Delete(string entityName, object obj)
        {
            _implementation.Delete(entityName,obj);
        }

        public int Delete(string query)
        {
            return _implementation.Delete(query);
        }

        public int Delete(string query, object value, IType type)
        {
            return _implementation.Delete(query,value,type);
        }

        public int Delete(string query, object[] values, IType[] types)
        {
            return _implementation.Delete(query,values,types);
        }

        public Task DeleteAsync(object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.DeleteAsync(obj,cancellationToken);
        }

        public Task DeleteAsync(string entityName, object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.DeleteAsync(entityName,obj,cancellationToken);
        }

        public Task<int> DeleteAsync(string query, CancellationToken cancellationToken = default)
        {
            return _implementation.DeleteAsync(query,cancellationToken);
        }

        public Task<int> DeleteAsync(string query, object value, IType type, CancellationToken cancellationToken = default)
        {
            return _implementation.DeleteAsync(query,value,type,cancellationToken);
        }

        public Task<int> DeleteAsync(string query, object[] values, IType[] types, CancellationToken cancellationToken = default)
        {
            return _implementation.DeleteAsync(query,values,types,cancellationToken);
        }

        public void DisableFilter(string filterName)
        {
            _implementation.DeleteAsync(filterName);
        }

        public DbConnection Disconnect()
        {
            return _implementation.Disconnect();
        }

        public void Dispose()
        {
            _implementation.Dispose();
        }

        public IFilter EnableFilter(string filterName)
        {
            return _implementation.EnableFilter(filterName);
        }

        public void Evict(object obj)
        {
            _implementation.Evict(obj);
        }

        public Task EvictAsync(object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.EvictAsync(obj,cancellationToken);
        }

        public void Flush()
        {
            _implementation.Flush();
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return _implementation.FlushAsync(cancellationToken);
        }

        public object Get(Type clazz, object id)
        {
            return _implementation.Get(clazz,id);
        }

        public object Get(Type clazz, object id, LockMode lockMode)
        {
            return _implementation.Get(clazz,id,lockMode);
        }

        public object Get(string entityName, object id)
        {
            return _implementation.Get(entityName,id);
        }

        public T Get<T>(object id)
        {
            return _implementation.Get<T>(id);
        }

        public T Get<T>(object id, LockMode lockMode)
        {
            return _implementation.Get<T>(id,lockMode);
        }

        public Task<object> GetAsync(Type clazz, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.GetAsync(clazz,id,cancellationToken);
        }

        public Task<object> GetAsync(Type clazz, object id, LockMode lockMode, CancellationToken cancellationToken = default)
        {
            return _implementation.GetAsync(clazz,id,lockMode,cancellationToken);
        }

        public Task<object> GetAsync(string entityName, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.GetAsync(entityName,id,cancellationToken);
        }

        public Task<T> GetAsync<T>(object id, CancellationToken cancellationToken = default)
        {
            return _implementation.GetAsync<T>(id,cancellationToken);
        }

        public Task<T> GetAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = default)
        {
            return _implementation.GetAsync<T>(id,lockMode,cancellationToken);
        }

        public LockMode GetCurrentLockMode(object obj)
        {
            return _implementation.GetCurrentLockMode(obj);
        }

        public IFilter GetEnabledFilter(string filterName)
        {
            return _implementation.GetEnabledFilter(filterName);
        }

        public string GetEntityName(object obj)
        {
            return _implementation.GetEntityName(obj);
        }

        public Task<string> GetEntityNameAsync(object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.GetEntityNameAsync(obj,cancellationToken);
        }

        public object GetIdentifier(object obj)
        {
            return _implementation.GetIdentifier(obj);
        }

        public IQuery GetNamedQuery(string queryName)
        {
            return _implementation.GetNamedQuery(queryName);
        }

        public ISession GetSession(EntityMode entityMode)
        {
            return _implementation.GetSession(entityMode);
        }

        public ISessionImplementor GetSessionImplementation()
        {
            return _implementation.GetSessionImplementation();
        }

        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public bool IsDirty()
        {
            return _implementation.IsDirty();
        }

        public Task<bool> IsDirtyAsync(CancellationToken cancellationToken = default)
        {
            return _implementation.IsDirtyAsync(cancellationToken);
        }

        public bool IsReadOnly(object entityOrProxy)
        {
            return _implementation.IsReadOnly(entityOrProxy);
        }

        public void JoinTransaction()
        {
            _implementation.JoinTransaction();
        }

        public object Load(Type theType, object id, LockMode lockMode)
        {
            return _implementation.Load(theType,id,lockMode);
        }

        public object Load(string entityName, object id, LockMode lockMode)
        {
            return _implementation.Load(entityName,id,lockMode);
        }

        public object Load(Type theType, object id)
        {
           return _implementation.Load(theType,id);
        }

        public T Load<T>(object id, LockMode lockMode)
        {
            return _implementation.Load<T>(id,lockMode);
        }

        public T Load<T>(object id)
        {
            return _implementation.Load<T>(id);
        }

        public object Load(string entityName, object id)
        {
            return _implementation.Load(entityName,id);
        }

        public void Load(object obj, object id)
        {
            _implementation.Load(obj,id);
        }

        public Task<object> LoadAsync(Type theType, object id, LockMode lockMode, CancellationToken cancellationToken = default)
        {
            return _implementation.LoadAsync(theType,id,lockMode,cancellationToken);
        }

        public Task<object> LoadAsync(string entityName, object id, LockMode lockMode, CancellationToken cancellationToken = default)
        {
            return _implementation.LoadAsync(entityName,id,lockMode,cancellationToken);
        }

        public Task<object> LoadAsync(Type theType, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.LoadAsync(theType,id,cancellationToken);
        }

        public Task<T> LoadAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = default)
        {
            return _implementation.LoadAsync<T>(id,lockMode,cancellationToken);
        }

        public Task<T> LoadAsync<T>(object id, CancellationToken cancellationToken = default)
        {
            return _implementation.LoadAsync<T>(id,cancellationToken);
        }

        public Task<object> LoadAsync(string entityName, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.LoadAsync(entityName,id,cancellationToken);
        }

        public Task LoadAsync(object obj, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.LoadAsync(obj,id,cancellationToken);
        }

        public void Lock(object obj, LockMode lockMode)
        {
            _implementation.Lock(obj,lockMode);
        }

        public void Lock(string entityName, object obj, LockMode lockMode)
        {
            _implementation.Lock(entityName,obj,lockMode);
        }

        public Task LockAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = default)
        {
            return _implementation.LockAsync(obj,lockMode,cancellationToken);
        }

        public Task LockAsync(string entityName, object obj, LockMode lockMode, CancellationToken cancellationToken = default)
        {
            return _implementation.LoadAsync(entityName,obj,lockMode,cancellationToken);
        }

        public object Merge(object obj)
        {
            return _implementation.Merge(obj);
        }

        public object Merge(string entityName, object obj)
        {
            return _implementation.Merge(entityName,obj);
        }

        public T Merge<T>(T entity) where T : class
        {
            return _implementation.Merge<T>(entity);
        }

        public T Merge<T>(string entityName, T entity) where T : class
        {
            return _implementation.Merge<T>(entityName,entity);
        }

        public Task<object> MergeAsync(object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.MergeAsync(obj,cancellationToken);
        }

        public Task<object> MergeAsync(string entityName, object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.MergeAsync(entityName,obj,cancellationToken);
        }

        public Task<T> MergeAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            return _implementation.MergeAsync(entity,cancellationToken);
        }

        public Task<T> MergeAsync<T>(string entityName, T entity, CancellationToken cancellationToken = default) where T : class
        {
            return _implementation.MergeAsync(entityName,entity,cancellationToken);
        }

        public void Persist(object obj)
        {
            _implementation.Persist(obj);
        }

        public void Persist(string entityName, object obj)
        {
            _implementation.Persist(entityName,obj);
        }

        public Task PersistAsync(object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.PersistAsync(obj,cancellationToken);
        }

        public Task PersistAsync(string entityName, object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.PersistAsync(entityName,obj,cancellationToken);
        }

        public IQueryable<T> Query<T>()
        {
            return _implementation.Query<T>();
        }

        public IQueryable<T> Query<T>(string entityName)
        {
            return _implementation.Query<T>(entityName);
        }

        public IQueryOver<T, T> QueryOver<T>() where T : class
        {
            return _implementation.QueryOver<T>();
        }

        public IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : class
        {
            return _implementation.QueryOver<T>(alias);
        }

        public IQueryOver<T, T> QueryOver<T>(string entityName) where T : class
        {
            return _implementation.QueryOver<T>(entityName);
        }

        public IQueryOver<T, T> QueryOver<T>(string entityName, Expression<Func<T>> alias) where T : class
        {
            return _implementation.QueryOver<T>(entityName,alias);
        }

        public void Reconnect()
        {
            _implementation.Reconnect();
        }

        public void Reconnect(DbConnection connection)
        {
            _implementation.Reconnect(connection);
        }

        public void Refresh(object obj)
        {
            _implementation.Refresh(obj);
        }

        public void Refresh(object obj, LockMode lockMode)
        {
            _implementation.Refresh(obj,lockMode);
        }

        public Task RefreshAsync(object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.RefreshAsync(obj,cancellationToken);
        }

        public Task RefreshAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = default)
        {
            return _implementation.RefreshAsync(obj,lockMode,cancellationToken);
        }

        public void Replicate(object obj, ReplicationMode replicationMode)
        {
            _implementation.Replicate(obj,replicationMode);
        }

        public void Replicate(string entityName, object obj, ReplicationMode replicationMode)
        {
            _implementation.Replicate(entityName,obj,replicationMode);
        }

        public Task ReplicateAsync(object obj, ReplicationMode replicationMode, CancellationToken cancellationToken = default)
        {
            return _implementation.ReplicateAsync(obj,replicationMode,cancellationToken);
        }

        public Task ReplicateAsync(string entityName, object obj, ReplicationMode replicationMode, CancellationToken cancellationToken = default)
        {
            return _implementation.ReplicateAsync(entityName,obj,replicationMode,cancellationToken);
        }

        public object Save(object obj)
        {
            return _implementation.Save(obj);
        }

        public void Save(object obj, object id)
        {
            _implementation.Save(obj,id);
        }

        public object Save(string entityName, object obj)
        {
            return _implementation.Save(entityName, obj);
        }

        public void Save(string entityName, object obj, object id)
        {
            _implementation.Save(entityName,obj,id);
        }

        public Task<object> SaveAsync(object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.SaveAsync(obj,cancellationToken);
        }

        public Task SaveAsync(object obj, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.SaveAsync(obj,id,cancellationToken);
        }

        public Task<object> SaveAsync(string entityName, object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.SaveAsync(entityName,obj,cancellationToken);
        }

        public Task SaveAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.SaveAsync(entityName,obj,id,cancellationToken);
        }

        public void SaveOrUpdate(object obj)
        {
            _implementation.SaveOrUpdate(obj);
        }

        public void SaveOrUpdate(string entityName, object obj)
        {
            _implementation.SaveOrUpdate(entityName,obj);
        }

        public void SaveOrUpdate(string entityName, object obj, object id)
        {
            _implementation.SaveOrUpdate(entityName,obj,id);
        }

        public Task SaveOrUpdateAsync(object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.SaveOrUpdateAsync(obj,cancellationToken);
        }

        public Task SaveOrUpdateAsync(string entityName, object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.SaveOrUpdateAsync(entityName,obj,cancellationToken);
        }

        public Task SaveOrUpdateAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.SaveOrUpdateAsync(entityName,obj,id,cancellationToken);
        }

        public ISharedSessionBuilder SessionWithOptions()
        {
            return _implementation.SessionWithOptions();
        }

        public ISession SetBatchSize(int batchSize)
        {
            return _implementation.SetBatchSize(batchSize);
        }

        public void SetReadOnly(object entityOrProxy, bool readOnly)
        {
            _implementation.SetReadOnly(entityOrProxy,readOnly);
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

        public void Update(object obj)
        {
            _implementation.Update(obj);
        }

        public void Update(object obj, object id)
        {
            _implementation.Update(obj,id);
        }

        public void Update(string entityName, object obj)
        {
            _implementation.Update(entityName,obj);
        }

        public void Update(string entityName, object obj, object id)
        {
            _implementation.Update(entityName,obj,id);
        }

        public Task UpdateAsync(object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.UpdateAsync(obj,cancellationToken);
        }

        public Task UpdateAsync(object obj, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.UpdateAsync(obj,id,cancellationToken);
        }

        public Task UpdateAsync(string entityName, object obj, CancellationToken cancellationToken = default)
        {
            return _implementation.UpdateAsync(entityName,obj,cancellationToken);
        }

        public Task UpdateAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default)
        {
            return _implementation.UpdateAsync(entityName,obj,id,cancellationToken);
        }

        public ITransaction GetCurrentTransaction()
        {
            return _implementation.GetCurrentTransaction();
        }
    }
}
