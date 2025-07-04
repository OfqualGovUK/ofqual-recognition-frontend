namespace Ofqual.Recognition.Frontend.Web.ViewModels.ApplicationAnswers;

/// <summary>
/// Represents a section of task reviews, typically used to group related task review items.
/// </summary>
public class TaskReviewSectionViewModel
{
    public string SectionName { get; set; }
    public List<TaskReviewGroupViewModel> TaskGroups { get; set; } = new List<TaskReviewGroupViewModel>();
}
