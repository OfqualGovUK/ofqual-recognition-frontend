namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class FileUploadViewModel
{
    /// <summary>
    /// The label shown above the file input.
    /// </summary>
    public required TextWithSizeViewModel Label { get; set; }

    /// <summary>
    /// The name of the field used for form submission.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Whether multiple files can be uploaded.
    /// </summary>
    public bool AllowMultiple { get; set; } = true;

    /// <summary>
    /// The section name for the review page.
    /// </summary>
    public string? SectionName { get; set; }

    /// <summary>
    /// Validation rules for the file upload.
    /// </summary>
    public ValidationRuleViewModel? Validation { get; set; }
}
