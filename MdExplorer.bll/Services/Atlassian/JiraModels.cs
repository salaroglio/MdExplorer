using System.Collections.Generic;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Connection coordinates for a single Jira Cloud site. Built per-call from
    /// the project's shared config (.development.yml: base url) and the user's
    /// own credentials (UserDB: email + decrypted API token). The token is never
    /// persisted here — this object lives only for the duration of one request.
    /// </summary>
    public class JiraConnection
    {
        public string BaseUrl { get; set; }
        public string Email { get; set; }
        public string ApiToken { get; set; }
    }

    /// <summary>Trimmed view of a Jira issue for triage lists.</summary>
    public class JiraIssueSummary
    {
        public string Key { get; set; }
        public string Summary { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string IssueType { get; set; }
        public string DueDate { get; set; }
        public string Assignee { get; set; }
        public string Url { get; set; }
        /// <summary>Short description snippet (truncated) in lists; full text in <see cref="JiraIssueDetail"/>.</summary>
        public string Description { get; set; }
    }

    /// <summary>Full (but still trimmed) view of one issue for planning.</summary>
    public class JiraIssueDetail : JiraIssueSummary
    {
        public string Reporter { get; set; }
        public List<string> Labels { get; set; } = new List<string>();
        public List<JiraComment> Comments { get; set; } = new List<JiraComment>();
        public List<string> Links { get; set; } = new List<string>();
    }

    public class JiraComment
    {
        public string Author { get; set; }
        public string Created { get; set; }
        public string Body { get; set; }
    }

    /// <summary>Identity returned by /rest/api/3/myself — used by test-connection.</summary>
    public class JiraMyself
    {
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
    }

    /// <summary>Input for creating an issue. Plain-text Description is wrapped into ADF.</summary>
    public class JiraCreateIssueRequest
    {
        public string ProjectKey { get; set; }
        public string Summary { get; set; }
        public string IssueType { get; set; }   // default "Task"
        public string Description { get; set; } // plain text -> ADF
        public string Priority { get; set; }    // optional, e.g. "High"
        public string DueDate { get; set; }     // optional, "yyyy-MM-dd"
        public bool AssignToSelf { get; set; } = true;
    }

    public class JiraCreatedIssue
    {
        public string Key { get; set; }
        public string Url { get; set; }
    }

    /// <summary>A Jira project as returned by /rest/api/3/project/search.</summary>
    public class JiraProject
    {
        public string Key { get; set; }
        public string Name { get; set; }
    }

    /// <summary>Fields to change on an existing issue. Only non-null/non-blank ones are sent.</summary>
    public class JiraUpdateIssueRequest
    {
        public string Summary { get; set; }
        public string Description { get; set; }   // plain text -> ADF
        public string Priority { get; set; }
        public string DueDate { get; set; }
    }

    /// <summary>An available workflow transition for an issue.</summary>
    public class JiraTransition
    {
        public string Id { get; set; }
        public string Name { get; set; }        // transition name, e.g. "Start Progress"
        public string ToStatus { get; set; }    // resulting status, e.g. "In Progress"
    }
}
