namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Represents a section of questions and answers in the review.
/// </summary>
public class QuestionAnswerSection
{
    public string? SectionHeading { get; set; }
    public List<QuestionAnswerReview> QuestionAnswers { get; set; } = new();
}