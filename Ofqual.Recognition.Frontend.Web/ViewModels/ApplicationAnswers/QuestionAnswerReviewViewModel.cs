namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionAnswerReviewViewModel
{
    public string QuestionText { get; set; }
    public required List<string> AnswerValue { get; set; }
    public string QuestionUrl { get; set; }
}