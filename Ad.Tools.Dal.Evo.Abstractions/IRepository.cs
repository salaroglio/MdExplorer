using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Evo.Abstractions
{
    /// <summary>
    /// Interfaccia generica per un repository.
    /// </summary>
    /// <typeparam name="T">Il tipo dell'entità gestita dal repository.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Ottiene un'entità tramite il suo identificatore primario.
        /// </summary>
        /// <param name="id">L'identificatore primario.</param>
        /// <param name="cancellationToken">Token per la cancellazione.</param>
        /// <returns>L'entità trovata o null.</returns>
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ottiene tutte le entità.
        /// </summary>
        /// <param name="cancellationToken">Token per la cancellazione.</param>
        /// <returns>Una collezione di tutte le entità.</returns>
        Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Ottiene entità che soddisfano una specifica condizione.
        /// </summary>
        /// <param name="predicate">La condizione da soddisfare.</param>
        /// <param name="cancellationToken">Token per la cancellazione.</param>
        /// <returns>Una collezione di entità che soddisfano la condizione.</returns>
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ottiene un IQueryable per costruire query più complesse.
        /// </summary>
        /// <returns>Un IQueryable per l'entità T.</returns>
        IQueryable<T> Query();

        /// <summary>
        /// Aggiunge una nuova entità.
        /// </summary>
        /// <param name="entity">L'entità da aggiungere.</param>
        /// <param name="cancellationToken">Token per la cancellazione.</param>
        /// <returns>Task.</returns>
        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Aggiunge una collezione di nuove entità.
        /// </summary>
        /// <param name="entities">Le entità da aggiungere.</param>
        /// <param name="cancellationToken">Token per la cancellazione.</param>
        /// <returns>Task.</returns>
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Aggiorna un'entità esistente.
        /// </summary>
        /// <param name="entity">L'entità da aggiornare.</param>
        void Update(T entity); // L'aggiornamento spesso non è async in ORM come NH/EF

        /// <summary>
        /// Rimuove un'entità esistente.
        /// </summary>
        /// <param name="entity">L'entità da rimuovere.</param>
        void Delete(T entity); // La rimozione spesso non è async in ORM come NH/EF

        /// <summary>
        /// Rimuove una collezione di entità esistenti.
        /// </summary>
        /// <param name="entities">Le entità da rimuovere.</param>
        void DeleteRange(IEnumerable<T> entities);
    }
}
