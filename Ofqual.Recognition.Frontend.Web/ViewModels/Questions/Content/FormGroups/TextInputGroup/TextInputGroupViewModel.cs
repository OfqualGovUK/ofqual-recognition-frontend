namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TextInputGroupViewModel
{
    /// <summary>
    /// The heading displayed above the group of text inputs.
    /// </summary>
    public TextWithSizeViewModel? Heading { get; set; }

    /// <summary>
    /// A list of text input fields to render.
    /// </summary>
    public required List<TextInputItemViewModel> Fields { get; set; }

    /// <summary>
    /// The section name shown on the review page.
    /// </summary>
    public string? SectionName { get; set; }
}
