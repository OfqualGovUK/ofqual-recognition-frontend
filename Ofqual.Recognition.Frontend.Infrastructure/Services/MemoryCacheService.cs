using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Ofqual.Recognition.Frontend.Infrastructure.Services;

public class MemoryCacheService : IMemoryCacheService
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public T? GetFromCache<T>(string cacheKey)
    {
        if (_memoryCache.TryGetValue(cacheKey, out var value) && value is T typedValue)
        {
            return typedValue;
        }

        return default;
    }

    public void RemoveFromCache(string cacheKey)
    {
        _memoryCache.Remove(cacheKey);
    }

    public void SetInCache<T>(string cacheKey, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }

        _memoryCache.Set(cacheKey, value, options);
    }

    public void UpsertPreEngagementAnswer(Guid questionId, Guid taskId, string answerJson)
    {
        if (string.IsNullOrWhiteSpace(answerJson))
        {
            throw new ArgumentException("Answer JSON cannot be null or empty.", nameof(answerJson));
        }

        var cacheKey = MemoryKeys.PreEngagementAnswers;
        var cachedAnswers = _memoryCache.Get(cacheKey) as List<PreEngagementAnswer> ?? new List<PreEngagementAnswer>();
        var existing = cachedAnswers.FirstOrDefault(a => a.QuestionId == questionId && a.TaskId == taskId);

        if (existing != null)
        {
            existing.AnswerJson = answerJson;
        }
        else
        {
            cachedAnswers.Add(new PreEngagementAnswer
            {
                QuestionId = questionId,
                TaskId = taskId,
                AnswerJson = answerJson,
                SubmittedDate = DateTime.UtcNow
            });
        }

        _memoryCache.Set(cacheKey, cachedAnswers, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(60)
        });
    }
}