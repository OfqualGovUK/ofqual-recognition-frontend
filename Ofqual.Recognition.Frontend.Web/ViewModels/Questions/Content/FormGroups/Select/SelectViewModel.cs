namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class SelectViewModel
{
    /// <summary>
    /// The text label displayed above the select dropdown.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Hint text shown below the label.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// The field name for the select element.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Indicates whether the select dropdown is disabled.
    /// </summary>
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// The list of options shown in the dropdown.
    /// </summary>
    public required List<SelectOptionViewModel> Options { get; set; }

    /// <summary>
    /// Validation rules applied to the select dropdown.
    /// </summary>
    public ValidationRuleViewModel? Validation { get; set; }
}
