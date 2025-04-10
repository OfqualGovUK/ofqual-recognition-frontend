namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class CheckBoxItemViewModel
{
    /// <summary>
    /// The text label shown to the user for this checkbox.
    /// </summary>
    public string? Label { get; set; }
    
    /// <summary>
    /// The value sent when this checkbox is selected.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// A list of select dropdowns that appear when this checkbox is selected.
    /// </summary>
    public List<SelectViewModel>? ConditionalSelects { get; set; }

    /// <summary>
    /// A list of text inputs that appear when this checkbox is selected.
    /// </summary>
    public List<TextInputViewModel>? ConditionalInputs { get; set; }
}