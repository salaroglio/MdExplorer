using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Abstractions.Interfaces
{
    public interface IDalMetafactory
    {        
        IDALFactory GetDALFactory(string databaseName);
    }
}
