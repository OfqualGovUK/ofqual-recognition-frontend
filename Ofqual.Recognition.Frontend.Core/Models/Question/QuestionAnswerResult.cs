namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Represents the result of submitting an question answer, including the next question URL if available.
/// </summary>
public class QuestionAnswerResult
{
    public string? NextQuestionUrl { get; set; }
}