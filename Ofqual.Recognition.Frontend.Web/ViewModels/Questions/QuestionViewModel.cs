using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Web.ViewModels.ApplicationAnswers;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class QuestionViewModel
{
    public Guid QuestionId { get; set; }
    public Guid TaskId { get; set; }
    public QuestionType QuestionType { get; set; }
    public required QuestionContentViewModel QuestionContent { get; set; }
    public required string CurrentQuestionUrl { get; set; }
    public string? AnswerJson { get; set; }
    public List<AttachmentDetailsViewModel>? Attachments { get; set; }
    public string? PreviousQuestionUrl { get; set; }
    public string? RedirectUrl { get; set; }
    public bool FromPreEngagement { get; set; }
    public ValidationViewModel? Validation { get; set; }
    public List<TaskReviewSectionViewModel>? TaskReviewSection{ get; set; }
}