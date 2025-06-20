namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TextInputItemViewModel
{
    /// <summary>
    /// The label displayed above the input field.
    /// </summary>
    public required string Label { get; set; }
    
    /// <summary>
    /// Hint text displayed below the label.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// The name of the field.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Whether the input is disabled.
    /// </summary>
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// The input type, e.g. text, number, or email.
    /// </summary>
    public string InputType { get; set; } = "text";

    /// <summary>
    /// Validation rules applied to the input field.
    /// </summary>
    public ValidationRuleViewModel? Validation { get; set; }
}
