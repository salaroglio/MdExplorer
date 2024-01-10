using System;

namespace MdExplorer.Service.Controllers.TabBar.Automapper
{
    public class LinkInsideMarkdownDto
    {
        // Include the properties you want to serialize here.
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string FullPath { get; set; }
        public string Source { get; set; }
        public int? SectionIndex { get; set; }
        public string MdTitle { get; set; }
        public string HTMLTitle { get; set; }
        public string LinkedCommand { get; set; }

        // Include the MarkdownFile property here.
        public MarkdownFileDto MarkdownFile { get; set; }
    }
}
