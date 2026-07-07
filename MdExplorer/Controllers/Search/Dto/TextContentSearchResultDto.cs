namespace MdExplorer.Service.Controllers.Search.Dto
{
    /// <summary>
    /// One full-text match inside a NON-markdown text file (separate text index).
    /// Distinct DTO from <see cref="ContentSearchResultDto"/> so the markdown search
    /// contract stays untouched. FullPath is the absolute path (for opening in the IDE).
    /// </summary>
    public class TextContentSearchResultDto
    {
        public System.Guid TextFileId { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public string Snippet { get; set; }
        public double Score { get; set; }
    }
}
