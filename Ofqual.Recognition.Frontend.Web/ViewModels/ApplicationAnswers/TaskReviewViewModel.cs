using Ofqual.Recognition.Frontend.Core.Validations;
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

/// <summary>
/// Represents a view model for reviewing tasks, containing sections of questions and answers.
/// </summary>
public class TaskReviewViewModel
{
    public List<TaskReviewGroupViewModel> QuestionAnswerSections { get; set; } = new();
    
    [ValidEnumValue(typeof(StatusType), ErrorMessage = "The status provided is not valid.")]
    public StatusType Answer { get; set; }
    public string? LastQuestionUrl { get; set; }
    public bool IsCompletedStatus { get; set; }
}