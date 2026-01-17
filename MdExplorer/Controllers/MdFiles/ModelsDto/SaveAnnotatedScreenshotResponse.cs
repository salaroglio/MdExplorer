/// <summary>
/// Response model for the SaveAnnotatedScreenshot endpoint.
/// </summary>
public class SaveAnnotatedScreenshotResponse
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Relative path to the saved original image (for rollback).
    /// </summary>
    public string OriginalImagePath { get; set; }

    /// <summary>
    /// Relative path to the saved annotated image (displayed in markdown).
    /// </summary>
    public string AnnotatedImagePath { get; set; }

    /// <summary>
    /// The markdown content that was inserted into the document.
    /// Includes the image reference and numbered list of descriptions.
    /// </summary>
    public string InsertedMarkdown { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string ErrorMessage { get; set; }
}
