using System;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public interface INeo4jConnectionPool
    {
        /// <summary>
        /// Returns the cached IDriver for the given project, or creates one using the provided
        /// credentials and caches it. Caller is responsible for fetching credentials from
        /// ProjectNeo4jSettings (UserDB) and decrypting the password (IPasswordProtector).
        /// </summary>
        IDriver GetOrCreateDriver(Guid projectId, string uri, string username, string passwordPlain);

        /// <summary>
        /// Drops the cached driver for the project (e.g. after settings change). The next
        /// GetOrCreateDriver call recreates it.
        /// </summary>
        void Invalidate(Guid projectId);

        /// <summary>
        /// Opens a transient driver, runs a trivial query, and closes it.
        /// Used by the "Test connection" button in project settings. Does NOT touch the cache.
        /// </summary>
        Task<bool> TestConnectionAsync(string uri, string username, string passwordPlain, string database);
    }
}
