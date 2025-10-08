using System;

namespace MdExplorer.Service.Controllers.Search.Dto
{
    public class FileSearchResultDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public string FileType { get; set; }
        public string MatchedField { get; set; }
        public string HighlightedText { get; set; }
    }
}