namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionViewModel
{
    public Guid QuestionId { get; set; }
    public string QuestionTypeName { get; set; }
    public QuestionContent QuestionContent { get; set; }
}

public class QuestionContent
{
    public string? Heading { get; set; }
    public List<HelpItemViewModel>? HelpBox { get; set; }
    public TextBoxViewModel? TextBox { get; set; }
}