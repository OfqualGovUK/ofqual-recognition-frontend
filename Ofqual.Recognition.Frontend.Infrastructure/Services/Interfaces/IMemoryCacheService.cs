namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IMemoryCacheService
{
    public T? GetFromCache<T>(string cacheKey);
    public void RemoveFromCache(string cacheKey);
    public void SetInCache<T>(string cacheKey, T value, TimeSpan? expiration = null);
    public void UpsertPreEngagementAnswer(Guid questionId, Guid taskId, string answerJson);
}