namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TaskReviewGroupViewModel
{
    public string? SectionHeading { get; set; }
    public List<TaskReviewItemViewModel> QuestionAnswers { get; set; } = new();
}