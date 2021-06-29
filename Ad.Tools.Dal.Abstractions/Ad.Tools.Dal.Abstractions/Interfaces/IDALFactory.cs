using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Abstractions.Interfaces
{
    public interface IDALFactory
    {
        ISession OpenSession();
        //void BeginTransaction();
        //void CommitTransaction();
        //void RollbackTransaction();
        void CloseSession();
        IDAL<T> GetDAL<T>();
    }
}
