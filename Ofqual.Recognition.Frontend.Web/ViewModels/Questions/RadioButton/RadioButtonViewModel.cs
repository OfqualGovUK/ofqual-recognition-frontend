namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class RadioButtonViewModel
{
    /// <summary>
    /// The heading shown above the radio buttons.
    /// </summary>
    public TextWithSizeViewModel? Heading { get; set; }

    /// <summary>
    /// Hint text shown below the heading.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// Paragraph text shown below the heading.
    /// </summary>
    public string? Paragraph { get; set; }

    /// <summary>
    /// A unique name used for the group of radio buttons.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The display name for the section shown on the review page.
    /// </summary>
    public string? SectionName { get; set; }

    /// <summary>
    /// Validation rules applied to the radio buttons group.
    /// </summary>
    public ValidationRuleViewModel? Validation { get; set; }

    /// <summary>
    /// A list of individual radio button items to render.
    /// </summary>
    public required List<RadioButtonItemViewModel> Radios { get; set; }
}