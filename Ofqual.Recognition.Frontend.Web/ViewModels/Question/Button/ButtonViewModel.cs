namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class ButtonViewModel
{
    /// <summary>
    /// The text shown on the button.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The button type, such as submit or button.
    /// </summary>
    public string Type { get; set; } = "submit";

    /// <summary>
    /// The name attribute used when submitting the form.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// The URL or path the form posts to when the button is clicked.
    /// </summary>
    public string? Action { get; set; }
}