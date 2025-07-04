namespace Ofqual.Recognition.Frontend.Web.ViewModels;

/// <summary>
/// Represents a group of task review items, typically used to display a section of questions and answers in the review process.
/// </summary>
public class TaskReviewGroupViewModel
{
    public string? SectionHeading { get; set; }
    public List<TaskReviewItemViewModel> QuestionAnswers { get; set; } = new();
}