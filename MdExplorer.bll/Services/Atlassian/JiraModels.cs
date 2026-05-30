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
    }

    /// <summary>Full (but still trimmed) view of one issue for planning.</summary>
    public class JiraIssueDetail : JiraIssueSummary
    {
        public string Description { get; set; }
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
}
