namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class RadioButtonItemViewModel
{
    /// <summary>
    /// The text label shown to the user for this radio button.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// The value sent when this radio button is selected.
    /// </summary>
    public string? Value { get; set; }

    // <summary>
    /// Hint text shown for this specific radio button item.
    /// </summary>
    public string? Hint { get; set; }
}