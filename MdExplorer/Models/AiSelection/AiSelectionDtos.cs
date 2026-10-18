namespace MdExplorer.Models.AiSelection
{
    public class GetMarkdownFragmentResponse
    {
        public string Fragment { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int TotalLines { get; set; }
        public string LineEnding { get; set; } // "crlf" | "lf"
    }

    public class ReplaceMarkdownSectionRequest
    {
        public string Path { get; set; }
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        // Fragment as returned by GET fragment ("\n"-normalized): re-verified against
        // the file on disk right before writing; on mismatch the request is rejected.
        // Nullable to bypass the implicit [Required] (which rejects empty strings);
        // null is rejected manually, "" stays legal ("" in NewText = delete the lines).
        public string? ExpectedOriginalText { get; set; }
        public string? NewText { get; set; }
        public string? ConnectionId { get; set; }
    }
}
