using Ad.Tools.Dal.Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;

namespace Ad.Tools.Dal
{
    public class SQDalFactory : IDALFactory
    {
        private ISessionFactory _sessionFactory;
        private ISession _session;
        private ITransaction _transaction;
        private ILogger _logger;        
        

        public SQDalFactory(ILogger<SQDalFactory> logger, ISessionFactory sessionFactory)
        {
            _logger = logger;
            _sessionFactory = sessionFactory;
        }

        public ISession OpenSession()
        {
            if (_session == null)
                _session = _sessionFactory.OpenSession();
            else
            {
                if (!_session.IsOpen)
                {
                    _session = _sessionFactory.OpenSession();
                }
            }
            _logger.LogDebug("Open Session");
            return _session;
        }

        //public void BeginTransaction()
        //{
        //    if (_transaction != null && _transaction.IsActive)
        //        return;
        //    _transaction = _session.BeginTransaction();
        //    _logger.LogDebug("Begin Transaction");
        //}

        //public void CommitTransaction()
        //{
        //    if (_transaction == null)
        //        return;
        //    else if (!_transaction.IsActive)
        //    {
        //        return;
        //    }
        //    _transaction.Commit();
        //    _logger.LogDebug("Commit Transaction");
        //}

        //public void RollbackTransaction()
        //{
        //    if (_transaction != null && !_transaction.IsActive)
        //        return;
        //    _transaction.Rollback();
        //}


        public void CloseSession()
        {
            if (_session == null)
            {
                return;
            }
            else if (!_session.IsOpen)
            {
                return;
            }
            _session.Close();
            _session.Dispose();
        }

        public IDAL<T> GetDAL<T>()
        {
            return new Dal<T>(_session);
        }        
       

        public void ExecuteSingleQuery(string sql)
        {
            throw new NotImplementedException();
        }
    }
}
