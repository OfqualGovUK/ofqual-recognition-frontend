namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class CheckBoxViewModel
{
    /// <summary>
    /// The heading shown above the checkboxes.
    /// </summary>
    public TextWithSizeViewModel? Heading { get; set; }

    /// <summary>
    /// Hint text shown below the heading.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// A unique name used for the checkbox group.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Validation rules applied to the checkbox group.
    /// </summary>
    public ValidationRuleViewModel? Validation { get; set; }

    /// <summary>
    /// A list of individual checkbox items to render.
    /// </summary>
    public required List<CheckBoxItemViewModel> CheckBoxes { get; set; }
}