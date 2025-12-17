using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TextWithSizeViewModel
{
    /// <summary>
    /// The text content to be displayed.
    /// </summary>
    public required string Text { get; set; }

    /// <summary>
    /// The display size of the text.
    /// </summary>
    public BodyTextSize Size { get; set; } = BodyTextSize.M;

    /// <summary>
    /// // HTML heading type to be used for the text.
    /// </summary>
    public HTMLHeading HeadingType { get; set; } = HTMLHeading.Default;

}