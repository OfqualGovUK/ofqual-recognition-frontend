namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class SelectOptionViewModel
{
    /// <summary>
    /// The text displayed to the user in the dropdown.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// The value submitted with the form when selected.
    /// </summary>
    public required string Value { get; set; }
    
    /// <summary>
    /// Indicates whether this option is pre-selected.
    /// </summary>
    public bool Selected { get; set; } = false;
}