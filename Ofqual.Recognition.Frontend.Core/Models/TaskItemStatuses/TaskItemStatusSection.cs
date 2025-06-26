namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Represents a section containing a list of tasks with their statuses.
/// </summary>
public class TaskItemStatusSection
{
    public Guid SectionId { get; set; }
    public required string SectionName { get; set; }
    public IEnumerable<TaskItemStatus> Tasks { get; set; } = new List<TaskItemStatus>();
}