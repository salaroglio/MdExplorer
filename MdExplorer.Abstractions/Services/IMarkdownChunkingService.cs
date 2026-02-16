using System.Collections.Generic;

namespace MdExplorer.Abstractions.Services
{
    public class MarkdownChunk
    {
        public string FilePath { get; set; }
        public string SectionTitle { get; set; }
        public string Content { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        /// <summary>
        /// "document" = markdown prose, "plantuml" = PlantUML diagram, "codeblock" = other code blocks
        /// </summary>
        public string ChunkType { get; set; } = "document";
        /// <summary>
        /// Groups chunks from the same heading section. Siblings share the same GroupId.
        /// </summary>
        public string GroupId { get; set; }
    }

    public interface IMarkdownChunkingService
    {
        List<MarkdownChunk> ChunkFile(string filePath, string content);
    }
}
