using System;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.KnowledgeGraph
{
    public class FusekiTestResult
    {
        public bool Success { get; set; }
        public bool ServerReachable { get; set; }
        public bool DatasetExists { get; set; }
        public long LatencyMs { get; set; }
        public string Error { get; set; }
    }

    public interface IFusekiClient
    {
        /// <summary>Verifica che il server risponda e che il dataset esista.</summary>
        Task<FusekiTestResult> TestAsync(string baseUri, string dataset, string username, string passwordPlain);

        /// <summary>Sanitizza un nome di progetto MDE per renderlo URL-safe come dataset Fuseki.
        /// Replace spazi/caratteri speciali con `_`, mantiene solo [A-Za-z0-9_-].</summary>
        string SanitizeDatasetName(string projectName);

        /// <summary>Crea il dataset sul server Fuseki (admin API: POST /$/datasets?dbName=...&dbType=tdb2).
        /// Idempotent: se il dataset esiste già, ritorna true senza errori.</summary>
        Task<bool> EnsureDatasetAsync(string baseUri, string dataset, string username, string passwordPlain);
    }
}
