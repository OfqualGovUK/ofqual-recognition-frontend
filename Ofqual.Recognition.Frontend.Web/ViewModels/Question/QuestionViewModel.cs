namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionViewModel
{
    public Guid QuestionId { get; set; }
    public required string QuestionTypeName { get; set; }
    public required QuestionContentViewModel QuestionContent { get; set; }
}