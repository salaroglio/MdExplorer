using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Abstractions.Interfaces
{
    public interface IDALFactory<T1> where T1:ISessionDB
    {
        T1 OpenSession();
        IDAL<T> GetDAL<T>();
    }
}
