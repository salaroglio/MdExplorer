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
        /// <summary>
        /// Runs a JQL search via /rest/api/3/search/jql (the GET form). Each row can carry
        /// custom fields: pass <paramref name="customFieldSelect"/> (names or customfield_ ids)
        /// to request specific ones, or null to include all of the site's populated custom fields.
        /// </summary>
        Task<IReadOnlyList<JiraIssueSummary>> SearchAsync(
            JiraConnection conn, string jql, int maxResults,
            IReadOnlyList<string> customFieldSelect = null, CancellationToken ct = default);

        /// <summary>Fetches one issue with the fields needed for planning.</summary>
        Task<JiraIssueDetail> GetIssueAsync(
            JiraConnection conn, string issueKey, CancellationToken ct = default);

        /// <summary>
        /// Calls /rest/api/3/myself to validate credentials. Throws
        /// <see cref="AtlassianApiException"/> on failure (401 = bad token).
        /// </summary>
        Task<JiraMyself> VerifyAsync(JiraConnection conn, CancellationToken ct = default);

        /// <summary>
        /// Creates an issue (POST /rest/api/3/issue) and, when
        /// <see cref="JiraCreateIssueRequest.AssignToSelf"/> is set, assigns it to
        /// the calling user. This is the one WRITE operation; it exists mainly so
        /// the agent can seed/triage work. Returns the created key + browse URL.
        /// </summary>
        Task<JiraCreatedIssue> CreateIssueAsync(
            JiraConnection conn, JiraCreateIssueRequest req, CancellationToken ct = default);

        /// <summary>Lists the Jira projects the user can see (key + name).</summary>
        Task<IReadOnlyList<JiraProject>> ListProjectsAsync(JiraConnection conn, CancellationToken ct = default);

        /// <summary>Adds a comment (plain text wrapped to ADF). Returns the comment id.</summary>
        Task<string> AddCommentAsync(JiraConnection conn, string issueKey, string body, CancellationToken ct = default);

        /// <summary>Edits an existing issue's fields (only the provided ones).</summary>
        Task UpdateIssueAsync(JiraConnection conn, string issueKey, JiraUpdateIssueRequest req, CancellationToken ct = default);

        /// <summary>Lists the workflow transitions currently available for an issue.</summary>
        Task<IReadOnlyList<JiraTransition>> GetTransitionsAsync(JiraConnection conn, string issueKey, CancellationToken ct = default);

        /// <summary>
        /// Discovers a project's workflow: the statuses (stages) available to each
        /// issue type, with their category (To Do / In Progress / Done).
        /// </summary>
        Task<IReadOnlyList<JiraIssueTypeStatuses>> GetProjectStatusesAsync(JiraConnection conn, string projectKey, CancellationToken ct = default);

        /// <summary>
        /// Applies a transition by name or target-status (case-insensitive). Throws
        /// with the available options if none matches. Returns the resulting status.
        /// </summary>
        Task<string> TransitionIssueAsync(JiraConnection conn, string issueKey, string transition, CancellationToken ct = default);

        /// <summary>
        /// Searches users by name or email (/rest/api/3/user/search). Used to resolve a
        /// human name to the accountId Jira Cloud needs to assign an issue. Requires the
        /// "Browse users" global permission on the token's account.
        /// </summary>
        Task<IReadOnlyList<JiraUser>> SearchUsersAsync(JiraConnection conn, string query, int maxResults, CancellationToken ct = default);

        /// <summary>
        /// Sets the assignee of an issue (PUT /rest/api/3/issue/{key}/assignee). Pass a
        /// null/empty accountId to unassign.
        /// </summary>
        Task AssignIssueAsync(JiraConnection conn, string issueKey, string accountId, CancellationToken ct = default);
    }
}
