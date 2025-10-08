using Ad.Tools.Dal.Evo.Abstractions;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ad.Tools.Dal.Evo
{
    /// <summary>
    /// Implementazione dell'Unit of Work pattern utilizzando NHibernate.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISessionFactory _sessionFactory;
        private ISession _session;
        private ITransaction? _transaction;
        private Dictionary<Type, object>? _repositories;
        private bool _disposed;

        public UnitOfWork(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
            // Apri la sessione qui o lazy-load in GetSession()
            _session = _sessionFactory.OpenSession();
        }

        // Proprietà per accedere alla sessione (opzionale, potrebbe non essere pubblica)
        // public ISession Session => _session;

        public IRepository<T> GetRepository<T>() where T : class
        {
            _repositories ??= new Dictionary<Type, object>();
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                // Crea un'istanza generica del repository
                var repositoryType = typeof(Repository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _session);
                if (repositoryInstance != null)
                {
                    _repositories.Add(type, repositoryInstance);
                }
                else
                {
                    throw new InvalidOperationException($"Impossibile creare il repository per {typeof(T)}");
                }
            }

            return (IRepository<T>)_repositories[type];
        }

        public void BeginTransaction()
        {
            if (_transaction != null && _transaction.IsActive)
            {
                // Potresti decidere di fare join alla transazione esistente o lanciare eccezione
                // throw new InvalidOperationException("Una transazione è già attiva.");
                return; // Già in transazione
            }
            _transaction = _session.BeginTransaction();
        }

        public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction == null || !_transaction.IsActive)
                {
                    // Se non c'è transazione esplicita, potresti avviarne una implicitamente
                    // o lanciare un'eccezione se si richiede una transazione esplicita.
                    // Qui assumiamo che il commit avvenga anche senza BeginTransaction esplicito.
                    // throw new InvalidOperationException("Nessuna transazione attiva da committare.");
                    await _session.FlushAsync(cancellationToken); // Flush per sincronizzare le modifiche
                    return 0; // O un valore che indichi successo senza transazione esplicita
                }

                await _transaction.CommitAsync(cancellationToken);
                // Il numero di oggetti salvati non è direttamente restituito da CommitAsync in NH.
                // Potrebbe richiedere logica aggiuntiva o essere approssimato a 0/1.
                // Spesso si ritorna 0 o 1 per indicare successo.
                return 1;
            }
            catch (Exception)
            {
                // Se il commit fallisce, esegui il rollback
                Rollback(); // Non usare await qui se Rollback non è async
                throw; // Rilancia l'eccezione originale
            }
            finally
            {
                 // Dopo il commit, la transazione è completata. Potresti volerla resettare.
                 // _transaction?.Dispose(); // Dispose della transazione dopo commit/rollback
                 // _transaction = null;
                 // Oppure, se BeginTransaction deve essere chiamato di nuovo per la prossima op:
                 if (_transaction != null)
                 {
                    _transaction.Dispose();
                    _transaction = null;
                 }
            }
        }

        public void Rollback()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Esegui il rollback se c'è una transazione attiva non committata
                try
                {
                    if (_transaction != null && _transaction.IsActive)
                    {
                        _transaction.Rollback();
                    }
                }
                finally
                {
                    _transaction?.Dispose();
                    _session?.Dispose(); // Chiudi e rilascia la sessione NHibernate
                }
            }

            _repositories = null; // Rilascia riferimenti ai repository
            _disposed = true;
        }

        // Finalizer (opzionale, solo se hai risorse non gestite direttamente)
        // ~UnitOfWork()
        // {
        //     Dispose(false);
        // }
    }
}
