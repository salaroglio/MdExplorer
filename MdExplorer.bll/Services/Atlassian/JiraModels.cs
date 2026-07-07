using System.Collections.Generic;
using System.Text.Json.Nodes;

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
        /// <summary>Status category: "To Do" / "In Progress" / "Done" — where the issue sits in the flow.</summary>
        public string StatusCategory { get; set; }
        /// <summary>Short description snippet (truncated) in lists; full text in <see cref="JiraIssueDetail"/>.</summary>
        public string Description { get; set; }

        /// <summary>
        /// Custom fields that have a value, keyed by their human name (e.g. "Story Points")
        /// with the value flattened to a readable scalar/list. Empty unless the issue has
        /// custom fields set (and, for search, unless they were requested). See <see cref="JiraFieldMeta"/>.
        /// </summary>
        public Dictionary<string, object> CustomFields { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>Full (but still trimmed) view of one issue for planning.</summary>
    public class JiraIssueDetail : JiraIssueSummary
    {
        public string Reporter { get; set; }
        public List<string> Labels { get; set; } = new List<string>();
        public List<JiraComment> Comments { get; set; } = new List<JiraComment>();
        public List<string> Links { get; set; } = new List<string>();
        // CustomFields is inherited from JiraIssueSummary.
    }

    /// <summary>
    /// One field definition from /rest/api/3/field. The bridge between a human field
    /// name ("Story Points") and Jira's opaque id ("customfield_10016"), plus the
    /// schema needed to shape a value on write (option → {value}, user → {accountId}, …).
    /// </summary>
    public class JiraFieldMeta
    {
        public string Id { get; set; }         // "customfield_10016" (custom) or a system id
        public string Name { get; set; }       // human name, e.g. "Story Points"
        public bool IsCustom { get; set; }
        public string SchemaType { get; set; } // string / number / option / user / array / date / datetime / …
        public string ItemsType { get; set; }  // for arrays: option / string / user / …
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

    /// <summary>
    /// A user candidate returned by /rest/api/3/user/search. Jira Cloud requires the
    /// opaque <see cref="AccountId"/> to assign an issue (name/email are not accepted),
    /// so this is the bridge between a human name and an assignment.
    /// </summary>
    public class JiraUser
    {
        public string AccountId { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }   // often hidden by the user's privacy settings
        public bool Active { get; set; }
        public string AccountType { get; set; }     // "atlassian" = a real, assignable person
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

        /// <summary>
        /// Optional custom fields, keyed by human name ("Story Points") or by the raw
        /// customfield_ id. Scalar values are shaped to Jira's expected JSON from the
        /// field's schema; a structured JSON value (object/array) is sent as-is.
        /// </summary>
        public JsonObject CustomFields { get; set; }
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

        /// <summary>
        /// Optional custom fields to change, keyed by human name ("Story Points") or by
        /// the raw customfield_ id. Same shaping rules as <see cref="JiraCreateIssueRequest.CustomFields"/>.
        /// </summary>
        public JsonObject CustomFields { get; set; }
    }

    /// <summary>An available workflow transition for an issue.</summary>
    public class JiraTransition
    {
        public string Id { get; set; }
        public string Name { get; set; }        // transition name, e.g. "Start Progress"
        public string ToStatus { get; set; }    // resulting status, e.g. "In Progress"
    }

    /// <summary>One status (stage) in a project's workflow.</summary>
    public class JiraStatus
    {
        public string Name { get; set; }        // e.g. "In Progress"
        public string Category { get; set; }    // "To Do" / "In Progress" / "Done"
    }

    /// <summary>The workflow stages available to an issue type within a project.</summary>
    public class JiraIssueTypeStatuses
    {
        public string IssueType { get; set; }
        public List<JiraStatus> Statuses { get; set; } = new List<JiraStatus>();
    }
}
