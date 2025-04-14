using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IQuestionService
{
    public Task<QuestionDetails?> GetQuestionDetails(string taskName, string questionName);
    public Task<QuestionAnswerSubmissionResponse?> SubmitQuestionAnswer(Guid applicationId, Guid taskId, Guid questionId, string answer);
    public Task<List<QuestionAnswerSection>?> GetTaskQuestionAnswers(Guid applicationId, Guid taskId);
}
