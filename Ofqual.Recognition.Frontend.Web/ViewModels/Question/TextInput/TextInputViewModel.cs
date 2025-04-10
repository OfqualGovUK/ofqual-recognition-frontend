namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TextInputViewModel
{
    /// <summary>
    /// The label shown above the input field.
    /// </summary>
    public string? Label { get; set; }
    
    /// <summary>
    /// Hint text shown below the label to guide the user.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// The name and id attribute for the input field.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Whether the input is disabled.
    /// </summary>
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// The input type, such as text, number or email.
    /// </summary>
    public string InputType { get; set; } = "text";
}