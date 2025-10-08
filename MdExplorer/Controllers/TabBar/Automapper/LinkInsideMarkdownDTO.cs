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
        public string MdContext { get; set; }
        public string LinkType { get {
                switch (Source)
                {
                    case "WorkLinkFromMarkdown":
                        return  "link";
                        break;
                    case "WorkLinkMdShowMd":
                        return "Publication";
                        break;
                    case "WorkLinkMdShowH2":
                        return "Excerpt";
                        break;
                }
                return string.Empty;
            } }
        public string MdTitle { get; set; }
        public string HTMLTitle { get; set; }
        public string LinkedCommand { get; set; }

        // Include the MarkdownFile property here.
        public MarkdownFileDto MarkdownFile { get; set; }
    }
}
