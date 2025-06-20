
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class ListContentViewModel
{
    public required List<string> Items { get; set; }
    public BodyListStyle Style { get; set; } = BodyListStyle.Bulleted;
}