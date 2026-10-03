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

        // ---- Path SPARQL (Fase 5a, §11): il Service legge e scrive triple. Fail-loud:
        // una risposta non-2xx solleva FusekiRequestException, mai un esito "quasi giusto". ----

        /// <summary>SELECT/ASK su <c>POST /{ds}/query</c>; ritorna i result JSON standard
        /// (application/sparql-results+json) grezzi — il parsing è del chiamante.</summary>
        Task<string> QueryAsync(string baseUri, string dataset, string sparql, string username, string passwordPlain);

        /// <summary>SPARQL Update (INSERT/DELETE) su <c>POST /{ds}/update</c>.</summary>
        Task UpdateAsync(string baseUri, string dataset, string sparqlUpdate, string username, string passwordPlain);

        /// <summary>Carica Turtle in un named graph via Graph Store Protocol
        /// (<c>POST /{ds}/data?graph=...</c>): append idempotente di triple.</summary>
        Task LoadGraphAsync(string baseUri, string dataset, string graphUri, string turtle, string username, string passwordPlain);
    }

    /// <summary>Richiesta SPARQL/GSP rifiutata dal server: status e corpo per diagnosi.</summary>
    public sealed class FusekiRequestException : Exception
    {
        public int StatusCode { get; }
        public string ResponseBody { get; }

        public FusekiRequestException(string operation, int statusCode, string responseBody)
            : base($"Fuseki {operation} fallita: HTTP {statusCode} — {Truncate(responseBody)}")
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        private static string Truncate(string s)
            => string.IsNullOrEmpty(s) ? "(nessun corpo)" : (s.Length <= 300 ? s : s.Substring(0, 300) + "…");
    }
}
