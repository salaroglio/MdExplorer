using System.Collections.Generic;

namespace MdExplorer.Features.Services.Atlassian
{
    /// <summary>
    /// Connection coordinates for a single Confluence Cloud site. Confluence lives
    /// on the SAME Atlassian site as Jira, under the /wiki path, and authenticates
    /// with the SAME per-user API token (Basic auth). <see cref="BaseUrl"/> is the
    /// Confluence base, i.e. {site}.atlassian.net/wiki. Built per-call; the token
    /// is never persisted on this object.
    /// </summary>
    public class ConfluenceConnection
    {
        /// <summary>Confluence base, e.g. https://acme.atlassian.net/wiki (note the /wiki).</summary>
        public string BaseUrl { get; set; }
        public string Email { get; set; }
        public string ApiToken { get; set; }
    }

    /// <summary>A Confluence space as returned by /wiki/api/v2/spaces.</summary>
    public class ConfluenceSpace
    {
        public string Id { get; set; }      // numeric id (string-typed; v2 returns it as a string)
        public string Key { get; set; }     // human key, e.g. "DEV"
        public string Name { get; set; }
        public string Type { get; set; }    // "global" / "personal"
    }

    /// <summary>One hit from a CQL search (/wiki/rest/api/search).</summary>
    public class ConfluenceSearchHit
    {
        public string Id { get; set; }       // content id (the page id for type=page)
        public string Type { get; set; }     // "page" / "blogpost" / ...
        public string Title { get; set; }
        public string SpaceKey { get; set; }
        public string Excerpt { get; set; }  // short snippet of the match (HTML stripped)
        public string Url { get; set; }      // absolute web URL
    }

    /// <summary>A single Confluence page with its body flattened to markdown.</summary>
    public class ConfluencePage
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string SpaceId { get; set; }
        public string Status { get; set; }   // "current" / "draft" / "archived"
        public int? Version { get; set; }
        /// <summary>Body flattened from ADF (atlas_doc_format) to markdown.</summary>
        public string Body { get; set; }
        public string Url { get; set; }      // absolute web URL
    }

    /// <summary>
    /// Input for creating a page. The body is markdown, converted to ADF before
    /// sending. Either SpaceId (numeric) or SpaceKey (human, resolved to id) must
    /// be provided.
    /// </summary>
    public class ConfluenceCreatePageRequest
    {
        public string SpaceKey { get; set; }   // optional if SpaceId given
        public string SpaceId { get; set; }     // optional if SpaceKey given
        public string Title { get; set; }
        public string MarkdownBody { get; set; }
        public string ParentId { get; set; }    // optional — place under a parent page
    }

    /// <summary>
    /// Input for updating a page. The body is markdown, converted to ADF. The
    /// current version is read first and incremented (Confluence optimistic lock).
    /// </summary>
    public class ConfluenceUpdatePageRequest
    {
        public string PageId { get; set; }
        public string Title { get; set; }        // optional — keep existing when null
        public string MarkdownBody { get; set; }
        public string VersionMessage { get; set; } // optional changelog note
    }

    /// <summary>Result of a create/update: the page id + browse URL.</summary>
    public class ConfluenceWriteResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int? Version { get; set; }
        public string Url { get; set; }
    }
}
