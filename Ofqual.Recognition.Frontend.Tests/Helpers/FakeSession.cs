
using Microsoft.AspNetCore.Http;

namespace Ofqual.Recognition.Frontend.Tests.Helpers;

public class FakeSession : ISession
{
    private readonly string _sessionId;
    private readonly Dictionary<string, byte[]> _store = new();

    public FakeSession(string sessionId)
    {
        _sessionId = sessionId;
    }

    public string Id
    {
        get { return _sessionId; }
    }

    public bool IsAvailable
    {
        get { return true; }
    }

    public IEnumerable<string> Keys
    {
        get { return _store.Keys; }
    }

    public void Clear()
    {
        _store.Clear();
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _store.Remove(key);
    }

    public void Set(string key, byte[] value)
    {
        _store[key] = value;
    }

    public bool TryGetValue(string key, out byte[] value)
    {
        if (_store.TryGetValue(key, out var temp))
        {
            value = temp!;
            return true;
        }

        value = Array.Empty<byte>();
        return false;
    }
}
