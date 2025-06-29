namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionAnswerReviewViewModel
{
    public required string QuestionText { get; set; }
    public required List<string> AnswerValue { get; set; }
    public required string QuestionUrl { get; set; }
}