using Ofqual.Recognition.Frontend.Core.Enums;

namespace Ofqual.Recognition.Frontend.Core.Models;

public class TaskDetails
{
    public Guid TaskId { get; set; }
    public required string TaskName { get; set; }
    public required string TaskNameUrl { get; set; }
    public int TaskOrderNumber { get; set; }
    public StageType Stage { get; set; }
    public StatusType Status { get; set; }
    public Guid SectionId { get; set; }
}