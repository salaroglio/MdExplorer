namespace MdExplorer.Models.MarkSearch
{
    public class SaveMarkSearchAnswerRequest
    {
        // Nullable to bypass the implicit [Required] of non-nullable strings
        // (ValidationProblemDetails would not be parsed by the client's catchError);
        // null/empty is rejected manually with an actionable message.
        public string? Content { get; set; }
    }

    public class SaveMarkSearchAnswerResponse
    {
        public string RelativePath { get; set; }
        public string FullPath { get; set; }
        public string FileName { get; set; }
    }
}
