namespace Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers;

/// <summary>
/// Represents an item in a task review, typically used for displaying a question and its answer on a review page.
/// </summary>
public class TaskReviewItem
{
    public string? QuestionText { get; set; }
    public List<string>? AnswerValue { get; set; }
    public string? QuestionUrl { get; set; }
}
