namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class SelectViewModel
{
    /// <summary>
    /// The label shown above the select dropdown.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Hint text shown below the label.
    /// </summary>
    public string? Hint { get; set; }

    /// <summary>
    /// The name and id attribute for the select element.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Whether the select element is disabled.
    /// </summary>
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// A list of selectable options.
    /// </summary>
    public required List<SelectOptionViewModel> Options { get; set; }
}