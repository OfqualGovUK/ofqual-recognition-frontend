using Ofqual.Recognition.Frontend.Core.Validations;
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TaskReviewViewModel
{
    public List<QuestionAnswerSectionViewModel> QuestionAnswerSections { get; set; } = new();
    
    [ValidEnumValue(typeof(TaskStatusEnum), ErrorMessage = "The status provided is not valid.")]
    public TaskStatusEnum Answer { get; set; }
    public string? LastQuestionUrl { get; set; }
    public bool IsCompletedStatus { get; set; }
}