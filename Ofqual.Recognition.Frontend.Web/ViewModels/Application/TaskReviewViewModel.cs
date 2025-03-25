using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Core.Validations;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TaskReviewViewModel
{
    [ValidEnumValue(typeof(TaskStatusEnum), ErrorMessage = "The status provided is not valid.")]
    public TaskStatusEnum Answer { get; set; }
}