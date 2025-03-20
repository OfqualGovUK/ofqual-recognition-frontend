using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Represents the status of a task.
/// </summary>
public class TaskItemStatusDto
{
    public Guid TaskId { get; set; }
    public required string TaskName { get; set; }
    public int TaskOrderNumber { get; set; }

    public Guid TaskStatusId { get; set; }
    public TaskStatusEnum Status { get; set; }
}