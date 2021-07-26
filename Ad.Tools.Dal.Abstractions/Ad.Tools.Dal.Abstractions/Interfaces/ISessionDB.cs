using NHibernate;
using NHibernate.Engine;
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

namespace Ad.Tools.Dal.Abstractions.Interfaces
{
    public interface ISessionDB:IDisposable
    {
        //
        // Summary:
        //     Get the statistics for this session.
        ISessionStatistics Statistics { get; }
        //
        // Summary:
        //     Get the current Unit of Work and return the associated ITransaction object.
        [Obsolete("Use GetCurrentTransaction extension method instead, and check for null.")]
        ITransaction Transaction { get; }
        //
        // Summary:
        //     The current cache mode.
        //
        // Remarks:
        //     Cache mode determines the manner in which this session can interact with the
        //     second level cache.
        CacheMode CacheMode { get; set; }
        //
        // Summary:
        //     Get the NHibernate.ISessionFactory that created this instance.
        ISessionFactory SessionFactory { get; }
        //
        // Summary:
        //     Gets the ADO.NET connection.
        //
        // Remarks:
        //     Applications are responsible for calling commit/rollback upon the connection
        //     before closing the ISession.
        DbConnection Connection { get; }
        //
        // Summary:
        //     Determines at which points Hibernate automatically flushes the session.
        //
        // Remarks:
        //     For a readonly session, it is reasonable to set the flush mode to FlushMode.Never
        //     at the start of the session (in order to achieve some extra performance).
        FlushMode FlushMode { get; set; }
        //
        // Summary:
        //     Is the session connected?
        //
        // Value:
        //     true if the session is connected.
        //
        // Remarks:
        //     A session is considered connected if there is a System.Data.Common.DbConnection
        //     (regardless of its state) or if the field connect is true. Meaning that it will
        //     connect at the next operation that requires a connection.
        bool IsConnected { get; }
        //
        // Summary:
        //     The read-only status for entities (and proxies) loaded into this Session.
        //
        // Remarks:
        //     When a proxy is initialized, the loaded entity will have the same read-only setting
        //     as the uninitialized proxy, regardless of the session's current setting.
        //     To change the read-only setting for a particular entity or proxy that is already
        //     in this session, see NHibernate.ISession.SetReadOnly(System.Object,System.Boolean).
        //     To override this session's read-only setting for entities and proxies loaded
        //     by a query, see NHibernate.IQuery.SetReadOnly(System.Boolean).
        //     This method is a facade for NHibernate.Engine.IPersistenceContext.DefaultReadOnly.
        bool DefaultReadOnly { get; set; }
        //
        // Summary:
        //     Is the ISession still open?
        bool IsOpen { get; }

