namespace MdExplorer.Service.Controllers.Search.Dto
{
    public class SearchRequestDto
    {
        public string SearchTerm { get; set; }
        public SearchType SearchType { get; set; } = SearchType.All;
        public int MaxResults { get; set; } = 50;
    }

    public enum SearchType
    {
        All,
        Files,
        Links
    }
}