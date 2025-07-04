namespace Ofqual.Recognition.Frontend.Web.ViewModels;

/// <summary>
/// Represents an item in the task review, containing details about a question and its answers.
/// </summary>
public class TaskReviewItemViewModel
{
    public required string QuestionText { get; set; }
    public required List<string> AnswerValue { get; set; }
    public required string QuestionUrl { get; set; }
}