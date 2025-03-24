using Ofqual.Recognition.Frontend.Core.Enums;
using Ofqual.Recognition.Frontend.Core.Extensions;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TaskItemViewModel
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public TaskStatusEnum Status { get; set; }

    public bool IsLink => Status != TaskStatusEnum.CannotStartYet;
    public string Url => TaskName == "Review application"
        ? $"/application/review-your-application-answers?taskId={TaskId}"
        : $"/application/review-your-task-answers?taskId={TaskId}";
    public string StatusDisplay => Status.GetDisplayName();
    public string TagClass => Status.GetTagClass();
}