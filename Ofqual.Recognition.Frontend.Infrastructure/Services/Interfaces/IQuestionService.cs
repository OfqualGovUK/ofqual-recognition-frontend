using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IQuestionService
{
    public Task<QuestionDetails?> GetQuestionDetails(string taskNameUrl, string questionNameUrl);
    public Task<bool> SubmitQuestionAnswer(Guid applicationId, Guid taskId, Guid questionId, string answer);
    public Task<List<QuestionAnswerSection>?> GetTaskQuestionAnswers(Guid applicationId, Guid taskId);
    public Task<QuestionAnswer?> GetQuestionAnswer(Guid applicationId, Guid questionId);
}
