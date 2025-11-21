using Ofqual.Recognition.Frontend.Core.Enums;
using System.Text.Json.Serialization;

namespace Ofqual.Recognition.Frontend.Core.Models;

public class QuestionDetails
{
    public Guid QuestionId { get; set; }
    public Guid TaskId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public QuestionTypeEnum QuestionType { get; set; }
    public required string QuestionContent { get; set; }
    public required string CurrentQuestionUrl { get; set; }
    public string? PreviousQuestionUrl { get; set; }
    public string? NextQuestionUrl { get; set; }
}