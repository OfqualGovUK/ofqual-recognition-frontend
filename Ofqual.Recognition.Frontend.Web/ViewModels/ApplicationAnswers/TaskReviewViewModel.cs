using Ofqual.Recognition.Frontend.Core.Validations;
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TaskReviewViewModel
{
    public List<QuestionAnswerReviewViewModel> questionAnswers { get; set; }

    [ValidEnumValue(typeof(TaskStatusEnum), ErrorMessage = "The status provided is not valid.")]
    public TaskStatusEnum Answer { get; set; }
}