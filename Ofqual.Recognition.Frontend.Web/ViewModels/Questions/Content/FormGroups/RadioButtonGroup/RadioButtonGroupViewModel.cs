namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class RadioButtonGroupViewModel
{
    /// <summary>
    /// The heading shown above the radio button group.
    /// </summary>
    public required TextWithSizeViewModel Heading { get; set; }

    /// <summary>
    /// Hint text displayed beneath the heading.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// Paragraph text displayed beneath the heading.
    /// </summary>
    public string? Paragraph { get; set; }

    /// <summary>
    /// The field name used for form submission and validation.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The list of individual radio button options.
    /// </summary>
    public required List<RadioButtonItemViewModel> Options { get; set; }

    /// <summary>
    /// The label used for this section on the review page.
    /// </summary>
    public string? SectionName { get; set; }

    /// <summary>
    /// Validation rules for the radio button group.
    /// </summary>
    public ValidationRuleViewModel? Validation { get; set; }
}
