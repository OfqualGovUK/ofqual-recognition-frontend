namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionAnswerSectionViewModel
{
    public string? SectionHeading { get; set; }
    public List<QuestionAnswerReviewViewModel> QuestionAnswers { get; set; } = new();
}