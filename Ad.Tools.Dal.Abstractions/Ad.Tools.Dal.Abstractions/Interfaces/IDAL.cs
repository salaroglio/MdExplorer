using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Abstractions.Interfaces
{
    public interface IDAL<T>
    {
        IQueryable<T> GetList();
        void Save(T element);
        void Delete(T element);
    }
}
