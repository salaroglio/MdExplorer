namespace MdExplorer.Service.Controllers.Search.Dto
{
    public class ContentSearchResultDto
    {
        public System.Guid MarkdownFileId { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Snippet { get; set; }
        public double Score { get; set; }
    }
}
