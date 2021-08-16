using Ad.Tools.Dal.Abstractions;
using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Decorators;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;

namespace Ad.Tools.Dal
{
    public class SQDalFactory<T1> : IDALFactory<T1> where T1:ISessionDB
    {
        private ISessionFactory _sessionFactory;
        private ISessionDB _session;
        private ILogger _logger;        
        

        public SQDalFactory(ILogger<SQDalFactory<T1>> logger, ISessionFactoryDB<T1> sessionFactory)
        {
            _logger = logger;
            _sessionFactory = sessionFactory;
        }

        public T1 OpenSession() 
        {
            var sessionDB = new SessionDB(_sessionFactory.OpenSession());
            return (T1) Convert.ChangeType( sessionDB,typeof(T1));
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
