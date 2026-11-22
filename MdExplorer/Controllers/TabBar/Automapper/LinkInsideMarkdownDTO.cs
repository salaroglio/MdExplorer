using System;
using MdExplorer.Abstractions.Entities.EngineDB;

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

        /// <summary>
        /// Dal collegamento del database, campo per campo (prima li copiava una libreria di mappatura, tolta con .NET 10): LinkType si calcola da Source,
        /// il file che contiene il collegamento diventa il suo DTO.
        /// </summary>
        public static LinkInsideMarkdownDto From(LinkInsideMarkdown link)
        {
            if (link == null) return null;
            return new LinkInsideMarkdownDto
            {
                Id = link.Id,
                Path = link.Path,
                FullPath = link.FullPath,
                Source = link.Source,
                SectionIndex = link.SectionIndex,
                MdContext = link.MdContext,
                MdTitle = link.MdTitle,
                HTMLTitle = link.HTMLTitle,
                LinkedCommand = link.LinkedCommand,
                MarkdownFile = MarkdownFileDto.From(link.MarkdownFile),
            };
        }
    }
}
