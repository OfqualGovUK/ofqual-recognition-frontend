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

    public void UpsertPreEngagementAnswer(Guid sessionId, Guid questionId, Guid taskId, string answerJson)
    {
        if (string.IsNullOrWhiteSpace(answerJson))
        {
            throw new ArgumentException("Answer JSON cannot be null or empty.", nameof(answerJson));
        }

        var cacheKey = $"{sessionId}_{SessionKeys.PreEngagementAnswers}";
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
                AnswerJson = answerJson
            });
        }

        _memoryCache.Set(cacheKey, cachedAnswers, new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(60)
        });
    }

    public PreEngagementAnswer? GetPreEngagementAnswer(Guid sessionId, Guid questionId, Guid taskId)
    {
        var cacheKey = $"{sessionId}_{SessionKeys.PreEngagementAnswers}";

        if (_memoryCache.TryGetValue(cacheKey, out var rawList) && rawList is List<PreEngagementAnswer> cachedAnswers)
        {
            return cachedAnswers.FirstOrDefault(a => a.QuestionId == questionId && a.TaskId == taskId);
        }

        return null;
    }
}