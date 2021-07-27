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
    public interface IUserSettingsDB : ISessionDB
    {

    }

    public class UserSettingsDB : SessionDB, IUserSettingsDB
    {
        public UserSettingsDB(ISession implementation) : base(implementation)
        {
        }
    }
}