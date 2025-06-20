namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class HelpItemViewModel
{
    /// <summary>
    /// A list of links included in this help item.
    /// </summary>
    public List<LinkViewModel>? Links { get; set; }

    /// <summary>
    /// A list of content blocks, such as a heading or paragraph.
    /// </summary>
    public List<BodyItemViewModel>? Content { get; set; }
}