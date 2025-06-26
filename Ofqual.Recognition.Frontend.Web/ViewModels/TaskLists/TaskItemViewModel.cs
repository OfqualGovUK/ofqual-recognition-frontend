using Ofqual.Recognition.Frontend.Core.Extensions;
using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TaskItemViewModel
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public TaskStatusEnum Status { get; set; }
    public string? Hint { get; set; }
    public required string FirstQuestionURL { get; set; }
    public bool IsLink => Status != TaskStatusEnum.CannotStartYet;
    public string StatusDisplay => Status.GetDisplayName();
    public string TagClass => Status.GetTagClass();
}