using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Services
{
    public interface ISearchService
    {
        /// <param name="projectPath">Root of the open project; required for Content/All
        /// searches to locate the side-car FTS database. Empty → Contents stays empty.</param>
        Task<SearchResult> SearchAsync(string searchTerm, SearchType searchType = SearchType.All, int maxResults = 50, string projectPath = null);
        Task<List<FileSearchResult>> SearchFilesAsync(string searchTerm, int maxResults = 50);
        Task<List<LinkSearchResult>> SearchLinksAsync(string searchTerm, int maxResults = 50);
        Task<List<ContentSearchResult>> SearchContentAsync(string searchTerm, string projectPath, int maxResults = 50);
    }

    public enum SearchType
    {
        All,
        Files,
        Links,
        Content
    }

    public class SearchResult
    {
        public List<FileSearchResult> Files { get; set; } = new List<FileSearchResult>();
        public List<LinkSearchResult> Links { get; set; } = new List<LinkSearchResult>();
        public List<ContentSearchResult> Contents { get; set; } = new List<ContentSearchResult>();
        public int TotalFiles { get; set; }
        public int TotalLinks { get; set; }
        public int TotalContents { get; set; }
        public string SearchTerm { get; set; }
        public long SearchDurationMs { get; set; }
    }

    public class ContentSearchResult
    {
        public System.Guid MarkdownFileId { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        /// <summary>HTML-escaped excerpt with &lt;mark&gt;…&lt;/mark&gt; around matches.</summary>
        public string Snippet { get; set; }
        /// <summary>bm25 score: lower is more relevant.</summary>
        public double Score { get; set; }
    }

    public class FileSearchResult
    {
        public System.Guid Id { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public string FileType { get; set; }
        public string MatchedField { get; set; }
        public string HighlightedText { get; set; }
    }

    public class LinkSearchResult
    {
        public System.Guid Id { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public string MdTitle { get; set; }
        public string HtmlTitle { get; set; }
        public string MdContext { get; set; }
        public string Source { get; set; }
        public string LinkedCommand { get; set; }
        public string MarkdownFileName { get; set; }
        public string MarkdownFilePath { get; set; }
        public string MatchedField { get; set; }
        public string HighlightedText { get; set; }
    }
}