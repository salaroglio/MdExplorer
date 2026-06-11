using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Thin read-only client over the Confluence Cloud REST API. Spaces and pages
    /// use the v2 API (/wiki/api/v2/...); full-text search uses the v1 CQL endpoint
    /// (/wiki/rest/api/search) because v2 has no search API. All calls are Basic-auth
    /// authenticated per-invocation with the same per-user API token as Jira.
    /// </summary>
    public interface IConfluenceClient
    {
        /// <summary>Lists the spaces the user can see (id + key + name), first page only.</summary>
        Task<IReadOnlyList<ConfluenceSpace>> ListSpacesAsync(
            ConfluenceConnection conn, int limit = 50, CancellationToken ct = default);

        /// <summary>
        /// Runs a CQL search via /wiki/rest/api/search (v1). The caller passes a
        /// free-form CQL query; the agent translates natural language into CQL.
        /// </summary>
        Task<IReadOnlyList<ConfluenceSearchHit>> SearchAsync(
            ConfluenceConnection conn, string cql, int limit = 20, CancellationToken ct = default);

        /// <summary>
        /// Fetches one page (v2) with its body requested as atlas_doc_format (ADF)
        /// and flattened to markdown via <see cref="AdfRenderer"/>.
        /// </summary>
        Task<ConfluencePage> GetPageAsync(
            ConfluenceConnection conn, string pageId, CancellationToken ct = default);

        /// <summary>
        /// Creates a page (POST /wiki/api/v2/pages). The markdown body is converted
        /// to ADF. The space is given by key (resolved to id) or id directly.
        /// </summary>
        Task<ConfluenceWriteResult> CreatePageAsync(
            ConfluenceConnection conn, ConfluenceCreatePageRequest req, CancellationToken ct = default);

        /// <summary>
        /// Updates a page (PUT /wiki/api/v2/pages/{id}). Reads the current version
        /// first and increments it (Confluence optimistic locking). The markdown
        /// body is converted to ADF.
        /// </summary>
        Task<ConfluenceWriteResult> UpdatePageAsync(
            ConfluenceConnection conn, ConfluenceUpdatePageRequest req, CancellationToken ct = default);
    }
}
