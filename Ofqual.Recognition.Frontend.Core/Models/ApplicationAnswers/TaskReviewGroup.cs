namespace Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers;

/// <summary>
/// Represents a group of task review items, typically used for displaying a section of questions and answers in a review.
/// </summary>
public class TaskReviewGroup
{
    public string? SectionHeading { get; set; }
    public List<TaskReviewItem> QuestionAnswers { get; set; } = new List<TaskReviewItem>();
}

