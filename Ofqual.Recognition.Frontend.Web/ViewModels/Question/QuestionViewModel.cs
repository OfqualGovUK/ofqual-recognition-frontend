namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionViewModel
{
    public Guid QuestionId { get; set; }
    public string QuestionTypeName { get; set; }
    public QuestionContent QuestionContent { get; set; }
}

public class QuestionContent
{
    public required string Title { get; set; }
    public string? Hint { get; set; }
}