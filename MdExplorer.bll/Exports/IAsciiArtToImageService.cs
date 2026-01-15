using System.Threading.Tasks;

namespace MdExplorer.Features.Exports
{
    /// <summary>
    /// Service to convert ASCII art code blocks to images for Word export.
    /// ASCII art with box-drawing characters (┌─│└┐┘├┤▼) renders poorly in Word,
    /// so we convert these blocks to PNG images before Pandoc processing.
    /// </summary>
    public interface IAsciiArtToImageService
    {
        /// <summary>
        /// Processes markdown content and converts ASCII art code blocks to images.
        /// </summary>
        /// <param name="markdown">The markdown content to process</param>
        /// <param name="projectPath">The project root path for saving images</param>
        /// <param name="filePath">The source file path (for naming images)</param>
        /// <returns>Markdown with ASCII art blocks replaced by image references</returns>
        Task<string> ConvertAsciiArtToImagesAsync(string markdown, string projectPath, string filePath);
    }
}
