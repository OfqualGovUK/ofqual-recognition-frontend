using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class BodyItemViewModel
{
    public required BodyItemType Type { get; set; }
    public HeadingContentViewModel? HeadingContent { get; set; }
    public ParagraphContentViewModel? ParagraphContent { get; set; }
    public ListContentViewModel? ListContent { get; set; }
    public ButtonContentViewModel? ButtonContent { get; set; }
}
