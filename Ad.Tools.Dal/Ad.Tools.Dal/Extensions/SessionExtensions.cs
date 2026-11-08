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
        public static IDAL<T> GetDal<T>(this ISessionDB session)
        {
            return new Dal<T>(session);
        }

        public static void Commit (this ISessionDB session)
        {
            var decorated = session as Ad.Tools.Dal.Decorators.SessionDB;
            if (decorated == null || !DalDiagnostics.Enabled)
            {
                session.GetCurrentTransaction()?.Commit();
                decorated?.ReportHold("commit");
                return;
            }
            var start = System.Diagnostics.Stopwatch.GetTimestamp();
            session.GetCurrentTransaction()?.Commit();
            decorated.ReportCommit(DalDiagnostics.ElapsedMs(start));
            decorated.ReportHold("commit");
        }

        public static void Rollback(this ISessionDB session)
        {
            session.GetCurrentTransaction()?.Rollback();
            (session as Ad.Tools.Dal.Decorators.SessionDB)?.ReportHold("rollback");
        }
    }
}
