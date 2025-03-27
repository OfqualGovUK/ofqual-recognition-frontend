namespace Ofqual.Recognition.Frontend.Web.ViewModels;

public class TaskSectionViewModel
{
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public List<TaskItemViewModel> Tasks { get; set; } = [];
}