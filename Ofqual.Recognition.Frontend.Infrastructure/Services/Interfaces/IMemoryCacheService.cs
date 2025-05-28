using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IMemoryCacheService
{
    public void UpsertPreEngagementAnswer(Guid questionId, Guid taskId, string answerJson);
    public PreEngagementAnswer? GetPreEngagementAnswer(Guid questionId, Guid taskId);
    public List<PreEngagementAnswer>? GetAllPreEngagementAnswers();
}