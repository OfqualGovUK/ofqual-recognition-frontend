namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class ButtonGroupViewModel
{
    /// <summary>
    /// List of buttons to render.
    /// </summary>
    public List<ButtonViewModel> Buttons { get; set; } = new();

    /// <summary>
    /// List of links to render.
    /// </summary>
    public List<LinkViewModel> Links { get; set; } = new();
}