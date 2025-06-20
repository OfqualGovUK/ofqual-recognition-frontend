
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class ParagraphContentViewModel
{
    public string? Text { get; set; }
    public BodyTextSize Size { get; set; } = BodyTextSize.M;
    public string? TextBeforeLink { get; set; }
    public string? TextAfterLink { get; set; }
    public LinkViewModel? Link { get; set; }
}
