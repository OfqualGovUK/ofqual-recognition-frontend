using Ofqual.Recognition.Frontend.Web.ViewModels;
using Ofqual.Recognition.Frontend.Core.Models;
using Newtonsoft.Json;

namespace Ofqual.Recognition.Frontend.Web.Mappers;

public static class QuestionMapper
{
    public static QuestionViewModel MapToViewModel(QuestionDetails question)
    {
        var json = JsonConvert.DeserializeObject<QuestionContentViewModel>(question.QuestionContent);
        var questionViewModel = new QuestionViewModel
        {
            QuestionTypeName = question.QuestionTypeName,
            QuestionId = question.QuestionId,
            TaskId = question.TaskId,
            QuestionContent = new QuestionContentViewModel
            {
                Heading = json?.Heading,
                Body = json?.Body,
                Help = json?.Help,
                FormGroup = json?.FormGroup
            },
            CurrentQuestionUrl = question.CurrentQuestionUrl,
            PreviousQuestionUrl = question.PreviousQuestionUrl
        };
        return questionViewModel;
    }

    public static TaskReviewViewModel MapToViewModel(List<QuestionAnswerReview> questionAnswers)
    {
        return new TaskReviewViewModel
        {
            QuestionAnswers = questionAnswers.Select(q => new QuestionAnswerReviewViewModel
            {
                QuestionText = q.QuestionText,
                QuestionUrl = q.QuestionUrl,
                AnswerValue = q.AnswerValue
            }).ToList()
        };
    }
}