using System.Collections.Generic;

namespace MdExplorer.Service.Controllers.Search.Dto
{
    public class SearchResultDto
    {
        public List<FileSearchResultDto> Files { get; set; } = new List<FileSearchResultDto>();
        public List<LinkSearchResultDto> Links { get; set; } = new List<LinkSearchResultDto>();
        public List<ContentSearchResultDto> Contents { get; set; } = new List<ContentSearchResultDto>();
        public int TotalFiles { get; set; }
        public int TotalLinks { get; set; }
        public int TotalContents { get; set; }
        public string SearchTerm { get; set; }
        public long SearchDurationMs { get; set; }
    }
}