        //
        // Summary:
        //     Begin a unit of work and return the associated ITransaction object.
        //
        // Returns:
        //     A transaction instance
        //
        // Remarks:
        //     If a new underlying transaction is required, begin the transaction. Otherwise
        //     continue the new work in the context of the existing underlying transaction.
        //     The class of the returned NHibernate.ITransaction object is determined by the
        //     property transaction_factory
        ITransaction BeginTransaction();
        //
        // Summary:
        //     Begin a transaction with the specified isolationLevel
        //
        // Parameters:
        //   isolationLevel:
        //     Isolation level for the new transaction
        //
        // Returns:
        //     A transaction instance having the specified isolation level
        ITransaction BeginTransaction(IsolationLevel isolationLevel);
        //
        // Summary:
        //     Cancel execution of the current query.
        //
        // Remarks:
        //     May be called from one thread to stop execution of a query in another thread.
        //     Use with care!
        void CancelQuery();
        //
        // Summary:
        //     Completely clear the session. Evict all loaded instances and cancel all pending
        //     saves, updates and deletions. Do not close open enumerables or instances of ScrollableResults.
        void Clear();
        //
        // Summary:
        //     End the ISession by disconnecting from the ADO.NET connection and cleaning up.
        //
        // Returns:
        //     The connection provided by the application or null
        //
        // Remarks:
        //     It is not strictly necessary to Close() the ISession but you must at least Disconnect()
        //     it.
        DbConnection Close();
        //
        // Summary:
        //     Is this instance associated with this Session?
        //
        // Parameters:
        //   obj:
        //     an instance of a persistent class
        //
        // Returns:
        //     true if the given instance is associated with this Session
        bool Contains(object obj);
        //
        // Summary:
        //     Creates a new Criteria for the entity class.
        //
        // Parameters:
        //   persistentClass:
        //     The class to Query
        //
        // Returns:
        //     An ICriteria object
        ICriteria CreateCriteria(System.Type persistentClass);
        //
        // Summary:
        //     Creates a new Criteria for the entity class.
        //
        // Type parameters:
        //   T:
        //     The entity class
        //
        // Returns:
        //     An ICriteria object
        ICriteria CreateCriteria<T>() where T : class;
        //
        // Summary:
        //     Creates a new Criteria for the entity class with a specific alias
        //
        // Parameters:
        //   persistentClass:
        //     The class to Query
        //
        //   alias:
        //     The alias of the entity
        //
        // Returns:
        //     An ICriteria object
        ICriteria CreateCriteria(System.Type persistentClass, string alias);
        //
        // Summary:
        //     Creates a new Criteria for the entity class with a specific alias
        //
        // Parameters:
        //   alias:
        //     The alias of the entity
        //
        // Type parameters:
        //   T:
        //     The entity class
        //
        // Returns:
        //     An ICriteria object
        ICriteria CreateCriteria<T>(string alias) where T : class;
        //
        // Summary:
        //     Create a new Criteria instance, for the given entity name, with the given alias.
        //
        // Parameters:
        //   entityName:
        //     The name of the entity to Query
        //
        //   alias:
        //     The alias of the entity
        //
        // Returns:
        //     An ICriteria object
        ICriteria CreateCriteria(string entityName, string alias);
        //
        // Summary:
        //     Create a new Criteria instance, for the given entity name.
        //
        // Parameters:
        //   entityName:
        //     The name of the entity to Query
        //
        // Returns:
        //     An ICriteria object
        ICriteria CreateCriteria(string entityName);
        //
        // Summary:
        //     Create a new instance of Query for the given collection and filter string
        //
        // Parameters:
        //   collection:
        //     A persistent collection
        //
        //   queryString:
        //     A hibernate query
        //
        // Returns:
        //     A query
        IQuery CreateFilter(object collection, string queryString);
        //
        // Summary:
        //     Create a new instance of Query for the given collection and filter string
        //
        // Parameters:
        //   collection:
        //     A persistent collection
        //
        //   queryString:
        //     A hibernate query
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     A query
        Task<IQuery> CreateFilterAsync(object collection, string queryString, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     An NHibernate.IMultiCriteria that can return a list of all the results of all
        //     the criterias.
        [Obsolete("Use ISession.CreateQueryBatch instead.")]
        IMultiCriteria CreateMultiCriteria();
        //
        // Summary:
        //     Create a multi query, a query that can send several queries to the server, and
        //     return all their results in a single call.
        //
        // Returns:
        //     An NHibernate.IMultiQuery that can return a list of all the results of all the
        //     queries. Note that each query result is itself usually a list.
        [Obsolete("Use ISession.CreateQueryBatch instead.")]
        IMultiQuery CreateMultiQuery();
        //
        // Summary:
        //     Create a new instance of Query for the given query string
        //
        // Parameters:
        //   queryString:
        //     A hibernate query string
        //
        // Returns:
        //     The query
        IQuery CreateQuery(string queryString);
        //
        // Summary:
        //     Create a new instance of NHibernate.ISQLQuery for the given SQL query string.
        //
        // Parameters:
        //   queryString:
        //     a query expressed in SQL
        //
        // Returns:
        //     An NHibernate.ISQLQuery from the SQL string
        ISQLQuery CreateSQLQuery(string queryString);
        //
        // Summary:
        //     Delete all objects returned by the query.
        //
        // Parameters:
        //   query:
        //     The query string
        //
        //   values:
        //     A list of values to be written to "?" placeholders in the query
        //
        //   types:
        //     A list of Hibernate types of the values
        //
        // Returns:
        //     The number of instances deleted
        int Delete(string query, object[] values, IType[] types);
        //
        // Summary:
        //     Delete all objects returned by the query.
        //
        // Parameters:
        //   query:
        //     The query string
        //
        //   value:
        //     A value to be written to a "?" placeholer in the query
        //
        //   type:
        //     The hibernate type of value.
        //
        // Returns:
        //     The number of instances deleted
        int Delete(string query, object value, IType type);
        //
        // Summary:
        //     Delete all objects returned by the query.
        //
        // Parameters:
        //   query:
        //     The query string
        //
        // Returns:
        //     Returns the number of objects deleted.
        int Delete(string query);
        //
        // Summary:
        //     Remove a persistent instance from the datastore. The object argument may be an
        //     instance associated with the receiving NHibernate.ISession or a transient instance
        //     with an identifier associated with existing persistent state. This operation
        //     cascades to associated instances if the association is mapped with cascade="delete".
        //
        // Parameters:
        //   entityName:
        //     The entity name for the instance to be removed.
        //
        //   obj:
        //     the instance to be removed
        void Delete(string entityName, object obj);
        //
        // Summary:
        //     Remove a persistent instance from the datastore.
        //
        // Parameters:
        //   obj:
        //     The instance to be removed
        //
        // Remarks:
        //     The argument may be an instance associated with the receiving ISession or a transient
        //     instance with an identifier associated with existing persistent state.
        void Delete(object obj);
        //
        // Summary:
        //     Delete all objects returned by the query.
        //
        // Parameters:
        //   query:
        //     The query string
        //
        //   values:
        //     A list of values to be written to "?" placeholders in the query
        //
        //   types:
        //     A list of Hibernate types of the values
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     The number of instances deleted
        Task<int> DeleteAsync(string query, object[] values, IType[] types, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Remove a persistent instance from the datastore. The object argument may be an
        //     instance associated with the receiving NHibernate.ISession or a transient instance
        //     with an identifier associated with existing persistent state. This operation
        //     cascades to associated instances if the association is mapped with cascade="delete".
        //
        // Parameters:
        //   entityName:
        //     The entity name for the instance to be removed.
        //
        //   obj:
        //     the instance to be removed
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        Task DeleteAsync(string entityName, object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Delete all objects returned by the query.
        //
        // Parameters:
        //   query:
        //     The query string
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     Returns the number of objects deleted.
        Task<int> DeleteAsync(string query, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Remove a persistent instance from the datastore.
        //
        // Parameters:
        //   obj:
        //     The instance to be removed
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     The argument may be an instance associated with the receiving ISession or a transient
        //     instance with an identifier associated with existing persistent state.
        Task DeleteAsync(object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Delete all objects returned by the query.
        //
        // Parameters:
        //   query:
        //     The query string
        //
        //   value:
        //     A value to be written to a "?" placeholer in the query
        //
        //   type:
        //     The hibernate type of value.
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     The number of instances deleted
        Task<int> DeleteAsync(string query, object value, IType type, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Disable the named filter for the current session.
        //
        // Parameters:
        //   filterName:
        //     The name of the filter to be disabled.
        void DisableFilter(string filterName);
        //
        // Summary:
        //     Disconnect the ISession from the current ADO.NET connection.
        //
        // Returns:
        //     The connection provided by the application or null
        //
        // Remarks:
        //     If the connection was obtained by Hibernate, close it or return it to the connection
        //     pool. Otherwise return it to the application. This is used by applications which
        //     require long transactions.
        DbConnection Disconnect();
        //
        // Summary:
        //     Enable the named filter for this current session.
        //
        // Parameters:
        //   filterName:
        //     The name of the filter to be enabled.
        //
        // Returns:
        //     The Filter instance representing the enabled filter.
        IFilter EnableFilter(string filterName);
        //
        // Summary:
        //     Remove this instance from the session cache.
        //
        // Parameters:
        //   obj:
        //     a persistent instance
        //
        // Remarks:
        //     Changes to the instance will not be synchronized with the database. This operation
        //     cascades to associated instances if the association is mapped with cascade="all"
        //     or cascade="all-delete-orphan".
        void Evict(object obj);
        //
        // Summary:
        //     Remove this instance from the session cache.
        //
        // Parameters:
        //   obj:
        //     a persistent instance
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     Changes to the instance will not be synchronized with the database. This operation
        //     cascades to associated instances if the association is mapped with cascade="all"
        //     or cascade="all-delete-orphan".
        Task EvictAsync(object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Force the ISession to flush.
        //
        // Remarks:
        //     Must be called at the end of a unit of work, before committing the transaction
        //     and closing the session (Transaction.Commit() calls this method). Flushing is
        //     the process of synchronizing the underlying persistent store with persistable
        //     state held in memory.
        void Flush();
        //
        // Summary:
        //     Force the ISession to flush.
        //
        // Parameters:
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     Must be called at the end of a unit of work, before committing the transaction
        //     and closing the session (Transaction.Commit() calls this method). Flushing is
        //     the process of synchronizing the underlying persistent store with persistable
        //     state held in memory.
        Task FlushAsync(CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Strongly-typed version of NHibernate.ISession.Get(System.Type,System.Object,NHibernate.LockMode)
        T Get<T>(object id, LockMode lockMode);
        //
        // Summary:
        //     Strongly-typed version of NHibernate.ISession.Get(System.Type,System.Object)
        T Get<T>(object id);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     or null if there is no such persistent instance. (If the instance, or a proxy
        //     for the instance, is already associated with the session, return that instance
        //     or proxy.)
        //
        // Parameters:
        //   clazz:
        //     a persistent class
        //
        //   id:
        //     an identifier
        //
        // Returns:
        //     a persistent instance or null
        object Get(System.Type clazz, object id);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     or null if there is no such persistent instance. Obtain the specified lock mode
        //     if the instance exists.
        //
        // Parameters:
        //   clazz:
        //     a persistent class
        //
        //   id:
        //     an identifier
        //
        //   lockMode:
        //     the lock mode
        //
        // Returns:
        //     a persistent instance or null
        object Get(System.Type clazz, object id, LockMode lockMode);
        //
        // Summary:
        //     Return the persistent instance of the given named entity with the given identifier,
        //     or null if there is no such persistent instance. (If the instance, or a proxy
        //     for the instance, is already associated with the session, return that instance
        //     or proxy.)
        //
        // Parameters:
        //   entityName:
        //     the entity name
        //
        //   id:
        //     an identifier
        //
        // Returns:
        //     a persistent instance or null
        object Get(string entityName, object id);
        //
        // Summary:
        //     Strongly-typed version of NHibernate.ISession.Get(System.Type,System.Object)
        Task<T> GetAsync<T>(object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the persistent instance of the given named entity with the given identifier,
        //     or null if there is no such persistent instance. (If the instance, or a proxy
        //     for the instance, is already associated with the session, return that instance
        //     or proxy.)
        //
        // Parameters:
        //   entityName:
        //     the entity name
        //
        //   id:
        //     an identifier
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     a persistent instance or null
        Task<object> GetAsync(string entityName, object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Strongly-typed version of NHibernate.ISession.Get(System.Type,System.Object,NHibernate.LockMode)
        Task<T> GetAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     or null if there is no such persistent instance. Obtain the specified lock mode
        //     if the instance exists.
        //
        // Parameters:
        //   clazz:
        //     a persistent class
        //
        //   id:
        //     an identifier
        //
        //   lockMode:
        //     the lock mode
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     a persistent instance or null
        Task<object> GetAsync(System.Type clazz, object id, LockMode lockMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     or null if there is no such persistent instance. (If the instance, or a proxy
        //     for the instance, is already associated with the session, return that instance
        //     or proxy.)
        //
        // Parameters:
        //   clazz:
        //     a persistent class
        //
        //   id:
        //     an identifier
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     a persistent instance or null
        Task<object> GetAsync(System.Type clazz, object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Determine the current lock mode of the given object
        //
        // Parameters:
        //   obj:
        //     A persistent instance
        //
        // Returns:
        //     The current lock mode
        LockMode GetCurrentLockMode(object obj);
        //
        // Summary:
        //     Retrieve a currently enabled filter by name.
        //
        // Parameters:
        //   filterName:
        //     The name of the filter to be retrieved.
        //
        // Returns:
        //     The Filter instance representing the enabled filter.
        IFilter GetEnabledFilter(string filterName);
        //
        // Summary:
        //     Return the entity name for a persistent entity
        //
        // Parameters:
        //   obj:
        //     a persistent entity
        //
        // Returns:
        //     the entity name
        string GetEntityName(object obj);
        //
        // Summary:
        //     Return the entity name for a persistent entity
        //
        // Parameters:
        //   obj:
        //     a persistent entity
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     the entity name
        Task<string> GetEntityNameAsync(object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the identifier of an entity instance cached by the ISession
        //
        // Parameters:
        //   obj:
        //     a persistent instance
        //
        // Returns:
        //     the identifier
        //
        // Remarks:
        //     Throws an exception if the instance is transient or associated with a different
        //     ISession
        object GetIdentifier(object obj);
        //
        // Summary:
        //     Obtain an instance of NHibernate.IQuery for a named query string defined in the
        //     mapping file.
        //
        // Parameters:
        //   queryName:
        //     The name of a query defined externally.
        //
        // Returns:
        //     An NHibernate.IQuery from a named query string.
        //
        // Remarks:
        //     The query can be either in HQL or SQL format.
        IQuery GetNamedQuery(string queryName);
        //
        // Summary:
        //     Starts a new Session with the given entity mode in effect. This secondary Session
        //     inherits the connection, transaction, and other context information from the
        //     primary Session. It has to be flushed or disposed by the developer since v5.
        //
        // Parameters:
        //   entityMode:
        //     Ignored.
        //
        // Returns:
        //     The new session.
        [Obsolete("Please use SessionWithOptions instead. Now requires to be flushed and disposed of.")]
        ISession GetSession(EntityMode entityMode);
        //
        // Summary:
        //     Gets the session implementation.
        //
        // Returns:
        //     An NHibernate implementation of the NHibernate.Engine.ISessionImplementor interface
        //
        // Remarks:
        //     This method is provided in order to get the NHibernate implementation of the
        //     session from wrapper implementations. Implementors of the NHibernate.ISession
        //     interface should return the NHibernate implementation of this method.
        ISessionImplementor GetSessionImplementation();
        //
        // Summary:
        //     Does this ISession contain any changes which must be synchronized with the database?
        //     Would any SQL be executed if we flushed this session? May trigger save cascades,
        //     which could cause themselves some SQL to be executed, especially if the identity
        //     id generator is used.
        //
        // Remarks:
        //     The default implementation first checks if it contains saved or deleted entities
        //     to be flushed. If not, it then delegate the check to its NHibernate.Event.IDirtyCheckEventListener,
        //     which by default is NHibernate.Event.Default.DefaultDirtyCheckEventListener.
        //     NHibernate.Event.Default.DefaultDirtyCheckEventListener replicates all the beginning
        //     of the flush process, checking dirtiness of entities loaded in the session and
        //     triggering their pending cascade operations in order to detect new and removed
        //     children. This can have the side effect of performing the NHibernate.ISession.Save(System.Object)
        //     of children, causing their id to be generated. Depending on their id generator,
        //     this can trigger calls to the database and even actually insert them if using
        //     an identity generator.
        bool IsDirty();
        //
        // Summary:
        //     Does this ISession contain any changes which must be synchronized with the database?
        //     Would any SQL be executed if we flushed this session? May trigger save cascades,
        //     which could cause themselves some SQL to be executed, especially if the identity
        //     id generator is used.
        //
        // Parameters:
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     The default implementation first checks if it contains saved or deleted entities
        //     to be flushed. If not, it then delegate the check to its NHibernate.Event.IDirtyCheckEventListener,
        //     which by default is NHibernate.Event.Default.DefaultDirtyCheckEventListener.
        //     NHibernate.Event.Default.DefaultDirtyCheckEventListener replicates all the beginning
        //     of the flush process, checking dirtiness of entities loaded in the session and
        //     triggering their pending cascade operations in order to detect new and removed
        //     children. This can have the side effect of performing the NHibernate.ISession.Save(System.Object)
        //     of children, causing their id to be generated. Depending on their id generator,
        //     this can trigger calls to the database and even actually insert them if using
        //     an identity generator.
        Task<bool> IsDirtyAsync(CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Is the specified entity (or proxy) read-only?
        //
        // Parameters:
        //   entityOrProxy:
        //     An entity (or NHibernate.Proxy.INHibernateProxy)
        //
        // Returns:
        //     true if the entity (or proxy) is read-only, otherwise false.
        //
        // Remarks:
        //     Facade for NHibernate.Engine.IPersistenceContext.IsReadOnly(System.Object).
        bool IsReadOnly(object entityOrProxy);
        //
        // Summary:
        //     Join the System.Transactions.Transaction.Current system transaction.
        //
        // Exceptions:
        //   T:NHibernate.HibernateException:
        //     Thrown if there is no current transaction.
        //
        // Remarks:
        //     Sessions auto-join current transaction by default on their first usage within
        //     a scope. This can be disabled with NHibernate.ISessionBuilder`1.AutoJoinTransaction(System.Boolean)
        //     from a session builder obtained with NHibernate.ISessionFactory.WithOptions,
        //     or with the auto-join transaction configuration setting.
        //     This method allows to explicitly join the current transaction. It does nothing
        //     if it is already joined.
        void JoinTransaction();
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     assuming that the instance exists.
        //
        // Parameters:
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        //
        // Type parameters:
        //   T:
        //     A persistent class
        //
        // Returns:
        //     The persistent instance or proxy
        //
        // Remarks:
        //     You should not use this method to determine if an instance exists (use a query
        //     or NHibernate.ISession.Get``1(System.Object) instead). Use this only to retrieve
        //     an instance that you assume exists, where non-existence would be an actual error.
        T Load<T>(object id);
        //
        // Summary:
        //     Read the persistent state associated with the given identifier into the given
        //     transient instance.
        //
        // Parameters:
        //   obj:
        //     An "empty" instance of the persistent class
        //
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        void Load(object obj, object id);
        //
        // Summary:
        //     Return the persistent instance of the given entityName with the given identifier,
        //     assuming that the instance exists.
        //
        // Parameters:
        //   entityName:
        //     The entity-name of a persistent class
        //
        //   id:
        //     a valid identifier of an existing persistent instance of the class
        //
        // Returns:
        //     The persistent instance or proxy
        //
        // Remarks:
        //     You should not use this method to determine if an instance exists (use NHibernate.ISession.Get(System.String,System.Object)
        //     instead). Use this only to retrieve an instance that you assume exists, where
        //     non-existence would be an actual error.
        object Load(string entityName, object id);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     obtaining the specified lock mode.
        //
        // Parameters:
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        //
        //   lockMode:
        //     The lock level
        //
        // Type parameters:
        //   T:
        //     A persistent class
        //
        // Returns:
        //     the persistent instance
        T Load<T>(object id, LockMode lockMode);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     assuming that the instance exists.
        //
        // Parameters:
        //   theType:
        //     A persistent class
        //
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        //
        // Returns:
        //     The persistent instance or proxy
        //
        // Remarks:
        //     You should not use this method to determine if an instance exists (use a query
        //     or NHibernate.ISession.Get(System.Type,System.Object) instead). Use this only
        //     to retrieve an instance that you assume exists, where non-existence would be
        //     an actual error.
        object Load(System.Type theType, object id);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     obtaining the specified lock mode, assuming the instance exists.
        //
        // Parameters:
        //   entityName:
        //     The entity-name of a persistent class
        //
        //   id:
        //     a valid identifier of an existing persistent instance of the class
        //
        //   lockMode:
        //     the lock level
        //
        // Returns:
        //     the persistent instance or proxy
        object Load(string entityName, object id, LockMode lockMode);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     obtaining the specified lock mode.
        //
        // Parameters:
        //   theType:
        //     A persistent class
        //
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        //
        //   lockMode:
        //     The lock level
        //
        // Returns:
        //     the persistent instance
        object Load(System.Type theType, object id, LockMode lockMode);
        //
        // Summary:
        //     Read the persistent state associated with the given identifier into the given
        //     transient instance.
        //
        // Parameters:
        //   obj:
        //     An "empty" instance of the persistent class
        //
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        Task LoadAsync(object obj, object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     assuming that the instance exists.
        //
        // Parameters:
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Type parameters:
        //   T:
        //     A persistent class
        //
        // Returns:
        //     The persistent instance or proxy
        //
        // Remarks:
        //     You should not use this method to determine if an instance exists (use a query
        //     or NHibernate.ISession.Get``1(System.Object) instead). Use this only to retrieve
        //     an instance that you assume exists, where non-existence would be an actual error.
        Task<T> LoadAsync<T>(object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     obtaining the specified lock mode.
        //
        // Parameters:
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        //
        //   lockMode:
        //     The lock level
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Type parameters:
        //   T:
        //     A persistent class
        //
        // Returns:
        //     the persistent instance
        Task<T> LoadAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     assuming that the instance exists.
        //
        // Parameters:
        //   theType:
        //     A persistent class
        //
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     The persistent instance or proxy
        //
        // Remarks:
        //     You should not use this method to determine if an instance exists (use a query
        //     or NHibernate.ISession.Get(System.Type,System.Object) instead). Use this only
        //     to retrieve an instance that you assume exists, where non-existence would be
        //     an actual error.
        Task<object> LoadAsync(System.Type theType, object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     obtaining the specified lock mode, assuming the instance exists.
        //
        // Parameters:
        //   entityName:
        //     The entity-name of a persistent class
        //
        //   id:
        //     a valid identifier of an existing persistent instance of the class
        //
        //   lockMode:
        //     the lock level
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     the persistent instance or proxy
        Task<object> LoadAsync(string entityName, object id, LockMode lockMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the persistent instance of the given entity class with the given identifier,
        //     obtaining the specified lock mode.
        //
        // Parameters:
        //   theType:
        //     A persistent class
        //
        //   id:
        //     A valid identifier of an existing persistent instance of the class
        //
        //   lockMode:
        //     The lock level
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     the persistent instance
        Task<object> LoadAsync(System.Type theType, object id, LockMode lockMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Return the persistent instance of the given entityName with the given identifier,
        //     assuming that the instance exists.
        //
        // Parameters:
        //   entityName:
        //     The entity-name of a persistent class
        //
        //   id:
        //     a valid identifier of an existing persistent instance of the class
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     The persistent instance or proxy
        //
        // Remarks:
        //     You should not use this method to determine if an instance exists (use NHibernate.ISession.Get(System.String,System.Object)
        //     instead). Use this only to retrieve an instance that you assume exists, where
        //     non-existence would be an actual error.
        Task<object> LoadAsync(string entityName, object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Obtain the specified lock level upon the given object.
        //
        // Parameters:
        //   obj:
        //     A persistent instance
        //
        //   lockMode:
        //     The lock level
        void Lock(object obj, LockMode lockMode);
        //
        // Summary:
        //     Obtain the specified lock level upon the given object.
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a persistent or transient instance
        //
        //   lockMode:
        //     the lock level
        //
        // Remarks:
        //     This may be used to perform a version check (NHibernate.LockMode.Read), to upgrade
        //     to a pessimistic lock (NHibernate.LockMode.Upgrade), or to simply reassociate
        //     a transient instance with a session (NHibernate.LockMode.None). This operation
        //     cascades to associated instances if the association is mapped with cascade="lock".
        void Lock(string entityName, object obj, LockMode lockMode);
        //
        // Summary:
        //     Obtain the specified lock level upon the given object.
        //
        // Parameters:
        //   obj:
        //     A persistent instance
        //
        //   lockMode:
        //     The lock level
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        Task LockAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Obtain the specified lock level upon the given object.
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a persistent or transient instance
        //
        //   lockMode:
        //     the lock level
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     This may be used to perform a version check (NHibernate.LockMode.Read), to upgrade
        //     to a pessimistic lock (NHibernate.LockMode.Upgrade), or to simply reassociate
        //     a transient instance with a session (NHibernate.LockMode.None). This operation
        //     cascades to associated instances if the association is mapped with cascade="lock".
        Task LockAsync(string entityName, object obj, LockMode lockMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Copy the state of the given object onto the persistent object with the same identifier.
        //     If there is no persistent instance currently associated with the session, it
        //     will be loaded. Return the persistent instance. If the given instance is unsaved,
        //     save a copy of and return it as a newly persistent instance. The given instance
        //     does not become associated with the session. This operation cascades to associated
        //     instances if the association is mapped with cascade="merge".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   obj:
        //     a detached instance with state to be copied
        //
        // Returns:
        //     an updated persistent instance
        object Merge(object obj);
        //
        // Summary:
        //     Copy the state of the given object onto the persistent object with the same identifier.
        //     If there is no persistent instance currently associated with the session, it
        //     will be loaded. Return the persistent instance. If the given instance is unsaved,
        //     save a copy of and return it as a newly persistent instance. The given instance
        //     does not become associated with the session. This operation cascades to associated
        //     instances if the association is mapped with cascade="merge".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   entityName:
        //     Name of the entity.
        //
        //   entity:
        //     a detached instance with state to be copied
        //
        // Returns:
        //     an updated persistent instance
        T Merge<T>(string entityName, T entity) where T : class;
        //
        // Summary:
        //     Copy the state of the given object onto the persistent object with the same identifier.
        //     If there is no persistent instance currently associated with the session, it
        //     will be loaded. Return the persistent instance. If the given instance is unsaved,
        //     save a copy of and return it as a newly persistent instance. The given instance
        //     does not become associated with the session. This operation cascades to associated
        //     instances if the association is mapped with cascade="merge".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   entity:
        //     a detached instance with state to be copied
        //
        // Returns:
        //     an updated persistent instance
        T Merge<T>(T entity) where T : class;
        //
        // Summary:
        //     Copy the state of the given object onto the persistent object with the same identifier.
        //     If there is no persistent instance currently associated with the session, it
        //     will be loaded. Return the persistent instance. If the given instance is unsaved,
        //     save a copy of and return it as a newly persistent instance. The given instance
        //     does not become associated with the session. This operation cascades to associated
        //     instances if the association is mapped with cascade="merge".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   entityName:
        //     Name of the entity.
        //
        //   obj:
        //     a detached instance with state to be copied
        //
        // Returns:
        //     an updated persistent instance
        object Merge(string entityName, object obj);
        //
        // Summary:
        //     Copy the state of the given object onto the persistent object with the same identifier.
        //     If there is no persistent instance currently associated with the session, it
        //     will be loaded. Return the persistent instance. If the given instance is unsaved,
        //     save a copy of and return it as a newly persistent instance. The given instance
        //     does not become associated with the session. This operation cascades to associated
        //     instances if the association is mapped with cascade="merge".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   obj:
        //     a detached instance with state to be copied
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     an updated persistent instance
        Task<object> MergeAsync(object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Copy the state of the given object onto the persistent object with the same identifier.
        //     If there is no persistent instance currently associated with the session, it
        //     will be loaded. Return the persistent instance. If the given instance is unsaved,
        //     save a copy of and return it as a newly persistent instance. The given instance
        //     does not become associated with the session. This operation cascades to associated
        //     instances if the association is mapped with cascade="merge".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   entityName:
        //     Name of the entity.
        //
        //   entity:
        //     a detached instance with state to be copied
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     an updated persistent instance
        Task<T> MergeAsync<T>(string entityName, T entity, CancellationToken cancellationToken = default) where T : class;
        //
        // Summary:
        //     Copy the state of the given object onto the persistent object with the same identifier.
        //     If there is no persistent instance currently associated with the session, it
        //     will be loaded. Return the persistent instance. If the given instance is unsaved,
        //     save a copy of and return it as a newly persistent instance. The given instance
        //     does not become associated with the session. This operation cascades to associated
        //     instances if the association is mapped with cascade="merge".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   entity:
        //     a detached instance with state to be copied
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     an updated persistent instance
        Task<T> MergeAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
        //
        // Summary:
        //     Copy the state of the given object onto the persistent object with the same identifier.
        //     If there is no persistent instance currently associated with the session, it
        //     will be loaded. Return the persistent instance. If the given instance is unsaved,
        //     save a copy of and return it as a newly persistent instance. The given instance
        //     does not become associated with the session. This operation cascades to associated
        //     instances if the association is mapped with cascade="merge".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   entityName:
        //     Name of the entity.
        //
        //   obj:
        //     a detached instance with state to be copied
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     an updated persistent instance
        Task<object> MergeAsync(string entityName, object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Make a transient instance persistent. This operation cascades to associated instances
        //     if the association is mapped with cascade="persist".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   obj:
        //     a transient instance to be made persistent
        void Persist(object obj);
        //
        // Summary:
        //     Make a transient instance persistent. This operation cascades to associated instances
        //     if the association is mapped with cascade="persist".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   entityName:
        //     Name of the entity.
        //
        //   obj:
        //     a transient instance to be made persistent
        void Persist(string entityName, object obj);
        //
        // Summary:
        //     Make a transient instance persistent. This operation cascades to associated instances
        //     if the association is mapped with cascade="persist".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   obj:
        //     a transient instance to be made persistent
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        Task PersistAsync(object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Make a transient instance persistent. This operation cascades to associated instances
        //     if the association is mapped with cascade="persist".
        //     The semantics of this method are defined by JSR-220.
        //
        // Parameters:
        //   entityName:
        //     Name of the entity.
        //
        //   obj:
        //     a transient instance to be made persistent
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        Task PersistAsync(string entityName, object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Creates a new Linq System.Linq.IQueryable`1 for the entity class and with given
        //     entity name.
        //
        // Parameters:
        //   entityName:
        //     The entity name.
        //
        // Type parameters:
        //   T:
        //     The type of entity to query.
        //
        // Returns:
        //     An System.Linq.IQueryable`1 instance
        IQueryable<T> Query<T>(string entityName);
        //
        // Summary:
        //     Creates a new Linq System.Linq.IQueryable`1 for the entity class.
        //
        // Type parameters:
        //   T:
        //     The entity class
        //
        // Returns:
        //     An System.Linq.IQueryable`1 instance
        IQueryable<T> Query<T>();
        //
        // Summary:
        //     Creates a new IQueryOver<T> for the entity class.
        //
        // Parameters:
        //   alias:
        //     The alias of the entity
        //
        // Type parameters:
        //   T:
        //     The entity class
        //
        // Returns:
        //     An IQueryOver<T> object
        IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : class;
        //
        // Summary:
        //     Creates a new IQueryOver{T} for the entity class.
        //
        // Parameters:
        //   entityName:
        //     The name of the entity to Query
        //
        //   alias:
        //     The alias of the entity
        //
        // Type parameters:
        //   T:
        //     The entity class
        //
        // Returns:
        //     An IQueryOver{T} object
        IQueryOver<T, T> QueryOver<T>(string entityName, Expression<Func<T>> alias) where T : class;
        //
        // Summary:
        //     Creates a new IQueryOver<T> for the entity class.
        //
        // Type parameters:
        //   T:
        //     The entity class
        //
        // Returns:
        //     An IQueryOver<T> object
        IQueryOver<T, T> QueryOver<T>() where T : class;
        //
        // Summary:
        //     Creates a new IQueryOver{T}; for the entity class.
        //
        // Parameters:
        //   entityName:
        //     The name of the entity to Query
        //
        // Type parameters:
        //   T:
        //     The entity class
        //
        // Returns:
        //     An IQueryOver{T} object
        IQueryOver<T, T> QueryOver<T>(string entityName) where T : class;
        //
        // Summary:
        //     Obtain a new ADO.NET connection.
        //
        // Remarks:
        //     This is used by applications which require long transactions
        void Reconnect();
        //
        // Summary:
        //     Reconnect to the given ADO.NET connection.
        //
        // Parameters:
        //   connection:
        //     An ADO.NET connection
        //
        // Remarks:
        //     This is used by applications which require long transactions
        void Reconnect(DbConnection connection);
        //
        // Summary:
        //     Re-read the state of the given instance from the underlying database.
        //
        // Parameters:
        //   obj:
        //     A persistent instance
        //
        // Remarks:
        //     It is inadvisable to use this to implement long-running sessions that span many
        //     business tasks. This method is, however, useful in certain special circumstances.
        //     For example,
        //     Where a database trigger alters the object state upon insert or update
        //     After executing direct SQL (eg. a mass update) in the same session
        //     After inserting a Blob or Clob
        void Refresh(object obj);
        //
        // Summary:
        //     Re-read the state of the given instance from the underlying database, with the
        //     given LockMode.
        //
        // Parameters:
        //   obj:
        //     a persistent or transient instance
        //
        //   lockMode:
        //     the lock mode to use
        //
        // Remarks:
        //     It is inadvisable to use this to implement long-running sessions that span many
        //     business tasks. This method is, however, useful in certain special circumstances.
        void Refresh(object obj, LockMode lockMode);
        //
        // Summary:
        //     Re-read the state of the given instance from the underlying database, with the
        //     given LockMode.
        //
        // Parameters:
        //   obj:
        //     a persistent or transient instance
        //
        //   lockMode:
        //     the lock mode to use
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     It is inadvisable to use this to implement long-running sessions that span many
        //     business tasks. This method is, however, useful in certain special circumstances.
        Task RefreshAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Re-read the state of the given instance from the underlying database.
        //
        // Parameters:
        //   obj:
        //     A persistent instance
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     It is inadvisable to use this to implement long-running sessions that span many
        //     business tasks. This method is, however, useful in certain special circumstances.
        //     For example,
        //     Where a database trigger alters the object state upon insert or update
        //     After executing direct SQL (eg. a mass update) in the same session
        //     After inserting a Blob or Clob
        Task RefreshAsync(object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Persist the state of the given detached instance, reusing the current identifier
        //     value. This operation cascades to associated instances if the association is
        //     mapped with cascade="replicate".
        //
        // Parameters:
        //   entityName:
        //
        //   obj:
        //     a detached instance of a persistent class
        //
        //   replicationMode:
        void Replicate(string entityName, object obj, ReplicationMode replicationMode);
        //
        // Summary:
        //     Persist all reachable transient objects, reusing the current identifier values.
        //     Note that this will not trigger the Interceptor of the Session.
        //
        // Parameters:
        //   obj:
        //     a detached instance of a persistent class
        //
        //   replicationMode:
        void Replicate(object obj, ReplicationMode replicationMode);
        //
        // Summary:
        //     Persist all reachable transient objects, reusing the current identifier values.
        //     Note that this will not trigger the Interceptor of the Session.
        //
        // Parameters:
        //   obj:
        //     a detached instance of a persistent class
        //
        //   replicationMode:
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        Task ReplicateAsync(object obj, ReplicationMode replicationMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Persist the state of the given detached instance, reusing the current identifier
        //     value. This operation cascades to associated instances if the association is
        //     mapped with cascade="replicate".
        //
        // Parameters:
        //   entityName:
        //
        //   obj:
        //     a detached instance of a persistent class
        //
        //   replicationMode:
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        Task ReplicateAsync(string entityName, object obj, ReplicationMode replicationMode, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Persist the given transient instance, first assigning a generated identifier.
        //
        // Parameters:
        //   obj:
        //     A transient instance of a persistent class
        //
        // Returns:
        //     The generated identifier
        //
        // Remarks:
        //     Save will use the current value of the identifier property if the Assigned generator
        //     is used.
        object Save(object obj);
        //
        // Summary:
        //     Persist the given transient instance, first assigning a generated identifier.
        //     (Or using the current value of the identifier property if the assigned generator
        //     is used.)
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a transient instance of a persistent class
        //
        // Returns:
        //     the generated identifier
        //
        // Remarks:
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        object Save(string entityName, object obj);
        //
        // Summary:
        //     Persist the given transient instance, using the given identifier.
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a transient instance of a persistent class
        //
        //   id:
        //     An unused valid identifier
        //
        // Remarks:
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        void Save(string entityName, object obj, object id);
        //
        // Summary:
        //     Persist the given transient instance, using the given identifier.
        //
        // Parameters:
        //   obj:
        //     A transient instance of a persistent class
        //
        //   id:
        //     An unused valid identifier
        void Save(object obj, object id);
        //
        // Summary:
        //     Persist the given transient instance, first assigning a generated identifier.
        //
        // Parameters:
        //   obj:
        //     A transient instance of a persistent class
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     The generated identifier
        //
        // Remarks:
        //     Save will use the current value of the identifier property if the Assigned generator
        //     is used.
        Task<object> SaveAsync(object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Persist the given transient instance, using the given identifier.
        //
        // Parameters:
        //   obj:
        //     A transient instance of a persistent class
        //
        //   id:
        //     An unused valid identifier
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        Task SaveAsync(object obj, object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Persist the given transient instance, first assigning a generated identifier.
        //     (Or using the current value of the identifier property if the assigned generator
        //     is used.)
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a transient instance of a persistent class
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Returns:
        //     the generated identifier
        //
        // Remarks:
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        Task<object> SaveAsync(string entityName, object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Persist the given transient instance, using the given identifier.
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a transient instance of a persistent class
        //
        //   id:
        //     An unused valid identifier
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        Task SaveAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Either Save() or Update() the given instance, depending upon the value of its
        //     identifier property.
        //
        // Parameters:
        //   obj:
        //     A transient instance containing new or updated state
        //
        // Remarks:
        //     By default the instance is always saved. This behaviour may be adjusted by specifying
        //     an unsaved-value attribute of the identifier property mapping
        void SaveOrUpdate(object obj);
        //
        // Summary:
        //     Either Save() or Update() the given instance, depending upon the value of its
        //     identifier property.
        //
        // Parameters:
        //   entityName:
        //     The name of the entity
        //
        //   obj:
        //     A transient instance containing new or updated state
        //
        //   id:
        //     Identifier of persistent instance
        //
        // Remarks:
        //     By default the instance is always saved. This behaviour may be adjusted by specifying
        //     an unsaved-value attribute of the identifier property mapping
        void SaveOrUpdate(string entityName, object obj, object id);
        //
        // Summary:
        //     Either NHibernate.ISession.Save(System.String,System.Object) or NHibernate.ISession.Update(System.String,System.Object)
        //     the given instance, depending upon resolution of the unsaved-value checks (see
        //     the manual for discussion of unsaved-value checking).
        //
        // Parameters:
        //   entityName:
        //     The name of the entity
        //
        //   obj:
        //     a transient or detached instance containing new or updated state
        //
        // Remarks:
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        void SaveOrUpdate(string entityName, object obj);
        //
        // Summary:
        //     Either Save() or Update() the given instance, depending upon the value of its
        //     identifier property.
        //
        // Parameters:
        //   entityName:
        //     The name of the entity
        //
        //   obj:
        //     A transient instance containing new or updated state
        //
        //   id:
        //     Identifier of persistent instance
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     By default the instance is always saved. This behaviour may be adjusted by specifying
        //     an unsaved-value attribute of the identifier property mapping
        Task SaveOrUpdateAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Either Save() or Update() the given instance, depending upon the value of its
        //     identifier property.
        //
        // Parameters:
        //   obj:
        //     A transient instance containing new or updated state
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     By default the instance is always saved. This behaviour may be adjusted by specifying
        //     an unsaved-value attribute of the identifier property mapping
        Task SaveOrUpdateAsync(object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Either NHibernate.ISession.Save(System.String,System.Object) or NHibernate.ISession.Update(System.String,System.Object)
        //     the given instance, depending upon resolution of the unsaved-value checks (see
        //     the manual for discussion of unsaved-value checking).
        //
        // Parameters:
        //   entityName:
        //     The name of the entity
        //
        //   obj:
        //     a transient or detached instance containing new or updated state
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        Task SaveOrUpdateAsync(string entityName, object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Obtain a NHibernate.ISession builder with the ability to grab certain information
        //     from this session. The built ISession will require its own flushes and disposal.
        //
        // Returns:
        //     The session builder.
        ISharedSessionBuilder SessionWithOptions();
        //
        // Summary:
        //     Sets the batch size of the session
        //
        // Parameters:
        //   batchSize:
        ISession SetBatchSize(int batchSize);
        //
        // Summary:
        //     Change the read-only status of an entity (or proxy).
        //
        // Parameters:
        //   entityOrProxy:
        //     An entity (or NHibernate.Proxy.INHibernateProxy).
        //
        //   readOnly:
        //     If true, the entity or proxy is made read-only; if false, it is made modifiable.
        //
        // Remarks:
        //     Read-only entities can be modified, but changes are not persisted. They are not
        //     dirty-checked and snapshots of persistent state are not maintained.
        //     Immutable entities cannot be made read-only.
        //     To set the default read-only setting for entities and proxies that are loaded
        //     into the session, see NHibernate.ISession.DefaultReadOnly.
        //     This method a facade for NHibernate.Engine.IPersistenceContext.SetReadOnly(System.Object,System.Boolean).
        void SetReadOnly(object entityOrProxy, bool readOnly);
        //
        // Summary:
        //     Update the persistent state associated with the given identifier.
        //
        // Parameters:
        //   obj:
        //     A transient instance containing updated state
        //
        //   id:
        //     Identifier of persistent instance
        //
        // Remarks:
        //     An exception is thrown if there is a persistent instance with the same identifier
        //     in the current session.
        void Update(object obj, object id);
        //
        // Summary:
        //     Update the persistent instance with the identifier of the given transient instance.
        //
        // Parameters:
        //   obj:
        //     A transient instance containing updated state
        //
        // Remarks:
        //     If there is a persistent instance with the same identifier, an exception is thrown.
        //     If the given transient instance has a null identifier, an exception will be thrown.
        void Update(object obj);
        //
        // Summary:
        //     Update the persistent instance associated with the given identifier.
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a detached instance containing updated state
        //
        //   id:
        //     Identifier of persistent instance
        //
        // Remarks:
        //     If there is a persistent instance with the same identifier, an exception is thrown.
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        void Update(string entityName, object obj, object id);
        //
        // Summary:
        //     Update the persistent instance with the identifier of the given detached instance.
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a detached instance containing updated state
        //
        // Remarks:
        //     If there is a persistent instance with the same identifier, an exception is thrown.
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        void Update(string entityName, object obj);
        //
        // Summary:
        //     Update the persistent instance with the identifier of the given transient instance.
        //
        // Parameters:
        //   obj:
        //     A transient instance containing updated state
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     If there is a persistent instance with the same identifier, an exception is thrown.
        //     If the given transient instance has a null identifier, an exception will be thrown.
        Task UpdateAsync(object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Update the persistent state associated with the given identifier.
        //
        // Parameters:
        //   obj:
        //     A transient instance containing updated state
        //
        //   id:
        //     Identifier of persistent instance
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     An exception is thrown if there is a persistent instance with the same identifier
        //     in the current session.
        Task UpdateAsync(object obj, object id, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Update the persistent instance with the identifier of the given detached instance.
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a detached instance containing updated state
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     If there is a persistent instance with the same identifier, an exception is thrown.
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        Task UpdateAsync(string entityName, object obj, CancellationToken cancellationToken = default);
        //
        // Summary:
        //     Update the persistent instance associated with the given identifier.
        //
        // Parameters:
        //   entityName:
        //     The Entity name.
        //
        //   obj:
        //     a detached instance containing updated state
        //
        //   id:
        //     Identifier of persistent instance
        //
        //   cancellationToken:
        //     A cancellation token that can be used to cancel the work
        //
        // Remarks:
        //     If there is a persistent instance with the same identifier, an exception is thrown.
        //     This operation cascades to associated instances if the association is mapped
        //     with cascade="save-update".
        Task UpdateAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default);


        ITransaction GetCurrentTransaction();
        
    }

}
