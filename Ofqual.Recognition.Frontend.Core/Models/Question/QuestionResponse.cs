namespace Ofqual.Recognition.Frontend.Core.Models;

public class QuestionResponse
{
    public Guid QuestionId { get; set; }
    public Guid TaskId { get; set; }
    public string QuestionTypeName { get; set; }
    public string QuestionContent { get; set; }
}