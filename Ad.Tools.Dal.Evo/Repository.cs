using Ad.Tools.Dal.Evo.Abstractions;
using NHibernate;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Evo
{
    /// <summary>
    /// Implementazione generica del repository utilizzando NHibernate.
    /// </summary>
    /// <typeparam name="T">Il tipo dell'entità.</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ISession _session;

        public Repository(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public virtual Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            return _session.GetAsync<T>(id, cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _session.Query<T>().ToListAsync(cancellationToken);
        }

        public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _session.Query<T>().Where(predicate).ToListAsync(cancellationToken);
        }

        public virtual IQueryable<T> Query()
        {
            return _session.Query<T>();
        }

        public virtual Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            return _session.SaveAsync(entity, cancellationToken);
            // Consider SaveOrUpdateAsync if entities might already exist and need updating instead of saving.
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            // NHibernate doesn't have a bulk SaveAsync. Iterate or use extensions if performance is critical.
            foreach (var entity in entities)
            {
                await _session.SaveAsync(entity, cancellationToken);
            }
            // Consider batching or specific NHibernate batch features for large collections.
        }

        public virtual void Update(T entity)
        {
            // NHibernate tracks changes, so often just modifying the loaded entity is enough.
            // Explicit Update might be needed if the entity was detached or for specific scenarios.
             _session.Update(entity);
            // Consider Merge or SaveOrUpdate for more complex scenarios (e.g., detached entities).
        }

        public virtual void Delete(T entity)
        {
            _session.Delete(entity);
        }

        public virtual void DeleteRange(IEnumerable<T> entities)
        {
            // NHibernate doesn't have a bulk Delete. Iterate or use HQL/SQL for bulk deletes.
            foreach (var entity in entities)
            {
                _session.Delete(entity);
            }
        }
    }
}
