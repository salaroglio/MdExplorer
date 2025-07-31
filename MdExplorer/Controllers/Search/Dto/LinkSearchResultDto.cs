using System;

namespace MdExplorer.Service.Controllers.Search.Dto
{
    public class LinkSearchResultDto
    {
        public Guid Id { get; set; }
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