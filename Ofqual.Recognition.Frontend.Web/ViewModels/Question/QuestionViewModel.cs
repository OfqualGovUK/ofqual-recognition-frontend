namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionViewModel
{
    public Guid QuestionId { get; set; }
    public string QuestionTypeName { get; set; }
    public QuestionContentViewModel QuestionContent { get; set; }
}