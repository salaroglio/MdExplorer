using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Decorators;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;

namespace Ad.Tools.Dal
{
    public class SQDalFactory : IDALFactory
    {
        private ISessionFactory _sessionFactory;
        private ISessionDB _session;
        private ITransaction _transaction;
        private ILogger _logger;        
        

        public SQDalFactory(ILogger<SQDalFactory> logger, ISessionFactory sessionFactory)
        {
            _logger = logger;
            _sessionFactory = sessionFactory;
        }

        public ISessionDB OpenSession()
        {
            var sessionDB = new SessionDB(_sessionFactory.OpenSession());
            return sessionDB;
        }       

        public void CloseSession()
        {           
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
