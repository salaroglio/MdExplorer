using System.Collections.Generic;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Services
{
    public interface ISearchService
    {
        Task<SearchResult> SearchAsync(string searchTerm, SearchType searchType = SearchType.All, int maxResults = 50);
        Task<List<FileSearchResult>> SearchFilesAsync(string searchTerm, int maxResults = 50);
        Task<List<LinkSearchResult>> SearchLinksAsync(string searchTerm, int maxResults = 50);
    }

    public enum SearchType
    {
        All,
        Files,
        Links
    }

    public class SearchResult
    {
        public List<FileSearchResult> Files { get; set; } = new List<FileSearchResult>();
        public List<LinkSearchResult> Links { get; set; } = new List<LinkSearchResult>();
        public int TotalFiles { get; set; }
        public int TotalLinks { get; set; }
        public string SearchTerm { get; set; }
        public long SearchDurationMs { get; set; }
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