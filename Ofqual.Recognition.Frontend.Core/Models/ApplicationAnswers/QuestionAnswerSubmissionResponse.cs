namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Returned after submitting an answer, containing the next question URL for redirection.
/// </summary>
public class QuestionAnswerSubmissionResponse
{
    public required string NextTaskNameUrl { get; set; }
    public required string NextQuestionNameUrl { get; set; }
}