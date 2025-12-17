
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class HeadingContentViewModel
{
    public required string Text { get; set; }
    public BodyTextSize Size { get; set; } = BodyTextSize.L;
    public HTMLHeading HeadingType { get; set; } = HTMLHeading.Default;
}
