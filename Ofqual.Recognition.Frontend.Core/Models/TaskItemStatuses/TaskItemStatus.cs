using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Represents the status of a task.
/// </summary>
public class TaskItemStatus
{
    public Guid TaskId { get; set; }
    public required string TaskName { get; set; }
    public StatusType Status { get; set; }
    public string? HintText { get; set; }
    public required string FirstQuestionURL { get; set; }
}