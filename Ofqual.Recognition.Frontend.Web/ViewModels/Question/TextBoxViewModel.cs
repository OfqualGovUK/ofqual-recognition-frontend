namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TextBoxViewModel
{
    public string? Label { get; set; }
    public string? Hint { get; set; }
    public string? Name { get; set; }
    public int? Rows { get; set; } = 5;
    public bool? SpellCheck { get; set; } = true;
    public ValidationRuleViewModel? Validation { get; set; }
}