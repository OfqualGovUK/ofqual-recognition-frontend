using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Core.Constants;

public class MemoryCacheService : IMemoryCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly ConcurrentDictionary<string, object> _keyLocks = new();

    public MemoryCacheService(IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
    {
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    private string BuildSessionScopedKey(string key)
    {
        var sessionId = _httpContextAccessor.HttpContext?.Session?.Id;
        if (string.IsNullOrEmpty(sessionId))
        {
            throw new InvalidOperationException("Session is not available.");
        }

        return $"{key}:{sessionId}";
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null) where T : class
    {
        var scopedKey = BuildSessionScopedKey(key);
        var options = new MemoryCacheEntryOptions().SetSlidingExpiration(ttl ?? TimeSpan.FromMinutes(30));
        _cache.Set(scopedKey, value, options);
    }

    public T? Get<T>(string key) where T : class
    {
        var scopedKey = BuildSessionScopedKey(key);
        return _cache.TryGetValue(scopedKey, out T? value) ? value : null;
    }

    public void Remove(string key)
    {
        var scopedKey = BuildSessionScopedKey(key);
        _cache.Remove(scopedKey);
    }

    public bool HasInMemoryCache(string key)
    {
        var scopedKey = BuildSessionScopedKey(key);
        return _cache.TryGetValue(scopedKey, out _);
    }

    public void AddOrAppendToList<T>(string key, T item, TimeSpan? ttl = null) where T : class
    {
        var scopedKey = BuildSessionScopedKey(key);
        var lockObj = _keyLocks.GetOrAdd(scopedKey, _ => new object());

        lock (lockObj)
        {
            List<T> existingList;

            if (_cache.TryGetValue(scopedKey, out List<T>? currentList) && currentList != null)
            {
                existingList = currentList;
            }
            else
            {
                existingList = new List<T>();
            }

            existingList.Add(item);
            var options = new MemoryCacheEntryOptions().SetSlidingExpiration(ttl ?? TimeSpan.FromMinutes(30));
            _cache.Set(scopedKey, existingList, options);
        }
    }

    public void RemoveFromList<T>(string key, Func<T, bool> match) where T : class
    {
        var scopedKey = BuildSessionScopedKey(key);
        var lockObj = _keyLocks.GetOrAdd(scopedKey, _ => new object());

        lock (lockObj)
        {
            if (_cache.TryGetValue(scopedKey, out List<T>? list) && list != null)
            {
                var itemToRemove = list.FirstOrDefault(match);
                if (itemToRemove != null)
                {
                    list.Remove(itemToRemove);
                    _cache.Set(scopedKey, list);
                }
            }
        }
    }

    public void RecalculateIsInOtherCriteria(Guid applicationId, bool isAfterDelete = false)
    {
        var allCacheKeys = _keyLocks.Keys
            .Where(k => k.StartsWith(MemoryCacheKeys.UploadedFilesByQuestion, StringComparison.OrdinalIgnoreCase)
                     && k.Contains($":{applicationId}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var allAttachments = new List<(string Key, AttachmentDetails Attachment)>();

        foreach (var key in allCacheKeys)
        {
            if (_cache.TryGetValue(key, out List<AttachmentDetails>? attachments) && attachments != null)
            {
                allAttachments.AddRange(attachments.Select(a => (key, a)));
            }
        }

        var duplicateFileNames = allAttachments
            .GroupBy(x => x.Attachment.FileName, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, attachment) in allAttachments)
        {
            var hasDuplicate = duplicateFileNames.Contains(attachment.FileName);

            if (hasDuplicate)
            {
                attachment.IsInOtherCriteria = true;
            }
            else if (isAfterDelete)
            {
                attachment.IsInOtherCriteria = false;
            }
        }

        foreach (var key in allCacheKeys)
        {
            if (_cache.TryGetValue(key, out List<AttachmentDetails>? attachments) && attachments != null)
            {
                _cache.Set(key, attachments);
            }
        }
    }
}
