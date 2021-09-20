using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Decorators;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.DB
{
    public interface IProjectDB : ISessionDB
    {
        
    }

    public class ProjectDB : SessionDB, IProjectDB
    {
        public ProjectDB(ISession implementation) : base(implementation)
        {
        }
    }
}
