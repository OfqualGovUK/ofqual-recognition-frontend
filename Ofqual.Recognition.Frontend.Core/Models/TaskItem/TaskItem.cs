
namespace Ofqual.Recognition.Frontend.Core.Models;

public class TaskItem
{
    public Guid TaskId { get; set; }
    public required string TaskName { get; set; }
    public required string TaskNameUrl { get; set; }
    public int TaskOrderNumber { get; set; }
    public Guid SectionId { get; set; }
}