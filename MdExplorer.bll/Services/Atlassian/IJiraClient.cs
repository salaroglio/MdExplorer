using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Thin read-only client over the Jira Cloud REST API v3. All calls are
    /// authenticated per-invocation with Basic auth (email + API token) because
    /// credentials are per-user and per-project — there is no shared, long-lived
    /// authenticated client.
    /// </summary>
    public interface IJiraClient
    {
        /// <summary>Runs a JQL search via /rest/api/3/search/jql (the GET form).</summary>
        Task<IReadOnlyList<JiraIssueSummary>> SearchAsync(
            JiraConnection conn, string jql, int maxResults, CancellationToken ct = default);

        /// <summary>Fetches one issue with the fields needed for planning.</summary>
        Task<JiraIssueDetail> GetIssueAsync(
            JiraConnection conn, string issueKey, CancellationToken ct = default);

        /// <summary>
        /// Calls /rest/api/3/myself to validate credentials. Throws
        /// <see cref="AtlassianApiException"/> on failure (401 = bad token).
        /// </summary>
        Task<JiraMyself> VerifyAsync(JiraConnection conn, CancellationToken ct = default);
    }
}
