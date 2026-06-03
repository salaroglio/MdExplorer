using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Builds the JQL queries used by the triage workflow. Kept as pure static
    /// functions so they can be unit-tested without any HTTP/Jira dependency.
    /// </summary>
    public static class JqlBuilder
    {
        /// <summary>
        /// Open issues assigned to the calling user, most urgent first.
        /// "Open" = not in the Done status category (statusCategory != Done),
        /// which is resolution-state agnostic across custom workflows.
        /// Ordered by priority (desc) then due date (asc, soonest first).
        /// When <paramref name="projectKeys"/> are supplied the search is scoped
        /// to those projects.
        /// </summary>
        public static string MyOpenIssuesByUrgency(IEnumerable<string> projectKeys)
        {
            var clauses = new List<string>
            {
                "assignee = currentUser()",
                "statusCategory != Done"
            };

            var keys = (projectKeys ?? Enumerable.Empty<string>())
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim())
                .ToList();

            if (keys.Count > 0)
            {
                var inList = string.Join(", ", keys.Select(k => "\"" + k.Replace("\"", "\\\"") + "\""));
                clauses.Add($"project IN ({inList})");
            }

            return string.Join(" AND ", clauses) + " ORDER BY priority DESC, duedate ASC";
        }
    }
}
