/// <summary>
/// Represents a single marker description in the screenshot annotation wizard.
/// </summary>
public class MarkerDescriptionDto
{
    /// <summary>
    /// The sequential number of the marker (1, 2, 3, etc.)
    /// </summary>
    public int MarkerId { get; set; }

    /// <summary>
    /// The user-provided description for this marker
    /// </summary>
    public string Text { get; set; }
}
