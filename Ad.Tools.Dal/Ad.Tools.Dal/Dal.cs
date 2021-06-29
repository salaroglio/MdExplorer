using Ad.Tools.Dal.Abstractions.Interfaces;
using NHibernate;
using System.Linq;

namespace Ad.Tools.Dal
{
    public class Dal<T> : IDAL<T>

    {
        private readonly ISession session;

        public Dal(ISession session)
        {
            this.session = session;
        }
        public void Delete(T element)
        {
            session.Delete(element);
        }

        public IQueryable<T> GetList()
        {
            return session.Query<T>();
        }

        public void Save(T element)
        {
            session.SaveOrUpdate(element);
        }
    }
}
