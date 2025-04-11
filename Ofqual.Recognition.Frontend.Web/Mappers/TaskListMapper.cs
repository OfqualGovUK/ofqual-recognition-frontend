using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
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
                    Status = task.Status,
                    FirstQuestionURL = task.FirstQuestionURL,
                }).ToList()
            }).ToList()
        };
    }

    public async static Task<TaskReviewViewModel> MapToReviewViewModel(IEnumerable<TaskItemStatusSection> sections) =>
         new TaskReviewViewModel()
         {
            ApplicationTaskStatus = Core.Enums.TaskStatusEnum.NotStarted,
            SectionHeadings = sections.Select(section => new TaskReviewSectionHeadingViewModel
            {
                Title = section.SectionName,
                Items = section.Tasks.Select(sectionItem => new TaskReviewSectionItemViewModel
                {
                    Title = sectionItem.TaskName,
                    QuestionAnswers = [ new TaskReviewQuestionAnswerViewModel { Question = sectionItem.TaskName }]
                }).ToArray()
            }).ToArray()
         };
    
}