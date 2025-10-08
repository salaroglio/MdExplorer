using Ad.Tools.Dal.Evo.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using System;
using System.Reflection;

namespace Ad.Tools.Dal.Evo
{
    /// <summary>
    /// Extension methods for setting up DAL Evo services in an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class DalEvoServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the necessary services for Ad.Tools.Dal.Evo to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="mappingAssembly">The assembly containing FluentNHibernate mappings.</param>
        /// <param name="updateSchema">Flag indicating whether to update the database schema (use with caution).</param>
        /// <param name="useSqlite">Flag indicating whether to use SQLite configuration.</param>
        /// <param name="dbFilePath">The path to the SQLite database file (required if useSqlite is true).</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">Thrown if services or mappingAssembly are null.</exception>
        /// <exception cref="ArgumentException">Thrown if connectionString (or dbFilePath if useSqlite is true) is null or empty.</exception>
        public static IServiceCollection AddDalEvoFeatures(
            this IServiceCollection services,
            string connectionString,
            Assembly mappingAssembly,
            bool updateSchema = false,
            bool useSqlite = false,
            string? dbFilePath = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (mappingAssembly == null) throw new ArgumentNullException(nameof(mappingAssembly));

            if (useSqlite)
            {
                if (string.IsNullOrEmpty(dbFilePath))
                {
                     throw new ArgumentException("dbFilePath cannot be null or empty when useSqlite is true.", nameof(dbFilePath));
                }
                // Register ISessionFactory as Singleton for SQLite
                services.AddSingleton<ISessionFactory>(sp =>
                    NHibernateConfigurator.BuildSqliteSessionFactory(dbFilePath, mappingAssembly, updateSchema));
            }
            else
            {
                 if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentException("Connection string cannot be null or empty unless useSqlite is true.", nameof(connectionString));
                }
                // Register ISessionFactory as Singleton for other DBs (e.g., SQL Server)
                services.AddSingleton<ISessionFactory>(sp =>
                    NHibernateConfigurator.BuildSessionFactory(connectionString, mappingAssembly, updateSchema));
            }


            // Register IUnitOfWork as Scoped
            // A new UnitOfWork (and ISession) is created per scope (e.g., per web request)
            services.AddScoped<IUnitOfWork, UnitOfWork>(sp =>
            {
                var sessionFactory = sp.GetRequiredService<ISessionFactory>();
                // Pass the ISessionFactory to the UnitOfWork constructor
                // The UnitOfWork will open its own ISession
                return new UnitOfWork(sessionFactory);
            });

            // Register IRepository<> as Transient
            // A new Repository is created each time it's requested.
            // It will resolve the Scoped IUnitOfWork (or ISession if injected directly)
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            // Note: The Repository<T> constructor takes ISession.
            // We need to ensure it gets the ISession managed by the Scoped UnitOfWork.
            // One way is to inject IUnitOfWork into services that need repositories,
            // and then use unitOfWork.GetRepository<T>().
            // Alternatively, adjust Repository<T> to take IUnitOfWork, or register ISession as Scoped.

            // Let's adjust Repository<T> to take ISession, but register ISession as Scoped,
            // resolved from the IUnitOfWork to ensure it's the same session.
            services.AddScoped<ISession>(sp =>
            {
                 // This is tricky. We want the ISession managed by the *current* Scoped IUnitOfWork.
                 // Directly resolving IUnitOfWork here might create a new one if called outside its scope.
                 // A common pattern is to have UnitOfWork expose its Session,
                 // or have Repository take IUnitOfWork instead of ISession.

                 // Let's stick to the plan: Repository takes ISession.
                 // We need the UnitOfWork to manage the session lifecycle.
                 // The DI container needs to provide the *same* ISession instance
                 // to the UnitOfWork and any Repositories within the same scope.

                 // Option A: Inject IUnitOfWork into Repository<T> (Requires changing Repository<T> constructor)
                 // Option B: Register ISession as Scoped, opened by UnitOfWork.

                 // Let's try registering ISession as Scoped, but ensure it's managed correctly.
                 // The UnitOfWork already opens a session. We need to provide *that* session.

                 // Revisit: The current UnitOfWork opens its own session.
                 // The Repository needs *that* session.
                 // The simplest way is often:
                 // 1. Inject IUnitOfWork into the service/controller.
                 // 2. Call unitOfWork.GetRepository<T>() which internally passes its session.
                 // This avoids needing to register ISession separately.

                 // Let's assume the usage pattern will be injecting IUnitOfWork and calling GetRepository<T>.
                 // Therefore, the AddTransient registration for IRepository<> is sufficient,
                 // as the UnitOfWork will handle providing the correct session when GetRepository<T> is called.
                 // We don't need to register ISession separately in the container for this pattern.

                 // --> Keep AddTransient(typeof(IRepository<>), typeof(Repository<>));
                 // --> Ensure Repository<T> constructor takes ISession session (as it currently does).
                 // --> Ensure UnitOfWork.GetRepository<T>() correctly passes its managed ISession
                 //     to the Repository instance it creates (which it currently does via Activator.CreateInstance).

                 // No separate ISession registration needed if using unitOfWork.GetRepository<T>() pattern.
                 return null; // Placeholder, this registration isn't needed with the GetRepository pattern.
            });
             // Remove the unnecessary ISession registration attempt:
             services.Remove(ServiceDescriptor.Scoped<ISession>(sp => null!));


            return services;
        }
    }
}
