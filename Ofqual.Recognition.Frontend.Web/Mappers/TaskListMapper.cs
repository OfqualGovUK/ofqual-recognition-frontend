using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Web.ViewModels;

namespace Ofqual.Recognition.Frontend.Web.Mappers;

public static class TaskListMapper
{
    public static TaskListViewModel MapToViewModel(IEnumerable<TaskItemStatusSection> sections)
    {
        return new TaskListViewModel
        {
            Sections = sections.Select(section => new TaskSectionViewModel
            {
                SectionId = section.SectionId,
                SectionName = section.SectionName,
                Tasks = section.Tasks.Select(task => new TaskItemViewModel
                {
                    TaskId = task.TaskId,
                    TaskName = task.TaskName,
                    Status = task.Status
                }).ToList()
            }).ToList()
        };
    }
}