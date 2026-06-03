using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class Neo4jConnectionPool : INeo4jConnectionPool, IDisposable
    {
        private readonly ConcurrentDictionary<Guid, IDriver> _drivers = new();
        private readonly ILogger<Neo4jConnectionPool> _logger;

        public Neo4jConnectionPool(ILogger<Neo4jConnectionPool> logger)
        {
            _logger = logger;
        }

        public IDriver GetOrCreateDriver(Guid projectId, string uri, string username, string passwordPlain)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("Neo4j URI is required", nameof(uri));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Neo4j username is required", nameof(username));

            return _drivers.GetOrAdd(projectId, _ =>
            {
                _logger.LogInformation("[Neo4jConnectionPool] Creating driver for project {ProjectId} at {Uri}", projectId, uri);
                return GraphDatabase.Driver(uri, AuthTokens.Basic(username, passwordPlain ?? string.Empty));
            });
        }

        public void Invalidate(Guid projectId)
        {
            if (_drivers.TryRemove(projectId, out var driver))
            {
                _logger.LogInformation("[Neo4jConnectionPool] Invalidating driver for project {ProjectId}", projectId);
                try { driver.Dispose(); }
                catch (Exception ex) { _logger.LogWarning(ex, "[Neo4jConnectionPool] Error disposing driver for {ProjectId}", projectId); }
            }
        }

        public async Task<bool> TestConnectionAsync(string uri, string username, string passwordPlain, string database)
        {
            IDriver driver = null;
            try
            {
                driver = GraphDatabase.Driver(uri, AuthTokens.Basic(username, passwordPlain ?? string.Empty));
                await driver.VerifyConnectivityAsync();
                await using var session = driver.AsyncSession(b =>
                {
                    if (!string.IsNullOrWhiteSpace(database))
                        b.WithDatabase(database);
                    b.WithDefaultAccessMode(AccessMode.Read);
                });
                var result = await session.RunAsync("RETURN 1 AS ok");
                var record = await result.SingleAsync();
                return record["ok"].As<int>() == 1;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Neo4jConnectionPool] TestConnection failed for {Uri}", uri);
                return false;
            }
            finally
            {
                if (driver != null) await driver.DisposeAsync();
            }
        }

        public void Dispose()
        {
            foreach (var kvp in _drivers)
            {
                try { kvp.Value.Dispose(); }
                catch { /* swallow on shutdown */ }
            }
            _drivers.Clear();
        }
    }
}
