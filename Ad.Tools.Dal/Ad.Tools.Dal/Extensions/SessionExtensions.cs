using Ad.Tools.Dal.Abstractions.Interfaces;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Extensions
{
    public static class SessionExtensions
    {
        public static IDAL<T> GetDal<T>(this ISession session)
        {
            return new Dal<T>(session);
        }

        public static void Commit (this ISession session)
        {
            session.GetCurrentTransaction()?.Commit();
        }

        public static void Rollback(this ISession session)
        {
            session.GetCurrentTransaction()?.Rollback();
        }
    }
}
