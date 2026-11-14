using Microsoft.AspNetCore.Http;

/// <summary>
/// Request model for saving an annotated screenshot with marker descriptions.
/// Sent as multipart/form-data from the frontend wizard.
/// </summary>
public class SaveAnnotatedScreenshotRequest
{
    /// <summary>
    /// The original screenshot image (PNG) without any annotations.
    /// Kept for rollback purposes.
    /// </summary>
    public IFormFile OriginalImage { get; set; }

    /// <summary>
    /// The annotated screenshot image (PNG) with numbered markers drawn on it.
    /// </summary>
    public IFormFile AnnotatedImage { get; set; }

    /// <summary>
    /// Full path to the markdown document where the screenshot will be inserted.
    /// </summary>
    public string DocumentPath { get; set; }

    /// <summary>
    /// Base name for the image files (without extension or timestamps).
    /// </summary>
    public string ImageName { get; set; }

    /// <summary>
    /// JSON-serialized array of marker descriptions.
    /// Format: [{"markerId": 1, "text": "Description"}, ...]
    /// </summary>
    public string DescriptionsJson { get; set; }

    /// <summary>
    /// SignalR connection ID for sending notifications back to the client.
    /// </summary>
    public string ConnectionId { get; set; }
    // The anchor, as TriggerPasteWizard handed it to the wizard. Absent = append at the end. All
    // nullable on purpose: a non-nullable field is an implicit [Required] here, and the save
    // without an anchor would turn into a 400 before entering the method.
    public int? AnchorStartLine { get; set; }
    public int? AnchorEndLine { get; set; }

    /// <summary><c>before</c> | <c>after</c>.</summary>
    public string? AnchorPosition { get; set; }

    /// <summary>The anchor block's lines as they were at the right-click; re-checked before writing.</summary>
    public string? AnchorExpectedText { get; set; }
}
