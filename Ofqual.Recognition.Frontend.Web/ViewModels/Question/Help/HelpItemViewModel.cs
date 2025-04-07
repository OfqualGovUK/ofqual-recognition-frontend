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

    /// <summary>
    /// Returns true if this help item contains any links.
    /// </summary>
    public bool HasLinks => Links != null && Links.Count != 0;
    
    /// <summary>
    /// Returns true if this help item contains any content.
    /// </summary>
    public bool HasContent => Content != null && Content.Count != 0;
}