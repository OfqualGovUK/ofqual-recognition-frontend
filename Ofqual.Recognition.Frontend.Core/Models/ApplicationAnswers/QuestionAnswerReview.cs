namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Used for displaying a question and its answer on a review page.
/// </summary>
public class QuestionAnswerReview
{
    public required string QuestionText { get; set; }
    public required List<string> AnswerValue { get; set; }
    public required string QuestionUrl { get; set; }
}