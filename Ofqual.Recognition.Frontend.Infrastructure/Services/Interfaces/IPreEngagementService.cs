using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IPreEngagementService
{
    public Task<PreEngagementQuestion?> GetFirstPreEngagementQuestion();
    public Task<QuestionDetails?> GetPreEngagementQuestionDetails(string taskNameUrl, string questionNameUrl);
    public Task<ValidationResponse?> ValidatePreEngagementAnswer(Guid questionId, string answerJson);
}
