namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TextFieldsViewModel
{    
    /// <summary>
     /// The name of the field.
     /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The label shown above the text box.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Hint text shown below the label.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// Whether the text field is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Validation rules for the text box.
    /// </summary>
    public ValidationRuleViewModel? Validation { get; set; }
}
