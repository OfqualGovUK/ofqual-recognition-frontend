namespace Ofqual.Recognition.Frontend.Core.Models;

public class PreEngagementAnswer
{
    public Guid QuestionId { get; set; }
    public Guid TaskId { get; set; }
    public string? AnswerJson { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}