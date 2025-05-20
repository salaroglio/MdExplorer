using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Evo.Abstractions
{
    /// <summary>
    /// Definisce il contratto per il pattern Unit of Work.
    /// Gestisce il ciclo di vita della sessione/transazione e fornisce accesso ai repository.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Ottiene un'istanza del repository per il tipo di entità specificato.
        /// </summary>
        /// <typeparam name="T">Il tipo dell'entità.</typeparam>
        /// <returns>Un'istanza di IRepository<T>.</returns>
        IRepository<T> GetRepository<T>() where T : class;

        /// <summary>
        /// Salva tutte le modifiche apportate nel contesto dell'Unit of Work al database sottostante.
        /// </summary>
        /// <param name="cancellationToken">Token per la cancellazione.</param>
        /// <returns>Il numero di oggetti scritti nel database.</returns>
        Task<int> CommitAsync(CancellationToken cancellationToken = default);

        // --- Gestione Transazione Esplicita (Opzionale ma consigliata) ---

        /// <summary>
        /// Avvia una nuova transazione di database. Se una transazione è già attiva, potrebbe lanciare un'eccezione o unirsi ad essa a seconda dell'implementazione.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Annulla la transazione corrente, annullando tutte le modifiche apportate dall'inizio della transazione.
        /// </summary>
        void Rollback();
    }
}
