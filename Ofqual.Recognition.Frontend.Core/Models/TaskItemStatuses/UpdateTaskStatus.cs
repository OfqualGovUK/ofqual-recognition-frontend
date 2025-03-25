using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Represents an update to a task's status.
/// </summary>
public class UpdateTaskStatus
{
    public TaskStatusEnum Status { get; set; }
}