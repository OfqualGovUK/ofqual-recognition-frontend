namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TextareaViewModel
{
    /// <summary>
    /// The label shown above the text box.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Hint text shown below the label.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// The name of the field
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// The number of rows shown in the text area.
    /// </summary>
    public int? Rows { get; set; } = 5;

    /// <summary>
    /// Whether to use the browser's spellcheck.
    /// </summary>
    public bool? SpellCheck { get; set; } = true;

    /// <summary>
    /// Validation rules for the text box.
    /// </summary>
    public ValidationRuleViewModel? Validation { get; set; }
}