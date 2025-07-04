using Ofqual.Recognition.Frontend.Core.Models.ApplicationAnswers;
using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Web.ViewModels.ApplicationAnswers;

namespace Ofqual.Recognition.Frontend.Web.Mappers;

public class ApplicationAnswersMapper
{
    public static List<TaskReviewSectionViewModel> MapToViewModel(List<TaskReviewSection> taskReviewSection)
    {
        return taskReviewSection.Select(section => new TaskReviewSectionViewModel
        {
            SectionName = section.SectionName,
            TaskGroups = section.TaskGroups.Select(group => new TaskReviewGroupViewModel
            {
                SectionHeading = group.SectionHeading,
                QuestionAnswers = group.QuestionAnswers.Select(qa => new TaskReviewItemViewModel
                {
                    AnswerValue = qa.AnswerValue,
                    QuestionText = qa.QuestionText,
                    QuestionUrl = qa.QuestionUrl
                }).ToList()
            }).ToList()
        }).ToList();
    }
}
