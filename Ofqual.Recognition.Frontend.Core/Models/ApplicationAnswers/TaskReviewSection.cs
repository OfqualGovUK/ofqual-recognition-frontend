namespace Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers;

/// <summary>
/// Represents a section in a task review, typically used for displaying a collection of task review groups.
/// </summary>
public class TaskReviewSection
{
    public string SectionName { get; set; }
    public List<TaskReviewGroup> TaskGroups { get; set; } = new List<TaskReviewGroup>();
}
