using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Core.Helpers;

public class AnswerHelper
{
    public List<QuestionAnswerSection> MapQuestionAnswerSections(List<QuestionAnswerSection> questionAnswerSections)
    {
        return questionAnswerSections;
    }

    public (QuestionDetails questionDetails, QuestionAnswer? questionAnswer) MapQuestionAnswer(QuestionDetails questionDetails, QuestionAnswer? questionAnswer)
    { 
        return (questionDetails, questionAnswer);
    }

    //public static List<QuestionAnswer> GetAnswersListHelper(Guid applicationId, Guid taskId, Task<QuestionAnswer?> questionService)
    //{
    //    var questionAnswer = questionService.GetQuestionAnswer(applicationId, taskId);

    //    if (questionAnswer == null)
    //    {
    //        Log.Warning("No question answer found for taskId: {taskId} in applicationId: {applicationId}", taskId, applicationId);
    //        return new List<QuestionAnswer>();
    //    }

    //    var questionAnswerList = new List<QuestionAnswer>
    //    {
    //        questionAnswer
    //    };

    //    return questionAnswerList;
    //}
}