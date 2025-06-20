namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class CheckBoxGroupViewModel
{
    /// <summary>
    /// The heading shown above the checkboxes.
    /// </summary>
    public required TextWithSizeViewModel Heading { get; set; }

    /// <summary>
    /// Hint text shown below the heading.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// A unique name used for the checkbox group.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The display name for the section shown on the review page.
    /// </summary>
    public string? SectionName { get; set; }

    /// <summary>
    /// A list of selectable checkbox options.
    /// </summary>
    public required List<CheckBoxItemViewModel> Options { get; set; }
    
    /// <summary>
    /// Validation rules applied to the checkbox group.
    /// </summary>
    public ValidationRuleViewModel? Validation { get; set; }
}