using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IMemoryCacheService
{
    public void UpsertPreEngagementAnswer(Guid sessionId, Guid questionId, Guid taskId, string answerJson);
    public PreEngagementAnswer? GetPreEngagementAnswer(Guid sessionId, Guid questionId, Guid taskId);
}