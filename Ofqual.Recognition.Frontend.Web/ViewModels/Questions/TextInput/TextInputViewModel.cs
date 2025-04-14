namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TextInputViewModel
{
    /// <summary>
    /// The collection of text input fields in the group.
    /// </summary>
    public List<TextInputItemViewModel> TextInputs { get; set; }

    /// <summary>
    /// The display name for the section shown on the review page.
    /// </summary>
    public string? SectionName { get; set; }
}