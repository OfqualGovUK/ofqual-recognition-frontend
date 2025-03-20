namespace Ofqual.Recognition.Frontend.Core.Models;

/// <summary>
/// Represents a section containing a list of tasks with their statuses.
/// </summary>
public class TaskItemStatusSectionDto
{
    public Guid SectionId { get; set; }
    public required string SectionName { get; set; }
    public int SectionOrderNumber { get; set; }

    public IEnumerable<TaskItemStatusDto> Tasks { get; set; } = new List<TaskItemStatusDto>();
}