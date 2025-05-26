namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Represents the users PreEngagement details.
/// </summary>
public class PreEngagement
{
    public int OrderNumber { get; set; }
    public Guid TaskId { get; set; }
    public Guid QuestionId { get; set; }
    public string TaskName { get; set; }
    public string CurrentTaskNameUrl { get; set; }
    public string CurrentQuestionNameUrl { get; set; }
    public string QuestionContent { get; set; }
    public string NextTaskNameUrl { get; set; }
    public string NextQuestionNameUrl { get; set; }
    public string PreviousQuestionUrl { get; set; }
    public string QuestionTypeName { get; set; }
}