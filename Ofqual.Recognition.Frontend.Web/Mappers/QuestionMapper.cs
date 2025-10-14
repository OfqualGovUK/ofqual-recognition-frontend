using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Models;
using Newtonsoft.Json;

namespace Ofqual.Recognition.Frontend.Web.Mappers;

public static class QuestionMapper
{
    public static QuestionViewModel MapToViewModel(QuestionDetails question)
    {
        var json = JsonConvert.DeserializeObject<QuestionContentViewModel>(question.QuestionContent);

        QuestionViewModel questionViewModel = new QuestionViewModel
        {
            QuestionType = question.QuestionType,
            QuestionId = question.QuestionId,
            TaskId = question.TaskId,
            QuestionContent = new QuestionContentViewModel
            {
                Body = json?.Body,
                Sidebar = json?.Sidebar,
                FormGroup = json?.FormGroup
            },
            CurrentQuestionUrl = question.CurrentQuestionUrl,
            PreviousQuestionUrl = question.PreviousQuestionUrl
        };

        return questionViewModel;
    }

    public static TaskReviewViewModel MapToViewModel(List<QuestionAnswerSection> sections)
    {
        return new TaskReviewViewModel
        {
            QuestionAnswerSections = sections.Select(section => new TaskReviewGroupViewModel
            {
                SectionHeading = section.SectionHeading,
                QuestionAnswers = section.QuestionAnswers.Select(q => new TaskReviewItemViewModel
                {
                    QuestionText = q.QuestionText,
                    QuestionUrl = q.QuestionUrl,
                    AnswerValue = q.AnswerValue
                }).ToList()
            }).ToList()
        };
    }

    public static ValidationViewModel MapToViewModel(ValidationResponse validationResponse)
    {
        return new ValidationViewModel
        {
            Errors = validationResponse.Errors?.Select(e => new ErrorItemViewModel
            {
                PropertyName = e.PropertyName,
                ErrorMessage = e.ErrorMessage
            })
        };
    }
}