using System;
using System.Net;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Raised when a Jira/Confluence Cloud REST call fails. Carries the HTTP
    /// status so callers can map 401 to an actionable "regenerate your token"
    /// message. We throw instead of returning a degraded result so the failure
    /// is explicit (no silent fallbacks).
    /// </summary>
    public class AtlassianApiException : Exception
    {
        public HttpStatusCode? StatusCode { get; }

        /// <summary>True for 401/403 — credential problems the user must fix.</summary>
        public bool IsAuthFailure =>
            StatusCode == HttpStatusCode.Unauthorized || StatusCode == HttpStatusCode.Forbidden;

        public AtlassianApiException(string message, HttpStatusCode? statusCode = null, Exception inner = null)
            : base(message, inner)
        {
            StatusCode = statusCode;
        }
    }
}
