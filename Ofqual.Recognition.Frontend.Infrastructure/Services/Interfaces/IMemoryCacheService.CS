namespace Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

public interface IMemoryCacheService
{
    public void Set<T>(string key, T value, TimeSpan? ttl = null) where T : class;
    public T? Get<T>(string key) where T : class;
    public void Remove(string key);
    public bool HasInMemoryCache(string key);
    public void AddOrAppendToList<T>(string key, T item, TimeSpan? ttl = null) where T : class;
    public void RemoveFromList<T>(string key, Func<T, bool> match) where T : class;
    public void RecalculateIsInOtherCriteria(Guid applicationId, bool isAfterDelete = false);
}
